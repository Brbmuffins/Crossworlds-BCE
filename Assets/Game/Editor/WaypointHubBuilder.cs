#if UNITY_EDITOR
using Mirror;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// BCE/Setup/4w — Place WaypointMapTrigger in the Hub scene.
///
/// Adds one WaypointMapTrigger object to HUB.unity so players can walk up to
/// the Waystone obelisk, press E, and see the world map to travel to Darkwood
/// and Ashen Wastelands.
///
/// Run with HUB.unity open (or this builder opens it for you). Idempotent.
/// After running, reposition the "Waystone Map" object next to the visual
/// Waystone obelisk in the Hub scene, then Ctrl+S.
/// </summary>
public static class WaypointHubBuilder
{
    const string HubScenePath = SceneNames.HubPath;

    [MenuItem("BCE/Setup/4w ▶ Place Waypoint Map Trigger in Hub", priority = 10)]
    static void Build()
    {
        // Open Hub scene if not already loaded
        Scene hubScene = SceneManager.GetSceneByPath(HubScenePath);
        if (!hubScene.isLoaded)
        {
            if (!System.IO.File.Exists(HubScenePath))
            {
                EditorUtility.DisplayDialog("Waypoint Hub Builder",
                    $"Hub scene not found at:\n{HubScenePath}\n\nRun BCE/Setup/1 first.",
                    "OK");
                return;
            }
            hubScene = EditorSceneManager.OpenScene(HubScenePath, OpenSceneMode.Additive);
        }

        // Idempotency: skip if one already exists
        foreach (var root in hubScene.GetRootGameObjects())
        {
            if (root.GetComponentInChildren<WaypointMapTrigger>() != null)
            {
                EditorUtility.DisplayDialog("Waypoint Hub Builder",
                    "WaypointMapTrigger already present in Hub scene.\n\nNothing to do.",
                    "OK");
                return;
            }
        }

        // Create the waypoint map object
        var go = new GameObject("Waystone Map");
        go.AddComponent<NetworkIdentity>();
        go.AddComponent<SphereCollider>().isTrigger = true;

        var trigger = go.AddComponent<WaypointMapTrigger>();
        trigger.promptText = "[E] Open World Map";
        trigger.interactionRange = 6f;

        // Default position near the centre of the hub — owner should move it to sit
        // next to the visual Waystone obelisk after running this builder.
        go.transform.position = new Vector3(0f, 0f, 5f);

        SceneManager.MoveGameObjectToScene(go, hubScene);
        EditorSceneManager.MarkSceneDirty(hubScene);
        EditorSceneManager.SaveScene(hubScene);

        Selection.activeGameObject = go;
        EditorGUIUtility.PingObject(go);

        EditorUtility.DisplayDialog("Waypoint Hub Builder",
            "✓ Waystone Map placed in Hub scene and saved.\n\n" +
            "The object spawned at (0, 0, 5). Move it next to the Waystone\n" +
            "obelisk in the scene view, then Ctrl+S to save again.\n\n" +
            "The WaypointMapTrigger already knows about Darkwood and Ashen Wastelands\n" +
            "(see WaypointMapTrigger.DefaultNodes). No extra config needed.",
            "Done");
    }

    [MenuItem("BCE/Setup/4w ▶ Place Waypoint Map Trigger in Hub", true)]
    static bool Validate() => !Application.isPlaying;
}
#endif
