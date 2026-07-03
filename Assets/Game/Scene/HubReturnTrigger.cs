using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;

/// <summary>
/// HubReturnTrigger — placed in arena scenes on the return portal collider.
/// Auto-disables in the hub (buildIndex == 2) so it never fires outside arenas.
///
/// Flow:
///   Local player enters trigger → saves progress → disconnects → loads hub scene.
///
/// The save-then-disconnect order ensures progress is not lost on scene change.
/// NetworkManager.singleton.StopClient() is used rather than hard disconnect so
/// Mirror can clean up SyncVars and NetworkIdentities properly.
/// </summary>
public class HubReturnTrigger : MonoBehaviour
{
    [Header("Hub Scene")]
    [Tooltip("Build-settings name of the hub scene. Must match your build order.")]
    public string hubSceneName = "Hub";

    [Header("Prompt")]
    public string promptText = "Press E to Return to Hub";

    bool _returning = false;

    // ── Lifecycle ─────────────────────────────────────────────────────────────
    void Awake()
    {
        // Hub-guard: disable if in hub (buildIndex 2) or login/select scenes (0-1)
        int buildIdx = SceneManager.GetActiveScene().buildIndex;
        if (buildIdx <= 2)
        {
            enabled = false;
            return;
        }
    }

    // ── Trigger ───────────────────────────────────────────────────────────────
    void OnTriggerEnter(Collider other)
    {
#if !UNITY_SERVER
        if (_returning) return;
        if (!other.CompareTag("Player")) return;

        var netId = other.GetComponent<NetworkIdentity>();
        if (netId == null || !netId.isLocalPlayer) return;

        _returning = true;
        ReturnToHub();
#endif
    }

    // ── Return flow ───────────────────────────────────────────────────────────
    void ReturnToHub()
    {
#if !UNITY_SERVER
        Debug.Log("[HUB] Player returning to hub — saving progress...");

        // Save progress before disconnecting
        PlayerProgressManager.Local?.SaveProgress();

        // Disconnect from the game server; NetworkManager will handle cleanup
        if (NetworkManager.singleton != null)
        {
            NetworkManager.singleton.StopClient();
        }
        else
        {
            // Fallback if no NetworkManager in scene
            SceneManager.LoadScene(hubSceneName);
        }
#endif
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.3f, 0.8f, 1f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, 2f);
    }
#endif
}
