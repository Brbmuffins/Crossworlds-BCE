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

    // ── Telegraph ────────────────────────────────────────────────────────────────
    [Header("Attack Telegraph")]
    [Tooltip("Seconds of red AoE preview shown before the attack lands. 0 = no telegraph.")]
    public float telegraphDuration = 0.45f;

    // ── VFX / Audio (client-side only) ───────────────────────────────────────────
    [Header("VFX — assign from brbmuffins packs")]
    [Tooltip("Impact spark on melee hit, e.g. Sparks red.prefab or MetalImpacts.prefab")]
    public GameObject meleeImpactVFX;
    [Tooltip("Explosion/smoke on death, e.g. SmallExplosion.prefab or Smoke puff.prefab")]
    public GameObject deathVFX;

    // ── XP / Gold reward (broadcast to all players on death) ─────────────────────
    [Header("XP Reward")]
    [Tooltip("XP awarded to every player in the scene on death")]
    public int xpReward   = 20;
    [Tooltip("Bonus gold awarded to every player (on top of drop table gold)")]
    public int goldReward = 0;

    [Header("API")]
    [Tooltip("Matches enemy_templates.id in the DB (e.g. 'goblin_grunt'). When set, death calls /api/combat/hit + /api/combat/kill for server-authoritative XP/gold. Leave empty to use local xpReward/goldReward only.")]
    public string enemyTemplateId = "";

    // ── Private ──────────────────────────────────────────────────────────────────
    private Health               _health;
    private NavMeshAgent         _agent;
    private StatusEffectManager  _status;   // may be null on basic enemies
    private float                _baseSpeed;
    private Transform            _target;
    private Vector3              _spawnPos;
    private float                _attackTimer;
#if !UNITY_SERVER
    private Animator             _anim;     // cached on OnStartClient; null if no mesh yet
#endif

    // ─────────────────────────────────────────────────────────────────────────────

    void Awake()
    {
        _health    = GetComponent<Health>();
        _agent     = GetComponent<NavMeshAgent>();
        _status    = GetComponent<StatusEffectManager>();
        _baseSpeed = _agent != null ? _agent.speed : 0f;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        _spawnPos = transform.position;
        _health.onDeath.AddListener(OnDeath);
        StartCoroutine(BehaviorLoop());
    }

#if !UNITY_SERVER
    public override void OnStartClient()
    {
        base.OnStartClient();
        // Cache animator from child mesh (assigned by EnemyBuilder or Inspector).
        _anim = GetComponentInChildren<Animator>();
        // Show floating damage numbers + play GetHit anim on every hit (client-local).
        _health.onDamageTaken.AddListener(OnClientDamageTaken);
        // Attach world-space health bar (self-builds its Canvas in Awake).
        if (GetComponent<EnemyHealthBar>() == null)
            gameObject.AddComponent<EnemyHealthBar>();
        // Death VFX — procedural burst fallback when no prefab is assigned.
        if (GetComponent<EnemyDeathVFX>() == null)
            gameObject.AddComponent<EnemyDeathVFX>();
        // Session stats: OnStartClient fires on every client for every spawned
        // enemy (host or dedicated server), unlike WaveSpawner's server-only path.
        CombatSessionTracker.Local?.NotifyEnemySpawned(gameObject);
    }

    void OnClientDamageTaken(float amount)
    {
        Vector3 spawnPos = transform.position + Vector3.up * 1.8f;
        var type = amount >= damage * 1.3f
            ? FloatingDamageText.DamageType.Critical
            : FloatingDamageText.DamageType.Normal;
        FloatingDamageText.Spawn(spawnPos, amount, type);
        _anim?.SetTrigger("GetHit");
    }
#endif

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

        if (found != null) { _target = found; state = EnemyState.Chase; }
    }

    [Server]
    void TickChase()
    {
        // Target gone or dead → return home
        if (_target == null || !(_target.GetComponent<Health>()?.IsAlive ?? false))
        {
            ResetToIdle();
            return;
        }

        // Leash check
        if (Vector3.Distance(transform.position, _spawnPos) > leashRadius)
        {
            ResetToIdle();
            _agent?.SetDestination(_spawnPos);
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

        StartCoroutine(AttackSequence());
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

        if (!isRanged)
        {
            targetHealth.TakeDamage(damage, gameObject);
            RpcMeleeSwing();
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
            RpcRangedShot();
        }
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Attack sequence with telegraph
    // ─────────────────────────────────────────────────────────────────────────────

    [Server]
    IEnumerator AttackSequence()
    {
        if (telegraphDuration > 0f && _target != null)
        {
            // Show red indicator at attack landing zone before damage lands
            Vector3 telegraphPos = isRanged
                ? transform.position + transform.forward * preferredRange  // rough projectile landing
                : _target.position;                                         // melee: target feet

            float radius = isRanged ? 0.6f : attackRange;
            RpcShowTelegraph(telegraphPos, radius, telegraphDuration);
            yield return new WaitForSeconds(telegraphDuration);
        }

        PerformAttack();
    }

    [ClientRpc]
    void RpcShowTelegraph(Vector3 center, float radius, float duration)
    {
#if !UNITY_SERVER
        // Red flat cylinder on the ground — same technique as AbilityCaster indicators
        var indicator = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        indicator.transform.position = center + Vector3.up * 0.02f;
        indicator.transform.localScale = new Vector3(radius * 2f, 0.02f, radius * 2f);

        var col = indicator.GetComponent<Collider>();
        if (col != null) Object.Destroy(col);

        var mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = new Color(1f, 0.12f, 0.12f, 0.55f);
        indicator.GetComponent<Renderer>().material = mat;

        Object.Destroy(indicator, duration);
#endif
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Death
    // ─────────────────────────────────────────────────────────────────────────────

    [Server]
    void OnDeath()
    {
        state = EnemyState.Dead;
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

        // Award XP and gold to all clients
        if (xpReward > 0 || goldReward > 0)
            RpcAwardProgress(xpReward, goldReward);

        yield return new WaitForSeconds(2.6f);
        NetworkServer.Destroy(gameObject);
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
#if !UNITY_SERVER
        if (_anim == null) _anim = GetComponentInChildren<Animator>(); // late-bind if mesh added after spawn
        if (_anim == null) return;

        bool isMoving = newState == EnemyState.Chase || newState == EnemyState.Attack;
        _anim.SetFloat("Speed", isMoving ? 1f : 0f);

        if (newState == EnemyState.Dead)
            _anim.SetTrigger("Death");
#endif
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // RPCs (Week 7: wire anim + SFX)
    // ─────────────────────────────────────────────────────────────────────────────

    [ClientRpc]
    void RpcMeleeSwing()
    {
#if !UNITY_SERVER
        _anim?.SetTrigger("Attack");
        CombatAudio.Instance?.PlayMeleeHit();

        if (meleeImpactVFX != null)
        {
            Vector3 hitPos = transform.position + Vector3.up * 1f;
            var fx = Instantiate(meleeImpactVFX, hitPos, Quaternion.identity);
            Destroy(fx, 2f);
        }
#endif
    }

    [ClientRpc]
    void RpcRangedShot()
    {
#if !UNITY_SERVER
        _anim?.SetTrigger("Attack");
        CombatAudio.Instance?.PlayRangedHit();
#endif
    }

    [ClientRpc]
    void RpcPlayDeathEffect()
    {
#if !UNITY_SERVER
        _anim?.SetTrigger("Death");
        CombatAudio.Instance?.PlayDeath();

        if (deathVFX != null)
        {
            Vector3 pos = transform.position + Vector3.up * 0.5f;
            var fx = Instantiate(deathVFX, pos, Quaternion.identity);
            Destroy(fx, 3f);
        }
#endif
    }

    [ClientRpc]
    void RpcAwardProgress(int xp, int gold)
    {
#if !UNITY_SERVER
        if (!string.IsNullOrEmpty(enemyTemplateId))
        {
            // Server-authoritative path: hit + kill API awards XP/gold and logs loot.
            // Fallback to local award if the API fails (network error, auth missing).
            StartCoroutine(PostCombatKill(enemyTemplateId, xp, gold));
        }
        else
        {
            if (xp   > 0) PlayerProgressManager.Local?.AwardXp(xp);
            if (gold > 0) PlayerProgressManager.Local?.AwardGold(gold);
        }
#endif
    }

#if !UNITY_SERVER
    IEnumerator PostCombatKill(string templateId, int fallbackXp, int fallbackGold)
    {
        int charId    = AuthManager.CharacterId;
        string jwt    = AuthManager.Token;
        if (charId <= 0 || string.IsNullOrEmpty(jwt))
        {
            // No auth — fall back to local award so XP always registers
            if (fallbackXp   > 0) PlayerProgressManager.Local?.AwardXp(fallbackXp);
            if (fallbackGold > 0) PlayerProgressManager.Local?.AwardGold(fallbackGold);
            yield break;
        }

        byte[] HitBody(string tid, float dmg)
        {
            string j = $"{{\"characterId\":{charId},\"enemyTemplateId\":\"{tid}\",\"damageDealt\":{dmg}}}";
            return System.Text.Encoding.UTF8.GetBytes(j);
        }

        // 1. POST hit (satisfies the server's 30s hit-gate before kill)
        using (var hitReq = new UnityWebRequest($"{ServerConfig.AuthBaseUrl}/api/combat/hit", "POST"))
        {
            hitReq.uploadHandler   = new UploadHandlerRaw(HitBody(templateId, damage));
            hitReq.downloadHandler = new DownloadHandlerBuffer();
            hitReq.SetRequestHeader("Content-Type", "application/json");
            hitReq.SetRequestHeader("Authorization", $"Bearer {jwt}");
            hitReq.timeout = 5;
            yield return hitReq.SendWebRequest();
            // hit failure is non-fatal — kill will fail the gate below and we fallback
        }

        // 2. POST kill — server awards XP + gold + rolls loot into DB
        string killJson = $"{{\"characterId\":{charId},\"enemyTemplateId\":\"{templateId}\"}}";
        using (var killReq = new UnityWebRequest($"{ServerConfig.AuthBaseUrl}/api/combat/kill", "POST"))
        {
            killReq.uploadHandler   = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(killJson));
            killReq.downloadHandler = new DownloadHandlerBuffer();
            killReq.SetRequestHeader("Content-Type", "application/json");
            killReq.SetRequestHeader("Authorization", $"Bearer {jwt}");
            killReq.timeout = 8;
            yield return killReq.SendWebRequest();

            if (killReq.result == UnityWebRequest.Result.Success)
            {
                // Refresh progress from server so XP bar + gold reflect DB truth
                PlayerProgressManager.Local?.Refresh();
            }
            else
            {
                Debug.LogWarning($"[COMBAT] Kill API failed ({killReq.responseCode}): {killReq.error} — awarding locally");
                if (fallbackXp   > 0) PlayerProgressManager.Local?.AwardXp(fallbackXp);
                if (fallbackGold > 0) PlayerProgressManager.Local?.AwardGold(fallbackGold);
            }
        }
    }
#endif

    // ─────────────────────────────────────────────────────────────────────────────
    // Gizmos
    // ─────────────────────────────────────────────────────────────────────────────

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
