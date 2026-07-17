using System.Collections.Generic;
using Mirror;
using UnityEngine;

// ═══════════════════════════════════════════════════════════════════════════
//  RodNetworkManager
//
//  Inspector setup:
//    • classPrefabs[0] = Warden prefab
//    • classPrefabs[1] = Ironclad prefab
//    • classPrefabs[2] = Shadowblade prefab
//    • classPrefabs[3] = Cleric prefab
//    • classPrefabs[4] = Arcanist prefab
//    • Authenticator   = RodNetworkAuthenticator (same GameObject)
//    • Network Address = 15.204.243.36
//
//  offlineScene / onlineScene are set in Awake() — do NOT set in Inspector.
//
//  Class selection is now server-authoritative:
//    - Production: RodNetworkAuthenticator fetches class from DB after JWT verify.
//      OnCreatePlayer reads conn.authenticationData (RodPlayerAuth.classIndex).
//    - Dev mode: authenticationData.fromDB = false, falls back to CreatePlayerMessage.
//
//  Spawn position:
//    - Production: last saved position from DB (RodPlayerAuth.spawnX/Y/Z).
//    - Dev/first login: default spawn or Mirror start position.
// ═══════════════════════════════════════════════════════════════════════════

[AddComponentMenu("BCE/Network/Rod Network Manager")]
public class RodNetworkManager : NetworkManager
{
    [Header("Class Prefabs")]
    [Tooltip("0=Warden, 1=Ironclad, 2=Shadowblade, 3=Cleric, 4=Arcanist")]
    public GameObject[] classPrefabs;

    [Header("World / Combat Prefabs")]
    [Tooltip("Enemy types, WorldItem, bosses — registered as spawnable on both client and server. " +
             "Assign: Enemy_Grunt, Enemy_Ranged, Enemy_Elite, WorldItem, Wisp_Mob, Wraith.")]
    public GameObject[] worldPrefabs;

    [Header("Auth Server")]
    [Tooltip("Must match RodNetworkAuthenticator.authServerURL")]
    public string authServerURL = "http://15.204.243.36:3000";

    readonly Dictionary<int, CreatePlayerMessage> _lastCreatePlayerMessages = new Dictionary<int, CreatePlayerMessage>();

    // ── Self-configure ────────────────────────────────────────────────────────

    public override void Awake()
    {
        autoCreatePlayer = false;
        playerPrefab     = null;

        // Wire scenes in code so they're never mis-set in the Inspector.
        // Mirror uses offlineScene to auto-navigate back to login on disconnect —
        // this is what makes Logout and chat teardown work correctly.
        offlineScene = "Assets/Game/Scenes/LoginScene.unity";
        onlineScene  = "Assets/Game/Scenes/Darkwood.unity";   // renamed from Hub.unity

        if (transport == null)
            transport = GetComponent<Mirror.Transport>();
        if (authenticator == null)
            authenticator = GetComponent<NetworkAuthenticator>();

        base.Awake();
    }

    // ── Custom network message ────────────────────────────────────────────────
    // selectedClass is only used in dev mode (fromDB = false).
    // In production the server reads class from conn.authenticationData.

    public struct CreatePlayerMessage : NetworkMessage
    {
        public string username;
        public int    selectedClass;
    }

    // ── Headless dedicated-server auto-start ──────────────────────────────────

    public override void Start()
    {
        base.Start();

        // GraphicsDeviceType.Null = dedicated server build with Server Optimizations.
        // Skip the LoginManager UI and go straight to StartServer().
        if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null)
        {
            Debug.Log("[RodNM] Headless server detected — StartServer()");
            StartServer();
        }
    }

    // ── Client startup ────────────────────────────────────────────────────────
    // Two-layer registration so prefabs are found in both player builds and editor:
    //   1. Add to spawnPrefabs BEFORE base.OnStartClient() — Mirror's built-in path
    //      calls RegisterPrefab on everything in that list during base execution.
    //   2. Call RegisterPrefab directly AFTER base as belt-and-suspenders.
    // This prevents "Could not spawn assetId=..." errors in non-editor builds where
    // Mirror can't recompute assetId from GUID at runtime (no #if UNITY_EDITOR branch).

    public override void OnStartClient()
    {
        // Stage all prefabs into spawnPrefabs BEFORE base.OnStartClient() so Mirror's
        // built-in RegisterPrefab pass picks them all up in one shot.
        RegisterIntoSpawnList(classPrefabs);
        RegisterIntoSpawnList(worldPrefabs);

        base.OnStartClient(); // registers everything now in spawnPrefabs

        // Belt-and-suspenders direct registration for non-editor builds
        DirectRegister(classPrefabs);
        DirectRegister(worldPrefabs);
    }

    void RegisterIntoSpawnList(GameObject[] prefabs)
    {
        if (prefabs == null) return;
        foreach (var p in prefabs)
            if (p != null && !spawnPrefabs.Contains(p))
                spawnPrefabs.Add(p);
    }

    void DirectRegister(GameObject[] prefabs)
    {
        if (prefabs == null) return;
        foreach (var p in prefabs)
            if (p != null) NetworkClient.RegisterPrefab(p);
    }

    // ── Client connected + authenticated ──────────────────────────────────────
    // Mirror fires OnClientConnect AFTER authentication completes (when an
    // authenticator is present). Send CreatePlayerMessage here so the server
    // knows which class to spawn and what username to assign.

    public override void OnClientConnect()
    {
        base.OnClientConnect();

        NetworkClient.Send(new CreatePlayerMessage
        {
            username      = PlayerPrefs.GetString("username", "Player"),
            selectedClass = PlayerPrefs.GetInt("SelectedCharacter", 0),
        });
        Debug.Log("[RodNM] Sent CreatePlayerMessage — awaiting spawn.");
    }

    // ── Server ───────────────────────────────────────────────────────────────

    public override void OnStartServer()
    {
        base.OnStartServer();
        NetworkServer.RegisterHandler<CreatePlayerMessage>(OnCreatePlayer);
    }

    public override void OnServerConnect(NetworkConnectionToClient conn)
    {
        base.OnServerConnect(conn);
        // Auth is handled by RodNetworkAuthenticator — wait for CreatePlayerMessage.
    }

    void OnCreatePlayer(NetworkConnectionToClient conn, CreatePlayerMessage msg)
    {
        _lastCreatePlayerMessages[conn.connectionId] = msg;

        if (classPrefabs == null || classPrefabs.Length == 0)
        {
            Debug.LogError("[RodNM] classPrefabs is empty — run BCE ▶ Setup ▶ 4.");
            return;
        }

        // Guard against a client sending CreatePlayerMessage more than once.
        // A second AddPlayerForConnection throws and leaks the just-instantiated
        // prefab as an orphan in the server scene.
        if (conn.identity != null)
        {
            Debug.LogWarning("[RodNM] Connection already has a player — ignoring duplicate CreatePlayerMessage.");
            return;
        }

        var auth = conn.authenticationData as RodPlayerAuth;

        // Class: prefer DB value; fall back to what the client sent (dev mode only)
        int classIndex = (auth != null && auth.fromDB)
            ? Mathf.Clamp(auth.classIndex, 0, classPrefabs.Length - 1)
            : Mathf.Clamp(msg.selectedClass, 0, classPrefabs.Length - 1);

        GameObject prefab = classPrefabs[classIndex];
        if (prefab == null)
        {
            Debug.LogError($"[RodNM] No prefab for class {classIndex} — falling back to 0.");
            prefab = classPrefabs[0];
        }

        // Spawn position: DB saved position, or Mirror start position, or safe default.
        // Guard: if DB coords are all zero the character has never saved a position
        // (first login). Treat that as a fresh spawn so players don't pile up at origin.
        Vector3 spawnPos;
        bool hasSavedPos = auth != null && auth.fromDB
                           && (auth.spawnX != 0f || auth.spawnY != 0f || auth.spawnZ != 0f);

        if (hasSavedPos)
        {
            spawnPos = new Vector3(auth.spawnX, auth.spawnY, auth.spawnZ);
        }
        else
        {
            Transform startPos = GetStartPosition();
            if (startPos != null)
            {
                spawnPos = startPos.position;
            }
            else
            {
                // Scatter players in a ring so they don't spawn inside each other
                float angle = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
                spawnPos = new Vector3(Mathf.Sin(angle) * 3f, 1f, Mathf.Cos(angle) * 3f);
            }
        }

        // Prefer server-verified username from auth data; fall back to client-sent value
        string username = (auth != null && !string.IsNullOrEmpty(auth.username))
            ? auth.username : msg.username;

        GameObject player = Instantiate(prefab, spawnPos, Quaternion.identity);
        player.name = username;

        var identity = player.GetComponent<PlayerIdentity>();
        if (identity != null)
        {
            identity.playerName = username;
            identity.classIndex = classIndex;
            identity.characterId = auth != null ? auth.characterId : -1;
        }

        // Attach position saver — saves back to DB on disconnect or app quit
        if (auth != null && auth.characterId > 0)
        {
            var saver = player.AddComponent<RodPositionSaver>();
            saver.characterId   = auth.characterId;
            saver.authServerURL = authServerURL;
            saver.jwt           = auth.jwt;
        }

        NetworkServer.AddPlayerForConnection(conn, player);
        Debug.Log($"[RodNM] Spawned {username} as class {classIndex} at {spawnPos} " +
                  $"(fromDB={auth?.fromDB}, hasSavedPos={hasSavedPos})");
    }

    public override void OnServerSceneChanged(string sceneName)
    {
        base.OnServerSceneChanged(sceneName);

        if (PlaceHubReturnPlayers(sceneName))
            return;

        RespawnMissingPlayersAfterSceneChange(sceneName);
    }

    bool PlaceHubReturnPlayers(string sceneName)
    {
        if (!HubReturnArrival.TryGetRequestForScene(sceneName, out string spawnId, out bool applySpawnRotation))
            return false;

        Transform spawnPoint = HubReturnSpawnPoint.Find(spawnId);
        if (spawnPoint == null)
        {
            Debug.LogWarning($"[RodNM] Hub return spawn '{spawnId}' was not found in {sceneName}. Falling back to normal scene spawns.");
            HubReturnArrival.Clear();
            RespawnMissingPlayersAfterSceneChange(sceneName);
            return true;
        }

        int total = CountAuthenticatedConnections();
        int index = 0;
        int moved = 0;
        int respawned = 0;

        foreach (NetworkConnectionToClient conn in NetworkServer.connections.Values)
        {
            if (conn == null || !conn.isAuthenticated)
                continue;

            Vector3 offset = HubReturnArrival.OffsetForPlayer(spawnPoint, index, total);
            index++;

            if (conn.identity != null)
            {
                HubReturnArrival.PlacePlayer(conn.identity.gameObject, spawnPoint, offset, applySpawnRotation);
                moved++;
                continue;
            }

            Quaternion rotation = applySpawnRotation ? spawnPoint.rotation : Quaternion.identity;
            if (SpawnPlayerForSceneChange(
                    conn,
                    spawnPoint.position + offset,
                    rotation,
                    "hub return"))
            {
                respawned++;
            }
        }

        HubReturnArrival.Clear();
        Debug.Log($"[RodNM] Hub return landed {moved} existing player(s) and respawned {respawned} player(s) at {spawnPoint.name}.");
        return true;
    }

    void RespawnMissingPlayersAfterSceneChange(string sceneName)
    {
        int respawned = 0;

        foreach (NetworkConnectionToClient conn in NetworkServer.connections.Values)
        {
            if (conn == null || !conn.isAuthenticated || conn.identity != null)
                continue;

            if (SpawnPlayerForSceneChange(conn, null, null, $"scene change to {sceneName}"))
                respawned++;
        }

        if (respawned > 0)
            Debug.Log($"[RodNM] Respawned {respawned} player(s) after scene change to {sceneName}.");
    }

    bool SpawnPlayerForSceneChange(
        NetworkConnectionToClient conn,
        Vector3? forcedSpawnPos,
        Quaternion? forcedSpawnRotation,
        string context)
    {
        if (classPrefabs == null || classPrefabs.Length == 0)
        {
            Debug.LogError("[RodNM] classPrefabs is empty; cannot respawn player after scene change.");
            return false;
        }

        if (conn.identity != null)
            return false;

        var auth = conn.authenticationData as RodPlayerAuth;
        CreatePlayerMessage msg = GetRememberedCreatePlayerMessage(conn);

        int classIndex = (auth != null && auth.fromDB)
            ? Mathf.Clamp(auth.classIndex, 0, classPrefabs.Length - 1)
            : Mathf.Clamp(msg.selectedClass, 0, classPrefabs.Length - 1);

        GameObject prefab = classPrefabs[classIndex];
        if (prefab == null)
        {
            Debug.LogError($"[RodNM] No prefab for class {classIndex}; falling back to 0.");
            prefab = classPrefabs[0];
        }

        Vector3 spawnPos;
        Quaternion spawnRot = forcedSpawnRotation ?? Quaternion.identity;
        if (forcedSpawnPos.HasValue)
        {
            spawnPos = forcedSpawnPos.Value;
        }
        else
        {
            Transform startPos = GetStartPosition();
            if (startPos != null)
            {
                spawnPos = startPos.position;
                if (!forcedSpawnRotation.HasValue)
                    spawnRot = startPos.rotation;
            }
            else
            {
                float angle = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
                spawnPos = new Vector3(Mathf.Sin(angle) * 3f, 1f, Mathf.Cos(angle) * 3f);
            }
        }

        string username = (auth != null && !string.IsNullOrEmpty(auth.username))
            ? auth.username : msg.username;
        if (string.IsNullOrWhiteSpace(username))
            username = "Player";

        GameObject player = Instantiate(prefab, spawnPos, spawnRot);
        player.name = username;

        var identity = player.GetComponent<PlayerIdentity>();
        if (identity != null)
        {
            identity.playerName = username;
            identity.classIndex = classIndex;
            identity.characterId = auth != null ? auth.characterId : -1;
        }

        if (auth != null && auth.characterId > 0)
        {
            var saver = player.AddComponent<RodPositionSaver>();
            saver.characterId = auth.characterId;
            saver.authServerURL = authServerURL;
            saver.jwt = auth.jwt;
        }

        NetworkServer.AddPlayerForConnection(conn, player);
        Debug.Log($"[RodNM] Spawned {username} as class {classIndex} for {context} at {spawnPos}.");
        return true;
    }

    int CountAuthenticatedConnections()
    {
        int count = 0;
        foreach (NetworkConnectionToClient conn in NetworkServer.connections.Values)
        {
            if (conn != null && conn.isAuthenticated)
                count++;
        }

        return count;
    }

    CreatePlayerMessage GetRememberedCreatePlayerMessage(NetworkConnectionToClient conn)
    {
        if (_lastCreatePlayerMessages.TryGetValue(conn.connectionId, out CreatePlayerMessage msg))
            return msg;

        var auth = conn.authenticationData as RodPlayerAuth;
        return new CreatePlayerMessage
        {
            username = auth != null && !string.IsNullOrEmpty(auth.username) ? auth.username : "Player",
            selectedClass = auth != null ? auth.classIndex : 0,
        };
    }

    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        base.OnServerDisconnect(conn);
        _lastCreatePlayerMessages.Remove(conn.connectionId);
    }
}
