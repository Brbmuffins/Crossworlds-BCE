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
    [Tooltip("Keeps assigned mobs in their authored formation and makes the group share patrol and combat-return waypoints.")]
    public bool groupFormationPatrol = true;
    [Min(0f)] public float waypointWaitSeconds = 1f;
    [Min(0.05f)] public float arrivalDistance = 0.35f;
    public List<Transform> waypoints = new();
    [Tooltip("Scene mobs assigned to this route. The server restores their patrol agents after additive scene loading.")]
    public List<GameObject> patrolObjects = new();

    bool _serverAgentsEnsured;
    int _sharedResumeWaypoint = -1;
    float _sharedResumeExpires;
    bool _formationProgressInitialized;
    int _formationWaypointIndex;
    int _formationDirection = 1;
    float _formationWaitUntil;
    readonly HashSet<EnemyPatrolAgent> _formationArrivals = new();

    public int Count => waypoints?.Count ?? 0;

    public bool TryGetWaypoint(int index, out Vector3 position)
    {
        position = default;
        if (waypoints == null || index < 0 || index >= waypoints.Count ||
            waypoints[index] == null) return false;
        position = waypoints[index].position;
        return true;
    }

    public Vector3 GetWaypointForward(int index, int direction = 1)
    {
        if (Count <= 1) return transform.forward;
        index = Mathf.Clamp(index, 0, Count - 1);
        int next;
        if (mode == EnemyPatrolMode.PingPong && direction < 0)
            next = Mathf.Max(0, index - 1);
        else if (mode == EnemyPatrolMode.OneWay)
            next = index < Count - 1 ? index + 1 : index - 1;
        else
            next = (index + 1) % Count;

        if (waypoints[index] == null || waypoints[next] == null)
            return transform.forward;
        Vector3 forward = waypoints[next].position - waypoints[index].position;
        forward.y = 0f;
        return forward.sqrMagnitude > 0.0001f
            ? forward.normalized
            : transform.forward;
    }

    public int GetSharedResumeWaypoint(Vector3 fallbackPosition)
    {
        if (Count <= 1) return 0;
        if (_sharedResumeWaypoint >= 0 && Time.time <= _sharedResumeExpires)
            return _sharedResumeWaypoint;

        Vector3 center = Vector3.zero;
        int memberCount = 0;
        foreach (GameObject patrolObject in patrolObjects)
        {
            if (patrolObject == null || !patrolObject.activeInHierarchy) continue;
            Health health = patrolObject.GetComponent<Health>();
            if (health != null && !health.IsAlive) continue;
            center += patrolObject.transform.position;
            memberCount++;
        }
        if (memberCount > 0) center /= memberCount;
        else center = fallbackPosition;

        _sharedResumeWaypoint = FindNearestWaypoint(center);
        _formationWaypointIndex = _sharedResumeWaypoint;
        _formationProgressInitialized = true;
        _formationWaitUntil = 0f;
        _formationArrivals.Clear();
        // Combat exit can occur on different AI ticks. Hold the shared choice
        // long enough for every member of the formation to receive it.
        _sharedResumeExpires = Time.time + 5f;
        return _sharedResumeWaypoint;
    }

    public void GetFormationProgress(int startingWaypoint, out int waypointIndex,
        out int direction, out float waitUntil)
    {
        if (!_formationProgressInitialized)
        {
            _formationWaypointIndex = Mathf.Clamp(
                startingWaypoint, 0, Mathf.Max(0, Count - 1));
            _formationDirection = 1;
            _formationWaitUntil = 0f;
            _formationProgressInitialized = true;
        }
        waypointIndex = _formationWaypointIndex;
        direction = _formationDirection;
        waitUntil = _formationWaitUntil;
    }

    public bool ReportFormationArrival(EnemyPatrolAgent member,
        out int waypointIndex, out int direction, out float waitUntil)
    {
        GetFormationProgress(member.startingWaypoint,
            out waypointIndex, out direction, out waitUntil);
        _formationArrivals.Add(member);

        int required = 0;
        foreach (GameObject patrolObject in patrolObjects)
        {
            if (patrolObject == null || !patrolObject.activeInHierarchy) continue;
            Health health = patrolObject.GetComponent<Health>();
            if (health != null && !health.IsAlive) continue;
            EnemyPatrolAgent patrol = patrolObject.GetComponent<EnemyPatrolAgent>();
            if (patrol != null && patrol.UsesGroupFormation) required++;
        }
        if (required > 0 && _formationArrivals.Count < required)
            return false;

        AdvanceFormationProgress();
        _formationArrivals.Clear();
        _formationWaitUntil = Time.time + Mathf.Max(0f, waypointWaitSeconds);
        waypointIndex = _formationWaypointIndex;
        direction = _formationDirection;
        waitUntil = _formationWaitUntil;
        return true;
    }

    void AdvanceFormationProgress()
    {
        if (Count <= 1) return;
        switch (mode)
        {
            case EnemyPatrolMode.Loop:
                _formationWaypointIndex = (_formationWaypointIndex + 1) % Count;
                break;
            case EnemyPatrolMode.PingPong:
                if (_formationWaypointIndex >= Count - 1) _formationDirection = -1;
                else if (_formationWaypointIndex <= 0) _formationDirection = 1;
                _formationWaypointIndex += _formationDirection;
                break;
            case EnemyPatrolMode.OneWay:
                _formationWaypointIndex = Mathf.Min(
                    _formationWaypointIndex + 1, Count - 1);
                break;
        }
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
            if (!patrol.UsesGroupFormation)
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

    int FindNearestWaypoint(Vector3 position)
    {
        int nearest = 0;
        float nearestDistance = float.MaxValue;
        for (int i = 0; i < Count; i++)
        {
            if (!TryGetWaypoint(i, out Vector3 waypoint)) continue;
            Vector3 delta = waypoint - position;
            delta.y = 0f;
            float distance = delta.sqrMagnitude;
            if (distance >= nearestDistance) continue;
            nearestDistance = distance;
            nearest = i;
        }
        return nearest;
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
    [Tooltip("Keeps this mob's authored spacing relative to the rest of its patrol group.")]
    public bool preserveFormation;
    [Tooltip("Sideways and forward spacing from the patrol route center.")]
    public Vector2 formationOffset;

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
        if (UsesGroupFormation)
        {
            route.GetFormationProgress(startingWaypoint,
                out int sharedWaypoint, out int sharedDirection, out float sharedWait);
            if (_waypointIndex != sharedWaypoint || _direction != sharedDirection)
            {
                _waypointIndex = sharedWaypoint;
                _direction = sharedDirection;
                _hasDestination = false;
                if (agent.hasPath) agent.ResetPath();
            }
            _waitUntil = sharedWait;
        }
        if (Time.time < _waitUntil)
        {
            if (agent.hasPath) agent.ResetPath();
            return true;
        }

        if (!TryGetPatrolWaypoint(out Vector3 waypoint))
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
            return CompleteWaypoint(agent);
        }

        if (_hasDestination && !agent.pathPending &&
            (!agent.hasPath || agent.remainingDistance <= arrival))
        {
            return CompleteWaypoint(agent);
        }

        if (!_hasDestination)
            BeginLeg(agent, waypoint);
        return true;
    }

    bool CompleteWaypoint(NavMeshAgent agent)
    {
        _hasDestination = false;
        if (UsesGroupFormation)
        {
            bool groupRouteFinished = route.Count <= 1 ||
                (route.mode == EnemyPatrolMode.OneWay &&
                 _waypointIndex >= route.Count - 1);
            if (groupRouteFinished)
            {
                if (agent.hasPath) agent.ResetPath();
                return true;
            }

            bool advanced = route.ReportFormationArrival(this,
                out _waypointIndex, out _direction, out _waitUntil);
            if (!advanced || Time.time < _waitUntil)
            {
                if (agent.hasPath) agent.ResetPath();
                return true;
            }
            if (TryGetPatrolWaypoint(out Vector3 groupWaypoint))
                BeginLeg(agent, groupWaypoint);
            return true;
        }

        float wait = Mathf.Max(0f, route.waypointWaitSeconds);
        bool routeFinished = route.Count <= 1 ||
            (route.mode == EnemyPatrolMode.OneWay &&
             _waypointIndex >= route.Count - 1);
        if (routeFinished)
        {
            if (agent.hasPath) agent.ResetPath();
            return true;
        }
        Advance();
        if (wait > 0f)
        {
            if (agent.hasPath) agent.ResetPath();
            _waitUntil = Time.time + wait;
            return true;
        }

        // A zero-wait patrol should flow through the waypoint. Replace the
        // destination immediately so the agent never enters a one-tick idle state.
        if (TryGetPatrolWaypoint(out Vector3 nextWaypoint))
            BeginLeg(agent, nextWaypoint);
        return true;
    }

    bool TryGetPatrolWaypoint(out Vector3 waypoint)
    {
        return TryGetPatrolWaypoint(_waypointIndex, _direction, out waypoint);
    }

    bool TryGetPatrolWaypoint(int waypointIndex, int direction, out Vector3 waypoint)
    {
        if (!route.TryGetWaypoint(waypointIndex, out waypoint))
            return false;
        if (!UsesGroupFormation) return true;

        Vector3 forward = route.GetWaypointForward(waypointIndex, direction);
        Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;
        waypoint += right * formationOffset.x + forward * formationOffset.y;
        return true;
    }

    void BeginLeg(NavMeshAgent agent, Vector3 waypoint)
    {
        // A formation slot can sit beside a valid route waypoint on a narrow
        // NavMesh. Search far enough to recover toward the route center instead
        // of pausing and skipping the waypoint.
        float formationRecovery = UsesGroupFormation
            ? formationOffset.magnitude + agent.radius
            : 0f;
        float sampleRadius = Mathf.Max(
            Mathf.Max(1.5f, agent.height), formationRecovery);
        if (NavMesh.SamplePosition(waypoint, out NavMeshHit hit, sampleRadius, agent.areaMask))
        {
            agent.isStopped = false;
            _hasDestination = agent.SetDestination(hit.position);
            return;
        }

        string targetKind = UsesGroupFormation ? "formation position near" : "waypoint";
        Debug.LogWarning($"[Patrol Forge] The {targetKind} " +
                         $"'{route.waypoints[_waypointIndex].name}' is not near the NavMesh.",
                         route.waypoints[_waypointIndex]);
        _waitUntil = Time.time + 1f;
        Advance();
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

    public void ResumeFromNearestWaypoint()
    {
        if (!HasUsableRoute) return;
        if (_agent == null)
            _agent = GetComponent<NavMeshAgent>();

        int nearest;
        if (UsesGroupFormation)
        {
            nearest = route.GetSharedResumeWaypoint(transform.position);
        }
        else
        {
            nearest = 0;
            float nearestDistance = float.MaxValue;
            for (int i = 0; i < route.Count; i++)
            {
                if (!route.TryGetWaypoint(i, out Vector3 waypoint))
                    continue;
                Vector3 delta = waypoint - transform.position;
                delta.y = 0f;
                float distance = delta.sqrMagnitude;
                if (distance >= nearestDistance) continue;
                nearestDistance = distance;
                nearest = i;
            }
        }

        _waypointIndex = nearest;
        if (route.mode == EnemyPatrolMode.PingPong)
        {
            if (_waypointIndex <= 0) _direction = 1;
            else if (_waypointIndex >= route.Count - 1) _direction = -1;
        }
        _initialized = true;
        _hasDestination = false;
        _waitUntil = 0f;
        _navMeshRecoveryAttempted = false;
        _reportedInvalidSetup = false;

        if (_agent == null || !_agent.isActiveAndEnabled) return;
        _agent.isStopped = false;
        if (_agent.isOnNavMesh && _agent.hasPath)
            _agent.ResetPath();
        if (EnsureOnNavMesh(_agent) &&
            TryGetPatrolWaypoint(out Vector3 nearestWaypoint))
            BeginLeg(_agent, nearestWaypoint);
    }

    void Initialize()
    {
        if (_initialized) return;
        _waypointIndex = Mathf.Clamp(startingWaypoint, 0, Mathf.Max(0, route.Count - 1));
        _direction = 1;
        _initialized = true;
    }

    public bool UsesGroupFormation =>
        preserveFormation && route != null && route.groupFormationPatrol;

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
