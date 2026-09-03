#if UNITY_EDITOR
using System.IO;
using System.Text;
using Mirror;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Builds the shared network ore prefab and safe spawn bounds for non-Hub zones.</summary>
public static class DynamicOreSpawnerBuilder
{
    const string PrefabFolder = "Assets/Game/Resources/Gathering/NetworkPrefabs";
    const string PrefabPath = PrefabFolder + "/DynamicMineralVein.prefab";
    const string DefinitionPath = "Assets/Game/Resources/NodeForge/Definitions/copper_vein.asset";

    static readonly string[] ZonePaths =
    {
        SceneNames.DarkwoodPath,
        SceneNames.ToujamBasinPath,
        SceneNames.BoneyardPath,
        SceneNames.AshenWastelandsPath,
        SceneNames.GMIslandPath,
        SceneNames.GatheringZonePath,
        SceneNames.ArenaCopperPath,
        SceneNames.PvpZonePath,
    };

    [MenuItem("BCE/Setup/12 ▶ Dynamic Ore Spawns — Build", priority = 54)]
    static void Build()
    {
        string originalPath = SceneManager.GetActiveScene().path;
        var report = new StringBuilder("Dynamic Ore Spawn Builder\n\n");
        BuildPrefab(report);
        report.AppendLine("SKIP (placeholder has no ground): " + SceneNames.VoidDungeonPath);

        foreach (string path in ZonePaths)
        {
            if (!File.Exists(path))
            {
                report.AppendLine($"SKIP (scene absent): {path}");
                continue;
            }

            Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            OreSpawnVolume existing = Object.FindAnyObjectByType<OreSpawnVolume>();
            if (existing != null)
            {
                report.AppendLine($"OK (existing volume): {scene.name}");
                continue;
            }

            if (!TryGetPlayableBounds(out Bounds bounds))
            {
                report.AppendLine($"NEEDS MANUAL VOLUME: {scene.name}");
                continue;
            }

            GameObject root = new("Dynamic Ore Spawn Volume");
            root.transform.position = bounds.center;
            OreSpawnVolume volume = root.AddComponent<OreSpawnVolume>();
            volume.size = new Vector3(
                Mathf.Max(20f, bounds.size.x * 0.9f),
                Mathf.Max(20f, bounds.size.y + 10f),
                Mathf.Max(20f, bounds.size.z * 0.9f));
            volume.groundSearchHeight = 20f;
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            report.AppendLine($"ADDED: {scene.name} ({volume.size.x:0} × {volume.size.z:0}m)");
        }

        if (!string.IsNullOrEmpty(originalPath) && File.Exists(originalPath))
            EditorSceneManager.OpenScene(originalPath, OpenSceneMode.Single);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Dynamic Ore Spawns", report.ToString(), "OK");
    }

    [MenuItem("BCE/Validate/Dynamic Ore Spawns")]
    static void Validate()
    {
        var report = new StringBuilder();
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        report.AppendLine(prefab != null && prefab.GetComponent<NetworkIdentity>() != null &&
                          prefab.GetComponent<GatheringNodeNetworkState>() != null
            ? "PASS: network ore prefab"
            : "FAIL: network ore prefab missing or incomplete");
        report.AppendLine("SKIP: VoidDungeon — placeholder scene has no playable ground");

        string originalPath = SceneManager.GetActiveScene().path;
        foreach (string path in ZonePaths)
        {
            if (!File.Exists(path)) continue;
            Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            int count = Object.FindObjectsByType<OreSpawnVolume>(FindObjectsInactive.Include,
                FindObjectsSortMode.None).Length;
            report.AppendLine($"{(count > 0 ? "PASS" : "FAIL")}: {scene.name} — {count} volume(s)");
        }
        if (!string.IsNullOrEmpty(originalPath) && File.Exists(originalPath))
            EditorSceneManager.OpenScene(originalPath, OpenSceneMode.Single);
        EditorUtility.DisplayDialog("Dynamic Ore Spawn Validation", report.ToString(), "OK");
    }

    static void BuildPrefab(StringBuilder report)
    {
        Directory.CreateDirectory(PrefabFolder);
        GatheringNodeDefinition definition =
            AssetDatabase.LoadAssetAtPath<GatheringNodeDefinition>(DefinitionPath);
        if (definition == null)
        {
            report.AppendLine("FAIL: copper_vein definition missing");
            return;
        }

        GameObject root = new("Dynamic Mineral Vein");
        root.AddComponent<NetworkIdentity>();
        BoxCollider collider = root.AddComponent<BoxCollider>();
        collider.center = definition.colliderCenter;
        collider.size = definition.colliderSize;
        GatheringNodeNetworkState state = root.AddComponent<GatheringNodeNetworkState>();
        definition.ApplyTo(state);

        AfkGatheringStation station = root.AddComponent<AfkGatheringStation>();
        definition.ApplyTo(station);
        if (definition.visualPrefab != null)
        {
            GameObject visual = (GameObject)PrefabUtility.InstantiatePrefab(definition.visualPrefab, root.transform);
            visual.name = "Ore Visual";
            visual.transform.localPosition = definition.visualLocalPosition;
            visual.transform.localRotation = Quaternion.Euler(definition.visualLocalEulerAngles);
            visual.transform.localScale = definition.visualLocalScale;
        }

        PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        Object.DestroyImmediate(root);
        report.AppendLine($"BUILT: {PrefabPath}");
    }

    static bool TryGetPlayableBounds(out Bounds result)
    {
        result = default;
        bool found = false;
        foreach (Terrain terrain in Object.FindObjectsByType<Terrain>(FindObjectsInactive.Exclude,
                     FindObjectsSortMode.None))
        {
            Vector3 size = terrain.terrainData.size;
            Encapsulate(ref result, ref found, new Bounds(terrain.transform.position + size * 0.5f, size));
        }
        if (found) return true;

        foreach (Collider collider in Object.FindObjectsByType<Collider>(FindObjectsInactive.Exclude,
                     FindObjectsSortMode.None))
        {
            if (collider.isTrigger || collider.bounds.size.x > 1000f || collider.bounds.size.z > 1000f)
                continue;
            Encapsulate(ref result, ref found, collider.bounds);
        }
        return found;
    }

    static void Encapsulate(ref Bounds aggregate, ref bool found, Bounds next)
    {
        if (!found) { aggregate = next; found = true; }
        else aggregate.Encapsulate(next);
    }
}
#endif
