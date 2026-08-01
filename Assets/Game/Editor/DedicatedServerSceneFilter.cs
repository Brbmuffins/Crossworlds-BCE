#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Removes client-only authored visuals from the temporary server scene copy.</summary>
public sealed class DedicatedServerSceneFilter : IProcessSceneWithReport
{
    public int callbackOrder => -1000;

    public void OnProcessScene(Scene scene, BuildReport report)
    {
        if (!BuildPipeline.isBuildingPlayer ||
            EditorUserBuildSettings.standaloneBuildSubtarget != StandaloneBuildSubtarget.Server)
            return;

        int removed = 0;
        foreach (GameObject root in scene.GetRootGameObjects())
        foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
        {
            if (child != null && child.name == "QuestMarker")
            {
                Object.DestroyImmediate(child.gameObject);
                removed++;
            }
        }

        if (removed > 0)
            Debug.Log($"[DedicatedServerSceneFilter] {scene.path}: removed {removed} client-only QuestMarker object(s).");
    }
}
#endif
