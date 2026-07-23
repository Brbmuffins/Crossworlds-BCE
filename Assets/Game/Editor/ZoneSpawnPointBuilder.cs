using System.Collections.Generic;
using System.IO;
using Mirror;                       // NetworkStartPosition
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

// ═══════════════════════════════════════════════════════════════════════════
//  ZoneSpawnPointBuilder — BCE ▶ Setup ▶ 6s
//
//  Every zone needs at least one HubReturnSpawnPoint. ZoneManager resolves an
//  arriving player's position with HubReturnSpawnPoint.FindInScene, which looks
//  for (1) a matching spawnId, (2) any spawn point in that scene, (3) any
//  NetworkStartPosition in that scene. A zone with none of those drops the
//  player at (0, 1, 0) — which for a terrain-based map means under the ground.
//
//  Audit on 2026-07-23 found Darkwood with zero of both.
//
//  This places ONE fallback spawn point per zone that lacks one, on walkable
//  ground. It is a safety net, not level design: drag the created object to the
//  zone's intended entrance afterwards. Re-running never moves an existing point.
//
//  Placement, best method first:
//    1. Raycast down from high above the centre of the zone's rendered bounds.
//    2. Snap that hit to the NavMesh so the player lands somewhere an agent can
//       actually stand.
//    3. If there is no NavMesh, keep the raycast hit.
//    4. If nothing is hit at all, (0, 1, 0) with a loud warning.
// ═══════════════════════════════════════════════════════════════════════════

public static class ZoneSpawnPointBuilder
{
    const float RaycastHeight = 1000f;
    const float NavMeshSnapRadius = 50f;

    /// <summary>
    /// How far above the ground a spawn point should sit. Enough that the player
    /// drops onto the surface instead of starting inside it, without a long fall.
    /// </summary>
    const float GroundClearance = 2f;

    [MenuItem("BCE/Setup/6t - Fix Zone Spawn Heights")]
    public static void FixHeights()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            Debug.Log("[BCE 6t] Cancelled — unsaved scenes were not saved.");
            return;
        }

        var report = new List<string>();

        foreach (string zone in SceneNames.Zones)
        {
            string path = PathForZone(zone);
            if (path == null || !File.Exists(path))
            {
                report.Add($"— {zone}: no scene file, skipped.");
                continue;
            }

            FixHeightsInScene(zone, path, report);
        }

        string summary = string.Join("\n", report);
        Debug.Log("[BCE 6t] Spawn heights:\n" + summary);
        EditorUtility.DisplayDialog("BCE ▶ 6t Fix Zone Spawn Heights", summary, "OK");
    }

    /// <summary>
    /// Drops every spawn point in a zone onto the surface beneath it. 6s only
    /// guarantees a spawn point EXISTS — Ashen Wastelands had one sitting at y=50
    /// on a terrain whose surface is ~56, so players spawned inside the ground.
    /// </summary>
    static void FixHeightsInScene(string zone, string path, List<string> report)
    {
        Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
        Physics.SyncTransforms();

        List<HubReturnSpawnPoint> points = FindInScene<HubReturnSpawnPoint>(scene);
        if (points.Count == 0)
        {
            report.Add($"— {zone}: no spawn points — run 6s first.");
            return;
        }

        bool changed = false;

        foreach (HubReturnSpawnPoint point in points)
        {
            Vector3 current = point.transform.position;

            // Start well above the point so a spawn currently BURIED in terrain still
            // finds the surface — casting from the point itself would start underground
            // and hit nothing.
            var origin = new Vector3(current.x, current.y + RaycastHeight, current.z);

            if (!Physics.Raycast(origin, Vector3.down, out RaycastHit hit, RaycastHeight * 2f))
            {
                report.Add($"⚠ {zone}/{point.name}: nothing beneath it — left at y={current.y:F2}. " +
                           $"Check by hand.");
                continue;
            }

            float targetY = hit.point.y + GroundClearance;
            if (Mathf.Abs(targetY - current.y) < 0.05f)
            {
                report.Add($"✓ {zone}/{point.name}: already on the surface (y={current.y:F2}).");
                continue;
            }

            Undo.RecordObject(point.transform, "Fix spawn height");
            point.transform.position = new Vector3(current.x, targetY, current.z);
            changed = true;

            report.Add($"✚ {zone}/{point.name}: y {current.y:F2} → {targetY:F2} " +
                       $"(surface {hit.point.y:F2} + {GroundClearance} clearance, hit '{hit.collider.name}').");
        }

        if (changed)
        {
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }
    }

    [MenuItem("BCE/Setup/6s - Ensure Zone Spawn Points")]
    public static void Run()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            Debug.Log("[BCE 6s] Cancelled — unsaved scenes were not saved.");
            return;
        }

        var report = new List<string>();

        foreach (string zone in SceneNames.Zones)
        {
            string path = PathForZone(zone);

            if (path == null || !File.Exists(path))
            {
                report.Add($"— {zone}: no scene file, skipped.");
                continue;
            }

            EnsureSpawnPoint(zone, path, report);
        }

        string summary = string.Join("\n", report);
        Debug.Log("[BCE 6s] Zone spawn points:\n" + summary);
        EditorUtility.DisplayDialog("BCE ▶ 6s Zone Spawn Points", summary, "OK");
    }

    static void EnsureSpawnPoint(string zone, string path, List<string> report)
    {
        Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);

        List<HubReturnSpawnPoint> existing = FindInScene<HubReturnSpawnPoint>(scene);

        if (existing.Count > 0)
        {
            report.Add($"✓ {zone}: already has {existing.Count} spawn point(s) — untouched.");
            return;
        }

        List<NetworkStartPosition> starts = FindInScene<NetworkStartPosition>(scene);

        if (starts.Count > 0)
        {
            report.Add($"✓ {zone}: no HubReturnSpawnPoint, but {starts.Count} " +
                       $"NetworkStartPosition(s) exist — FindInScene falls back to those, leaving alone.");
            return;
        }

        Vector3 position = ResolveSpawnPosition(scene, out string method);

        var go = new GameObject($"ZoneSpawn_{zone.Replace(" ", "")}");
        go.transform.position = position;
        var point = go.AddComponent<HubReturnSpawnPoint>();
        point.spawnId = HubReturnSpawnPoint.DefaultSpawnId;

        Undo.RegisterCreatedObjectUndo(go, "Create zone spawn point");
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        report.Add($"✚ {zone}: CREATED '{go.name}' at {position} (via {method}). " +
                   $"Move it to the real entrance.");
    }

    /// <summary>
    /// Components of type T inside one scene, including inactive objects.
    /// Deliberately not Object.FindObjectsByType: that overload is deprecated in
    /// Unity 6, and it searches every loaded scene rather than the one we opened —
    /// the same global-lookup mistake ROADMAP 6.5 exists to clean up.
    /// </summary>
    static List<T> FindInScene<T>(Scene scene) where T : Component
    {
        var results = new List<T>();

        foreach (GameObject root in scene.GetRootGameObjects())
            results.AddRange(root.GetComponentsInChildren<T>(true));

        return results;
    }

    // ── Placement ─────────────────────────────────────────────────────────────

    static Vector3 ResolveSpawnPosition(Scene scene, out string method)
    {
        Vector2 centre = RenderedCentreXZ(scene);

        // Physics queries in edit mode need transforms flushed to the physics scene.
        Physics.SyncTransforms();

        var origin = new Vector3(centre.x, RaycastHeight, centre.y);

        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, RaycastHeight * 2f))
        {
            Vector3 ground = hit.point + Vector3.up * 0.5f;

            if (NavMesh.SamplePosition(ground, out NavMeshHit navHit, NavMeshSnapRadius, NavMesh.AllAreas))
            {
                method = "raycast + NavMesh snap";
                return navHit.position + Vector3.up * 0.5f;
            }

            method = "raycast (no NavMesh found to snap to)";
            return ground;
        }

        if (NavMesh.SamplePosition(new Vector3(centre.x, 0f, centre.y),
                                   out NavMeshHit fallbackHit, NavMeshSnapRadius * 10f, NavMesh.AllAreas))
        {
            method = "NavMesh sample (nothing hit by raycast)";
            return fallbackHit.position + Vector3.up * 0.5f;
        }

        method = "FALLBACK ORIGIN — verify this by hand";
        return Vector3.up;
    }

    /// <summary>
    /// Middle of everything the zone renders. Better than assuming the origin is
    /// inside the playable area — Darkwood spans x[-571..71], so its origin sits
    /// near an edge rather than in the middle of the map.
    /// </summary>
    static Vector2 RenderedCentreXZ(Scene scene)
    {
        bool any = false;
        Bounds bounds = default;

        foreach (GameObject root in scene.GetRootGameObjects())
        {
            foreach (Renderer r in root.GetComponentsInChildren<Renderer>(true))
            {
                if (r == null) continue;

                if (!any) { bounds = r.bounds; any = true; }
                else bounds.Encapsulate(r.bounds);
            }
        }

        return any ? new Vector2(bounds.center.x, bounds.center.z) : Vector2.zero;
    }

    static string PathForZone(string zoneName)
    {
        switch (zoneName)
        {
            case SceneNames.Hub:             return SceneNames.HubPath;
            case SceneNames.Darkwood:        return SceneNames.DarkwoodPath;
            case SceneNames.ToujamBasin:     return SceneNames.ToujamBasinPath;
            case SceneNames.AshenWastelands: return SceneNames.AshenWastelandsPath;
            case SceneNames.GMIsland:        return SceneNames.GMIslandPath;
            case SceneNames.VoidDungeon:     return SceneNames.VoidDungeonPath;
            case SceneNames.GatheringZone:   return SceneNames.GatheringZonePath;
            case SceneNames.ArenaCopper:     return SceneNames.ArenaCopperPath;
            default:                         return null;
        }
    }
}
