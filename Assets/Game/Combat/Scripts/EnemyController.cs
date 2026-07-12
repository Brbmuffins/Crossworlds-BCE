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
    public enum EnemyState { Idle, Chase, Attack, Dead }

    [SyncVar(hook = nameof(OnStateChanged))]
    public EnemyState state = EnemyState.Idle;

    // ── Detection ────────────────────────────────────────────────────────────────
    [Header("Detection")]
    public float aggroRadius = 8f;
    public float leashRadius = 20f;
    public bool aggroWhenDamaged = true;
    [Min(0f)] public float retaliationRadius = 20f;

    // ── Combat ───────────────────────────────────────────────────────────────────
    [Header("Combat")]
    public float attackRange    = 1.5f;
    public float attackInterval = 1.5f;
    public float damage         = 12f;

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
    private bool                 _hasDeathParam;
    private bool                 _returningHome;

    static readonly int SpeedHash = Animator.StringToHash("Speed");
    static readonly int AttackHash = Animator.StringToHash("Attack");
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

    public override void OnStartServer()
    {
        base.OnStartServer();
        _spawnPos = transform.position;
        _spawnRot = transform.rotation;
        _health.onDeath.AddListener(OnDeath);
        _health.onDamagedBy.AddListener(OnDamagedByServer);
        StartCoroutine(BehaviorLoop());
    }

    public override void OnStopServer()
    {
        if (_health != null)
        {
            _health.onDeath.RemoveListener(OnDeath);
            _health.onDamagedBy.RemoveListener(OnDamagedByServer);
        }

        base.OnStopServer();
    }

    public void SetAggroTarget(Transform target)
    {
        if (NetworkClient.active && !NetworkServer.active) return;
        if (state == EnemyState.Dead) return;

        _target = target;
        _returningHome = false;
        state = _target != null ? EnemyState.Chase : EnemyState.Idle;
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Behavior loop
    // ─────────────────────────────────────────────────────────────────────────────

    [Server]
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

    [Server]
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

        if (found != null) SetAggroTarget(found);
    }

    [Server]
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

    [Server]
    void ReturnToSpawnPoint()
    {
        if (_agent == null || !_agent.isActiveAndEnabled) return;

        _returningHome = true;
        _agent.isStopped = false;
        _agent.speed = _baseSpeed;
        _agent.SetDestination(_spawnPos);
        FaceMoveDirection(_spawnPos);
    }

    [Server]
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

    [Server]
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
                _agent?.SetDestination(_target.position);

            if (dist <= attackRange) state = EnemyState.Attack;
        }
        else
        {
            _agent?.SetDestination(_target.position);
            if (dist <= attackRange) state = EnemyState.Attack;
        }
    }

    [Server]
    void TickAttack()
    {
        if (_target == null || !(_target.GetComponent<Health>()?.IsAlive ?? false))
        {
            ResetToIdle();
            return;
        }

        float dist = Vector3.Distance(transform.position, _target.position);

        // Target stepped out of range — re-chase
        if (dist > attackRange * 1.3f) { state = EnemyState.Chase; return; }

        // Stand still for melee; keep pathing for ranged backpedal
        if (!isRanged) _agent?.SetDestination(transform.position);

        // Face target
        Vector3 dir = (_target.position - transform.position); dir.y = 0f;
        if (dir.sqrMagnitude > 0.01f) transform.rotation = Quaternion.LookRotation(dir);

        // Tick attack cooldown (0.2s = one BehaviorLoop tick)
        _attackTimer -= 0.2f;
        if (_attackTimer > 0f) return;
        _attackTimer = attackInterval;

        PerformAttack();
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Attack
    // ─────────────────────────────────────────────────────────────────────────────

    [Server]
    void PerformAttack()
    {
        if (_target == null) return;
        var targetHealth = _target.GetComponent<Health>();
        if (targetHealth == null || !targetHealth.IsAlive) return;

        // Silenced: cannot attack (mirrors EnemyAI behaviour)
        if (_status != null && _status.IsSilenced) return;

        // Get target netId so the client Rpc can restrict hitstop to the hit player only
        var targetNetId = _target.GetComponent<NetworkIdentity>();
        uint hitNetId = targetNetId != null ? targetNetId.netId : 0u;

        if (!isRanged)
        {
            targetHealth.TakeDamage(damage, gameObject);
            RpcMeleeSwing(hitNetId);
        }
        else
        {
            if (projectilePrefab != null)
            {
                Vector3    spawnPos = transform.position + Vector3.up * 1.2f;
                Quaternion spawnRot = Quaternion.LookRotation(
                    (_target.position + Vector3.up * 0.5f) - spawnPos);
                var proj = Instantiate(projectilePrefab, spawnPos, spawnRot);
                var ep   = proj.GetComponent<EnemyProjectile>();
                if (ep != null) ep.Init(damage);
                NetworkServer.Spawn(proj);
            }
            else
            {
                // Fallback instant damage if no projectile prefab set
                targetHealth.TakeDamage(damage, gameObject);
            }
            RpcRangedShot(hitNetId);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Death
    // ─────────────────────────────────────────────────────────────────────────────

    [Server]
    void OnDeath()
    {
        state = EnemyState.Dead;
        _returningHome = false;
        StopAllCoroutines();

        if (_agent != null && _agent.isActiveAndEnabled) _agent.enabled = false;
        foreach (var col in GetComponents<Collider>()) col.enabled = false;

        StartCoroutine(DeathSequence());
    }

    [Server]
    IEnumerator DeathSequence()
    {
        RpcPlayDeathEffect();
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
        if (!string.IsNullOrEmpty(enemyTemplateId))
            RpcNotifyEnemyKilled(enemyTemplateId);

        if (!respawnAfterDeath)
        {
            NetworkServer.Destroy(gameObject);
            yield break;
        }

        SetVisualsVisible(false);
        RpcSetVisualsVisible(false);

        float delay = Mathf.Max(0f, respawnDelay);
        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        RespawnAtSpawnPoint();
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

    [Server]
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
        NetworkServer.Spawn(wi);
    }

    [Server]
    void RespawnAtSpawnPoint()
    {
        transform.SetPositionAndRotation(_spawnPos, _spawnRot);
        _target = null;
        _attackTimer = 0f;
        _returningHome = false;
        state = EnemyState.Idle;

        _status?.RemoveAll();
        _health.currentHealth = _health.maxHealth;
        _health.onHealthChanged?.Invoke(_health.currentHealth, _health.maxHealth);

        foreach (var col in GetComponents<Collider>())
            col.enabled = true;

        if (_agent != null)
        {
            _agent.enabled = true;
            if (_agent.isOnNavMesh)
            {
                _agent.Warp(_spawnPos);
                _agent.ResetPath();
                _agent.velocity = Vector3.zero;
                _agent.isStopped = false;
                _agent.speed = _baseSpeed;
            }
        }

        SetVisualsVisible(true);
        ResetAnimators();
        BroadcastMessage("OnEnemyRespawned", SendMessageOptions.DontRequireReceiver);
        RpcRespawn(_spawnPos, _spawnRot);

        StartCoroutine(BehaviorLoop());
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
        SetVisualsVisible(true);
        ResetAnimators();
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
        }
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────────────────────────────────────────

    [Server]
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

    // ─────────────────────────────────────────────────────────────────────────────
    // RPCs (Week 7: wire anim + SFX)
    // ─────────────────────────────────────────────────────────────────────────────

    [ClientRpc]
    void RpcMeleeSwing(uint targetNetId)
    {
#if UNITY_EDITOR || !UNITY_SERVER
        TriggerAnimator(AttackHash, _hasAttackParam);

        bool isElite = CompareTag("Elite");

        // Sound plays for everyone (positional audio sells the hit universally)
        CombatAudio.Instance?.PlayMeleeHit();

        // Hitstop and shake only on the client whose local player was hit.
        // Otherwise all 4 clients freeze every time any enemy swings at anyone.
        bool isLocalTarget = NetworkClient.localPlayer != null
                          && NetworkClient.localPlayer.GetComponent<NetworkIdentity>()?.netId == targetNetId;
        if (isLocalTarget)
        {
            HitstopManager.Freeze(isElite ? HitstopManager.Weight.Medium : HitstopManager.Weight.Light);
            ScreenShake.AddTrauma(isElite ? 0.20f : 0.12f);
        }
#endif
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
        bool isElite = CompareTag("Elite");
        ScreenShake.AddTrauma(isElite ? 0.45f : 0.20f);

        // Kill-blow hitstop
        HitstopManager.Freeze(isElite ? HitstopManager.Weight.Heavy : HitstopManager.Weight.Medium);
#endif
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
            else if (parameter.nameHash == DeathHash && parameter.type == AnimatorControllerParameterType.Trigger)
                _hasDeathParam = true;
        }
    }

    void SetAnimatorSpeed(float speed)
    {
        if (_animator != null && _hasSpeedParam)
            _animator.SetFloat(SpeedHash, speed);
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
