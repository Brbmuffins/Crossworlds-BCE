using System.Collections;
using UnityEngine;
using Mirror;

/// <summary>
/// EnemyHeavyAttack — Companion to EnemyController.
///
/// Adds a random "hard hitter" special ability that fires on a cooldown
/// while the enemy is in combat, giving mobs spell-book-style variety.
///
/// Abilities (inspired by the player spellbook):
///   GroundSlam    — AOE circle around self, medium damage + Stagger
///   VoidBurst     — AOE circle at target pos, damage + Slow + Cursed DoT
///   ChainLightning— jumps to up to 3 nearby players, damage + Weakened
///   GroundSpikes  — cone toward target, high damage + Slow
///   HexBlast      — single target, massive damage + Weakened + Cursed
///
/// Setup:
///   1. Add this component alongside EnemyController on any enemy prefab.
///   2. Optionally restrict `availableTypes` to a subset in the Inspector.
///   3. Leave availableTypes empty → enemy uses all 5 types randomly.
///
/// Mirror discipline: all state changes and damage are [Server] only.
/// Client-side VFX/feedback behind #if UNITY_EDITOR || !UNITY_SERVER.
/// </summary>
public class EnemyHeavyAttack : NetworkBehaviour
{
    public enum HeavyAbilityType
    {
        GroundSlam,      // AOE self-centered, stagger
        VoidBurst,       // AOE at target position, slow + DoT
        ChainLightning,  // chain to 3 players, weakened
        GroundSpikes,    // cone toward target, slow
        HexBlast         // single target, heavy damage, weakened + DoT
    }

    // ── Config ────────────────────────────────────────────────────────────────────

    [Header("Cooldown")]
    [Tooltip("Minimum seconds between heavy attacks.")]
    public float minCooldown = 10f;
    [Tooltip("Maximum seconds between heavy attacks.")]
    public float maxCooldown = 18f;

    [Header("Damage")]
    [Tooltip("Heavy attack damage = EnemyController.damage × this multiplier.")]
    public float damageMultiplier = 2.5f;

    [Header("Available Abilities")]
    [Tooltip("Which heavy types this enemy can roll. Leave empty to allow all 5.")]
    public HeavyAbilityType[] availableTypes;

    // ── State ─────────────────────────────────────────────────────────────────────

    EnemyController _enemy;
    Health          _health;

    static readonly HeavyAbilityType[] _allTypes =
        (HeavyAbilityType[])System.Enum.GetValues(typeof(HeavyAbilityType));

    // ── Lifecycle ─────────────────────────────────────────────────────────────────

    public override void OnStartServer()
    {
        base.OnStartServer();
        _enemy  = GetComponent<EnemyController>();
        _health = GetComponent<Health>();

        if (_enemy == null || _health == null)
        {
            Debug.LogWarning($"[HeavyAttack] {name}: requires EnemyController + Health.");
            return;
        }

        StartCoroutine(HeavyAttackLoop());
    }

    // ── Server loop ───────────────────────────────────────────────────────────────

    [Server]
    IEnumerator HeavyAttackLoop()
    {
        // Stagger start slightly so multiple enemies don't all fire at once
        yield return new WaitForSeconds(Random.Range(2f, 8f));

        while (_health.IsAlive)
        {
            yield return new WaitForSeconds(Random.Range(minCooldown, maxCooldown));

            if (!_health.IsAlive) yield break;

            // Only fire during active combat states
            if (_enemy.state != EnemyController.EnemyState.Attack &&
                _enemy.state != EnemyController.EnemyState.Chase)
                continue;

            HeavyAbilityType chosen = PickAbility();
            yield return StartCoroutine(ExecuteAbility(chosen));
        }
    }

    [Server]
    HeavyAbilityType PickAbility()
    {
        var pool = (availableTypes != null && availableTypes.Length > 0)
            ? availableTypes
            : _allTypes;
        return pool[Random.Range(0, pool.Length)];
    }

    [Server]
    IEnumerator ExecuteAbility(HeavyAbilityType type)
    {
        // Telegraph (wind-up visible to clients before damage lands)
        RpcTelegraph(type, transform.position);
        yield return new WaitForSeconds(0.65f);

        if (!_health.IsAlive) yield break;

        float dmg = _enemy.damage * damageMultiplier;

        switch (type)
        {
            case HeavyAbilityType.GroundSlam:
                // AOE ring at own feet — stagger anything in 4 m
                HitPlayersInRadius(transform.position, 4f, dmg,
                    StatusEffectType.Stagger, 1.5f, 0f);
                break;

            case HeavyAbilityType.VoidBurst:
                // AOE at target's last position — slow + cursed DoT ticking for 5 s
                Vector3 burstPos = GetTargetOrSelf();
                HitPlayersInRadius(burstPos, 5f, dmg * 0.75f,
                    StatusEffectType.Slow, 4f, 0.35f);
                HitPlayersInRadius(burstPos, 5f, 0f,
                    StatusEffectType.Cursed, 5f, dmg * 0.08f); // 8 % per tick DoT
                break;

            case HeavyAbilityType.ChainLightning:
                // Jumps across up to 3 players in a 14 m radius, weakening each
                HitChain(14f, maxJumps: 3, dmgPerJump: dmg * 0.85f,
                    falloff: 0.25f,
                    statusType: StatusEffectType.Weakened, statusDur: 4f, statusVal: 0f);
                break;

            case HeavyAbilityType.GroundSpikes:
                // Cone toward target — high damage + slow
                HitPlayersCone(transform.position, GetTargetOrSelf(), 5f, 50f, dmg * 1.1f,
                    StatusEffectType.Slow, 3f, 0.4f);
                break;

            case HeavyAbilityType.HexBlast:
                // Single target — heavy damage + weakened + cursed
                HitNearestPlayer(dmg * 1.8f,
                    StatusEffectType.Weakened, 5f, 0f,
                    StatusEffectType.Cursed,   5f, dmg * 0.12f);
                break;
        }

        RpcAbilityFired(type, transform.position);
    }

    // ── Damage helpers (all [Server]) ────────────────────────────────────────────

    [Server]
    void HitPlayersInRadius(Vector3 center, float radius, float dmg,
        StatusEffectType status, float statusDur, float statusVal)
    {
        foreach (var col in Physics.OverlapSphere(center, radius))
        {
            if (!col.CompareTag("Player")) continue;
            ApplyHit(col.gameObject, dmg, status, statusDur, statusVal);
        }
    }

    [Server]
    void HitChain(float searchRadius, int maxJumps, float dmgPerJump,
        float falloff, StatusEffectType statusType, float statusDur, float statusVal)
    {
        var hits  = Physics.OverlapSphere(transform.position, searchRadius);
        int jumps = 0;

        foreach (var col in hits)
        {
            if (jumps >= maxJumps) break;
            if (!col.CompareTag("Player")) continue;
            float scaled = dmgPerJump * Mathf.Pow(1f - falloff, jumps);
            ApplyHit(col.gameObject, scaled, statusType, statusDur, statusVal);
            jumps++;
        }
    }

    [Server]
    void HitPlayersCone(Vector3 origin, Vector3 targetPos, float range, float angleDeg,
        float dmg, StatusEffectType status, float statusDur, float statusVal)
    {
        Vector3 forward = (targetPos - origin).normalized;
        forward.y = 0f;
        if (forward == Vector3.zero) forward = transform.forward;

        foreach (var col in Physics.OverlapSphere(origin, range))
        {
            if (!col.CompareTag("Player")) continue;
            Vector3 dir = (col.transform.position - origin).normalized;
            dir.y = 0f;
            if (Vector3.Angle(forward, dir) > angleDeg * 0.5f) continue;
            ApplyHit(col.gameObject, dmg, status, statusDur, statusVal);
        }
    }

    [Server]
    void HitNearestPlayer(float dmg,
        StatusEffectType s1, float d1, float v1,
        StatusEffectType s2, float d2, float v2)
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        float best = Mathf.Infinity;
        GameObject target = null;

        foreach (var p in players)
        {
            if (p == null) continue;
            var h = p.GetComponent<Health>();
            if (h == null || !h.IsAlive) continue;
            float sqr = (p.transform.position - transform.position).sqrMagnitude;
            if (sqr < best) { best = sqr; target = p; }
        }

        if (target == null) return;
        ApplyHit(target, dmg, s1, d1, v1);
        var sem = target.GetComponent<StatusEffectManager>();
        sem?.AddEffect(new StatusEffect(s2, d2, v2, gameObject));
    }

    [Server]
    void ApplyHit(GameObject target, float dmg, StatusEffectType status,
        float statusDur, float statusVal)
    {
        if (target == null) return;
        var h = target.GetComponent<Health>();
        if (h == null || !h.IsAlive) return;

        if (dmg > 0f) h.TakeDamage(dmg, gameObject);

        var sem = target.GetComponent<StatusEffectManager>();
        if (sem != null && statusDur > 0f)
            sem.AddEffect(new StatusEffect(status, statusDur, statusVal, gameObject));
    }

    Vector3 GetTargetOrSelf()
    {
        // Try to read the enemy's current target via reflection — it's private,
        // so we fall back to the nearest player if none found.
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        float best = Mathf.Infinity;
        Vector3 pos = transform.position;

        foreach (var p in players)
        {
            if (p == null) continue;
            var h = p.GetComponent<Health>();
            if (h == null || !h.IsAlive) continue;
            float sqr = (p.transform.position - transform.position).sqrMagnitude;
            if (sqr < best) { best = sqr; pos = p.transform.position; }
        }

        return pos;
    }

    // ── Client-side feedback ──────────────────────────────────────────────────────

    [ClientRpc]
    void RpcTelegraph(HeavyAbilityType type, Vector3 pos)
    {
#if UNITY_EDITOR || !UNITY_SERVER
        // Visual telegraph: warning text above the enemy so players can dodge
        string label = type switch
        {
            HeavyAbilityType.GroundSlam      => "⚡SLAM",
            HeavyAbilityType.VoidBurst       => "☠VOID",
            HeavyAbilityType.ChainLightning  => "⚡CHAIN",
            HeavyAbilityType.GroundSpikes    => "▲SPIKES",
            HeavyAbilityType.HexBlast        => "☠HEX",
            _                                => "!!",
        };

        FloatingDamageText.Spawn(pos + Vector3.up * 2.2f, 0,
            FloatingDamageText.DamageType.Normal, label);

        // Light screen shake as a warning pulse
        ScreenShake.AddTrauma(0.08f);
#endif
    }

    [ClientRpc]
    void RpcAbilityFired(HeavyAbilityType type, Vector3 pos)
    {
#if UNITY_EDITOR || !UNITY_SERVER
        // Impact shake — heavier than a normal melee swing
        bool isBig = type == HeavyAbilityType.GroundSlam || type == HeavyAbilityType.HexBlast;
        ScreenShake.AddTrauma(isBig ? 0.35f : 0.22f);
        CombatAudio.Instance?.PlayMeleeHit();
#endif
    }
}
