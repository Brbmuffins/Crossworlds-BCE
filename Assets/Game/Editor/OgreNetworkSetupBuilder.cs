#if UNITY_EDITOR
using Mirror;
using UnityEditor;
using UnityEngine;

/// <summary>
/// BCE/Setup/4o — Makes the O'gar Brute ogre a real server-authoritative networked enemy.
///
/// Why this exists:
///   The ogre prefab ("Idle.prefab", named after the source Idle.fbx) was authored with
///   Health + FieldGhoulNPC + OgreAnimationDriver but no NetworkIdentity. Health is a
///   NetworkBehaviour, so Mirror logs
///       "Health on Idle requires a NetworkIdentity"
///   on every load/import. Worse than the log line: without an identity the server never
///   owns its health, so damage is client-local and each client kills their own copy.
///
///   It inherited that shape from FieldGhoulSetupBuilder, which deliberately strips
///   NetworkIdentity off the Hub ghouls (its comment: identities hide scene objects when
///   no server is running). The ogre is a Darkwood combat mob, not Hub set dressing, so it
///   gets the same treatment as Enemy_Grunt/Ranged/Elite instead.
///
///   FieldGhoulNPC is already safe to network: every AI path (Update, WanderLoop,
///   OnDamagedBy) is gated behind CanRunServerSide(), so the NavMesh wander/chase runs
///   server-only and will not fight the NetworkTransform on clients. No AI changes needed.
///
/// What it adds (idempotent — safe to re-run):
///   • NetworkIdentity
///   • NetworkTransformUnreliable — syncDirection ServerToClient (server runs the NavMesh AI)
///   • NetworkAnimator            — clientAuthority = false
/// Mirrors the server-auth enemy config in NetworkSyncFixer (BCE/Setup/4n), which refuses
/// to touch this prefab precisely because it has no identity yet. Run this first, then 4n
/// stops skipping it.
///
/// AFTER RUNNING: open Assets/Game/Scenes/Darkwood.unity and save it. Mirror bakes the
/// scene-object sceneId on save; without that the placed ogre will not spawn for clients.
/// </summary>
public static class OgreNetworkSetupBuilder
{
    // Exact path — the prefab's name ("Idle") is far too generic for AssetDatabase search.
    const string OgrePrefabPath = "Assets/Game/3D Models/Enemies/Ogres/O'gar Brute/Idle.prefab";

    [MenuItem("BCE/Setup/4o ▶ Network the O'gar Brute (Ogre)", priority = 8)]
    static void NetworkTheOgre()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(OgrePrefabPath);
        if (prefab == null)
        {
            EditorUtility.DisplayDialog("Ogre Not Found",
                "Could not load the ogre prefab at:\n\n" + OgrePrefabPath +
                "\n\nIf it moved, update OgrePrefabPath in OgreNetworkSetupBuilder.cs.",
                "OK");
            return;
        }

        string report;
        try
        {
            using var scope = new PrefabUtility.EditPrefabContentsScope(OgrePrefabPath);
            var root = scope.prefabContentsRoot;

            bool addedIdentity = EnsureComponent<NetworkIdentity>(root, out _);
            bool addedTransform = EnsureNetworkTransform(root);
            bool addedAnimator = EnsureNetworkAnimator(root);

            report =
                $"  • NetworkIdentity            — {(addedIdentity ? "added" : "already present")}\n" +
                $"  • NetworkTransformUnreliable — {(addedTransform ? "added" : "already present")} (ServerToClient)\n" +
                $"  • NetworkAnimator            — {(addedAnimator ? "added" : "already present")} (server authority)";
        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("Ogre Networking Failed", e.Message, "OK");
            return;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Ogre Networked ✓",
            report +
            "\n\nNEXT — REQUIRED:\n" +
            "Open Assets/Game/Scenes/Darkwood.unity and save it (Ctrl+S).\n" +
            "Mirror bakes the scene-object sceneId on save; until then the placed\n" +
            "ogre will not spawn for clients.\n\n" +
            "Then rebuild + redeploy the dedicated server so it ships the new prefab.",
            "Done");
    }

    [MenuItem("BCE/Setup/4o ▶ Network the O'gar Brute (Ogre)", true)]
    static bool Validate() => !Application.isPlaying;

    static bool EnsureComponent<T>(GameObject root, out T component) where T : Component
    {
        component = root.GetComponent<T>();
        if (component != null) return false;

        component = root.AddComponent<T>();
        EditorUtility.SetDirty(component);
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

    static bool EnsureNetworkAnimator(GameObject root)
    {
        var na = root.GetComponent<NetworkAnimator>();
        bool added = false;
        if (na == null)
        {
            na = root.AddComponent<NetworkAnimator>();
            added = true;
        }

        // Animator lives on the imported FBX child, not the variant root.
        if (na.animator == null)
            na.animator = root.GetComponentInChildren<Animator>(true);

        na.clientAuthority = false;

        EditorUtility.SetDirty(na);
        return added;
    }
}
#endif
