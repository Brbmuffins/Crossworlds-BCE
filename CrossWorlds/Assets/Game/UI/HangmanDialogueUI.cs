using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// HangmanDialogueUI — Dialogue panel for The Hangman NPC.
/// Self-bootstrapping — auto-creates its own Canvas. No prefab or scene setup needed.
///
/// Copy to: Assets/Game/UI/HangmanDialogueUI.cs
///
/// Usage (called by HangmanNPC):
///   HangmanDialogueUI.Show(npc);   // open panel, fade in
///   HangmanDialogueUI.Hide();      // close panel, fade out
///
/// Rotating flavor text lines cycle on each open.
/// ESC also closes the panel.
/// </summary>
#if !UNITY_SERVER
public class HangmanDialogueUI : MonoBehaviour
{
    public static HangmanDialogueUI Instance { get; private set; }

    // Rotating flavor lines — cycles on each open
    static readonly string[] FlavorLines = new string[]
    {
        "The arena awaits. Only the strong survive.",
        "Step into the void... or step aside.",
        "I have seen a hundred challengers. I have buried them all.",
    };

    private HangmanNPC  _activeNPC;
    private int         _flavorIndex;

    // ─── UI refs ──────────────────────────────────────────────────────────────
    private GameObject             _panelRoot;
    private CanvasGroup            _canvasGroup;
    private TextMeshProUGUI        _flavorText;
    private Button                 _enterBtn;
    private Button                 _leaveBtn;

    // ─── Bootstrap ────────────────────────────────────────────────────────────
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        if (Instance != null) return;
        var go = new GameObject("[HangmanDialogueUI]");
        DontDestroyOnLoad(go);
        go.AddComponent<HangmanDialogueUI>();
    }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        BuildPanel();
    }

    void OnDestroy() { if (Instance == this) Instance = null; }

    // ─── Public API ───────────────────────────────────────────────────────────
    public static void Show(HangmanNPC npc)
    {
        if (Instance == null) return;
        Instance._activeNPC = npc;
        Instance.OpenPanel();
    }

    public static void Hide()
    {
        if (Instance == null) return;
        Instance.StartCoroutine(Instance.FadeOut());
    }

    // ─── Build Panel ──────────────────────────────────────────────────────────
    void BuildPanel()
    {
        // Canvas
        var canvasGo = new GameObject("HangmanCanvas");
        canvasGo.transform.SetParent(transform);
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode  = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 20;
        canvasGo.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasGo.AddComponent<GraphicRaycaster>();

        // Panel background
        _panelRoot = new GameObject("Panel");
        _panelRoot.transform.SetParent(canvasGo.transform, false);
        _canvasGroup = _panelRoot.AddComponent<CanvasGroup>();

        var panelRt = _panelRoot.AddComponent<RectTransform>();
        panelRt.anchorMin = new Vector2(0.5f, 0.5f);
        panelRt.anchorMax = new Vector2(0.5f, 0.5f);
        panelRt.pivot     = new Vector2(0.5f, 0.5f);
        panelRt.sizeDelta = new Vector2(440f, 260f);

        var panelImg = _panelRoot.AddComponent<Image>();
        panelImg.color = new Color(0.06f, 0.06f, 0.10f, 0.95f);

        // NPC Name header
        var header = MakeLabel(_panelRoot.transform, "THE HANGMAN", 26f,
            new Vector2(0f, 1f), new Vector2(1f, 1f),
            new Vector2(0f, -20f), new Vector2(0f, -70f));
        header.color     = new Color(0.85f, 0.20f, 0.20f);
        header.fontStyle = FontStyles.Bold;
        header.alignment = TextAlignmentOptions.Center;

        // Divider line
        var divGo = new GameObject("Divider");
        divGo.transform.SetParent(_panelRoot.transform, false);
        var divImg = divGo.AddComponent<Image>();
        divImg.color = new Color(0.5f, 0.1f, 0.1f, 0.8f);
        var divRt = divGo.GetComponent<RectTransform>();
        divRt.anchorMin = new Vector2(0.05f, 1f);
        divRt.anchorMax = new Vector2(0.95f, 1f);
        divRt.pivot     = new Vector2(0.5f, 1f);
        divRt.anchoredPosition = new Vector2(0f, -74f);
        divRt.sizeDelta = new Vector2(0f, 2f);

        // Flavor text
        _flavorText = MakeLabel(_panelRoot.transform, "", 14f,
            new Vector2(0.05f, 0.3f), new Vector2(0.95f, 0.85f),
            Vector2.zero, Vector2.zero);
        _flavorText.color     = new Color(0.80f, 0.75f, 0.70f);
        _flavorText.alignment = TextAlignmentOptions.Center;
        _flavorText.enableWordWrapping = true;

        // Buttons
        _enterBtn = MakeButton(_panelRoot.transform, "ENTER THE ARENA",
            new Color(0.7f, 0.1f, 0.1f), new Vector2(-60f, 30f));
        _enterBtn.onClick.AddListener(OnEnterClicked);

        _leaveBtn = MakeButton(_panelRoot.transform, "LEAVE",
            new Color(0.2f, 0.2f, 0.2f), new Vector2(100f, 30f));
        _leaveBtn.onClick.AddListener(OnLeaveClicked);

        _canvasGroup.alpha = 0f;
        canvasGo.SetActive(false);
        _panelRoot.transform.parent.gameObject.SetActive(false);

        // Store canvas ref so we can activate it
        _panelRoot.transform.parent.gameObject.SetActive(false);
        // Save the canvas to re-activate on Show
        _panelRoot.transform.parent.GetComponent<Canvas>().enabled = true;
        _panelRoot.transform.parent.gameObject.SetActive(false);
    }

    // ─── Open / Close ─────────────────────────────────────────────────────────
    void OpenPanel()
    {
        _flavorText.text = FlavorLines[_flavorIndex % FlavorLines.Length];
        _flavorIndex++;

        _panelRoot.transform.parent.gameObject.SetActive(true);
        StopAllCoroutines();
        StartCoroutine(FadeIn());
    }

    IEnumerator FadeIn()
    {
        _canvasGroup.alpha = 0f;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * 4f;
            _canvasGroup.alpha = Mathf.Clamp01(t);
            yield return null;
        }
        _canvasGroup.alpha = 1f;
    }

    IEnumerator FadeOut()
    {
        float t = _canvasGroup.alpha;
        while (t > 0f)
        {
            t -= Time.deltaTime * 4f;
            _canvasGroup.alpha = Mathf.Clamp01(t);
            yield return null;
        }
        _canvasGroup.alpha = 0f;
        _panelRoot.transform.parent.gameObject.SetActive(false);
        _activeNPC = null;
    }

    // ─── Button Callbacks ─────────────────────────────────────────────────────
    void OnEnterClicked()
    {
        if (_activeNPC == null) { Hide(); return; }
        StartCoroutine(FadeOut());
        _activeNPC.ConfirmChallenge();
    }

    void OnLeaveClicked() => Hide();

    // ─── ESC to close ─────────────────────────────────────────────────────────
    void Update()
    {
        if (_panelRoot == null) return;
        if (!_panelRoot.transform.parent.gameObject.activeSelf) return;
        if (UnityEngine.InputSystem.Keyboard.current?.escapeKey.wasPressedThisFrame == true)
            Hide();
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────
    static TextMeshProUGUI MakeLabel(Transform parent, string text, float size,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
    {
        var go  = new GameObject("Label");
        go.transform.SetParent(parent, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text     = text;
        tmp.fontSize = size;
        tmp.color    = Color.white;
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
        rt.offsetMin = offsetMin; rt.offsetMax = offsetMax;
        return tmp;
    }

    static Button MakeButton(Transform parent, string label, Color bg, Vector2 anchoredPos)
    {
        var go = new GameObject(label);
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color = bg;
        var btn = go.AddComponent<Button>();
        var rt  = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0f);
        rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot     = new Vector2(0.5f, 0f);
        rt.sizeDelta = new Vector2(180f, 40f);
        rt.anchoredPosition = anchoredPos;

        var lblGo  = new GameObject("Label");
        lblGo.transform.SetParent(go.transform, false);
        var lblTmp = lblGo.AddComponent<TextMeshProUGUI>();
        lblTmp.text      = label;
        lblTmp.fontSize  = 13f;
        lblTmp.color     = Color.white;
        lblTmp.alignment = TextAlignmentOptions.Center;
        lblTmp.fontStyle = FontStyles.Bold;
        var lblRt = lblGo.GetComponent<RectTransform>();
        lblRt.anchorMin = Vector2.zero; lblRt.anchorMax = Vector2.one;
        lblRt.offsetMin = lblRt.offsetMax = Vector2.zero;

        return btn;
    }
}
#endif
