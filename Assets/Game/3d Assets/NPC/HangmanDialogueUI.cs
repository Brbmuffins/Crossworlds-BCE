#if UNITY_EDITOR || !UNITY_SERVER
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// HangmanDialogueUI — self-bootstrapping singleton.
/// Auto-creates its own Canvas child panel. DontDestroyOnLoad.
///
/// Shows:
///   • NPC header: "The Hangman"
///   • Random rotating flavor line (one of 3, picked on Show())
///   • [Enter Arena] button  → HangmanNPC.Instance.ConfirmChallenge()
///   • [Leave] button + ESC → Hide() with fade-out
///
/// Fade in / out via CanvasGroup.alpha coroutine (0.2s each).
/// Auto-finds HangmanNPC via FindObjectOfType if Instance is null.
/// Sort order 130 — above inventory/mastery UI.
/// </summary>
public class HangmanDialogueUI : MonoBehaviour
{
    public static HangmanDialogueUI Instance { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Bootstrap()
    {
        if (Instance != null) return;
        var go = new GameObject("[HangmanDialogueUI]");
        DontDestroyOnLoad(go);
        Instance = go.AddComponent<HangmanDialogueUI>();
    }

    // ── Flavor lines ──────────────────────────────────────────────────────────
    static readonly string[] FlavorLines =
    {
        "The arena awaits. Only the strong survive.",
        "Step inside. Your glory — or your grave.",
        "Many challengers. Few champions.",
    };

    // ── Colours ───────────────────────────────────────────────────────────────
    static readonly Color ColOverlay  = new Color(0.00f, 0.00f, 0.03f, 0.80f);
    static readonly Color ColPanel    = new Color(0.06f, 0.05f, 0.10f, 0.97f);
    static readonly Color ColBorder   = new Color(0.50f, 0.10f, 0.10f, 1.00f); // blood-red border
    static readonly Color ColTitle    = new Color(0.90f, 0.80f, 0.60f, 1.00f);
    static readonly Color ColFlavor   = new Color(0.75f, 0.70f, 0.65f, 0.90f);
    static readonly Color ColEnterBtn = new Color(0.65f, 0.10f, 0.10f, 1.00f);
    static readonly Color ColLeaveBtn = new Color(0.20f, 0.20f, 0.25f, 1.00f);
    static readonly Color ColBtnText  = new Color(1.00f, 1.00f, 1.00f, 0.95f);

    // ── UI refs ───────────────────────────────────────────────────────────────
    CanvasGroup     _group;
    GameObject      _panel;
    TextMeshProUGUI _flavorText;
    GameObject      _notificationGO;
    TextMeshProUGUI _notificationText;
    bool            _open = false;

    // ── Lifecycle ─────────────────────────────────────────────────────────────
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        BuildUI();
        _group.alpha          = 0f;
        _group.interactable   = false;
        _group.blocksRaycasts = false;
        _panel.SetActive(false);
    }

    void Update()
    {
        if (_open && UnityEngine.InputSystem.Keyboard.current?.escapeKey.wasPressedThisFrame == true)
            Hide();
    }

    // ── Public API ────────────────────────────────────────────────────────────

    public void Show()
    {
        if (_open) return;
        _open = true;

        // Pick a random flavor line
        _flavorText.text = FlavorLines[Random.Range(0, FlavorLines.Length)];

        _panel.SetActive(true);
        StopAllCoroutines();
        StartCoroutine(FadeTo(1f, 0.2f));
    }

    public void Hide()
    {
        if (!_open) return;
        _open = false;
        StopAllCoroutines();
        StartCoroutine(FadeOutThenDisable());
    }

    // ── Button handlers ───────────────────────────────────────────────────────
    void OnEnterArena()
    {
        var npc = HangmanNPC.Instance ?? Object.FindFirstObjectByType<HangmanNPC>();
        if (npc == null)
        {
            Debug.LogWarning("[HANGMAN UI] HangmanNPC not found — cannot confirm challenge");
            Hide();
            return;
        }
        npc.ConfirmChallenge();
    }

    void OnLeave() => Hide();

    /// <summary>
    /// Show a brief toast notification (e.g. "You're already in combat!").
    /// Does NOT open the main dialogue panel — safe to call at any time.
    /// Auto-dismisses after 2.5 seconds.
    /// </summary>
    public void ShowMessage(string msg)
    {
        if (_notificationText == null || _notificationGO == null) return;
        _notificationText.text = msg;
        StopCoroutine(nameof(DismissNotification)); // cancel any running dismiss
        StartCoroutine(nameof(DismissNotification));
    }

    IEnumerator DismissNotification()
    {
        _notificationGO.SetActive(true);
        yield return new WaitForSecondsRealtime(2.5f);
        _notificationGO.SetActive(false);
    }

    // ── Fade coroutines ───────────────────────────────────────────────────────
    IEnumerator FadeTo(float target, float duration)
    {
        float start   = _group.alpha;
        float elapsed = 0f;
        _group.interactable   = true;
        _group.blocksRaycasts = true;

        while (elapsed < duration)
        {
            elapsed         += Time.unscaledDeltaTime;
            _group.alpha     = Mathf.Lerp(start, target, elapsed / duration);
            yield return null;
        }
        _group.alpha = target;
    }

    IEnumerator FadeOutThenDisable()
    {
        _group.interactable   = false;
        _group.blocksRaycasts = false;
        yield return StartCoroutine(FadeTo(0f, 0.2f));
        _panel.SetActive(false);
    }

    // ── Build UI ──────────────────────────────────────────────────────────────
    void BuildUI()
    {
        // Canvas
        var cGO = new GameObject("HangmanDialogueCanvas");
        cGO.transform.SetParent(transform, false);
        var canvas = cGO.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 130;
        var scaler = cGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight  = 0.5f;
        cGO.AddComponent<GraphicRaycaster>();

        // CanvasGroup on canvas root for global alpha control
        _group = cGO.AddComponent<CanvasGroup>();

        // Screen dim overlay
        var dimGO = new GameObject("Dim");
        dimGO.transform.SetParent(cGO.transform, false);
        Stretch(dimGO.AddComponent<RectTransform>());
        dimGO.AddComponent<Image>().color = ColOverlay;

        // Panel — centred, 420×280
        _panel = new GameObject("HangmanPanel");
        _panel.transform.SetParent(cGO.transform, false);
        var pRT = _panel.AddComponent<RectTransform>();
        pRT.anchorMin = new Vector2(0.5f, 0.5f);
        pRT.anchorMax = new Vector2(0.5f, 0.5f);
        pRT.pivot     = new Vector2(0.5f, 0.5f);
        pRT.anchoredPosition = Vector2.zero;
        pRT.sizeDelta = new Vector2(420f, 280f);

        // Border (behind panel bg)
        var borderGO = new GameObject("Border");
        borderGO.transform.SetParent(_panel.transform, false);
        var bRT = borderGO.AddComponent<RectTransform>();
        bRT.anchorMin = Vector2.zero; bRT.anchorMax = Vector2.one;
        bRT.offsetMin = new Vector2(-2f, -2f); bRT.offsetMax = new Vector2(2f, 2f);
        borderGO.AddComponent<Image>().color = ColBorder;
        borderGO.transform.SetAsFirstSibling();

        // Panel background
        _panel.AddComponent<Image>().color = ColPanel;

        // ── Title: "The Hangman" ──────────────────────────────────────────────
        var titleGO = MakeTMP("Title", _panel.transform,
            new Vector2(0f, 0.72f), new Vector2(1f, 1f));
        titleGO.text      = "The Hangman";
        titleGO.fontSize  = 26f;
        titleGO.color     = ColTitle;
        titleGO.fontStyle = FontStyles.Bold;
        titleGO.alignment = TextAlignmentOptions.Center;

        // ── Divider line ──────────────────────────────────────────────────────
        var divGO = new GameObject("Divider");
        divGO.transform.SetParent(_panel.transform, false);
        var divRT = divGO.AddComponent<RectTransform>();
        divRT.anchorMin        = new Vector2(0.05f, 0.70f);
        divRT.anchorMax        = new Vector2(0.95f, 0.70f);
        divRT.offsetMin        = new Vector2(0f, -1f);
        divRT.offsetMax        = new Vector2(0f,  1f);
        divGO.AddComponent<Image>().color = new Color(0.4f, 0.15f, 0.15f, 0.6f);

        // ── Flavor line ───────────────────────────────────────────────────────
        _flavorText = MakeTMP("FlavorLine", _panel.transform,
            new Vector2(0.05f, 0.36f), new Vector2(0.95f, 0.68f));
        _flavorText.fontSize  = 14f;
        _flavorText.color     = ColFlavor;
        _flavorText.fontStyle = FontStyles.Italic;
        _flavorText.alignment = TextAlignmentOptions.Center;
        _flavorText.enableWordWrapping = true;
        _flavorText.text = FlavorLines[0];

        // ── [Enter Arena] button ──────────────────────────────────────────────
        var enterBtn = MakeButton("EnterArenaBtn", _panel.transform,
            new Vector2(0.08f, 0.08f), new Vector2(0.55f, 0.30f),
            ColEnterBtn, "Enter Arena", 16f);
        enterBtn.onClick.AddListener(OnEnterArena);

        // ── [Leave] button ────────────────────────────────────────────────────
        var leaveBtn = MakeButton("LeaveBtn", _panel.transform,
            new Vector2(0.60f, 0.08f), new Vector2(0.92f, 0.30f),
            ColLeaveBtn, "Leave", 14f);
        leaveBtn.onClick.AddListener(OnLeave);

        // Hint: ESC to close
        var hintTxt = MakeTMP("EscHint", _panel.transform,
            new Vector2(0f, 0f), new Vector2(1f, 0.08f));
        hintTxt.text      = "Press ESC to leave";
        hintTxt.fontSize  = 9f;
        hintTxt.color     = new Color(0.5f, 0.5f, 0.55f, 0.7f);
        hintTxt.alignment = TextAlignmentOptions.Center;

        // ── Notification pill (shared with canvas root, not nested in panel) ───
        // Appears above the panel briefly when ShowMessage() is called.
        _notificationGO = new GameObject("Notification");
        _notificationGO.transform.SetParent(cGO.transform, false);
        var nRT = _notificationGO.AddComponent<RectTransform>();
        nRT.anchorMin        = new Vector2(0.5f, 0.5f);
        nRT.anchorMax        = new Vector2(0.5f, 0.5f);
        nRT.pivot            = new Vector2(0.5f, 0.5f);
        nRT.anchoredPosition = new Vector2(0f, 220f);
        nRT.sizeDelta        = new Vector2(320f, 44f);
        _notificationGO.AddComponent<Image>().color = new Color(0.55f, 0.08f, 0.08f, 0.92f);

        _notificationText = MakeTMP("NotifText", _notificationGO.transform,
            Vector2.zero, Vector2.one);
        _notificationText.fontSize  = 14f;
        _notificationText.color     = Color.white;
        _notificationText.fontStyle = FontStyles.Bold;
        _notificationText.alignment = TextAlignmentOptions.Center;
        _notificationText.text      = "";
        _notificationGO.SetActive(false);
    }

    // ── UI helpers ────────────────────────────────────────────────────────────
    static TextMeshProUGUI MakeTMP(string name, Transform parent, Vector2 anchMin, Vector2 anchMax)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchMin; rt.anchorMax = anchMax;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        var t = go.AddComponent<TextMeshProUGUI>();
        t.fontSize  = 14f;
        t.color     = Color.white;
        t.alignment = TextAlignmentOptions.Center;
        return t;
    }

    static Button MakeButton(string name, Transform parent,
        Vector2 anchMin, Vector2 anchMax, Color bgColor, string label, float fontSize)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchMin; rt.anchorMax = anchMax;
        rt.offsetMin = new Vector2(4f, 4f); rt.offsetMax = new Vector2(-4f, -4f);

        var img = go.AddComponent<Image>();
        img.color = bgColor;

        var btn = go.AddComponent<Button>();
        var colors = btn.colors;
        colors.normalColor      = bgColor;
        colors.highlightedColor = new Color(bgColor.r + 0.12f, bgColor.g + 0.12f, bgColor.b + 0.12f, 1f);
        colors.pressedColor     = new Color(bgColor.r - 0.10f, bgColor.g - 0.10f, bgColor.b - 0.10f, 1f);
        colors.selectedColor    = bgColor;
        btn.colors = colors;

        var lbl = MakeTMP($"{name}_Label", go.transform, Vector2.zero, Vector2.one);
        lbl.text      = label;
        lbl.fontSize  = fontSize;
        lbl.color     = new Color(1f, 1f, 1f, 0.95f);
        lbl.fontStyle = FontStyles.Bold;
        lbl.alignment = TextAlignmentOptions.Center;

        return btn;
    }

    static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }
}
#endif
