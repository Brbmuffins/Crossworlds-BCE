using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using TMPro;

/// <summary>
/// ClericRadarUI — Shows a compact portrait row for allies below 50% HP.
/// Only visible when the local player is a Cleric (classIndex == 3).
///
/// Copy to: Assets/Game/UI/ClericRadarUI.cs
/// Self-bootstrapping — auto-creates its own Canvas. No scene setup needed.
///
/// Scans nearby PlayerIdentity/Health components every 0.5s to avoid per-frame cost.
/// </summary>
#if !UNITY_SERVER
public class ClericRadarUI : MonoBehaviour
{
    public static ClericRadarUI Instance { get; private set; }

    private const int CLERIC_CLASS = 3;
    private const float SCAN_INTERVAL = 0.5f;
    private const float LOW_HP_THRESHOLD = 0.5f;

    private Canvas    _canvas;
    private Transform _portraitRoot;

    private readonly List<PortraitSlot> _slots = new List<PortraitSlot>();
    private bool _isVisible;

    struct PortraitSlot
    {
        public GameObject go;
        public Image      fill;
        public TextMeshProUGUI label;
    }

    // ─── Bootstrap ────────────────────────────────────────────────────────────
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        if (Instance != null) return;
        var go = new GameObject("[ClericRadarUI]");
        DontDestroyOnLoad(go);
        go.AddComponent<ClericRadarUI>();
    }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        BuildCanvas();
    }

    void OnDestroy() { if (Instance == this) Instance = null; }

    IEnumerator Start()
    {
        // Wait until local player is available
        while (NetworkClient.localPlayer == null)
            yield return new WaitForSeconds(0.5f);

        var pid = NetworkClient.localPlayer.GetComponent<PlayerIdentity>();
        if (pid == null || pid.classIndex != CLERIC_CLASS)
        {
            // Not a Cleric — stay hidden forever
            _canvas.gameObject.SetActive(false);
            yield break;
        }

        _canvas.gameObject.SetActive(true);
        StartCoroutine(ScanLoop());
    }

    // ─── Build Canvas ─────────────────────────────────────────────────────────
    void BuildCanvas()
    {
        var canvasGo = new GameObject("ClericRadarCanvas");
        canvasGo.transform.SetParent(transform);
        _canvas = canvasGo.AddComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = 5;
        canvasGo.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasGo.AddComponent<GraphicRaycaster>();

        // Anchor panel to top-left
        var panel = new GameObject("RadarPanel");
        panel.transform.SetParent(canvasGo.transform, false);
        var rt = panel.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot     = new Vector2(0f, 1f);
        rt.anchoredPosition = new Vector2(10f, -10f);

        var layout = panel.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 4f;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth  = false;

        _portraitRoot = panel.transform;
        canvasGo.SetActive(false);
    }

    // ─── Scan Loop ────────────────────────────────────────────────────────────
    IEnumerator ScanLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(SCAN_INTERVAL);

            var allPlayers = FindObjectsByType<PlayerIdentity>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            var lowHp = new List<(string name, float frac)>();

            foreach (var pid in allPlayers)
            {
                var h = pid.GetComponent<Health>();
                if (h == null || h.isDead) continue;
                if (h.HpPercent < LOW_HP_THRESHOLD)
                    lowHp.Add((pid.playerName, h.HpPercent));
            }

            RefreshPortraits(lowHp);
        }
    }

    void RefreshPortraits(List<(string name, float frac)> allies)
    {
        // Ensure we have enough slots
        while (_slots.Count < allies.Count)
            _slots.Add(CreateSlot());

        for (int i = 0; i < _slots.Count; i++)
        {
            if (i < allies.Count)
            {
                var (name, frac) = allies[i];
                _slots[i].go.SetActive(true);
                _slots[i].fill.fillAmount = frac;
                _slots[i].label.text = name;

                // Pulse red if critical (< 25%)
                _slots[i].fill.color = frac < 0.25f
                    ? Color.Lerp(Color.red, Color.yellow, Time.time % 1f)
                    : new Color(0.2f, 0.8f, 0.2f);
            }
            else
            {
                _slots[i].go.SetActive(false);
            }
        }
    }

    PortraitSlot CreateSlot()
    {
        var go = new GameObject("Portrait");
        go.transform.SetParent(_portraitRoot, false);

        var rt = go.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(120f, 28f);

        var bg = go.AddComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 0.6f);

        // HP fill bar
        var fillGo = new GameObject("Fill");
        fillGo.transform.SetParent(go.transform, false);
        var fillImg = fillGo.AddComponent<Image>();
        fillImg.color = new Color(0.2f, 0.8f, 0.2f);
        fillImg.type  = Image.Type.Filled;
        fillImg.fillMethod = Image.FillMethod.Horizontal;
        var fillRt = fillGo.GetComponent<RectTransform>();
        fillRt.anchorMin = Vector2.zero; fillRt.anchorMax = Vector2.one;
        fillRt.offsetMin = fillRt.offsetMax = Vector2.zero;

        // Name label
        var labelGo = new GameObject("Label");
        labelGo.transform.SetParent(go.transform, false);
        var tmp = labelGo.AddComponent<TextMeshProUGUI>();
        tmp.fontSize  = 11f;
        tmp.color     = Color.white;
        tmp.alignment = TextAlignmentOptions.Left;
        var labelRt = labelGo.GetComponent<RectTransform>();
        labelRt.anchorMin = Vector2.zero; labelRt.anchorMax = Vector2.one;
        labelRt.offsetMin = new Vector2(4f, 0f); labelRt.offsetMax = Vector2.zero;

        return new PortraitSlot { go = go, fill = fillImg, label = tmp };
    }
}
#endif
