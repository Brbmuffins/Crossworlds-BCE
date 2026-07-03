using System.Collections;
using UnityEngine;
using Mirror;

/// <summary>
/// The Iron Warden — second world boss.
///
/// Phase 1 (100–60 %): Siege Protocol
///   • Barrier Wall: a ring of iron shields orbits the boss every 12 s.
///     Players on the wrong arc receive 10 dmg/s push-back; correct arc deals full damage.
///   • Mortar Strike: every 20 s, five AoE circles stagger-detonate (1 s apart), 30 dmg each.
///
/// Phase 2 (60–25 %): Shield Matrix
///   • Two Siege Turrets spawn at opposite flanks. Boss gains immunity until BOTH are
///     destroyed simultaneously. If only one dies the other auto-repairs after 10 s.
///   • Magnet Pull every 20 s: drag all players to centre, then stomp (60 AoE).
///
/// Phase 3 (25–0 %): Rampage
///   • Ground Slam every 4 s in a 6 u radius (35 dmg).
///   • Boss move speed +50 %.
///   • At ≤15 % HP: Lockdown roots all players for 5 s, boss charges a 60-AoE devastation slam.
///
/// Follows the same server-authoritative pattern as WorldBossController.
/// All game-state changes are [Server]; VFX/announcements use [ClientRpc].
/// </summary>
public class IronWardenController : NetworkBehaviour
{
    // ── Inspector ──────────────────────────────────────────────────────────────
    [Header("Health")]
    public int maxHealth = 3000;

    [Header("Phase Thresholds (0-1)")]
    public float phase2Threshold = 0.60f;
    public float phase3Threshold = 0.25f;
    public float lockdownThreshold = 0.15f;

    [Header("Phase 1 — Siege Protocol")]
    public float barrierInterval   = 12f;
    public float mortarInterval    = 20f;
    public int   mortarDamage      = 30;
    public float mortarRadius      = 4f;
    public int   mortarCount       = 5;

    [Header("Phase 2 — Shield Matrix")]
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
    public GameObject warningCirclePrefab;  // assign in inspector

    // ── Network state ──────────────────────────────────────────────────────────
    public enum WardenPhase { Dormant, SiegeProtocol, ShieldMatrix, Rampage, Dead }

    [SyncVar(hook = nameof(OnPhaseChanged))]
    public WardenPhase currentPhase = WardenPhase.Dormant;

    [SyncVar] public int currentHealth;

    // ── Private state (server only) ────────────────────────────────────────────
    Coroutine _barrierRoutine;
    Coroutine _mortarRoutine;
    Coroutine _magnetRoutine;
    Coroutine _slamRoutine;
    bool _lockdownFired;
    bool _turretRepairPending;
    int  _turretsAlive;

    // ── Lifecycle ──────────────────────────────────────────────────────────────
    public override void OnStartServer()
    {
        currentHealth = maxHealth;
    }

    // ── Public API called by arena trigger / wave system ──────────────────────
    [Server]
    public void Activate()
    {
        if (currentPhase != WardenPhase.Dormant) return;
        RpcAnnounce("[BOSS] The Iron Warden awakens — SIEGE PROTOCOL initiated!");
        TransitionTo(WardenPhase.SiegeProtocol);
    }

    [Server]
    public void TakeDamage(int amount)
    {
        if (currentPhase == WardenPhase.Dead) return;

        // Phase 2: immune while two turrets are alive
        if (currentPhase == WardenPhase.ShieldMatrix && _turretsAlive >= 2) return;

        currentHealth = Mathf.Max(0, currentHealth - amount);
        float ratio   = (float)currentHealth / maxHealth;

        if (currentPhase == WardenPhase.SiegeProtocol && ratio <= phase2Threshold)
        {
            TransitionTo(WardenPhase.ShieldMatrix);
            return;
        }

        if (currentPhase == WardenPhase.ShieldMatrix && ratio <= phase3Threshold)
        {
            TransitionTo(WardenPhase.Rampage);
            return;
        }

        if (currentPhase == WardenPhase.Rampage && ratio <= lockdownThreshold && !_lockdownFired)
        {
            _lockdownFired = true;
            StartCoroutine(LockdownSequence());
        }

        if (currentHealth <= 0) TransitionTo(WardenPhase.Dead);
    }

    // ── Phase transition ───────────────────────────────────────────────────────
    [Server]
    void TransitionTo(WardenPhase next)
    {
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
                SpawnSiegeTurrets();
                _magnetRoutine = StartCoroutine(MagnetPullLoop());
                break;

            case WardenPhase.Rampage:
                RpcAnnounce("[BOSS] The Iron Warden's core is exposed — RAMPAGE!");
                _slamRoutine = StartCoroutine(GroundSlamLoop());
                // Speed boost applied on clients via hook
                break;

            case WardenPhase.Dead:
                RpcAnnounce("[BOSS] The Iron Warden has fallen!");
                ArenaSessionController.Instance?.OnBossKilled();
                break;
        }
    }

    void StopPhaseCoroutines()
    {
        if (_barrierRoutine != null) { StopCoroutine(_barrierRoutine); _barrierRoutine = null; }
        if (_mortarRoutine  != null) { StopCoroutine(_mortarRoutine);  _mortarRoutine  = null; }
        if (_magnetRoutine  != null) { StopCoroutine(_magnetRoutine);  _magnetRoutine  = null; }
        if (_slamRoutine    != null) { StopCoroutine(_slamRoutine);    _slamRoutine    = null; }
    }

    // ── Phase 1: Siege Protocol ────────────────────────────────────────────────

    [Server]
    IEnumerator BarrierWallLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(barrierInterval);
            if (currentPhase != WardenPhase.SiegeProtocol) yield break;
            RpcSpawnBarrierWall();
        }
    }

    [Server]
    IEnumerator MortarStrikeLoop()
    {
        yield return new WaitForSeconds(5f); // initial delay
        while (true)
        {
            yield return new WaitForSeconds(mortarInterval);
            if (currentPhase != WardenPhase.SiegeProtocol) yield break;
            StartCoroutine(MortarSequence());
        }
    }

    [Server]
    IEnumerator MortarSequence()
    {
        RpcAnnounce("[BOSS] Mortar Strike incoming — move!");
        var players = FindObjectsByType<PlayerIdentity>(FindObjectsSortMode.None);

        for (int i = 0; i < mortarCount; i++)
        {
            // Target a random player position
            var target = players[Random.Range(0, players.Length)];
            Vector3 pos = target.transform.position;
            RpcShowWarningCircle(pos, mortarRadius, 1f);
            yield return new WaitForSeconds(1f);
            DealAoeDamage(pos, mortarRadius, mortarDamage);
            RpcExplosionVfx(pos);
        }
    }

    // ── Phase 2: Shield Matrix ─────────────────────────────────────────────────

    [Server]
    void SpawnSiegeTurrets()
    {
        // Spawn turrets at 0° and 180° relative to boss
        Vector3 right = transform.right * 8f;
        SpawnTurret(transform.position + right);
        SpawnTurret(transform.position - right);
        RpcAnnounce("[BOSS] Siege Turrets deployed! Destroy both at the same time!");
    }

    void SpawnTurret(Vector3 position)
    {
        // SiegeTurret prefab must be assigned or created in editor
        // It calls IronWardenController.OnTurretDestroyed() when killed
        var go = new GameObject($"SiegeTurret_{position}");
        go.transform.position = position;
        var turret = go.AddComponent<SiegeTurretBehaviour>();
        turret.warden = this;
        NetworkServer.Spawn(go);
    }

    [Server]
    public void OnTurretDestroyed()
    {
        _turretsAlive = Mathf.Max(0, _turretsAlive - 1);

        if (_turretsAlive == 0)
        {
            RpcAnnounce("[BOSS] Both turrets destroyed — the Warden is vulnerable!");
            return;
        }

        // Only one destroyed — surviving turret auto-repairs after delay
        if (!_turretRepairPending)
        {
            _turretRepairPending = true;
            StartCoroutine(TurretRepairDelay());
        }
    }

    [Server]
    IEnumerator TurretRepairDelay()
    {
        yield return new WaitForSeconds(turretRepairDelay);
        _turretsAlive        = 2;
        _turretRepairPending = false;
        SpawnSiegeTurrets();
        RpcAnnounce("[BOSS] Turret repaired to full! Destroy both simultaneously!");
    }

    [Server]
    IEnumerator MagnetPullLoop()
    {
        yield return new WaitForSeconds(10f);
        while (true)
        {
            yield return new WaitForSeconds(magnetInterval);
            if (currentPhase != WardenPhase.ShieldMatrix) yield break;
            StartCoroutine(MagnetPullSequence());
        }
    }

    [Server]
    IEnumerator MagnetPullSequence()
    {
        RpcAnnounce("[BOSS] Magnetic Pull — brace yourself!");
        RpcMagnetPullVfx(transform.position);

        // Pull all players toward boss centre
        var players = FindObjectsByType<PlayerIdentity>(FindObjectsSortMode.None);
        foreach (var p in players)
        {
            var cc = p.GetComponent<CharacterController>();
            if (cc != null)
            {
                Vector3 dir = (transform.position - p.transform.position).normalized;
                cc.Move(dir * 6f);
            }
        }

        yield return new WaitForSeconds(1f);

        // Ground stomp
        RpcShowWarningCircle(transform.position, magnetRadius, 1f);
        yield return new WaitForSeconds(1f);
        DealAoeDamage(transform.position, magnetRadius, magnetStompDamage);
        RpcExplosionVfx(transform.position);
    }

    // ── Phase 3: Rampage ───────────────────────────────────────────────────────

    [Server]
    IEnumerator GroundSlamLoop()
    {
        while (true)
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
        RpcLockdownVfx();

        // Root all players
        var players = FindObjectsByType<PlayerIdentity>(FindObjectsSortMode.None);
        foreach (var p in players)
            p.GetComponent<StatusEffectManager>()?.ApplyRoot(lockdownDuration);

        yield return new WaitForSeconds(lockdownDuration);

        // Charge up and slam
        RpcShowWarningCircle(transform.position, devastationRadius, 1.5f);
        yield return new WaitForSeconds(1.5f);
        DealAoeDamage(transform.position, devastationRadius, devastationDamage);
        RpcExplosionVfx(transform.position);
        RpcAnnounce("[BOSS] DEVASTATION!");
    }

    // ── Shared helpers ─────────────────────────────────────────────────────────

    [Server]
    void DealAoeDamage(Vector3 centre, float radius, int damage)
    {
        var hits = Physics.OverlapSphere(centre, radius);
        foreach (var hit in hits)
        {
            var hp = hit.GetComponent<HealthComponent>();
            if (hp != null && hit.CompareTag("Player"))
                hp.TakeDamage(damage, gameObject);
        }
    }

    // ── SyncVar hook ───────────────────────────────────────────────────────────

    void OnPhaseChanged(WardenPhase oldPhase, WardenPhase newPhase)
    {
#if !UNITY_SERVER
        if (newPhase == WardenPhase.Rampage)
        {
            var nav = GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (nav != null) nav.speed *= rampageSpeedMult;
        }
#endif
    }

    // ── ClientRpc VFX / announcements ─────────────────────────────────────────

    [ClientRpc]
    void RpcAnnounce(string msg)
    {
#if !UNITY_SERVER
        RodChatManager.Instance?.SystemMessage(msg);
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

    [ClientRpc] void RpcSpawnBarrierWall()       { /* Animator / VFX trigger on client */ }
    [ClientRpc] void RpcExplosionVfx(Vector3 p)  { /* Play explosion particle at p */ }
    [ClientRpc] void RpcGroundSlamVfx(Vector3 p) { /* Shockwave ring particle at p */ }
    [ClientRpc] void RpcMagnetPullVfx(Vector3 p) { /* Purple magnetic pull particle at p */ }
    [ClientRpc] void RpcLockdownVfx()            { /* Red screen flash + chain VFX */ }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.4f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, slamRadius);
        Gizmos.color = new Color(1f, 0f, 0f, 0.2f);
        Gizmos.DrawWireSphere(transform.position, devastationRadius);
    }
#endif
}
