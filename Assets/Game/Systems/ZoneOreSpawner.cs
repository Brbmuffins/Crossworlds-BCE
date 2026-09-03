using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Maintains an occupancy-scaled, server-authoritative ore population per zone instance.</summary>
[DisallowMultipleComponent]
public sealed class ZoneOreSpawner : MonoBehaviour
{
    const string PrefabResource = "Gathering/NetworkPrefabs/DynamicMineralVein";

    [Min(1)] public int minimumNodes = 10;
    [Min(0)] public int nodesPerAdditionalPlayer = 2;
    [Min(1)] public int maximumNodes = 30;
    [Min(1f)] public float reconcileSeconds = 15f;
    [Min(1f)] public float minimumNodeSpacing = 5f;
    [Min(0f)] public float minimumPlayerSpacing = 8f;
    [Range(0f, 89f)] public float maximumGroundSlope = 42f;
    [Min(1)] public int placementAttemptsPerNode = 40;

    readonly Dictionary<int, List<GameObject>> nodesByScene = new();
    ZoneManager zoneManager;
    GameObject nodePrefab;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        if (FindAnyObjectByType<ZoneOreSpawner>() != null) return;
        GameObject root = new("Zone Ore Spawner");
        DontDestroyOnLoad(root);
        root.AddComponent<ZoneOreSpawner>();
    }

    IEnumerator Start()
    {
        nodePrefab = Resources.Load<GameObject>(PrefabResource);
        while (zoneManager == null)
        {
            if (NetworkServer.active) zoneManager = ZoneManager.Instance;
            yield return null;
        }

        zoneManager.ZoneLoaded += OnZoneLoaded;
        zoneManager.ZoneOccupancyChanged += OnOccupancyChanged;
        zoneManager.ZoneUnloading += OnZoneUnloading;

        while (true)
        {
            yield return new WaitForSeconds(reconcileSeconds);
            ReconcileAllLoadedZones();
        }
    }

    void OnDestroy()
    {
        if (zoneManager == null) return;
        zoneManager.ZoneLoaded -= OnZoneLoaded;
        zoneManager.ZoneOccupancyChanged -= OnOccupancyChanged;
        zoneManager.ZoneUnloading -= OnZoneUnloading;
    }

    void OnZoneLoaded(Scene scene) => Reconcile(scene, zoneManager.OccupantCount(scene));
    void OnOccupancyChanged(Scene scene, int occupants) => Reconcile(scene, occupants);

    void OnZoneUnloading(Scene scene)
    {
        if (!nodesByScene.TryGetValue(scene.handle, out List<GameObject> nodes)) return;
        for (int i = nodes.Count - 1; i >= 0; i--)
            if (nodes[i] != null) NetworkServer.Destroy(nodes[i]);
        nodesByScene.Remove(scene.handle);
    }

    void ReconcileAllLoadedZones()
    {
        if (!NetworkServer.active || zoneManager == null) return;
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            int occupants = zoneManager.OccupantCount(scene);
            if (occupants > 0) Reconcile(scene, occupants);
        }
    }

    void Reconcile(Scene scene, int occupants)
    {
        if (!NetworkServer.active || nodePrefab == null || !Eligible(scene) || occupants <= 0) return;
        if (!nodesByScene.TryGetValue(scene.handle, out List<GameObject> nodes))
            nodesByScene[scene.handle] = nodes = new List<GameObject>();
        nodes.RemoveAll(node => node == null);

        int target = Mathf.Clamp(minimumNodes + Mathf.Max(0, occupants - 1) * nodesPerAdditionalPlayer,
            minimumNodes, maximumNodes);
        while (nodes.Count < target)
        {
            if (!TryFindPosition(scene, nodes, out Vector3 position))
            {
                Debug.LogWarning($"[ORE SPAWNER] Could place only {nodes.Count}/{target} nodes in " +
                                 $"'{scene.name}' handle={scene.handle}. Check its OreSpawnVolumes.");
                break;
            }

            GameObject node = Instantiate(nodePrefab, position, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f));
            node.name = $"Dynamic Mineral Vein {nodes.Count + 1}";
            SceneManager.MoveGameObjectToScene(node, scene);
            GatheringNodeNetworkState state = node.GetComponent<GatheringNodeNetworkState>();
            if (state != null) state.persistentNodeId = $"dynamic:{scene.name}:{scene.handle}:{nodes.Count + 1}";
            NetworkServer.Spawn(node);
            nodes.Add(node);
        }

        // Extra nodes retire only once depleted, avoiding a node disappearing while mined.
        for (int i = nodes.Count - 1; i >= target; i--)
        {
            GameObject node = nodes[i];
            GatheringNodeNetworkState state = node != null ? node.GetComponent<GatheringNodeNetworkState>() : null;
            if (state != null && !state.IsDepleted) continue;
            if (node != null) NetworkServer.Destroy(node);
            nodes.RemoveAt(i);
        }
    }

    bool TryFindPosition(Scene scene, List<GameObject> existing, out Vector3 position)
    {
        position = default;
        OreSpawnVolume[] volumes = FindVolumes(scene);
        if (volumes.Length == 0) return false;
        PhysicsScene physics = scene.GetPhysicsScene();
        for (int attempt = 0; attempt < placementAttemptsPerNode; attempt++)
        {
            OreSpawnVolume volume = volumes[Random.Range(0, volumes.Length)];
            if (!physics.Raycast(volume.RandomRayOrigin(), Vector3.down, out RaycastHit hit,
                    volume.RayDistance, ~0, QueryTriggerInteraction.Ignore)) continue;
            if (Vector3.Angle(hit.normal, Vector3.up) > maximumGroundSlope) continue;
            if (TooClose(hit.point, existing, minimumNodeSpacing)) continue;
            if (TooCloseToPlayers(scene, hit.point, minimumPlayerSpacing)) continue;
            position = hit.point;
            return true;
        }
        return false;
    }

    static OreSpawnVolume[] FindVolumes(Scene scene)
    {
        List<OreSpawnVolume> result = new();
        foreach (GameObject root in scene.GetRootGameObjects())
            result.AddRange(root.GetComponentsInChildren<OreSpawnVolume>(true));
        return result.ToArray();
    }

    static bool TooClose(Vector3 point, List<GameObject> nodes, float distance)
    {
        float square = distance * distance;
        foreach (GameObject node in nodes)
            if (node != null && (node.transform.position - point).sqrMagnitude < square) return true;
        return false;
    }

    static bool TooCloseToPlayers(Scene scene, Vector3 point, float distance)
    {
        float square = distance * distance;
        foreach (NetworkConnectionToClient connection in NetworkServer.connections.Values)
        {
            GameObject player = connection?.identity?.gameObject;
            if (player != null && player.scene.handle == scene.handle &&
                (player.transform.position - point).sqrMagnitude < square) return true;
        }
        return false;
    }

    static bool Eligible(Scene scene) => scene.IsValid() && scene.isLoaded && SceneNames.IsZone(scene.name) &&
        !string.Equals(scene.name, SceneNames.Hub, System.StringComparison.OrdinalIgnoreCase);
}
