using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// ForgeNPC — proximity trigger that opens CraftingUI when the local player presses E.
///
/// Place on any NPC GameObject in the Hub. Requires a Collider (set as trigger).
/// Shows a world-space "Press E to Craft" billboard prompt when the player is in range.
///
/// profession_id defaults to 1 (Smithing). Can be changed in Inspector for Mining (2), etc.
///
/// No Mirror NetworkBehaviour — ForgeNPC is a purely client-side interaction.
/// The NPC visual is just a scene object; crafting API calls happen in CraftingUI.
/// </summary>
public class ForgeNPC : MonoBehaviour
{
    [Header("Crafting")]
    [Tooltip("Profession opened by this NPC: 1=Smithing, 2=Mining, 3=Alchemy")]
    public int professionId = 1;

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

        float dist = Vector3.Distance(transform.position, _localPlayer.position);
        bool inRange = dist <= interactRange;

        // Show/hide prompt
        if (inRange != _promptVisible)
        {
            _promptVisible = inRange;
            _promptGO.SetActive(inRange);
        }

        // Billboard: face camera
        if (_promptVisible)
        {
            var cam = Camera.main;
            if (cam != null)
                _promptGO.transform.rotation = Quaternion.LookRotation(
                    _promptGO.transform.position - cam.transform.position,
                    cam.transform.up);
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
#if !UNITY_SERVER
        if (CraftingUI.Instance != null)
            CraftingUI.Instance.Open(professionId);
        else
            Debug.LogWarning($"[ForgeNPC] CraftingUI.Instance is null — make sure it's in the scene or self-bootstrapping.");
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
        foreach (var id in FindObjectsByType<Mirror.NetworkIdentity>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
            if (id.isLocalPlayer) return id.transform;
        return null;
    }

    // ── Editor gizmo ─────────────────────────────────────────────────────────
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.7f, 0.1f, 0.3f);
        Gizmos.DrawSphere(transform.position, interactRange);
    }
}
