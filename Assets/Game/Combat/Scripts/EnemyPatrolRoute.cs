using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyPatrolMode
{
    Loop,
    PingPong,
    OneWay
}

/// <summary>A scene-authored route shared by any number of patrol agents.</summary>
public sealed class EnemyPatrolRoute : MonoBehaviour
{
    public EnemyPatrolMode mode = EnemyPatrolMode.Loop;
    [Min(0f)] public float waypointWaitSeconds = 1f;
    [Min(0.05f)] public float arrivalDistance = 0.35f;
    public List<Transform> waypoints = new();
    [Tooltip("Scene mobs assigned to this route. The server restores their patrol agents after additive scene loading.")]
    public List<GameObject> patrolObjects = new();

    bool _serverAgentsEnsured;

    public int Count => waypoints?.Count ?? 0;

    public bool TryGetWaypoint(int index, out Vector3 position)
    {
        position = default;
        if (waypoints == null || index < 0 || index >= waypoints.Count ||
            waypoints[index] == null) return false;
        position = waypoints[index].position;
        return true;
    }

    void Update()
    {
        if (_serverAgentsEnsured || !NetworkServer.active) return;
        _serverAgentsEnsured = true;

        foreach (GameObject patrolObject in patrolObjects)
        {
            if (patrolObject == null) continue;
            EnemyPatrolAgent patrol = patrolObject.GetComponent<EnemyPatrolAgent>();
            if (patrol == null)
                patrol = patrolObject.AddComponent<EnemyPatrolAgent>();
            patrol.route = this;
            patrol.startingWaypoint = FindNextWaypoint(patrolObject.transform.position);
            patrol.enabled = true;
            patrol.ResetPatrol();
        }
    }

    int FindNextWaypoint(Vector3 position)
    {
        if (Count <= 1) return 0;
        int nearest = 0;
        float nearestDistance = float.MaxValue;
        for (int i = 0; i < Count; i++)
        {
            if (waypoints[i] == null) continue;
            float distance = (waypoints[i].position - position).sqrMagnitude;
            if (distance >= nearestDistance) continue;
            nearestDistance = distance;
            nearest = i;
        }
        return mode == EnemyPatrolMode.OneWay
            ? Mathf.Min(nearest + 1, Count - 1)
            : (nearest + 1) % Count;
    }
}

/// <summary>
/// Server-authoritative patrol state. EnemyController calls TickPatrol only
/// while idle, so combat and leash-return navigation always take priority.
/// </summary>
public sealed class EnemyPatrolAgent : MonoBehaviour
{
    public EnemyPatrolRoute route;
    public int startingWaypoint;

    EnemyController _controller;
    NavMeshAgent _agent;
    int _waypointIndex;
    int _direction = 1;
    float _waitUntil;
    float _nextTick;
    bool _initialized;
    bool _hasDestination;
    bool _navMeshRecoveryAttempted;
    bool _reportedInvalidSetup;
    bool _reportedRuntimeState;

    public bool HasUsableRoute => route != null && route.Count > 0;

    void Awake()
    {
        _controller = GetComponent<EnemyController>();
        _agent = GetComponent<NavMeshAgent>();
    }

    void OnEnable()
    {
        _nextTick = 0f;
        ResetPatrol();
    }

    void Update()
    {
        if (!HasAuthority() || Time.time < _nextTick) return;
        _nextTick = Time.time + 0.2f;

        if (_controller == null)
            _controller = GetComponent<EnemyController>();
        if (_agent == null)
            _agent = GetComponent<NavMeshAgent>();

        if (!_reportedRuntimeState)
        {
            _reportedRuntimeState = true;
            string controllerState = _controller != null
                ? $"{_controller.state}, target={(_controller.CurrentTarget != null ? _controller.CurrentTarget.name : "none")}, returning={_controller.IsReturningHome}"
                : "none";
            Debug.Log(
                $"[Patrol Forge] Runtime '{name}': route={(route != null ? route.name : "none")}, " +
                $"start={startingWaypoint}, controller={controllerState}, " +
                $"agent={(_agent != null ? "present" : "missing")}, " +
                $"enabled={(_agent != null && _agent.isActiveAndEnabled)}, " +
                $"onNavMesh={(_agent != null && _agent.isOnNavMesh)}, server={NetworkServer.active}.",
                this);
        }

        if (_controller != null &&
            (_controller.state != EnemyController.EnemyState.Idle ||
             _controller.CurrentTarget != null ||
             _controller.IsReturningHome))
            return;

        TickPatrol(_agent);
    }

    public bool TickPatrol(NavMeshAgent agent)
    {
        if (!HasAuthority())
            return false;

        if (route == null || route.Count == 0 || agent == null ||
            !agent.isActiveAndEnabled)
        {
            ReportInvalidSetup(agent == null
                ? "has no NavMeshAgent"
                : route == null || route.Count == 0
                    ? "has no usable route or waypoints"
                    : "has a disabled NavMeshAgent");
            return false;
        }

        if (!EnsureOnNavMesh(agent))
            return false;

        Initialize();
        if (Time.time < _waitUntil)
        {
            if (agent.hasPath) agent.ResetPath();
            return true;
        }

        if (!route.TryGetWaypoint(_waypointIndex, out Vector3 waypoint))
        {
            Advance();
            return true;
        }

        // Multiple agents can share a route. Once one occupies the exact waypoint,
        // avoidance keeps the others roughly two radii away; accept that as arrival
        // so a patrol group cannot deadlock behind its lead agent.
        float sharedRouteArrival = agent.radius * 2.25f;
        float arrival = Mathf.Max(
            route.arrivalDistance,
            agent.stoppingDistance + 0.05f,
            sharedRouteArrival);

        // A model may begin inside its first waypoint's arrival radius. In that
        // case NavMeshAgent can decline to create a path, so recognize arrival
        // directly and advance instead of repeatedly requesting the same point.
        Vector3 arrivalDelta = waypoint - transform.position;
        arrivalDelta.y = 0f;
        if (!_hasDestination && arrivalDelta.sqrMagnitude <= arrival * arrival)
        {
            if (agent.hasPath) agent.ResetPath();
            _waitUntil = Time.time + Mathf.Max(0f, route.waypointWaitSeconds);
            Advance();
            return true;
        }

        if (_hasDestination && !agent.pathPending &&
            (!agent.hasPath || agent.remainingDistance <= arrival))
        {
            _hasDestination = false;
            agent.ResetPath();
            _waitUntil = Time.time + Mathf.Max(0f, route.waypointWaitSeconds);
            Advance();
            return true;
        }

        if (!_hasDestination)
        {
            float sampleRadius = Mathf.Max(1.5f, agent.height);
            if (NavMesh.SamplePosition(waypoint, out NavMeshHit hit, sampleRadius, agent.areaMask))
            {
                agent.isStopped = false;
                _hasDestination = agent.SetDestination(hit.position);
            }
            else
            {
                Debug.LogWarning($"[Patrol Forge] Waypoint '{route.waypoints[_waypointIndex].name}' " +
                                 $"is not near the NavMesh.", route.waypoints[_waypointIndex]);
                _waitUntil = Time.time + 1f;
                Advance();
            }
        }
        return true;
    }

    public void ResetPatrol()
    {
        _initialized = false;
        _hasDestination = false;
        _waitUntil = 0f;
        _navMeshRecoveryAttempted = false;
        _reportedInvalidSetup = false;
        _reportedRuntimeState = false;
    }

    public void SuspendForCombat()
    {
        _hasDestination = false;
        _waitUntil = 0f;
    }

    void Initialize()
    {
        if (_initialized) return;
        _waypointIndex = Mathf.Clamp(startingWaypoint, 0, Mathf.Max(0, route.Count - 1));
        _direction = 1;
        _initialized = true;
    }

    void Advance()
    {
        int count = route.Count;
        if (count <= 1) return;

        switch (route.mode)
        {
            case EnemyPatrolMode.Loop:
                _waypointIndex = (_waypointIndex + 1) % count;
                break;
            case EnemyPatrolMode.PingPong:
                if (_waypointIndex >= count - 1) _direction = -1;
                else if (_waypointIndex <= 0) _direction = 1;
                _waypointIndex += _direction;
                break;
            case EnemyPatrolMode.OneWay:
                _waypointIndex = Mathf.Min(_waypointIndex + 1, count - 1);
                break;
        }
    }

    bool HasAuthority()
    {
        // Non-networked scene NPCs (for example quest givers) must simulate
        // locally on each client because Mirror has no identity to synchronize.
        if (GetComponent<NetworkIdentity>() == null) return true;
        if (NetworkServer.active) return true;
        return !NetworkClient.active;
    }

    bool EnsureOnNavMesh(NavMeshAgent agent)
    {
        if (agent.isOnNavMesh) return true;
        if (_navMeshRecoveryAttempted) return false;

        _navMeshRecoveryAttempted = true;
        float sampleRadius = Mathf.Max(3f, agent.height * 2f);
        if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit,
                sampleRadius, agent.areaMask) &&
            agent.Warp(hit.position))
        {
            Debug.Log($"[Patrol Forge] Repositioned '{name}' onto the nearby NavMesh.", this);
            return true;
        }

        ReportInvalidSetup(
            $"is not on a baked NavMesh and no walkable point was found within {sampleRadius:0.##} units");
        return false;
    }

    void ReportInvalidSetup(string reason)
    {
        if (_reportedInvalidSetup) return;
        _reportedInvalidSetup = true;
        Debug.LogWarning($"[Patrol Forge] '{name}' cannot patrol because it {reason}.", this);
    }
}
