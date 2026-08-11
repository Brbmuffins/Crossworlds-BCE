#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

public static class SpellIconImporter
{
    const string IconDir = "Assets/Game/UI/Icons/Spells";
    const string PrefabDir = "Assets/Game/Game_Prefabs";

    [MenuItem("BCE/Generate Placeholder Icons")]
    public static void GenerateIcons()
    {
        Debug.Log("[SpellIconImporter] Starting placeholder icon generation...");

        // Ensure target directory exists
        if (!Directory.Exists(IconDir))
        {
            Directory.CreateDirectory(IconDir);
            AssetDatabase.Refresh();
        }

        string[] classes = { "Marauder", "Ironclad", "Shadowblade", "Cleric", "Arcanist", "Necromancer" };
        Color[] classColors = {
            new Color(0.18f, 0.49f, 0.20f), // Marauder: Forest Green #2E7D32
            new Color(0.90f, 0.32f, 0.00f), // Ironclad: Steel Orange #E65100
            new Color(0.29f, 0.08f, 0.55f), // Shadowblade: Deep Purple #4A148C
            new Color(0.00f, 0.41f, 0.36f), // Cleric: Teal Gold #00695C
            new Color(0.10f, 0.14f, 0.49f), // Arcanist: Arcane Blue #1A237E
            new Color(0.18f, 0.38f, 0.12f)  // Necromancer: Necrotic Green
        };

        for (int c = 0; iClassIndex(c, out string className, out Color classColor); c++)
        {
            string prefabPath = $"{PrefabDir}/{className}.prefab";
            var prefabObj = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefabObj == null)
            {
                Debug.LogWarning($"[SpellIconImporter] Prefab not found at {prefabPath}");
                continue;
            }

            using (var scope = new PrefabUtility.EditPrefabContentsScope(prefabPath))
            {
                var caster = scope.prefabContentsRoot.GetComponent<AbilityCaster>();
                if (caster == null)
                    caster = scope.prefabContentsRoot.GetComponentInChildren<AbilityCaster>();

                if (caster == null)
                {
                    Debug.LogWarning($"[SpellIconImporter] No AbilityCaster found on {prefabPath}");
                    continue;
                }

                if (caster.spellbook == null || caster.spellbook.Length == 0)
                {
                    Debug.LogWarning($"[SpellIconImporter] Spellbook is empty on {prefabPath}");
                    continue;
                }

                bool dirty = false;
                foreach (var spell in caster.spellbook)
                {
                    if (spell == null) continue;

                    string safeName = CleanFileName(spell.abilityName);
                    string iconPath = $"{IconDir}/{safeName}_icon.png";

                    // Create Texture if it doesn't exist
                    if (!File.Exists(iconPath))
                    {
                        CreatePlaceholderIconTexture(iconPath, spell.abilityName, classColor);
                    }

                    // Ensure imported as Sprite
                    ConfigureAsSprite(iconPath);

                    // Load Sprite and assign
                    var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(iconPath);
                    if (sprite != null)
                    {
                        spell.icon = sprite;
                        dirty = true;
                    }
                }

                if (dirty)
                {
                    Debug.Log($"[SpellIconImporter] Assigned placeholder icons to {className} spellbook.");
                }
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[SpellIconImporter] ✓ Finished icon generation & assignment!");
    }

    private static bool iClassIndex(int index, out string name, out Color color)
    {
        string[] classes = { "Marauder", "Ironclad", "Shadowblade", "Cleric", "Arcanist", "Necromancer" };
        Color[] classColors = {
            new Color(0.18f, 0.49f, 0.20f), // Marauder: Forest Green #2E7D32
            new Color(0.90f, 0.32f, 0.00f), // Ironclad: Steel Orange #E65100
            new Color(0.29f, 0.08f, 0.55f), // Shadowblade: Deep Purple #4A148C
            new Color(0.00f, 0.41f, 0.36f), // Cleric: Teal Gold #00695C
            new Color(0.10f, 0.14f, 0.49f), // Arcanist: Arcane Blue #1A237E
            new Color(0.18f, 0.38f, 0.12f)  // Necromancer: Necrotic Green
        };

        if (index >= 0 && index < classes.Length)
        {
            name = classes[index];
            color = classColors[index];
            return true;
        }

        name = "";
        color = Color.white;
        return false;
    }

    private static string CleanFileName(string name)
    {
        if (string.IsNullOrEmpty(name)) return "Unnamed";
        foreach (char c in Path.GetInvalidFileNameChars())
        {
            name = name.Replace(c, '_');
        }
        return name.Replace(' ', '_');
    }

    private static void CreatePlaceholderIconTexture(string path, string label, Color bgColor)
    {
        const int size = 64;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);

        // Simple gradient/bordered solid block
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                bool isBorder = (x < 3 || x >= size - 3 || y < 3 || y >= size - 3);
                if (isBorder)
                {
                    tex.SetPixel(x, y, Color.white);
                }
                else
                {
                    float factor = 0.7f + 0.3f * ((float)y / size); // soft bottom-to-top gradient
                    Color finalCol = new Color(bgColor.r * factor, bgColor.g * factor, bgColor.b * factor, 1.0f);
                    tex.SetPixel(x, y, finalCol);
                }
            }
        }

        tex.Apply();
        byte[] bytes = tex.EncodeToPNG();
        File.WriteAllBytes(path, bytes);
        Object.DestroyImmediate(tex);

        AssetDatabase.ImportAsset(path);
    }

    private static void ConfigureAsSprite(string path)
    {
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null && importer.textureType != TextureImporterType.Sprite)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Bilinear;
            EditorUtility.SetDirty(importer);
            importer.SaveAndReimport();
        }
    }
}
#endif
