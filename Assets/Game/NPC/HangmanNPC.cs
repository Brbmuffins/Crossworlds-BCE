using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;

/// <summary>
/// HangmanNPC — NetworkBehaviour. Place on the Hangman NPC GameObject in the Hub scene.
/// Implements INPCInteractable so NPCInteractionManager handles the E-key prompt.
///
/// Auto-setup:
///   • SphereCollider trigger (r=3) auto-created in Awake if none present.
///   • Registers itself as Instance on Awake.
///
/// Flow:
///   Player enters trigger zone
///   → NPCInteractionManager shows "Press E to challenge The Hangman"
///   → Player presses E → Interact() → CombatSessionTracker.IsInSession check
///   → If clear: HangmanDialogueUI.Instance.Show()
///   → Player clicks [Enter Arena] → ConfirmChallenge() → CmdChallengeHangman()
///   → Server validates not-in-arena → NetworkManager.ServerChangeScene("Arena")
/// </summary>
public class HangmanNPC : NetworkBehaviour, INPCInteractable
{
    // ── Singleton ─────────────────────────────────────────────────────────────
    public static HangmanNPC Instance { get; private set; }

    // ── INPCInteractable ──────────────────────────────────────────────────────
    public string PromptText => "Press E to challenge The Hangman";

    // ── Config ────────────────────────────────────────────────────────────────
    [Header("Trigger Zone")]
    [Tooltip("Auto-created if no Collider is present. Override radius here.")]
    public float interactRadius = 3f;

    [Header("Arena")]
    public string arenaSceneName = SceneNames.ArenaCopper;

    // ── Lifecycle ─────────────────────────────────────────────────────────────
    void Awake()
    {
        if (Instance != null && Instance != this)
            Debug.LogWarning("[HANGMAN] Duplicate HangmanNPC in scene — there should only be one.");
        Instance = this;

        // Auto-create trigger collider if not already configured
        if (GetComponent<Collider>() == null)
        {
            var sc    = gameObject.AddComponent<SphereCollider>();
            sc.radius    = interactRadius;
            sc.isTrigger = true;
        }
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    // ── Trigger zone ──────────────────────────────────────────────────────────
    void OnTriggerEnter(Collider other)
    {
        // Only for the LOCAL player
        var ni = other.GetComponent<NetworkIdentity>();
        if (ni == null || !ni.isLocalPlayer) return;

        // Only interactable in Hub
        if (!SceneManager.GetActiveScene().name.Contains(SceneNames.Hub)) return;

#if UNITY_EDITOR || !UNITY_SERVER
        NPCInteractionManager.Instance?.RegisterNearby(this);
#endif
    }

    void OnTriggerExit(Collider other)
    {
        var ni = other.GetComponent<NetworkIdentity>();
        if (ni == null || !ni.isLocalPlayer) return;

#if UNITY_EDITOR || !UNITY_SERVER
        NPCInteractionManager.Instance?.UnregisterNearby(this);
        HangmanDialogueUI.Instance?.Hide();
#endif
    }

    // ── INPCInteractable.Interact ─────────────────────────────────────────────
    public void Interact()
    {
#if UNITY_EDITOR || !UNITY_SERVER
        // Block if already in a session
        if (CombatSessionTracker.Local?.IsInSession == true)
        {
            Debug.Log("[HANGMAN] Player is already in a combat session — challenge blocked");
            HangmanDialogueUI.Instance?.ShowMessage("You're already in combat!");
            return;
        }

        HangmanDialogueUI.Instance?.Show();
#endif
    }

    // ── Confirm challenge (called from HangmanDialogueUI) ─────────────────────
    /// <summary>
    /// Called by HangmanDialogueUI when the player clicks [Enter Arena].
    /// Hides the dialogue and sends the Command to the server.
    /// </summary>
    public void ConfirmChallenge()
    {
#if UNITY_EDITOR || !UNITY_SERVER
        HangmanDialogueUI.Instance?.Hide();
#endif
        CmdChallengeHangman();
    }

    // ── Command ───────────────────────────────────────────────────────────────
    // ── TargetRpc: "already in session" reply ─────────────────────────────────
    [TargetRpc]
    void TargetAlreadyInSession(NetworkConnection target)
    {
#if UNITY_EDITOR || !UNITY_SERVER
        HangmanDialogueUI.Instance?.ShowMessage("You're already in combat!");
#endif
    }

    [Command(requiresAuthority = false)]
    void CmdChallengeHangman(NetworkConnectionToClient sender = null)
    {
        // Server guard: don't change if already in Arena
        if (SceneManager.GetActiveScene().name.Contains("Arena"))
        {
            Debug.LogWarning("[NPC] Already in Arena — CmdChallengeHangman ignored");
            if (sender != null) TargetAlreadyInSession(sender);
            return;
        }

        // Log with character ID if available
        if (sender?.identity != null)
        {
            var pid    = sender.identity.GetComponent<PlayerIdentity>();
            int charId = pid != null ? pid.characterId : -1;
            Debug.Log($"[NPC] Player char#{charId} challenged The Hangman");
        }
        else
        {
            Debug.Log("[NPC] Player challenged The Hangman (no sender identity)");
        }

        // Scene change — all clients follow
        if (NetworkServer.active && NetworkManager.singleton != null)
            NetworkManager.singleton.ServerChangeScene(arenaSceneName);
        else
            Debug.LogWarning("[NPC] ServerChangeScene failed — NetworkManager not active");
    }

    // ── Gizmo ─────────────────────────────────────────────────────────────────
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.8f, 0.3f, 0.3f, 0.25f);
        Gizmos.DrawSphere(transform.position, interactRadius);
    }
}
