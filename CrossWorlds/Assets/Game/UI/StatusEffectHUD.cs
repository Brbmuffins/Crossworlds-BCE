using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// StatusEffectHUD — Row of up to 6 icons showing the local player's
/// active status effects (Slow, Stagger, Silenced, Cursed, Weakened, Bound)
/// with a countdown timer below each icon.
///
/// Copy to: Assets/Game/UI/StatusEffectHUD.cs
/// Self-bootstrapping — auto-creates Canvas. No scene setup needed.
///
/// Wire-up: StatusEffectManager should call:
///   StatusEffectHUD.AddEffect(name, duration);
///   StatusEffectHUD.RemoveEffect(name);
///
/// If StatusEffectManager pushes its own list, swap the static calls
/// for event subscriptions in Awake().
/// </summary>
#if !UNITY_SERVER
public class StatusEffectHUD : MonoBehaviour
{
    public static StatusEffectHUD Instance { get; private set; }

    private const int MAX_SLOTS = 6;

    // Color per effect type
    static readonly Dictionary<string, Color> EffectColors = new Dictionary<string, Color>
    {
        { "Slow",      new Color(0.40f, 0.60f, 1.00f) },   // blue
        { "Stagger",   new Color(1.00f, 0.60f, 0.10f) },   // orange
        { "Silenced",  new Color(0.70f, 0.20f, 0.70f) },   // purple
        { "Cursed",    new Color(0.20f, 0.80f, 0.20f) },   // green
        { "Weakened",  new Color(1.00f, 0.20f, 0.20f) },   // red
        { "Bound",     new Color(0.90f, 0.85f, 0.20f) },   // yellow
    };

    // Active effects tracked locally: effectName → expiry time
    private readonly Dictionary<string, float> _active = new Dictionary<string, float>();

    private Canvas _canvas;
    private readonly List<EffectSlot> _slots = new List<EffectSlot>();

    struct EffectSlot
    {
        public GameObject go;
        public Image      bg;
        public TextMeshProUGUI nameLabel;
        public TextMeshProUGUI timerLabel;
    }

    // ─── Bootstrap ────────────────────────────────────────────────────────────
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        if (Instance != null) return;
        var go = new GameObject("[StatusEffectHUD]");
        DontDestroyOnLoad(go);
        go.AddComponent<StatusEffectHUD>();
    }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        BuildCanvas();
    }

    void OnDestroy() { if (Instance == this) Instance = null; }

    // ─── Public API ───────────────────────────────────────────────────────────
    public static void AddEffect(string effectName, float duration)
    {
        if (Instance == null) return;
        Instance._active[effectName] = Time.time + duration;
        Instance.RefreshSlots();
    }

    public static void RemoveEffect(string effectName)
    {
        if (Instance == null) return;
        Instance._active.Remove(effectName);
        Instance.RefreshSlots();
    }

    // ─── Build Canvas ─────────────────────────────────────────────────────────
    void BuildCanvas()
    {
        var canvasGo = new GameObject("StatusEffectCanvas");
        canvasGo.transform.SetParent(transform);
        _canvas = canvasGo.AddComponent<Canvas>();
        _canvas.renderMode  = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = 6;
        canvasGo.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasGo.AddComponent<GraphicRaycaster>();

        // Row pinned above the player health bar (bottom-centre)
        var row = new GameObject("EffectRow");
        row.transform.SetParent(canvasGo.transform, false);
        var rowRt = row.AddComponent<RectTransform>();
        rowRt.anchorMin = new Vector2(0.5f, 0f);
        rowRt.anchorMax = new Vector2(0.5f, 0f);
        rowRt.pivot     = new Vector2(0.5f, 0f);
        rowRt.anchoredPosition = new Vector2(0f, 80f);
        var layout = row.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 6f;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth  = false;

        for (int i = 0; i < MAX_SLOTS; i++)
            _slots.Add(CreateSlot(row.transform));

        RefreshSlots();
    }

    EffectSlot CreateSlot(Transform parent)
    {
        var go = new GameObject($"EffectSlot");
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(44f, 52f);

        var bg = go.AddComponent<Image>();
        bg.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);

        // Effect name
        var nameGo  = new GameObject("Name");
        nameGo.transform.SetParent(go.transform, false);
        var nameTmp = nameGo.AddComponent<TextMeshProUGUI>();
        nameTmp.fontSize  = 8f;
        nameTmp.color     = Color.white;
        nameTmp.alignment = TextAlignmentOptions.Center;
        var nameRt = nameGo.GetComponent<RectTransform>();
        nameRt.anchorMin = new Vector2(0f, 0.5f);
        nameRt.anchorMax = new Vector2(1f, 1f);
        nameRt.offsetMin = nameRt.offsetMax = Vector2.zero;

        // Timer
        var timerGo  = new GameObject("Timer");
        timerGo.transform.SetParent(go.transform, false);
        var timerTmp = timerGo.AddComponent<TextMeshProUGUI>();
        timerTmp.fontSize  = 10f;
        timerTmp.color     = new Color(0.9f, 0.9f, 0.9f);
        timerTmp.alignment = TextAlignmentOptions.Center;
        var timerRt = timerGo.GetComponent<RectTransform>();
        timerRt.anchorMin = new Vector2(0f, 0f);
        timerRt.anchorMax = new Vector2(1f, 0.5f);
        timerRt.offsetMin = timerRt.offsetMax = Vector2.zero;

        go.SetActive(false);
        return new EffectSlot { go = go, bg = bg, nameLabel = nameTmp, timerLabel = timerTmp };
    }

    // ─── Update timers and prune expired effects ───────────────────────────────
    void Update()
    {
        bool dirty = false;
        var toRemove = new List<string>();
        foreach (var kvp in _active)
            if (Time.time >= kvp.Value)
                toRemove.Add(kvp.Key);
        foreach (var key in toRemove) { _active.Remove(key); dirty = true; }
        if (dirty) RefreshSlots();

        // Update countdown timers live
        int idx = 0;
        foreach (var kvp in _active)
        {
            if (idx >= MAX_SLOTS) break;
            float remaining = Mathf.Max(0f, kvp.Value - Time.time);
            _slots[idx].timerLabel.text = $"{remaining:F1}s";
            idx++;
        }
    }

    void RefreshSlots()
    {
        int idx = 0;
        foreach (var kvp in _active)
        {
            if (idx >= MAX_SLOTS) break;
            _slots[idx].go.SetActive(true);
            _slots[idx].nameLabel.text = kvp.Key;
            Color c = EffectColors.TryGetValue(kvp.Key, out var col) ? col : Color.white;
            _slots[idx].bg.color = new Color(c.r * 0.3f, c.g * 0.3f, c.b * 0.3f, 0.85f);
            _slots[idx].nameLabel.color = c;
            idx++;
        }
        for (int i = idx; i < MAX_SLOTS; i++)
            _slots[i].go.SetActive(false);
    }
}
#endif
