using Mirror;
using UnityEngine;
using UnityEngine.AI;

// Simple enemy AI for world mobs and lightweight enemies.
// Aggro can be gained by proximity, by damage, or by player passives.
[RequireComponent(typeof(Health))]
public class EnemyAI : MonoBehaviour
{
    [Header("Combat")]
    public float moveSpeed = 2.5f;
    public float attackRange = 1.8f;
    public float attackDamage = 8f;
    [Tooltip("Attacks per second.")]
    public float attackRate = 1f;
    public bool isElite = false;

    [Header("Aggro")]
    public Transform aggroTarget;
    public string playerTag = "Player";
    [Min(0f)] public float aggroRadius = 8f;
    [Min(0f)] public float leashRadius = 20f;
    public bool aggroWhenDamaged = true;
    [Min(0f)] public float retaliationRadius = 20f;
    [Min(0.05f)] public float targetSearchInterval = 0.25f;
    public bool returnHomeWhenLeashed = true;

    [Header("Stealth")]
    [Tooltip("After losing a target to stealth, how long before the AI searches again.")]
    public float stealthConfusionDuration = 6f;

    Health _health;
    StatusEffectManager _status;
    NavMeshAgent _agent;
    Vector3 _homePosition;
    Quaternion _homeRotation;
    float _attackTimer;
    float _nextSearchTime;
    float _searchSuppressedUntil;
    float _baseAgentSpeed;
    bool _returningHome;

    public bool HasAggroTarget => aggroTarget != null;
    public bool IsReturningHome => _returningHome;
    public Vector3 HomePosition => _homePosition;
    public Quaternion HomeRotation => _homeRotation;

    void Awake()
    {
        _health = GetComponent<Health>();
        _status = GetComponent<StatusEffectManager>();
        _agent = GetComponent<NavMeshAgent>();
        _baseAgentSpeed = _agent != null ? _agent.speed : moveSpeed;
        _homePosition = transform.position;
        _homeRotation = transform.rotation;
    }

    void OnEnable()
    {
        if (_health == null)
            _health = GetComponent<Health>();

        if (_health != null)
        {
            _health.onDamagedBy.RemoveListener(OnDamagedBy);
            _health.onDamagedBy.AddListener(OnDamagedBy);
        }
    }

    void OnDisable()
    {
        if (_health != null)
            _health.onDamagedBy.RemoveListener(OnDamagedBy);
    }

    void Start()
    {
        _homePosition = transform.position;
        _homeRotation = transform.rotation;
    }

    void OnEnemyRespawned()
    {
        aggroTarget = null;
        _returningHome = false;
        _attackTimer = 0f;
        _searchSuppressedUntil = 0f;
        _nextSearchTime = Time.time + targetSearchInterval;
        StopMoving();
    }

    void Update()
    {
        if (!CanRunAI()) return;
        if (_health == null || !_health.IsAlive)
        {
            StopMoving();
            return;
        }

        if (_status != null && (_status.IsStaggered || _status.IsBound))
        {
            StopMoving();
            return;
        }

        ValidateAggroTarget();

        if (aggroTarget == null)
        {
            SearchForTargetIfReady();
            if (aggroTarget == null && _returningHome)
                ReturnHome();
            return;
        }

        float dist = Vector3.Distance(transform.position, aggroTarget.position);
        if (dist > attackRange)
        {
            ChaseTarget();
            return;
        }

        TryAttackTarget();
    }

    public void SetAggroTarget(Transform t)
    {
        if (t == null && aggroTarget != null)
            _searchSuppressedUntil = Time.time + stealthConfusionDuration;

        aggroTarget = t;
        _attackTimer = 0f;
        _returningHome = t == null;

        if (aggroTarget != null)
        {
            _searchSuppressedUntil = 0f;
            _returningHome = false;
        }
    }

    void OnDamagedBy(GameObject source)
    {
        if (!CanRunAI()) return;
        if (!aggroWhenDamaged || source == null) return;
        if (_health == null || !_health.IsAlive) return;

        Transform attacker = ResolvePlayerTransform(source);
        if (attacker == null)
            attacker = FindNearestPlayer(Mathf.Max(aggroRadius, retaliationRadius));

        if (attacker != null)
            SetAggroTarget(attacker);
    }

    void ValidateAggroTarget()
    {
        if (aggroTarget == null) return;

        Health targetHealth = GetTargetHealth(aggroTarget);
        if (targetHealth != null && !targetHealth.IsAlive)
        {
            aggroTarget = null;
            _returningHome = true;
            return;
        }

        if (leashRadius > 0f && Vector3.Distance(_homePosition, aggroTarget.position) > leashRadius)
        {
            aggroTarget = null;
            _attackTimer = 0f;
            _returningHome = true;
        }
    }

    void SearchForTargetIfReady()
    {
        if (Time.time < _searchSuppressedUntil) return;
        if (Time.time < _nextSearchTime) return;

        _nextSearchTime = Time.time + targetSearchInterval;
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
        if (aggroTarget == null) return;

        float slow = _status != null ? _status.GetSlowFraction() : 0f;
        float speed = moveSpeed * (1f - slow);

        if (CanUseAgent())
        {
            _agent.isStopped = false;
            _agent.speed = speed;
            _agent.stoppingDistance = Mathf.Max(0.1f, attackRange * 0.85f);
            _agent.SetDestination(aggroTarget.position);
            FaceTarget(aggroTarget.position);
            return;
        }

        Vector3 dir = aggroTarget.position - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude <= 0.001f) return;

        Vector3 stepDir = dir.normalized;
        transform.position += stepDir * speed * Time.deltaTime;
        transform.forward = Vector3.Slerp(transform.forward, stepDir, Time.deltaTime * 6f);
    }

    void TryAttackTarget()
    {
        StopMoving();
        FaceTarget(aggroTarget.position);

        if (_status != null && _status.IsSilenced) return;

        _attackTimer += Time.deltaTime;
        float interval = 1f / Mathf.Max(0.05f, attackRate);
        if (_attackTimer < interval) return;

        _attackTimer = 0f;
        Attack();
    }

    void Attack()
    {
        if (aggroTarget == null) return;

        Health targetHealth = GetTargetHealth(aggroTarget);
        if (targetHealth == null || !targetHealth.IsAlive) return;

        BroadcastMessage("PlayAttack", SendMessageOptions.DontRequireReceiver);
        targetHealth.TakeDamage(attackDamage, gameObject);
    }

    void ReturnHome()
    {
        if (!returnHomeWhenLeashed)
        {
            _returningHome = false;
            return;
        }
        if (Vector3.Distance(transform.position, _homePosition) <= 0.25f)
        {
            _returningHome = false;
            transform.rotation = _homeRotation;
            return;
        }

        if (CanUseAgent())
        {
            _agent.isStopped = false;
            _agent.speed = _baseAgentSpeed > 0f ? _baseAgentSpeed : moveSpeed;
            _agent.stoppingDistance = 0.1f;
            _agent.SetDestination(_homePosition);
            FaceMoveDirection(_homePosition);
            return;
        }

        Vector3 dir = _homePosition - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude <= 0.001f)
        {
            transform.rotation = _homeRotation;
            return;
        }

        Vector3 stepDir = dir.normalized;
        transform.position += stepDir * moveSpeed * Time.deltaTime;
        transform.forward = Vector3.Slerp(transform.forward, stepDir, Time.deltaTime * 6f);
    }

    void StopMoving()
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

    Health GetTargetHealth(Transform target)
    {
        if (target == null) return null;

        Health health = target.GetComponent<Health>();
        if (health != null) return health;

        health = target.GetComponentInParent<Health>();
        if (health != null) return health;

        return target.GetComponentInChildren<Health>();
    }

    bool CanUseAgent()
    {
        return _agent != null && _agent.enabled && _agent.isOnNavMesh;
    }

    bool CanRunAI()
    {
        return !NetworkClient.active || NetworkServer.active;
    }

    void OnDrawGizmosSelected()
    {
        Vector3 center = Application.isPlaying ? _homePosition : transform.position;

        Gizmos.color = new Color(1f, 0.65f, 0f, 0.25f);
        Gizmos.DrawWireSphere(center, aggroRadius);

        Gizmos.color = new Color(1f, 0f, 0f, 0.25f);
        Gizmos.DrawWireSphere(center, attackRange);

        Gizmos.color = new Color(0.25f, 0.65f, 1f, 0.2f);
        Gizmos.DrawWireSphere(center, leashRadius);
    }
}
