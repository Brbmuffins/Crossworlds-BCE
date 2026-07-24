using UnityEditor;
using UnityEngine;

// ═══════════════════════════════════════════════════════════════════════════
//  TestingMenu — BCE ▶ Testing
//
//  Multi-client testing needs the game connection pointed at a local host while
//  authentication still goes to the real auth server, because there is no local
//  copy of the auth API. ServerConfig.GameServerOverrideKey does that; this is
//  just a front end so nobody has to hand-edit PlayerPrefs.
//
//  PlayerPrefs are shared between the editor, Multiplayer Play Mode virtual
//  players, and any build on the same machine (same registry key), so setting
//  this affects every local client. That is what you want for this test — but
//  remember to clear it before connecting to the live server again.
// ═══════════════════════════════════════════════════════════════════════════

public static class TestingMenu
{
    [MenuItem("BCE/Testing/Game Server → localhost (127.0.0.1)")]
    public static void UseLocalhost()
    {
        PlayerPrefs.SetString(ServerConfig.GameServerOverrideKey, "127.0.0.1");
        PlayerPrefs.Save();
        Report();
    }

    [MenuItem("BCE/Testing/Game Server → clear override (use live server)")]
    public static void ClearOverride()
    {
        PlayerPrefs.DeleteKey(ServerConfig.GameServerOverrideKey);
        PlayerPrefs.Save();
        Report();
    }

    [MenuItem("BCE/Testing/Show current server routing")]
    public static void Report()
    {
        string message =
            $"Auth API:    {ServerConfig.AuthBaseUrl}\n" +
            $"Game server: {ServerConfig.GameServerIP}\n\n" +
            (ServerConfig.HasGameServerOverride
                ? "Override ACTIVE — the game connects locally while auth still uses the live server.\n" +
                  "Clear it before testing against the live game server."
                : "No override — auth and game both use the same address.");

        Debug.Log("[BCE Testing] Server routing:\n" + message);
        EditorUtility.DisplayDialog("BCE ▶ Server Routing", message, "OK");
    }
}
