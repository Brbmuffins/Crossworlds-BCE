#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class WispMobBuilder
{
    const string PrefabDir = "Assets/Game/Prefabs";
    const string PrefabPath = PrefabDir + "/Wisp_Mob.prefab";
    static readonly Color WispColor = new Color(0.48f, 0.9f, 1f, 1f);

    [MenuItem("Rate of Decay/Wisps/Create Wisp Mob Prefab")]
    public static void CreateWispPrefab()
    {
        EnsureDir(PrefabDir);

        GameObject wisp = BuildWispObject();
        PrefabUtility.SaveAsPrefabAsset(wisp, PrefabPath, out bool ok);
        Object.DestroyImmediate(wisp);
        AssetDatabase.Refresh();

        if (ok)
            Debug.Log("[WispMobBuilder] Wisp_Mob.prefab saved to Assets/Game/Prefabs.");
        else
            Debug.LogError("[WispMobBuilder] Failed to save Wisp_Mob.prefab.");
    }

    [MenuItem("Rate of Decay/Wisps/Add 6 Wisps To Open Scene")]
    public static void AddWispsToOpenScene()
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        if (prefab == null)
        {
            CreateWispPrefab();
            prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        }

        if (prefab == null)
        {
            Debug.LogError("[WispMobBuilder] Could not load Wisp_Mob.prefab.");
            return;
        }

        GameObject root = GameObject.Find("WispMobs");
        if (root == null)
            root = new GameObject("WispMobs");

        Vector3[] positions =
        {
            new Vector3(-8f, 2.2f,  6f),
            new Vector3( 7f, 2.6f,  9f),
            new Vector3(12f, 2.0f, -4f),
            new Vector3(-10f, 3.0f, -8f),
            new Vector3( 2f, 2.4f, 13f),
            new Vector3(-2f, 1.8f, -12f),
        };

        for (int i = 0; i < positions.Length; i++)
        {
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            instance.name = $"Wisp_Mob_{i + 1}";
            instance.transform.SetParent(root.transform);
            instance.transform.position = positions[i];

            WispMob wisp = instance.GetComponent<WispMob>();
            if (wisp != null)
            {
                wisp.wanderRadius = 5.5f + i * 0.4f;
                wisp.minHeight = 1.4f;
                wisp.maxHeight = 4.8f;
            }
        }

        Selection.activeGameObject = root;
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("[WispMobBuilder] Added 6 Wisp mobs to the open scene. Save the scene when the placement looks good.");
    }

    [MenuItem("Rate of Decay/Wisps/Fix Existing Wisps Staying Off In Play")]
    public static void FixExistingWisps()
    {
        int fixedCount = 0;
        foreach (WispMob wisp in Object.FindObjectsByType<WispMob>(FindObjectsInactive.Include))
        {
            GameObject go = wisp.gameObject;

            foreach (Mirror.NetworkTransformBase networkTransform in go.GetComponents<Mirror.NetworkTransformBase>())
                Object.DestroyImmediate(networkTransform);

            Mirror.NetworkIdentity networkIdentity = go.GetComponent<Mirror.NetworkIdentity>();
            if (networkIdentity != null)
                Object.DestroyImmediate(networkIdentity);

            if (!go.activeSelf)
                go.SetActive(true);

            RepairWispVisuals(go);
            EditorUtility.SetDirty(go);
            fixedCount++;
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log($"[WispMobBuilder] Fixed {fixedCount} wisp object(s). Save the scene after confirming they stay on in Play.");
    }

    [MenuItem("Rate of Decay/Wisps/Repair Wisp Colors In Open Scene")]
    public static void RepairWispColorsInOpenScene()
    {
        FixExistingWisps();
    }

    static GameObject BuildWispObject()
    {
        GameObject root = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        root.name = "Wisp_Mob";
        root.tag = "Enemy";
        root.transform.localScale = Vector3.one * 0.18f;

        SphereCollider collider = root.GetComponent<SphereCollider>();
        collider.isTrigger = true;
        collider.radius = 1.4f;

        Renderer renderer = root.GetComponent<Renderer>();
        renderer.sharedMaterial = CreateCoreMaterial();
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;

        TrailRenderer trail = root.AddComponent<TrailRenderer>();
        trail.time = 0.75f;
        trail.minVertexDistance = 0.04f;
        trail.widthMultiplier = 0.18f;
        trail.numCornerVertices = 4;
        trail.numCapVertices = 4;
        trail.alignment = LineAlignment.View;
        trail.textureMode = LineTextureMode.Stretch;
        trail.sharedMaterial = CreateTrailMaterial();
        trail.startColor = new Color(WispColor.r, WispColor.g, WispColor.b, 0.8f);
        trail.endColor = new Color(WispColor.r, WispColor.g, WispColor.b, 0f);

        GameObject lightObj = new GameObject("GlowLight");
        lightObj.transform.SetParent(root.transform, false);
        Light light = lightObj.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = WispColor;
        light.intensity = 2.2f;
        light.range = 5.5f;
        light.shadows = LightShadows.None;

        Health health = root.AddComponent<Health>();
        health.maxHealth = 20f;

        WispMob mob = root.AddComponent<WispMob>();
        mob.glowColor = WispColor;
        mob.wanderRadius = 7f;

        return root;
    }

    static void RepairWispVisuals(GameObject go)
    {
        Renderer renderer = go.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.sharedMaterial = CreateCoreMaterial();
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }

        TrailRenderer trail = go.GetComponent<TrailRenderer>();
        if (trail != null)
        {
            trail.sharedMaterial = CreateTrailMaterial();
            trail.startColor = new Color(WispColor.r, WispColor.g, WispColor.b, 0.8f);
            trail.endColor = new Color(WispColor.r, WispColor.g, WispColor.b, 0f);
        }

        Light light = go.GetComponentInChildren<Light>();
        if (light != null)
        {
            light.color = WispColor;
            light.intensity = 2.2f;
            light.range = 5.5f;
        }

        WispMob wisp = go.GetComponent<WispMob>();
        if (wisp != null)
            wisp.glowColor = WispColor;
    }

    static Material CreateCoreMaterial()
    {
        Material material = CloneKnownGoodMaterial();
        if (material == null)
            material = new Material(FindShader("Universal Render Pipeline/Lit", "Universal Render Pipeline/Unlit", "Sprites/Default"));

        material.name = "Wisp_Core_RuntimeMaterial";
        SetMaterialColor(material, WispColor);

        if (material.HasProperty("_EmissionColor"))
        {
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", WispColor * 2.8f);
        }

        return material;
    }

    static Material CreateTrailMaterial()
    {
        Material material = CloneKnownGoodMaterial();
        if (material == null)
            material = new Material(FindShader("Universal Render Pipeline/Particles/Unlit", "Universal Render Pipeline/Unlit", "Sprites/Default"));

        material.name = "Wisp_Trail_RuntimeMaterial";
        SetMaterialColor(material, new Color(WispColor.r, WispColor.g, WispColor.b, 0.8f));
        return material;
    }

    static void SetMaterialColor(Material material, Color color)
    {
        if (material.HasProperty("_BaseColor"))
            material.SetColor("_BaseColor", color);

        if (material.HasProperty("_Color"))
            material.SetColor("_Color", color);

    }

    static Shader FindShader(params string[] names)
    {
        foreach (string name in names)
        {
            Shader shader = Shader.Find(name);
            if (shader != null && shader.name != "Hidden/InternalErrorShader")
                return shader;
        }

        return Shader.Find("Sprites/Default");
    }

    static Material CloneKnownGoodMaterial()
    {
        string[] folders = { "Assets/Game", "Assets/World/Hub", "Assets/World" };
        string[] materialGuids = AssetDatabase.FindAssets("t:Material", folders);

        foreach (string guid in materialGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (path.Contains("Wisp_"))
                continue;

            Material template = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (template == null || template.shader == null)
                continue;

            if (template.shader.name == "Hidden/InternalErrorShader")
                continue;

            return new Material(template.shader);
        }

        return null;
    }

    static void EnsureDir(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
            return;

        string parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
        string folder = Path.GetFileName(path);

        if (!string.IsNullOrEmpty(parent))
            EnsureDir(parent);

        AssetDatabase.CreateFolder(string.IsNullOrEmpty(parent) ? "Assets" : parent, folder);
    }
}
#endif
