using System.IO;
using UnityEngine;
using UnityEditor;

// BCE ▶ Assign Spell Icons
public static class SpellIconAssigner
{
    const string IconFolder = "Assets/Game/UI/CharacterSelect/AbilityIcons";

    static readonly string[] HeroPrefabs = new[]
    {
        "Assets/Game/Game_Prefabs/Warden.prefab",
        "Assets/Game/Game_Prefabs/Ironclad.prefab",
        "Assets/Game/Game_Prefabs/Shadowblade.prefab",
        "Assets/Game/Game_Prefabs/Cleric.prefab",
        "Assets/Game/Game_Prefabs/Arcanist.prefab",
    };

    [MenuItem("BCE/Assign Spell Icons")]
    public static void AssignIcons()
    {
        // Step 1: force all PNGs to Sprite import type
        int reimported = EnsureSpritesImported();
        Debug.Log($"[SpellIconAssigner] Re-imported {reimported} textures as Sprite.");
        if (reimported > 0) { AssetDatabase.SaveAssets(); AssetDatabase.Refresh(); }

        // Step 2: spot-test one known sprite to confirm loading works at all
        string testPath = IconFolder + "/arcane-step.png";
        var testSprite = AssetDatabase.LoadAssetAtPath<Sprite>(testPath);
        Debug.Log($"[SpellIconAssigner] Test load '{testPath}': {(testSprite != null ? "OK" : "NULL")}");

        // Step 3: assign to prefabs
        int assigned = 0, skipped = 0;

        foreach (string prefabPath in HeroPrefabs)
        {
            if (!File.Exists(prefabPath))
            {
                Debug.LogWarning($"[SpellIconAssigner] File not found: {prefabPath}");
                continue;
            }

            using var scope = new PrefabUtility.EditPrefabContentsScope(prefabPath);
            var root = scope.prefabContentsRoot;
            if (root == null) { Debug.LogWarning($"[SpellIconAssigner] EditPrefabContentsScope gave null root for {prefabPath}"); continue; }

            var caster = root.GetComponentInChildren<AbilityCaster>(true);
            if (caster == null) { Debug.LogWarning($"[SpellIconAssigner] No AbilityCaster on {prefabPath}"); continue; }

            var book = caster.spellbook;
            if (book == null) { Debug.LogWarning($"[SpellIconAssigner] spellbook is null on {prefabPath}"); continue; }

            Debug.Log($"[SpellIconAssigner] {prefabPath}: {book.Length} spellbook entries");

            bool dirty = false;
            foreach (var ab in book)
            {
                if (ab == null) continue;
                var sprite = FindSprite(ab.abilityName);
                if (sprite == null) { skipped++; Debug.Log($"  no icon: '{ab.abilityName}'"); continue; }
                if (ab.icon == sprite) continue;
                ab.icon = sprite;
                assigned++;
                dirty = true;
                Debug.Log($"  assigned: '{ab.abilityName}' → {sprite.name}");
            }
            if (dirty) EditorUtility.SetDirty(root);
        }

        AssetDatabase.SaveAssets();
        string msg = $"Assigned {assigned} icons.\n{skipped} spells had no matching PNG.";
        Debug.Log($"[SpellIconAssigner] Done. {msg}");
        EditorUtility.DisplayDialog("Spell Icons", msg, "OK");
    }

    static int EnsureSpritesImported()
    {
        int count = 0;
        // Use "" to find all assets; t:Texture2D misses assets already typed as Sprite in some Unity versions.
        string[] guids = AssetDatabase.FindAssets("", new[] { IconFolder });
        Debug.Log($"[SpellIconAssigner] Found {guids.Length} assets in {IconFolder}");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (!path.EndsWith(".png", System.StringComparison.OrdinalIgnoreCase)) continue;
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) continue;
            // Both type AND mode must be correct — Multiple-mode sprites have no sub-assets unless sliced.
            bool needsFix = importer.textureType != TextureImporterType.Sprite
                         || importer.spriteImportMode != SpriteImportMode.Single;
            if (!needsFix) continue;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            count++;
        }
        return count;
    }

    // Unity AssetDatabase requires forward slashes on all platforms.
    static string AdbPath(string folder, string file) =>
        folder.TrimEnd('/', '\\') + "/" + file;

    static Sprite FindSprite(string abilityName)
    {
        // Exact match (handles "Ice Spikes.png")
        var s = TryLoad(AdbPath(IconFolder, abilityName + ".png"));
        if (s != null) return s;

        // Kebab-case: "Arcane Step"→"arcane-step"  "Conjurer's Surge"→"conjurers-surge"
        string kebab = abilityName.ToLowerInvariant().Replace("'", "").Replace(" ", "-");
        s = TryLoad(AdbPath(IconFolder, kebab + ".png"));
        if (s != null) return s;

        // Thematic fallback for spells with no dedicated PNG yet
        string fallback = FallbackName(abilityName);
        if (fallback != null)
            return TryLoad(AdbPath(IconFolder, fallback + ".png"));

        return null;
    }

    static Sprite TryLoad(string path)
    {
        var s = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (s != null) return s;
        // SpriteImportMode.Multiple stores sprites as sub-assets; main asset is Texture2D.
        foreach (var obj in AssetDatabase.LoadAllAssetsAtPath(path))
            if (obj is Sprite sp) return sp;
        return null;
    }

    static string FallbackName(string abilityName) => abilityName switch
    {
        "Healing Cone"       => "mending-circle",
        "Mending Beam"       => "soul-bond",
        "Conflagration Cone" => "ember-surge",
        "Ember Beam"         => "ember-surge",
        "Meteor Shower"      => "collapsing-void",
        "Dark Mark"          => "silence-ward",
        "Fan of Blades"      => "dark-harvest",
        "Ether Lance"        => "void-bolt",
        // Arcanist new
        "Fireball"           => "ember-surge",
        "Chain Lightning"    => "storm-lash",
        "Frost Nova"         => "ice-spikes",
        // Warden new
        "Thorn Volley"       => "dark-harvest",
        "Earth Surge"        => "shatter-ground",
        "Vine Grasp"         => "silence-ward",
        // Ironclad new
        "Hammer Strike"      => "shatter-ground",
        "War Cry"            => "mending-circle",
        "Juggernaut Rush"    => "ember-surge",
        // Shadowblade new
        "Blade Flurry"       => "dark-harvest",
        "Poison Cloud"       => "void-bolt",
        "Death Strike"       => "collapsing-void",
        // Cleric new
        "Holy Bolt"          => "soul-bond",
        "Divine Shield"      => "mending-circle",
        "Smite"              => "soul-bond",
        _                    => null,
    };
}
