#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// AbilityVFXAssignerBuilder — BCE/Heroes/Assign Ability VFX
///
/// Scans the brbmuffins VFX packs already in the project and assigns
/// castVFX + hitVFX prefabs to every AbilityDef in all 5 class spellbooks.
///
/// Safe to re-run — skips entries that already match. Run after importing
/// new VFX prefabs or adding abilities.
///
/// Fantasy Pack (dark runes/plasma/lightning) takes priority over Magic Pack
/// when both contain a same-named prefab.
/// </summary>
public static class AbilityVFXAssignerBuilder
{
    // Search order matters: Fantasy Pack first so "Magic circle" resolves to
    // the darker runic version rather than the Magic Pack cartoon one.
    static readonly string[] SearchFolders =
    {
        "Assets/brbmuffins Dark Arts",
        "Assets/brbmuffins Moves/brbmuffins Human Animations/Unity Demo Scenes",
        "Assets/brbmuffins Studio/brbmuffins Magic Pack",
        "Assets/brbmuffins VFX",
        "Assets/brbmuffins FX",
        "Assets/brbmuffins Trails",
    };

    static readonly string[] HeroPrefabs =
    {
        "Assets/Game/Prefabs/Warden.prefab",
        "Assets/Game/Prefabs/Ironclad.prefab",
        "Assets/Game/Prefabs/Shadowblade.prefab",
        "Assets/Game/Prefabs/Cleric.prefab",
        "Assets/Game/Prefabs/Arcanist.prefab",
    };

    // ── VFX map ───────────────────────────────────────────────────────────────
    // Key   = abilityName (must match AbilityDef.abilityName exactly)
    // Value = (castVFX prefab name, hitVFX prefab name)
    // Use null to leave a slot unchanged.
    static readonly Dictionary<string, (string cast, string hit)> VfxMap =
        new Dictionary<string, (string, string)>
    {
        // ── Arcanist ─────────────────────────────────────────────────────────
        { "Void Bolt",          ("Plazma sphere",                   "Plexus AoE") },
        { "Storm Lash",         ("Human_SpellAura_Lightning",       "Electro slash") },
        { "Ember Surge",        ("Human_SpellAura_Fire",            "Human_Spell_Fireball") },
        { "Mind Spike",         ("Dard magic shoot",                "Electro hit") },
        { "Binding Wave",       ("Magic circle",                    "Red energy explosion") },
        { "Arcane Ward",        ("Human_Spell_Shield",              null) },
        { "Conjurer's Surge",   ("Human_SpellAura_Lightning",       "Human_Spell_Shockwave_Explosion") },
        { "Forked Lightning",   ("Human_SpellAura_Lightning",       "Lightning attack") },
        { "Collapsing Void",    ("Death magic circle",              "Explosion") },
        { "Void Maw",           ("Death magic circle",              "Plexus AoE") },
        { "Arcane Step",        ("Teleport",                        "Glowing orbs") },

        // ── Warden (Engineer) ─────────────────────────────────────────────────
        { "Runic Sentinel",     ("Magic circle",                    "Electro hit") },
        { "Runic Snare",        ("Magic circle",                    "Ground AOE explosion") },
        { "Rune Chain",         ("Lightning strike skill",          "Electro slash") },
        { "Mending Circle",     ("Human_SpellAura_Heal",            "Healing circle") },

        // ── Ironclad (Guardian) ───────────────────────────────────────────────
        { "Counter Blow",       ("Charge slash red",                "AoE slash orange") },
        { "Gravity Slam",       ("Human_SpellAura_Shockwave_Ground","Ground AOE explosion") },
        { "Shieldwall Charge",  ("Charge slash blue",               "Explosion") },
        { "Stalwart Stance",    ("Human_Spell_Shield",              null) },
        { "Iron Rampart",       ("Mana wall",                       null) },

        // ── Shadowblade ───────────────────────────────────────────────────────
        { "Shadow Veil",        ("Smoke vortex",                    null) },
        { "Silence Ward",       ("Mana wall",                       "Freeze circle") },
        { "Dark Harvest",       ("Death magic circle",              "Smoke AOE explosion") },

        // ── Cleric ────────────────────────────────────────────────────────────
        { "Battle Hymn",        ("Human_SpellAura_Heal",            null) },
        { "Spirit Redirect",    ("Magic arrow",                     "Human_SpellAura_Heal") },
        { "Mend",               ("Human_SpellAura_Heal",            "Human_Spell_Heal") },
        { "Soul Bond",          ("Human_SpellAura_Heal",            null) },
        { "Spirit Wisps",       ("Human_SpellAura_Heal",            "Human_Spell_Heal") },
        { "Divine Spark",       ("Human_SpellAura_Heal",            "Human_Spell_Heal") },
        { "Sacred Aegis",       ("Human_Spell_Shield",              null) },
        { "Dispel",             ("Magic circle",                    "Electro hit") },
        { "Temporal Grace",     ("Ice freeze skill",                "Human_Spell_Ice") },
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

            var so          = new SerializedObject(caster);
            var spellbookProp = so.FindProperty("spellbook");
            if (spellbookProp == null || !spellbookProp.isArray) continue;

            string heroName = System.IO.Path.GetFileNameWithoutExtension(prefabPath);

            for (int i = 0; i < spellbookProp.arraySize; i++)
            {
                var elem        = spellbookProp.GetArrayElementAtIndex(i);
                var nameProp    = elem.FindPropertyRelative("abilityName");
                var castVfxProp = elem.FindPropertyRelative("castVFX");
                var hitVfxProp  = elem.FindPropertyRelative("hitVFX");

                if (nameProp == null) continue;
                string abilityName = nameProp.stringValue;

                if (!VfxMap.TryGetValue(abilityName, out var vfx)) continue;

                int changed = 0;
                if (vfx.cast != null)
                    changed += Set(castVfxProp, vfx.cast, lookup,
                        $"{heroName}/{abilityName} castVFX");
                if (vfx.hit != null)
                    changed += Set(hitVfxProp, vfx.hit, lookup,
                        $"{heroName}/{abilityName} hitVFX");

                total += changed;
            }

            so.ApplyModifiedProperties();
            Debug.Log($"[VFXAssigner] Processed {System.IO.Path.GetFileNameWithoutExtension(prefabPath)}");
        }

        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog("Ability VFX",
            $"Done. Assigned {total} VFX reference(s) across all class prefabs.", "OK");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    static Dictionary<string, string> BuildPrefabLookup()
    {
        var lookup = new Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase);
        string[] guids = AssetDatabase.FindAssets("t:Prefab", SearchFolders);

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string name = System.IO.Path.GetFileNameWithoutExtension(path);
            // First match wins — SearchFolders order puts Fantasy Pack first so
            // "Magic circle" resolves to the dark runic one, not the cartoon one.
            if (!lookup.ContainsKey(name))
                lookup[name] = path;
        }
        return lookup;
    }

    static int Set(SerializedProperty prop, string prefabName,
        Dictionary<string, string> lookup, string label)
    {
        if (prop == null) return 0;

        if (!lookup.TryGetValue(prefabName, out string path))
        {
            Debug.LogWarning($"[VFXAssigner] '{prefabName}' not found — skipping {label}");
            return 0;
        }

        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab == null) return 0;
        if (prop.objectReferenceValue == prefab) return 0; // already correct

        prop.objectReferenceValue = prefab;
        Debug.Log($"[VFXAssigner] {label}  →  {path}");
        return 1;
    }

    [MenuItem("BCE/Heroes/Assign Ability VFX", true)]
    static bool Validate() => !Application.isPlaying;
}
#endif
