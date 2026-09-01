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

        int removedMarkers = 0;
        foreach (GameObject root in scene.GetRootGameObjects())
        foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
        {
            if (child != null && child.name == "QuestMarker")
            {
                Object.DestroyImmediate(child.gameObject);
                removedMarkers++;
            }
        }

        // These authoring/client behaviours are intentionally absent from a
        // UNITY_SERVER player. Remove them from the temporary build-scene copy
        // so Unity never serializes references to types that the dedicated
        // server cannot load. GatheringNodeNetworkState and NetworkIdentity
        // remain on the object and continue to own shared depletion state.
        int removedGatheringBehaviours = 0;
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            foreach (GatheringNodeInstance instance in root.GetComponentsInChildren<GatheringNodeInstance>(true))
            {
                if (instance == null) continue;
                Object.DestroyImmediate(instance);
                removedGatheringBehaviours++;
            }
            foreach (AfkGatheringStation station in root.GetComponentsInChildren<AfkGatheringStation>(true))
            {
                if (station == null) continue;
                Object.DestroyImmediate(station);
                removedGatheringBehaviours++;
            }
        }

        if (removedMarkers > 0 || removedGatheringBehaviours > 0)
            Debug.Log($"[DedicatedServerSceneFilter] {scene.path}: removed " +
                      $"{removedMarkers} client-only QuestMarker object(s) and " +
                      $"{removedGatheringBehaviours} client-only gathering behaviour(s).");
    }
}
#endif
