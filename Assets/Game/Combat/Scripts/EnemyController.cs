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
    public const int EnemyForgeRuntimeProfileVersion = 27;

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
    [Header("Roaming")]
    [Tooltip("Allows this enemy to choose server-authoritative NavMesh destinations while idle.")]
    public bool enableRoaming = true;
    [Min(0f)] public float roamingRadius = 8f;
    [Min(0f)] public float roamingMinWait = 2f;
    [Min(0f)] public float roamingMaxWait = 5f;

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
    [Tooltip("Bit mask of configured Enemy Forge attack animation slots. Bit 0 is Combat Attack 1.")]
    public int attackAnimationVariantMask = 1;
    [Tooltip("Impact delay for each Enemy Forge attack animation slot.")]
    public float[] attackAnimationImpactDelays = { 0.35f, 0.35f, 0.35f, 0.35f };
    [Header("Animation Playback")]
    [Range(0.25f, 3f)] public float idleAnimationSpeed = 1f;
    [Range(0.25f, 3f)] public float chaseAnimationSpeed = 1f;
    public float[] attackAnimationSpeeds = { 1f, 1f, 1f, 1f };
    [Range(0.25f, 3f)] public float getHitAnimationSpeed = 1f;
    [Range(0.25f, 3f)] public float deathAnimationSpeed = 1f;
    [Tooltip("Local offsets from the right hand (or body fallback) for Attack 1-4 VFX origins.")]
    public Vector3[] attackVfxOffsets =
        { Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero };

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
    [Tooltip("Main animated body used for corpse grounding. Weapons and accessories must not be assigned.")]
    public SkinnedMeshRenderer corpseGroundingRenderer;
    [Tooltip("Model-specific vertical adjustment after grounding. Negative values lower the corpse.")]
    [Range(-0.5f, 0.5f)] public float corpseGroundOffset = -0.05f;

    // ── Private ──────────────────────────────────────────────────────────────────
    private Health               _health;
    private NavMeshAgent         _agent;
    private NetworkTransformBase _networkTransform;
    private StatusEffectManager  _status;   // may be null on basic enemies
    private EnemyHeavyAttack     _heavyAttack;
    private EnemySfxProfile      _sfx;
    private EnemyPatrolAgent     _patrolAgent;
    private float                _baseSpeed;
    private float                _configuredStoppingDistance;
    private Transform            _target;
    public Transform CurrentTarget => _target;
    public bool IsReturningHome => _returningHome;
    private Vector3              _spawnPos;
    private Vector3              _combatLeashOrigin;
    private Quaternion           _spawnRot;
    private float                _attackTimer;
    private int                  _activeAttackVariant;
    private Animator             _animator;
    private bool                 _hasSpeedParam;
    private bool                 _hasAttackParam;
    private bool                 _hasAttackVariantParam;
    private bool                 _hasGetHitParam;
    private bool                 _hasDeathParam;
    private bool                 _returningHome;
    private float                _targetAnimatorSpeed;
    private readonly List<Collider> _deathDisabledColliders = new List<Collider>();
    private bool                 _simulationInitialized;
    private bool                 _attackInProgress;
    private bool                 _hasRoamDestination;
    private float                _nextRoamTime;
    private float                _deathGroundY;
    private float                _deathRootY;
    private bool                 _hasDeathGround;
    private float                _corpseSnapUntil;
    private bool                 _corpseGroundingReported;
    private SkinnedMeshRenderer  _corpseBodyRenderer;
    private Mesh                 _corpseBakedMesh;
    private bool                 _corpseRenderStateCaptured;
    private readonly List<Animator> _corpseAnimators = new List<Animator>();
    private readonly List<AnimatorCullingMode> _corpseAnimatorCullingModes = new List<AnimatorCullingMode>();
    private readonly List<SkinnedMeshRenderer> _corpseRenderers = new List<SkinnedMeshRenderer>();
    private readonly List<bool> _corpseRendererUpdateModes = new List<bool>();

    bool HasSimulationAuthority => NetworkServer.active ||
        (allowOfflineSimulation && !NetworkClient.active && !NetworkServer.active);

    static readonly int SpeedHash = Animator.StringToHash("Speed");
    static readonly int AttackHash = Animator.StringToHash("Attack");
    static readonly int AttackVariantHash = Animator.StringToHash("AttackVariant");
    static readonly int GetHitHash = Animator.StringToHash("GetHit");
    static readonly int DeathHash = Animator.StringToHash("Death");

    // ─────────────────────────────────────────────────────────────────────────────

    void Awake()
    {
        _health    = GetComponent<Health>();
        _agent     = GetComponent<NavMeshAgent>();
        _networkTransform = GetComponent<NetworkTransformBase>();
        _status    = GetComponent<StatusEffectManager>();
        _heavyAttack = GetComponent<EnemyHeavyAttack>();
        _sfx = GetComponent<EnemySfxProfile>();
        _patrolAgent = GetComponent<EnemyPatrolAgent>();
        _baseSpeed = _agent != null ? _agent.speed : 0f;
        _configuredStoppingDistance = _agent != null ? Mathf.Max(0f, _agent.stoppingDistance) : 0f;
        _animator  = GetComponentInChildren<Animator>();
        CacheAnimatorParameters();
        ApplyAnimationPlaybackSpeeds();
    }

    void Start()
    {
        if (allowOfflineSimulation && !NetworkClient.active && !NetworkServer.active)
            InitializeSimulation();
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        // Scene-added components can be restored after Awake during prefab/scene
        // deserialization. Refresh here so every networked patrol instance is seen.
        _patrolAgent = GetComponent<EnemyPatrolAgent>();
        QuestEnemyTarget.EnsureAttached(gameObject);
        InitializeSimulation();
    }

    void ApplyAnimationPlaybackSpeeds()
    {
        if (_animator == null) return;
        SetAnimatorFloatIfPresent("IdleSpeed", idleAnimationSpeed);
        SetAnimatorFloatIfPresent("ChaseSpeed", chaseAnimationSpeed);
        for (int i = 0; i < 4; i++)
        {
            float speed = attackAnimationSpeeds != null && i < attackAnimationSpeeds.Length
                ? attackAnimationSpeeds[i] : 1f;
            SetAnimatorFloatIfPresent($"Attack{i + 1}Speed", speed);
        }
        SetAnimatorFloatIfPresent("GetHitSpeed", getHitAnimationSpeed);
        SetAnimatorFloatIfPresent("DeathSpeed", deathAnimationSpeed);
    }

    void SetAnimatorFloatIfPresent(string parameterName, float value)
    {
        foreach (AnimatorControllerParameter parameter in _animator.parameters)
        {
            if (parameter.type != AnimatorControllerParameterType.Float ||
                parameter.name != parameterName) continue;
            _animator.SetFloat(parameterName, Mathf.Clamp(value, 0.25f, 3f));
            return;
        }
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
        _combatLeashOrigin = _spawnPos;
        _spawnRot = transform.rotation;
        _hasRoamDestination = false;
        ScheduleNextRoam();
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
        if (HasSimulationAuthority)
            UpdateAnimatorLocomotionTarget();

        // On remote clients NetworkAnimator owns synchronized parameters. Writing
        // Speed here would overwrite the value received from the server.
        if (HasSimulationAuthority && _animator != null && _hasSpeedParam)
        {
            // Stop locomotion immediately when navigation stops. Damping a zero
            // value kept the walk cycle running after the body was stationary.
            if (_targetAnimatorSpeed <= 0f)
                _animator.SetFloat(SpeedHash, 0f);
            else
                _animator.SetFloat(SpeedHash, _targetAnimatorSpeed, 0.08f, Time.deltaTime);
        }

        // The behaviour state machine runs at 5 Hz, but combat facing needs to be
        // updated every frame or quick-moving targets visibly outrun the turn.
        if (HasSimulationAuthority && _target != null &&
            (state == EnemyState.Chase || state == EnemyState.Attack))
            FaceAttackTarget();
    }

    void UpdateAnimatorLocomotionTarget()
    {
        if (state == EnemyState.Dead || _attackInProgress ||
            (_status != null && _status.IsBound))
        {
            _targetAnimatorSpeed = 0f;
            return;
        }

        if (_agent == null || !_agent.isActiveAndEnabled)
        {
            _targetAnimatorSpeed = state == EnemyState.Chase ? 1f : 0f;
            return;
        }

        float movementSpeed = Mathf.Max(_agent.velocity.magnitude, _agent.desiredVelocity.magnitude);
        float movingThreshold = Mathf.Max(0.05f, _baseSpeed * 0.05f);
        _targetAnimatorSpeed = movementSpeed > movingThreshold ? 1f : 0f;
    }

    void LateUpdate()
    {
        if (keepCorpseGrounded && state == EnemyState.Dead && HasSimulationAuthority)
            StabilizeCorpseGrounding();

    }

    void StabilizeCorpseGrounding()
    {
        if (!_hasDeathGround) return;

        // Apply the model-specific offset directly from the grounded root position
        // captured at death. Animated mesh bounds can contain hidden/outlier
        // vertices and previously counteracted the user's requested adjustment.
        float targetRootY = _deathRootY + corpseGroundOffset;
        float correction = targetRootY - transform.position.y;
        if (Mathf.Abs(correction) < 0.002f) return;

        bool snapImmediately = Time.time <= _corpseSnapUntil;
        float appliedCorrection = snapImmediately
            ? correction
            : Mathf.Clamp(correction, -0.15f, 0.15f);
        Vector3 position = transform.position;
        position.y += appliedCorrection;
        transform.position = position;

        // Death grounding is a deliberate server-authoritative relocation.
        // Teleport during the short opening correction window so Mirror
        // interpolation cannot hide or delay model-specific corpse offsets.
        if (snapImmediately && NetworkServer.active && _networkTransform != null)
            _networkTransform.ServerTeleport(position, transform.rotation);

        if (!_corpseGroundingReported)
        {
            _corpseGroundingReported = true;
            Debug.Log($"[EnemyController] Corpse grounding '{name}': ground={_deathGroundY:F3}, " +
                      $"baseRootY={_deathRootY:F3}, offset={corpseGroundOffset:F3}, " +
                      $"targetRootY={targetRootY:F3}, applied={appliedCorrection:F3}.", this);
        }
    }

    bool TryGetVisibleRendererBottom(out float bottom)
    {
        bottom = 0f;
        Renderer primary = _corpseBodyRenderer != null ? _corpseBodyRenderer : corpseGroundingRenderer;
        float largestVolume = -1f;

        // An explicit Enemy Forge body reference is authoritative. Automatic
        // selection remains as a safe fallback for older/non-forged prefabs.
        if (primary == null)
        {
            foreach (var renderer in GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                if (renderer == null || !renderer.enabled || !renderer.gameObject.activeInHierarchy) continue;
                Vector3 size = renderer.bounds.size;
                float volume = size.x * size.y * size.z;
                if (volume <= largestVolume) continue;
                largestVolume = volume;
                primary = renderer;
            }
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

            // Bake in renderer-local space. TransformPoint below applies the
            // hierarchy scale exactly once; useScale=true would double-apply it.
            _corpseBodyRenderer.BakeMesh(_corpseBakedMesh, false);
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
        Vector3 origin = transform.position + Vector3.up * sampleRadius;
        float nearestGroundDistance = float.PositiveInfinity;
        float nearestSurfaceDistance = float.PositiveInfinity;
        float nearestGroundY = 0f;
        float nearestSurfaceY = 0f;
        bool foundTaggedGround = false;
        bool foundSurface = false;
        foreach (var hit in ZonePhysics.RaycastAll(gameObject, origin, Vector3.down, sampleRadius * 3f,
                     Physics.AllLayers, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider == null || hit.collider.transform.IsChildOf(transform)) continue;
            if (hit.normal.y < 0.5f) continue;

            if (hit.collider.CompareTag("Ground"))
            {
                if (hit.distance >= nearestGroundDistance) continue;
                nearestGroundDistance = hit.distance;
                nearestGroundY = hit.point.y;
                foundTaggedGround = true;
                continue;
            }

            if (hit.distance >= nearestSurfaceDistance) continue;
            nearestSurfaceDistance = hit.distance;
            nearestSurfaceY = hit.point.y;
            foundSurface = true;
        }

        if (foundTaggedGround)
        {
            groundY = nearestGroundY;
            return true;
        }

        if (foundSurface)
        {
            groundY = nearestSurfaceY;
            return true;
        }

        int areaMask = _agent != null ? _agent.areaMask : NavMesh.AllAreas;
        if (NavMesh.SamplePosition(transform.position, out NavMeshHit navHit, sampleRadius, areaMask))
        {
            groundY = navHit.position.y;
            return true;
        }

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
        _sfx?.PlayGetHit();
#endif
    }

    void PlayGetHitAnimation()
    {
        if (NetworkServer.active) RpcPlayGetHitAnimation();
        else
        {
            TriggerAnimator(GetHitHash, _hasGetHitParam);
#if UNITY_EDITOR || !UNITY_SERVER
            _sfx?.PlayGetHit();
#endif
        }
    }

    public void SetAggroTarget(Transform target)
    {
        if (NetworkClient.active && !NetworkServer.active) return;
        if (state == EnemyState.Dead) return;
        if (_returningHome && target != null) return;

        bool enteringAggro = _target == null && target != null;
        if (enteringAggro)
        {
            _combatLeashOrigin = transform.position;
            _patrolAgent?.SuspendForCombat();
        }
        _target = target;
        _returningHome = false;
        _hasRoamDestination = false;
        if (_agent != null && _agent.isActiveAndEnabled && _agent.isOnNavMesh)
            _agent.ResetPath();
        state = _target != null ? EnemyState.Chase : EnemyState.Idle;
        if (_target != null)
            FaceTargetImmediately();
        _attackTimer = _target != null ? EnemyCrowdUtility.FirstAttackDelay(this, attackInterval) : 0f;
        if (enteringAggro)
        {
            _heavyAttack?.OnAggroAcquired(_target);
            PlayAggroSfx();
        }
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
        // Also recover after editor hot reloads or runtime component changes.
        if (_patrolAgent == null)
            _patrolAgent = GetComponent<EnemyPatrolAgent>();

        TickReturnHomeFacing();
        if (_returningHome) return;

        // A destination can be inside the roaming circle while the NavMesh path
        // curves outside it. Enforce the enemy's actual idle position as well.
        if (enableRoaming && roamingRadius > 0f &&
            IsOutsideHomeRadius(transform.position, roamingRadius, 0.25f))
        {
            ReturnToSpawnPoint();
            return;
        }

        var hits = ZonePhysics.OverlapSphere(gameObject, transform.position, aggroRadius);
        float     nearest = float.MaxValue;
        Transform found   = null;

        foreach (var col in hits)
        {
            if (col.transform.IsChildOf(transform)) continue;
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
        if (found != null)
        {
            SetAggroTarget(found);
            return;
        }

        TickRoaming();
    }

    void TickRoaming()
    {
        if (!enableRoaming || roamingRadius <= 0f || _returningHome ||
            _agent == null || !_agent.isActiveAndEnabled || !_agent.isOnNavMesh)
            return;

        if (_hasRoamDestination)
        {
            if (_agent.pathPending) return;
            float arrivalDistance = Mathf.Max(0.25f, _agent.stoppingDistance + 0.1f);
            if (_agent.hasPath && _agent.remainingDistance > arrivalDistance) return;

            _hasRoamDestination = false;
            _agent.ResetPath();
            ScheduleNextRoam();
            return;
        }

        if (Time.time < _nextRoamTime) return;

        int areaMask = _agent.areaMask;
        for (int attempt = 0; attempt < 8; attempt++)
        {
            Vector2 offset2D = Random.insideUnitCircle * roamingRadius;
            Vector3 candidate = _spawnPos + new Vector3(offset2D.x, 0f, offset2D.y);
            float sampleRadius = Mathf.Max(1.5f, _agent.height);
            if (!NavMesh.SamplePosition(candidate, out NavMeshHit hit, sampleRadius, areaMask)) continue;
            Vector3 horizontal = hit.position - _spawnPos;
            horizontal.y = 0f;
            if (horizontal.sqrMagnitude > roamingRadius * roamingRadius) continue;

            _agent.isStopped = false;
            _agent.speed = _baseSpeed;
            _agent.stoppingDistance = 0.15f;
            if (!_agent.SetDestination(hit.position)) continue;
            _hasRoamDestination = true;
            return;
        }

        _nextRoamTime = Time.time + 1f;
    }

    void ScheduleNextRoam()
    {
        float minWait = Mathf.Max(0f, roamingMinWait);
        float maxWait = Mathf.Max(minWait, roamingMaxWait);
        _nextRoamTime = Time.time + Random.Range(minWait, maxWait);
    }

    void OnDamagedByServer(GameObject source)
    {
        if (!aggroWhenDamaged || state == EnemyState.Dead || _returningHome) return;

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
            PlayerIdentity identity = player.GetComponent<PlayerIdentity>();
            if (identity != null && !identity.zoneVisualsReady) continue;

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

        _target = null;
        _attackTimer = 0f;
        _attackInProgress = false;
        _hasRoamDestination = false;
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
            ScheduleNextRoam();
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
            ResumePatrolOrReturnHome();
            return;
        }

        // Leash check
        if (IsOutsideCombatLeash(transform.position) || !IsTargetWithinLeash(_target))
        {
            ResetToIdle();
            ResumePatrolOrReturnHome();
            return;
        }

        // Opening casts are server-authoritative and hold the caster in place
        // until the configured animation windup resolves or is cancelled.
        if (isRanged && _heavyAttack != null && _heavyAttack.IsOpeningCastInProgress)
        {
            if (_agent != null && _agent.isActiveAndEnabled && _agent.isOnNavMesh)
            {
                _agent.ResetPath();
                _agent.velocity = Vector3.zero;
            }
            FaceAttackTarget();
            return;
        }

        // Dedicated casters hold their ground anywhere inside their maximum
        // cast range. Leaving that range resumes normal NavMesh pursuit.
        if (isRanged && _heavyAttack != null && _heavyAttack.ShouldHoldCastingPosition(_target))
        {
            if (_agent != null && _agent.isActiveAndEnabled && _agent.isOnNavMesh)
            {
                _agent.ResetPath();
                _agent.velocity = Vector3.zero;
            }
            FaceAttackTarget();
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
            // Enemy Forge's stopping distance is the minimum ring around the
            // player. The small agent stopping distance below is only tolerance
            // around this per-enemy crowd slot, not player spacing.
            float slotRadius = Mathf.Max(
                EnemyCrowdUtility.MeleeSlotRadius(attackRange),
                _configuredStoppingDistance);
            Vector3 slot = EnemyCrowdUtility.ChaseSlot(transform, _target, slotRadius);
            if (_agent != null) _agent.stoppingDistance = 0.1f;
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
            ResumePatrolOrReturnHome();
            return;
        }

        if (IsOutsideCombatLeash(transform.position) || !IsTargetWithinLeash(_target))
        {
            ResetToIdle();
            ResumePatrolOrReturnHome();
            return;
        }

        float dist = Vector3.Distance(transform.position, _target.position);

        // Target stepped out of range — re-chase
        float allowedAttackRange = isRanged ? attackRange * 1.3f : EnemyCrowdUtility.MeleeAttackReach(attackRange);
        if (dist > allowedAttackRange) { state = EnemyState.Chase; return; }

        // Stand still for melee; keep pathing for ranged backpedal
        if (!isRanged)
        {
            float slotRadius = Mathf.Max(
                EnemyCrowdUtility.MeleeSlotRadius(attackRange),
                _configuredStoppingDistance);
            Vector3 slot = EnemyCrowdUtility.ChaseSlot(transform, _target, slotRadius);
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

        // Heavy spells own their cast animation through the configured impact
        // frame. Do not let the normal ranged loop restart that animation.
        if (isRanged && _heavyAttack != null && _heavyAttack.IsAbilityInProgress)
            return;

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

        int attackVariant = SelectAttackAnimationVariant();
        _activeAttackVariant = attackVariant;
        if (isRanged) PlayRangedShot(hitNetId, attackVariant);
        else PlayMeleeSwing(hitNetId, attackVariant);

        StartCoroutine(ResolveAttackImpact(targetHealth, GetAttackImpactDelay(attackVariant)));
    }

    IEnumerator ResolveAttackImpact(Health intendedTarget, float impactDelay)
    {
        _attackInProgress = true;
        if (impactDelay > 0f)
            yield return new WaitForSeconds(impactDelay);

        _attackInProgress = false;
        if (!_simulationInitialized || state == EnemyState.Dead || _returningHome ||
            intendedTarget == null || !intendedTarget.IsAlive)
            yield break;

        float allowedRange = isRanged ? attackRange * 1.3f : EnemyCrowdUtility.MeleeAttackReach(attackRange);
        if (Vector3.Distance(transform.position, intendedTarget.transform.position) > allowedRange)
            yield break;

        PlayImpactSfx();
        if (!isRanged || projectilePrefab == null)
        {
            intendedTarget.TakeDamage(damage, gameObject);
            yield break;
        }

        Vector3 spawnPos = ResolveAttackVfxOrigin(_activeAttackVariant);
        Quaternion spawnRot = Quaternion.LookRotation(
            (intendedTarget.transform.position + Vector3.up * 0.5f) - spawnPos);
        var proj = Instantiate(projectilePrefab, spawnPos, spawnRot);
        var ep = proj.GetComponent<EnemyProjectile>();
        if (ep != null) ep.Init(damage);
        ZoneScene.PlaceWith(proj, gameObject);   // keep the projectile in this enemy's zone
        if (NetworkServer.active) NetworkServer.Spawn(proj);
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Death
    // ─────────────────────────────────────────────────────────────────────────────

    void OnDeath()
    {
        _heavyAttack?.OnCombatEnded();
        PrepareCorpseRendering();
        _corpseBodyRenderer = corpseGroundingRenderer;
        _deathRootY = transform.position.y;
        _hasDeathGround = keepCorpseGrounded && TryResolveDeathGround(out _deathGroundY);
        _corpseSnapUntil = Time.time + 0.3f;
        _corpseGroundingReported = false;
        state = EnemyState.Dead;
        _returningHome = false;
        _hasRoamDestination = false;
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
        ZoneScene.PlaceWith(wi, gameObject);   // drop belongs to the zone the enemy died in
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
        _hasRoamDestination = false;
        ScheduleNextRoam();
        _status?.RemoveAll();
        _health.currentHealth = _health.maxHealth;
        _health.isInvulnerable = respawnProtectionSeconds > 0f;
        _health.SetEnemyTargetTagActive(true);
        _health.onHealthChanged?.Invoke(_health.currentHealth, _health.maxHealth);

        foreach (var col in _deathDisabledColliders)
            if (col != null) col.enabled = true;
        _deathDisabledColliders.Clear();

        SetVisualsVisible(true);
        RestoreCorpseRendering();
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
        RestoreCorpseRendering();
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

    void PrepareCorpseRendering()
    {
        if (_corpseRenderStateCaptured) return;
        _corpseRenderStateCaptured = true;
        _corpseAnimators.Clear();
        _corpseAnimatorCullingModes.Clear();
        _corpseRenderers.Clear();
        _corpseRendererUpdateModes.Clear();

        foreach (var animator in GetComponentsInChildren<Animator>(true))
        {
            if (animator == null) continue;
            _corpseAnimators.Add(animator);
            _corpseAnimatorCullingModes.Add(animator.cullingMode);
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        }

        foreach (var renderer in GetComponentsInChildren<SkinnedMeshRenderer>(true))
        {
            if (renderer == null) continue;
            _corpseRenderers.Add(renderer);
            _corpseRendererUpdateModes.Add(renderer.updateWhenOffscreen);
            renderer.updateWhenOffscreen = true;
        }
    }

    void RestoreCorpseRendering()
    {
        if (!_corpseRenderStateCaptured) return;

        for (int i = 0; i < _corpseAnimators.Count && i < _corpseAnimatorCullingModes.Count; i++)
            if (_corpseAnimators[i] != null)
                _corpseAnimators[i].cullingMode = _corpseAnimatorCullingModes[i];

        for (int i = 0; i < _corpseRenderers.Count && i < _corpseRendererUpdateModes.Count; i++)
            if (_corpseRenderers[i] != null)
                _corpseRenderers[i].updateWhenOffscreen = _corpseRendererUpdateModes[i];

        _corpseAnimators.Clear();
        _corpseAnimatorCullingModes.Clear();
        _corpseRenderers.Clear();
        _corpseRendererUpdateModes.Clear();
        _corpseRenderStateCaptured = false;
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────────────────────────────────────────

    void ResetToIdle()
    {
        _heavyAttack?.OnCombatEnded();
        _target      = null;
        state        = EnemyState.Idle;
        _attackTimer = 0f;
        _hasRoamDestination = false;
        ScheduleNextRoam();
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

    void PlayAggroSfx()
    {
        if (NetworkServer.active) RpcPlayAggroSfx();
#if UNITY_EDITOR || !UNITY_SERVER
        else _sfx?.PlayAggro();
#endif
    }

    [ClientRpc]
    void RpcPlayAggroSfx()
    {
#if UNITY_EDITOR || !UNITY_SERVER
        _sfx?.PlayAggro();
#endif
    }

    void PlayImpactSfx()
    {
        if (NetworkServer.active) RpcPlayImpactSfx();
#if UNITY_EDITOR || !UNITY_SERVER
        else _sfx?.PlayImpact();
#endif
    }

    [ClientRpc]
    void RpcPlayImpactSfx()
    {
#if UNITY_EDITOR || !UNITY_SERVER
        _sfx?.PlayImpact();
#endif
    }

    [ClientRpc]
    void RpcMeleeSwing(uint targetNetId, int attackVariant)
    {
#if UNITY_EDITOR || !UNITY_SERVER
        PlayAttackAnimation(attackVariant);

        bool eliteImpact = IsEliteEnemy();

        // Sound plays for everyone (positional audio sells the hit universally)
        if (_sfx == null || !_sfx.PlayAttack(attackVariant))
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

    void PlayMeleeSwing(uint targetNetId, int attackVariant)
    {
        if (NetworkServer.active) RpcMeleeSwing(targetNetId, attackVariant);
        else
        {
            PlayAttackAnimation(attackVariant);
#if UNITY_EDITOR || !UNITY_SERVER
            if (_sfx == null || !_sfx.PlayAttack(attackVariant))
                CombatAudio.Instance?.PlayMeleeHit();
#endif
        }
    }

    [ClientRpc]
    void RpcRangedShot(uint targetNetId, int attackVariant)
    {
#if UNITY_EDITOR || !UNITY_SERVER
        PlayAttackAnimation(attackVariant);

        if (_sfx == null || !_sfx.PlayAttack(attackVariant))
            CombatAudio.Instance?.PlayRangedHit();
        // Ranged: no hitstop; light shake only on the targeted player's client
        bool isLocalTarget = NetworkClient.localPlayer != null
                          && NetworkClient.localPlayer.GetComponent<NetworkIdentity>()?.netId == targetNetId;
        if (isLocalTarget) ScreenShake.AddTrauma(0.10f);
#endif
    }

    void PlayRangedShot(uint targetNetId, int attackVariant)
    {
        if (NetworkServer.active) RpcRangedShot(targetNetId, attackVariant);
        else
        {
            PlayAttackAnimation(attackVariant);
#if UNITY_EDITOR || !UNITY_SERVER
            if (_sfx == null || !_sfx.PlayAttack(attackVariant))
                CombatAudio.Instance?.PlayRangedHit();
#endif
        }
    }

    public bool IsTargetWithinLeash(Transform target)
    {
        if (target == null) return false;
        return !IsOutsideRadius(target.position, CombatLeashOrigin, leashRadius);
    }

    bool IsOutsideCombatLeash(Vector3 position)
    {
        return IsOutsideRadius(position, CombatLeashOrigin, leashRadius);
    }

    Vector3 CombatLeashOrigin =>
        _patrolAgent != null && _patrolAgent.HasUsableRoute ? _combatLeashOrigin : _spawnPos;

    void ResumePatrolOrReturnHome()
    {
        if (_patrolAgent != null && _patrolAgent.HasUsableRoute)
        {
            _patrolAgent.ResumeFromNearestWaypoint();
            return;
        }
        ReturnToSpawnPoint();
    }

    bool IsOutsideHomeRadius(Vector3 position, float radius, float tolerance = 0f)
    {
        return IsOutsideRadius(position, _spawnPos, radius, tolerance);
    }

    static bool IsOutsideRadius(
        Vector3 position, Vector3 origin, float radius, float tolerance = 0f)
    {
        float allowedRadius = Mathf.Max(0f, radius + tolerance);
        Vector3 fromSpawn = position - origin;
        fromSpawn.y = 0f;
        return fromSpawn.sqrMagnitude > allowedRadius * allowedRadius;
    }

    [Server]
    public float PlayCastAnimation()
    {
        return PlayCastAnimation(out _);
    }

    [Server]
    public float PlayCastAnimation(out int attackVariant)
    {
        attackVariant = SelectAttackAnimationVariant();
        _activeAttackVariant = attackVariant;
        RpcCastAnimation(attackVariant);
        return GetAttackImpactDelay(attackVariant);
    }

    public Vector3 ResolveCurrentAttackVfxOrigin() =>
        ResolveAttackVfxOrigin(_activeAttackVariant);

    public Vector3 ResolveAttackVfxOrigin(int attackVariant)
    {
        Vector3 offset = attackVfxOffsets != null &&
            attackVariant >= 0 && attackVariant < attackVfxOffsets.Length
            ? attackVfxOffsets[attackVariant]
            : Vector3.zero;
        return ResolveAttackVfxOrigin(
            transform, _animator, GetComponentsInChildren<Renderer>(true), offset);
    }

    public static Vector3 ResolveAttackVfxOrigin(
        Transform root, Animator animator, Renderer[] renderers, Vector3 offset)
    {
        if (root == null) return offset;
        if (animator != null && animator.isHuman)
        {
            Transform hand = animator.GetBoneTransform(HumanBodyBones.RightHand);
            if (hand != null)
                return hand.position + root.TransformDirection(offset);
        }

        // Generic rigs have no HumanBodyBones mapping. Derive a stable chest/cast
        // position from visible bounds so different source-root scales match runtime.
        if (renderers != null && renderers.Length > 0)
        {
            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
                if (renderers[i] != null && renderers[i].enabled)
                    bounds.Encapsulate(renderers[i].bounds);
            Vector3 chest = new Vector3(
                bounds.center.x,
                Mathf.Lerp(bounds.min.y, bounds.max.y, 0.62f),
                bounds.center.z);
            chest += root.forward * Mathf.Max(0.05f, bounds.extents.z * 0.25f);
            return chest + root.TransformDirection(offset);
        }

        return root.position + root.up * 1.2f + root.forward * 0.35f +
            root.TransformDirection(offset);
    }

    [Server]
    public void CancelCastAnimation()
    {
        RpcCancelCastAnimation();
    }

    [ClientRpc]
    void RpcCastAnimation(int attackVariant)
    {
#if UNITY_EDITOR || !UNITY_SERVER
        PlayAttackAnimation(attackVariant);
        if (_sfx == null || !_sfx.PlayAttack(attackVariant))
            CombatAudio.Instance?.PlayAbilityCast();
#endif
    }

    [ClientRpc]
    void RpcCancelCastAnimation()
    {
#if UNITY_EDITOR || !UNITY_SERVER
        if (_animator == null) return;
        if (_hasAttackParam) _animator.ResetTrigger(AttackHash);
        _animator.CrossFade("Chase", 0.08f);
#endif
    }

    [ClientRpc]
    void RpcPlayDeathEffect()
    {
#if UNITY_EDITOR || !UNITY_SERVER
        PrepareCorpseRendering();
        TriggerAnimator(DeathHash, _hasDeathParam);

        // Layer 3 — Death sound
        if (_sfx == null || !_sfx.PlayDeath())
            CombatAudio.Instance?.PlayDeath();

        // Layer 4 — Spawn death VFX at position (EnemyDeathVFX handles the actual prefab)
        FloatingDamageText.Spawn(transform.position + Vector3.up * 1.5f, 0,
            FloatingDamageText.DamageType.Normal, "×");

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
            if (_sfx == null || !_sfx.PlayDeath())
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
            else if (parameter.nameHash == AttackVariantHash && parameter.type == AnimatorControllerParameterType.Int)
                _hasAttackVariantParam = true;
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

    int SelectAttackAnimationVariant()
    {
        int mask = attackAnimationVariantMask | 1;
        int count = 0;
        for (int i = 0; i < 4; i++)
            if ((mask & (1 << i)) != 0)
                count++;

        int pick = UnityEngine.Random.Range(0, count);
        for (int i = 0; i < 4; i++)
        {
            if ((mask & (1 << i)) == 0) continue;
            if (pick-- == 0) return i;
        }
        return 0;
    }

    public float GetAttackImpactDelay(int attackVariant)
    {
        if (attackAnimationImpactDelays != null &&
            attackVariant >= 0 && attackVariant < attackAnimationImpactDelays.Length)
            return Mathf.Max(0f, attackAnimationImpactDelays[attackVariant]);
        return Mathf.Max(0f, attackImpactDelay);
    }

    void PlayAttackAnimation(int attackVariant)
    {
        if (_animator != null && _hasAttackVariantParam)
            _animator.SetInteger(AttackVariantHash, Mathf.Clamp(attackVariant, 0, 3));
        TriggerAnimator(AttackHash, _hasAttackParam);
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
