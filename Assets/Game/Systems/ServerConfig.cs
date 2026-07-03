using UnityEngine;

// ═══════════════════════════════════════════════════════════════════════════
//  ServerConfig — single source of truth for the VPS address.
//  To override at runtime (e.g. LAN testing) set PlayerPrefs key "serverIP"
//  before login; clear it to restore the compiled default.
// ═══════════════════════════════════════════════════════════════════════════
public static class ServerConfig
{
    public const string DefaultServerIP = "15.204.243.36";

    // Reads PlayerPrefs override first; falls back to compiled constant.
    public static string ServerIP =>
        PlayerPrefs.GetString("serverIP", DefaultServerIP);

    public static string AuthBaseUrl  => $"http://{ServerIP}:3000";
    public static string GameServerIP => ServerIP;
}
