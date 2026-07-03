using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;

/// <summary>
/// RodNetworkManager — CrossWorlds BCE custom NetworkManager.
///
/// Handles:
///   - Player prefab spawn with PlayerIdentity sync
///   - CombatSessionTracker.NotifyAllySpawned() hook (auto, no manual wire-up)
///   - Scene flow: LoginScene → CharacterSelect → Hub → Arena
///   - DontDestroyOnLoad singleton (Mirror standard)
///
/// Setup:
///   Add to a NetworkManager GameObject in LoginScene.
///   Assign playerPrefab in inspector.
///   Assign the 4 scene names in inspector or leave defaults.
///
/// Copy to: Assets/Game/Systems/RodNetworkManager.cs
/// </summary>
public class RodNetworkManager : NetworkManager
{
    // ─── Inspector ────────────────────────────────────────────────────────────
    [Header("Scene Names")]
    public string loginScene       = "LoginScene";
    public string charSelectScene  = "CharacterSelect";
    public string hubScene         = "Hub";
    public string arenaScene       = "Arena";

    [Header("Spawn")]
    [Tooltip("How many seconds after Arena load before spawning players")]
    public float arenaSpawnDelay = 1f;

    // ─── Singleton ────────────────────────────────────────────────────────────
    public static new RodNetworkManager singleton => (RodNetworkManager)NetworkManager.singleton;

    // ─── Server: Player Spawn ─────────────────────────────────────────────────
    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        // Spawn at the registered start positions, or fall back to origin
        Transform start = GetStartPosition();
        Vector3    pos  = start != null ? start.position : Vector3.zero;
        Quaternion rot  = start != null ? start.rotation : Quaternion.identity;

        GameObject player = Instantiate(playerPrefab, pos, rot);
        NetworkServer.AddPlayerForConnection(conn, player);

        // ── Wire identity data (sent from client via CmdSetIdentity) ─────────
        // PlayerIdentity.playerName / classIndex / characterId are set via
        // [Command] calls from the client once this object exists.

        Debug.Log($"[NET] Player spawned for conn#{conn.connectionId} at {pos}");
    }

    // ─── Server: Player Disconnect ────────────────────────────────────────────
    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        base.OnServerDisconnect(conn);
        Debug.Log($"[NET] Client disconnected: conn#{conn.connectionId}");
    }

    // ─── Client: Connection ───────────────────────────────────────────────────
    public override void OnClientConnect()
    {
        base.OnClientConnect();
        Debug.Log("[NET] Client connected to server");
    }

    // ─── Client: Scene Changed ────────────────────────────────────────────────
    public override void OnClientSceneChanged()
    {
        base.OnClientSceneChanged();

        string scene = SceneManager.GetActiveScene().name;

        if (scene == arenaScene)
        {
            // ArenaSessionController handles BeginSession via SceneManager.sceneLoaded,
            // but we also hook ally spawn here once local player exists.
            StartCoroutine(NotifyAllySpawnWhenReady());
        }
    }

    // ─── Ally Spawn Hook ──────────────────────────────────────────────────────
    /// <summary>
    /// Waits until NetworkClient.localPlayer exists then notifies CombatSessionTracker.
    /// Runs client-side so CombatSessionTracker.Local is the correct instance.
    /// </summary>
    IEnumerator NotifyAllySpawnWhenReady()
    {
        float timeout = 10f;
        float elapsed = 0f;

        while (NetworkClient.localPlayer == null && elapsed < timeout)
        {
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;
        }

        if (NetworkClient.localPlayer == null)
        {
            Debug.LogWarning("[NET] NotifyAllySpawn timed out — local player never appeared");
            yield break;
        }

        // Notify all connected players (including self)
        foreach (var conn in NetworkServer.connections.Values)
        {
            if (conn.identity != null)
                CombatSessionTracker.Local?.NotifyAllySpawned(conn.identity.gameObject);
        }

        Debug.Log("[NET] CombatSessionTracker notified of ally spawns");
    }

    // ─── Server: Scene Change ─────────────────────────────────────────────────
    public override void OnServerSceneChanged(string sceneName)
    {
        base.OnServerSceneChanged(sceneName);
        Debug.Log($"[NET] Server scene changed to: {sceneName}");
    }

    // ─── Scene Navigation Helpers (call from UI) ──────────────────────────────
    [Server]
    public void GoToHub()   => ServerChangeScene(hubScene);

    [Server]
    public void GoToArena() => ServerChangeScene(arenaScene);

    [Server]
    public void GoToHub_FromArena() => ServerChangeScene(hubScene);
}
