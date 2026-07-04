using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ShieldValueHUD — World-space shield bar that hovers above a shielded ally.
/// Attach to any shield-source (Cleric ability, buff). Auto-follows target.
/// Hides when shield is depleted or target is lost.
///
/// Copy to: Assets/Game/UI/ShieldValueHUD.cs
///
/// Usage:
///   ShieldValueHUD.ShowFor(allyTransform, maxShield);
///   ShieldValueHUD.UpdateValue(current, max);
///   ShieldValueHUD.Hide();
///
/// Self-creates a world-space canvas — no prefab or scene placement needed.
/// </summary>
#if !UNITY_SERVER
public class ShieldValueHUD : MonoBehaviour
{
    // Singleton — one active shield HUD at a time (extend to a pool for multi-shield)
    public static ShieldValueHUD Instance { get; private set; }

    private Transform  _target;
    private Canvas     _canvas;
    private Image      _fillBar;
    private float      _currentShield;
    private float      _maxShield;
    private Vector3    _offset = new Vector3(0f, 2.4f, 0f);   // hover above head

    static readonly Color ShieldColor = new Color(0.35f, 0.70f, 1f, 0.90f);   // pale blue

    // ─── Bootstrap ────────────────────────────────────────────────────────────
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        if (Instance != null) return;
        var go = new GameObject("[ShieldValueHUD]");
        DontDestroyOnLoad(go);
        go.AddComponent<ShieldValueHUD>();
    }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        BuildCanvas();
    }

    void OnDestroy() { if (Instance == this) Instance = null; }

    // ─── Build UI ─────────────────────────────────────────────────────────────
    void BuildCanvas()
    {
        // World-space canvas
        var canvasGo = new GameObject("ShieldCanvas");
        canvasGo.transform.SetParent(transform);
        _canvas = canvasGo.AddComponent<Canvas>();
        _canvas.renderMode = RenderMode.WorldSpace;
        canvasGo.AddComponent<UnityEngine.UI.CanvasScaler>();

        var rt = canvasGo.GetComponent<RectTransform>();
        rt.sizeDelta   = new Vector2(1.2f, 0.12f);
        rt.localScale  = Vector3.one * 0.01f;

        // Background
        var bg = new GameObject("BG");
        bg.transform.SetParent(canvasGo.transform, false);
        var bgImg   = bg.AddComponent<Image>();
        bgImg.color = new Color(0f, 0f, 0f, 0.5f);
        var bgRt    = bg.GetComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero; bgRt.anchorMax = Vector2.one;
        bgRt.offsetMin = bgRt.offsetMax = Vector2.zero;

        // Fill
        var fill = new GameObject("Fill");
        fill.transform.SetParent(canvasGo.transform, false);
        _fillBar = fill.AddComponent<Image>();
        _fillBar.color = ShieldColor;
        _fillBar.type  = Image.Type.Filled;
        _fillBar.fillMethod = Image.FillMethod.Horizontal;
        var fillRt    = fill.GetComponent<RectTransform>();
        fillRt.anchorMin = Vector2.zero; fillRt.anchorMax = Vector2.one;
        fillRt.offsetMin = fillRt.offsetMax = Vector2.zero;

        canvasGo.SetActive(false);
    }

    // ─── Public API ───────────────────────────────────────────────────────────
    public static void ShowFor(Transform target, float maxShield)
    {
        if (Instance == null) return;
        Instance._target     = target;
        Instance._maxShield  = maxShield;
        Instance._currentShield = maxShield;
        Instance._canvas.gameObject.SetActive(true);
        Instance.UpdateFill();
    }

    public static void UpdateValue(float current, float max)
    {
        if (Instance == null) return;
        Instance._currentShield = current;
        Instance._maxShield     = max;
        Instance.UpdateFill();
        if (current <= 0f) Hide();
    }

    public static void Hide()
    {
        if (Instance == null) return;
        Instance._canvas.gameObject.SetActive(false);
        Instance._target = null;
    }

    // ─── Update ───────────────────────────────────────────────────────────────
    void Update()
    {
        if (_target == null) { Hide(); return; }

        // Follow target in world space
        _canvas.transform.position = _target.position + _offset;

        // Face camera
        var cam = Camera.main;
        if (cam != null)
            _canvas.transform.forward = cam.transform.forward;
    }

    void UpdateFill()
    {
        if (_fillBar == null) return;
        _fillBar.fillAmount = _maxShield > 0f ? Mathf.Clamp01(_currentShield / _maxShield) : 0f;
    }
}
#endif
