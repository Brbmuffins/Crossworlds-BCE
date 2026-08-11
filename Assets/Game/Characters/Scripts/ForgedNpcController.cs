using System.Collections;
using Mirror;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Server-authoritative movement for non-combat NPCs created by Enemy Forge.
/// The server alone drives the NavMeshAgent; NetworkTransform and NetworkAnimator
/// replicate the resulting movement and animation to clients.
/// </summary>
[RequireComponent(typeof(NetworkIdentity), typeof(NavMeshAgent))]
[AddComponentMenu("BCE/World/Forged NPC Controller")]
public sealed class ForgedNpcController : NetworkBehaviour
{
    [Header("Identity")]
    public string npcDisplayName = "NPC";

    [Header("Roaming")]
    public bool enableRoaming = true;
    [Min(0f)] public float roamingRadius = 8f;
    [Min(0f)] public float roamingMinWait = 2f;
    [Min(0f)] public float roamingMaxWait = 5f;

    [Header("Animation")]
    public string movementSpeedParameter = "Speed";

    NavMeshAgent _agent;
    Animator _animator;
    Vector3 _spawnPosition;
    Coroutine _roamingRoutine;
    int _speedHash;
    bool _hasSpeedParameter;

    public override void OnStartServer()
    {
        base.OnStartServer();
        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponentInChildren<Animator>(true);
        CacheAnimationParameter();
        _spawnPosition = transform.position;

        if (_agent != null && !_agent.isOnNavMesh &&
            NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 5f, NavMesh.AllAreas))
            _agent.Warp(hit.position);

        if (enableRoaming && roamingRadius > 0f)
            _roamingRoutine = StartCoroutine(Roam());
        else
            StopMoving();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        // Remote clients receive the authoritative transform from Mirror and must
        // never run their own NavMesh simulation.
        if (!isServer)
        {
            _agent = GetComponent<NavMeshAgent>();
            if (_agent != null) _agent.enabled = false;
        }
    }

    public override void OnStopServer()
    {
        if (_roamingRoutine != null) StopCoroutine(_roamingRoutine);
        _roamingRoutine = null;
        base.OnStopServer();
    }

    [ServerCallback]
    void Update()
    {
        if (_agent == null || !_agent.enabled || !_agent.isOnNavMesh)
        {
            SetMovementSpeed(0f);
            return;
        }
        SetMovementSpeed(_agent.velocity.magnitude);
    }

    [Server]
    IEnumerator Roam()
    {
        while (true)
        {
            StopMoving();
            yield return new WaitForSeconds(Random.Range(
                Mathf.Max(0f, roamingMinWait),
                Mathf.Max(roamingMinWait, roamingMaxWait)));

            Vector2 offset = Random.insideUnitCircle * roamingRadius;
            Vector3 requested = _spawnPosition + new Vector3(offset.x, 0f, offset.y);
            if (_agent != null && _agent.enabled && _agent.isOnNavMesh &&
                NavMesh.SamplePosition(requested, out NavMeshHit hit,
                    Mathf.Max(1f, roamingRadius), NavMesh.AllAreas))
            {
                _agent.isStopped = false;
                _agent.SetDestination(hit.position);
                while (_agent.enabled && _agent.isOnNavMesh &&
                       (_agent.pathPending ||
                        _agent.remainingDistance > _agent.stoppingDistance + 0.1f))
                    yield return null;
            }
        }
    }

    [Server]
    void StopMoving()
    {
        if (_agent != null && _agent.enabled && _agent.isOnNavMesh)
        {
            _agent.isStopped = true;
            _agent.ResetPath();
        }
        SetMovementSpeed(0f);
    }

    void CacheAnimationParameter()
    {
        _hasSpeedParameter = false;
        _speedHash = Animator.StringToHash(movementSpeedParameter ?? "Speed");
        if (_animator == null || _animator.runtimeAnimatorController == null) return;
        foreach (AnimatorControllerParameter parameter in _animator.parameters)
        {
            if (parameter.nameHash != _speedHash || parameter.type != AnimatorControllerParameterType.Float)
                continue;
            _hasSpeedParameter = true;
            break;
        }
    }

    void SetMovementSpeed(float speed)
    {
        if (_hasSpeedParameter && _animator != null)
            _animator.SetFloat(_speedHash, Mathf.Max(0f, speed));
    }
}
