using Mirror;
using UnityEngine;

// ═══════════════════════════════════════════════════════════════════════════
//  RodNetworkManager
//  Replace the default NetworkManager component on your NetworkManager
//  GameObject with this one.
//
//  Inspector setup:
//    • classPrefabs[0] = Engineer prefab   (must be registered in spawnable prefabs)
//    • classPrefabs[1] = Guardian prefab
//    • classPrefabs[2] = Wraith prefab
//    • classPrefabs[3] = Medic prefab
//    • Authenticator   = RodNetworkAuthenticator (same GameObject)
//    • Online Scene    = "GameWorld"
//    • Network Address = 15.204.243.36
//
//  Flow (client side):
//    CharacterSelectUI.Play() → StartClient() → authenticator handshake →
//    OnClientConnect() → send CreatePlayerMessage with username + class
//
//  Flow (server side):
//    OnServerAddPlayer is NOT used (autoCreate = false).
//    Instead we handle CreatePlayerMessage and spawn the right prefab.
// ═══════════════════════════════════════════════════════════════════════════

[AddComponentMenu("RoD/Network/Rod Network Manager")]
public class RodNetworkManager : NetworkManager
{
    [Header("Class Prefabs")]
    [Tooltip("Index must match CharacterSelectUI character order: 0=Engineer, 1=Guardian, 2=Wraith, 3=Medic")]
    public GameObject[] classPrefabs;

    // ── Self-configure ────────────────────────────────────────────────────────

    public override void Awake()
    {
        // Never let Mirror auto-spawn a default player — we spawn per-class.
        autoCreatePlayer = false;
        playerPrefab     = null;

        // If the editor setup script already wired transport / authenticator,
        // these are no-ops. If someone adds this component manually without
        // running the setup menu, auto-grab them from the same GameObject.
        if (transport == null)
            transport = GetComponent<Mirror.Transport>();
        if (authenticator == null)
            authenticator = GetComponent<NetworkAuthenticator>();

        base.Awake();
    }

    // ── Custom network message ────────────────────────────────────────────────

    public struct CreatePlayerMessage : NetworkMessage
    {
        public string username;
        public int    selectedClass;
    }

    // ── Client startup — register all class prefabs with Mirror ──────────────

    public override void OnStartClient()
    {
        base.OnStartClient();

        // Mirror requires every networked prefab to be registered on the client
        // so it knows how to instantiate them when the server spawns one.
        foreach (var prefab in classPrefabs)
        {
            if (prefab != null)
                NetworkClient.RegisterPrefab(prefab);
        }
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
        // Authentication is handled by RodNetworkAuthenticator.
        // We don't do anything here — wait for CreatePlayerMessage instead.
    }

    void OnCreatePlayer(NetworkConnectionToClient conn, CreatePlayerMessage msg)
    {
        if (classPrefabs == null || classPrefabs.Length == 0)
        {
            Debug.LogError("[RodNM] classPrefabs is empty! Run RoD ▶ Setup ▶ 4 to assign class prefabs.");
            return;
        }

        // Clamp to valid range
        int classIndex = Mathf.Clamp(msg.selectedClass, 0, classPrefabs.Length - 1);
        GameObject prefab = classPrefabs[classIndex];

        if (prefab == null)
        {
            Debug.LogError($"[RodNM] No prefab assigned for class index {classIndex}. Using index 0.");
            prefab = classPrefabs[0];
        }

        // Pick a spawn point (uses Mirror's built-in start positions, or safe default above ground)
        Transform startPos = GetStartPosition();
        GameObject player = startPos != null
            ? Instantiate(prefab, startPos.position, startPos.rotation)
            : Instantiate(prefab, new Vector3(0f, 2f, 0f), Quaternion.identity);

        // Stamp the player's name so it shows up in the game world
        player.name = msg.username;

        // Try to set display name if the player has a component for it
        PlayerIdentity identity = player.GetComponent<PlayerIdentity>();
        if (identity != null)
        {
            identity.playerName = msg.username;
            identity.classIndex = classIndex;
        }

        NetworkServer.AddPlayerForConnection(conn, player);
        Debug.Log($"[RodNM] Spawned {msg.username} as class {classIndex} at {player.transform.position}");
    }

    // ── Client ───────────────────────────────────────────────────────────────

    public override void OnClientConnect()
    {
        base.OnClientConnect();

        // Authenticator has already accepted us. Now tell the server who we are
        // and which class to spawn.
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

    /// <summary>
    /// Call from a UI button to connect to the server.
    /// Address and port are set in the NetworkManager Inspector.
    /// </summary>
    public static void ConnectToServer()
    {
        if (singleton == null)
        {
            Debug.LogError("[RodNM] No NetworkManager singleton found.");
            return;
        }
        singleton.StartClient();
    }
}
