using System.Collections;
using Mirror;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Roaming world mob behavior. Wanders while idle, then chases and attacks players
/// who enter its aggro radius or damage it.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class EnemyWanderAI : MonoBehaviour
{
    [Header("Wander")]
    public float wanderRadius = 8f;
    public float minWaitTime = 2f;
    public float maxWaitTime = 5f;

    [Header("Aggro")]
    public bool enableAggro = true;
    public string playerTag = "Player";
    [Min(0f)] public float aggroRadius = 8f;
    [Min(0f)] public float leashRadius = 20f;
    public bool aggroWhenDamaged = true;
    [Min(0f)] public float retaliationRadius = 20f;
    [Min(0.05f)] public float targetSearchInterval = 0.25f;

    [Header("Combat")]
    public float chaseSpeed = 2.5f;
    public float attackRange = 1.8f;
    public float attackDamage = 8f;
    [Tooltip("Attacks per second.")]
    public float attackRate = 1f;

    [Header("Animation (optional)")]
    [Tooltip("Animator bool set true while moving. Only used if the controller declares this parameter.")]
    public string walkBoolParam = "isMoving";

    NavMeshAgent _agent;
    Animator _anim;
    Health _health;
    StatusEffectManager _status;
    EnemyAI _enemyAI;
    Transform _aggroTarget;
    Vector3 _origin;
    Quaternion _originRotation;
    Coroutine _wanderRoutine;
    bool _hasWalkParam;
    bool _stoppedForDeath;
    bool _returningToOrigin;
    bool _hasOrigin;
    float _baseAgentSpeed;
    float _nextTargetSearchTime;
    float _attackTimer;

    public bool HasAggroTarget => _aggroTarget != null
        || _returningToOrigin
        || (_enemyAI != null && _enemyAI.enabled && (_enemyAI.HasAggroTarget || _enemyAI.IsReturningHome));
    public bool HasLeashReturnPoint => _hasOrigin;
    public Vector3 LeashReturnPosition => _origin;
    public Quaternion LeashReturnRotation => _originRotation;

    void OnEnable()
    {
        BindHealthEvents();
    }

    void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        _anim = GetComponentInChildren<Animator>();
        _health = GetComponent<Health>();
        _status = GetComponent<StatusEffectManager>();
        _enemyAI = GetComponent<EnemyAI>();

        if (_agent != null && !_agent.isOnNavMesh)
        {
            if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 5f, NavMesh.AllAreas))
            {
                _agent.Warp(hit.position);
                Debug.Log($"[EnemyWanderAI] Snapped '{gameObject.name}' to NavMesh at {hit.position}");
            }
        }

        CaptureOrigin();
        _baseAgentSpeed = _agent != null ? _agent.speed : chaseSpeed;

        if (_health != null)
        {
            BindHealthEvents();

            if (!_health.IsAlive)
            {
                StopForDeath();
                return;
            }
        }

        if (_anim != null && _anim.runtimeAnimatorController != null)
        {
            foreach (var parameter in _anim.parameters)
            {
                if (parameter.name == walkBoolParam)
                {
                    _hasWalkParam = true;
                    break;
                }
            }
        }

        if (CanRunServerSide())
            _wanderRoutine = StartCoroutine(WanderLoop());
    }

    void BindHealthEvents()
    {
        if (_health == null)
            _health = GetComponent<Health>();

        if (_health == null) return;

        _health.onDeath.RemoveListener(StopForDeath);
        _health.onDeath.AddListener(StopForDeath);
        _health.onHealthChanged.RemoveListener(OnHealthChanged);
        _health.onHealthChanged.AddListener(OnHealthChanged);
        _health.onDamagedBy.RemoveListener(OnDamagedBy);
        _health.onDamagedBy.AddListener(OnDamagedBy);
    }

    void Update()
    {
        if (!CanRunServerSide()) return;
        if (!enableAggro || HasExternalEnemyAI()) return;
        if (_stoppedForDeath || (_health != null && !_health.IsAlive)) return;

        if (_status != null && (_status.IsStaggered || _status.IsBound))
        {
            StopAgent();
            SetWalking(false);
            return;
        }

        ValidateAggroTarget();

        if (_aggroTarget == null)
        {
            SearchForTargetIfReady();
            if (_aggroTarget == null && _returningToOrigin)
                ReturnToOrigin();
            return;
        }

        Vector3 slot = EnemyCrowdUtility.ChaseSlot(transform, _aggroTarget, EnemyCrowdUtility.MeleeSlotRadius(attackRange));
        if (EnemyCrowdUtility.ShouldMoveToMeleeSlot(transform, _aggroTarget, slot, attackRange))
        {
            ChaseTarget();
            return;
        }

        TryAttackTarget();
    }

    void OnDisable()
    {
        if (_health != null)
        {
            _health.onDeath.RemoveListener(StopForDeath);
            _health.onHealthChanged.RemoveListener(OnHealthChanged);
            _health.onDamagedBy.RemoveListener(OnDamagedBy);
        }
    }

    public void SetAggroTarget(Transform target)
    {
        if (HasExternalEnemyAI())
        {
            _enemyAI.SetAggroTarget(target);
            return;
        }

        _aggroTarget = target;
        float interval = 1f / Mathf.Max(0.05f, attackRate);
        _attackTimer = _aggroTarget != null ? EnemyCrowdUtility.ReadyCountUpAttackTimer(this, interval) : 0f;
        _returningToOrigin = false;
    }

    IEnumerator WanderLoop()
    {
        while (!_stoppedForDeath)
        {
            if (HasAggroTarget)
            {
                SetWalking(false);
                yield return new WaitForSeconds(0.25f);
                continue;
            }

            Vector2 circle = Random.insideUnitCircle * wanderRadius;
            Vector3 dest = _origin + new Vector3(circle.x, 0f, circle.y);

            if (CanUseAgent()
                && NavMesh.SamplePosition(dest, out NavMeshHit hit, wanderRadius, NavMesh.AllAreas))
            {
                _agent.isStopped = false;
                _agent.speed = _baseAgentSpeed > 0f ? _baseAgentSpeed : chaseSpeed;
                _agent.SetDestination(hit.position);
            }

            yield return null;

            while (!_stoppedForDeath
                && !HasAggroTarget
                && CanUseAgent()
                && (_agent.pathPending || _agent.remainingDistance > _agent.stoppingDistance + 0.3f))
            {
                SetWalking(true);
                yield return new WaitForSeconds(0.25f);
            }

            SetWalking(false);
            yield return new WaitForSeconds(Random.Range(minWaitTime, maxWaitTime));
        }
    }

    void OnDamagedBy(GameObject source)
    {
        if (!CanRunServerSide()) return;
        if (!enableAggro || !aggroWhenDamaged || source == null) return;
        if (_stoppedForDeath || (_health != null && !_health.IsAlive)) return;

        Transform attacker = ResolvePlayerTransform(source);
        if (attacker == null)
            attacker = FindNearestPlayer(Mathf.Max(aggroRadius, retaliationRadius));

        if (attacker != null)
            SetAggroTarget(attacker);
    }

    void ValidateAggroTarget()
    {
        if (_aggroTarget == null) return;

        Health targetHealth = GetTargetHealth(_aggroTarget);
        if (targetHealth != null && !targetHealth.IsAlive)
        {
            ClearAggro();
            return;
        }

        if (leashRadius > 0f && Vector3.Distance(_origin, _aggroTarget.position) > leashRadius)
            ClearAggro();
    }

    void SearchForTargetIfReady()
    {
        if (Time.time < _nextTargetSearchTime) return;

        _nextTargetSearchTime = Time.time + targetSearchInterval;
        Transform found = FindNearestPlayer(aggroRadius);
        if (found != null)
            SetAggroTarget(found);
    }

    Transform FindNearestPlayer(float radius)
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag(playerTag);
        float radiusSqr = radius > 0f ? radius * radius : Mathf.Infinity;
        float best = Mathf.Infinity;
        Transform found = null;

        foreach (var player in players)
        {
            if (player == null) continue;

            Health playerHealth = GetTargetHealth(player.transform);
            if (playerHealth != null && !playerHealth.IsAlive) continue;

            float sqrDist = (player.transform.position - transform.position).sqrMagnitude;
            if (sqrDist > radiusSqr || sqrDist >= best) continue;

            best = sqrDist;
            found = player.transform;
        }

        return found;
    }

    Transform ResolvePlayerTransform(GameObject source)
    {
        Transform current = source.transform;
        while (current != null)
        {
            if (current.CompareTag(playerTag))
                return current;

            Health sourceHealth = current.GetComponent<Health>();
            if (sourceHealth != null && sourceHealth.isPlayer && sourceHealth.IsAlive)
                return sourceHealth.transform;

            current = current.parent;
        }

        return null;
    }

    void ChaseTarget()
    {
        if (_aggroTarget == null) return;

        float slow = _status != null ? _status.GetSlowFraction() : 0f;
        float speed = chaseSpeed * (1f - slow);

        if (CanUseAgent())
        {
            _agent.isStopped = false;
            _agent.speed = speed;
            _agent.stoppingDistance = 0.25f;
            _agent.SetDestination(EnemyCrowdUtility.ChaseSlot(transform, _aggroTarget, EnemyCrowdUtility.MeleeSlotRadius(attackRange)));
        }
        else
        {
            Vector3 destination = EnemyCrowdUtility.ChaseSlot(transform, _aggroTarget, EnemyCrowdUtility.MeleeSlotRadius(attackRange));
            MoveDirectlyToward(destination, speed);
        }

        FaceTarget(_aggroTarget.position);
        SetWalking(true);
    }

    void TryAttackTarget()
    {
        StopAgent();
        SetWalking(false);
        FaceTarget(_aggroTarget.position);

        if (_status != null && _status.IsSilenced) return;

        _attackTimer += Time.deltaTime;
        float interval = 1f / Mathf.Max(0.05f, attackRate);
        if (_attackTimer < interval) return;

        _attackTimer = 0f;
        Attack();
    }

    void Attack()
    {
        if (_aggroTarget == null) return;

        Health targetHealth = GetTargetHealth(_aggroTarget);
        if (targetHealth == null || !targetHealth.IsAlive) return;

        BroadcastMessage("PlayAttack", SendMessageOptions.DontRequireReceiver);
        targetHealth.TakeDamage(attackDamage, gameObject);
    }

    void ClearAggro()
    {
        _aggroTarget = null;
        _attackTimer = 0f;
        _returningToOrigin = true;
        SetWalking(false);
        ReturnToOrigin();
    }

    void ReturnToOrigin()
    {
        if (Vector3.Distance(transform.position, _origin) <= 0.25f)
        {
            _returningToOrigin = false;
            StopAgent();
            return;
        }

        float speed = _baseAgentSpeed > 0f ? _baseAgentSpeed : chaseSpeed;
        if (CanUseAgent())
        {
            _agent.isStopped = false;
            _agent.speed = speed;
            _agent.stoppingDistance = 0.1f;
            _agent.SetDestination(_origin);
            FaceMoveDirection(_origin);
            return;
        }

        MoveDirectlyToward(_origin, speed);
    }

    void OnHealthChanged(float current, float max)
    {
        if (current <= 0f)
            StopForDeath();
    }

    void StopForDeath()
    {
        if (_stoppedForDeath)
            return;

        _stoppedForDeath = true;
        _aggroTarget = null;
        _returningToOrigin = false;
        SetWalking(false);

        if (_wanderRoutine != null)
        {
            StopCoroutine(_wanderRoutine);
            _wanderRoutine = null;
        }

        StopAgent();
        if (_agent != null)
            _agent.enabled = false;
    }

    void OnEnemyRespawned()
    {
        _stoppedForDeath = false;
        _aggroTarget = null;
        _returningToOrigin = false;
        if (!_hasOrigin)
            CaptureOrigin();

        if (_agent == null)
            _agent = GetComponent<NavMeshAgent>();

        if (_agent != null)
        {
            _agent.enabled = true;
            if (_agent.isOnNavMesh)
            {
                _agent.Warp(transform.position);
                _agent.isStopped = false;
            }
        }

        SetWalking(false);

        if (_wanderRoutine == null && isActiveAndEnabled && CanRunServerSide())
            _wanderRoutine = StartCoroutine(WanderLoop());
    }

    void StopAgent()
    {
        if (!CanUseAgent()) return;

        _agent.ResetPath();
        _agent.velocity = Vector3.zero;
        _agent.isStopped = true;
    }

    void FaceTarget(Vector3 targetPosition)
    {
        Vector3 dir = targetPosition - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 10f);
    }

    void FaceMoveDirection(Vector3 fallbackTarget)
    {
        Vector3 dir = Vector3.zero;

        if (CanUseAgent())
        {
            dir = _agent.desiredVelocity.sqrMagnitude > 0.01f
                ? _agent.desiredVelocity
                : _agent.velocity;
        }

        if (dir.sqrMagnitude <= 0.01f)
            dir = fallbackTarget - transform.position;

        dir.y = 0f;
        if (dir.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 10f);
    }

    void MoveDirectlyToward(Vector3 targetPosition, float speed)
    {
        Vector3 dir = targetPosition - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude <= 0.001f) return;

        Vector3 stepDir = dir.normalized;
        float step = Mathf.Min(speed * Time.deltaTime, dir.magnitude);
        transform.position += stepDir * step;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(stepDir), Time.deltaTime * 10f);
    }

    Health GetTargetHealth(Transform target)
    {
        if (target == null) return null;

        Health health = target.GetComponent<Health>();
        if (health != null) return health;

        health = target.GetComponentInParent<Health>();
        if (health != null) return health;

        return target.GetComponentInChildren<Health>();
    }

    bool HasExternalEnemyAI()
    {
        return _enemyAI != null && _enemyAI.enabled;
    }

    bool CanUseAgent()
    {
        return _agent != null && _agent.enabled && _agent.isOnNavMesh;
    }

    bool CanRunServerSide()
    {
        return !NetworkClient.active || NetworkServer.active;
    }

    void SetWalking(bool walking)
    {
        if (_hasWalkParam && _anim != null)
            _anim.SetBool(walkBoolParam, walking);
    }

    void CaptureOrigin()
    {
        _origin = transform.position;
        _originRotation = transform.rotation;
        _hasOrigin = true;
    }

    void OnDrawGizmosSelected()
    {
        Vector3 center = Application.isPlaying ? _origin : transform.position;

        Gizmos.color = new Color(1f, 0.65f, 0f, 0.25f);
        Gizmos.DrawWireSphere(center, aggroRadius);

        Gizmos.color = new Color(1f, 0f, 0f, 0.25f);
        Gizmos.DrawWireSphere(center, attackRange);

        Gizmos.color = new Color(0.25f, 0.65f, 1f, 0.2f);
        Gizmos.DrawWireSphere(center, leashRadius);
    }
}
