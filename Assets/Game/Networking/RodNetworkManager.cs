using Mirror;
using UnityEngine;

// ═══════════════════════════════════════════════════════════════════════════
//  RodNetworkManager
//
//  Inspector setup:
//    • classPrefabs[0] = Engineer prefab
//    • classPrefabs[1] = Guardian prefab
//    • classPrefabs[2] = Wraith prefab
//    • classPrefabs[3] = Medic prefab
//    • Authenticator   = RodNetworkAuthenticator (same GameObject)
//    • Online Scene    = "GameWorld"
//    • Network Address = 15.204.243.36
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

[AddComponentMenu("RoD/Network/Rod Network Manager")]
public class RodNetworkManager : NetworkManager
{
    [Header("Class Prefabs")]
    [Tooltip("0=Engineer, 1=Guardian, 2=Wraith, 3=Medic")]
    public GameObject[] classPrefabs;

    [Header("Auth Server")]
    [Tooltip("Must match RodNetworkAuthenticator.authServerURL")]
    public string authServerURL = "http://15.204.243.36:3000";

    // ── Self-configure ────────────────────────────────────────────────────────

    public override void Awake()
    {
        autoCreatePlayer = false;
        playerPrefab     = null;

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

    // ── Client startup ────────────────────────────────────────────────────────

    public override void OnStartClient()
    {
        base.OnStartClient();
        foreach (var prefab in classPrefabs)
            if (prefab != null) NetworkClient.RegisterPrefab(prefab);
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
        if (classPrefabs == null || classPrefabs.Length == 0)
        {
            Debug.LogError("[RodNM] classPrefabs is empty — run RoD ▶ Setup ▶ 4.");
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

        // Spawn position: DB saved position, or Mirror start position, or safe default
        Vector3 spawnPos;
        if (auth != null && auth.fromDB)
        {
            spawnPos = new Vector3(auth.spawnX, auth.spawnY, auth.spawnZ);
        }
        else
        {
            Transform startPos = GetStartPosition();
            spawnPos = startPos != null ? startPos.position : new Vector3(0f, 2f, 0f);
        }

        GameObject player = Instantiate(prefab, spawnPos, Quaternion.identity);
        player.name = msg.username;

        var identity = player.GetComponent<PlayerIdentity>();
        if (identity != null)
        {
            identity.playerName = msg.username;
            identity.classIndex = classIndex;
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
        Debug.Log($"[RodNM] Spawned {msg.username} as class {classIndex} at {spawnPos} " +
                  $"(fromDB={auth?.fromDB})");
    }

    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        // RodPositionSaver.OnDestroy handles the HTTP call when the player object
        // is destroyed as part of disconnect — nothing extra needed here.
        base.OnServerDisconnect(conn);
        Debug.Log($"[RodNM] Client disconnected: {conn}");
    }

    // ── Client ───────────────────────────────────────────────────────────────

    public override void OnClientConnect()
    {
        base.OnClientConnect();

        // Send username + class (class is dev-mode fallback only;
        // server uses DB value when auth.fromDB = true)
        NetworkClient.Send(new CreatePlayerMessage
        {
            username      = PlayerPrefs.GetString("username", "Player"),
            selectedClass = PlayerPrefs.GetInt("SelectedCharacter", 0)
        });
    }

    public override void OnClientDisconnect()
    {
        base.OnClientDisconnect();
        Debug.Log("[RodNM] Disconnected from server.");
    }

    // ── Utility ───────────────────────────────────────────────────────────────

    public static void ConnectToServer()
    {
        if (singleton == null) { Debug.LogError("[RodNM] No NetworkManager singleton."); return; }
        singleton.StartClient();
    }
}
