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
    CanvasGroup      _cg;

    float _displayFraction = 0f;
    float _targetFraction  = 0f;

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
        PlayerProgressManager.Local.OnDataRefreshed += OnRefreshed;
        PlayerProgressManager.Local.OnLevelUp       += OnLevelUp;
    }

    void Update()
    {
        if (_fill == null) return;
        _displayFraction = Mathf.MoveTowards(_displayFraction, _targetFraction, Time.deltaTime * 1.2f);
        _fill.fillAmount = _displayFraction;
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
        _levelText.text = $"Lv {newLevel}";
        StartCoroutine(LevelUpFlash());
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
        _fill.type     = Image.Type.Filled;
        _fill.fillMethod = Image.FillMethod.Horizontal;
        _fill.fillAmount = 0f;
        _fill.raycastTarget = false;
        StretchFull(fillGO.GetComponent<RectTransform>());

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
    }

    static void StretchFull(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }
}
#endif
