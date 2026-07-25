#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// AbilityIconAssignerBuilder — BCE/Heroes/Assign Ability Icons
///
/// Loads all ability icon PNGs from Assets/Game/UI/CharacterSelect/AbilityIcons/
/// and assigns them to matching AbilityDef.icon fields on all 5 class prefabs.
///
/// Matching: ability name is lowercased, apostrophes stripped, spaces → hyphens.
///   "Mending Circle" → "mending-circle"   → mending-circle.png
///   "Conjurer's Surge" → "conjurers-surge" → conjurers-surge.png
///
/// Run once after importing new icon PNGs. Safe to re-run — only sets icons
/// that are currently null or changed.
/// </summary>
public static class AbilityIconAssignerBuilder
{
    const string IconFolder = "Assets/Game/UI/CharacterSelect/AbilityIcons";

    static readonly string[] PrefabPaths =
    {
        "Assets/Game/Game_Prefabs/Marauder.prefab",
        "Assets/Game/Prefabs/Ironclad.prefab",
        "Assets/Game/Prefabs/Shadowblade.prefab",
        "Assets/Game/Prefabs/Cleric.prefab",
        "Assets/Game/Prefabs/Arcanist.prefab",
    };

    [MenuItem("BCE/Heroes/Assign Ability Icons")]
    static void AssignIcons()
    {
        // ── Build name → sprite lookup ────────────────────────────────────────
        var lookup = new Dictionary<string, Sprite>();
        string[] guids = AssetDatabase.FindAssets("t:Sprite", new[] { IconFolder });
        foreach (string guid in guids)
        {
            string path   = AssetDatabase.GUIDToAssetPath(guid);
            var    sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null) continue;
            // Key = filename without extension, already hyphen-cased
            string key = System.IO.Path.GetFileNameWithoutExtension(path).ToLowerInvariant();
            lookup[key] = sprite;
        }

        if (lookup.Count == 0)
        {
            EditorUtility.DisplayDialog("Ability Icons",
                $"No sprites found in:\n{IconFolder}\n\nMake sure the PNGs are imported as Sprite (Texture Type).", "OK");
            return;
        }

        int total = 0;

        // ── Process each class prefab ─────────────────────────────────────────
        foreach (string prefabPath in PrefabPaths)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
            {
                Debug.LogWarning($"[IconAssigner] Prefab not found: {prefabPath}");
                continue;
            }

            using var scope = new PrefabUtility.EditPrefabContentsScope(prefabPath);
            var root   = scope.prefabContentsRoot;
            var caster = root.GetComponent<AbilityCaster>();
            if (caster == null)
            {
                Debug.LogWarning($"[IconAssigner] No AbilityCaster on {prefabPath}");
                continue;
            }

            var so = new SerializedObject(caster);

            // Check both 'abilities' (equipped bar) and 'spellbook' (full pool)
            AssignIconsToArray(so, "abilities", lookup, ref total);
            AssignIconsToArray(so, "spellbook", lookup, ref total);

            so.ApplyModifiedProperties();
            Debug.Log($"[IconAssigner] Processed {System.IO.Path.GetFileNameWithoutExtension(prefabPath)}");
        }

        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog("Ability Icons",
            $"Done. Assigned {total} icon(s) across all class prefabs.", "OK");
    }

    static void AssignIconsToArray(SerializedObject so, string arrayPropName,
        Dictionary<string, Sprite> lookup, ref int total)
    {
        var arrayProp = so.FindProperty(arrayPropName);
        if (arrayProp == null || !arrayProp.isArray) return;

        for (int i = 0; i < arrayProp.arraySize; i++)
        {
            var elem     = arrayProp.GetArrayElementAtIndex(i);
            var nameProp = elem.FindPropertyRelative("abilityName");
            var iconProp = elem.FindPropertyRelative("icon");
            if (nameProp == null || iconProp == null) continue;

            string key = NormalizeAbilityName(nameProp.stringValue);
            if (!lookup.TryGetValue(key, out Sprite sprite)) continue;

            if (iconProp.objectReferenceValue == sprite) continue; // already set

            iconProp.objectReferenceValue = sprite;
            total++;
            Debug.Log($"[IconAssigner]   {nameProp.stringValue} → {key}.png");
        }
    }

    /// <summary>
    /// "Conjurer's Surge" → "conjurers-surge"
    /// Lower-case, strip apostrophes, spaces → hyphens, trim.
    /// </summary>
    static string NormalizeAbilityName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return "";
        return name.ToLowerInvariant()
                   .Replace("'", "")
                   .Replace("'", "")
                   .Replace(" ", "-")
                   .Trim();
    }

    [MenuItem("BCE/Heroes/Assign Ability Icons", true)]
    static bool Validate() => !Application.isPlaying;
}
#endif
