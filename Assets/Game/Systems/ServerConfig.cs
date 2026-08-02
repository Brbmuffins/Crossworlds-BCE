using UnityEngine;

// ═══════════════════════════════════════════════════════════════════════════
//  ServerConfig — single source of truth for the VPS address AND environment.
//
//  Two axes:
//    • IP     — which box. Override with PlayerPrefs "serverIP" for LAN testing;
//               clear to restore the compiled default.
//    • ENV    — Prod vs Dev. The login screen's toggle writes PlayerPrefs
//               "environment" ("prod"/"dev"). Dev points every client→auth call
//               and the Mirror game connection at the ISOLATED dev stack
//               (auth :3001 + game :7778 + its own DB), so dev testing can never
//               touch prod accounts/characters.
//
//  Same binary, both envs: the DEDICATED SERVER picks its port + auth URL from
//  launch args (-port / -authurl in RodNetworkManager), so one build runs as
//  either prod (:7777 → auth :3000) or dev (:7778 → auth :3001).
// ═══════════════════════════════════════════════════════════════════════════

public enum ServerEnvironment { Prod, Dev }

public static class ServerConfig
{
    public const string DefaultServerIP = "15.204.243.36";

    /// <summary>PlayerPrefs key that points the GAME connection somewhere else than auth.</summary>
    public const string GameServerOverrideKey = "gameServerIP";

    /// <summary>PlayerPrefs key holding the selected environment ("prod" | "dev").</summary>
    public const string EnvironmentKey = "environment";

    // ── Per-environment ports ────────────────────────────────────────────────
    // Auth = the Node/Express API; Game = the Mirror/KCP UDP listener.
    // NOTE: 3001 is Uptime Kuma, 3500 SpacetimeDB, 4000 dashboard, 5000 rod-realtime —
    // all taken. Dev auth uses 3002 (next free); dev game uses 7778.
    public const int    ProdAuthPort = 3000;
    public const int    DevAuthPort  = 3002;
    public const ushort ProdGamePort = 7777;
    public const ushort DevGamePort  = 7778;

    // ── Environment ──────────────────────────────────────────────────────────
    public static ServerEnvironment Environment
    {
        get => PlayerPrefs.GetString(EnvironmentKey, "prod") == "dev"
            ? ServerEnvironment.Dev
            : ServerEnvironment.Prod;
        set
        {
            PlayerPrefs.SetString(EnvironmentKey, value == ServerEnvironment.Dev ? "dev" : "prod");
            PlayerPrefs.Save();
        }
    }

    public static bool IsDev => Environment == ServerEnvironment.Dev;

    /// <summary>Auth API port for the current environment.</summary>
    public static int AuthPort => IsDev ? DevAuthPort : ProdAuthPort;

    /// <summary>Mirror/KCP game port for the current environment. Apply to the transport before StartClient().</summary>
    public static ushort GamePort => IsDev ? DevGamePort : ProdGamePort;

    // ── Addresses ─────────────────────────────────────────────────────────────
    // Reads PlayerPrefs override first; falls back to compiled constant.
    public static string ServerIP =>
        PlayerPrefs.GetString("serverIP", DefaultServerIP);

    /// <summary>
    /// Base URL for ALL client→auth calls. Environment-aware: dev traffic goes to
    /// the isolated dev auth on :3001. Every REST singleton already builds off this,
    /// so flipping the toggle reroutes login, inventory, mastery, crafting, etc.
    /// </summary>
    public static string AuthBaseUrl => $"http://{ServerIP}:{AuthPort}";

    /// <summary>
    /// Where the Mirror client connects. Normally the same box as the auth API, but
    /// they are two different services and testing needs to split them: connecting a
    /// second client to an editor host on 127.0.0.1 still has to authenticate against
    /// the real auth server, since there is no local copy of it.
    /// Set via BCE ▶ Testing ▶ Game Server, or clear to fall back to ServerIP.
    /// </summary>
    public static string GameServerIP =>
        PlayerPrefs.GetString(GameServerOverrideKey, ServerIP);

    public static bool HasGameServerOverride =>
        !string.IsNullOrEmpty(PlayerPrefs.GetString(GameServerOverrideKey, ""));
}
