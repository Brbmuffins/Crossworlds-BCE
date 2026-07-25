#if UNITY_EDITOR || !UNITY_SERVER
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// HeroMasteryHUD — self-bootstrapping slim overlay.
///
/// Shows the CURRENT class's mastery XP bar anchored to the bottom of the screen,
/// above the ability bar. Pressing H toggles the full HeroMasteryUI panel open/closed.
///
/// On level-up: bar flashes gold then fades back to the class colour over 0.6s.
///
/// Sort order 93 — above ability bar (90) and ShieldValueHUD, below StatusEffectHUD (95).
///
/// Reads:
///   HeroMasteryManager.Local.Masteries[classIdx] — level, XpFraction
///   PlayerProgressManager.Local.ClassIndex        — which hero bar to show
/// </summary>
public class HeroMasteryHUD : MonoBehaviour
{
    // ── Bootstrap ─────────────────────────────────────────────────────────────
    public static HeroMasteryHUD Instance { get; private set; }

    // [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Bootstrap()
    {
        if (Instance != null) return;
        var go = new GameObject("[HeroMasteryHUD]");
        DontDestroyOnLoad(go);
        Instance = go.AddComponent<HeroMasteryHUD>();
    }

    // ── Colours ───────────────────────────────────────────────────────────────
    static readonly Color ColFill   = new Color(0.95f, 0.75f, 0.10f, 1.00f); // default gold
    static readonly Color ColFlash  = Color.white;
    static readonly Color ColBg     = new Color(0.05f, 0.05f, 0.08f, 0.82f);
    static readonly Color ColText   = new Color(1.00f, 1.00f, 1.00f, 0.90f);
    static readonly Color ColLabel  = new Color(0.75f, 0.75f, 0.85f, 0.80f);

    // Class fill colours (one per hero 0-4, matches ClassColor in ClericRadarUI)
    static readonly Color[] ClassColors =
    {
        new Color(0.40f, 0.80f, 0.40f, 1f), // 0 Marauder — green
        new Color(0.60f, 0.60f, 0.75f, 1f), // 1 Ironclad — silver-blue
        new Color(0.60f, 0.10f, 0.80f, 1f), // 2 Shadowblade — purple
        new Color(0.95f, 0.80f, 0.20f, 1f), // 3 Cleric   — gold
        new Color(0.30f, 0.55f, 1.00f, 1f), // 4 Arcanist — blue
    };

    // ── UI refs ───────────────────────────────────────────────────────────────
    GameObject      _root;
    Image           _bg;
    Image           _barFill;
    TextMeshProUGUI _levelText;
    TextMeshProUGUI _labelText; // "HERO MASTERY"

    // ── State ─────────────────────────────────────────────────────────────────
    bool  _visible     = true;
    float _flashTimer  = 0f;
    const float FlashDuration = 0.6f;

    int   _lastClass   = -1;

    // ── Lifecycle ─────────────────────────────────────────────────────────────
    void Awake()
    {
        BuildUI();
        if (HeroMasteryManager.Local != null)
        {
            HeroMasteryManager.Local.OnMasteryChanged += OnMasteryChanged;
            HeroMasteryManager.Local.OnHeroLevelUp    += OnHeroLevelUp;
        }
    }

    void OnDestroy()
    {
        if (HeroMasteryManager.Local != null)
        {
            HeroMasteryManager.Local.OnMasteryChanged -= OnMasteryChanged;
            HeroMasteryManager.Local.OnHeroLevelUp    -= OnHeroLevelUp;
        }
    }

    void Update()
    {
        // Flash animation
        if (_flashTimer > 0f)
        {
            _flashTimer -= Time.deltaTime;
            float t = _flashTimer / FlashDuration;
            _barFill.color = Color.Lerp(GetClassColor(), ColFlash, t);
        }

        // Refresh bar whenever class or data changes
        int classIdx = PlayerProgressManager.Local?.ClassIndex ?? 0;
        if (classIdx != _lastClass)
        {
            _lastClass = classIdx;
            RefreshBar();
        }
    }

    // ── Event handlers ────────────────────────────────────────────────────────
    void OnMasteryChanged()           => RefreshBar();
    void OnHeroLevelUp(int hero, int lv) { if (hero == _lastClass) TriggerFlash(); }

    // ── Public ────────────────────────────────────────────────────────────────
    public void SetVisible(bool vis)
    {
        _visible = vis;
        _root.SetActive(vis);
    }

    // ── Internal ──────────────────────────────────────────────────────────────
    void RefreshBar()
    {
        var mgr = HeroMasteryManager.Local;
        if (mgr == null) return;

        int classIdx = PlayerProgressManager.Local?.ClassIndex ?? 0;
        if (classIdx < 0 || classIdx >= mgr.Masteries.Length) return;

        var entry = mgr.Masteries[classIdx];
        _levelText.text = $"Lv{entry.level}";

        // Animate bar fill
        float target = entry.XpFraction;
        _barFill.fillAmount = Mathf.Lerp(_barFill.fillAmount, target, Time.deltaTime * 5f);

        // Update class colour if not flashing
        if (_flashTimer <= 0f)
            _barFill.color = GetClassColor();
    }

    void TriggerFlash()
    {
        _flashTimer    = FlashDuration;
        _barFill.color = ColFlash;
    }

    Color GetClassColor()
    {
        int idx = PlayerProgressManager.Local?.ClassIndex ?? 0;
        return (idx >= 0 && idx < ClassColors.Length) ? ClassColors[idx] : ColFill;
    }

    // ── UI construction ───────────────────────────────────────────────────────
    void BuildUI()
    {
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 93;
        var scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight  = 0.5f;

        // Root panel — slim bar, anchored bottom-centre
        _root = new GameObject("MasteryHUDRoot");
        _root.transform.SetParent(transform, false);
        var rootRect = _root.AddComponent<RectTransform>();
        rootRect.anchorMin        = new Vector2(0.20f, 0f);
        rootRect.anchorMax        = new Vector2(0.80f, 0f);
        rootRect.pivot            = new Vector2(0.5f, 0f);
        rootRect.anchoredPosition = new Vector2(0f, 46f); // above ability bar footprint
        rootRect.sizeDelta        = new Vector2(0f, 18f);

        // Background
        var bgGO = new GameObject("BG", typeof(RectTransform));
        bgGO.transform.SetParent(_root.transform, false);
        StretchFull(bgGO);
        _bg = bgGO.AddComponent<Image>();
        _bg.color = ColBg;

        // XP fill bar
        var barGO = new GameObject("Fill");
        barGO.transform.SetParent(_root.transform, false);
        var barRect = barGO.AddComponent<RectTransform>();
        barRect.anchorMin = Vector2.zero;
        barRect.anchorMax = Vector2.one;
        barRect.offsetMin = new Vector2(2f, 2f);
        barRect.offsetMax = new Vector2(-2f, -2f);
        _barFill = barGO.AddComponent<Image>();
        _barFill.color      = ColFill;
        _barFill.type       = Image.Type.Filled;
        _barFill.fillMethod = Image.FillMethod.Horizontal;
        _barFill.fillAmount = 0f;

        // Level label (left side)
        var lvGO = new GameObject("Level");
        lvGO.transform.SetParent(_root.transform, false);
        var lvRect = lvGO.AddComponent<RectTransform>();
        lvRect.anchorMin        = new Vector2(0f, 0f);
        lvRect.anchorMax        = new Vector2(0.12f, 1f);
        lvRect.offsetMin        = new Vector2(4f, 0f);
        lvRect.offsetMax        = Vector2.zero;
        _levelText = lvGO.AddComponent<TextMeshProUGUI>();
        _levelText.fontSize  = 10f;
        _levelText.color     = ColText;
        _levelText.alignment = TextAlignmentOptions.Left;
        _levelText.fontStyle = FontStyles.Bold;
        _levelText.text      = "Lv1";

        // "HERO MASTERY" label (right side, dimmed)
        var labelGO = new GameObject("Label");
        labelGO.transform.SetParent(_root.transform, false);
        var labelRect = labelGO.AddComponent<RectTransform>();
        labelRect.anchorMin        = new Vector2(0.85f, 0f);
        labelRect.anchorMax        = new Vector2(1f,    1f);
        labelRect.offsetMin        = Vector2.zero;
        labelRect.offsetMax        = new Vector2(-4f, 0f);
        _labelText = labelGO.AddComponent<TextMeshProUGUI>();
        _labelText.fontSize  = 7f;
        _labelText.color     = ColLabel;
        _labelText.alignment = TextAlignmentOptions.Right;
        _labelText.text      = "MASTERY";
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
