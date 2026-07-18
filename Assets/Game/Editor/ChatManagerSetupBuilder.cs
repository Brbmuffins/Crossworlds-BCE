#if UNITY_EDITOR
using System.Collections.Generic;
using System.Text;
using Mirror;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// BCE ▶ Setup ▶ 4p — Persistent Chat Manager.
///
/// Converts chat from a per-scene baked object into a single persistent networked
/// object that survives ServerChangeScene, so it keeps working as players move
/// between world zones (ROADMAP: "chat works online as we move around the world").
///
/// One click does three things:
///   1. Creates/refreshes Assets/Game/Networking/ChatManager.prefab
///      (NetworkIdentity + RodChatManager — the UI is fully procedural, no refs).
///   2. Assigns it to RodNetworkManager.chatManagerPrefab in every OPEN scene that
///      has the manager (it lives in LoginScene).
///   3. Deletes any baked RodChatManager scene object from every OPEN scene (the old
///      Darkwood copy) — a leftover would duplicate the networked relay.
///
/// It operates on currently-open scenes and saves the ones it changes. Run it with
/// BOTH LoginScene (to wire the manager) and Darkwood (to strip the baked object)
/// loaded — additively is fine — or run it once per scene.
/// </summary>
public static class ChatManagerSetupBuilder
{
    const string PrefabPath = "Assets/Game/Networking/ChatManager.prefab";

    [MenuItem("BCE/Setup/4p ▶ Persistent Chat Manager", priority = 9)]
    static void Build()
    {
        var report = new StringBuilder();

        // ── 1. Prefab ──────────────────────────────────────────────────────
        GameObject prefab = EnsurePrefab(report);
        if (prefab == null)
        {
            EditorUtility.DisplayDialog("Persistent Chat Manager", report.ToString(), "OK");
            return;
        }

        // ── 2 + 3. Wire managers + strip baked chat objects in open scenes ──
        int wired = 0, stripped = 0, scenesTouched = 0;
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (!scene.isLoaded) continue;

            bool changed = false;
            var roots = scene.GetRootGameObjects();

            foreach (var root in roots)
            {
                foreach (var nm in root.GetComponentsInChildren<RodNetworkManager>(true))
                {
                    if (nm.chatManagerPrefab != prefab)
                    {
                        nm.chatManagerPrefab = prefab;
                        EditorUtility.SetDirty(nm);
                        wired++;
                        changed = true;
                        report.AppendLine($"  ✓ Wired RodNetworkManager in {scene.name}");
                    }
                }
            }

            // Re-fetch roots — deletions below mutate the hierarchy.
            foreach (var root in scene.GetRootGameObjects())
            {
                foreach (var chat in root.GetComponentsInChildren<RodChatManager>(true))
                {
                    report.AppendLine($"  ✗ Removed baked RodChatManager '{chat.gameObject.name}' from {scene.name}");
                    Object.DestroyImmediate(chat.gameObject);
                    stripped++;
                    changed = true;
                }
            }

            if (changed)
            {
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                scenesTouched++;
            }
        }

        if (wired == 0)
            report.AppendLine("  ⚠ No RodNetworkManager in any open scene — open LoginScene and re-run to wire the reference.");
        if (stripped == 0)
            report.AppendLine("  · No baked RodChatManager found in open scenes (open Darkwood and re-run if one remains).");

        report.AppendLine();
        report.AppendLine($"Saved {scenesTouched} scene(s). Commit the prefab, LoginScene, and Darkwood together.");
        EditorUtility.DisplayDialog("Persistent Chat Manager", report.ToString(), "Done");
    }

    static GameObject EnsurePrefab(StringBuilder report)
    {
        var existing = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        if (existing != null)
        {
            // Guarantee the two required components even if the prefab was hand-edited.
            bool ok = existing.GetComponent<NetworkIdentity>() != null
                   && existing.GetComponent<RodChatManager>() != null;
            if (ok)
            {
                report.AppendLine($"  · Prefab already present: {PrefabPath}");
                return existing;
            }
        }

        var go = new GameObject("ChatManager");
        go.AddComponent<NetworkIdentity>();
        go.AddComponent<RodChatManager>();

        var prefab = PrefabUtility.SaveAsPrefabAsset(go, PrefabPath, out bool success);
        Object.DestroyImmediate(go);

        if (!success || prefab == null)
        {
            report.AppendLine($"  ✗ FAILED to write prefab at {PrefabPath}");
            return null;
        }

        report.AppendLine($"  ✓ Created prefab: {PrefabPath}");
        return prefab;
    }

    [MenuItem("BCE/Setup/4p ▶ Persistent Chat Manager", true)]
    static bool Validate() => !Application.isPlaying;
}
#endif
