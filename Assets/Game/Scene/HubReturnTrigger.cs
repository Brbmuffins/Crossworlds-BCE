using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;

/// <summary>
/// HubReturnTrigger — placed in arena scenes on the return portal collider.
/// Auto-disables in the hub (buildIndex == 2) so it never fires outside arenas.
///
/// Flow:
///   Local player enters trigger → saves progress → server ends arena session
///   → client disconnects → NetworkManager returns to hub via offlineScene.
///
/// The save-then-disconnect order ensures progress is not lost on scene change.
/// NetworkManager.singleton.StopClient() is used rather than hard disconnect so
/// Mirror can clean up SyncVars and NetworkIdentities properly.
/// </summary>
public class HubReturnTrigger : NetworkBehaviour
{
    [Header("Hub Scene")]
    [Tooltip("Build-settings name of the hub scene. Must match your build order.")]
    public string hubSceneName = SceneNames.Hub;

    [Header("Prompt")]
    public string promptText = "Press E to Return to Hub";

#if !UNITY_SERVER
    bool _returning = false;
#endif

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

        PlayerProgressManager.Local?.SaveProgress();

        // Tell server to end the arena session before we disconnect
        if (isClient) CmdEndArenaSession();

        if (NetworkManager.singleton != null)
            NetworkManager.singleton.StopClient();
        else
            SceneManager.LoadScene(hubSceneName);
#endif
    }

    [Command(requiresAuthority = false)]
    void CmdEndArenaSession()
    {
        ArenaSessionController.Instance?.EndSession();
        var spawner = FindAnyObjectByType<WaveSpawner>();
        spawner?.StopWaves();
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.3f, 0.8f, 1f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, 2f);
    }
#endif
}
