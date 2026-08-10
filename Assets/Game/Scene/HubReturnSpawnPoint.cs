using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
[AddComponentMenu("BCE/Scene/Hub Return Spawn Point")]
public class HubReturnSpawnPoint : MonoBehaviour
{
    public const string DefaultSpawnId = "HubReturn";

    [Tooltip("Travel requests with the same id will land players here.")]
    public string spawnId = DefaultSpawnId;

    public static Transform Find(string requestedSpawnId)
    {
        string id = NormalizeId(requestedSpawnId);
        HubReturnSpawnPoint[] points = FindObjectsByType<HubReturnSpawnPoint>(
            FindObjectsInactive.Exclude);

        foreach (HubReturnSpawnPoint point in points)
        {
            if (point == null) continue;
            if (string.Equals(NormalizeId(point.spawnId), id, StringComparison.OrdinalIgnoreCase))
                return point.transform;
        }

        if (points.Length > 0)
            return points[0].transform;

        Transform namedPoint = FindNamedFallback(id);
        if (namedPoint != null)
            return namedPoint;

        NetworkStartPosition startPosition = FindAnyObjectByType<NetworkStartPosition>();
        return startPosition != null ? startPosition.transform : null;
    }

    /// <summary>
    /// Scene-scoped lookup (ROADMAP 6.3/6.5). Find() above searches globally, which is
    /// correct while exactly one zone is loaded and WRONG once zones load additively —
    /// it would happily return Darkwood's spawn point to someone entering Hub.
    ///
    /// Deliberately does NOT fall back to FindNamedFallback: GameObject.Find is global
    /// and would reintroduce the very cross-zone bug this exists to avoid. Returning
    /// null lets the caller log which zone is missing a spawn point.
    /// </summary>
    public static Transform FindInScene(Scene scene, string requestedSpawnId)
    {
        if (!scene.IsValid() || !scene.isLoaded) return null;

        string id = NormalizeId(requestedSpawnId);
        HubReturnSpawnPoint[] points = FindObjectsByType<HubReturnSpawnPoint>(
            FindObjectsInactive.Exclude);

        var matches = new List<HubReturnSpawnPoint>();
        var scenePoints = new List<HubReturnSpawnPoint>();

        foreach (HubReturnSpawnPoint point in points)
        {
            if (point == null) continue;
            if (point.gameObject.scene != scene) continue;

            scenePoints.Add(point);
            if (string.Equals(NormalizeId(point.spawnId), id, StringComparison.OrdinalIgnoreCase))
                matches.Add(point);
        }

        if (matches.Count > 0)
            return ChooseSpawn(scene, matches).transform;

        if (scenePoints.Count > 0)
            return ChooseSpawn(scene, scenePoints).transform;

        foreach (NetworkStartPosition start in FindObjectsByType<NetworkStartPosition>(
                     FindObjectsInactive.Exclude))
        {
            if (start != null && start.gameObject.scene == scene)
                return start.transform;
        }

        return null;
    }

    static HubReturnSpawnPoint ChooseSpawn(Scene scene, List<HubReturnSpawnPoint> points)
    {
        bool randomize = points.Count > 1 &&
            string.Equals(scene.name, SceneNames.PvpZone, StringComparison.OrdinalIgnoreCase);

        return randomize
            ? points[UnityEngine.Random.Range(0, points.Count)]
            : points[0];
    }

    static string NormalizeId(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? DefaultSpawnId : value.Trim();
    }

    static Transform FindNamedFallback(string requestedSpawnId)
    {
        if (!string.IsNullOrWhiteSpace(requestedSpawnId))
        {
            GameObject exactMatch = GameObject.Find(requestedSpawnId);
            if (exactMatch != null)
                return exactMatch.transform;
        }

        string[] names =
        {
            "HubReturnSpawn",
            "Hub Return Spawn",
            "HubWaypoint",
            "Hub Waypoint",
            "Hub",
            "PlayerSpawnPoint",
            "Player Spawn Point",
            "SpawnPoint",
            "SpawnPoint_0"
        };

        foreach (string name in names)
        {
            GameObject found = GameObject.Find(name);
            if (found != null)
                return found.transform;
        }

        return null;
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0.1f, 0.9f, 0.7f, 0.85f);
        Gizmos.DrawSphere(transform.position, 0.35f);
        Gizmos.DrawRay(transform.position, transform.forward * 1.5f);
    }
#endif
}
