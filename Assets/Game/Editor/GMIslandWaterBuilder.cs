#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class GMIslandWaterBuilder
{
    const string ScenePath = "Assets/Game/Scenes/GM Island.unity";
    const string MaterialPath = "Assets/Game/World/Water/GMIslandWater.mat";
    const string WaterName = "GM Island Water";

    [MenuItem("BCE/GM Island/Build Animated Water")]
    public static void BuildAnimatedWater()
    {
        if (!File.Exists(ScenePath))
        {
            EditorUtility.DisplayDialog("GM Island Missing", $"Could not find:\n{ScenePath}", "OK");
            return;
        }

        var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
        if (material == null)
        {
            EditorUtility.DisplayDialog("Water Material Missing", $"Could not load:\n{MaterialPath}", "OK");
            return;
        }

        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        GameObject water = GameObject.Find(WaterName);

        if (water == null)
        {
            water = GameObject.CreatePrimitive(PrimitiveType.Plane);
            water.name = WaterName;
        }

        water.transform.position = new Vector3(0f, -0.15f, 0f);
        water.transform.rotation = Quaternion.identity;
        water.transform.localScale = new Vector3(30f, 1f, 30f);

        var renderer = water.GetComponent<MeshRenderer>();
        if (renderer != null)
            renderer.sharedMaterial = material;

        var collider = water.GetComponent<Collider>();
        if (collider != null)
            Object.DestroyImmediate(collider);

        EditorUtility.SetDirty(water);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();

        EditorUtility.DisplayDialog("GM Island Water Ready",
            "Created/updated GM Island Water with the animated URP water material.",
            "Done");
    }
}
#endif
