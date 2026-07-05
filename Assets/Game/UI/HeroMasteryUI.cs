#if UNITY_EDITOR || !UNITY_SERVER
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// HeroMasteryUI — 5-hero card panel, toggled by pressing H (or via HeroMasteryHUD).
///
/// Self-bootstrapping singleton, DontDestroyOnLoad, sortingOrder 94.
///
/// Layout: full-screen dim + centred panel (500×340) containing five hero cards side-by-side.
/// Each card shows:
///   • Hero name + class icon colour swatch
///   • Level badge (bold)
///   • XP progress bar (coloured by class)
///   • Unlocked reward text: "Lv6: +8% Damage" / "Lv10: +5% CDR" (greyed if locked)
///
/// Current player's class card has a gold border highlight.
/// On level-up the corresponding card flashes white for 0.5s.
///
/// Reads: HeroMasteryManager.Local, PlayerProgressManager.Local.ClassIndex
/// </summary>
public class HeroMasteryUI : MonoBehaviour
{
    // ── Bootstrap ─────────────────────────────────────────────────────────────
    public static HeroMasteryUI Instance { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Bootstrap()
    {
        if (Instance != null) return;
        var go = new GameObject("[HeroMasteryUI]");
        DontDestroyOnLoad(go);
        Instance = go.AddComponent<HeroMasteryUI>();
    }

    // ── Hero metadata ─────────────────────────────────────────────────────────
    static readonly string[] HeroNames = { "Warden", "Ironclad", "Shadowblade", "Cleric", "Arcanist" };

    // Matches ClassColors in HeroMasteryHUD
    static readonly Color[] HeroColors =
    {
        new Color(0.40f, 0.80f, 0.40f, 1f),
        new Color(0.60f, 0.60f, 0.75f, 1f),
        new Color(0.60f, 0.10f, 0.80f, 1f),
        new Color(0.95f, 0.80f, 0.20f, 1f),
        new Color(0.30f, 0.55f, 1.00f, 1f),
    };

    // Reward text per hero per milestone
    static readonly string[,] RewardTextL6 =
    {
        { "Warden",     "+8% Damage"  },
        { "Ironclad",   "+10% Max HP" },
        { "Shadowblade","+8% Damage"  },
        { "Cleric",     "+15% Healing"},
        { "Arcanist",   "+8% CDR"     },
    };
    static readonly string[,] RewardTextL10 =
    {
        { "Warden",     "+5% CDR"     },
        { "Ironclad",   "+8% Damage"  },
        { "Shadowblade","+8% CDR"     },
        { "Cleric",     "+10% Max HP" },
        { "Arcanist",   "+10% Damage" },
    };

    // ── Colour palette ────────────────────────────────────────────────────────
    static readonly Color ColOverlay   = new Color(0.02f, 0.03f, 0.06f, 0.88f);
    static readonly Color ColPanel     = new Color(0.06f, 0.07f, 0.11f, 0.98f);
    static readonly Color ColCard      = new Color(0.10f, 0.11f, 0.16f, 1.00f);
    static readonly Color ColBorder    = new Color(0.95f, 0.80f, 0.20f, 1.00f);
    static readonly Color ColLocked    = new Color(0.45f, 0.45f, 0.50f, 0.80f);
    static readonly Color ColUnlocked  = new Color(0.80f, 1.00f, 0.80f, 1.00f);
    static readonly Color ColTitle     = new Color(1.00f, 0.92f, 0.60f, 1.00f);
    static readonly Color ColWhite     = new Color(1.00f, 1.00f, 1.00f, 0.90f);
    static readonly Color ColDim       = new Color(0.65f, 0.65f, 0.70f, 0.75f);

    // ── UI refs ───────────────────────────────────────────────────────────────
    GameObject _panel;
    Image      _dimOverlay;

    // Per-card refs (5 cards)
    struct CardRefs
    {
        public Image           bg;
        public Image           borderOutline;
        public Image           colourSwatch;
        public Image           xpBarFill;
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI levelText;
        public TextMeshProUGUI rewardL6;
        public TextMeshProUGUI rewardL10;
    }
    CardRefs[] _cards = new CardRefs[5];

    // ── State ─────────────────────────────────────────────────────────────────
    bool  _open = false;
    float _flashTimer = 0f;
    int   _flashHero  = -1;
    const float FlashDuration = 0.5f;

    // ── Lifecycle ─────────────────────────────────────────────────────────────
    void Awake()
    {
        BuildUI();
        _panel.SetActive(false);
        if (_dimOverlay != null) _dimOverlay.gameObject.SetActive(false); // else it blocks ALL clicks

        if (HeroMasteryManager.Local != null)
        {
            HeroMasteryManager.Local.OnMasteryChanged += Refresh;
            HeroMasteryManager.Local.OnHeroLevelUp    += OnHeroLevelUp;
        }
    }

    void OnDestroy()
    {
        if (HeroMasteryManager.Local != null)
        {
            HeroMasteryManager.Local.OnMasteryChanged -= Refresh;
            HeroMasteryManager.Local.OnHeroLevelUp    -= OnHeroLevelUp;
        }
    }

    void Update()
    {
        // Dismiss on Escape or H
        if (_open)
        {
            var kb = UnityEngine.InputSystem.Keyboard.current;
            if (kb != null && (kb.escapeKey.wasPressedThisFrame || kb.hKey.wasPressedThisFrame))
                Close();
        }

        // Flash animation
        if (_flashTimer > 0f && _flashHero >= 0)
        {
            _flashTimer -= Time.deltaTime;
            float t = _flashTimer / FlashDuration;
            _cards[_flashHero].bg.color = Color.Lerp(ColCard, Color.white, t);
            if (_flashTimer <= 0f) _cards[_flashHero].bg.color = ColCard;
        }
    }

    // ── Public ────────────────────────────────────────────────────────────────
    public void Toggle() { if (_open) Close(); else Open(); }

    public void Open()
    {
        if (_dimOverlay != null) _dimOverlay.gameObject.SetActive(true);
        _panel.SetActive(true);
        _open = true;
        Refresh();
    }

    public void Close()
    {
        _panel.SetActive(false);
        if (_dimOverlay != null) _dimOverlay.gameObject.SetActive(false);
        _open = false;
    }

    // ── Refresh all cards ─────────────────────────────────────────────────────
    void Refresh()
    {
        if (!_open) return;
        var mgr = HeroMasteryManager.Local;
        if (mgr == null) return;

        int classIdx = PlayerProgressManager.Local?.ClassIndex ?? 0;

        for (int i = 0; i < 5; i++)
        {
            var entry = mgr.Masteries[i];
            var c     = _cards[i];
            bool isCurrent = (i == classIdx);

            // Card border highlight for current class
            c.borderOutline.color = isCurrent ? ColBorder : new Color(0.2f, 0.2f, 0.25f, 0.6f);

            // Level
            c.levelText.text = $"Lv{entry.level}";

            // XP bar
            c.xpBarFill.fillAmount = entry.XpFraction;
            c.xpBarFill.color      = HeroColors[i];

            // Reward text
            bool hasL6  = entry.level >= 6;
            bool hasL10 = entry.level >= 10;
            c.rewardL6.text  = $"L6:  {RewardTextL6[i, 1]}";
            c.rewardL6.color = hasL6  ? ColUnlocked : ColLocked;
            c.rewardL10.text = $"L10: {RewardTextL10[i, 1]}";
            c.rewardL10.color = hasL10 ? ColUnlocked : ColLocked;
        }
    }

    void OnHeroLevelUp(int heroId, int newLevel)
    {
        if (heroId < 0 || heroId >= 5) return;
        _flashHero  = heroId;
        _flashTimer = FlashDuration;
        if (_open) Refresh();
    }

    // ── UI construction ───────────────────────────────────────────────────────
    void BuildUI()
    {
        // Canvas root needs a RectTransform for children to resolve their UI layout.
        if (GetComponent<RectTransform>() == null)
            gameObject.AddComponent<RectTransform>();

        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 94;
        var scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight  = 0.5f;
        gameObject.AddComponent<GraphicRaycaster>();

        // Dim overlay (full screen)
        var dimGO = new GameObject("DimOverlay", typeof(RectTransform));
        dimGO.transform.SetParent(transform, false);
        StretchFull(dimGO);
        _dimOverlay = dimGO.AddComponent<Image>();
        _dimOverlay.color = ColOverlay;

        // Centre panel — 500 wide × 340 tall
        _panel = new GameObject("Panel");
        _panel.transform.SetParent(transform, false);
        var panelRT = _panel.AddComponent<RectTransform>();
        panelRT.anchorMin = new Vector2(0.5f, 0.5f);
        panelRT.anchorMax = new Vector2(0.5f, 0.5f);
        panelRT.pivot     = new Vector2(0.5f, 0.5f);
        panelRT.sizeDelta = new Vector2(520f, 360f);
        panelRT.anchoredPosition = Vector2.zero;
        var panelBg = _panel.AddComponent<Image>();
        panelBg.color = ColPanel;

        // Title
        var titleGO = new GameObject("Title");
        titleGO.transform.SetParent(_panel.transform, false);
        var titleRT = titleGO.AddComponent<RectTransform>();
        titleRT.anchorMin = new Vector2(0f, 0.88f);
        titleRT.anchorMax = new Vector2(1f, 1.00f);
        titleRT.offsetMin = titleRT.offsetMax = Vector2.zero;
        var titleTxt = titleGO.AddComponent<TextMeshProUGUI>();
        titleTxt.text      = "Hero Mastery";
        titleTxt.fontSize  = 18f;
        titleTxt.color     = ColTitle;
        titleTxt.alignment = TextAlignmentOptions.Center;
        titleTxt.fontStyle = FontStyles.Bold;

        // Subtitle
        var subGO = new GameObject("Subtitle");
        subGO.transform.SetParent(_panel.transform, false);
        var subRT = subGO.AddComponent<RectTransform>();
        subRT.anchorMin = new Vector2(0f, 0.80f);
        subRT.anchorMax = new Vector2(1f, 0.88f);
        subRT.offsetMin = subRT.offsetMax = Vector2.zero;
        var subTxt = subGO.AddComponent<TextMeshProUGUI>();
        subTxt.text      = "Level 6 and Level 10 unlock passive bonuses for all heroes";
        subTxt.fontSize  = 9f;
        subTxt.color     = ColDim;
        subTxt.alignment = TextAlignmentOptions.Center;

        // 5 hero cards — horizontal row
        const float cardW   = 88f;
        const float cardH   = 240f;
        const float cardGap = 12f;
        float totalW = 5 * cardW + 4 * cardGap;
        float startX = -totalW / 2f + cardW / 2f;

        for (int i = 0; i < 5; i++)
        {
            float cx = startX + i * (cardW + cardGap);
            _cards[i] = BuildCard(i, cx, -20f, cardW, cardH);
        }

        // Dismiss hint
        var hintGO = new GameObject("Hint");
        hintGO.transform.SetParent(_panel.transform, false);
        var hintRT = hintGO.AddComponent<RectTransform>();
        hintRT.anchorMin = new Vector2(0f, 0f);
        hintRT.anchorMax = new Vector2(1f, 0.10f);
        hintRT.offsetMin = hintRT.offsetMax = Vector2.zero;
        var hintTxt = hintGO.AddComponent<TextMeshProUGUI>();
        hintTxt.text      = "Press H or Esc to close";
        hintTxt.fontSize  = 8f;
        hintTxt.color     = ColDim;
        hintTxt.alignment = TextAlignmentOptions.Center;
    }

    CardRefs BuildCard(int heroIndex, float cx, float cy, float w, float h)
    {
        var refs = new CardRefs();

        var cardGO = new GameObject($"Card_{HeroNames[heroIndex]}");
        cardGO.transform.SetParent(_panel.transform, false);
        var rt = cardGO.AddComponent<RectTransform>();
        rt.anchorMin        = new Vector2(0.5f, 0.5f);
        rt.anchorMax        = new Vector2(0.5f, 0.5f);
        rt.pivot            = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(cx, cy);
        rt.sizeDelta        = new Vector2(w, h);

        // Border outline — first child, extends 1px beyond card bounds
        var borderGO = new GameObject("Border");
        borderGO.transform.SetParent(cardGO.transform, false);
        var borderRT = borderGO.AddComponent<RectTransform>();
        borderRT.anchorMin = Vector2.zero;
        borderRT.anchorMax = Vector2.one;
        borderRT.offsetMin = new Vector2(-1f, -1f);
        borderRT.offsetMax = new Vector2( 1f,  1f);
        refs.borderOutline = borderGO.AddComponent<Image>();
        refs.borderOutline.color = new Color(0.2f, 0.2f, 0.25f, 0.6f);

        // Card background — second child, covers border except the 1px edge
        var bgChildGO = new GameObject("CardBg", typeof(RectTransform));
        bgChildGO.transform.SetParent(cardGO.transform, false);
        StretchFull(bgChildGO);
        refs.bg = bgChildGO.AddComponent<Image>();
        refs.bg.color = ColCard;

        // ── Colour swatch (top strip, hero colour) ────────────────────────────
        var swatchGO = new GameObject("Swatch");
        swatchGO.transform.SetParent(cardGO.transform, false);
        var swatchRT = swatchGO.AddComponent<RectTransform>();
        swatchRT.anchorMin = new Vector2(0f, 0.82f);
        swatchRT.anchorMax = new Vector2(1f, 1.00f);
        swatchRT.offsetMin = swatchRT.offsetMax = Vector2.zero;
        refs.colourSwatch = swatchGO.AddComponent<Image>();
        refs.colourSwatch.color = HeroColors[heroIndex];

        // ── Hero name ─────────────────────────────────────────────────────────
        refs.nameText = AddTMP(cardGO.transform, "Name",
            new Vector2(0f, 0.68f), new Vector2(1f, 0.82f),
            11f, FontStyles.Bold, ColWhite, TextAlignmentOptions.Center);
        refs.nameText.text = HeroNames[heroIndex];

        // ── Level badge ───────────────────────────────────────────────────────
        refs.levelText = AddTMP(cardGO.transform, "Level",
            new Vector2(0f, 0.57f), new Vector2(1f, 0.68f),
            16f, FontStyles.Bold, ColTitle, TextAlignmentOptions.Center);
        refs.levelText.text = "Lv1";

        // ── XP bar background ─────────────────────────────────────────────────
        var xpBgGO = new GameObject("XpBg");
        xpBgGO.transform.SetParent(cardGO.transform, false);
        var xpBgRT = xpBgGO.AddComponent<RectTransform>();
        xpBgRT.anchorMin = new Vector2(0.05f, 0.50f);
        xpBgRT.anchorMax = new Vector2(0.95f, 0.56f);
        xpBgRT.offsetMin = xpBgRT.offsetMax = Vector2.zero;
        var xpBgImg = xpBgGO.AddComponent<Image>();
        xpBgImg.color = new Color(0.05f, 0.05f, 0.07f, 1f);

        // ── XP fill ───────────────────────────────────────────────────────────
        var xpFillGO = new GameObject("XpFill", typeof(RectTransform));
        xpFillGO.transform.SetParent(xpBgGO.transform, false);
        StretchFull(xpFillGO);
        refs.xpBarFill = xpFillGO.AddComponent<Image>();
        refs.xpBarFill.color      = HeroColors[heroIndex];
        refs.xpBarFill.type       = Image.Type.Filled;
        refs.xpBarFill.fillMethod = Image.FillMethod.Horizontal;
        refs.xpBarFill.fillAmount = 0f;

        // ── L6 reward ─────────────────────────────────────────────────────────
        refs.rewardL6 = AddTMP(cardGO.transform, "RewardL6",
            new Vector2(0f, 0.32f), new Vector2(1f, 0.50f),
            8.5f, FontStyles.Normal, ColLocked, TextAlignmentOptions.Center);
        refs.rewardL6.text = $"L6: {RewardTextL6[heroIndex, 1]}";

        // ── L10 reward ────────────────────────────────────────────────────────
        refs.rewardL10 = AddTMP(cardGO.transform, "RewardL10",
            new Vector2(0f, 0.14f), new Vector2(1f, 0.32f),
            8.5f, FontStyles.Normal, ColLocked, TextAlignmentOptions.Center);
        refs.rewardL10.text = $"L10: {RewardTextL10[heroIndex, 1]}";

        return refs;
    }

    // ── UI helpers ────────────────────────────────────────────────────────────
    static TextMeshProUGUI AddTMP(Transform parent, string name,
        Vector2 anchorMin, Vector2 anchorMax,
        float fontSize, FontStyles style, Color color, TextAlignmentOptions align)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
        rt.offsetMin = new Vector2(3f, 0f); rt.offsetMax = new Vector2(-3f, 0f);
        var t = go.AddComponent<TextMeshProUGUI>();
        t.fontSize  = fontSize;
        t.fontStyle = style;
        t.color     = color;
        t.alignment = align;
        return t;
    }

    void StretchFull(GameObject go)
    {
        var r = go.GetComponent<RectTransform>() ?? go.AddComponent<RectTransform>();
        r.anchorMin = Vector2.zero;
        r.anchorMax = Vector2.one;
        r.offsetMin = r.offsetMax = Vector2.zero;
    }
}
#endif
