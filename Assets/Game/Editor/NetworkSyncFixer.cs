using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEditor;
using UnityEngine;

/// <summary>
/// BCE/Setup/4n — Adds the missing movement-replication components to the player
/// and enemy prefabs so remote players/enemies are SEEN MOVING by other clients.
///
/// Why this exists:
///   The 5 hero prefabs (Marauder/Ironclad/Shadowblade/Cleric/Arcanist) and the 3
///   base enemy prefabs (Grunt/Ranged/Elite) carry a NetworkIdentity — so they
///   SPAWN on every client — but they have no NetworkTransform. Mirror never
///   replicates their position, so to everyone except the owner they stand frozen
///   at their spawn point. Health.cs already calls GetComponent&lt;NetworkTransformBase&gt;()
///   on respawn (line ~656), and the newer enemy builders (Cyclops/FieldGhoul/Wisp)
///   add one — these older prefabs simply predate that and were never updated.
///
/// What it adds (idempotent — safe to re-run):
///   • Heroes  → NetworkTransformUnreliable (syncDirection = ClientToServer, i.e.
///               client authority, matching the client-driven PlayerMovement) +
///               NetworkAnimator (clientAuthority = true) so walk/run/attack anims
///               replicate.
///   • Enemies → NetworkTransformUnreliable (syncDirection = ServerToClient, i.e.
///               server authority, since the server runs their NavMesh AI) +
///               NetworkAnimator (clientAuthority = false).
///
/// Prefabs are found by name via AssetDatabase so this keeps working regardless of
/// where the prefab files live (they were relocated to Game_Prefabs/Muffin Junk).
/// </summary>
public static class NetworkSyncFixer
{
    static readonly string[] HeroNames  = { "Marauder", "Ironclad", "Shadowblade", "Cleric", "Arcanist" };
    static readonly string[] EnemyNames = { "Enemy_Grunt", "Enemy_Ranged", "Enemy_Elite" };

    [MenuItem("BCE/Setup/4n ▶ Add Movement Sync (Players + Enemies)", priority = 7)]
    static void AddMovementSync()
    {
        var report = new List<string>();

        foreach (var name in HeroNames)
            report.Add(Process(name, clientAuthority: true));

        foreach (var name in EnemyNames)
            report.Add(Process(name, clientAuthority: false));

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog(
            "Movement Sync",
            "Configured NetworkTransform + NetworkAnimator:\n\n" + string.Join("\n", report) +
            "\n\nHeroes use client authority; enemies use server authority.\n" +
            "Rebuild + redeploy the dedicated server so the new prefabs ship.",
            "Done");
    }

    static string Process(string prefabName, bool clientAuthority)
    {
        string path = FindPrefabPath(prefabName);
        if (path == null)
            return $"  ✗ {prefabName} — prefab not found";

        try
        {
            using var scope = new PrefabUtility.EditPrefabContentsScope(path);
            var root = scope.prefabContentsRoot;

            if (root.GetComponent<NetworkIdentity>() == null)
                return $"  ✗ {prefabName} — no NetworkIdentity (not a networked prefab?)";

            bool addedNT   = EnsureNetworkTransform(root, clientAuthority);
            bool addedAnim = EnsureNetworkAnimator(root, clientAuthority);

            string status = (addedNT || addedAnim) ? "updated" : "already correct";
            string auth   = clientAuthority ? "client-auth" : "server-auth";
            return $"  ✓ {prefabName} ({auth}) — {status}";
        }
        catch (System.Exception e)
        {
            return $"  ✗ {prefabName} — error: {e.Message}";
        }
    }

    static bool EnsureNetworkTransform(GameObject root, bool clientAuthority)
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

        nt.syncDirection = clientAuthority
            ? SyncDirection.ClientToServer
            : SyncDirection.ServerToClient;

        // Position + rotation sync on; scale off (characters never scale in play).
        nt.syncPosition = true;
        nt.syncRotation = true;
        nt.syncScale    = false;

        EditorUtility.SetDirty(nt);
        return added;
    }

    static bool EnsureNetworkAnimator(GameObject root, bool clientAuthority)
    {
        var na = root.GetComponent<NetworkAnimator>();
        bool added = false;
        if (na == null)
        {
            na = root.AddComponent<NetworkAnimator>();
            added = true;
        }

        if (na.animator == null)
            na.animator = root.GetComponentInChildren<Animator>(true);

        na.clientAuthority = clientAuthority;

        EditorUtility.SetDirty(na);
        return added;
    }

    static string FindPrefabPath(string prefabName)
    {
        // Exact-name match among prefab assets (FindAssets does substring/fuzzy).
        return AssetDatabase.FindAssets($"{prefabName} t:Prefab")
            .Select(AssetDatabase.GUIDToAssetPath)
            .FirstOrDefault(p =>
                System.IO.Path.GetFileNameWithoutExtension(p) == prefabName);
    }
}
