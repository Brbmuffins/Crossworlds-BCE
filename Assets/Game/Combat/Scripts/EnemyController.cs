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

    // ── DB Template ID ────────────────────────────────────────────────────────────
    // Must match an id in the enemy_templates DB table (e.g. "grunt_basic").
    // Used by PostCombatKill to award XP/gold via POST /api/combat/kill.
    // Assign in the Inspector on each prefab after running seed_arena_content_2026-07-06.sql.
    [Header("Server")]
    public string enemyTemplateId = "grunt_basic";

    // ── Private ──────────────────────────────────────────────────────────────────
    private Health               _health;
    private NavMeshAgent         _agent;
    private StatusEffectManager  _status;   // may be null on basic enemies
    private float                _baseSpeed;
    private Transform            _target;
    private Vector3              _spawnPos;
    private float                _attackTimer;

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

        yield return new WaitForSeconds(2.6f);

        // Notify clients of the kill so the LOCAL client can POST /api/combat/kill
        // with its own JWT. The server doesn't hold player JWTs — client-initiated
        // kill reports with the hit-gate anti-exploit design is the correct pattern.
        if (!string.IsNullOrEmpty(enemyTemplateId))
            RpcNotifyEnemyKilled(enemyTemplateId);

        NetworkServer.Destroy(gameObject);
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
        // Hook animator here when animation rig is ready (Week 7)
        // GetComponent<Animator>()?.SetInteger("state", (int)newState);
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // RPCs (Week 7: wire anim + SFX)
    // ─────────────────────────────────────────────────────────────────────────────

    [ClientRpc]
    void RpcMeleeSwing(uint targetNetId)
    {
#if UNITY_EDITOR || !UNITY_SERVER
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
