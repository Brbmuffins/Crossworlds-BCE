using System.Collections;
using System.Collections.Generic;
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

    [Header("Ranged Casting")]
    [Tooltip("Maximum distance from a ranged enemy to its current target before a cast can begin.")]
    [Min(0.1f)] public float castDistanceToTarget = 10f;

    [Header("Opening Cast")]
    [Tooltip("Starts an opening cast as soon as this ranged enemy acquires a target.")]
    public bool castImmediatelyOnAggro = true;
    [Tooltip("Selects the opening spell from the available spell pool.")]
    public bool openingCastRandom = true;
    [Tooltip("Opening spell used when random selection is disabled.")]
    public HeavyAbilityType openingCastType = HeavyAbilityType.HexBlast;
    [Tooltip("Additional server-authoritative delay before the opening cast begins.")]
    [Min(0f)] public float openingCastDelay = 0f;
    [Tooltip("Allows only one opening cast during each combat engagement.")]
    public bool openingCastOncePerAggro = true;
    [Tooltip("Requires an unobstructed path to the target before the opening cast begins.")]
    public bool openingCastRequiresLineOfSight = true;
    [Tooltip("Cancels the opening cast when its target dies or leaves the enemy leash before impact.")]
    public bool cancelOpeningCastIfTargetInvalid = true;

    [Header("Available Abilities")]
    [Tooltip("Which heavy types this enemy can roll. Leave empty to allow all 5.")]
    public HeavyAbilityType[] availableTypes;

    // ── State ─────────────────────────────────────────────────────────────────────

    EnemyController _enemy;
    Health          _health;
    Coroutine       _openingCastRoutine;
    bool            _openingCastUsedThisAggro;
    bool            _abilityInProgress;

    public bool IsOpeningCastInProgress => _openingCastRoutine != null;

    public bool ShouldHoldCastingPosition(Transform target)
    {
        if (target == null || _enemy == null || !_enemy.isRanged) return false;
        if (Vector3.Distance(transform.position, target.position) > castDistanceToTarget) return false;
        return !openingCastRequiresLineOfSight || HasLineOfSight(target);
    }

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

    [Server]
    public void OnAggroAcquired(Transform target)
    {
        if (!castImmediatelyOnAggro || target == null) return;
        if (_enemy == null) _enemy = GetComponent<EnemyController>();
        if (_health == null) _health = GetComponent<Health>();
        if (_enemy == null || _health == null || !_health.IsAlive || !_enemy.isRanged) return;
        if (openingCastOncePerAggro && _openingCastUsedThisAggro) return;
        if (_openingCastRoutine != null || _abilityInProgress) return;
        if (Vector3.Distance(transform.position, target.position) > castDistanceToTarget) return;
        if (openingCastRequiresLineOfSight && !HasLineOfSight(target)) return;

        _openingCastUsedThisAggro = true;
        _openingCastRoutine = StartCoroutine(OpeningCastRoutine(target));
    }

    [Server]
    public void OnCombatEnded()
    {
        if (_openingCastRoutine != null)
        {
            StopCoroutine(_openingCastRoutine);
            _openingCastRoutine = null;
        }
        _abilityInProgress = false;
        _openingCastUsedThisAggro = false;
    }

    [Server]
    IEnumerator OpeningCastRoutine(Transform target)
    {
        _abilityInProgress = true;
        float remainingDelay = Mathf.Max(0f, openingCastDelay);
        while (remainingDelay > 0f)
        {
            if (cancelOpeningCastIfTargetInvalid && !IsOpeningTargetValid(target))
            {
                FinishOpeningCast();
                yield break;
            }
            float step = Mathf.Min(0.1f, remainingDelay);
            yield return new WaitForSeconds(step);
            remainingDelay -= step;
        }

        if ((cancelOpeningCastIfTargetInvalid && !IsOpeningTargetValid(target)) ||
            (openingCastRequiresLineOfSight && !HasLineOfSight(target)))
        {
            FinishOpeningCast();
            yield break;
        }

        HeavyAbilityType chosen = openingCastRandom ? PickAbility() : openingCastType;
        yield return StartCoroutine(ExecuteAbility(chosen, target, cancelOpeningCastIfTargetInvalid));
        FinishOpeningCast();
    }

    void FinishOpeningCast()
    {
        _abilityInProgress = false;
        _openingCastRoutine = null;
    }

    // ── Server loop ───────────────────────────────────────────────────────────────

    [Server]
    IEnumerator HeavyAttackLoop()
    {
        // Immediate-opening ranged casters synchronize their normal loop to the
        // opening cast. Other enemies retain the staggered startup.
        if (!_enemy.isRanged || !castImmediatelyOnAggro)
            yield return new WaitForSeconds(Random.Range(2f, 8f));

        while (_health.IsAlive)
        {
            // Only fire during active combat states
            if (_enemy.state != EnemyController.EnemyState.Attack &&
                _enemy.state != EnemyController.EnemyState.Chase)
            {
                yield return new WaitForSeconds(0.2f);
                continue;
            }
            if (_abilityInProgress)
            {
                yield return null;
                continue;
            }

            if (_enemy.isRanged)
            {
                Transform target = _enemy.CurrentTarget;
                if (target == null ||
                    Vector3.Distance(transform.position, target.position) > castDistanceToTarget ||
                    (openingCastRequiresLineOfSight && !HasLineOfSight(target)))
                {
                    yield return new WaitForSeconds(0.1f);
                    continue;
                }
            }

            float cooldown = Random.Range(
                Mathf.Max(0f, minCooldown),
                Mathf.Max(Mathf.Max(0f, minCooldown), maxCooldown));
            if (cooldown > 0f)
                yield return new WaitForSeconds(cooldown);
            if (!_health.IsAlive) yield break;
            if (_abilityInProgress) continue;
            if (_enemy.state != EnemyController.EnemyState.Attack &&
                _enemy.state != EnemyController.EnemyState.Chase)
                continue;

            HeavyAbilityType chosen = PickAbility();
            _abilityInProgress = true;
            Transform castTarget = _enemy.isRanged ? _enemy.CurrentTarget : null;
            yield return StartCoroutine(ExecuteAbility(chosen, castTarget, _enemy.isRanged));
            _abilityInProgress = false;
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
    IEnumerator ExecuteAbility(HeavyAbilityType type, Transform requiredTarget = null, bool cancelIfInvalid = false)
    {
        if (cancelIfInvalid && !IsOpeningTargetValid(requiredTarget))
        {
            _enemy.CancelCastAnimation();
            yield break;
        }
        // Trigger the prefab-selected cast/attack clip on every client before
        // the telegraph and resolve damage at its configured impact point.
        _enemy.PlayCastAnimation();
        RpcTelegraph(type, transform.position);
        float windup = Mathf.Max(0.05f, _enemy.attackImpactDelay);
        float remainingWindup = windup;
        while (remainingWindup > 0f)
        {
            if (cancelIfInvalid && (!IsOpeningTargetValid(requiredTarget) ||
                (openingCastRequiresLineOfSight && !HasLineOfSight(requiredTarget))))
            {
                _enemy.CancelCastAnimation();
                yield break;
            }
            float step = Mathf.Min(0.05f, remainingWindup);
            yield return new WaitForSeconds(step);
            remainingWindup -= step;
        }

        if (!_health.IsAlive) yield break;
        if (cancelIfInvalid && !IsOpeningTargetValid(requiredTarget))
        {
            _enemy.CancelCastAnimation();
            yield break;
        }
        if (requiredTarget != null && openingCastRequiresLineOfSight && !HasLineOfSight(requiredTarget))
        {
            _enemy.CancelCastAnimation();
            yield break;
        }

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
                Vector3[] chainHits = HitChain(14f, maxJumps: 3, dmgPerJump: dmg * 0.85f,
                    falloff: 0.25f,
                    statusType: StatusEffectType.Weakened, statusDur: 4f, statusVal: 0f,
                    preferredFirstTarget: requiredTarget);
                if (chainHits.Length > 0)
                    RpcPresentChainLightning(chainHits);
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

        // Zero cooldown means the next cast starts when this animation cycle
        // finishes, not on top of the current spell's impact frame.
        float animationRecovery = Mathf.Max(0f, _enemy.attackInterval - windup);
        while (animationRecovery > 0f)
        {
            if (cancelIfInvalid && (!IsOpeningTargetValid(requiredTarget) ||
                (openingCastRequiresLineOfSight && !HasLineOfSight(requiredTarget))))
            {
                _enemy.CancelCastAnimation();
                yield break;
            }
            float step = Mathf.Min(0.05f, animationRecovery);
            yield return new WaitForSeconds(step);
            animationRecovery -= step;
        }
    }

    bool IsOpeningTargetValid(Transform target)
    {
        if (target == null || _enemy == null || _health == null || !_health.IsAlive) return false;
        Health targetHealth = target.GetComponent<Health>();
        if (targetHealth == null || !targetHealth.IsAlive) return false;
        if (_enemy.CurrentTarget != target) return false;
        if (!_enemy.IsTargetWithinLeash(target)) return false;
        return Vector3.Distance(transform.position, target.position) <= castDistanceToTarget;
    }

    bool HasLineOfSight(Transform target)
    {
        if (target == null) return false;
        Vector3 origin = transform.position + Vector3.up * 1.25f;
        Vector3 destination = target.position + Vector3.up;
        Vector3 direction = destination - origin;
        float distance = direction.magnitude;
        if (distance <= 0.01f) return true;

        RaycastHit[] hits = Physics.RaycastAll(
            origin, direction / distance, distance, ~0, QueryTriggerInteraction.Ignore);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
        foreach (RaycastHit hit in hits)
        {
            Transform hitTransform = hit.transform;
            if (hitTransform == null || hitTransform == transform || hitTransform.IsChildOf(transform))
                continue;
            return hitTransform == target || hitTransform.IsChildOf(target) || target.IsChildOf(hitTransform);
        }
        return true;
    }

    // ── Damage helpers (all [Server]) ────────────────────────────────────────────

    [Server]
    void HitPlayersInRadius(Vector3 center, float radius, float dmg,
        StatusEffectType status, float statusDur, float statusVal)
    {
        foreach (var col in ZonePhysics.OverlapSphere(gameObject, center, radius))
        {
            if (!col.CompareTag("Player")) continue;
            ApplyHit(col.gameObject, dmg, status, statusDur, statusVal);
        }
    }

    [Server]
    Vector3[] HitChain(float searchRadius, int maxJumps, float dmgPerJump,
        float falloff, StatusEffectType statusType, float statusDur, float statusVal,
        Transform preferredFirstTarget)
    {
        var candidates = new List<GameObject>();
        foreach (var col in ZonePhysics.OverlapSphere(gameObject, transform.position, searchRadius))
        {
            Health playerHealth = col.GetComponentInParent<Health>();
            if (playerHealth == null || !playerHealth.isPlayer || !playerHealth.IsAlive) continue;
            GameObject player = playerHealth.gameObject;
            if (!candidates.Contains(player))
                candidates.Add(player);
        }

        var ordered = new List<GameObject>(maxJumps);
        Health preferredHealth = preferredFirstTarget != null
            ? preferredFirstTarget.GetComponentInParent<Health>()
            : null;
        GameObject preferred = preferredHealth != null && preferredHealth.isPlayer && preferredHealth.IsAlive
            ? preferredHealth.gameObject
            : null;

        // The cast target was already validated before this method. Always make
        // it the first jump; searchRadius controls acquisition of extra bounces,
        // not whether the spell can hit its intended primary target.
        if (preferred != null)
        {
            candidates.Remove(preferred);
            ordered.Add(preferred);
        }

        Vector3 searchFrom = ordered.Count > 0 ? ordered[0].transform.position : transform.position;
        while (ordered.Count < maxJumps && candidates.Count > 0)
        {
            int nearestIndex = 0;
            float nearestSqr = float.PositiveInfinity;
            for (int i = 0; i < candidates.Count; i++)
            {
                float sqr = (candidates[i].transform.position - searchFrom).sqrMagnitude;
                if (sqr >= nearestSqr) continue;
                nearestSqr = sqr;
                nearestIndex = i;
            }

            GameObject next = candidates[nearestIndex];
            candidates.RemoveAt(nearestIndex);
            ordered.Add(next);
            searchFrom = next.transform.position;
        }

        var hitPositions = new Vector3[ordered.Count];
        for (int jump = 0; jump < ordered.Count; jump++)
        {
            GameObject target = ordered[jump];
            float scaled = dmgPerJump * Mathf.Pow(1f - falloff, jump);
            ApplyHit(target, scaled, statusType, statusDur, statusVal);
            hitPositions[jump] = target.transform.position + Vector3.up;
        }
        return hitPositions;
    }

    [Server]
    void HitPlayersCone(Vector3 origin, Vector3 targetPos, float range, float angleDeg,
        float dmg, StatusEffectType status, float statusDur, float statusVal)
    {
        Vector3 forward = (targetPos - origin).normalized;
        forward.y = 0f;
        if (forward == Vector3.zero) forward = transform.forward;

        foreach (var col in ZonePhysics.OverlapSphere(gameObject, origin, range))
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
            if (p.scene != gameObject.scene) continue;
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
        Transform currentTarget = _enemy != null ? _enemy.CurrentTarget : null;
        if (currentTarget != null)
        {
            var currentHealth = currentTarget.GetComponent<Health>();
            if (currentHealth != null && currentHealth.IsAlive)
                return currentTarget.position;
        }
        // Try to read the enemy's current target via reflection — it's private,
        // so we fall back to the nearest player if none found.
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        float best = Mathf.Infinity;
        Vector3 pos = transform.position;

        foreach (var p in players)
        {
            if (p == null) continue;
            if (p.scene != gameObject.scene) continue;
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

    [ClientRpc]
    void RpcPresentChainLightning(Vector3[] hitPositions)
    {
#if UNITY_EDITOR || !UNITY_SERVER
        ChainLightningVFXProfile profile = ChainLightningVFXProfile.LoadArcane();
        if (profile != null)
            profile.Present(gameObject, hitPositions);
#endif
    }
}
