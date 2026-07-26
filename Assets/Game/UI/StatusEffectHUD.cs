#if UNITY_EDITOR || !UNITY_SERVER
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// StatusEffectHUD — self-bootstrapping icon row showing active status effects
/// on the LOCAL player. Mounted below the player HP bar.
/// Polls StatusEffectManager on local player every 0.2s (throttled).
/// Max 6 icons, sorted by remaining duration descending.
/// </summary>
public class StatusEffectHUD : MonoBehaviour
{
    // ── Bootstrap ─────────────────────────────────────────────────────────────
    public static StatusEffectHUD Instance { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        if (Instance != null) return;
        var go = new GameObject("[StatusEffectHUD]");
        DontDestroyOnLoad(go);
        Instance = go.AddComponent<StatusEffectHUD>();
    }

    // ── Config ────────────────────────────────────────────────────────────────
    const int    MaxIcons    = 6;
    const float  IconSize    = 32f;
    const float  IconSpacing = 6f;
    const float  PollRate    = 0.2f;

    // Debuff colors
    static readonly Dictionary<StatusEffectType, Color> EffectColors = new()
    {
        { StatusEffectType.Slow,     new Color(0.30f, 0.50f, 1.00f) },
        { StatusEffectType.Stagger,  new Color(1.00f, 0.55f, 0.10f) },
        { StatusEffectType.Silenced, new Color(0.55f, 0.55f, 0.55f) },
        { StatusEffectType.Cursed,   new Color(0.65f, 0.10f, 0.90f) },
        { StatusEffectType.Weakened, new Color(0.85f, 0.15f, 0.15f) },
        { StatusEffectType.Bound,    new Color(0.90f, 0.85f, 0.20f) },
    };

    static readonly Dictionary<StatusEffectType, string> EffectNames = new()
    {
        { StatusEffectType.Slow,     "Slow"     },
        { StatusEffectType.Stagger,  "Stagger"  },
        { StatusEffectType.Silenced, "Silenced" },
        { StatusEffectType.Cursed,   "Cursed"   },
        { StatusEffectType.Weakened, "Weakened" },
        { StatusEffectType.Bound,    "Bound"    },
    };

    // ── UI refs ───────────────────────────────────────────────────────────────
    private Canvas          _canvas;
    private Transform       _rowRoot;
    private IconSlot[]      _slots = new IconSlot[MaxIcons];

    // ── State ─────────────────────────────────────────────────────────────────
    private StatusEffectManager _localSEM;
    private float               _pollTimer = 0f;

    // ── Lifecycle ─────────────────────────────────────────────────────────────
    void Awake()   => BuildUI();
    void OnEnable() => StartCoroutine(PollLoop());

    IEnumerator PollLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(PollRate);
            TryBindLocalPlayer();
            Refresh();
        }
    }

    // ── Bind to local player ──────────────────────────────────────────────────
    void TryBindLocalPlayer()
    {
        if (_localSEM != null) return;

        bool networkActive = Mirror.NetworkClient.active || Mirror.NetworkServer.active;

        // Cache local player's StatusEffectManager — throttled search
        var identities = FindObjectsByType<Mirror.NetworkIdentity>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        foreach (var ni in identities)
        {
            bool isLocal = networkActive ? ni.isLocalPlayer : (ni.CompareTag("Player") || ni.GetComponent<PlayerMovement>() != null);
            if (!isLocal) continue;
            _localSEM = ni.GetComponent<StatusEffectManager>();
            break;
        }
    }

    // ── Refresh ───────────────────────────────────────────────────────────────
    void Refresh()
    {
        if (_localSEM == null)
        {
            HideAll();
            return;
        }

        // Get active effects via reflection-safe public API
        var active = _localSEM.GetAll();
        if (active == null) { HideAll(); return; }

        // Sort by remaining duration descending
        active.Sort((a, b) => b.remainingTime.CompareTo(a.remainingTime));

        for (int i = 0; i < MaxIcons; i++)
        {
            if (_slots[i].root == null) continue;
            if (i < active.Count)
                ShowSlot(i, active[i]);
            else
                _slots[i].root.SetActive(false);
        }
    }

    void HideAll()
    {
        for (int i = 0; i < MaxIcons; i++)
            if (_slots[i].root != null) _slots[i].root.SetActive(false);
    }

    void ShowSlot(int i, StatusEffect effect)
    {
        var slot = _slots[i];
        if (slot.root == null) return;
        slot.root.SetActive(true);

        Color c = EffectColors.TryGetValue(effect.type, out var col)
            ? col : new Color(0.5f, 0.5f, 0.5f);

        slot.bg.color    = new Color(c.r * 0.4f, c.g * 0.4f, c.b * 0.4f, 0.9f);
        slot.accent.color = c;
        slot.label.text  = EffectNames.TryGetValue(effect.type, out var nm) ? nm : effect.type.ToString();

        float frac = effect.duration > 0f
            ? Mathf.Clamp01(effect.remainingTime / effect.duration)
            : 1f;
        slot.durationBar.fillAmount = frac;
        slot.durationBar.color      = Color.Lerp(new Color(c.r, c.g, c.b, 0.5f), c, frac);
    }

    // ── Build UI ──────────────────────────────────────────────────────────────
    void BuildUI()
    {
        _canvas = gameObject.AddComponent<Canvas>();
        _canvas.renderMode  = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = 95; // below HP bar (100), above world
        gameObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        gameObject.AddComponent<GraphicRaycaster>();

        // Row root — centered bottom, just below HP bar
        var rowGO = new GameObject("StatusRow"); rowGO.transform.SetParent(transform, false);
        var rowRect = rowGO.AddComponent<RectTransform>();
        rowRect.anchorMin = new Vector2(0.5f, 0f);
        rowRect.anchorMax = new Vector2(0.5f, 0f);
        rowRect.pivot     = new Vector2(0.5f, 0f);
        rowRect.anchoredPosition = new Vector2(0f, 72f); // 72px above bottom = below HP bar
        rowRect.sizeDelta = new Vector2(
            MaxIcons * (IconSize + IconSpacing), IconSize + 16f);

        var hlg = rowGO.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing           = IconSpacing;
        hlg.childAlignment    = TextAnchor.LowerLeft;
        hlg.childControlWidth = false; hlg.childControlHeight = false;
        hlg.childForceExpandWidth = false; hlg.childForceExpandHeight = false;

        _rowRoot = rowGO.transform;

        for (int i = 0; i < MaxIcons; i++)
            _slots[i] = CreateSlot(i);
    }

    IconSlot CreateSlot(int i)
    {
        var root = new GameObject($"Slot_{i}"); root.transform.SetParent(_rowRoot, false);
        var rootRect = root.AddComponent<RectTransform>();
        rootRect.sizeDelta = new Vector2(IconSize, IconSize + 14f);

        var slot = new IconSlot { root = root };

        // Background
        var bg = new GameObject("BG"); bg.transform.SetParent(root.transform, false);
        var bgRect = bg.AddComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0f, 0.2f);
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = bgRect.offsetMax = Vector2.zero;
        slot.bg = bg.AddComponent<Image>();
        slot.bg.color = new Color(0.2f, 0.2f, 0.2f, 0.85f);

        // Accent border
        var acc = new GameObject("Accent"); acc.transform.SetParent(root.transform, false);
        var accRect = acc.AddComponent<RectTransform>();
        accRect.anchorMin = new Vector2(0f, 0.2f);
        accRect.anchorMax = Vector2.one;
        accRect.offsetMin = new Vector2(-1f, -1f);
        accRect.offsetMax = new Vector2(1f, 1f);
        slot.accent = acc.AddComponent<Image>();
        slot.accent.color = Color.white;
        // Put behind bg
        acc.transform.SetSiblingIndex(0);

        // Name label
        var lbl = new GameObject("Label"); lbl.transform.SetParent(root.transform, false);
        var lblRect = lbl.AddComponent<RectTransform>();
        lblRect.anchorMin = new Vector2(0f, 0.2f);
        lblRect.anchorMax = Vector2.one;
        lblRect.offsetMin = lblRect.offsetMax = Vector2.zero;
        slot.label = lbl.AddComponent<TextMeshProUGUI>();
        slot.label.fontSize  = 8f;
        slot.label.color     = Color.white;
        slot.label.alignment = TextAlignmentOptions.Center;
        slot.label.fontStyle = FontStyles.Bold;
        slot.label.enableWordWrapping = true;

        // Duration bar (at very bottom)
        var barGO = new GameObject("DurBar"); barGO.transform.SetParent(root.transform, false);
        var barRect = barGO.AddComponent<RectTransform>();
        barRect.anchorMin = Vector2.zero;
        barRect.anchorMax = new Vector2(1f, 0.2f);
        barRect.offsetMin = barRect.offsetMax = Vector2.zero;
        slot.durationBar = barGO.AddComponent<Image>();
        slot.durationBar.type       = Image.Type.Filled;
        slot.durationBar.fillMethod = Image.FillMethod.Horizontal;
        slot.durationBar.fillAmount = 1f;
        slot.durationBar.color      = Color.white;

        root.SetActive(false);
        return slot;
    }

    struct IconSlot
    {
        public GameObject        root;
        public Image             bg;
        public Image             accent;
        public TextMeshProUGUI   label;
        public Image             durationBar;
    }
}
#endif
