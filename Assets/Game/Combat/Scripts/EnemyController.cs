using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Mirror;

/// <summary>
/// EnemyController — Server-authoritative enemy AI.
/// State machine: Idle → Chase → Attack → Dead
/// Supports melee and ranged variants via isRanged toggle.
/// Respects StatusEffectManager (Stagger, Bound, Slow) — same as EnemyAI.
///
/// Required: Health, NavMeshAgent, NetworkIdentity
/// Optional: StatusEffectManager (auto-detected), Collider
/// Setup via BCE/Setup/4a (grunt), 4b (ranged), 4c (elite)
/// </summary>
[RequireComponent(typeof(Health))]
[RequireComponent(typeof(NavMeshAgent))]
public class EnemyController : NetworkBehaviour
{
    public const int EnemyForgeRuntimeProfileVersion = 8;

    public enum EnemyState { Idle, Chase, Attack, Dead }

    [SyncVar(hook = nameof(OnStateChanged))]
    public EnemyState state = EnemyState.Idle;

    [Header("Runtime Authority")]
    [Tooltip("Allows this enemy to run without an active Mirror server. Enemy Forge enables this only for Offline Local Testing.")]
    public bool allowOfflineSimulation = false;
    [HideInInspector] public int enemyForgeRuntimeProfileVersion;

    // ── Detection ────────────────────────────────────────────────────────────────
    [Header("Detection")]
    public float aggroRadius = 8f;
    public float leashRadius = 20f;
    public bool aggroWhenDamaged = true;
    [Min(0f)] public float retaliationRadius = 20f;

    // ── Combat ───────────────────────────────────────────────────────────────────
    [Header("Combat")]
    [Tooltip("Treats this enemy as an elite for heavier hitstop/screen shake. Also auto-detected from enemyTemplateId/name containing 'elite'.")]
    public bool isElite = false;
    public float attackRange    = 1.5f;
    public float attackInterval = 1.5f;
    public float damage         = 12f;
    [Tooltip("Degrees per second used to face the target while attacking.")]
    [Min(0f)] public float combatTurnSpeed = 1080f;
    [Tooltip("Seconds between starting the attack animation and applying its hit. Enemy Forge derives this from the selected attack clip.")]
    [Min(0f)] public float attackImpactDelay = 0.35f;
    [Tooltip("Immediately faces the aggro target and keeps facing it during chase and attack.")]
    public bool lockFacingOnAggro = false;

    // ── Ranged ───────────────────────────────────────────────────────────────────
    [Header("Ranged")]
    public bool      isRanged         = false;
    public GameObject projectilePrefab;
    public float     preferredRange   = 5f;
    public float     tooCloseDistance = 3f;

    // ── Drops ────────────────────────────────────────────────────────────────────
    [Header("Drops")]
    public DropTable  dropTable;
    public GameObject worldItemPrefab;

    // ── DB Template ID ────────────────────────────────────────────────────────────
    // Must match an id in the enemy_templates DB table (e.g. "grunt_basic").
    // Used by PostCombatKill to award XP/gold via POST /api/combat/kill.
    // Assign in the Inspector on each prefab after running seed_arena_content_2026-07-06.sql.
    [Header("Server")]
    public string enemyTemplateId = "grunt_basic";

    [Header("Death / Respawn")]
    [Tooltip("Seconds the dead model remains visible before it despawns.")]
    public float deadModelVisibleSeconds = 3f;
    [Tooltip("If enabled, this enemy despawns after death, waits, then respawns at its original spawn point.")]
    public bool respawnAfterDeath = false;
    [Tooltip("Seconds after despawn before this enemy respawns.")]
    public float respawnDelay = 30f;
    [Tooltip("Seconds of damage immunity after respawning, preventing persistent area effects from immediately killing the enemy again.")]
    [Min(0f)] public float respawnProtectionSeconds = 2f;
    [Tooltip("Keeps the rendered corpse grounded if the selected death animation contains vertical root or skeleton drift.")]
    public bool keepCorpseGrounded = true;

    // ── Private ──────────────────────────────────────────────────────────────────
    private Health               _health;
    private NavMeshAgent         _agent;
    private StatusEffectManager  _status;   // may be null on basic enemies
    private float                _baseSpeed;
    private Transform            _target;
    private Vector3              _spawnPos;
    private Quaternion           _spawnRot;
    private float                _attackTimer;
    private Animator             _animator;
    private bool                 _hasSpeedParam;
    private bool                 _hasAttackParam;
    private bool                 _hasGetHitParam;
    private bool                 _hasDeathParam;
    private bool                 _returningHome;
    private float                _targetAnimatorSpeed;
    private readonly List<Collider> _deathDisabledColliders = new List<Collider>();
    private bool                 _simulationInitialized;
    private bool                 _attackInProgress;
    private float                _deathGroundY;
    private bool                 _hasDeathGround;
    private SkinnedMeshRenderer  _corpseBodyRenderer;
    private Mesh                 _corpseBakedMesh;

    bool HasSimulationAuthority => NetworkServer.active ||
        (allowOfflineSimulation && !NetworkClient.active && !NetworkServer.active);

    static readonly int SpeedHash = Animator.StringToHash("Speed");
    static readonly int AttackHash = Animator.StringToHash("Attack");
    static readonly int GetHitHash = Animator.StringToHash("GetHit");
    static readonly int DeathHash = Animator.StringToHash("Death");

    // ─────────────────────────────────────────────────────────────────────────────

    void Awake()
    {
        _health    = GetComponent<Health>();
        _agent     = GetComponent<NavMeshAgent>();
        _status    = GetComponent<StatusEffectManager>();
        _baseSpeed = _agent != null ? _agent.speed : 0f;
        _animator  = GetComponentInChildren<Animator>();
        CacheAnimatorParameters();
    }

    void Start()
    {
        if (allowOfflineSimulation && !NetworkClient.active && !NetworkServer.active)
            InitializeSimulation();
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        InitializeSimulation();
    }

    public override void OnStopServer()
    {
        ShutdownSimulation();

        base.OnStopServer();
    }

    void OnDestroy()
    {
        ShutdownSimulation();
        if (_corpseBakedMesh != null) Destroy(_corpseBakedMesh);
    }

    void InitializeSimulation()
    {
        if (_simulationInitialized || !HasSimulationAuthority) return;
        _simulationInitialized = true;
        // Preserve the authored transform position. NavMeshAgent.nextPosition may
        // include an internal vertical projection and caused respawns to drift upward.
        _spawnPos = transform.position;
        _spawnRot = transform.rotation;
        _health.onDeath.AddListener(OnDeath);
        _health.onDamageTaken.AddListener(OnDamageTakenServer);
        _health.onDamagedBy.AddListener(OnDamagedByServer);
        StartCoroutine(BehaviorLoop());
    }

    void ShutdownSimulation()
    {
        if (!_simulationInitialized || _health == null) return;
        _simulationInitialized = false;
        _health.onDeath.RemoveListener(OnDeath);
        _health.onDamageTaken.RemoveListener(OnDamageTakenServer);
        _health.onDamagedBy.RemoveListener(OnDamagedByServer);
    }

    void Update()
    {
        if (_animator != null && _hasSpeedParam)
            _animator.SetFloat(SpeedHash, _targetAnimatorSpeed, 0.12f, Time.deltaTime);

        // The behaviour state machine runs at 5 Hz, but combat facing needs to be
        // updated every frame or quick-moving targets visibly outrun the turn.
        if (HasSimulationAuthority && state == EnemyState.Attack && _target != null && !lockFacingOnAggro)
            FaceAttackTarget();
    }

    void LateUpdate()
    {
        if (keepCorpseGrounded && state == EnemyState.Dead && HasSimulationAuthority)
            StabilizeCorpseGrounding();

        if (lockFacingOnAggro && HasSimulationAuthority && _target != null &&
            (state == EnemyState.Chase || state == EnemyState.Attack))
            FaceTargetImmediately();
    }

    void StabilizeCorpseGrounding()
    {
        if (!_hasDeathGround || !TryGetVisibleRendererBottom(out float currentBottom)) return;

        float correction = _deathGroundY - currentBottom;
        if (Mathf.Abs(correction) < 0.002f) return;

        // Limit each correction to avoid a one-frame pop when a death clip changes pose.
        Vector3 position = transform.position;
        position.y += Mathf.Clamp(correction, -0.15f, 0.15f);
        transform.position = position;
    }

    bool TryGetVisibleRendererBottom(out float bottom)
    {
        bottom = 0f;
        Renderer primary = _corpseBodyRenderer;
        float largestVolume = -1f;

        // Prefer the largest skinned renderer: accessory meshes, weapons, VFX,
        // and selection rings must not decide where the corpse touches ground.
        foreach (var renderer in GetComponentsInChildren<SkinnedMeshRenderer>(true))
        {
            if (renderer == null || !renderer.enabled || !renderer.gameObject.activeInHierarchy) continue;
            Vector3 size = renderer.bounds.size;
            float volume = size.x * size.y * size.z;
            if (volume <= largestVolume) continue;
            largestVolume = volume;
            primary = renderer;
        }

        _corpseBodyRenderer = primary as SkinnedMeshRenderer;

        // Renderer.bounds may remain at the imported standing pose. BakeMesh reads
        // the currently deformed death pose, including hips/root-bone translation.
        if (_corpseBodyRenderer != null)
        {
            if (_corpseBakedMesh == null)
            {
                _corpseBakedMesh = new Mesh { name = name + "_CorpseGroundProbe" };
                _corpseBakedMesh.MarkDynamic();
            }

            _corpseBodyRenderer.BakeMesh(_corpseBakedMesh, true);
            Bounds bakedBounds = _corpseBakedMesh.bounds;
            if (bakedBounds.size.sqrMagnitude > 0f)
            {
                bottom = float.PositiveInfinity;
                Transform bodyTransform = _corpseBodyRenderer.transform;
                Vector3 min = bakedBounds.min;
                Vector3 max = bakedBounds.max;
                for (int x = 0; x < 2; x++)
                for (int y = 0; y < 2; y++)
                for (int z = 0; z < 2; z++)
                {
                    Vector3 corner = new Vector3(x == 0 ? min.x : max.x,
                        y == 0 ? min.y : max.y, z == 0 ? min.z : max.z);
                    bottom = Mathf.Min(bottom, bodyTransform.TransformPoint(corner).y);
                }
                return !float.IsInfinity(bottom);
            }
        }

        if (primary == null)
        {
            foreach (var renderer in GetComponentsInChildren<MeshRenderer>(true))
            {
                if (renderer == null || !renderer.enabled || !renderer.gameObject.activeInHierarchy) continue;
                Vector3 size = renderer.bounds.size;
                float volume = size.x * size.y * size.z;
                if (volume <= largestVolume) continue;
                largestVolume = volume;
                primary = renderer;
            }
        }

        if (primary == null) return false;
        bottom = primary.bounds.min.y;
        return true;
    }

    bool TryResolveDeathGround(out float groundY)
    {
        groundY = 0f;
        float sampleRadius = _agent != null ? Mathf.Max(2f, _agent.height) : 3f;
        int areaMask = _agent != null ? _agent.areaMask : NavMesh.AllAreas;
        if (NavMesh.SamplePosition(transform.position, out NavMeshHit navHit, sampleRadius, areaMask))
        {
            groundY = navHit.position.y;
            return true;
        }

        Vector3 origin = transform.position + Vector3.up * sampleRadius;
        float nearestDistance = float.PositiveInfinity;
        bool foundSurface = false;
        foreach (var hit in Physics.RaycastAll(origin, Vector3.down, sampleRadius * 3f,
                     Physics.AllLayers, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider != null && hit.collider.transform.IsChildOf(transform)) continue;
            if (hit.distance >= nearestDistance) continue;
            nearestDistance = hit.distance;
            groundY = hit.point.y;
            foundSurface = true;
        }
        if (foundSurface) return true;

        return TryGetVisibleRendererBottom(out groundY);
    }

    void FaceTargetImmediately()
    {
        Vector3 direction = _target.position - transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude > 0.0001f)
            transform.rotation = Quaternion.LookRotation(direction);
    }

    void FaceAttackTarget()
    {
        Vector3 direction = _target.position - transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude <= 0.0001f) return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = combatTurnSpeed <= 0f
            ? targetRotation
            : Quaternion.RotateTowards(transform.rotation, targetRotation, combatTurnSpeed * Time.deltaTime);
    }

    void OnDamageTakenServer(float amount)
    {
        if (amount > 0f && state != EnemyState.Dead)
            PlayGetHitAnimation();
    }

    [ClientRpc]
    void RpcPlayGetHitAnimation()
    {
#if UNITY_EDITOR || !UNITY_SERVER
        TriggerAnimator(GetHitHash, _hasGetHitParam);
#endif
    }

    void PlayGetHitAnimation()
    {
        if (NetworkServer.active) RpcPlayGetHitAnimation();
        else TriggerAnimator(GetHitHash, _hasGetHitParam);
    }

    public void SetAggroTarget(Transform target)
    {
        if (NetworkClient.active && !NetworkServer.active) return;
        if (state == EnemyState.Dead) return;

        _target = target;
        _returningHome = false;
        state = _target != null ? EnemyState.Chase : EnemyState.Idle;
        if (lockFacingOnAggro && _target != null)
            FaceTargetImmediately();
        _attackTimer = _target != null ? EnemyCrowdUtility.FirstAttackDelay(this, attackInterval) : 0f;
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Behavior loop
    // ─────────────────────────────────────────────────────────────────────────────

    IEnumerator BehaviorLoop()
    {
        var tick = new WaitForSeconds(0.2f);

        while (_health.IsAlive)
        {
            yield return tick;

            // Stagger: cannot act this tick
            if (_status != null && _status.IsStaggered) continue;

            switch (state)
            {
                case EnemyState.Idle:   TickIdle();   break;
                case EnemyState.Chase:  TickChase();  break;
                case EnemyState.Attack: TickAttack(); break;
            }
        }
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // State logic
    // ─────────────────────────────────────────────────────────────────────────────

    void TickIdle()
    {
        TickReturnHomeFacing();

        var hits = Physics.OverlapSphere(transform.position, aggroRadius);
        float     nearest = float.MaxValue;
        Transform found   = null;

        foreach (var col in hits)
        {
            if (!col.CompareTag("Player")) continue;
            var h = col.GetComponent<Health>();
            if (h == null || !h.IsAlive) continue;
            float d = Vector3.Distance(transform.position, col.transform.position);
            if (d < nearest) { nearest = d; found = col.transform; }
        }

        // NOTE: DynamicDifficultyScaler.maxAggroPerPlayer is intended to cap how
        // many enemies chase one player here — skip acquisition when `found`
        // already has that many chasers. Not yet wired (needs a per-player
        // chaser count); left as the enforcement seam. See §4 combat-feel doc.
        if (found != null) SetAggroTarget(found);
    }

    void OnDamagedByServer(GameObject source)
    {
        if (!aggroWhenDamaged || state == EnemyState.Dead) return;

        Transform attacker = ResolvePlayerTransform(source);
        if (attacker == null)
            attacker = FindNearestPlayer(Mathf.Max(aggroRadius, retaliationRadius));

        if (attacker == null) return;

        SetAggroTarget(attacker);
    }

    Transform ResolvePlayerTransform(GameObject source)
    {
        if (source == null) return null;

        Transform current = source.transform;
        while (current != null)
        {
            if (current.CompareTag("Player"))
                return current;

            Health sourceHealth = current.GetComponent<Health>();
            if (sourceHealth != null && sourceHealth.isPlayer && sourceHealth.IsAlive)
                return sourceHealth.transform;

            current = current.parent;
        }

        return null;
    }

    Transform FindNearestPlayer(float radius)
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        float radiusSqr = radius > 0f ? radius * radius : Mathf.Infinity;
        float best = Mathf.Infinity;
        Transform found = null;

        foreach (var player in players)
        {
            if (player == null) continue;

            Health playerHealth = player.GetComponent<Health>();
            if (playerHealth != null && !playerHealth.IsAlive) continue;

            float sqrDist = (player.transform.position - transform.position).sqrMagnitude;
            if (sqrDist > radiusSqr || sqrDist >= best) continue;

            best = sqrDist;
            found = player.transform;
        }

        return found;
    }

    void ReturnToSpawnPoint()
    {
        if (_agent == null || !_agent.isActiveAndEnabled) return;

        _returningHome = true;
        _agent.isStopped = false;
        _agent.speed = _baseSpeed;
        _agent.SetDestination(_spawnPos);
        FaceMoveDirection(_spawnPos);
    }

    void TickReturnHomeFacing()
    {
        if (!_returningHome) return;

        if (Vector3.Distance(transform.position, _spawnPos) <= 0.25f)
        {
            _returningHome = false;
            if (_agent != null && _agent.isActiveAndEnabled)
            {
                _agent.ResetPath();
                _agent.velocity = Vector3.zero;
            }
            transform.rotation = _spawnRot;
            return;
        }

        FaceMoveDirection(_spawnPos);
    }

    void FaceMoveDirection(Vector3 fallbackTarget)
    {
        Vector3 dir = Vector3.zero;

        if (_agent != null && _agent.isActiveAndEnabled)
        {
            dir = _agent.desiredVelocity.sqrMagnitude > 0.01f
                ? _agent.desiredVelocity
                : _agent.velocity;
        }

        if (dir.sqrMagnitude <= 0.01f)
            dir = fallbackTarget - transform.position;

        dir.y = 0f;
        if (dir.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.LookRotation(dir);
    }

    void TickChase()
    {
        // Target gone or dead → return home
        if (_target == null || !(_target.GetComponent<Health>()?.IsAlive ?? false))
        {
            ResetToIdle();
            ReturnToSpawnPoint();
            return;
        }

        // Leash check
        if (Vector3.Distance(transform.position, _spawnPos) > leashRadius)
        {
            ResetToIdle();
            ReturnToSpawnPoint();
            return;
        }

        // Bound — cannot move
        if (_status != null && _status.IsBound)
        {
            _agent?.ResetPath();
            return;
        }

        // Apply slow
        float slow = _status != null ? _status.GetSlowFraction() : 0f;
        if (_agent != null) _agent.speed = _baseSpeed * (1f - slow);

        float dist = Vector3.Distance(transform.position, _target.position);

        if (isRanged)
        {
            // Back off if too close; otherwise close to preferred range
            if (dist < tooCloseDistance)
            {
                Vector3 away = (transform.position - _target.position).normalized;
                _agent?.SetDestination(transform.position + away * 3f);
            }
            else
            {
                Vector3 slot = EnemyCrowdUtility.ChaseSlot(
                    transform,
                    _target,
                    Mathf.Max(preferredRange, tooCloseDistance + 0.5f),
                    1.1f);
                if (_agent != null) _agent.stoppingDistance = 0.65f;
                _agent?.SetDestination(slot);
            }

            if (dist <= attackRange) state = EnemyState.Attack;
        }
        else
        {
            Vector3 slot = EnemyCrowdUtility.ChaseSlot(transform, _target, EnemyCrowdUtility.MeleeSlotRadius(attackRange));
            if (_agent != null) _agent.stoppingDistance = 0.25f;
            _agent?.SetDestination(slot);

            if (EnemyCrowdUtility.CanMeleeAttack(transform, _target, slot, attackRange))
            {
                state = EnemyState.Attack;
            }
        }
    }

    void TickAttack()
    {
        if (_target == null || !(_target.GetComponent<Health>()?.IsAlive ?? false))
        {
            ResetToIdle();
            return;
        }

        float dist = Vector3.Distance(transform.position, _target.position);

        // Target stepped out of range — re-chase
        float allowedAttackRange = isRanged ? attackRange * 1.3f : EnemyCrowdUtility.MeleeAttackReach(attackRange);
        if (dist > allowedAttackRange) { state = EnemyState.Chase; return; }

        // Stand still for melee; keep pathing for ranged backpedal
        if (!isRanged)
        {
            Vector3 slot = EnemyCrowdUtility.ChaseSlot(transform, _target, EnemyCrowdUtility.MeleeSlotRadius(attackRange));
            if (EnemyCrowdUtility.ShouldMoveToMeleeSlot(transform, _target, slot, attackRange))
            {
                state = EnemyState.Chase;
                return;
            }

            _agent?.SetDestination(transform.position);
        }
        else
        {
            Vector3 slot = EnemyCrowdUtility.ChaseSlot(
                transform,
                _target,
                Mathf.Max(preferredRange, tooCloseDistance + 0.5f),
                1.1f);
            if (_agent != null) _agent.stoppingDistance = 0.65f;
            _agent?.SetDestination(slot);
        }

        FaceAttackTarget();

        // Tick attack cooldown (0.2s = one BehaviorLoop tick)
        _attackTimer -= 0.2f;
        if (_attackTimer > 0f) return;
        _attackTimer = attackInterval;

        PerformAttack();
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Attack
    // ─────────────────────────────────────────────────────────────────────────────

    void PerformAttack()
    {
        if (_attackInProgress) return;
        if (_target == null) return;
        var targetHealth = _target.GetComponent<Health>();
        if (targetHealth == null || !targetHealth.IsAlive) return;

        // Silenced: cannot attack (mirrors EnemyAI behaviour)
        if (_status != null && _status.IsSilenced) return;

        // Get target netId so the client Rpc can restrict hitstop to the hit player only
        var targetNetId = _target.GetComponent<NetworkIdentity>();
        uint hitNetId = targetNetId != null ? targetNetId.netId : 0u;

        if (isRanged) PlayRangedShot(hitNetId);
        else PlayMeleeSwing(hitNetId);

        StartCoroutine(ResolveAttackImpact(targetHealth));
    }

    IEnumerator ResolveAttackImpact(Health intendedTarget)
    {
        _attackInProgress = true;
        if (attackImpactDelay > 0f)
            yield return new WaitForSeconds(attackImpactDelay);

        _attackInProgress = false;
        if (!_simulationInitialized || state == EnemyState.Dead || intendedTarget == null || !intendedTarget.IsAlive)
            yield break;

        float allowedRange = isRanged ? attackRange * 1.3f : EnemyCrowdUtility.MeleeAttackReach(attackRange);
        if (Vector3.Distance(transform.position, intendedTarget.transform.position) > allowedRange)
            yield break;

        if (!isRanged || projectilePrefab == null)
        {
            intendedTarget.TakeDamage(damage, gameObject);
            yield break;
        }

        Vector3 spawnPos = transform.position + Vector3.up * 1.2f;
        Quaternion spawnRot = Quaternion.LookRotation(
            (intendedTarget.transform.position + Vector3.up * 0.5f) - spawnPos);
        var proj = Instantiate(projectilePrefab, spawnPos, spawnRot);
        var ep = proj.GetComponent<EnemyProjectile>();
        if (ep != null) ep.Init(damage);
        if (NetworkServer.active) NetworkServer.Spawn(proj);
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Death
    // ─────────────────────────────────────────────────────────────────────────────

    void OnDeath()
    {
        _corpseBodyRenderer = null;
        _hasDeathGround = keepCorpseGrounded && TryResolveDeathGround(out _deathGroundY);
        state = EnemyState.Dead;
        _returningHome = false;
        StopAllCoroutines();

        _health.SetEnemyTargetTagActive(false);
        if (_agent != null && _agent.isActiveAndEnabled) _agent.enabled = false;
        _deathDisabledColliders.Clear();
        foreach (var col in GetComponentsInChildren<Collider>(true))
        {
            if (!col.enabled) continue;
            col.enabled = false;
            _deathDisabledColliders.Add(col);
        }

        StartCoroutine(DeathSequence());
    }

    IEnumerator DeathSequence()
    {
        PlayDeathEffect();
        yield return new WaitForSeconds(0.4f);   // brief VFX moment

        if (dropTable != null)
        {
            var (items, gold) = dropTable.RollDrops();

            foreach (var (itemId, qty) in items)
            {
                SpawnWorldItem(itemId, qty);
                Debug.Log($"[LOOT] {name} dropped {qty}x {itemId}");
            }

            if (gold > 0)
            {
                SpawnWorldItem($"gold:{gold}", 1);
                Debug.Log($"[LOOT] {name} dropped {gold} gold");
            }
        }

        float remainingDeadModelTime = Mathf.Max(0f, deadModelVisibleSeconds - 0.4f);
        if (remainingDeadModelTime > 0f)
            yield return new WaitForSeconds(remainingDeadModelTime);

        // Notify clients of the kill so the LOCAL client can POST /api/combat/kill
        // with its own JWT. The server doesn't hold player JWTs — client-initiated
        // kill reports with the hit-gate anti-exploit design is the correct pattern.
        if (NetworkServer.active && !string.IsNullOrEmpty(enemyTemplateId))
            RpcNotifyEnemyKilled(enemyTemplateId);

        if (!respawnAfterDeath)
        {
            if (NetworkServer.active) NetworkServer.Destroy(gameObject);
            else Destroy(gameObject);
            yield break;
        }

        SetVisualsVisible(false);
        if (NetworkServer.active) RpcSetVisualsVisible(false);

        float delay = Mathf.Max(0f, respawnDelay);
        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        yield return RespawnAtSpawnPoint();
    }

    /// <summary>
    /// Fired on all clients after enemy death. Only the local client posts the kill
    /// report — remote clients ignore it (their own kills fire their own reports).
    /// </summary>
    [ClientRpc]
    void RpcNotifyEnemyKilled(string templateId)
    {
#if UNITY_EDITOR || !UNITY_SERVER
        // Only the local (owning) client sends the API call to avoid duplicate reports.
        // NetworkClient.localPlayer is null until a player spawns — guard it.
        if (NetworkClient.localPlayer == null) return;
        var pi = NetworkClient.localPlayer.GetComponent<PlayerIdentity>();
        if (pi == null) return;

        int    charId = pi.characterId;
        string token  = AuthManager.Token;
        if (charId <= 0 || string.IsNullOrEmpty(token)) return;

        // Kick off via CombatSessionTracker if available (it batches and retries);
        // otherwise fire directly.
        var tracker = CombatSessionTracker.Local;
        if (tracker != null)
            tracker.PostKill(charId, templateId, token);
        else
            StartCoroutine(DirectPostKill(charId, templateId, token));
    }

    System.Collections.IEnumerator DirectPostKill(int charId, string templateId, string token)
    {
        string url  = $"{ServerConfig.AuthBaseUrl}/api/combat/kill";
        string body = $"{{\"characterId\":{charId},\"enemyTemplateId\":\"{templateId}\"}}";

        using var req = new UnityEngine.Networking.UnityWebRequest(url, "POST");
        req.uploadHandler   = new UnityEngine.Networking.UploadHandlerRaw(
            System.Text.Encoding.UTF8.GetBytes(body));
        req.downloadHandler = new UnityEngine.Networking.DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        req.SetRequestHeader("Authorization", $"Bearer {token}");

        yield return req.SendWebRequest();

        if (req.result != UnityEngine.Networking.UnityWebRequest.Result.Success)
            Debug.LogWarning($"[COMBAT] kill POST failed: {req.error}");
        else
            Debug.Log($"[COMBAT] kill posted: char={charId} enemy={templateId}");
#endif
    }

    void SpawnWorldItem(string itemId, int qty)
    {
        if (worldItemPrefab == null)
        {
            Debug.LogWarning($"[COMBAT] {name}: worldItemPrefab not assigned — loot lost");
            return;
        }

        Vector3 offset = Random.insideUnitSphere * 1.2f;
        offset.y = 0.5f;

        var wi   = Instantiate(worldItemPrefab, transform.position + offset, Quaternion.identity);
        var comp = wi.GetComponent<WorldItem>();
        if (comp != null) { comp.itemId = itemId; comp.quantity = qty; }
        if (NetworkServer.active) NetworkServer.Spawn(wi);
    }

    IEnumerator RespawnAtSpawnPoint()
    {
        Debug.Log($"[EnemyController] Respawn started for '{name}' near {_spawnPos}.", this);
        Vector3 respawnPosition = _spawnPos;
        if (_agent != null)
        {
            if (_agent.enabled) _agent.enabled = false;
            const int attempts = 10;
            bool found = false;
            float sampleRadius = Mathf.Max(4f, _agent.height * 2f);
            for (int attempt = 0; attempt < attempts; attempt++)
            {
                if (NavMesh.SamplePosition(_spawnPos, out NavMeshHit hit, sampleRadius,
                    _agent.areaMask))
                {
                    respawnPosition = hit.position;
                    found = true;
                    break;
                }
                yield return new WaitForSeconds(0.5f);
            }

            if (!found)
            {
                Debug.LogError($"[EnemyController] Cannot respawn '{name}': no NavMesh point was found near {_spawnPos}.", this);
                yield break;
            }
        }

        transform.SetPositionAndRotation(respawnPosition, _spawnRot);
        _target = null;
        _attackTimer = 0f;
        _returningHome = false;

        if (_agent != null)
        {
            _agent.enabled = true;
            if (_agent.isOnNavMesh)
            {
                _agent.Warp(respawnPosition);
                _agent.ResetPath();
                _agent.velocity = Vector3.zero;
                _agent.isStopped = false;
                _agent.speed = _baseSpeed;
            }
            else
            {
                Debug.LogError($"[EnemyController] Respawn point for '{name}' was sampled but the agent could not join the NavMesh.", this);
                _agent.enabled = false;
                yield break;
            }
        }

        state = EnemyState.Idle;
        _status?.RemoveAll();
        _health.currentHealth = _health.maxHealth;
        _health.isInvulnerable = respawnProtectionSeconds > 0f;
        _health.SetEnemyTargetTagActive(true);
        _health.onHealthChanged?.Invoke(_health.currentHealth, _health.maxHealth);

        foreach (var col in _deathDisabledColliders)
            if (col != null) col.enabled = true;
        _deathDisabledColliders.Clear();

        SetVisualsVisible(true);
        ResetAnimators();
        _targetAnimatorSpeed = 0f;
        if (_animator != null)
        {
            _animator.ResetTrigger(AttackHash);
            _animator.ResetTrigger(GetHitHash);
            _animator.ResetTrigger(DeathHash);
            _animator.Play("Idle", 0, 0f);
            _animator.Update(0f);
        }
        Physics.SyncTransforms();
        BroadcastMessage("OnEnemyRespawned", SendMessageOptions.DontRequireReceiver);
        if (NetworkServer.active) RpcRespawn(respawnPosition, _spawnRot);

        StartCoroutine(BehaviorLoop());
        if (respawnProtectionSeconds > 0f)
            StartCoroutine(ClearRespawnProtection(respawnProtectionSeconds));
        Debug.Log($"[EnemyController] Respawn completed for '{name}' at {respawnPosition}; agentOnNavMesh={_agent == null || _agent.isOnNavMesh}.", this);
    }

    IEnumerator ClearRespawnProtection(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (_health != null) _health.isInvulnerable = false;
    }

    [ClientRpc]
    void RpcSetVisualsVisible(bool visible)
    {
        SetVisualsVisible(visible);
    }

    [ClientRpc]
    void RpcRespawn(Vector3 position, Quaternion rotation)
    {
        transform.SetPositionAndRotation(position, rotation);
        _health.SetEnemyTargetTagActive(true);
        SetVisualsVisible(true);
        ResetAnimators();
        _targetAnimatorSpeed = 0f;
        if (_animator != null)
        {
            _animator.ResetTrigger(AttackHash);
            _animator.ResetTrigger(GetHitHash);
            _animator.ResetTrigger(DeathHash);
            _animator.Play("Idle", 0, 0f);
            _animator.Update(0f);
        }
        Physics.SyncTransforms();
        BroadcastMessage("OnEnemyRespawned", SendMessageOptions.DontRequireReceiver);
    }

    void SetVisualsVisible(bool visible)
    {
        foreach (var renderer in GetComponentsInChildren<Renderer>(true))
            renderer.enabled = visible;
    }

    void ResetAnimators()
    {
        foreach (var animator in GetComponentsInChildren<Animator>(true))
        {
            animator.Rebind();
            animator.Update(0f);
            animator.Update(Mathf.Lerp(0.04f, 0.45f, EnemyCrowdUtility.Stable01(this, 59)));
        }
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────────────────────────────────────────

    void ResetToIdle()
    {
        _target      = null;
        state        = EnemyState.Idle;
        _attackTimer = 0f;
        if (_agent != null && _agent.isActiveAndEnabled) _agent.ResetPath();
        // Restore full speed (slow may have been applied)
        if (_agent != null) _agent.speed = _baseSpeed;
    }

    void OnStateChanged(EnemyState _, EnemyState newState)
    {
        SetAnimatorSpeed(newState == EnemyState.Chase ? 1f : 0f);

        if (newState == EnemyState.Dead)
            TriggerAnimator(DeathHash, _hasDeathParam);
    }

    bool IsEliteEnemy()
    {
        if (isElite)
            return true;

        if (!string.IsNullOrEmpty(enemyTemplateId)
            && enemyTemplateId.IndexOf("elite", System.StringComparison.OrdinalIgnoreCase) >= 0)
            return true;

        return name.IndexOf("elite", System.StringComparison.OrdinalIgnoreCase) >= 0;
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // RPCs (Week 7: wire anim + SFX)
    // ─────────────────────────────────────────────────────────────────────────────

    [ClientRpc]
    void RpcMeleeSwing(uint targetNetId)
    {
#if UNITY_EDITOR || !UNITY_SERVER
        TriggerAnimator(AttackHash, _hasAttackParam);

        bool eliteImpact = IsEliteEnemy();

        // Sound plays for everyone (positional audio sells the hit universally)
        CombatAudio.Instance?.PlayMeleeHit();

        // Hitstop and shake only on the client whose local player was hit.
        // Otherwise all 4 clients freeze every time any enemy swings at anyone.
        bool isLocalTarget = NetworkClient.localPlayer != null
                          && NetworkClient.localPlayer.GetComponent<NetworkIdentity>()?.netId == targetNetId;
        if (isLocalTarget)
        {
            HitstopManager.Freeze(eliteImpact ? HitstopManager.Weight.Medium : HitstopManager.Weight.Light);
            ScreenShake.AddTrauma(eliteImpact ? 0.20f : 0.12f);
        }
#endif
    }

    void PlayMeleeSwing(uint targetNetId)
    {
        if (NetworkServer.active) RpcMeleeSwing(targetNetId);
        else
        {
            TriggerAnimator(AttackHash, _hasAttackParam);
#if UNITY_EDITOR || !UNITY_SERVER
            CombatAudio.Instance?.PlayMeleeHit();
#endif
        }
    }

    [ClientRpc]
    void RpcRangedShot(uint targetNetId)
    {
#if UNITY_EDITOR || !UNITY_SERVER
        TriggerAnimator(AttackHash, _hasAttackParam);

        CombatAudio.Instance?.PlayRangedHit();
        // Ranged: no hitstop; light shake only on the targeted player's client
        bool isLocalTarget = NetworkClient.localPlayer != null
                          && NetworkClient.localPlayer.GetComponent<NetworkIdentity>()?.netId == targetNetId;
        if (isLocalTarget) ScreenShake.AddTrauma(0.10f);
#endif
    }

    void PlayRangedShot(uint targetNetId)
    {
        if (NetworkServer.active) RpcRangedShot(targetNetId);
        else
        {
            TriggerAnimator(AttackHash, _hasAttackParam);
#if UNITY_EDITOR || !UNITY_SERVER
            CombatAudio.Instance?.PlayRangedHit();
#endif
        }
    }

    [ClientRpc]
    void RpcPlayDeathEffect()
    {
#if UNITY_EDITOR || !UNITY_SERVER
        TriggerAnimator(DeathHash, _hasDeathParam);

        // Layer 3 — Death sound
        CombatAudio.Instance?.PlayDeath();

        // Layer 4 — Spawn death VFX at position (EnemyDeathVFX handles the actual prefab)
        FloatingDamageText.Spawn(transform.position + Vector3.up * 1.5f, 0,
            FloatingDamageText.DamageType.Normal, "✕");

        // Layer 5 — Kill-blow shake (stronger for elites)
        bool eliteImpact = IsEliteEnemy();
        ScreenShake.AddTrauma(eliteImpact ? 0.45f : 0.20f);

        // Kill-blow hitstop
        HitstopManager.Freeze(eliteImpact ? HitstopManager.Weight.Heavy : HitstopManager.Weight.Medium);
#endif
    }

    void PlayDeathEffect()
    {
        if (NetworkServer.active) RpcPlayDeathEffect();
        else
        {
            TriggerAnimator(DeathHash, _hasDeathParam);
#if UNITY_EDITOR || !UNITY_SERVER
            CombatAudio.Instance?.PlayDeath();
#endif
        }
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Gizmos
    // ─────────────────────────────────────────────────────────────────────────────

    void CacheAnimatorParameters()
    {
        if (_animator == null || _animator.runtimeAnimatorController == null)
            return;

        foreach (AnimatorControllerParameter parameter in _animator.parameters)
        {
            if (parameter.nameHash == SpeedHash && parameter.type == AnimatorControllerParameterType.Float)
                _hasSpeedParam = true;
            else if (parameter.nameHash == AttackHash && parameter.type == AnimatorControllerParameterType.Trigger)
                _hasAttackParam = true;
            else if (parameter.nameHash == GetHitHash && parameter.type == AnimatorControllerParameterType.Trigger)
                _hasGetHitParam = true;
            else if (parameter.nameHash == DeathHash && parameter.type == AnimatorControllerParameterType.Trigger)
                _hasDeathParam = true;
        }

        if (_hasGetHitParam && _animator.runtimeAnimatorController is AnimatorOverrideController overrideController)
        {
            var mappings = new List<KeyValuePair<AnimationClip, AnimationClip>>();
            overrideController.GetOverrides(mappings);
            bool hasGetHitClip = mappings.Exists(pair => pair.Key != null &&
                pair.Key.name == "EnemyForge_GetHit" && pair.Value != null);
            _hasGetHitParam = hasGetHitClip;
        }
    }

    void SetAnimatorSpeed(float speed)
    {
        _targetAnimatorSpeed = speed;
    }

    void TriggerAnimator(int hash, bool hasParameter)
    {
        if (_animator != null && hasParameter)
            _animator.SetTrigger(hash);
    }

    void OnDrawGizmosSelected()
    {
        Vector3 origin = Application.isPlaying ? _spawnPos : transform.position;

        Gizmos.color = new Color(1f, 1f, 0f, 0.35f);
        Gizmos.DrawWireSphere(transform.position, aggroRadius);

        Gizmos.color = new Color(1f, 0.1f, 0.1f, 0.6f);
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = new Color(0.2f, 0.6f, 1f, 0.15f);
        Gizmos.DrawWireSphere(origin, leashRadius);
    }
}
