using Mirror;
using UnityEngine;

// ═══════════════════════════════════════════════════════════════════════════
//  PlayerIdentity
//  Add to every class prefab (Engineer, Guardian, Wraith, Medic).
//  Syncs the player's name and class to all clients.
//
//  Usage: read playerName / classIndex anywhere on the player object.
// ═══════════════════════════════════════════════════════════════════════════

public class PlayerIdentity : NetworkBehaviour
{
    [SyncVar] public string playerName   = "Player";
    [SyncVar] public int    classIndex   = 0;
    [SyncVar] public int    characterId  = -1;  // DB row id — used by inventory/progress APIs

    static readonly string[] ClassNames = { "Warden", "Ironclad", "Shadowblade", "Cleric", "Arcanist" };

    public string ClassName => classIndex >= 0 && classIndex < ClassNames.Length
        ? ClassNames[classIndex]
        : "Unknown";

    /// <summary>The local player's identity, or null before spawn.</summary>
    public static PlayerIdentity Local { get; private set; }

    // ── Zone travel (ROADMAP 6.4) ─────────────────────────────────────────────

    /// <summary>
    /// Asks the server to move THIS player to another zone. Entry point for
    /// interactables that are plain MonoBehaviours and so cannot send a Command of
    /// their own — HubPortal, for one. Anything that used to call
    /// SceneManager.LoadScene directly must come through here instead: a local scene
    /// load moves the client without telling the server, and on a host it replaces
    /// the active scene and tears down every additively-loaded zone for everyone.
    /// </summary>
    public static void RequestZoneTravel(string zoneName, string spawnId = null)
    {
        if (Local == null)
        {
            Debug.LogWarning("[PlayerIdentity] No local player yet — travel request ignored.");
            return;
        }

        Local.CmdRequestZoneTravel(zoneName, spawnId);
    }

    [Command]
    void CmdRequestZoneTravel(string zoneName, string spawnId)
    {
        if (!SceneNames.IsZone(zoneName))
        {
            Debug.LogWarning($"[PlayerIdentity] {playerName} requested unknown zone '{zoneName}' — denied.");
            return;
        }

        // GM Island is a privilege, not a place. Everything else is open.
        if (string.Equals(zoneName, SceneNames.GMIsland, System.StringComparison.OrdinalIgnoreCase))
        {
            var auth = connectionToClient?.authenticationData as RodPlayerAuth;
            if (auth == null || !auth.gmAllowed)
            {
                Debug.LogWarning($"[PlayerIdentity] {playerName} requested GM Island without GM — denied.");
                return;
            }
        }

        if (ZoneManager.Instance == null)
        {
            Debug.LogError("[PlayerIdentity] ZoneManager missing — run BCE ▶ Setup ▶ 6z.");
            return;
        }

        ZoneManager.Instance.MovePlayerToZone(connectionToClient, zoneName, spawnId);
    }

    public override void OnStopLocalPlayer()
    {
        if (Local == this) Local = null;
    }

    public override void OnStartLocalPlayer()
    {
        Local = this;

        // Tag this as the local player object so other scripts can find it
        gameObject.name = playerName + " (Local)";

        // Refresh nameplate (it will hide itself for local player)
        GetComponent<PlayerNameplate>()?.Refresh();

#if UNITY_EDITOR || !UNITY_SERVER
        // Populate AuthManager so InventoryManager and combat kill API have credentials.
        // SyncVars (characterId) are applied before OnStartLocalPlayer fires.
        AuthManager.CharacterId = characterId;
        AuthManager.Token       = PlayerPrefs.GetString("jwt_token", "");

        // Re-trigger inventory load now that auth is ready (Start() ran too early).
        var inv = InventoryManager.Instance;
        if (inv != null) inv.StartCoroutine(inv.LoadInventory());

        // Same for professions — their Start() also ran before CharacterId was set.
        ProfessionManager.Local?.Load();

        // Wire combat session tracker so it can count healing done this run
        CombatSessionTracker.Local?.NotifyAllySpawned(gameObject);
#endif
    }

    public override void OnStartClient()
    {
        // Update display name for remote players
        if (!isLocalPlayer)
            gameObject.name = playerName;

        // Attach nameplate if not already present, then populate it.
        // SyncVars are populated before OnStartClient fires on the client,
        // so playerName and classIndex are already correct here.
        var plate = GetComponent<PlayerNameplate>();
        if (plate == null) plate = gameObject.AddComponent<PlayerNameplate>();
        plate.Refresh();

        // Notify player list so it updates immediately on join
#if UNITY_EDITOR || !UNITY_SERVER
        PlayerListUI.RequestRefresh();
#endif

        // Session stats: track every player (not just local) so healing done to
        // party members counts. HashSet inside the tracker dedupes re-notifies.
#if UNITY_EDITOR || !UNITY_SERVER
        CombatSessionTracker.Local?.NotifyAllySpawned(gameObject);
#endif
    }

    public override void OnStopClient()
    {
        // Notify player list immediately on leave
#if UNITY_EDITOR || !UNITY_SERVER
        PlayerListUI.RequestRefresh();
#endif
    }
}
