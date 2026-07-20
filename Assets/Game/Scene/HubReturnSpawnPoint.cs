using System;
using Mirror;
using UnityEngine;

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
            FindObjectsInactive.Exclude,
            FindObjectsSortMode.None);

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
