#if !UNITY_SERVER
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// CharacterStatsHUD — self-bootstrapping singleton.
/// Shows STR / AGI / INT / VIT in a compact panel (bottom-left by default).
/// Reads from PlayerProgressManager.Local. Updates on OnDataRefreshed.
/// Press C to toggle open/closed.
/// Auto-creates Canvas + all UI — no Inspector setup required.
/// Sort order 92.
/// </summary>
public class CharacterStatsHUD : MonoBehaviour
{
    public static CharacterStatsHUD Instance { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Bootstrap()
    {
        if (Instance != null) return;
        var go = new GameObject("[CharacterStatsHUD]");
        DontDestroyOnLoad(go);
        Instance = go.AddComponent<CharacterStatsHUD>();
    }

    // ── Colours ───────────────────────────────────────────────────────────────
    static readonly Color ColBg     = new Color(0.04f, 0.04f, 0.08f, 0.88f);
    static readonly Color ColTitle  = new Color(0.75f, 0.75f, 0.90f, 0.90f);
    static readonly Color ColStr    = new Color(1.00f, 0.38f, 0.30f, 1f);
    static readonly Color ColAgi    = new Color(0.30f, 0.90f, 0.40f, 1f);
    static readonly Color ColInt    = new Color(0.35f, 0.60f, 1.00f, 1f);
    static readonly Color ColVit    = new Color(0.95f, 0.80f, 0.25f, 1f);
    static readonly Color ColVal    = new Color(1.00f, 1.00f, 1.00f, 0.90f);

    // ── UI refs ───────────────────────────────────────────────────────────────
    GameObject      _panel;
    TextMeshProUGUI _strVal, _agiVal, _intVal, _vitVal;

    // ── State ─────────────────────────────────────────────────────────────────
    bool _open       = false;
    bool _subscribed = false;

    // ── Lifecycle ─────────────────────────────────────────────────────────────
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        BuildUI();
        _panel.SetActive(false);
    }

    void Update()
    {
        // Late-subscribe once manager is ready
        if (!_subscribed && PlayerProgressManager.Local != null)
        {
            PlayerProgressManager.Local.OnDataRefreshed += Refresh;
            _subscribed = true;
            Refresh();
        }

        // C key toggles panel
        if (UnityEngine.InputSystem.Keyboard.current?.cKey.wasPressedThisFrame == true)
            Toggle();
    }

    void OnDisable()
    {
        if (_subscribed && PlayerProgressManager.Local != null)
            PlayerProgressManager.Local.OnDataRefreshed -= Refresh;
        _subscribed = false;
    }

    // ── Public ────────────────────────────────────────────────────────────────
    public void Toggle() { _open = !_open; _panel.SetActive(_open); if (_open) Refresh(); }

    // ── Refresh ───────────────────────────────────────────────────────────────
    void Refresh()
    {
        var m = PlayerProgressManager.Local;
        if (m == null || !_open) return;
        _strVal.text = m.StatStr.ToString();
        _agiVal.text = m.StatAgi.ToString();
        _intVal.text = m.StatInt.ToString();
        _vitVal.text = m.StatVit.ToString();
    }

    // ── Build UI ──────────────────────────────────────────────────────────────
    void BuildUI()
    {
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 92;
        var scaler = gameObject.AddComponent<UnityEngine.UI.CanvasScaler>();
        scaler.uiScaleMode         = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight  = 0.5f;
        gameObject.AddComponent<GraphicRaycaster>();

        // Panel — bottom-left
        _panel = new GameObject("StatsPanel");
        _panel.transform.SetParent(transform, false);
        var pRT = _panel.AddComponent<RectTransform>();
        pRT.anchorMin        = new Vector2(0f, 0f);
        pRT.anchorMax        = new Vector2(0f, 0f);
        pRT.pivot            = new Vector2(0f, 0f);
        pRT.anchoredPosition = new Vector2(14f, 14f);
        pRT.sizeDelta        = new Vector2(110f, 100f);
        _panel.AddComponent<Image>().color = ColBg;

        // Title
        var titleGO = MakeTMP("Title", _panel.transform,
            new Vector2(0f, 0.84f), new Vector2(1f, 1f));
        titleGO.text      = "<size=9>STATS  <size=7><color=#8888AA>C to close</color></size></size>";
        titleGO.color     = ColTitle;
        titleGO.alignment = TextAlignmentOptions.Center;

        // 4 stat rows
        float[] anchY = { 0.62f, 0.40f, 0.18f, 0.0f };
        string[] names = { "STR", "AGI", "INT", "VIT" };
        Color[]  cols  = { ColStr, ColAgi, ColInt, ColVit };

        for (int i = 0; i < 4; i++)
        {
            float bot = anchY[i], top = (i < 3 ? anchY[i] + 0.20f : 0.18f);
            // Label
            var lbl = MakeTMP($"Lbl{names[i]}", _panel.transform,
                new Vector2(0f, bot), new Vector2(0.45f, bot + 0.20f));
            lbl.text      = names[i];
            lbl.color     = cols[i];
            lbl.fontSize  = 10f;
            lbl.fontStyle = FontStyles.Bold;
            lbl.alignment = TextAlignmentOptions.Left;
            var lRT = lbl.GetComponent<RectTransform>();
            lRT.offsetMin = new Vector2(8f, 0f); lRT.offsetMax = Vector2.zero;

            // Value
            var val = MakeTMP($"Val{names[i]}", _panel.transform,
                new Vector2(0.45f, bot), new Vector2(1f, bot + 0.20f));
            val.color     = ColVal;
            val.fontSize  = 10f;
            val.alignment = TextAlignmentOptions.Right;
            var vRT = val.GetComponent<RectTransform>();
            vRT.offsetMin = Vector2.zero; vRT.offsetMax = new Vector2(-8f, 0f);

            if      (i == 0) _strVal = val;
            else if (i == 1) _agiVal = val;
            else if (i == 2) _intVal = val;
            else              _vitVal = val;
        }
    }

    static TextMeshProUGUI MakeTMP(string name, Transform parent, Vector2 anchMin, Vector2 anchMax)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchMin; rt.anchorMax = anchMax;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        var t = go.AddComponent<TextMeshProUGUI>();
        t.fontSize  = 10f;
        t.color     = Color.white;
        t.alignment = TextAlignmentOptions.Left;
        return t;
    }
}
#endif
