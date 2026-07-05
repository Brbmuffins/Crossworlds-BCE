#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// BCE/Hub World/Setup Field Ghoul NPCs
///
/// Wires the existing "Field Goul" models already in the Hub scene:
///   1. NavMeshAgent   — pathfinding (speed 2, stopping 0.5)
///   2. FieldGhoulNPC  — wander loop (plain MonoBehaviour, works in editor + server)
///
/// Re-running is safe — components are added only if missing.
/// NavMesh must be baked (Window → AI → Navigation → Bake).
/// </summary>
public static class FieldGhoulSetupBuilder
{
    [MenuItem("BCE/Hub World/Setup Field Ghoul NPCs")]
    public static void SetupGhouls()
    {
        var scene = EditorSceneManager.GetActiveScene();
        var roots = scene.GetRootGameObjects();
        int wired = 0;

        foreach (var root in roots)
            wired += WireGhoulsIn(root);

        if (wired == 0)
        {
            EditorUtility.DisplayDialog("No Field Ghouls Found",
                "No GameObjects with 'Field Goul' in their name were found.\n\n" +
                "Make sure you are in the Hub scene with the Field Goul models placed.",
                "OK");
            return;
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorUtility.DisplayDialog("Field Ghouls Wired ✓",
            $"Wired {wired} Field Goul object(s).\n\n" +
            "Hit Play to test. They will wander immediately on the NavMesh.",
            "Done!");
    }

    [MenuItem("BCE/Hub World/Setup Field Ghoul NPCs", true)]
    static bool Validate() => !Application.isPlaying;

    static int WireGhoulsIn(GameObject go)
    {
        int count = 0;
        if (go.name.IndexOf("Field Goul", System.StringComparison.OrdinalIgnoreCase) >= 0)
        {
            WireOne(go);
            count++;
        }
        foreach (Transform child in go.transform)
            count += WireGhoulsIn(child.gameObject);
        return count;
    }

    static void WireOne(GameObject go)
    {
        // Strip any Mirror components added by the previous version of this builder.
        // NetworkIdentity causes Mirror to hide scene objects when no server is running.
        var ni = go.GetComponent<Mirror.NetworkIdentity>();
        if (ni != null) Object.DestroyImmediate(ni);
        var nt = go.GetComponent<Mirror.NetworkTransformReliable>();
        if (nt != null) Object.DestroyImmediate(nt);

        var agent = go.GetComponent<NavMeshAgent>();
        if (agent == null) agent = go.AddComponent<NavMeshAgent>();
        agent.speed            = 2f;
        agent.angularSpeed     = 120f;
        agent.stoppingDistance = 0.5f;
        agent.radius           = 0.4f;
        agent.height           = 1.8f;
        agent.baseOffset       = 0f;

        var npc = go.GetComponent<FieldGhoulNPC>();
        if (npc == null) npc = go.AddComponent<FieldGhoulNPC>();
        npc.wanderRadius = 8f;
        npc.minWaitTime  = 2f;
        npc.maxWaitTime  = 5f;

        EditorUtility.SetDirty(go);
        Debug.Log($"[GhoulNPC] Wired: {go.name}");
    }
}
#endif
