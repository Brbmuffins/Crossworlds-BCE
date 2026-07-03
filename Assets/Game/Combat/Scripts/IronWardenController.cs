using System.Collections;
using UnityEngine;
using Mirror;

/// <summary>
/// The Iron Warden — second world boss.
///
/// Phase 1 (100–60 %): Siege Protocol
///   Barrier Wall orbits every 12 s; Mortar Strike every 20 s (5 staggered blasts, 30 dmg each).
///
/// Phase 2 (60–25 %): Shield Matrix
///   Boss immune until both Siege Turrets destroyed simultaneously.
///   Survivor auto-repairs after 10 s if only one dies. Magnet Pull every 20 s.
///
/// Phase 3 (25–0 %): Rampage
///   Ground Slam every 4 s (35 dmg, 6u). Speed +50 %.
///   At 15 % HP: Lockdown (Bound 5 s) then Devastation slam (60 dmg, 10u).
///
/// Follows same server-authoritative pattern as WorldBossController:
///   Health.onHealthChanged drives phase transitions (no polling).
///   Health.onDeath drives the death sequence.
///   All game-state on [Server]; VFX/announcements via [ClientRpc].
/// </summary>
[RequireComponent(typeof(Health))]
[RequireComponent(typeof(StatusEffectManager))]
public class IronWardenController : NetworkBehaviour
{
    // ── Inspector ──────────────────────────────────────────────────────────────
    [Header("Phase Thresholds (0–1)")]
    public float phase2Threshold   = 0.60f;
    public float phase3Threshold   = 0.25f;
    public float lockdownThreshold = 0.15f;

    [Header("Phase 1 — Siege Protocol")]
    public float barrierInterval = 12f;
    public float mortarInterval  = 20f;
    public int   mortarDamage    = 30;
    public float mortarRadius    = 4f;
    public int   mortarCount     = 5;

    [Header("Phase 2 — Shield Matrix")]
    public GameObject siegeTurretPrefab;    // assign in Inspector (editor step)
    public float magnetInterval    = 20f;
    public int   magnetStompDamage = 60;
    public float magnetRadius      = 8f;
    public float turretRepairDelay = 10f;

    [Header("Phase 3 — Rampage")]
    public float slamInterval      = 4f;
    public int   slamDamage        = 35;
    public float slamRadius        = 6f;
    public float rampageSpeedMult  = 1.5f;
    public int   lockdownDuration  = 5;
    public int   devastationDamage = 60;
    public float devastationRadius = 10f;

    [Header("Telegraph")]
    public GameObject warningCirclePrefab;  // assign in Inspector

    [Header("Drop Table")]
    [Tooltip("WorldItem.prefab — must be registered in NetworkManager.spawnPrefabs")]
    public GameObject worldItemPrefab;
    public System.Collections.Generic.List<string> guaranteedDropItemIds =
        new System.Collections.Generic.List<string> { "helm_iron", "chest_iron" };
    public System.Collections.Generic.List<string> rareDropItemIds =
        new System.Collections.Generic.List<string> { "helm_gold", "flask_damage" };
    [Range(0f, 1f)] public float rareDropChance = 0.30f;

    // ── Network state ──────────────────────────────────────────────────────────
    public enum WardenPhase { Dormant, SiegeProtocol, ShieldMatrix, Rampage, Dead }

    [SyncVar(hook = nameof(OnPhaseChanged))]
    public WardenPhase currentPhase = WardenPhase.Dormant;

    // ── Private (server only) ──────────────────────────────────────────────────
    Health         _health;
    UnityEngine.AI.NavMeshAgent _agent;
    float          _baseSpeed;

    Coroutine _barrierRoutine;
    Coroutine _mortarRoutine;
    Coroutine _magnetRoutine;
    Coroutine _slamRoutine;

    bool _lockdownFired;
    Coroutine _repairRoutine;
    int  _turretsAlive;
    bool _inTransition;
    readonly System.Collections.Generic.List<GameObject> _liveTurrets =
        new System.Collections.Generic.List<GameObject>();

    // ── Lifecycle ──────────────────────────────────────────────────────────────
    void Awake()
    {
        _health = GetComponent<Health>();
        _agent  = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (_agent != null) _baseSpeed = _agent.speed;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        _health.onHealthChanged.AddListener(OnHealthChanged);
        _health.onDeath.AddListener(OnWardenDeath);
    }

    // ── Entry point (called by arena trigger) ──────────────────────────────────
    [Server]
    public void Activate()
    {
        if (currentPhase != WardenPhase.Dormant) return;
        RpcAnnounce("[BOSS] The Iron Warden awakens — SIEGE PROTOCOL initiated!");
        TransitionTo(WardenPhase.SiegeProtocol);
    }

    // ── Health event → phase transitions ──────────────────────────────────────
    [Server]
    void OnHealthChanged(float current, float max)
    {
        if (_inTransition || currentPhase == WardenPhase.Dead) return;

        // Immune during Phase 2 while turrets live — Health still fires events
        // but we don't transition down on those hits
        float fraction = max > 0f ? current / max : 0f;

        if (currentPhase == WardenPhase.SiegeProtocol && fraction <= phase2Threshold)
        {
            TransitionTo(WardenPhase.ShieldMatrix);
            return;
        }

        if (currentPhase == WardenPhase.ShieldMatrix && fraction <= phase3Threshold)
        {
            TransitionTo(WardenPhase.Rampage);
            return;
        }

        if (currentPhase == WardenPhase.Rampage
            && !_lockdownFired && fraction <= lockdownThreshold)
        {
            _lockdownFired = true;
            StartCoroutine(LockdownSequence());
        }
    }

    [Server]
    void OnWardenDeath()
    {
        TransitionTo(WardenPhase.Dead);
    }

    // ── Phase transition ───────────────────────────────────────────────────────
    [Server]
    void TransitionTo(WardenPhase next)
    {
        _inTransition = true;
        StopPhaseCoroutines();
        currentPhase = next;

        switch (next)
        {
            case WardenPhase.SiegeProtocol:
                _barrierRoutine = StartCoroutine(BarrierWallLoop());
                _mortarRoutine  = StartCoroutine(MortarStrikeLoop());
                break;

            case WardenPhase.ShieldMatrix:
                RpcAnnounce("[BOSS] Shield Matrix online — destroy BOTH turrets simultaneously!");
                _turretsAlive = 2;
                _health.SetDamageReduction(1f);   // immune while turrets stand
                SpawnSiegeTurrets();
                _magnetRoutine = StartCoroutine(MagnetPullLoop());
                break;

            case WardenPhase.Rampage:
                RpcAnnounce("[BOSS] The Iron Warden's core is exposed — RAMPAGE!");
                _health.ClearDamageReduction();
                DespawnTurrets();
                if (_agent != null) _agent.speed = _baseSpeed * rampageSpeedMult;
                _slamRoutine = StartCoroutine(GroundSlamLoop());
                break;

            case WardenPhase.Dead:
                if (_agent != null) _agent.speed = 0f;
                RpcAnnounce("[BOSS] The Iron Warden has fallen!");
                StartCoroutine(DeathSequence());
                break;
        }
        _inTransition = false;
    }

    void StopPhaseCoroutines()
    {
        if (_barrierRoutine != null) { StopCoroutine(_barrierRoutine); _barrierRoutine = null; }
        if (_mortarRoutine  != null) { StopCoroutine(_mortarRoutine);  _mortarRoutine  = null; }
        if (_magnetRoutine  != null) { StopCoroutine(_magnetRoutine);  _magnetRoutine  = null; }
        if (_slamRoutine    != null) { StopCoroutine(_slamRoutine);    _slamRoutine    = null; }
        if (_repairRoutine  != null) { StopCoroutine(_repairRoutine);  _repairRoutine  = null; }
    }

    [Server]
    void DespawnTurrets()
    {
        foreach (var t in _liveTurrets)
            if (t != null) NetworkServer.Destroy(t);
        _liveTurrets.Clear();
    }

    // ── Phase 1 ────────────────────────────────────────────────────────────────

    [Server]
    IEnumerator BarrierWallLoop()
    {
        while (currentPhase == WardenPhase.SiegeProtocol && _health.IsAlive)
        {
            yield return new WaitForSeconds(barrierInterval);
            if (currentPhase != WardenPhase.SiegeProtocol) yield break;
            RpcSpawnBarrierWall();
        }
    }

    [Server]
    IEnumerator MortarStrikeLoop()
    {
        yield return new WaitForSeconds(5f);
        while (currentPhase == WardenPhase.SiegeProtocol && _health.IsAlive)
        {
            yield return new WaitForSeconds(mortarInterval);
            if (currentPhase != WardenPhase.SiegeProtocol) yield break;
            yield return StartCoroutine(MortarSequence());
        }
    }

    [Server]
    IEnumerator MortarSequence()
    {
        RpcAnnounce("[BOSS] Mortar Strike incoming — spread out!");
        var players = FindObjectsByType<PlayerIdentity>(FindObjectsSortMode.None);
        if (players.Length == 0) yield break;

        for (int i = 0; i < mortarCount; i++)
        {
            var target = players[Random.Range(0, players.Length)];
            Vector3 pos = target.transform.position;
            RpcShowWarningCircle(pos, mortarRadius, 1f);
            yield return new WaitForSeconds(1f);
            DealAoeDamage(pos, mortarRadius, mortarDamage);
            RpcExplosionVfx(pos);
        }
    }

    // ── Phase 2 ────────────────────────────────────────────────────────────────

    [Server]
    void SpawnSiegeTurrets()
    {
        if (siegeTurretPrefab == null)
        {
            Debug.LogError("[WARDEN] siegeTurretPrefab not assigned — Phase 2 turrets will not spawn. Assign in Inspector.");
            return;
        }
        SpawnTurretAt(transform.position + transform.right * 8f);
        SpawnTurretAt(transform.position - transform.right * 8f);
        RpcAnnounce("[BOSS] Siege Turrets deployed — destroy both at the same time!");
    }

    [Server]
    void SpawnTurretAt(Vector3 pos)
    {
        var go = Instantiate(siegeTurretPrefab, pos, Quaternion.identity);
        var turret = go.GetComponent<SiegeTurretBehaviour>();
        if (turret != null) turret.warden = this;
        NetworkServer.Spawn(go);
        _liveTurrets.Add(go);
    }

    [Server]
    public void OnTurretDestroyed()
    {
        if (currentPhase != WardenPhase.ShieldMatrix) return;

        _turretsAlive = Mathf.Max(0, _turretsAlive - 1);
        if (_turretsAlive == 0)
        {
            // Both down inside the repair window — drop immunity, cancel any pending repair
            if (_repairRoutine != null) { StopCoroutine(_repairRoutine); _repairRoutine = null; }
            _health.ClearDamageReduction();
            RpcAnnounce("[BOSS] Both turrets destroyed — the Warden is vulnerable!");
            return;
        }
        if (_repairRoutine == null)
            _repairRoutine = StartCoroutine(TurretRepairDelay());
    }

    [Server]
    IEnumerator TurretRepairDelay()
    {
        yield return new WaitForSeconds(turretRepairDelay);
        _repairRoutine = null;
        if (currentPhase != WardenPhase.ShieldMatrix) yield break;

        DespawnTurrets();               // remove the survivor, spawn a fresh pair
        _turretsAlive = 2;
        _health.SetDamageReduction(1f); // re-immune in case it lapsed
        SpawnSiegeTurrets();
        RpcAnnounce("[BOSS] Turret repaired — destroy BOTH simultaneously!");
    }

    [Server]
    IEnumerator MagnetPullLoop()
    {
        yield return new WaitForSeconds(10f);
        while (currentPhase == WardenPhase.ShieldMatrix && _health.IsAlive)
        {
            yield return new WaitForSeconds(magnetInterval);
            if (currentPhase != WardenPhase.ShieldMatrix) yield break;
            yield return StartCoroutine(MagnetPullSequence());
        }
    }

    [Server]
    IEnumerator MagnetPullSequence()
    {
        RpcAnnounce("[BOSS] Magnetic Pull — brace yourself!");
        // Pull happens client-side via CharacterController — server sends position
        RpcPullPlayersTo(transform.position);
        yield return new WaitForSeconds(1f);
        RpcShowWarningCircle(transform.position, magnetRadius, 1f);
        yield return new WaitForSeconds(1f);
        DealAoeDamage(transform.position, magnetRadius, magnetStompDamage);
        RpcExplosionVfx(transform.position);
    }

    // ── Phase 3 ────────────────────────────────────────────────────────────────

    [Server]
    IEnumerator GroundSlamLoop()
    {
        while (currentPhase == WardenPhase.Rampage && _health.IsAlive)
        {
            yield return new WaitForSeconds(slamInterval);
            if (currentPhase != WardenPhase.Rampage) yield break;
            RpcShowWarningCircle(transform.position, slamRadius, 0.4f);
            yield return new WaitForSeconds(0.45f);
            DealAoeDamage(transform.position, slamRadius, slamDamage);
            RpcGroundSlamVfx(transform.position);
        }
    }

    [Server]
    IEnumerator LockdownSequence()
    {
        RpcAnnounce("[BOSS] LOCKDOWN — The Iron Warden prepares a devastating blow!");

        // Root all players via StatusEffectManager.Bound
        var players = FindObjectsByType<PlayerIdentity>(FindObjectsSortMode.None);
        foreach (var p in players)
            p.GetComponent<StatusEffectManager>()?.AddEffect(
                new StatusEffect(StatusEffectType.Bound, lockdownDuration, 0f));

        RpcLockdownVfx();
        yield return new WaitForSeconds(lockdownDuration);

        RpcShowWarningCircle(transform.position, devastationRadius, 1.5f);
        yield return new WaitForSeconds(1.5f);
        DealAoeDamage(transform.position, devastationRadius, devastationDamage);
        RpcExplosionVfx(transform.position);
        RpcAnnounce("[BOSS] DEVASTATION!");
    }

    // ── Death ──────────────────────────────────────────────────────────────────

    [Server]
    IEnumerator DeathSequence()
    {
        RpcPlayDeathVfx();
        yield return new WaitForSeconds(3f);
        RollDrops();
        ArenaSessionController.Instance?.EndSession();
        yield return new WaitForSeconds(2f);
        NetworkServer.Destroy(gameObject);
    }

    [Server]
    void RollDrops()
    {
        Vector3 scatter = transform.position + Vector3.forward * 2f;
        foreach (var id in guaranteedDropItemIds)
            TrySpawnDrop(id, scatter);
        foreach (var id in rareDropItemIds)
            if (Random.value <= rareDropChance)
                TrySpawnDrop(id, scatter + Random.insideUnitSphere * 1.5f);
    }

    [Server]
    void TrySpawnDrop(string itemId, Vector3 pos)
    {
        if (worldItemPrefab == null)
        {
            Debug.LogWarning($"[WARDEN] worldItemPrefab not assigned — {itemId} lost");
            return;
        }
        pos.y = transform.position.y + 0.5f;
        var wi   = Instantiate(worldItemPrefab, pos, Quaternion.identity);
        var comp = wi.GetComponent<WorldItem>();
        if (comp != null) { comp.itemId = itemId; comp.quantity = 1; }
        NetworkServer.Spawn(wi);
        Debug.Log($"[WARDEN] Dropped: {itemId}");
    }

    // ── Shared helpers ─────────────────────────────────────────────────────────

    [Server]
    void DealAoeDamage(Vector3 centre, float radius, int damage)
    {
        var hits = Physics.OverlapSphere(centre, radius);
        foreach (var hit in hits)
        {
            if (!hit.CompareTag("Player")) continue;
            hit.GetComponent<Health>()?.TakeDamage(damage, gameObject);
        }
    }

    // ── SyncVar hook ───────────────────────────────────────────────────────────
    void OnPhaseChanged(WardenPhase _, WardenPhase newPhase)
    {
#if !UNITY_SERVER
        FindAnyObjectByType<WorldBossHealthBar>()?.OnPhaseChanged(
            newPhase == WardenPhase.Dead
                ? WorldBossController.BossPhase.Dead
                : WorldBossController.BossPhase.Phase1);
#endif
    }

    // ── ClientRpc ──────────────────────────────────────────────────────────────

    [ClientRpc]
    void RpcAnnounce(string msg)
    {
#if !UNITY_SERVER
        RodChatManager.Instance?.AddSystemMessage(msg);
#endif
    }

    [ClientRpc]
    void RpcShowWarningCircle(Vector3 pos, float radius, float duration)
    {
#if !UNITY_SERVER
        if (warningCirclePrefab == null) return;
        var go = Instantiate(warningCirclePrefab, pos, Quaternion.identity);
        go.transform.localScale = Vector3.one * radius * 2f;
        Destroy(go, duration + 0.1f);
#endif
    }

    // Pulls local player toward bossPos — runs client-side so CharacterController is available
    [ClientRpc]
    void RpcPullPlayersTo(Vector3 bossPos)
    {
#if !UNITY_SERVER
        var localPlayer = NetworkClient.localPlayer;
        if (localPlayer == null) return;
        var cc = localPlayer.GetComponent<CharacterController>();
        if (cc == null) return;
        Vector3 dir = (bossPos - localPlayer.transform.position).normalized;
        cc.Move(dir * 6f);
#endif
    }

    [ClientRpc] void RpcSpawnBarrierWall()      { /* barrier wall VFX/collider — editor step */ }
    [ClientRpc] void RpcExplosionVfx(Vector3 p) { /* explosion particle at p */ }
    [ClientRpc] void RpcGroundSlamVfx(Vector3 p){ /* shockwave ring at p */ }
    [ClientRpc] void RpcLockdownVfx()           { /* red screen flash + chain VFX */ }
    [ClientRpc] void RpcPlayDeathVfx()          { /* death sequence particle */ }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.4f, 0f, 0.25f);
        Gizmos.DrawWireSphere(transform.position, slamRadius);
        Gizmos.color = new Color(1f, 0f, 0f, 0.15f);
        Gizmos.DrawWireSphere(transform.position, devastationRadius);
    }
#endif
}
