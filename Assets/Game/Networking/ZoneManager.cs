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
    readonly Dictionary<int, HashSet<int>> _occupants = new Dictionary<int, HashSet<int>>();
    // scene handle → the Scene itself (handles are ints; we need the struct back)
    readonly Dictionary<int, Scene> _scenesByHandle = new Dictionary<int, Scene>();
    // connection id → scene handle it is currently in
    readonly Dictionary<int, int> _connZone = new Dictionary<int, int>();
    // shared zone name → scene handle (instanced zones deliberately absent)
    readonly Dictionary<string, int> _sharedZones = new Dictionary<string, int>();
    // instance key ("zone:party") → scene handle, so a party lands in one copy
    readonly Dictionary<string, int> _instances = new Dictionary<string, int>();
    // scene handles with a pending unload, so we can cancel on re-entry
    readonly Dictionary<int, Coroutine> _pendingUnloads = new Dictionary<int, Coroutine>();
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
        int previousHandle = _connZone.TryGetValue(conn.connectionId, out int prev) ? prev : -1;

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
        if (previousHandle != -1 && previousHandle != destination.handle)
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
        int existing = -1;
        if (!instanced && _sharedZones.TryGetValue(zoneName, out int sharedHandle))
            existing = sharedHandle;
        else if (instanced && _instances.TryGetValue(key, out int instHandle))
            existing = instHandle;

        if (existing != -1 && _scenesByHandle.TryGetValue(existing, out Scene reuse)
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
    }

    /// <summary>Schedules an unload if the zone is empty. Safe to call repeatedly.</summary>
    void ReleaseZone(int handle)
    {
        if (_occupants.TryGetValue(handle, out HashSet<int> set) && set.Count > 0)
            return;   // still occupied — nothing to do

        if (_pendingUnloads.ContainsKey(handle)) return;
        _pendingUnloads[handle] = StartCoroutine(UnloadAfterDelay(handle));
    }

    void RemoveOccupant(int connId)
    {
        if (!_connZone.TryGetValue(connId, out int handle)) return;
        _connZone.Remove(connId);

        if (_occupants.TryGetValue(handle, out HashSet<int> set))
        {
            set.Remove(connId);
            if (set.Count == 0) ReleaseZone(handle);
        }
    }

    void CancelPendingUnload(int handle)
    {
        if (!_pendingUnloads.TryGetValue(handle, out Coroutine routine)) return;
        if (routine != null) StopCoroutine(routine);
        _pendingUnloads.Remove(handle);
    }

    IEnumerator UnloadAfterDelay(int handle)
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
        Forget(handle);

        Debug.Log($"[Zone] Unloading empty '{zoneName}' (handle {handle}).");
        yield return SceneManager.UnloadSceneAsync(scene);
    }

    void Forget(int handle)
    {
        _occupants.Remove(handle);
        _scenesByHandle.Remove(handle);

        for (int i = _simulated.Count - 1; i >= 0; i--)
            if (_simulated[i].handle == handle) _simulated.RemoveAt(i);

        // Collect keys first — removing during a foreach over the dictionary throws.
        RemoveByValue(_sharedZones, handle);
        RemoveByValue(_instances, handle);
    }

    static void RemoveByValue(Dictionary<string, int> map, int handle)
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

        var controller = player.GetComponent<CharacterController>();
        if (controller != null) controller.enabled = false;

        player.transform.SetPositionAndRotation(position, rotation);

        if (controller != null) controller.enabled = true;

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
        if (!_connZone.TryGetValue(conn.connectionId, out int handle)) return 0;
        return _occupants.TryGetValue(handle, out HashSet<int> set) ? set.Count : 0;
    }

    /// <summary>Zone a connection is currently in, or null if it has not been placed.</summary>
    public Scene? ZoneOf(NetworkConnectionToClient conn)
    {
        if (conn == null) return null;
        if (!_connZone.TryGetValue(conn.connectionId, out int handle)) return null;
        if (!_scenesByHandle.TryGetValue(handle, out Scene scene)) return null;
        return scene;
    }
}
