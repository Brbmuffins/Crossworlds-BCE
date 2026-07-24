using System.Collections;
using Mirror;
using UnityEngine;

/// <summary>
/// WispCombat — server-authoritative combat layer for WispMob.
///
/// Wisps float rather than path, so no NavMeshAgent is needed.
/// WispMob drives ambient wander; WispCombat takes over movement when a
/// target is aggroed and handles damage + death.
///
/// Network: requires NetworkIdentity + NetworkTransformReliable on the prefab.
/// WispMob is disabled on pure clients so local SmoothDamp doesn't fight the
/// synced position from NetworkTransformReliable.
///
/// Attacks:
///   Contact  — trigger-sphere enter deals contactDamage immediately.
///   Pulse    — periodic AoE within pulseRadius; fires when in range.
///
/// Run BCE/Setup/4w to patch Wisp_Mob.prefab with all required components.
/// </summary>
[RequireComponent(typeof(Health))]
[RequireComponent(typeof(WispMob))]
public class WispCombat : NetworkBehaviour
{
    [Header("Detection")]
    public float aggroRadius  = 8f;
    public float leashRadius  = 22f;

    [Header("Combat")]
    public float contactDamage = 8f;
    public float pulseDamage   = 12f;
    public float pulseRadius   = 3.5f;
    public float pulseCooldown = 4f;

    [Header("Chase")]
    public float chaseHeight   = 2.2f;  // hover height above target while aggroed
    public float chaseSmoothTime = 0.4f;
    public float chaseMaxSpeed   = 4.5f;

    // ── Private ─────────────────────────────────────────────────────────────────

    private Health    _health;
    private WispMob   _wispMob;
    private Transform _target;
    private Vector3   _spawnPos;
    private Vector3   _chaseVelocity;
    private float     _pulseTimer;

    // ─────────────────────────────────────────────────────────────────────────────
    // Lifecycle
    // ─────────────────────────────────────────────────────────────────────────────

    void Awake()
    {
        _health  = GetComponent<Health>();
        _wispMob = GetComponent<WispMob>();
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        _spawnPos = transform.position;
        _pulseTimer = EnemyCrowdUtility.FirstAttackDelay(this, pulseCooldown);
        _health.onDeath.AddListener(OnDeath);
        _health.onDamagedBy.AddListener(OnDamagedBy);
        StartCoroutine(AggroLoop());
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        // Server drives position via NetworkTransformReliable; WispMob's SmoothDamp
        // would fight the synced position on clients. Disable it here; the server
        // will re-enable/disable it as needed for ambient vs chase.
        if (!isServer && _wispMob != null)
            _wispMob.enabled = false;
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Server movement: chase override over WispMob ambient wander
    // ─────────────────────────────────────────────────────────────────────────────

    void Update()
    {
        if (!isServer) return;
        if (!_health.IsAlive) return;

        _pulseTimer -= Time.deltaTime;

        if (_target == null)
        {
            // No target — let WispMob handle ambient wander
            if (_wispMob != null && !_wispMob.enabled)
                _wispMob.enabled = true;
            return;
        }

        // Suspend WispMob wander while chasing (both run SmoothDamp — one must win)
        if (_wispMob != null && _wispMob.enabled)
            _wispMob.enabled = false;

        // Leash
        if (Vector3.Distance(transform.position, _spawnPos) > leashRadius)
        {
            _target = null;
            return;
        }

        // Float towards a personal slot near the target instead of stacking directly above it.
        Vector3 slot = EnemyCrowdUtility.ChaseSlot(transform, _target, 1.25f, 0.65f);
        Vector3 desired = new Vector3(slot.x, _target.position.y + chaseHeight, slot.z);
        transform.position = Vector3.SmoothDamp(
            transform.position, desired, ref _chaseVelocity,
            chaseSmoothTime, chaseMaxSpeed);

        // Face target (horizontal only)
        Vector3 dir = _target.position - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.01f)
            transform.forward = Vector3.Slerp(transform.forward, dir.normalized, Time.deltaTime * 5f);

        // Pulse when in range
        if (_pulseTimer <= 0f &&
            Vector3.Distance(transform.position, _target.position) <= pulseRadius + 0.5f)
        {
            _pulseTimer = pulseCooldown;
            Pulse();
        }
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Aggro scanning
    // ─────────────────────────────────────────────────────────────────────────────

    [Server]
    IEnumerator AggroLoop()
    {
        var tick = new WaitForSeconds(0.3f);
        while (_health.IsAlive)
        {
            yield return tick;

            // Validate existing target
            if (_target != null)
            {
                var h = _target.GetComponent<Health>();
                if (h == null || !h.IsAlive ||
                    Vector3.Distance(transform.position, _spawnPos) > leashRadius)
                    _target = null;
                continue;
            }

            // Scan for nearest player
            var hits    = ZonePhysics.OverlapSphere(gameObject, transform.position, aggroRadius);
            float best  = float.MaxValue;
            Transform found = null;

            foreach (var col in hits)
            {
                if (!col.CompareTag("Player")) continue;
                var h = col.GetComponent<Health>();
                if (h == null || !h.IsAlive) continue;
                float d = Vector3.SqrMagnitude(transform.position - col.transform.position);
                if (d < best) { best = d; found = col.transform; }
            }

            if (found != null) _target = found;
        }
    }

    [Server]
    void OnDamagedBy(GameObject source)
    {
        if (_target != null || source == null) return;
        if (source.CompareTag("Player"))
            _target = source.transform;
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Contact damage (trigger sphere already on Wisp_Mob prefab)
    // ─────────────────────────────────────────────────────────────────────────────

    void OnTriggerEnter(Collider other)
    {
        if (!isServer) return;
        if (!other.CompareTag("Player")) return;

        other.GetComponent<Health>()?.TakeDamage(contactDamage, gameObject);
        RpcContactFlash();
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Pulse: small AoE burst every pulseCooldown seconds
    // ─────────────────────────────────────────────────────────────────────────────

    [Server]
    void Pulse()
    {
        var hits = ZonePhysics.OverlapSphere(gameObject, transform.position, pulseRadius);
        foreach (var col in hits)
        {
            if (!col.CompareTag("Player")) continue;
            col.GetComponent<Health>()?.TakeDamage(pulseDamage, gameObject);
        }
        RpcPulseVFX();
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Death
    // ─────────────────────────────────────────────────────────────────────────────

    [Server]
    void OnDeath()
    {
        _target = null;
        StopAllCoroutines();
        StartCoroutine(DeathSequence());
    }

    [Server]
    IEnumerator DeathSequence()
    {
        RpcDeathBurst();
        yield return new WaitForSeconds(1f);
        NetworkServer.Destroy(gameObject);
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // RPCs
    // ─────────────────────────────────────────────────────────────────────────────

    [ClientRpc]
    void RpcContactFlash()
    {
#if UNITY_EDITOR || !UNITY_SERVER
        ScreenShake.AddTrauma(0.06f);
        CombatAudio.Instance?.PlayMeleeHit();
#endif
    }

    [ClientRpc]
    void RpcPulseVFX()
    {
#if UNITY_EDITOR || !UNITY_SERVER
        FloatingDamageText.Spawn(transform.position + Vector3.up,
            (int)pulseDamage, FloatingDamageText.DamageType.Normal);
        ScreenShake.AddTrauma(0.09f);
#endif
    }

    [ClientRpc]
    void RpcDeathBurst()
    {
#if UNITY_EDITOR || !UNITY_SERVER
        ScreenShake.AddTrauma(0.12f);
        CombatAudio.Instance?.PlayDeath();
        FloatingDamageText.Spawn(transform.position + Vector3.up * 1.5f,
            0, FloatingDamageText.DamageType.Normal, "×");
#endif
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Gizmos
    // ─────────────────────────────────────────────────────────────────────────────

    void OnDrawGizmosSelected()
    {
        Vector3 origin = Application.isPlaying ? _spawnPos : transform.position;

        Gizmos.color = new Color(0.48f, 0.9f, 1f, 0.2f);
        Gizmos.DrawWireSphere(origin, aggroRadius);

        Gizmos.color = new Color(1f, 0.3f, 0.3f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, pulseRadius);

        Gizmos.color = new Color(0.2f, 0.6f, 1f, 0.1f);
        Gizmos.DrawWireSphere(origin, leashRadius);
    }
}
