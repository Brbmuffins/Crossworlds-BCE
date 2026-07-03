#if !UNITY_SERVER
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

// INPCInteractable is defined in INPCInteractable.cs (no server guard — HangmanNPC needs it on both builds)

// ═══════════════════════════════════════════════════════════════════════════
//  NPCInteractionManager
//  Self-bootstrapping singleton. DontDestroyOnLoad.
//
//  • Tracks the single currently-nearby NPC (last registered wins).
//  • Shows / hides a "Press E to …" TMP label centred on-screen.
//  • In Update() fires Interact() when E is pressed.
//  • Auto-disables its prompt in Arena scenes.
//
//  Usage:
//    NPCInteractionManager.Instance.RegisterNearby(myNPC);
//    NPCInteractionManager.Instance.UnregisterNearby(myNPC);
// ═══════════════════════════════════════════════════════════════════════════
public class NPCInteractionManager : MonoBehaviour
{
    public static NPCInteractionManager Instance { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Bootstrap()
    {
        if (Instance != null) return;
        var go = new GameObject("[NPCInteractionManager]");
        DontDestroyOnLoad(go);
        Instance = go.AddComponent<NPCInteractionManager>();
    }

    // ── State ─────────────────────────────────────────────────────────────────
    INPCInteractable _currentNPC;
    GameObject       _promptGO;
    TextMeshProUGUI  _promptLabel;
    Image            _promptBg;

    static readonly Color ColText = new Color(1.00f, 0.95f, 0.60f, 1f);
    static readonly Color ColBg   = new Color(0.00f, 0.00f, 0.00f, 0.55f);

    // ── Lifecycle ─────────────────────────────────────────────────────────────
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        BuildUI();
        SetVisible(false);
    }

    void Update()
    {
        // Auto-hide in Arena scenes
        if (SceneManager.GetActiveScene().name.Contains("Arena"))
        {
            if (_promptGO != null && _promptGO.activeSelf) SetVisible(false);
            return;
        }

        if (_currentNPC == null) return;

        var kb = UnityEngine.InputSystem.Keyboard.current;
        if (kb == null) return;

        // Don't fire if typing in an input field
        if (IsTypingInField()) return;

        if (kb.eKey.wasPressedThisFrame)
            _currentNPC.Interact();
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>Register an NPC as the one nearest the local player.</summary>
    public void RegisterNearby(INPCInteractable npc)
    {
        _currentNPC = npc;
        if (_promptLabel != null)
            _promptLabel.text = npc.PromptText;
        SetVisible(true);
    }

    /// <summary>Unregister an NPC (only clears if it is the currently registered one).</summary>
    public void UnregisterNearby(INPCInteractable npc)
    {
        if (_currentNPC != npc) return;
        _currentNPC = null;
        SetVisible(false);
    }

    // ── Build UI ──────────────────────────────────────────────────────────────
    void BuildUI()
    {
        // Canvas
        var cGO = new GameObject("NPCPromptCanvas");
        cGO.transform.SetParent(transform, false);
        var canvas = cGO.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 110;
        var scaler = cGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight  = 0.5f;

        // Prompt pill — centre-screen, 140px above midpoint
        _promptGO = new GameObject("NPC_Prompt");
        _promptGO.transform.SetParent(cGO.transform, false);
        var rt = _promptGO.AddComponent<RectTransform>();
        rt.anchorMin        = new Vector2(0.5f, 0.5f);
        rt.anchorMax        = new Vector2(0.5f, 0.5f);
        rt.pivot            = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(0f, 140f);
        rt.sizeDelta        = new Vector2(360f, 42f);

        // Background pill
        _promptBg       = _promptGO.AddComponent<Image>();
        _promptBg.color = ColBg;

        // Label
        var labelGO = new GameObject("PromptLabel");
        labelGO.transform.SetParent(_promptGO.transform, false);
        var lRT = labelGO.AddComponent<RectTransform>();
        lRT.anchorMin = Vector2.zero;
        lRT.anchorMax = Vector2.one;
        lRT.offsetMin = new Vector2(10f, 0f);
        lRT.offsetMax = new Vector2(-10f, 0f);

        _promptLabel             = labelGO.AddComponent<TextMeshProUGUI>();
        _promptLabel.text        = "Press E to interact";
        _promptLabel.fontSize    = 18f;
        _promptLabel.color       = ColText;
        _promptLabel.alignment   = TextAlignmentOptions.Center;
        _promptLabel.fontStyle   = FontStyles.Bold;
    }

    void SetVisible(bool vis)
    {
        if (_promptGO != null) _promptGO.SetActive(vis);
    }

    static bool IsTypingInField()
    {
        var es = UnityEngine.EventSystems.EventSystem.current;
        if (es == null) return false;
        var sel = es.currentSelectedGameObject;
        return sel != null && sel.GetComponent<TMP_InputField>() != null;
    }
}
#endif
