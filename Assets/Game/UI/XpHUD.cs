#if !UNITY_SERVER
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// XpHUD — self-bootstrapping singleton.
/// Slim XP progress bar anchored below the Gold HUD (top-right corner).
/// Reads from PlayerProgressManager.Local. Updates on OnDataRefreshed and OnLevelUp.
/// Auto-creates Canvas + all UI elements — no Inspector setup required.
/// Sort order 91.
///
/// On level-up: bar flashes white briefly.
/// </summary>
public class XpHUD : MonoBehaviour
{
    public static XpHUD Instance { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        if (Instance != null) return;
        var go = new GameObject("[XpHUD]");
        DontDestroyOnLoad(go);
        Instance = go.AddComponent<XpHUD>();
    }

    // ── Colours ───────────────────────────────────────────────────────────────
    static readonly Color ColFill  = new Color(0.30f, 0.70f, 1.00f, 1f);
    static readonly Color ColBg    = new Color(0.04f, 0.04f, 0.06f, 0.80f);
    static readonly Color ColFlash = Color.white;
    static readonly Color ColText  = new Color(1f, 1f, 1f, 0.90f);
    static readonly Color ColDim   = new Color(0.65f, 0.70f, 0.80f, 0.70f);

    // ── UI refs ───────────────────────────────────────────────────────────────
    Image           _barFill;
    TextMeshProUGUI _levelText;
    TextMeshProUGUI _xpText;

    // ── State ─────────────────────────────────────────────────────────────────
    bool  _subscribed  = false;
    float _flashTimer  = 0f;
    const float FlashDuration = 0.5f;

    // ── Lifecycle ─────────────────────────────────────────────────────────────
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        BuildUI();
    }

    void Update()
    {
        // Late-subscribe once PlayerProgressManager is ready
        if (!_subscribed && PlayerProgressManager.Local != null)
        {
            PlayerProgressManager.Local.OnDataRefreshed += Refresh;
            PlayerProgressManager.Local.OnLevelUp       += OnLevelUp;
            _subscribed = true;
            Refresh();
        }

        // Flash animation
        if (_flashTimer > 0f)
        {
            _flashTimer -= Time.deltaTime;
            float t = _flashTimer / FlashDuration;
            _barFill.color = Color.Lerp(ColFill, ColFlash, t);
            if (_flashTimer <= 0f) _barFill.color = ColFill;
        }
    }

    void OnDisable()
    {
        if (PlayerProgressManager.Local != null && _subscribed)
        {
            PlayerProgressManager.Local.OnDataRefreshed -= Refresh;
            PlayerProgressManager.Local.OnLevelUp       -= OnLevelUp;
        }
        _subscribed = false;
    }

    // ── Refresh ───────────────────────────────────────────────────────────────
    void Refresh()
    {
        var mgr = PlayerProgressManager.Local;
        if (mgr == null) return;
        _levelText.text     = $"Lv{mgr.Level}";
        _xpText.text        = $"{mgr.Xp}/{mgr.XpToNext}";
        _barFill.fillAmount = Mathf.Lerp(_barFill.fillAmount, mgr.XpFraction, Time.deltaTime * 6f);
    }

    void OnLevelUp(int newLevel)
    {
        _flashTimer = FlashDuration;
        Refresh();
    }

    // ── Build UI ──────────────────────────────────────────────────────────────
    void BuildUI()
    {
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 91;
        var scaler = gameObject.AddComponent<UnityEngine.UI.CanvasScaler>();
        scaler.uiScaleMode         = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight  = 0.5f;

        // Root — top-right, below gold HUD
        var root = new GameObject("XpHUDRoot");
        root.transform.SetParent(transform, false);
        var rootRT = root.AddComponent<RectTransform>();
        rootRT.anchorMin        = new Vector2(1f, 1f);
        rootRT.anchorMax        = new Vector2(1f, 1f);
        rootRT.pivot            = new Vector2(1f, 1f);
        rootRT.anchoredPosition = new Vector2(-14f, -48f); // below GoldHUD (14 + 28 + 6)
        rootRT.sizeDelta        = new Vector2(110f, 16f);

        // Background
        root.AddComponent<Image>().color = ColBg;

        // XP fill bar
        var fillGO = new GameObject("XpFill");
        fillGO.transform.SetParent(root.transform, false);
        var fillRT = fillGO.AddComponent<RectTransform>();
        fillRT.anchorMin = new Vector2(0f, 0f);
        fillRT.anchorMax = new Vector2(1f, 1f);
        fillRT.offsetMin = new Vector2(1f, 1f);
        fillRT.offsetMax = new Vector2(-1f, -1f);
        _barFill             = fillGO.AddComponent<Image>();
        _barFill.color       = ColFill;
        _barFill.type        = Image.Type.Filled;
        _barFill.fillMethod  = Image.FillMethod.Horizontal;
        _barFill.fillAmount  = 0f;

        // Level label (left)
        var lvGO = new GameObject("Level");
        lvGO.transform.SetParent(root.transform, false);
        var lvRT = lvGO.AddComponent<RectTransform>();
        lvRT.anchorMin        = new Vector2(0f, 0f);
        lvRT.anchorMax        = new Vector2(0.35f, 1f);
        lvRT.offsetMin        = new Vector2(4f, 0f);
        lvRT.offsetMax        = Vector2.zero;
        _levelText            = lvGO.AddComponent<TextMeshProUGUI>();
        _levelText.text       = "Lv1";
        _levelText.fontSize   = 8f;
        _levelText.color      = ColText;
        _levelText.fontStyle  = FontStyles.Bold;
        _levelText.alignment  = TextAlignmentOptions.Left;

        // XP text (right, dimmed)
        var xpGO = new GameObject("XpValue");
        xpGO.transform.SetParent(root.transform, false);
        var xpRT = xpGO.AddComponent<RectTransform>();
        xpRT.anchorMin        = new Vector2(0.35f, 0f);
        xpRT.anchorMax        = new Vector2(1f, 1f);
        xpRT.offsetMin        = Vector2.zero;
        xpRT.offsetMax        = new Vector2(-4f, 0f);
        _xpText               = xpGO.AddComponent<TextMeshProUGUI>();
        _xpText.text          = "0/100";
        _xpText.fontSize      = 7f;
        _xpText.color         = ColDim;
        _xpText.alignment     = TextAlignmentOptions.Right;
    }
}
#endif
