#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// BCE/Hub World/Setup Field Ghoul NPCs
///
/// Wires the existing "Field Goul" models already in the Hub scene:
///   1. Tag → "Enemy"   — abilities use CompareTag("Enemy") for targeting
///   2. Health           — required for TakeDamage; 200 HP, passive (no aggro)
///   3. NavMeshAgent     — pathfinding (speed 2, stopping 0.5)
///   4. FieldGhoulNPC    — wander loop (plain MonoBehaviour, editor + server)
///   5. CapsuleCollider  — if no collider exists, so raycasts/overlaps can hit it
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

        // Tag — abilities filter targets with CompareTag("Enemy")
        go.tag = "Enemy";

        // Health — required for TakeDamage calls in AbilityCaster
        var health = go.GetComponent<Health>();
        if (health == null) health = go.AddComponent<Health>();
        health.maxHealth     = 200f;
        health.currentHealth = 200f;

        // Collider — abilities use Physics.OverlapSphere / SphereCastAll to find hits.
        // Add a capsule on the root if nothing collidable is already present.
        if (go.GetComponentInChildren<Collider>() == null)
        {
            var cap = go.AddComponent<CapsuleCollider>();
            cap.center = new Vector3(0f, 0.9f, 0f);
            cap.radius = 0.4f;
            cap.height = 1.8f;
        }

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
