#if !UNITY_SERVER
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// WaveHUD — top-center arena overlay showing current wave, enemy count, and prep countdown.
///
/// Self-bootstrapping. Finds WaveSpawner by polling (scenes may load it later).
/// Reads WaveSpawner SyncVars: currentWave, enemiesAlive, waveActive.
///
/// Layout: top-center, 260×70px panel.
///   [  Wave 3  ]
///   [  12 enemies remaining  ]
///   or [  Prep: 4s  ] between waves.
///
/// Hidden on non-arena scenes (no WaveSpawner found → stays invisible).
/// </summary>
public class WaveHUD : MonoBehaviour
{
    // ── Colours ───────────────────────────────────────────────────────────────
    static readonly Color ColBg       = new Color(0.05f, 0.05f, 0.08f, 0.82f);
    static readonly Color ColWave     = new Color(1.00f, 0.85f, 0.25f);
    static readonly Color ColEnemies  = new Color(0.90f, 0.40f, 0.20f);
    static readonly Color ColPrep     = new Color(0.40f, 0.80f, 1.00f);
    static readonly Color ColClear    = new Color(0.25f, 0.95f, 0.40f);

    // ── UI refs ───────────────────────────────────────────────────────────────
    Text  _waveLabel;
    Text  _statusLabel;
    Image _bg;

    // ── Runtime ───────────────────────────────────────────────────────────────
    WaveSpawner _spawner;
    int         _lastWave    = -1;
    int         _lastAlive   = -1;
    bool        _lastActive  = false;
    bool        _flashing    = false;
    float       _searchTimer = 0f;

    // ── Bootstrap ─────────────────────────────────────────────────────────────
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        var go = new GameObject("WaveHUD", typeof(RectTransform));
        DontDestroyOnLoad(go);
        go.AddComponent<WaveHUD>();
    }

    // ── Lifecycle ─────────────────────────────────────────────────────────────
    void Awake()
    {
        BuildUI();
        SetVisible(false);
    }

    void Update()
    {
        // Search for WaveSpawner at most every 1.5s to avoid per-frame overhead
        if (_spawner == null)
        {
            _searchTimer -= Time.deltaTime;
            if (_searchTimer > 0f) return;
            _searchTimer = 1.5f;
            _spawner = FindFirstObjectByType<WaveSpawner>();
            if (_spawner == null) { SetVisible(false); return; }
            SetVisible(true);
        }

        // Poll SyncVars — cheap, one object, once per frame
        int  wave   = _spawner.currentWave;
        int  alive  = _spawner.enemiesAlive;
        bool active = _spawner.waveActive;

        // Only update labels when state changes
        if (wave == _lastWave && alive == _lastAlive && active == _lastActive) return;
        _lastWave   = wave;
        _lastAlive  = alive;
        _lastActive = active;

        _waveLabel.text = wave > 0 ? $"Wave {wave}" : "Incoming…";

        if (!active && wave > 0)
        {
            // Between waves
            _statusLabel.text  = "Wave Complete!";
            _statusLabel.color = ColClear;
            if (!_flashing) StartCoroutine(FlashClear());
        }
        else if (active)
        {
            int e = Mathf.Max(0, alive);
            _statusLabel.text  = e == 1 ? "1 enemy remaining" : $"{e} enemies remaining";
            _statusLabel.color = ColEnemies;
        }
        else
        {
            _statusLabel.text  = "Get ready…";
            _statusLabel.color = ColPrep;
        }
    }

    // ── Wave-clear flash ──────────────────────────────────────────────────────
    IEnumerator FlashClear()
    {
        _flashing = true;
        for (int i = 0; i < 3; i++)
        {
            _bg.color = new Color(0.15f, 0.50f, 0.15f, 0.88f);
            yield return new WaitForSecondsRealtime(0.18f);
            _bg.color = ColBg;
            yield return new WaitForSecondsRealtime(0.18f);
        }
        _flashing = false;
    }

    // ── UI construction ───────────────────────────────────────────────────────
    void BuildUI()
    {
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 85;
        gameObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

        // Panel: top-center
        var panelGO = new GameObject("WavePanel", typeof(RectTransform));
        panelGO.transform.SetParent(transform, false);
        var panelRT = panelGO.GetComponent<RectTransform>();
        panelRT.anchorMin        = new Vector2(0.5f, 1f);
        panelRT.anchorMax        = new Vector2(0.5f, 1f);
        panelRT.pivot            = new Vector2(0.5f, 1f);
        panelRT.sizeDelta        = new Vector2(260f, 62f);
        panelRT.anchoredPosition = new Vector2(0f, -14f);
        _bg = panelGO.AddComponent<Image>();
        _bg.color = ColBg;

        // Wave label (large, top)
        var waveGO = new GameObject("WaveLabel", typeof(RectTransform));
        waveGO.transform.SetParent(panelGO.transform, false);
        var waveRT = waveGO.GetComponent<RectTransform>();
        waveRT.anchorMin = new Vector2(0f, 0.5f);
        waveRT.anchorMax = Vector2.one;
        waveRT.offsetMin = new Vector2(8f, 0f);
        waveRT.offsetMax = new Vector2(-8f, -4f);
        _waveLabel = waveGO.AddComponent<Text>();
        _waveLabel.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        _waveLabel.fontSize  = 22;
        _waveLabel.fontStyle = FontStyle.Bold;
        _waveLabel.color     = ColWave;
        _waveLabel.alignment = TextAnchor.MiddleCenter;

        // Status label (smaller, bottom)
        var statusGO = new GameObject("StatusLabel", typeof(RectTransform));
        statusGO.transform.SetParent(panelGO.transform, false);
        var statusRT = statusGO.GetComponent<RectTransform>();
        statusRT.anchorMin = Vector2.zero;
        statusRT.anchorMax = new Vector2(1f, 0.5f);
        statusRT.offsetMin = new Vector2(8f, 4f);
        statusRT.offsetMax = new Vector2(-8f, 0f);
        _statusLabel = statusGO.AddComponent<Text>();
        _statusLabel.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        _statusLabel.fontSize  = 14;
        _statusLabel.color     = ColEnemies;
        _statusLabel.alignment = TextAnchor.MiddleCenter;
    }

    void SetVisible(bool v)
    {
        foreach (Transform t in transform) t.gameObject.SetActive(v);
    }
}
#endif
