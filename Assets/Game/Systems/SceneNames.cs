// ═══════════════════════════════════════════════════════════════════════════
//  SceneNames — canonical scene names and asset paths.
//  Use these everywhere instead of string literals so a scene rename is a
//  one-line change here, not a grep-and-pray across the codebase.
// ═══════════════════════════════════════════════════════════════════════════
public static class SceneNames
{
    // ── Scene names (as they appear in Build Settings) ────────────────────
    public const string Login           = "LoginScene";
    public const string CharacterSelect = "CharacterSelect";
    public const string Hub             = "Hub";
    public const string ArenaCopper     = "Arena_Copper";

    // ── Full asset paths (used by editor scripts and Mirror scene fields) ─
    public const string LoginPath           = "Assets/Game/Scenes/LoginScene.unity";
    public const string CharacterSelectPath = "Assets/Game/Scenes/CharacterSelect.unity";
    public const string HubPath             = "Assets/Game/Scenes/Hub.unity";
    public const string ArenaCopperPath     = "Assets/Game/Scenes/Arena_Copper.unity";
}
