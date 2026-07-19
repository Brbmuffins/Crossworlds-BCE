#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// AbilityVFXAssignerBuilder — BCE/Heroes/Assign Ability VFX
///
/// Assigns castVFX, hitVFX, and deployablePrefab (+ turretPrefab for Runic
/// Sentinel) to every AbilityDef in all 5 class spellbooks.
///
/// Prefab lookup searches brbmuffins packs in priority order so the dark
/// Fantasy Pack "Magic circle" beats the cartoon Magic Pack one. FBX models
/// in the Engineer/Turret folder are also included for the turret slot.
///
/// Safe to re-run — only writes fields that differ from current value.
/// Run BCE → Heroes → Assign Ability VFX after any new VFX import.
/// </summary>
public static class AbilityVFXAssignerBuilder
{
    static readonly string[] SearchFolders =
    {
        "Assets/brbmuffins Dark Arts",
        "Assets/brbmuffins Moves/brbmuffins Human Animations/Unity Demo Scenes",
        "Assets/brbmuffins Studio/brbmuffins Magic Pack",
        "Assets/brbmuffins VFX",
        "Assets/brbmuffins FX",
        "Assets/brbmuffins Trails",
        "Assets/Game/Characters/Engineer/Turret",
        "Assets/Game/Networking",       // deployable prefabs incl. IceGuardianTurret
    };

    static readonly string[] HeroPrefabs =
    {
        "Assets/Game/Game_Prefabs/Warden.prefab",
        "Assets/Game/Game_Prefabs/Ironclad.prefab",
        "Assets/Game/Game_Prefabs/Shadowblade.prefab",
        "Assets/Game/Game_Prefabs/Cleric.prefab",
        "Assets/Game/Game_Prefabs/Arcanist.prefab",
    };

    // ── Main VFX map ──────────────────────────────────────────────────────────
    // (castVFX, hitVFX, deployablePrefab) — null = leave slot unchanged
    static readonly Dictionary<string, (string cast, string hit, string deploy)> VfxMap =
        new Dictionary<string, (string, string, string)>
    {
        // ── Arcanist ─────────────────────────────────────────────────────────
        { "Void Bolt",          ("Plazma sphere",                    "Plexus AoE",                null) },
        { "Storm Lash",         ("Human_SpellAura_Lightning",        "Electro slash",             null) },
        { "Ember Surge",        ("Human_SpellAura_Fire",             "Human_Spell_Fireball",      null) },
        { "Mind Spike",         ("Dard magic shoot",                 "Electro hit",               null) },
        { "Binding Wave",       ("Magic circle",                     "Red energy explosion",      "Magic circle") },
        { "Arcane Ward",        ("Human_Spell_Shield",               null,                        null) },
        { "Conjurer's Surge",   ("Human_SpellAura_Lightning",        "Human_Spell_Shockwave_Explosion", null) },
        { "Forked Lightning",   ("Human_SpellAura_Lightning",        "Lightning attack",          null) },
        { "Collapsing Void",    ("Death magic circle",               "Explosion",                 "Death magic circle") },
        { "Void Maw",           ("Death magic circle",               "Plexus AoE",                "Death magic circle") },
        { "Arcane Step",        ("Teleport",                         "Glowing orbs",              null) },

        // ── Warden ───────────────────────────────────────────────────────────
        // Runic Sentinel turretPrefab is handled separately in TurretMap below
        { "Runic Sentinel",     ("Magic circle",                     "Electro hit",               "Magic circle") },
        { "Runic Snare",        ("Magic circle",                     "Ground AOE explosion",      "Magic circle") },
        { "Rune Chain",         ("Lightning strike skill",           "Electro slash",             null) },
        { "Mending Circle",     ("Human_SpellAura_Heal",             "Healing circle",            "Healing circle") },

        // ── Ironclad ─────────────────────────────────────────────────────────
        { "Counter Blow",       ("Charge slash red",                 "AoE slash orange",          null) },
        { "Gravity Slam",       ("Human_SpellAura_Shockwave_Ground", "Ground AOE explosion",      "Ground spikes") },
        { "Shieldwall Charge",  ("Charge slash blue",                "Explosion",                 null) },
        { "Stalwart Stance",    ("Human_Spell_Shield",               null,                        null) },
        { "Iron Rampart",       ("Mana wall",                        null,                        "Mana wall") },

        // ── Shadowblade ───────────────────────────────────────────────────────
        { "Shadow Veil",        ("Smoke vortex",                     null,                        null) },
        { "Silence Ward",       ("Mana wall",                        "Freeze circle",             "Freeze circle") },
        { "Dark Harvest",       ("Death magic circle",               "Smoke AOE explosion",       "Death magic circle") },

        // ── Cleric ────────────────────────────────────────────────────────────
        { "Battle Hymn",        ("Human_SpellAura_Heal",             null,                        "Healing circle") },
        { "Spirit Redirect",    ("Magic arrow",                      "Human_SpellAura_Heal",      null) },
        { "Mend",               ("Human_SpellAura_Heal",             "Human_Spell_Heal",          null) },
        { "Soul Bond",          ("Human_SpellAura_Heal",             null,                        "Magic circle") },
        { "Spirit Wisps",       ("Human_SpellAura_Heal",             "Human_Spell_Heal",          null) },
        { "Divine Spark",       ("Human_SpellAura_Heal",             "Human_Spell_Heal",          null) },
        { "Sacred Aegis",       ("Human_Spell_Shield",               null,                        null) },
        { "Dispel",             ("Magic circle",                     "Electro hit",               null) },
        { "Temporal Grace",     ("Ice freeze skill",                 "Human_Spell_Ice",           "Freeze circle") },
    };

    // ── Turret map ────────────────────────────────────────────────────────────
    // Abilities with spawnTurret=true get a turretPrefab assigned here by name.
    // Run BCE/Setup/4g first to create IceGuardianTurret.prefab in Game/Networking/.
    static readonly Dictionary<string, string> TurretMap =
        new Dictionary<string, string>
    {
        { "Runic Sentinel", "IceGuardianTurret" },  // BCE/Setup/4g creates this
    };

    // ── Entry point ───────────────────────────────────────────────────────────

    [MenuItem("BCE/Heroes/Assign Ability VFX")]
    static void AssignVFX()
    {
        var lookup = BuildPrefabLookup();
        if (lookup.Count == 0)
        {
            EditorUtility.DisplayDialog("Ability VFX",
                "No prefabs found in the brbmuffins folders.\n" +
                "Make sure the VFX packs are imported.", "OK");
            return;
        }

        int total = 0;

        foreach (string prefabPath in HeroPrefabs)
        {
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (go == null)
            {
                Debug.LogWarning($"[VFXAssigner] Hero prefab not found: {prefabPath}");
                continue;
            }

            using var scope = new PrefabUtility.EditPrefabContentsScope(prefabPath);
            var caster = scope.prefabContentsRoot.GetComponent<AbilityCaster>();
            if (caster == null)
            {
                Debug.LogWarning($"[VFXAssigner] No AbilityCaster on {prefabPath}");
                continue;
            }

            var so            = new SerializedObject(caster);
            var spellbookProp = so.FindProperty("spellbook");
            if (spellbookProp == null || !spellbookProp.isArray) continue;

            string heroName = System.IO.Path.GetFileNameWithoutExtension(prefabPath);

            for (int i = 0; i < spellbookProp.arraySize; i++)
            {
                var elem           = spellbookProp.GetArrayElementAtIndex(i);
                var nameProp       = elem.FindPropertyRelative("abilityName");
                var castVfxProp    = elem.FindPropertyRelative("castVFX");
                var hitVfxProp     = elem.FindPropertyRelative("hitVFX");
                var deployProp     = elem.FindPropertyRelative("deployablePrefab");
                var turretProp     = elem.FindPropertyRelative("turretPrefab");

                if (nameProp == null) continue;
                string abilityName = nameProp.stringValue;
                int changed = 0;

                // Cast + hit + deployable
                if (VfxMap.TryGetValue(abilityName, out var vfx))
                {
                    if (vfx.cast   != null) changed += Set(castVfxProp,  vfx.cast,   lookup, $"{heroName}/{abilityName} castVFX");
                    if (vfx.hit    != null) changed += Set(hitVfxProp,   vfx.hit,    lookup, $"{heroName}/{abilityName} hitVFX");
                    if (vfx.deploy != null) changed += Set(deployProp,    vfx.deploy, lookup, $"{heroName}/{abilityName} deployablePrefab");
                }

                // Turret model
                if (TurretMap.TryGetValue(abilityName, out string turretName))
                    changed += Set(turretProp, turretName, lookup, $"{heroName}/{abilityName} turretPrefab");

                total += changed;
            }

            so.ApplyModifiedProperties();
            Debug.Log($"[VFXAssigner] Processed {heroName}");
        }

        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog("Ability VFX",
            $"Done. Assigned {total} reference(s) across all class prefabs.\n\n" +
            "Runic Sentinel turretPrefab → IceGuardianTurret (networked deployable).\n" +
            "Run BCE/Setup/4g first if the prefab does not exist yet.", "OK");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    static Dictionary<string, string> BuildPrefabLookup()
    {
        var lookup = new Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase);

        // Prefabs — first match wins (SearchFolders order = priority)
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", SearchFolders);
        foreach (string guid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string name = System.IO.Path.GetFileNameWithoutExtension(path);
            if (!lookup.ContainsKey(name)) lookup[name] = path;
        }

        // Also include FBX models from the turret folder so turretPrefab can
        // reference the raw mesh import as a visual placeholder.
        string[] modelGuids = AssetDatabase.FindAssets("t:Model",
            new[] { "Assets/Game/Characters/Engineer/Turret" });
        foreach (string guid in modelGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string name = System.IO.Path.GetFileNameWithoutExtension(path);
            if (!lookup.ContainsKey(name)) lookup[name] = path;
        }

        return lookup;
    }

    static int Set(SerializedProperty prop, string assetName,
        Dictionary<string, string> lookup, string label)
    {
        if (prop == null) return 0;

        if (!lookup.TryGetValue(assetName, out string path))
        {
            Debug.LogWarning($"[VFXAssigner] '{assetName}' not found — skipping {label}");
            return 0;
        }

        var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (asset == null) return 0;
        if (prop.objectReferenceValue == asset) return 0;

        prop.objectReferenceValue = asset;
        Debug.Log($"[VFXAssigner] {label}  →  {path}");
        return 1;
    }

    [MenuItem("BCE/Heroes/Assign Ability VFX", true)]
    static bool Validate() => !Application.isPlaying;
}
#endif
