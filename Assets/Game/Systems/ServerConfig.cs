using UnityEngine;

// ═══════════════════════════════════════════════════════════════════════════
//  ServerConfig — single source of truth for the VPS address.
//  To override at runtime (e.g. LAN testing) set PlayerPrefs key "serverIP"
//  before login; clear it to restore the compiled default.
// ═══════════════════════════════════════════════════════════════════════════
public static class ServerConfig
{
    public const string DefaultServerIP = "15.204.243.36";

    /// <summary>PlayerPrefs key that points the GAME connection somewhere else than auth.</summary>
    public const string GameServerOverrideKey = "gameServerIP";

    // Reads PlayerPrefs override first; falls back to compiled constant.
    public static string ServerIP =>
        PlayerPrefs.GetString("serverIP", DefaultServerIP);

    public static string AuthBaseUrl => $"http://{ServerIP}:3000";

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
