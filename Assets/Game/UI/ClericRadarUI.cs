#if UNITY_EDITOR || !UNITY_SERVER
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;

/// <summary>
/// ClericRadarUI — self-bootstrapping. Only active when ClassIndex == 3 (Cleric).
/// Scans all networked Health components every 0.5s.
///   ≤40% HP: ally nameplate outline pulses red (sine 0.4→1.0, 0.8s cycle)
///   ≤20% HP: nameplate outline solid red + scale 1.15× + one-shot ping sound
///
/// Also maintains a small 80×80 portrait widget (top-left) showing the lowest HP ally.
/// </summary>
public class ClericRadarUI : MonoBehaviour
{
    // ── Bootstrap ─────────────────────────────────────────────────────────────
    public static ClericRadarUI Instance { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        if (Instance != null) return;
        var go = new GameObject("[ClericRadarUI]");
        DontDestroyOnLoad(go);
        Instance = go.AddComponent<ClericRadarUI>();
    }

    // ── Config ────────────────────────────────────────────────────────────────
    const float ScanInterval   = 0.5f;
    const float LowHpThreshold  = 0.40f;   // pulse
    const float CritHpThreshold = 0.20f;   // solid red + scale + ping
    const float PulseSpeed       = Mathf.PI * 2f / 0.8f; // full cycle / 0.8s

    [Tooltip("Optional one-shot ping clip played when an ally drops below 20%.")]
    public AudioClip pingClip;

    // ── UI refs ───────────────────────────────────────────────────────────────
    private Canvas          _canvas;
    private GameObject      _portraitRoot;
    private Image           _portraitCircle;
    private TextMeshProUGUI _portraitName;
    private TextMeshProUGUI _portraitHpPct;

    // Tracked outline states: key = nameplate Image component on ally
    private class NameplateState
    {
        public Image    outline;
        public bool     pinged; // one-shot ping guard
        public float    pulseT;
    }
    private readonly Dictionary<Health, NameplateState> _tracked = new();

    // ── State ─────────────────────────────────────────────────────────────────
    private bool  _isCleric   = false;
    private float _scanTimer  = 0f;

    // ── Lifecycle ─────────────────────────────────────────────────────────────
    void Awake()     => BuildUI();
    void OnEnable()  => StartCoroutine(ScanLoop());

    IEnumerator ScanLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(ScanInterval);
            CheckClericClass();
            if (_isCleric)
                ScanAllies();
        }
    }

    void CheckClericClass()
    {
        var pm = PlayerProgressManager.Local;
        _isCleric = pm != null && pm.ClassIndex == 3;
        _portraitRoot.SetActive(_isCleric);
        if (!_isCleric) ClearAllOutlines();
    }

    // ── Scan ──────────────────────────────────────────────────────────────────
    void ScanAllies()
    {
        // Throttled FindObjectsByType — only once per scan interval
        var allHealth = FindObjectsByType<Health>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        Health lowestAlly = null;
        float  lowestFrac = 1f;

        // Remove stale entries
        var toRemove = new List<Health>();
        foreach (var kv in _tracked)
            if (kv.Key == null) toRemove.Add(kv.Key);
        foreach (var h in toRemove) _tracked.Remove(h);

        foreach (var h in allHealth)
        {
            // Only allied players (not self)
            if (!h.isPlayer) continue;
            var ni = h.GetComponent<NetworkIdentity>();
            if (ni == null || ni.isLocalPlayer) continue;
            if (!h.IsAlive) continue;

            float frac = h.Fraction;

            // Track lowest
            if (frac < lowestFrac) { lowestFrac = frac; lowestAlly = h; }

            if (frac <= LowHpThreshold)
            {
                EnsureTracked(h);
                // Pulse / solid handled in Update
            }
            else
            {
                // Ally recovered — clear outline
                if (_tracked.TryGetValue(h, out var st))
                {
                    ResetOutline(st);
                    st.pinged = false;
                }
            }

            // One-shot ping at ≤20%
            if (frac <= CritHpThreshold)
            {
                if (_tracked.TryGetValue(h, out var st) && !st.pinged)
                {
                    st.pinged = true;
                    PlayPing(h.transform.position);
                }
            }
        }

        // Update portrait
        UpdatePortrait(lowestAlly, lowestFrac);
    }

    void EnsureTracked(Health h)
    {
        if (_tracked.ContainsKey(h)) return;

        // Find PlayerNameplate outline image — look for a child named "Outline" or first Image
        var nameplate = h.GetComponentInChildren<PlayerNameplate>();
        Image outline = null;
        if (nameplate != null)
        {
            var outlineGO = nameplate.transform.Find("Outline");
            if (outlineGO != null) outline = outlineGO.GetComponent<Image>();
            if (outline == null) outline = nameplate.GetComponentInChildren<Image>();
        }

        _tracked[h] = new NameplateState { outline = outline, pinged = false };
    }

    void ResetOutline(NameplateState st)
    {
        if (st.outline == null) return;
        st.outline.color    = new Color(1f, 1f, 1f, 0f);
        st.outline.transform.localScale = Vector3.one;
    }

    void ClearAllOutlines()
    {
        foreach (var kv in _tracked)
            if (kv.Value != null) ResetOutline(kv.Value);
        _tracked.Clear();
    }

    // ── Update — drive outline animations ────────────────────────────────────
    void Update()
    {
        if (!_isCleric) return;

        foreach (var kv in _tracked)
        {
            var h  = kv.Key;
            var st = kv.Value;
            if (h == null || st.outline == null) continue;

            float frac = h.Fraction;

            if (frac <= CritHpThreshold)
            {
                // Solid red + scale 1.15
                st.outline.color            = new Color(1f, 0.15f, 0.15f, 1f);
                st.outline.transform.localScale = Vector3.one * 1.15f;
            }
            else if (frac <= LowHpThreshold)
            {
                // Pulse
                st.pulseT += Time.deltaTime * PulseSpeed;
                float alpha = Mathf.Lerp(0.4f, 1.0f, (Mathf.Sin(st.pulseT) + 1f) * 0.5f);
                st.outline.color = new Color(1f, 0.15f, 0.15f, alpha);
                st.outline.transform.localScale = Vector3.one;
            }
        }
    }

    // ── Portrait widget ───────────────────────────────────────────────────────
    void UpdatePortrait(Health ally, float frac)
    {
        if (ally == null)
        {
            _portraitRoot.SetActive(false);
            return;
        }
        _portraitRoot.SetActive(true);

        // Hero color by class index
        var pi = ally.GetComponent<PlayerIdentity>();
        int classIdx = pi != null ? pi.classIndex : 0;
        _portraitCircle.color = ClassColor(classIdx);

        string name = pi != null ? pi.playerName : "Ally";
        _portraitName.text  = name;
        _portraitHpPct.text = $"{Mathf.RoundToInt(frac * 100f)}% HP";
        _portraitHpPct.color = frac <= CritHpThreshold
            ? new Color(1f, 0.3f, 0.3f)
            : frac <= LowHpThreshold
                ? new Color(1f, 0.7f, 0.1f)
                : new Color(0.4f, 1f, 0.4f);
    }

    Color ClassColor(int idx) => idx switch
    {
        0 => new Color(0.4f, 0.7f, 0.4f),  // Marauder — green
        1 => new Color(0.6f, 0.6f, 0.7f),  // Ironclad — silver
        2 => new Color(0.5f, 0.1f, 0.7f),  // Shadowblade — purple
        3 => new Color(0.9f, 0.85f, 0.3f), // Cleric   — gold
        4 => new Color(0.3f, 0.5f, 1.0f),  // Arcanist — blue
        _ => Color.white
    };

    // ── Ping ─────────────────────────────────────────────────────────────────
    void PlayPing(Vector3 pos)
    {
        if (pingClip != null)
            AudioSource.PlayClipAtPoint(pingClip, pos, 0.6f);
    }

    // ── Build UI ──────────────────────────────────────────────────────────────
    void BuildUI()
    {
        _canvas = gameObject.AddComponent<Canvas>();
        _canvas.renderMode  = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = 92;
        gameObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        gameObject.AddComponent<GraphicRaycaster>();

        // Portrait widget — 80×80, anchored top-left
        _portraitRoot = new GameObject("ClericPortrait"); _portraitRoot.transform.SetParent(transform, false);
        var prRect = _portraitRoot.AddComponent<RectTransform>();
        prRect.anchorMin = new Vector2(0f, 1f); prRect.anchorMax = new Vector2(0f, 1f);
        prRect.pivot     = new Vector2(0f, 1f);
        prRect.anchoredPosition = new Vector2(12f, -90f); // below Cleric's own HP bar
        prRect.sizeDelta = new Vector2(90f, 90f);

        // Background
        var bgGO = new GameObject("BG"); bgGO.transform.SetParent(_portraitRoot.transform, false);
        bgGO.AddComponent<RectTransform>(); // Unity 6: must exist before StretchFull/Image
        StretchFull(bgGO);
        bgGO.AddComponent<Image>().color = new Color(0.05f, 0.06f, 0.09f, 0.88f);

        // Circle
        var circleGO = new GameObject("Circle"); circleGO.transform.SetParent(_portraitRoot.transform, false);
        var cRect = circleGO.AddComponent<RectTransform>();
        cRect.anchorMin = new Vector2(0.1f, 0.35f); cRect.anchorMax = new Vector2(0.9f, 0.95f);
        cRect.offsetMin = cRect.offsetMax = Vector2.zero;
        _portraitCircle = circleGO.AddComponent<Image>();
        _portraitCircle.color = Color.white;

        // Name
        var nameGO = new GameObject("Name"); nameGO.transform.SetParent(_portraitRoot.transform, false);
        var nRect = nameGO.AddComponent<RectTransform>();
        nRect.anchorMin = new Vector2(0f, 0.18f); nRect.anchorMax = new Vector2(1f, 0.38f);
        nRect.offsetMin = new Vector2(2f, 0f); nRect.offsetMax = new Vector2(-2f, 0f);
        _portraitName = nameGO.AddComponent<TextMeshProUGUI>();
        _portraitName.fontSize = 10f; _portraitName.color = Color.white;
        _portraitName.alignment = TextAlignmentOptions.Center;
        _portraitName.fontStyle = FontStyles.Bold;

        // HP%
        var hpGO = new GameObject("HpPct"); hpGO.transform.SetParent(_portraitRoot.transform, false);
        var hpRect = hpGO.AddComponent<RectTransform>();
        hpRect.anchorMin = new Vector2(0f, 0f); hpRect.anchorMax = new Vector2(1f, 0.18f);
        hpRect.offsetMin = hpRect.offsetMax = Vector2.zero;
        _portraitHpPct = hpGO.AddComponent<TextMeshProUGUI>();
        _portraitHpPct.fontSize = 10f; _portraitHpPct.color = new Color(1f, 0.3f, 0.3f);
        _portraitHpPct.alignment = TextAlignmentOptions.Center;
        _portraitHpPct.fontStyle = FontStyles.Bold;

        _portraitRoot.SetActive(false);
    }

    void StretchFull(GameObject go)
    {
        var r = go.GetComponent<RectTransform>() ?? go.AddComponent<RectTransform>();
        r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one;
        r.offsetMin = r.offsetMax = Vector2.zero;
    }
}
#endif
