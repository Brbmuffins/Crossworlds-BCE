#if !UNITY_SERVER
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ArenaClearUI — wave-complete banner + full "ARENA CLEAR" screen.
///
/// Self-bootstrapping. Finds WaveSpawner and hooks into its SyncVars each scene load.
///
/// Wave clear banner: brief "WAVE N CLEAR" toast when waveActive goes false → true.
/// Arena clear: displayed when WaveSpawner is done (all waves) — full-screen overlay
///   showing XP and gold earned this run via PlayerProgressManager delta.
///
/// Keyboard: any key / click dismisses the arena-clear screen.
/// </summary>
public class ArenaClearUI : MonoBehaviour
{
    /// <summary>Fired once when the arena clear screen appears. CombatSessionTracker listens here.</summary>
    public static event System.Action OnRunComplete;

    // ── Colours ───────────────────────────────────────────────────────────────
    static readonly Color ColBanner   = new Color(0.06f, 0.06f, 0.10f, 0.88f);
    static readonly Color ColGold     = new Color(1.00f, 0.82f, 0.10f);
    static readonly Color ColGreen    = new Color(0.20f, 0.90f, 0.40f);
    static readonly Color ColOverlay  = new Color(0.02f, 0.04f, 0.08f, 0.92f);
    static readonly Color ColWhite    = new Color(1.00f, 1.00f, 1.00f, 0.95f);
    static readonly Color ColDim      = new Color(0.75f, 0.75f, 0.80f, 0.80f);

    // ── Wave banner ───────────────────────────────────────────────────────────
    Image   _bannerBg;
    Text    _bannerText;
    bool    _bannerRunning;

    // ── Arena clear screen ────────────────────────────────────────────────────
    GameObject  _clearScreen;
    Text        _clearTitle;
    Text        _clearWaves;
    Text        _clearXp;
    Text        _clearGold;
    Text        _clearDismiss;
    bool        _clearShowing;

    // ── Runtime ───────────────────────────────────────────────────────────────
    WaveSpawner _spawner;
    int         _lastWave    = 0;
    bool        _lastActive  = false;
    float       _searchTimer = 0f;

    // Track progress delta for the run
    int _runStartLevel;
    int _runStartGold;

    // ── Bootstrap ─────────────────────────────────────────────────────────────
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        var go = new GameObject("ArenaClearUI", typeof(RectTransform));
        DontDestroyOnLoad(go);
        go.AddComponent<ArenaClearUI>();
    }

    // ── Lifecycle ─────────────────────────────────────────────────────────────
    void Awake()    { BuildUI(); HideBanner(); _clearScreen.SetActive(false); }

    void Update()
    {
        // Dismiss clear screen on any key/click
        if (_clearShowing)
        {
            if (Input.anyKeyDown) DismissClearScreen();
            return;
        }

        // Find WaveSpawner (throttled)
        if (_spawner == null)
        {
            _searchTimer -= Time.deltaTime;
            if (_searchTimer > 0f) return;
            _searchTimer = 2f;
            _spawner = FindFirstObjectByType<WaveSpawner>();
            if (_spawner == null) return;
            // Snapshot start-of-run progress
            _runStartLevel = PlayerProgressManager.Local?.Level ?? 0;
            _runStartGold  = PlayerProgressManager.Local?.Gold  ?? 0;
        }

        int  wave   = _spawner.currentWave;
        bool active = _spawner.waveActive;

        // Wave just cleared (transitioned from active → not active, and wave increased)
        if (_lastActive && !active && wave > 0)
        {
            if (!_bannerRunning)
                StartCoroutine(ShowWaveBanner($"Wave {wave} Clear!"));
        }

        // Detect arena complete: spawner has stopped and last wave was the final one
        // WaveSpawner doesn't have a "done" flag, so we check: waveActive was true, now false
        // AND spawner._running is false (no public accessor — we infer from wave not incrementing)
        // Simplest safe heuristic: if wave just decreased (looped to 0), arena restarted.
        // For final wave detection: wave > 0, !active, and wave didn't change this frame.
        // A better solution: WaveSpawner exposes bool IsComplete via SyncVar (add in future).
        // For now: show arena clear if wave >= baseEnemiesPerWave's implied max and not active.

        _lastWave   = wave;
        _lastActive = active;
    }

    // ── Called externally by WaveSpawner when all waves finish ────────────────
    /// <summary>
    /// Call this from WaveSpawner.OnAllWavesComplete (server → ClientRpc) when
    /// all waves are finished. Displays the Arena Clear screen.
    /// </summary>
    public static void ShowArenaClear()
    {
        var instance = FindFirstObjectByType<ArenaClearUI>();
        instance?.ShowClearScreen();
    }

    // ── Wave clear banner ─────────────────────────────────────────────────────
    IEnumerator ShowWaveBanner(string text)
    {
        _bannerRunning = true;
        _bannerText.text = text;

        // Fade in
        yield return Fade(_bannerBg, _bannerText, 0f, 1f, 0.25f);
        yield return new WaitForSecondsRealtime(1.2f);
        // Fade out
        yield return Fade(_bannerBg, _bannerText, 1f, 0f, 0.35f);

        HideBanner();
        _bannerRunning = false;
    }

    IEnumerator Fade(Image bg, Text txt, float from, float to, float dur)
    {
        Color bgBase  = ColBanner; bgBase.a  = 0f;
        Color txtBase = ColGreen;  txtBase.a = 0f;

        float t = 0f;
        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            float a = Mathf.Lerp(from, to, t / dur);
            bgBase.a  = ColBanner.a * a;
            txtBase.a = a;
            bg.color  = bgBase;
            txt.color = txtBase;
            yield return null;
        }
    }

    void HideBanner()
    {
        var c = _bannerBg.color; c.a = 0f; _bannerBg.color = c;
        var tc = _bannerText.color; tc.a = 0f; _bannerText.color = tc;
    }

    // ── Arena clear screen ────────────────────────────────────────────────────
    void ShowClearScreen()
    {
        var pm = PlayerProgressManager.Local;
        int xpGained   = pm != null ? Mathf.Max(0, pm.Level - _runStartLevel) * 100 : 0; // rough
        int goldGained  = pm != null ? Mathf.Max(0, pm.Gold - _runStartGold)       : 0;

        _clearTitle.text   = "ARENA CLEAR";
        _clearWaves.text   = _spawner != null ? $"Wave {_spawner.currentWave} Completed" : "";
        _clearXp.text      = $"XP Earned:    +{xpGained}";
        _clearGold.text    = $"Gold Earned:  +{goldGained}";
        _clearDismiss.text = "Press any key to continue";

        _clearScreen.SetActive(true);
        _clearShowing = true;
        OnRunComplete?.Invoke();

        // Re-snapshot for next run
        _runStartLevel = pm?.Level ?? 0;
        _runStartGold  = pm?.Gold  ?? 0;
    }

    void DismissClearScreen()
    {
        _clearScreen.SetActive(false);
        _clearShowing = false;
    }

    // ── UI construction ───────────────────────────────────────────────────────
    void BuildUI()
    {
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 95;
        gameObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

        BuildBanner();
        BuildClearScreen();
    }

    void BuildBanner()
    {
        // Slim horizontal banner, vertically centred at 70% of screen
        var bgGO = new GameObject("WaveBanner", typeof(RectTransform));
        bgGO.transform.SetParent(transform, false);
        var bgRT = bgGO.GetComponent<RectTransform>();
        bgRT.anchorMin        = new Vector2(0.15f, 0.65f);
        bgRT.anchorMax        = new Vector2(0.85f, 0.72f);
        bgRT.offsetMin        = Vector2.zero;
        bgRT.offsetMax        = Vector2.zero;
        _bannerBg = bgGO.AddComponent<Image>();
        _bannerBg.color = new Color(ColBanner.r, ColBanner.g, ColBanner.b, 0f);

        var txtGO = new GameObject("BannerText", typeof(RectTransform));
        txtGO.transform.SetParent(bgGO.transform, false);
        var txtRT = txtGO.GetComponent<RectTransform>();
        txtRT.anchorMin = Vector2.zero; txtRT.anchorMax = Vector2.one;
        txtRT.offsetMin = Vector2.zero; txtRT.offsetMax = Vector2.zero;
        _bannerText = txtGO.AddComponent<Text>();
        _bannerText.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        _bannerText.fontSize  = 28;
        _bannerText.fontStyle = FontStyle.Bold;
        _bannerText.alignment = TextAnchor.MiddleCenter;
        _bannerText.color     = new Color(ColGreen.r, ColGreen.g, ColGreen.b, 0f);
    }

    void BuildClearScreen()
    {
        _clearScreen = new GameObject("ClearScreen", typeof(RectTransform));
        _clearScreen.transform.SetParent(transform, false);
        var csRT = _clearScreen.GetComponent<RectTransform>();
        csRT.anchorMin = Vector2.zero; csRT.anchorMax = Vector2.one;
        csRT.offsetMin = Vector2.zero; csRT.offsetMax = Vector2.zero;

        // Full-screen dim overlay
        var overlay = _clearScreen.AddComponent<Image>();
        overlay.color = ColOverlay;

        // Content panel
        var panelGO = new GameObject("Panel", typeof(RectTransform));
        panelGO.transform.SetParent(_clearScreen.transform, false);
        var panelRT = panelGO.GetComponent<RectTransform>();
        panelRT.anchorMin        = new Vector2(0.3f, 0.3f);
        panelRT.anchorMax        = new Vector2(0.7f, 0.72f);
        panelRT.offsetMin        = Vector2.zero;
        panelRT.offsetMax        = Vector2.zero;

        _clearTitle   = AddLabel(panelGO.transform, "Title",   new Vector2(0f, 0.78f), new Vector2(1f, 1f),   34, FontStyle.Bold, ColGold,  TextAnchor.UpperCenter);
        _clearWaves   = AddLabel(panelGO.transform, "Waves",   new Vector2(0f, 0.58f), new Vector2(1f, 0.78f), 18, FontStyle.Normal, ColWhite, TextAnchor.MiddleCenter);
        _clearXp      = AddLabel(panelGO.transform, "XP",      new Vector2(0f, 0.40f), new Vector2(1f, 0.58f), 18, FontStyle.Normal, ColGreen, TextAnchor.MiddleLeft);
        _clearGold    = AddLabel(panelGO.transform, "Gold",    new Vector2(0f, 0.24f), new Vector2(1f, 0.40f), 18, FontStyle.Normal, ColGold,  TextAnchor.MiddleLeft);
        _clearDismiss = AddLabel(panelGO.transform, "Dismiss", new Vector2(0f, 0f),    new Vector2(1f, 0.18f), 13, FontStyle.Normal, ColDim,   TextAnchor.LowerCenter);
    }

    static Text AddLabel(Transform parent, string name,
        Vector2 anchorMin, Vector2 anchorMax,
        int fontSize, FontStyle style, Color color, TextAnchor align)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
        rt.offsetMin = new Vector2(8f, 0f); rt.offsetMax = new Vector2(-8f, 0f);
        var t = go.AddComponent<Text>();
        t.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.fontSize  = fontSize;
        t.fontStyle = style;
        t.color     = color;
        t.alignment = align;
        return t;
    }
}
#endif
