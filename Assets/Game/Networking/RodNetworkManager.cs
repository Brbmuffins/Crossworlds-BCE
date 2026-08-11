using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

// ═══════════════════════════════════════════════════════════════════════════
//  RodNetworkManager
//
//  Inspector setup:
//    • classPrefabs[0] = Marauder prefab
//    • classPrefabs[1] = Ironclad prefab
//    • classPrefabs[2] = Shadowblade prefab
//    • classPrefabs[3] = Cleric prefab
//    • classPrefabs[4] = Arcanist prefab
//    • classPrefabs[5] = Necromancer prefab
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
    [Tooltip("0=Marauder, 1=Templar, 2=Night Hunter, 3=Cleric, 4=Arcanist, 5=Necromancer")]
    public GameObject[] classPrefabs;

    [Header("World / Combat Prefabs")]
    [Tooltip("Enemy types, WorldItem, bosses — registered as spawnable on both client and server. " +
             "Assign: Enemy_Grunt, Enemy_Ranged, Enemy_Elite, WorldItem, Wisp_Mob, Wraith.")]
    public GameObject[] worldPrefabs;

    [Header("Persistent Networked Objects")]
    [Tooltip("ChatManager prefab (NetworkIdentity + RodChatManager). Run BCE/Setup/4p to create and wire. " +
             "Spawned once on the server at startup; persists across ServerChangeScene via DontDestroyOnLoad.")]
    public GameObject chatManagerPrefab;

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
        //
        // onlineScene is the empty CONTAINER, not Hub (ROADMAP 6.3). Every zone —
        // Hub included — is loaded additively on top of it by ZoneManager, so
        // players can be in different zones at once. Pointing this back at Hub
        // re-breaks that: Mirror would ServerChangeScene everyone into Hub.
        offlineScene = SceneNames.LoginPath;
        onlineScene  = SceneNames.ContainerPath;

        if (transport == null)
            transport = GetComponent<Mirror.Transport>();
        if (authenticator == null)
            authenticator = GetComponent<NetworkAuthenticator>();

        base.Awake();
    }

    // ── Return to character select without a full logout ──────────────────────
    // ESC-menu "Change Character": tear down the game connection and KEEP the session
    // (jwt_token) so the player picks a different class without re-entering credentials.
    //
    // We do NOT redirect offlineScene to CharacterSelect: that scene has no baked
    // NetworkManager, and on disconnect Mirror pulls the live manager out of DDOL
    // expecting the offline scene to supply a fresh one — so landing on CharacterSelect
    // directly leaves NetworkManager.singleton == null ("No NetworkManager" at Deploy).
    // Instead we take the normal disconnect path to LoginScene (which DOES have a
    // NetworkManager) and set a one-shot flag; LoginManager reads it on load and
    // forwards straight to CharacterSelect, skipping the login UI.
    public static bool PendingChangeCharacter;

    public void ReturnToCharacterSelect()
    {
        PendingChangeCharacter = true;

        if (NetworkServer.active && NetworkClient.isConnected)
            StopHost();                 // editor/dev host mode
        else if (NetworkClient.isConnected || NetworkClient.active)
            StopClient();               // normal client connected to the VPS game server
        else
        {
            // Not connected — the live manager is still the DDOL singleton, so a direct
            // load keeps it. No disconnect will fire, so clear the flag and go straight.
            PendingChangeCharacter = false;
            UnityEngine.SceneManagement.SceneManager.LoadScene(SceneNames.CharacterSelect);
        }
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
            ApplyServerLaunchArgs();
            Debug.Log("[RodNM] Headless server detected — StartServer()");
            StartServer();
        }
    }

    // ── Launch-arg overrides (dev/prod on one binary) ─────────────────────────
    //   -port <n>       UDP port for the KCP transport (prod 7777 / dev 7778)
    //   -authurl <url>  auth server this game server validates JWTs against
    //                   (prod http://127.0.0.1:3000 / dev http://127.0.0.1:3010)
    // Missing args keep the Inspector-baked values, so an un-flagged launch is prod.
    void ApplyServerLaunchArgs()
    {
        string[] args = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i] == "-port" && ushort.TryParse(args[i + 1], out ushort p))
            {
                if (transport is PortTransport pt)
                {
                    pt.Port = p;
                    Debug.Log($"[RodNM] Launch arg -port → transport bound to {p}");
                }
                else
                {
                    Debug.LogWarning("[RodNM] -port given but transport is not a PortTransport; ignoring.");
                }
            }
            else if (args[i] == "-authurl")
            {
                string url = args[i + 1];
                authServerURL = url;
                if (authenticator is RodNetworkAuthenticator ra)
                    ra.authServerURL = url;
                Debug.Log($"[RodNM] Launch arg -authurl → validating JWTs against {url}");
            }
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
        GameObject[] generatedLootPrefabs =
            Resources.LoadAll<GameObject>("EnemyForge/Loot");

        // Stage all prefabs into spawnPrefabs BEFORE base.OnStartClient() so Mirror's
        // built-in RegisterPrefab pass picks them all up in one shot.
        RegisterIntoSpawnList(classPrefabs);
        RegisterIntoSpawnList(worldPrefabs);
        RegisterIntoSpawnList(generatedLootPrefabs);
        if (chatManagerPrefab != null)
            RegisterIntoSpawnList(new[] { chatManagerPrefab });

        base.OnStartClient(); // registers everything now in spawnPrefabs

        // Belt-and-suspenders direct registration for non-editor builds
        DirectRegister(classPrefabs);
        DirectRegister(worldPrefabs);
        DirectRegister(generatedLootPrefabs);
        if (chatManagerPrefab != null)
            DirectRegister(new[] { chatManagerPrefab });
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

        // Spawn a single persistent ChatManager that survives ServerChangeScene.
        // DontDestroyOnLoad moves it out of any scene so Mirror won't destroy it on
        // scene transitions; clients receive it via the normal spawn message.
        if (chatManagerPrefab != null)
        {
            var chatGO = Instantiate(chatManagerPrefab);
            DontDestroyOnLoad(chatGO);
            // ZoneScene placement is deliberately NOT applied here: the ChatManager is
            // world-global, not zone-scoped, so it stays in the DontDestroyOnLoad scene.
            // ⚠ That makes it invisible to SceneInterestManagement (strict scene equality),
            // which would silence chat for everyone once IM is enabled — see ROADMAP 6.5.
            NetworkServer.Spawn(chatGO);
            Debug.Log("[RodNM] ChatManager spawned and marked DontDestroyOnLoad.");
        }
        else
        {
            Debug.LogWarning("[RodNM] chatManagerPrefab not assigned — chat will not work. " +
                             "Run BCE/Setup/4p with LoginScene open to fix.");
        }
    }

    public override void OnStopServer()
    {
        // Zones are additively loaded and outlive the server otherwise. Their scene
        // objects keep NetworkIdentity.isServer set — it is a stored flag, not a live
        // read of NetworkServer.active — so NetworkAnimator carries on trying to send
        // Rpcs and logs "called without an active server" every FixedUpdate.
        ZoneManager.Instance?.UnloadAllZones();

        base.OnStopServer();
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
        // Also guard against characters that disconnected while falling through the map (Y < -20).
        Vector3 spawnPos;
        bool hasSavedPos = auth != null && auth.fromDB
                           && (auth.spawnX != 0f || auth.spawnY != 0f || auth.spawnZ != 0f)
                           && auth.spawnY > -20f;

        if (hasSavedPos)
        {
            spawnPos = new Vector3(auth.spawnX, auth.spawnY, auth.spawnZ);
        }
        else
        {
            // ROADMAP 6.5: GetStartPosition() searches every loaded scene, so with zones
            // resident additively it could hand back another zone's start point. Without a
            // saved position, SpawnPlayerIntoZone calls ZoneManager.PlaceAtSpawnPoint once
            // the destination scene exists, which resolves the spawn INSIDE that zone.
            // This value is only the pre-placement seed.
            float angle = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
            spawnPos = new Vector3(Mathf.Sin(angle) * 3f, 1f, Mathf.Cos(angle) * 3f);
        }

        // Prefer server-verified username from auth data; fall back to client-sent value
        string username = (auth != null && !string.IsNullOrEmpty(auth.username))
            ? auth.username : msg.username;

        // ROADMAP 6.3: the player must be filed into their saved zone, which may not
        // be loaded yet. Hand off to a coroutine — AddPlayerForConnection cannot run
        // until the zone scene exists and the client has been told to load it.
        StartCoroutine(SpawnPlayerIntoZone(conn, prefab, classIndex, username, spawnPos, auth, hasSavedPos));
    }

    IEnumerator SpawnPlayerIntoZone(NetworkConnectionToClient conn, GameObject prefab,
                                    int classIndex, string username, Vector3 spawnPos,
                                    RodPlayerAuth auth, bool hasSavedPos)
    {
        // Login always returns players to HUB regardless of the zone they logged out
        // in (overrides ROADMAP 6.2 zone-persistence by request). The saved DB position
        // is only valid inside its own zone, so ignore it here and drop onto HUB's spawn
        // point. In-session zone travel (portals/waypoints) is unaffected.
        string zoneName = SceneNames.Hub;
        hasSavedPos = false;

        if (ZoneManager.Instance == null)
        {
            Debug.LogError("[RodNM] ZoneManager missing — add it to the NetworkManager GameObject " +
                           "(BCE ▶ Setup ▶ 6z). Cannot spawn players.");
            yield break;
        }

        Scene zone = default;
        yield return ZoneManager.Instance.PrepareZone(conn, zoneName, null, s => zone = s);

        if (!zone.IsValid())
        {
            Debug.LogError($"[RodNM] Could not prepare zone '{zoneName}' for {username} — no spawn.");
            yield break;
        }

        // Connection may have dropped during the async scene load.
        if (!NetworkServer.connections.ContainsKey(conn.connectionId) || conn.identity != null)
            yield break;

        GameObject player = Instantiate(prefab, spawnPos, Quaternion.identity);
        player.name = username;

        // File into the zone BEFORE AddPlayerForConnection: interest management reads
        // gameObject.scene when it builds the initial observer set.
        ZoneScene.PlaceIn(player, zone);

        // A saved position is only meaningful inside the zone it was saved in. Without
        // one, fall back to that zone's spawn point rather than to a global search.
        if (!hasSavedPos)
            ZoneManager.Instance.PlaceAtSpawnPoint(player, zone, null);

        var identity = player.GetComponent<PlayerIdentity>();
        if (identity != null)
        {
            identity.playerName = username;
            identity.classIndex = classIndex;
            identity.characterId = auth != null ? auth.characterId : -1;
        }

        // Apply account-owned progression before the player enters combat. Production
        // values came from the server's authenticated GET /character request; the client
        // cannot choose them in CreatePlayerMessage.
        if (auth != null && auth.fromDB)
        {
            var stats = player.GetComponent<CharacterStats>();
            if (stats != null)
                stats.SetProgressionStats(classIndex, auth.level,
                    auth.statStr, auth.statAgi, auth.statInt, auth.statVit);
        }

        // Attach position saver — saves back to DB on disconnect or app quit
        if (auth != null && auth.characterId > 0)
        {
            var saver = player.AddComponent<RodPositionSaver>();
            saver.characterId   = auth.characterId;
            saver.authServerURL = authServerURL;
            saver.jwt           = auth.jwt;
        }

        EnsureHostClientReadyForAddPlayer(conn);
        NetworkServer.AddPlayerForConnection(conn, player);
        ZoneManager.Instance.RegisterInitialPlacement(conn, zone);
        QuestLocalRuntime.ServerLoad(conn);

        Debug.Log($"[RodNM] Spawned {username} as class {classIndex} in zone {zone.name} at " +
                  $"{player.transform.position} (fromDB={auth?.fromDB}, hasSavedPos={hasSavedPos})");
    }


    static void EnsureHostClientReadyForAddPlayer(NetworkConnectionToClient conn)
    {
        if (!(conn is LocalConnectionToClient)) return;
        if (!NetworkClient.active || NetworkClient.ready) return;
        if (NetworkClient.connection == null || !NetworkClient.connection.isAuthenticated) return;

        NetworkClient.Ready();
    }

    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        QuestLocalRuntime.ServerForget(conn);
        // Free the player's zone slot first — this is what lets an emptied zone
        // unload (ROADMAP 6.3). base.OnServerDisconnect destroys conn.identity.
        ZoneManager.Instance?.OnPlayerDisconnected(conn);

        base.OnServerDisconnect(conn);
        _lastCreatePlayerMessages.Remove(conn.connectionId);
    }
}
