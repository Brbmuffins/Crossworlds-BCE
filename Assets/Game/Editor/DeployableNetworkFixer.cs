using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mirror;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// BCE/Setup/4d — Makes player ability deployables + turrets network-spawnable so
/// other players SEE spell objects appear in the world.
///
/// The deployable behaviours (ShockMine, NaniteSwarm, Singularity, LastBastionWall,
/// NullFieldZone, RestorationBeacon, TurretController) were plain MonoBehaviours on
/// prefabs with no NetworkIdentity, and AbilityCaster spawned them with plain
/// Instantiate — so they existed only on the server and were invisible to clients.
///
/// AbilityCaster now calls NetworkServer.Spawn on them (guarded on NetworkIdentity),
/// and the behaviours gate their gameplay to the server (DeployableNet.IsAuthority).
/// This tool supplies the missing prefab components + spawn registration:
///   • NetworkIdentity on every deployable/turret prefab (required to replicate).
///   • NetworkTransformUnreliable (server authority) on the movers — NaniteSwarm
///     (drifts) and TurretController (rotates to aim) — so their motion replicates.
///     Static deployables need none: NetworkServer.Spawn already sends their pose.
///   • Registers all of them in RodNetworkManager.worldPrefabs + spawnPrefabs so
///     clients can instantiate the incoming spawns.
///
/// Idempotent — safe to re-run.
/// </summary>
public static class DeployableNetworkFixer
{
    const string LOGIN_SCENE = "Assets/Game/Scenes/LoginScene.unity";
    static readonly string[] SearchFolders = { "Assets/Game" };

    // Behaviour types that mark a prefab as a networked deployable.
    static readonly Type[] StaticDeployables =
    {
        typeof(ShockMineBehaviour), typeof(SingularityBehaviour), typeof(LastBastionWall),
        typeof(NullFieldZone), typeof(RestorationBeacon),
    };

    // Movers also get a NetworkTransform so their server-driven motion replicates.
    static readonly Type[] MovingDeployables =
    {
        typeof(NaniteSwarmBehaviour), typeof(TurretController),
    };

    [MenuItem("BCE/Setup/4d ▶ Network Deployables + Turret", priority = 8)]
    static void Run()
    {
        var touched = new List<GameObject>();
        var report  = new List<string>();

        foreach (var path in FindPrefabPaths())
        {
            var root = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (root == null) continue;

            bool isStatic = StaticDeployables.Any(t => root.GetComponent(t) != null);
            bool isMover  = MovingDeployables.Any(t => root.GetComponent(t) != null);
            if (!isStatic && !isMover) continue;

            try
            {
                using (var scope = new PrefabUtility.EditPrefabContentsScope(path))
                {
                    bool ni = EnsureNetworkIdentity(scope.prefabContentsRoot);
                    bool nt = isMover && EnsureNetworkTransform(scope.prefabContentsRoot);
                    report.Add($"  ✓ {Path.GetFileNameWithoutExtension(path)} " +
                               $"({(isMover ? "mover" : "static")}) " +
                               $"— {(ni || nt ? "updated" : "already correct")}");
                }
                touched.Add(AssetDatabase.LoadAssetAtPath<GameObject>(path));
            }
            catch (Exception e)
            {
                report.Add($"  ✗ {Path.GetFileNameWithoutExtension(path)} — {e.Message}");
            }
        }

        // Force-reimport so NetworkIdentity.OnValidate assigns assetIds (SaveAsPrefab /
        // EditPrefabContentsScope don't trigger it — same fix as RodPrefabBuilder).
        foreach (var go in touched)
        {
            var p = AssetDatabase.GetAssetPath(go);
            if (!string.IsNullOrEmpty(p)) AssetDatabase.ImportAsset(p, ImportAssetOptions.ForceUpdate);
        }
        AssetDatabase.SaveAssets();

        bool wired = RegisterInWorldPrefabs(touched, report);

        EditorUtility.DisplayDialog(
            "Network Deployables",
            (report.Count > 0 ? string.Join("\n", report) : "  (no deployable prefabs found)") +
            (wired ? "\n\nRegistered in RodNetworkManager.worldPrefabs ✅"
                   : "\n\n⚠ Could not auto-register in worldPrefabs — add them manually.") +
            "\n\nRebuild + redeploy the dedicated server so the networked prefabs ship.",
            "Done");
    }

    static IEnumerable<string> FindPrefabPaths() =>
        AssetDatabase.FindAssets("t:Prefab", SearchFolders)
            .Select(AssetDatabase.GUIDToAssetPath)
            .Distinct();

    static bool EnsureNetworkIdentity(GameObject root)
    {
        if (root.GetComponent<NetworkIdentity>() != null) return false;
        root.AddComponent<NetworkIdentity>();
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
        if (nt.target == null) nt.target = root.transform;   // Reset doesn't fire via AddComponent
        nt.syncDirection = SyncDirection.ServerToClient;      // server-authoritative
        nt.syncPosition  = true;
        nt.syncRotation  = true;                              // turret aim
        nt.syncScale     = false;
        EditorUtility.SetDirty(nt);
        return added;
    }

    static bool RegisterInWorldPrefabs(List<GameObject> prefabs, List<string> report)
    {
        if (prefabs.Count == 0) return false;
        if (!File.Exists(LOGIN_SCENE)) { report.Add("  ✗ LoginScene not found"); return false; }

        var scene = EditorSceneManager.OpenScene(LOGIN_SCENE, OpenSceneMode.Single);
        RodNetworkManager nm = scene.GetRootGameObjects()
            .Select(r => r.GetComponent<RodNetworkManager>())
            .FirstOrDefault(x => x != null);
        if (nm == null) { report.Add("  ✗ RodNetworkManager not in LoginScene"); return false; }

        var world = new List<GameObject>(nm.worldPrefabs ?? Array.Empty<GameObject>());
        int added = 0;
        foreach (var p in prefabs)
        {
            if (p == null) continue;
            if (!world.Contains(p)) { world.Add(p); added++; }
            if (!nm.spawnPrefabs.Contains(p)) nm.spawnPrefabs.Add(p);
        }
        nm.worldPrefabs = world.ToArray();

        EditorUtility.SetDirty(nm);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        report.Add($"  → worldPrefabs: +{added} new (total {world.Count})");
        return true;
    }
}
