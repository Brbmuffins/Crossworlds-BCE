#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// BCE/Setup/GO — one click for every prefab-level fix that online play needs.
///
/// Why this exists: the individual fixers (4n, 4o, 4d) all existed but had never been run,
/// and nothing recorded which ones mattered or in what order. Each one is idempotent, so
/// this is safe to re-run after any merge — worth doing whenever prefabs come in from
/// someone else's branch.
///
/// What it runs, in order:
///   4n — NetworkTransform + NetworkAnimator on the 5 heroes + 3 base enemies.
///        Without it they spawn for other clients and stand frozen forever.
///   4o — NetworkIdentity + movement sync on the hand-placed world mobs
///        (ogre, cyclops, Gunda, training dummy).
///   4d — NetworkIdentity on ability deployables/turrets + registers them in
///        RodNetworkManager, so other players can see spell objects.
///        NOTE: as of 2026-07-15 this finds NOTHING — no prefab carries a deployable
///        behaviour. The AbilityCaster deployable slots are null and the few
///        ability.deployablePrefab fallbacks point at raw brbmuffins VFX prefabs with
///        no scripts and no identity. 4d is correct but has nothing to act on until the
///        deployable prefabs are actually built. See ROADMAP 2.7.
///
/// Every step here touches prefab ASSETS or LoginScene only — none of them depend on
/// which scene you have open, so this cannot eat unsaved scene work. The boss (6) is
/// deliberately NOT included: it builds into the ACTIVE scene and must run with Darkwood
/// open, and 6b must run after 6 has created the NullShard prefab.
/// </summary>
public static class MultiplayerSetupOrchestrator
{
    static readonly (string menu, string what)[] Steps =
    {
        ("BCE/Setup/4n ▶ Add Movement Sync (Players + Enemies)", "Heroes + enemies replicate movement"),
        ("BCE/Setup/4o ▶ Network World Mobs (Ogre, Cyclops, Gunda, Dummy)", "Hand-placed world mobs become networked enemies"),
        ("BCE/Setup/4d ▶ Network Deployables + Turret",          "Spell deployables replicate"),
    };

    [MenuItem("BCE/Setup/GO ▶ Make Multiplayer Work (runs 4n + 4o + 4d)", priority = 0)]
    public static void RunAll()
    {
        if (!EditorUtility.DisplayDialog("Make Multiplayer Work",
                "Runs, in order:\n\n" +
                "  4n — hero + enemy movement sync\n" +
                "  4o — networked ogre\n" +
                "  4d — networked spell deployables\n\n" +
                "Each step shows its own result dialog. All are idempotent.\n" +
                "4d will switch to LoginScene to register prefabs — save your work first.\n\n" +
                "Continue?",
                "Run All", "Cancel"))
            return;

        var results = new List<string>();
        foreach (var (menu, what) in Steps)
        {
            bool ok = EditorApplication.ExecuteMenuItem(menu);
            results.Add($"  {(ok ? "✓" : "✗")} {what}{(ok ? "" : "  — MENU NOT FOUND, run it by hand")}");
            if (!ok) Debug.LogWarning($"[BCE/GO] Menu item not found: {menu}");
        }

        EditorUtility.DisplayDialog("Multiplayer Setup — Prefab Steps Done",
            string.Join("\n", results) +
            "\n\nSTILL TO DO (these need a scene open — cannot be automated):\n\n" +
            "1. Open Assets/Game/Scenes/Darkwood.unity\n" +
            "2. BCE/Setup/6  — create the Null Architect boss\n" +
            "3. Move the boss where you want it, then SAVE (Ctrl+S)\n" +
            "   ← this also bakes the ogre's sceneId. Do not skip it.\n" +
            "4. BCE/Setup/6b — register NullShard + WorldItem for spawning\n" +
            "5. Commit + push to main — CI builds and deploys the server for you\n\n" +
            "Then launch two clients and log in.",
            "Got it");
    }
}
#endif
