using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

// ═══════════════════════════════════════════════════════════════════════════
//  ZoneManager — server-side multi-zone world (ROADMAP 6.3)
//
//  WHAT THIS REPLACES
//  ServerChangeScene moves EVERY connected player, so one person walking a
//  waypoint dragged the whole server into Darkwood. PortalTransition worked
//  around that with a client-local SceneManager.LoadScene that never told the
//  server, leaving the client in the arena while the server still had that
//  player's identity in Hub.
//
//  HOW IT WORKS
//  The active scene is an empty container holding only the NetworkManager.
//  Every zone — Hub included — is loaded additively on top of it, server-side,
//  on demand. A player is filed into their zone with MoveGameObjectToScene
//  (server only) and told to load it with a per-connection SceneMessage.
//  SceneDistanceInterestManagement (6.6) then scopes observers by scene.
//
//  SHARED vs INSTANCED (owner decision, ROADMAP open question 8)
//  Open zones are one shared copy. Dungeons and arenas get one copy per party.
//  Because instanced copies share a scene NAME, everything here keys on the
//  scene HANDLE — a name-keyed ref-count would merge two parties' dungeons into
//  one entry and unload a scene somebody is still standing in.
//
//  CLIENT-SIDE NOTE
//  Mirror instantiates client-side spawned objects into the client's ACTIVE
//  scene, so on a client the additive zone scenes hold only level geometry.
//  Unloading one cannot destroy networked objects — those are removed by Mirror
//  when the client stops observing them. That is why the unload can safely
//  follow the move.
//
//  HOST MODE CAVEAT
//  NetworkManager.ClientChangeScene returns early when NetworkServer.active, so
//  a host client does not process its own SceneMessages — it already has every
//  zone loaded because the server loaded them. Interest management still filters
//  what it observes, but a host may render neighbouring zones' geometry. The
//  dedicated server is the production path; this only affects in-editor hosting.
// ═══════════════════════════════════════════════════════════════════════════

[AddComponentMenu("BCE/Network/Zone Manager")]
public class ZoneManager : MonoBehaviour
{
    public event System.Action<Scene> ZoneLoaded;
    public event System.Action<Scene, int> ZoneOccupancyChanged;
    public event System.Action<Scene> ZoneUnloading;
    const float ArrivalMinimumSpacing = 1.1f;
    const float ArrivalGroundSearchHeight = 3f;
    const float ArrivalGroundSearchDistance = 7f;
    const float ArrivalMaximumGroundDelta = 2.5f;
    const float ArrivalMaximumSlope = 50f;
    const int ArrivalRingCount = 6;

    public static ZoneManager Instance { get; private set; }

    [Tooltip("Zones that get one copy per party instead of one shared copy.")]
    public string[] instancedZones =
    {
        SceneNames.VoidDungeon,
        SceneNames.ArenaCopper,
    };

    [Tooltip("Seconds an empty zone stays loaded before unloading. Stops a player " +
             "portalling out and straight back from thrashing a scene load.")]
    public float unloadDelaySeconds = 30f;

    // scene handle → connection ids currently in it
    readonly Dictionary<SceneHandle, HashSet<int>> _occupants = new Dictionary<SceneHandle, HashSet<int>>();
    // scene handle → the Scene itself (handles are ints; we need the struct back)
    readonly Dictionary<SceneHandle, Scene> _scenesByHandle = new Dictionary<SceneHandle, Scene>();
    // connection id → scene handle it is currently in
    readonly Dictionary<int, SceneHandle> _connZone = new Dictionary<int, SceneHandle>();
    // shared zone name → scene handle (instanced zones deliberately absent)
    readonly Dictionary<string, SceneHandle> _sharedZones = new Dictionary<string, SceneHandle>();
    // instance key ("zone:party") → scene handle, so a party lands in one copy
    readonly Dictionary<string, SceneHandle> _instances = new Dictionary<string, SceneHandle>();
    // scene handles with a pending unload, so we can cancel on re-entry
    readonly Dictionary<SceneHandle, Coroutine> _pendingUnloads = new Dictionary<SceneHandle, Coroutine>();
    // zones that own a physics scene we must step ourselves — see FixedUpdate
    readonly List<Scene> _simulated = new List<Scene>();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    /// <summary>
    /// Steps each zone's physics scene.
    ///
    /// Unity auto-simulates ONLY the default physics scene. A scene loaded with
    /// localPhysicsMode gets its own PhysicsScene that nothing advances unless we
    /// do it here — so every Rigidbody in a zone would sit frozen: no gravity, no
    /// collision, and MovePosition silently doing nothing. That is exactly what
    /// happened the first time a player spawned into Hub and could not move.
    ///
    /// (Mirror's MultipleAdditiveScenes example passes localPhysicsMode and never
    /// simulates, which is where the omission was copied from.)
    ///
    /// NavMeshAgent movement is unaffected either way — it is not physics — which
    /// is why enemies kept moving while the player could not.
    /// </summary>
    void FixedUpdate()
    {
        if (!NetworkServer.active) return;

        for (int i = _simulated.Count - 1; i >= 0; i--)
        {
            Scene scene = _simulated[i];

            if (!scene.IsValid() || !scene.isLoaded)
            {
                _simulated.RemoveAt(i);
                continue;
            }

            PhysicsScene physics = scene.GetPhysicsScene();
            if (physics.IsValid()) physics.Simulate(Time.fixedDeltaTime);
        }
    }

    public bool IsInstanced(string zoneName)
    {
        if (instancedZones == null) return false;
        foreach (string z in instancedZones)
            if (string.Equals(z, zoneName, System.StringComparison.OrdinalIgnoreCase))
                return true;
        return false;
    }

    // ── Entry point ───────────────────────────────────────────────────────────

    /// <summary>
    /// Moves one player into a zone, loading it if needed. Nobody else is affected.
    /// <paramref name="instanceKey"/> groups players into the same copy of an
    /// instanced zone — pass a party id once parties exist; null gives the caller
    /// their own private instance. Ignored for shared zones.
    /// </summary>
    // NOTE: no [Server] attributes anywhere in this class. ZoneManager is a plain
    // MonoBehaviour, and Mirror's weaver rejects [Server]/[Client] outside a
    // NetworkBehaviour. Server-only entry points guard on NetworkServer.active.
    public void MovePlayerToZone(NetworkConnectionToClient conn, string zoneName,
                                 string spawnId = null, string instanceKey = null)
    {
        if (!NetworkServer.active) return;
        if (conn == null) return;

        zoneName = SceneNames.NormalizeZone(zoneName);
        conn.identity?.GetComponent<PlayerIdentity>()?.ServerBeginZoneVisualLoad();
        RodChatManager.Instance?.TargetBeginZoneTravel(conn, zoneName);
        StartCoroutine(MovePlayerRoutine(conn, zoneName, spawnId, instanceKey));
    }

    /// <summary>
    /// Loads the zone if needed and tells this one client to load it additively.
    /// Public so the initial player spawn (RodNetworkManager) can use the same path
    /// as a later zone change. Yields an invalid Scene on failure.
    /// </summary>
    public IEnumerator PrepareZone(NetworkConnectionToClient conn, string zoneName,
                                   string instanceKey, System.Action<Scene> onReady)
    {
        zoneName = SceneNames.NormalizeZone(zoneName);

        Scene destination = default;
        yield return AcquireZone(zoneName, instanceKey, conn, s => destination = s);

        // Falling back to Hub beats stranding the player with no scene at all — which
        // is what an unbuilt or misspelled destination would otherwise do, and at login
        // it would mean they simply never spawn.
        if ((!destination.IsValid() || !destination.isLoaded) && zoneName != SceneNames.Hub)
        {
            Debug.LogError($"[Zone] Could not acquire '{zoneName}' for conn {conn.connectionId} — " +
                           $"falling back to {SceneNames.Hub}.");
            zoneName = SceneNames.Hub;
            yield return AcquireZone(zoneName, null, conn, s => destination = s);
        }

        if (!destination.IsValid() || !destination.isLoaded)
        {
            Debug.LogError($"[Zone] Could not acquire '{zoneName}' for conn {conn.connectionId}.");
            onReady(default);
            yield break;
        }

        // Player may have disconnected during the async load.
        if (!NetworkServer.connections.ContainsKey(conn.connectionId))
        {
            ReleaseZone(destination.handle);
            onReady(default);
            yield break;
        }

        // Tell THIS client to load the destination additively, and give it a frame
        // to start. Mirror pauses client message processing during the load and
        // resumes after, so spawn traffic queues rather than being lost.
        // Deliberately `yield return null` and not WaitForEndOfFrame: the latter can
        // fail to resume in headless batchmode, which is the production path.
        conn.Send(new SceneMessage
        {
            sceneName      = zoneName,
            sceneOperation = SceneOperation.LoadAdditive
        });
        yield return null;

        onReady(destination);
    }

    /// <summary>Records a freshly spawned player as an occupant of its starting zone.</summary>
    public void RegisterInitialPlacement(NetworkConnectionToClient conn, Scene scene)
    {
        if (conn == null || !scene.IsValid()) return;
        AssignOccupant(conn.connectionId, scene);
    }

    IEnumerator MovePlayerRoutine(NetworkConnectionToClient conn, string zoneName,
                                  string spawnId, string instanceKey)
    {
        SceneHandle previousHandle = _connZone.TryGetValue(conn.connectionId, out SceneHandle prev) ? prev : default;

        // LoadingScreen.Show starts this fade on the traveling client. Give it time
        // to finish before initiating additive loading so the source scene's music
        // never carries into the destination load.
        yield return new WaitForSeconds(MusicController.TravelFadeOutSeconds);

        if (conn == null || !NetworkServer.connections.ContainsKey(conn.connectionId))
            yield break;

        Scene destination = default;
        yield return PrepareZone(conn, zoneName, instanceKey, s => destination = s);
        if (!destination.IsValid())
        {
            conn.identity?.GetComponent<PlayerIdentity>()?.ServerCompleteZoneVisualLoad();
            RodChatManager.Instance?.TargetCompleteZoneTravel(conn, zoneName);
            yield break;
        }

        // File the player into the destination scene, server-side only. This is
        // what interest management keys off.
        if (conn.identity != null)
        {
            GameObject player = conn.identity.gameObject;

            // Persist before the move so the DB never holds the new zone paired
            // with the old zone's coordinates (ROADMAP 6.2).
            var saver = player.GetComponent<RodPositionSaver>();
            if (saver != null) saver.SaveNow();

            ZoneScene.PlaceIn(player, destination);
            PlaceAtSpawnPoint(player, destination, spawnId);

#if UNITY_EDITOR || !UNITY_SERVER
            // Host mode shares the server's additive scenes with its local client.
            // Apply the destination camera, skybox and lighting in this same frame,
            // before the newly placed player can be rendered with the old zone's
            // environment. Remote clients refresh from scene load/unload callbacks.
            if (conn.identity.isLocalPlayer)
            {
                ZoneCameraDirector.RefreshNow();
                LoadingScreen.NotifyEnvironmentReady();
            }
#endif
        }

        // Book-keeping. RemoveOccupant must run BEFORE AssignOccupant or the
        // player stays counted in their old zone forever and it never unloads.
        if (previousHandle != default && previousHandle != destination.handle)
        {
            // Grab the name before RemoveOccupant can schedule the scene's removal.
            string previousName =
                _scenesByHandle.TryGetValue(previousHandle, out Scene old) && old.IsValid()
                    ? old.name : null;

            RemoveOccupant(conn.connectionId);

            if (previousName != null)
                conn.Send(new SceneMessage
                {
                    sceneName      = previousName,
                    sceneOperation = SceneOperation.UnloadAdditive
                });
        }

        AssignOccupant(conn.connectionId, destination);

        // Explicit completion is required for same-zone moves and cached additive
        // zones, where the client may receive no sceneLoaded callback.
        RodChatManager.Instance?.TargetCompleteZoneTravel(conn, zoneName);

        Debug.Log($"[Zone] conn {conn.connectionId} → {zoneName} (handle {destination.handle}).");
    }

    // ── Scene acquisition ─────────────────────────────────────────────────────

    IEnumerator AcquireZone(string zoneName, string instanceKey,
                            NetworkConnectionToClient conn, System.Action<Scene> onReady)
    {
        bool instanced = IsInstanced(zoneName);
        string key = instanced
            ? $"{zoneName}:{instanceKey ?? conn.connectionId.ToString()}"
            : null;

        // Reuse an existing copy when one applies.
        SceneHandle existing = default;
        if (!instanced && _sharedZones.TryGetValue(zoneName, out SceneHandle sharedHandle))
            existing = sharedHandle;
        else if (instanced && _instances.TryGetValue(key, out SceneHandle instHandle))
            existing = instHandle;

        if (existing != default && _scenesByHandle.TryGetValue(existing, out Scene reuse)
            && reuse.IsValid() && reuse.isLoaded)
        {
            CancelPendingUnload(existing);
            onReady(reuse);
            yield break;
        }

        // Not every name in SceneNames.Zones has a scene file yet (Arena_Copper and
        // Gathering Zone are still unbuilt). Without this check a character whose saved
        // zone is one of them would fail to spawn at all — caught before it can strand
        // anyone, rather than after.
        if (!Application.CanStreamedLevelBeLoaded(zoneName))
        {
            Debug.LogError($"[Zone] '{zoneName}' is not in Build Settings — cannot load.");
            onReady(default);
            yield break;
        }

        // Load a fresh copy. Physics3D keeps zones from colliding with each other
        // until the world-space offsets in 6.7 land.
        AsyncOperation load = SceneManager.LoadSceneAsync(zoneName, new LoadSceneParameters
        {
            loadSceneMode    = LoadSceneMode.Additive,
            localPhysicsMode = LocalPhysicsMode.Physics3D
        });

        if (load == null)
        {
            Debug.LogError($"[Zone] LoadSceneAsync returned null for '{zoneName}'.");
            onReady(default);
            yield break;
        }

        yield return load;

        // The scene just loaded is the last one in the list.
        Scene loaded = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
        if (!loaded.IsValid())
        {
            Debug.LogError($"[Zone] '{zoneName}' loaded but the Scene handle is invalid.");
            onReady(default);
            yield break;
        }

        _scenesByHandle[loaded.handle] = loaded;
        if (instanced) _instances[key] = loaded.handle;
        else           _sharedZones[zoneName] = loaded.handle;

        // Register for manual physics stepping. Compared against the default scene so
        // that if the LocalPhysicsMode above is ever dropped, we don't double-simulate.
        PhysicsScene physics = loaded.GetPhysicsScene();
        if (physics.IsValid() && physics != Physics.defaultPhysicsScene)
            _simulated.Add(loaded);

        // Mirror only auto-spawns scene objects for the initial scene load, so an
        // additively-loaded zone's baked NetworkIdentities need spawning explicitly.
        NetworkServer.SpawnObjects();
        ZoneLoaded?.Invoke(loaded);

        Debug.Log($"[Zone] Loaded '{zoneName}' (handle {loaded.handle}, instanced={instanced}).");
        onReady(loaded);
    }

    // ── Occupancy + unloading ─────────────────────────────────────────────────

    void AssignOccupant(int connId, Scene scene)
    {
        if (!_occupants.TryGetValue(scene.handle, out HashSet<int> set))
        {
            set = new HashSet<int>();
            _occupants[scene.handle] = set;
        }

        set.Add(connId);
        _connZone[connId] = scene.handle;
        _scenesByHandle[scene.handle] = scene;
        CancelPendingUnload(scene.handle);
        ZoneOccupancyChanged?.Invoke(scene, set.Count);
    }

    /// <summary>Schedules an unload if the zone is empty. Safe to call repeatedly.</summary>
    void ReleaseZone(SceneHandle handle)
    {
        if (_occupants.TryGetValue(handle, out HashSet<int> set) && set.Count > 0)
            return;   // still occupied — nothing to do

        if (_pendingUnloads.ContainsKey(handle)) return;
        _pendingUnloads[handle] = StartCoroutine(UnloadAfterDelay(handle));
    }

    void RemoveOccupant(int connId)
    {
        if (!_connZone.TryGetValue(connId, out SceneHandle handle)) return;
        _connZone.Remove(connId);

        if (_occupants.TryGetValue(handle, out HashSet<int> set))
        {
            set.Remove(connId);
            if (_scenesByHandle.TryGetValue(handle, out Scene scene) && scene.IsValid())
                ZoneOccupancyChanged?.Invoke(scene, set.Count);
            if (set.Count == 0) ReleaseZone(handle);
        }
    }

    void CancelPendingUnload(SceneHandle handle)
    {
        if (!_pendingUnloads.TryGetValue(handle, out Coroutine routine)) return;
        if (routine != null) StopCoroutine(routine);
        _pendingUnloads.Remove(handle);
    }

    IEnumerator UnloadAfterDelay(SceneHandle handle)
    {
        yield return new WaitForSeconds(unloadDelaySeconds);

        _pendingUnloads.Remove(handle);

        // Someone may have walked back in while we waited.
        if (_occupants.TryGetValue(handle, out HashSet<int> set) && set.Count > 0)
            yield break;

        if (!_scenesByHandle.TryGetValue(handle, out Scene scene) || !scene.IsValid() || !scene.isLoaded)
        {
            Forget(handle);
            yield break;
        }

        string zoneName = scene.name;
        ZoneUnloading?.Invoke(scene);
        Forget(handle);

        Debug.Log($"[Zone] Unloading empty '{zoneName}' (handle {handle}).");
        yield return SceneManager.UnloadSceneAsync(scene);
    }

    public int OccupantCount(Scene scene)
    {
        return scene.IsValid() && _occupants.TryGetValue(scene.handle, out HashSet<int> set)
            ? set.Count
            : 0;
    }

    void Forget(SceneHandle handle)
    {
        _occupants.Remove(handle);
        _scenesByHandle.Remove(handle);

        for (int i = _simulated.Count - 1; i >= 0; i--)
            if (_simulated[i].handle == handle) _simulated.RemoveAt(i);

        // Collect keys first — removing during a foreach over the dictionary throws.
        RemoveByValue(_sharedZones, handle);
        RemoveByValue(_instances, handle);
    }

    static void RemoveByValue(Dictionary<string, SceneHandle> map, SceneHandle handle)
    {
        string found = null;
        foreach (var kvp in map)
            if (kvp.Value == handle) { found = kvp.Key; break; }

        if (found != null) map.Remove(found);
    }

    // ── Spawn placement ───────────────────────────────────────────────────────

    /// <summary>
    /// Positions a player at a named spawn point inside the destination scene.
    /// Falls back to the scene's first HubReturnSpawnPoint, then to the origin.
    /// </summary>
    public void PlaceAtSpawnPoint(GameObject player, Scene scene, string spawnId)
    {
        Transform target = HubReturnSpawnPoint.FindInScene(scene, spawnId);
        Vector3 position = target != null ? target.position : Vector3.up;
        Quaternion rotation = target != null ? target.rotation : Quaternion.identity;

        if (target == null)
            Debug.LogWarning($"[Zone] No spawn point '{spawnId}' in '{scene.name}' — using origin.");

        PlaceAtSafePosition(player, scene, position, rotation);
    }

    /// <summary>
    /// Keeps an exact arrival position when it is free. If another player already
    /// occupies it, selects a grounded, capsule-clear slot on expanding rings. This
    /// runs on the server after the traveler is filed into the destination scene, so
    /// simultaneous arrivals are resolved sequentially in that zone's physics scene.
    /// </summary>
    public void PlaceAtSafePosition(GameObject player, Scene scene, Vector3 desiredPosition,
                                    Quaternion rotation)
    {
        if (player == null || !scene.IsValid()) return;

        Physics.SyncTransforms();
        Vector3 position = ResolveArrivalPosition(player, desiredPosition, rotation);

        var controller = player.GetComponent<CharacterController>();
        if (controller != null) controller.enabled = false;

        var body = player.GetComponent<Rigidbody>();
        if (body != null)
        {
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            body.position = position;
            body.rotation = rotation;
        }

        player.transform.SetPositionAndRotation(position, rotation);

        if (controller != null) controller.enabled = true;

        Physics.SyncTransforms();

        // Without ServerTeleport the NetworkTransform treats a cross-zone jump as
        // ordinary movement and interpolates the player across the whole map —
        // visible as the character streaking through the world on every client.
        //
        // Only valid once the object is actually spawned: ServerTeleport sends a
        // ClientRpc, and the initial-spawn path calls this BEFORE
        // AddPlayerForConnection. There the plain transform write above is already
        // correct — Mirror ships the starting pose in the spawn message.
        var identity = player.GetComponent<NetworkIdentity>();
        if (identity == null || identity.netId == 0) return;

        var networkTransform = player.GetComponent<NetworkTransformBase>();
        if (networkTransform != null)
            networkTransform.ServerTeleport(position, rotation);
    }

    Vector3 ResolveArrivalPosition(GameObject player, Vector3 desiredPosition, Quaternion rotation)
    {
        if (!IsOccupiedByAnotherPlayer(player, desiredPosition, rotation))
            return desiredPosition;

        float spacing = Mathf.Max(ArrivalMinimumSpacing, PlayerCapsuleRadius(player) * 2f + 0.35f);
        float phase = Mathf.Abs(player.GetInstanceID() % 16) * (Mathf.PI * 2f / 16f);
        Vector3 fallback = desiredPosition;
        int fallbackCrowding = int.MaxValue;

        for (int ring = 1; ring <= ArrivalRingCount; ring++)
        {
            int slots = ring * 8;
            float radius = ring * spacing;

            for (int slot = 0; slot < slots; slot++)
            {
                float angle = phase + slot * (Mathf.PI * 2f / slots);
                Vector3 candidate = desiredPosition + new Vector3(
                    Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);

                if (!TryProjectArrivalToGround(player, desiredPosition.y, candidate,
                        out Vector3 grounded, out Collider groundCollider))
                    continue;

                if (!IsWorldSpaceClear(player, grounded, rotation, groundCollider))
                    continue;

                int crowding = CountNearbyPlayers(player, grounded, rotation);
                if (crowding == 0)
                {
                    Debug.Log($"[Zone] Arrival at {desiredPosition} occupied; placed " +
                              $"{player.name} in open slot {grounded}.");
                    return grounded;
                }

                if (crowding < fallbackCrowding)
                {
                    fallbackCrowding = crowding;
                    fallback = grounded;
                }
            }
        }

        Debug.LogWarning($"[Zone] No completely open arrival slot near {desiredPosition} for " +
                         $"{player.name}; using least-crowded grounded slot {fallback}.");
        return fallback;
    }

    static bool TryProjectArrivalToGround(GameObject player, float desiredY, Vector3 candidate,
                                          out Vector3 grounded, out Collider groundCollider)
    {
        grounded = candidate;
        groundCollider = null;
        float bestDelta = float.MaxValue;

        RaycastHit[] hits = ZonePhysics.RaycastAll(
            player, candidate + Vector3.up * ArrivalGroundSearchHeight, Vector3.down,
            ArrivalGroundSearchDistance, Physics.AllLayers, QueryTriggerInteraction.Ignore);

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider == null || IsOwnCollider(player, hit.collider)) continue;
            if (IsPlayerCollider(hit.collider)) continue;
            if (Vector3.Angle(hit.normal, Vector3.up) > ArrivalMaximumSlope) continue;

            float delta = Mathf.Abs(hit.point.y - desiredY);
            if (delta > ArrivalMaximumGroundDelta || delta >= bestDelta) continue;

            bestDelta = delta;
            grounded = new Vector3(candidate.x, hit.point.y, candidate.z);
            groundCollider = hit.collider;
        }

        return groundCollider != null;
    }

    static bool IsOccupiedByAnotherPlayer(GameObject player, Vector3 position, Quaternion rotation)
    {
        return CountNearbyPlayers(player, position, rotation) > 0;
    }

    static int CountNearbyPlayers(GameObject player, Vector3 position, Quaternion rotation)
    {
        GetCapsule(player, position, rotation, 0.15f,
            out Vector3 point0, out Vector3 point1, out float radius);

        Collider[] overlaps = ZonePhysics.OverlapCapsule(
            player, point0, point1, radius, Physics.AllLayers,
            QueryTriggerInteraction.Ignore);

        var roots = new HashSet<Transform>();
        foreach (Collider overlap in overlaps)
        {
            if (overlap == null || IsOwnCollider(player, overlap) || !IsPlayerCollider(overlap))
                continue;
            roots.Add(overlap.transform.root);
        }
        return roots.Count;
    }

    static bool IsWorldSpaceClear(GameObject player, Vector3 position, Quaternion rotation,
                                  Collider groundCollider)
    {
        GetCapsule(player, position + Vector3.up * 0.05f, rotation, -0.03f,
            out Vector3 point0, out Vector3 point1, out float radius);

        Collider[] overlaps = ZonePhysics.OverlapCapsule(
            player, point0, point1, radius, Physics.AllLayers,
            QueryTriggerInteraction.Ignore);

        foreach (Collider overlap in overlaps)
        {
            if (overlap == null || overlap == groundCollider || IsOwnCollider(player, overlap))
                continue;
            // Player occupancy is scored separately so a least-crowded grounded
            // fallback remains available if every slot is busy.
            if (IsPlayerCollider(overlap))
                continue;
            return false;
        }
        return true;
    }

    static void GetCapsule(GameObject player, Vector3 position, Quaternion rotation,
                           float radiusPadding, out Vector3 point0, out Vector3 point1,
                           out float radius)
    {
        CapsuleCollider capsule = player.GetComponent<CapsuleCollider>();
        if (capsule == null)
        {
            radius = Mathf.Max(0.1f, 0.35f + radiusPadding);
            point0 = position + Vector3.up * radius;
            point1 = position + Vector3.up * (1.8f - radius);
            return;
        }

        Vector3 scale = player.transform.lossyScale;
        float axisScale = Mathf.Abs(scale.y);
        float radialScale = Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.z));
        radius = Mathf.Max(0.05f, capsule.radius * radialScale + radiusPadding);
        float halfHeight = Mathf.Max(radius, capsule.height * axisScale * 0.5f);
        Vector3 center = position + rotation * Vector3.Scale(capsule.center, scale);
        Vector3 axis = rotation * Vector3.up;
        float segment = Mathf.Max(0f, halfHeight - radius);
        point0 = center - axis * segment;
        point1 = center + axis * segment;
    }

    static float PlayerCapsuleRadius(GameObject player)
    {
        CapsuleCollider capsule = player.GetComponent<CapsuleCollider>();
        if (capsule == null) return 0.35f;
        Vector3 scale = player.transform.lossyScale;
        return capsule.radius * Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.z));
    }

    static bool IsOwnCollider(GameObject player, Collider collider)
    {
        Transform transform = collider.transform;
        return transform == player.transform || transform.IsChildOf(player.transform);
    }

    static bool IsPlayerCollider(Collider collider)
    {
        return collider.GetComponentInParent<PlayerIdentity>() != null
               || collider.transform.root.CompareTag("Player");
    }

    // ── Connection lifecycle ──────────────────────────────────────────────────

    /// <summary>
    /// Tears down every loaded zone. Call when the server stops.
    ///
    /// Without this, additively-loaded zones stay loaded after shutdown with their
    /// networked scene objects still in them. NetworkIdentity.isServer is a stored
    /// flag rather than a live read of NetworkServer.active, so those objects still
    /// believe they are server-side: NetworkAnimator keeps ticking and throws
    /// "RPC ... called without an active server" every FixedUpdate. On a dedicated
    /// server a stop/start cycle would also leak the old scenes.
    /// </summary>
    public void UnloadAllZones()
    {
        StopAllCoroutines();
        _pendingUnloads.Clear();

        // Snapshot: unloading mutates the dictionary.
        var scenes = new List<Scene>(_scenesByHandle.Values);

        _occupants.Clear();
        _scenesByHandle.Clear();
        _connZone.Clear();
        _sharedZones.Clear();
        _instances.Clear();
        _simulated.Clear();

        // Unload synchronously-requested but async-completed; during application quit
        // Unity refuses scene unloads, so don't fight it — the process is going away.
        if (!Application.isPlaying) return;

        foreach (Scene scene in scenes)
        {
            if (!scene.IsValid() || !scene.isLoaded) continue;

            Debug.Log($"[Zone] Server stopping — unloading '{scene.name}'.");
            SceneManager.UnloadSceneAsync(scene);
        }
    }

    public void OnPlayerDisconnected(NetworkConnectionToClient conn)
    {
        if (conn == null) return;
        RemoveOccupant(conn.connectionId);
    }

    /// <summary>
    /// How many players share this connection's zone, including itself. 0 if unplaced.
    /// Callers use this to answer "am I the last one out?" before tearing down
    /// zone-wide state such as an arena run.
    /// </summary>
    public int OccupantCount(NetworkConnectionToClient conn)
    {
        if (conn == null) return 0;
        if (!_connZone.TryGetValue(conn.connectionId, out SceneHandle handle)) return 0;
        return _occupants.TryGetValue(handle, out HashSet<int> set) ? set.Count : 0;
    }

    /// <summary>Zone a connection is currently in, or null if it has not been placed.</summary>
    public Scene? ZoneOf(NetworkConnectionToClient conn)
    {
        if (conn == null) return null;
        if (!_connZone.TryGetValue(conn.connectionId, out SceneHandle handle)) return null;
        if (!_scenesByHandle.TryGetValue(handle, out Scene scene)) return null;
        return scene;
    }
}
