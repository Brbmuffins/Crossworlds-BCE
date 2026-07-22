#if UNITY_EDITOR
using System.Collections.Generic;
using Mirror;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// BCE/Setup/4o — Makes the hand-placed world mobs real server-authoritative networked
/// enemies (the ogre, the cyclops, the ghouls, the training dummy).
///
/// Why this exists:
///   These prefabs were authored with Health (+ NavMeshAgent + an AI behaviour) but no
///   NetworkIdentity. Health is a NetworkBehaviour, so Mirror logs
///       "Health on <name> requires a NetworkIdentity"
///   on every load/import. Worse than the log line: without an identity the server never
///   owns their health, so damage is client-local and each client kills their own copy.
///
///   They inherited that shape from FieldGhoulSetupBuilder, which deliberately strips
///   NetworkIdentity off the Hub ghouls (its comment: identities hide scene objects when
///   no server is running). These are Darkwood combat mobs, not Hub set dressing, so they
///   get the same treatment as Enemy_Grunt/Ranged/Elite instead.
///
///   EnemyWanderAI is already safe to network: every AI path (Update, WanderLoop,
///   OnDamagedBy) is gated behind CanRunServerSide(), so the NavMesh wander/chase runs
///   server-only and will not fight the NetworkTransform on clients. No AI changes needed.
///
/// What it adds (idempotent — safe to re-run):
///   • NetworkIdentity — always.
///   • NetworkTransformUnreliable (ServerToClient) — only for mobs that actually move
///     (detected via NavMeshAgent). A static dummy would just burn bandwidth.
///   • NetworkAnimator (server authority) — only where an Animator exists.
/// Mirrors the server-auth enemy config in NetworkSyncFixer (BCE/Setup/4n), which refuses
/// to touch these prefabs precisely because they have no identity yet.
///
/// AFTER RUNNING: open Assets/Game/Scenes/Darkwood.unity and save it. Mirror bakes the
/// scene-object sceneId on save; without that the placed mobs will not spawn for clients.
///
/// NOT included — Prefab_Brandalf. It has PlayerMovement and logs the same error, but it is
/// the 6th-hero model whose skin-vs-class question is still open (see ROADMAP). Networking
/// it would quietly commit to "it's a character". Left alone on purpose.
/// </summary>
public static class WorldMobNetworkFixer
{
    // Exact paths — several of these have names far too generic for AssetDatabase search
    // ("Idle" is the ogre, after its source Idle.fbx).
    static readonly string[] MobPrefabPaths =
    {
        "Assets/Game/3D Models/Enemies/Ogres/O'gar Brute/Prefab_OgreBrute.prefab",
        "Assets/Game/3D Models/Enemies/Cyclops/Cyclops Brute/Prefab/Cyclops Brute.prefab",
        "Assets/Game/3D Models/Enemies/Field Goul/Prefab/Gunda.prefab",
        "Assets/Game/Game_Prefabs/Prefab_TrainingDummy.prefab",
    };

    [MenuItem("BCE/Setup/4o ▶ Network World Mobs (Ogre, Cyclops, Gunda, Dummy)", priority = 8)]
    public static void NetworkWorldMobs()
    {
        var report = new List<string>();

        foreach (var path in MobPrefabPaths)
            report.Add(Process(path));

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("World Mobs Networked",
            string.Join("\n", report) +
            "\n\nNEXT — REQUIRED:\n" +
            "Open Assets/Game/Scenes/Darkwood.unity and save it (Ctrl+S).\n" +
            "Mirror bakes the scene-object sceneId on save; until then the placed\n" +
            "mobs will not spawn for clients.\n\n" +
            "Then commit + push — CI rebuilds and redeploys the server.",
            "Done");
    }

    [MenuItem("BCE/Setup/4o ▶ Network World Mobs (Ogre, Cyclops, Gunda, Dummy)", true)]
    static bool Validate() => !Application.isPlaying;

    static string Process(string path)
    {
        string name = System.IO.Path.GetFileNameWithoutExtension(path);

        if (AssetDatabase.LoadAssetAtPath<GameObject>(path) == null)
            return $"  ✗ {name} — not found at {path}";

        try
        {
            using var scope = new PrefabUtility.EditPrefabContentsScope(path);
            var root = scope.prefabContentsRoot;

            bool addedIdentity = EnsureIdentity(root);

            // Only movers need their transform replicated.
            bool moves = root.GetComponentInChildren<NavMeshAgent>(true) != null;
            bool addedNT = moves && EnsureNetworkTransform(root);

            var animator = root.GetComponentInChildren<Animator>(true);
            bool addedNA = animator != null && EnsureNetworkAnimator(root, animator);

            var bits = new List<string>();
            if (addedIdentity) bits.Add("Identity");
            if (addedNT) bits.Add("Transform");
            if (addedNA) bits.Add("Animator");

            string detail = bits.Count > 0 ? "added " + string.Join(" + ", bits) : "already correct";
            string kind = moves ? "mover" : "static";
            return $"  ✓ {name} ({kind}) — {detail}";
        }
        catch (System.Exception e)
        {
            return $"  ✗ {name} — error: {e.Message}";
        }
    }

    static bool EnsureIdentity(GameObject root)
    {
        if (root.GetComponent<NetworkIdentity>() != null) return false;
        var ni = root.AddComponent<NetworkIdentity>();
        EditorUtility.SetDirty(ni);
        return true;
    }

    static bool EnsureNetworkTransform(GameObject root)
    {
        var nt = root.GetComponent<NetworkTransformBase>();
        bool added = false;
        if (nt == null)
        {
            nt = root.AddComponent<NetworkTransformUnreliable>();
            added = true;
        }

        // target defaults to null when added from code (Reset doesn't fire) — wire it.
        if (nt.target == null) nt.target = root.transform;

        // Server runs the NavMesh AI, so the server owns the transform.
        nt.syncDirection = SyncDirection.ServerToClient;
        nt.syncPosition = true;
        nt.syncRotation = true;
        nt.syncScale = false;

        EditorUtility.SetDirty(nt);
        return added;
    }

    static bool EnsureNetworkAnimator(GameObject root, Animator animator)
    {
        var na = root.GetComponent<NetworkAnimator>();
        bool added = false;
        if (na == null)
        {
            na = root.AddComponent<NetworkAnimator>();
            added = true;
        }

        if (na.animator == null) na.animator = animator;
        na.clientAuthority = false;

        EditorUtility.SetDirty(na);
        return added;
    }
}
#endif
