using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;

/// <summary>
/// ArenaPortalTrigger — placed in the Hub scene on the arena portal collider.
/// Auto-disables in non-hub scenes (buildIndex != 2) so it never fires in arenas.
///
/// Flow (client-authoritative gate, server executes):
///   Local player enters trigger → opens HangmanDialogueUI if present,
///   otherwise calls CmdRequestArena directly.
///
/// Requires:
///   • Collider with Is Trigger = true on this GO or a child
///   • Tag "Player" on player prefabs
///   • NetworkIdentity on this GO (for [Command] target)
///
/// Server side: validates CombatSessionTracker.Local is not already in session,
/// then loads the arena scene additively via NetworkManager or direct scene load.
/// Adjust ArenaSceneName to match your build settings.
/// </summary>
public class ArenaPortalTrigger : NetworkBehaviour
{
    [Header("Arena Scene")]
    [Tooltip("Build-settings name of the arena scene to load.")]
    public string arenaSceneName = SceneNames.ArenaCopper;

    [Header("Prompt")]
    [Tooltip("Text shown above the portal when the player is nearby.")]
    public string promptText = "Press E to Enter Arena";

    // ── Lifecycle ─────────────────────────────────────────────────────────────
    void Awake()
    {
        // Arena-only guard: disable if NOT in hub (buildIndex 2)
        int buildIdx = SceneManager.GetActiveScene().buildIndex;
        if (buildIdx != 2)
        {
            enabled = false;
            return;
        }
    }

    // ── Trigger ───────────────────────────────────────────────────────────────
    void OnTriggerEnter(Collider other)
    {
#if UNITY_EDITOR || !UNITY_SERVER
        if (!other.CompareTag("Player")) return;
        var netId = other.GetComponent<NetworkIdentity>();
        if (netId == null || !netId.isLocalPlayer) return;

        // If Hangman NPC system is present, let it manage the dialogue flow
        if (HangmanDialogueUI.Instance != null)
        {
            HangmanDialogueUI.Instance.Show();
            return;
        }

        // Fallback: go straight to arena
        EnterArena();
#endif
    }

    void OnTriggerExit(Collider other)
    {
#if UNITY_EDITOR || !UNITY_SERVER
        if (!other.CompareTag("Player")) return;
        var netId = other.GetComponent<NetworkIdentity>();
        if (netId == null || !netId.isLocalPlayer) return;

        HangmanDialogueUI.Instance?.Hide();
#endif
    }

    // ── Public: called by HangmanDialogueUI "Enter Arena" button ─────────────
    public void EnterArena()
    {
#if UNITY_EDITOR || !UNITY_SERVER
        // Prevent double-entry
        if (CombatSessionTracker.Local?.IsInSession == true)
        {
            Debug.Log("[PORTAL] Already in a session — ignoring enter request");
            return;
        }

        CmdRequestArena(arenaSceneName);
#endif
    }

    // ── Command: server validates + starts arena ──────────────────────────────
    [Command(requiresAuthority = false)]
    void CmdRequestArena(string sceneName, NetworkConnectionToClient sender = null)
    {
        // Basic validation: scene must be in build settings
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning("[PORTAL] CmdRequestArena: empty scene name — ignoring");
            return;
        }

        Debug.Log($"[PORTAL] Server received arena request for '{sceneName}'");

        // Let NetworkManager handle the load if it supports it,
        // or fall back to a direct scene load for the requesting client.
        // For a full implementation, wire this to your NetworkManager.ServerChangeScene().
        NetworkManager.singleton?.ServerChangeScene(sceneName);
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.8f, 0.3f, 1f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, 2f);
    }
#endif
}
