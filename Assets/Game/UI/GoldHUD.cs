#if !UNITY_SERVER
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// GoldHUD — self-bootstrapping singleton.
/// Displays the local player's current gold in the top-right corner.
/// Reads from PlayerProgressManager.Local and updates on OnDataRefreshed.
/// Auto-creates its own Canvas and UI elements — no Inspector setup required.
/// Sort order 91.
/// </summary>
public class GoldHUD : MonoBehaviour
{
    public static GoldHUD Instance { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Bootstrap()
    {
        if (Instance != null) return;
        var go = new GameObject("[GoldHUD]");
        DontDestroyOnLoad(go);
        Instance = go.AddComponent<GoldHUD>();
    }

    // ── Colours ───────────────────────────────────────────────────────────────
    static readonly Color ColGold  = new Color(1.00f, 0.82f, 0.10f, 1f);
    static readonly Color ColBg    = new Color(0.04f, 0.04f, 0.06f, 0.80f);
    static readonly Color ColLabel = new Color(0.75f, 0.65f, 0.40f, 0.80f);

    // ── UI refs ───────────────────────────────────────────────────────────────
    TextMeshProUGUI _goldText;
    TextMeshProUGUI _labelText;

    // ── State ─────────────────────────────────────────────────────────────────
    int  _lastGold   = -1;
    bool _subscribed = false;

    // ── Lifecycle ─────────────────────────────────────────────────────────────
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        BuildUI();
    }

    void OnDisable()
    {
        if (_subscribed && PlayerProgressManager.Local != null)
            PlayerProgressManager.Local.OnDataRefreshed -= Refresh;
        _subscribed = false;
    }

    void Update()
    {
        // Late-subscribe once PlayerProgressManager is ready (single subscription)
        if (!_subscribed && PlayerProgressManager.Local != null)
        {
            PlayerProgressManager.Local.OnDataRefreshed += Refresh;
            _subscribed = true;
            Refresh();
        }
    }

    // ── Refresh ───────────────────────────────────────────────────────────────
    void Refresh()
    {
        var mgr = PlayerProgressManager.Local;
        if (mgr == null) return;
        if (mgr.Gold == _lastGold) return;
        _lastGold  = mgr.Gold;
        _goldText.text = mgr.Gold.ToString("N0");
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

        // Root pill — top-right corner
        var root = new GameObject("GoldHUDRoot");
        root.transform.SetParent(transform, false);
        var rootRT = root.AddComponent<RectTransform>();
        rootRT.anchorMin        = new Vector2(1f, 1f);
        rootRT.anchorMax        = new Vector2(1f, 1f);
        rootRT.pivot            = new Vector2(1f, 1f);
        rootRT.anchoredPosition = new Vector2(-14f, -14f);
        rootRT.sizeDelta        = new Vector2(110f, 28f);

        // Background
        var bg = root.AddComponent<Image>();
        bg.color = ColBg;

        // "G" label
        var labelGO = new GameObject("GLabel");
        labelGO.transform.SetParent(root.transform, false);
        var labelRT = labelGO.AddComponent<RectTransform>();
        labelRT.anchorMin        = new Vector2(0f, 0f);
        labelRT.anchorMax        = new Vector2(0f, 1f);
        labelRT.offsetMin        = new Vector2(6f,  0f);
        labelRT.offsetMax        = new Vector2(22f, 0f);
        _labelText              = labelGO.AddComponent<TextMeshProUGUI>();
        _labelText.text         = "G";
        _labelText.fontSize     = 11f;
        _labelText.color        = ColLabel;
        _labelText.fontStyle    = FontStyles.Bold;
        _labelText.alignment    = TextAlignmentOptions.Center;

        // Gold value
        var valGO = new GameObject("GoldValue");
        valGO.transform.SetParent(root.transform, false);
        var valRT = valGO.AddComponent<RectTransform>();
        valRT.anchorMin        = new Vector2(0f, 0f);
        valRT.anchorMax        = new Vector2(1f, 1f);
        valRT.offsetMin        = new Vector2(24f, 0f);
        valRT.offsetMax        = new Vector2(-6f, 0f);
        _goldText              = valGO.AddComponent<TextMeshProUGUI>();
        _goldText.text         = "0";
        _goldText.fontSize     = 13f;
        _goldText.color        = ColGold;
        _goldText.fontStyle    = FontStyles.Bold;
        _goldText.alignment    = TextAlignmentOptions.Right;
    }
}
#endif
