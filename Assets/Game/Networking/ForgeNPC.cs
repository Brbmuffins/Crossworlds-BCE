using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// ForgeNPC — proximity trigger that opens the ForgeCraftingPanel when the local player presses E.
///
/// Place on any NPC GameObject in the Hub. Requires a Collider (set as trigger).
/// Shows a world-space "Press E to Craft" billboard prompt when the player is in range.
///
/// No Mirror NetworkBehaviour — ForgeNPC is a purely client-side interaction.
/// The NPC visual is just a scene object; crafting API calls happen in ForgeCraftingPanel.
/// </summary>
public class ForgeNPC : MonoBehaviour
{
    [Header("Crafting")]
    // Retained for existing scene builders and serialized Forge NPCs. The
    // current panel loads every recipe and no longer filters by this value.
    [HideInInspector] public int professionId = 2;

    [Tooltip("Display name shown in the E-prompt")]
    public string npcName = "Forge Master";

    [Header("Interaction")]
    public float interactRange = 3.5f;

    // ── Prompt billboard ──────────────────────────────────────────────────────
    GameObject _promptGO;
    TextMesh   _promptMesh;
    bool       _promptVisible;

    // ── State ─────────────────────────────────────────────────────────────────
    Transform  _localPlayer;
    Camera     _camera;
    float      _scanTimer;

    // ── Lifecycle ─────────────────────────────────────────────────────────────
    void Awake()  { BuildPrompt(); }
    void Start()  { _promptGO.SetActive(false); }

    void Update()
    {
        // Find local player periodically
        _scanTimer -= Time.deltaTime;
        if (_localPlayer == null && _scanTimer <= 0f)
        {
            _scanTimer = 0.5f;
            _localPlayer = FindLocalPlayer();
        }
        if (_localPlayer == null) return;

        bool inRange = (transform.position - _localPlayer.position).sqrMagnitude <= interactRange * interactRange;

        // Show/hide prompt
        if (inRange != _promptVisible)
        {
            _promptVisible = inRange;
            _promptGO.SetActive(inRange);
        }

        // Billboard: face camera
        if (_promptVisible)
        {
            if (_camera == null || !_camera.isActiveAndEnabled)
                _camera = Camera.main;
            if (_camera != null)
                _promptGO.transform.rotation = Quaternion.LookRotation(
                    _promptGO.transform.position - _camera.transform.position,
                    _camera.transform.up);
        }

        // Interact
        if (inRange && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            OpenCrafting();
        }
    }

    // ── Actions ───────────────────────────────────────────────────────────────
    void OpenCrafting()
    {
#if UNITY_EDITOR || !UNITY_SERVER
        // Canonical crafting UI is ForgeCraftingPanel (Smelt + Craft tabs,
        // /api/professions/recipes). It loads all of the character's recipes, so
        // professionId isn't passed here.
        if (ForgeCraftingPanel.Instance != null)
            ForgeCraftingPanel.Instance.Open();
        else
            Debug.LogWarning("[ForgeNPC] ForgeCraftingPanel.Instance is null — add the ForgeCraftingPanel to the Hub scene and wire its Inspector fields.");
#endif
    }

    // ── Prompt world-space text ───────────────────────────────────────────────
    void BuildPrompt()
    {
        _promptGO = new GameObject("ForgePrompt");
        _promptGO.transform.SetParent(transform, false);
        _promptGO.transform.localPosition = new Vector3(0f, 3f, 0f);
        _promptGO.transform.localScale    = Vector3.one * 0.018f;

        _promptMesh = _promptGO.AddComponent<TextMesh>();
        _promptMesh.text          = $"[E]  {npcName}";
        _promptMesh.characterSize = 0.55f;
        _promptMesh.fontSize      = 60;
        _promptMesh.fontStyle     = FontStyle.Bold;
        _promptMesh.anchor        = TextAnchor.MiddleCenter;
        _promptMesh.alignment     = TextAlignment.Center;
        _promptMesh.color         = new Color(1.00f, 0.82f, 0.20f);
    }

    // ── Find local player ─────────────────────────────────────────────────────
    static Transform FindLocalPlayer()
    {
        return PlayerIdentity.Local != null ? PlayerIdentity.Local.transform : null;
    }

    // ── Editor gizmo ─────────────────────────────────────────────────────────
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.7f, 0.1f, 0.3f);
        Gizmos.DrawSphere(transform.position, interactRange);
    }
}
