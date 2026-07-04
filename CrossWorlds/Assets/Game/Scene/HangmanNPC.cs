using UnityEngine;
using Mirror;

/// <summary>
/// HangmanNPC — Hub NPC that challenges players to enter the arena.
/// Attach to the Hangman NPC GameObject in the Hub scene.
///
/// Copy to: Assets/Game/Scene/HangmanNPC.cs
///
/// Auto-registers with NPCInteractionManager on Start.
/// No Inspector wiring required — but set interactRadius in Inspector if needed.
///
/// Flow:
///   Player enters radius → NPCInteractionManager shows "Press E to challenge The Hangman"
///   Player presses E     → HangmanDialogueUI opens
///   Player clicks Enter  → CmdChallengeHangman() → server validates → ServerChangeScene("Arena")
///
/// Server logs: [NPC] Player char#X challenged The Hangman
/// </summary>
public class HangmanNPC : NetworkBehaviour, NPCInteractionManager.IHubNPC
{
    [Header("Interaction")]
    public float interactRadius = 3.5f;

    [Header("Billboard Prompt (optional — NPCInteractionManager handles global prompt)")]
    public bool showBillboard = false;

    // ─── IHubNPC interface ────────────────────────────────────────────────────
    public float InteractRadius => interactRadius;
    new public Transform transform => base.transform;

    // ─── Lifecycle ────────────────────────────────────────────────────────────
    void Start()
    {
        NPCInteractionManager.Register(this);
    }

    void OnDestroy()
    {
        NPCInteractionManager.Unregister(this);
    }

    // ─── IHubNPC.Interact — called by NPCInteractionManager on E press ────────
    public void Interact()
    {
#if !UNITY_SERVER
        // Guard: don't open if already in an arena session
        if (CombatSessionTracker.Local?.IsInSession == true)
        {
            Debug.Log("[NPC] Already in a combat session — cannot challenge The Hangman");
            return;
        }

        // Open the dialogue panel (client-side only)
        HangmanDialogueUI.Show(this);
#endif
    }

    // ─── Called by HangmanDialogueUI when player clicks "Enter Arena" ─────────
    public void ConfirmChallenge()
    {
#if !UNITY_SERVER
        CmdChallengeHangman();
#endif
    }

    // ─── Command: client → server ──────────────────────────────────────────────
    [Command(requiresAuthority = false)]
    void CmdChallengeHangman(NetworkConnectionToClient sender = null)
    {
        // Server-side validation
        if (sender == null || sender.identity == null) return;

        var pid = sender.identity.GetComponent<PlayerIdentity>();
        int charId = pid != null ? pid.characterId : 0;

        Debug.Log($"[NPC] Player char#{charId} challenged The Hangman");

        // Transition all players to Arena
        // Note: Mirror ServerChangeScene moves ALL connected clients.
        // For per-player instancing, replace with additive scene loading in Phase 2.
        NetworkManager.singleton.ServerChangeScene("Arena");
    }

#if !UNITY_SERVER
    // ─── Optional: gizmo to show interact radius in scene view ────────────────
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }
#endif
}
