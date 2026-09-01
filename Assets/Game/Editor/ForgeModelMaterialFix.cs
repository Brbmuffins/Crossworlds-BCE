#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

/// <summary>Maintains the Hub Forge's material and crafting interaction ownership.</summary>
public static class ForgeModelMaterialFix
{
    const string ForgePrefabPath = "Assets/Game/3D Models/HUB ASSETS/Forge/Hub Forge/prefab_hub_forge.prefab";
    const string ForgeTexturePath = "Assets/Game/3D Models/HUB ASSETS/Forge/Hub Forge/tripo_convert_5f26a899-7e11-4ff8-b38a-abb4646efd48.fbm/tripo_rgb_6aee4885-4940-488f-8987-b835a02d8108.png";
    const string ForgeMaterialPath = "Assets/Game/3D Models/HUB ASSETS/Forge/Hub Forge/forge_blacksmithing_two_sided.mat";

    [DidReloadScripts]
    static void ApplyMissingFix()
    {
        EditorApplication.delayCall += () =>
        {
            if (!IsApplied()) Apply();
        };
    }

    [MenuItem("BCE/Hub Setup/Fix Forge Two-Sided Material")]
    public static void Apply()
    {
        Material material = GetOrCreateMaterial();
        if (material == null) return;

        GameObject root = PrefabUtility.LoadPrefabContents(ForgePrefabPath);
        if (root == null)
        {
            Debug.LogError($"[FORGE MODEL] Missing prefab at {ForgePrefabPath}.");
            return;
        }

        try
        {
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
            {
                Debug.LogError("[FORGE MODEL] prefab_hub_forge contains no renderers.", root);
                return;
            }

            foreach (Renderer renderer in renderers)
            {
                Material[] materials = renderer.sharedMaterials;
                for (int index = 0; index < materials.Length; index++) materials[index] = material;
                renderer.sharedMaterials = materials;
            }

            ForgeNPC forge = root.GetComponent<ForgeNPC>() ?? root.AddComponent<ForgeNPC>();
            forge.professionId = 2;
            forge.npcName = "Craft";
            forge.interactRange = 3.5f;
            forge.promptHeight = 3f;

            PrefabUtility.SaveAsPrefabAsset(root, ForgePrefabPath);
            AssetDatabase.SaveAssets();
            Debug.Log($"[FORGE MODEL] Applied two-sided material and crafting interaction to {renderers.Length} renderer(s).");
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    static Material GetOrCreateMaterial()
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(ForgeTexturePath);
        if (shader == null || texture == null)
        {
            Debug.LogError("[FORGE MODEL] URP Lit shader or Forge base-color texture is missing.");
            return null;
        }

        Material material = AssetDatabase.LoadAssetAtPath<Material>(ForgeMaterialPath);
        if (material == null)
        {
            material = new Material(shader) { name = "forge_blacksmithing_two_sided" };
            AssetDatabase.CreateAsset(material, ForgeMaterialPath);
        }
        else material.shader = shader;

        material.SetTexture("_BaseMap", texture);
        material.SetTexture("_MainTex", texture);
        material.SetColor("_BaseColor", Color.white);
        material.SetColor("_Color", Color.white);
        material.SetFloat("_Surface", 0f);
        material.SetFloat("_Cull", 0f);
        material.SetFloat("_ZWrite", 1f);
        material.doubleSidedGI = true;
        material.enableInstancing = true;
        EditorUtility.SetDirty(material);
        return material;
    }

    static bool IsApplied()
    {
        Material material = AssetDatabase.LoadAssetAtPath<Material>(ForgeMaterialPath);
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(ForgePrefabPath);
        if (material == null || prefab == null || !Mathf.Approximately(material.GetFloat("_Cull"), 0f)) return false;
        if (prefab.GetComponent<ForgeNPC>() == null) return false;

        Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0) return false;
        foreach (Renderer renderer in renderers)
            foreach (Material assigned in renderer.sharedMaterials)
                if (assigned != material) return false;
        return true;
    }
}
#endif
