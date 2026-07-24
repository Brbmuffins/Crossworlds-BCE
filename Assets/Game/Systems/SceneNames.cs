// ═══════════════════════════════════════════════════════════════════════════
//  SceneNames — canonical scene names and asset paths.
//  Use these everywhere instead of string literals so a scene rename is a
//  one-line change here, not a grep-and-pray across the codebase.
// ═══════════════════════════════════════════════════════════════════════════
public static class SceneNames
{
    // ── Scene names (as they appear in Build Settings) ────────────────────
    // Empty scene holding only the NetworkManager. Zones load additively on top
    // of it (ROADMAP 6.3) — it is never a place a player can stand, so it is
    // deliberately absent from Zones[] below.
    public const string Container       = "_Container";
    public const string Login           = "LoginScene";
    public const string CharacterSelect = "CharacterSelect";
    public const string Hub             = "HUB";
    public const string Darkwood        = "Darkwood";
    public const string ArenaCopper     = "Arena_Copper";
    public const string ToujamBasin     = "Toujam Basin";
    public const string AshenWastelands = "Ashen Wastelands";
    public const string GMIsland        = "GM Island";
    public const string VoidDungeon     = "VoidDungeon";      // placeholder — future dungeon build
    public const string GatheringZone   = "Gathering Zone";   // AFK gathering + crafting

    // ── Full asset paths (used by editor scripts and Mirror scene fields) ─
    public const string ContainerPath       = "Assets/Game/Scenes/_Container.unity";
    public const string LoginPath           = "Assets/Game/Scenes/LoginScene.unity";
    public const string CharacterSelectPath = "Assets/Game/Scenes/CharacterSelect.unity";
    public const string HubPath             = "Assets/Game/Scenes/HUB.unity";
    public const string DarkwoodPath        = "Assets/Game/Scenes/Darkwood.unity";
    public const string ArenaCopperPath     = "Assets/Game/Scenes/Arena_Copper.unity";
    public const string ToujamBasinPath     = "Assets/Game/Scenes/Toujam Basin.unity";
    public const string AshenWastelandsPath = "Assets/Game/Scenes/Ashen Wastelands.unity";
    public const string GMIslandPath        = "Assets/Game/Scenes/GM Island.unity";
    public const string VoidDungeonPath     = "Assets/Game/Scenes/VoidDungeon.unity";
    public const string GatheringZonePath   = "Assets/Game/Scenes/Gathering Zone.unity";

    // ── Zone helpers (ROADMAP 6.2) ────────────────────────────────────────
    // A "zone" is a scene a character can be standing in when they log out, and
    // therefore a value that can be persisted to characters.map. LoginScene and
    // CharacterSelect are NOT zones — nobody is ever saved into them.

    /// <summary>Every scene a character can legitimately be saved in.</summary>
    public static readonly string[] Zones =
    {
        Hub, Darkwood, ToujamBasin, AshenWastelands, GMIsland, VoidDungeon,
        GatheringZone, ArenaCopper,
    };

    public static bool IsZone(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName)) return false;
        foreach (string zone in Zones)
            if (string.Equals(zone, sceneName, System.StringComparison.OrdinalIgnoreCase))
                return true;
        return false;
    }

    /// <summary>
    /// Coerces a stored or observed scene name into a zone we can safely spawn into.
    /// Unknown values — including the legacy "GameWorld" literal that every character
    /// row still holds, and anything a tampered client might send — collapse to Hub,
    /// which always has spawn points and a waypoint out.
    /// The server-side allowlist is the authoritative check; this is belt-and-braces
    /// so a bad value can never strand a player in a scene that does not exist.
    /// </summary>
    public static string NormalizeZone(string sceneName)
    {
        return IsZone(sceneName) ? sceneName : Hub;
    }
}
