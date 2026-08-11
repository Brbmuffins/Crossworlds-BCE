#if UNITY_EDITOR || !UNITY_SERVER
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

/// <summary>
/// XpBar — action-bar HUD. Displays current XP progress and level.
/// Reads from PlayerProgressManager.Local.
/// Self-bootstrapping via RuntimeInitializeOnLoadMethod.
/// Animates smoothly toward target fill. Flashes gold on level-up.
/// </summary>
public class XpBar : MonoBehaviour
{
    // ── Bootstrap ─────────────────────────────────────────────────────────────
    private static XpBar _instance;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Bootstrap()
    {
        if (_instance != null) return;
        var go = new GameObject("[XpBar]");
        DontDestroyOnLoad(go);
        _instance = go.AddComponent<XpBar>();
    }

    // ── UI refs ───────────────────────────────────────────────────────────────
    Canvas           _canvas;
    Image            _fill;
    Image            _fillBg;
    TextMeshProUGUI  _levelText;
    TextMeshProUGUI  _xpText;
    TextMeshProUGUI  _gainText;
    CanvasGroup      _cg;

    float _displayFraction = 0f;
    float _targetFraction  = 0f;
    float _fillVelocity;
    int _activeGainCount;

    const int   CanvasOrder       = 99;
    const float ActionBarXpWidth  = 530f;
    const float ActionBarXpHeight = 30f;
    const float ActionBarXpY      = 190f;

    static readonly Color NormalFill  = new Color(0.52f, 0.18f, 0.72f);
    static readonly Color NormalBg    = new Color(0.08f, 0.04f, 0.14f, 1.00f);
    static readonly Color LevelUpFill = new Color(1.0f,  0.80f, 0.10f);

    // ── Lifecycle ─────────────────────────────────────────────────────────────
    void Awake()
    {
        BuildUI();
        SceneManager.sceneLoaded += OnSceneLoaded;
        ApplySceneVisibility(SceneManager.GetActiveScene());
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplySceneVisibility(scene);
    }

    void ApplySceneVisibility(Scene scene)
    {
        if (_canvas == null) return;
        string sceneName = scene.name;
        bool menuScene = string.IsNullOrEmpty(sceneName)
            || sceneName == "Login"
            || sceneName == "LoginScene"
            || sceneName == "CharacterSelect";
        _canvas.enabled = !menuScene;
    }

    void OnEnable()
    {
        if (PlayerProgressManager.Local != null)
            Subscribe();
        // else: WaitForManager coroutine handles late bootstrap
        StartCoroutine(WaitForManager());
    }

    void OnDisable()
    {
        if (PlayerProgressManager.Local != null)
        {
            PlayerProgressManager.Local.OnDataRefreshed -= OnRefreshed;
            PlayerProgressManager.Local.OnLevelUp       -= OnLevelUp;
            PlayerProgressManager.Local.OnXpGained      -= OnXpGained;
        }
    }

    IEnumerator WaitForManager()
    {
        while (PlayerProgressManager.Local == null)
            yield return null;
        Subscribe();
        OnRefreshed(); // populate immediately
    }

    void Subscribe()
    {
        PlayerProgressManager.Local.OnDataRefreshed -= OnRefreshed;
        PlayerProgressManager.Local.OnLevelUp       -= OnLevelUp;
        PlayerProgressManager.Local.OnXpGained      -= OnXpGained;
        PlayerProgressManager.Local.OnDataRefreshed += OnRefreshed;
        PlayerProgressManager.Local.OnLevelUp       += OnLevelUp;
        PlayerProgressManager.Local.OnXpGained      += OnXpGained;
    }

    void Update()
    {
        if (_fill == null) return;
        _displayFraction = Mathf.SmoothDamp(
            _displayFraction,
            _targetFraction,
            ref _fillVelocity,
            0.35f,
            Mathf.Infinity,
            Time.unscaledDeltaTime);
        if (Mathf.Abs(_displayFraction - _targetFraction) < 0.0005f)
            _displayFraction = _targetFraction;
        SetFillFraction(_displayFraction);
    }

    // ── Callbacks ─────────────────────────────────────────────────────────────
    void OnRefreshed()
    {
        var pm = PlayerProgressManager.Local;
        _targetFraction  = pm.XpFraction;
        _levelText.text  = $"Lv {pm.Level}";
        _xpText.text     = $"{pm.Xp} / {pm.XpToNext} XP";
        _fill.color      = NormalFill;
    }

    void OnLevelUp(int newLevel)
    {
        _targetFraction = 0f;
        _displayFraction = 0f;
        _fillVelocity = 0f;
        SetFillFraction(0f);
        _levelText.text = $"Lv {newLevel}";
        StartCoroutine(LevelUpFlash());
    }

    void OnXpGained(int amount)
    {
        if (amount <= 0 || _gainText == null) return;

        // Give every confirmed kill its own label. Rapid kills therefore stack
        // briefly instead of replacing an earlier reward before it can fade.
        var gain = Instantiate(_gainText, _gainText.transform.parent);
        gain.gameObject.name = "XpGainText_Floating";
        gain.gameObject.SetActive(true);
        StartCoroutine(ShowXpGain(gain, amount, _activeGainCount++));
    }

    IEnumerator ShowXpGain(TextMeshProUGUI gain, int amount, int stackIndex)
    {
        gain.text = $"+{amount} XP";
        RectTransform rt = gain.rectTransform;
        Vector2 start = new Vector2(0f, 22f + Mathf.Min(stackIndex, 7) * 24f);
        Vector3 baseScale = rt.localScale;
        Color color = new Color(0.82f, 0.62f, 1f, 1f);
        float duration = 1.5f;

        for (float t = 0f; t < duration; t += Time.deltaTime)
        {
            float p = Mathf.Clamp01(t / duration);
            float eased = Mathf.SmoothStep(0f, 1f, p);
            rt.anchoredPosition = start + Vector2.up * (38f * eased);
            rt.localScale = baseScale * Mathf.Lerp(1.85f, 0.75f, eased);
            color.a = 1f - Mathf.Clamp01((p - 0.55f) / 0.45f);
            gain.color = color;
            yield return null;
        }

        Destroy(gain.gameObject);
        _activeGainCount = Mathf.Max(0, _activeGainCount - 1);
    }

    IEnumerator LevelUpFlash()
    {
        // Flash gold three times
        for (int i = 0; i < 3; i++)
        {
            _fill.color = LevelUpFill;
            _fillBg.color = new Color(0.6f, 0.5f, 0f, 0.5f);
            yield return new WaitForSeconds(0.18f);
            _fill.color = NormalFill;
            _fillBg.color = NormalBg;
            yield return new WaitForSeconds(0.18f);
        }

        // Punch scale on level label
        float t = 0f;
        while (t < 0.3f)
        {
            t += Time.deltaTime;
            float s = 1f + Mathf.Sin(t / 0.3f * Mathf.PI) * 0.4f;
            _levelText.transform.localScale = Vector3.one * s;
            yield return null;
        }
        _levelText.transform.localScale = Vector3.one;
    }

    // ── Build UI ──────────────────────────────────────────────────────────────
    void BuildUI()
    {
        // Canvas
        var cgo = new GameObject("XpBarCanvas",
            typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(CanvasGroup));
        DontDestroyOnLoad(cgo);
        _canvas = cgo.GetComponent<Canvas>();
        _canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = CanvasOrder;
        _cg = cgo.GetComponent<CanvasGroup>();
        _cg.blocksRaycasts = false;
        _cg.interactable   = false;
        _cg.alpha          = 0.95f;

        var scaler = cgo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight  = 0.5f;

        var root = _canvas.GetComponent<RectTransform>();

        // Sits in the top channel of PlayerHUD, between the wells.
        var container = new GameObject("XpBarContainer", typeof(RectTransform));
        container.transform.SetParent(root, false);
        var cRt = container.GetComponent<RectTransform>();
        cRt.anchorMin        = new Vector2(0.5f, 0f);
        cRt.anchorMax        = new Vector2(0.5f, 0f);
        cRt.pivot            = new Vector2(0.5f, 0.5f);
        cRt.anchoredPosition = new Vector2(0f, ActionBarXpY);
        cRt.sizeDelta        = new Vector2(ActionBarXpWidth, ActionBarXpHeight);

        // Background
        var bgGO = new GameObject("BG", typeof(RectTransform), typeof(Image));
        bgGO.transform.SetParent(container.transform, false);
        _fillBg = bgGO.GetComponent<Image>();
        _fillBg.color = NormalBg;
        _fillBg.raycastTarget = false;
        StretchFull(bgGO.GetComponent<RectTransform>());

        // Fill
        var fillGO = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        fillGO.transform.SetParent(container.transform, false);
        _fill = fillGO.GetComponent<Image>();
        _fill.color    = NormalFill;
        // This runtime Image has no source sprite. Image.Type.Filled does not
        // reliably clip a sprite-less image and can render as a solid full bar.
        // Resize its RectTransform from the left edge instead.
        _fill.type     = Image.Type.Simple;
        _fill.raycastTarget = false;
        SetFillFraction(0f);

        // Thin highlight line at top
        var shine = new GameObject("Shine", typeof(RectTransform), typeof(Image));
        shine.transform.SetParent(container.transform, false);
        var shineImage = shine.GetComponent<Image>();
        shineImage.color = new Color(1f, 1f, 1f, 0.12f);
        shineImage.raycastTarget = false;
        var shRt = shine.GetComponent<RectTransform>();
        shRt.anchorMin = new Vector2(0f, 1f); shRt.anchorMax = Vector2.one;
        shRt.offsetMin = new Vector2(0f, -2f); shRt.offsetMax = Vector2.zero;

        // Level label — tucked into the left side of the bar.
        var lvGO = new GameObject("LevelText", typeof(RectTransform), typeof(TextMeshProUGUI));
        lvGO.transform.SetParent(container.transform, false);
        _levelText = lvGO.GetComponent<TextMeshProUGUI>();
        _levelText.text      = "Lv 1";
        _levelText.fontSize  = 9f;
        _levelText.fontStyle = FontStyles.Bold;
        _levelText.color     = new Color(1f, 1f, 1f, 0.90f);
        _levelText.alignment = TextAlignmentOptions.MidlineLeft;
        _levelText.raycastTarget = false;
        var lvRt = lvGO.GetComponent<RectTransform>();
        lvRt.anchorMin = new Vector2(0f, 0f); lvRt.anchorMax = new Vector2(0.22f, 1f);
        lvRt.offsetMin = new Vector2(8f, 0f); lvRt.offsetMax = Vector2.zero;

        // XP label — centred on bar
        var xpGO = new GameObject("XpText", typeof(RectTransform), typeof(TextMeshProUGUI));
        xpGO.transform.SetParent(container.transform, false);
        _xpText = xpGO.GetComponent<TextMeshProUGUI>();
        _xpText.text      = "0 / 100 XP";
        _xpText.fontSize  = 9f;
        _xpText.color     = new Color(0.90f, 0.86f, 1f, 0.90f);
        _xpText.alignment = TextAlignmentOptions.Center;
        _xpText.raycastTarget = false;
        StretchFull(xpGO.GetComponent<RectTransform>());

        var gainGO = new GameObject("XpGainText", typeof(RectTransform), typeof(TextMeshProUGUI));
        gainGO.transform.SetParent(container.transform, false);
        _gainText = gainGO.GetComponent<TextMeshProUGUI>();
        _gainText.text = "";
        _gainText.fontSize = 16f;
        _gainText.fontStyle = FontStyles.Bold;
        _gainText.alignment = TextAlignmentOptions.Center;
        _gainText.raycastTarget = false;
        var gainRt = gainGO.GetComponent<RectTransform>();
        gainRt.anchorMin = new Vector2(0f, 1f);
        gainRt.anchorMax = new Vector2(1f, 1f);
        gainRt.pivot = new Vector2(0.5f, 0f);
        gainRt.anchoredPosition = new Vector2(0f, 22f);
        gainRt.sizeDelta = new Vector2(0f, 26f);
    }

    static void StretchFull(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    void SetFillFraction(float fraction)
    {
        if (_fill == null) return;
        RectTransform rt = _fill.rectTransform;
        float clamped = Mathf.Clamp01(fraction);
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = new Vector2(clamped, 1f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }
}
#endif
