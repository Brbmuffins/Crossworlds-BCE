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
    public const string Hub             = "HUB";
    public const string Darkwood        = "Darkwood";
    public const string ArenaCopper     = "Arena_Copper";
    public const string ToujamBasin     = "Toujam Basin";
    public const string AshenWastelands = "Ashen Wastelands";
    public const string GMIsland        = "GM Island";
    public const string VoidDungeon     = "VoidDungeon";   // placeholder — future dungeon build

    // ── Full asset paths (used by editor scripts and Mirror scene fields) ─
    public const string LoginPath           = "Assets/Game/Scenes/LoginScene.unity";
    public const string CharacterSelectPath = "Assets/Game/Scenes/CharacterSelect.unity";
    public const string HubPath             = "Assets/Game/Scenes/HUB.unity";
    public const string DarkwoodPath        = "Assets/Game/Scenes/Darkwood.unity";
    public const string ArenaCopperPath     = "Assets/Game/Scenes/Arena_Copper.unity";
    public const string ToujamBasinPath     = "Assets/Game/Scenes/Toujam Basin.unity";
    public const string AshenWastelandsPath = "Assets/Game/Scenes/Ashen Wastelands.unity";
    public const string GMIslandPath        = "Assets/Game/Scenes/GM Island.unity";
    public const string VoidDungeonPath     = "Assets/Game/Scenes/VoidDungeon.unity";
}
