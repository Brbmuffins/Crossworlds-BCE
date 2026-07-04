#if !UNITY_SERVER
using System.Collections;
using UnityEngine;
using Mirror;

/// <summary>
/// GmConsole — Client-side GM debug console. NEVER compiled into server builds.
///
/// Keyboard shortcut: ` (backtick) to toggle.
/// Commands:
///   /hp <amount>         — set local player HP
///   /kill                — kill local player (test death flow)
///   /give <itemId> <qty> — add item to inventory
///   /wave <n>            — trigger wave N on server (requires host)
///   /scene <name>        — change to named scene (host only)
///   /pos                 — print current world position
///   /fps                 — toggle FPS display
///
/// Copy to: Assets/Game/Systems/GmConsole.cs
/// </summary>
public class GmConsole : MonoBehaviour
{
    // ─── Bootstrap ────────────────────────────────────────────────────────────
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        // Only active in editor or development builds
#if !UNITY_EDITOR && !DEVELOPMENT_BUILD
        return;
#endif
        var go = new GameObject("[GmConsole]");
        go.AddComponent<GmConsole>();
        DontDestroyOnLoad(go);
    }

    // ─── State ────────────────────────────────────────────────────────────────
    bool   _visible  = false;
    string _input    = "";
    string _log      = "";
    bool   _showFps  = false;
    float  _fps      = 0f;
    float  _fpsTimer = 0f;

    readonly GUIStyle _logStyle  = new GUIStyle();
    readonly GUIStyle _inputStyle = new GUIStyle();

    void Start()
    {
        _logStyle.normal.textColor   = new Color(0.8f, 1f, 0.8f);
        _logStyle.fontSize            = 13;
        _inputStyle.normal.textColor  = Color.white;
        _inputStyle.fontSize          = 14;
    }

    // ─── Toggle ───────────────────────────────────────────────────────────────
    void Update()
    {
        // FPS counter
        if (_showFps)
        {
            _fpsTimer += Time.deltaTime;
            if (_fpsTimer >= 0.5f) { _fps = 1f / Time.deltaTime; _fpsTimer = 0f; }
        }

        if (UnityEngine.InputSystem.Keyboard.current.backquoteKey.wasPressedThisFrame)
        {
            _visible = !_visible;
            if (_visible) _input = "";
        }

        if (!_visible) return;

        if (UnityEngine.InputSystem.Keyboard.current.enterKey.wasPressedThisFrame && _input.Trim().Length > 0)
        {
            Execute(_input.Trim());
            _input = "";
        }
    }

    // ─── GUI ──────────────────────────────────────────────────────────────────
    void OnGUI()
    {
        if (_showFps)
            GUI.Label(new Rect(10, 10, 100, 20), $"FPS: {_fps:F0}", _logStyle);

        if (!_visible) return;

        // Background
        GUI.color = new Color(0, 0, 0, 0.85f);
        GUI.DrawTexture(new Rect(0, Screen.height - 220, Screen.width, 220), Texture2D.whiteTexture);
        GUI.color = Color.white;

        // Log
        GUI.Label(new Rect(10, Screen.height - 210, Screen.width - 20, 170), _log, _logStyle);

        // Input
        GUI.SetNextControlName("GmInput");
        _input = GUI.TextField(new Rect(10, Screen.height - 36, Screen.width - 20, 26), _input, _inputStyle);
        GUI.FocusControl("GmInput");
    }

    // ─── Command Parser ───────────────────────────────────────────────────────
    void Execute(string raw)
    {
        Log($"> {raw}");
        string[] parts = raw.Split(' ');
        string cmd = parts[0].ToLower().TrimStart('/');

        switch (cmd)
        {
            case "hp":
                if (parts.Length < 2 || !float.TryParse(parts[1], out float hp))
                { Log("Usage: /hp <amount>"); break; }
                SetLocalHp(hp);
                break;

            case "kill":
                KillLocalPlayer();
                break;

            case "give":
                if (parts.Length < 3 || !int.TryParse(parts[2], out int qty))
                { Log("Usage: /give <itemId> <qty>"); break; }
                GiveItem(parts[1], qty);
                break;

            case "scene":
                if (parts.Length < 2) { Log("Usage: /scene <name>"); break; }
                if (!NetworkServer.active) { Log("Host only"); break; }
                NetworkManager.singleton.ServerChangeScene(parts[1]);
                break;

            case "pos":
                var lp = NetworkClient.localPlayer;
                if (lp == null) { Log("No local player"); break; }
                Log($"Position: {lp.transform.position}");
                break;

            case "fps":
                _showFps = !_showFps;
                Log($"FPS display: {(_showFps ? "ON" : "OFF")}");
                break;

            case "clear":
                _log = "";
                break;

            case "help":
                Log("/hp <n>  /kill  /give <id> <qty>  /scene <n>  /pos  /fps  /clear");
                break;

            default:
                Log($"Unknown command: {cmd}  (type /help)");
                break;
        }
    }

    // ─── Commands ─────────────────────────────────────────────────────────────
    void SetLocalHp(float amount)
    {
        var lp = NetworkClient.localPlayer;
        if (lp == null) { Log("No local player"); return; }
        var h = lp.GetComponent<Health>();
        if (h == null) { Log("No Health component"); return; }
        // Must be server-side — use Command
        var id = lp.GetComponent<PlayerIdentity>();
        if (id != null) id.CmdGmSetHp(amount);
        Log($"Requested HP → {amount}");
    }

    void KillLocalPlayer()
    {
        var lp = NetworkClient.localPlayer;
        if (lp == null) { Log("No local player"); return; }
        var id = lp.GetComponent<PlayerIdentity>();
        if (id != null) id.CmdGmKill();
        Log("Kill command sent");
    }

    void GiveItem(string itemId, int qty)
    {
        var inv = InventoryManager.Instance;
        if (inv == null) { Log("InventoryManager not found"); return; }
        inv.OnItemPickedUp(itemId, qty);
        Log($"Gave {qty}x {itemId}");
    }

    // ─── Log Helper ───────────────────────────────────────────────────────────
    void Log(string msg)
    {
        _log += msg + "\n";
        // Keep last ~10 lines
        var lines = _log.Split('\n');
        if (lines.Length > 12)
            _log = string.Join("\n", lines, lines.Length - 12, 12);
    }
}
#endif
