using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// Attach to every character (player and enemy) that can receive status effects.
public class StatusEffectManager : MonoBehaviour
{
    private readonly List<StatusEffect> _effects = new List<StatusEffect>();

    [Tooltip("How often (s) damage-over-time effects deal their damage.")]
    public float dotTickInterval = 0.5f;
    private float         _dotTimer = 0f;
    private Health        _health;
    private CharacterStats _stats;   // this character's Tenacity (target side of the CC formula)

    // Stun diminishing-returns state (all tunables live on CombatBalanceConfig).
    private int   _stunsInWindow  = 0;
    private float _stunWindowEnds = 0f;
    private float _stunImmuneUntil = 0f;

    // Quick state queries used by other systems
    public bool IsSilenced  => HasEffect(StatusEffectType.Silenced);
    public bool IsWeakened  => HasEffect(StatusEffectType.Weakened);
    public bool IsBound     => HasEffect(StatusEffectType.Bound);
    public bool IsStaggered => HasEffect(StatusEffectType.Stagger);
    public bool IsStunned   => HasEffect(StatusEffectType.Stun);
    public bool IsStunImmune => Time.time < _stunImmuneUntil;

    public UnityEvent<StatusEffectType> onEffectAdded;
    public UnityEvent<StatusEffectType> onEffectRemoved;
    public UnityEvent                   onAllEffectsCleared;

    void Awake()
    {
        _health = GetComponent<Health>();
        _stats  = GetComponent<CharacterStats>();
    }

    void Update()
    {
        bool stunExpired = false;
        for (int i = _effects.Count - 1; i >= 0; i--)
        {
            _effects[i].remainingTime -= Time.deltaTime;
            if (_effects[i].IsExpired)
            {
                var t = _effects[i].type;
                _effects.RemoveAt(i);
                onEffectRemoved?.Invoke(t);
                if (t == StatusEffectType.Stun) stunExpired = true;
            }
        }
        if (stunExpired) SyncStunFlag();

        TickDamageOverTime();
    }

    // Mirror the live stun state onto the networked Health flag so client-side
    // player movement / ability code can gate on it. No-op on pure clients
    // (Health.SetStunned is server/offline-guarded).
    void SyncStunFlag()
    {
        _health?.SetStunned(IsStunned);
    }

    // Applies all active DamageOverTime effects on a fixed tick. Summing first
    // and dealing damage once per tick keeps onDamageTaken from firing every
    // frame (which would, e.g., spam Threat Protocol stacks).
    void TickDamageOverTime()
    {
        float dps = 0f;
        GameObject src = null;
        foreach (var e in _effects)
        {
            if (e.type != StatusEffectType.Cursed) continue;
            dps += e.value;
            if (src == null) src = e.source;
        }

        if (dps <= 0f) { _dotTimer = 0f; return; }

        _dotTimer += Time.deltaTime;
        if (_dotTimer < dotTickInterval) return;
        _dotTimer = 0f;

        _health?.TakeDamage(dps * dotTickInterval, src);
    }

    // Server-authoritative CC duration formula: Tenacity on the target shortens
    // control effects, ControlPower on the caster lengthens them. Applies to
    // Slow / Silenced / Bound (and Stun, via ApplyStun). Never touches DoTs
    // (Cursed), the Weakened amplifier, or the fixed Stagger interrupt.
    static bool IsControlDuration(StatusEffectType t) =>
        t == StatusEffectType.Slow || t == StatusEffectType.Silenced || t == StatusEffectType.Bound;

    float ScaleControlDuration(float baseDuration, GameObject caster)
    {
        float tenacity = _stats != null ? _stats.Tenacity : 0f;
        float control  = 0f;
        if (caster != null)
        {
            var cs = caster.GetComponentInParent<CharacterStats>();
            if (cs != null) control = cs.ControlPower;
        }
        return CombatBalanceConfig.CCDuration(baseDuration, tenacity, control);
    }

    /// <summary>
    /// Apply a Stun with the full fairness model: Tenacity/ControlPower scaling,
    /// hard diminishing returns on repeats within the window, and a Stun-immunity
    /// grant after the threshold. All numbers come from CombatBalanceConfig.
    /// Pass duration &lt;= 0 to use the config's stunBaseDuration. Server-only path.
    /// </summary>
    public void ApplyStun(float duration = 0f, GameObject caster = null)
    {
        var cfg   = CombatBalanceConfig.Instance;
        float now = Time.time;

        if (now < _stunImmuneUntil) return;   // immune — ignore entirely

        float baseDur = duration > 0f
            ? duration
            : (cfg != null ? cfg.stunBaseDuration : 1.5f);

        // Reset the DR window if it lapsed since the last stun.
        if (now > _stunWindowEnds) _stunsInWindow = 0;
        float window = cfg != null ? cfg.stunFalloffWindow : 8f;
        _stunWindowEnds = now + window;

        // Tenacity/ControlPower, then diminishing returns per repeat in the window.
        float scaled  = ScaleControlDuration(baseDur, caster);
        float falloff = cfg != null ? cfg.stunRepeatFalloff : 0.5f;
        scaled       *= Mathf.Pow(Mathf.Clamp01(falloff), _stunsInWindow);

        if (scaled > 0.01f)
        {
            AddEffect(new StatusEffect(StatusEffectType.Stun, scaled, 0f, caster));
            SyncStunFlag();
        }

        _stunsInWindow++;

        int threshold = cfg != null ? cfg.stunImmunityThreshold : 3;
        if (threshold > 0 && _stunsInWindow >= threshold)
        {
            float immDur = cfg != null ? cfg.stunImmunityDuration : 4f;
            _stunImmuneUntil = now + immDur;
            _stunsInWindow   = 0;
        }
    }

    // Adds effect; refreshes duration if same type already present.
    public void AddEffect(StatusEffect effect)
    {
        // Fold in the CC duration formula for control effects (Stun arrives here
        // pre-scaled from ApplyStun, so it's excluded to avoid double-scaling).
        if (effect != null && IsControlDuration(effect.type))
        {
            float scaled = ScaleControlDuration(effect.duration, effect.source);
            effect.duration      = scaled;
            effect.remainingTime = scaled;
        }

        for (int i = 0; i < _effects.Count; i++)
        {
            if (_effects[i].type == effect.type)
            {
                _effects[i].remainingTime = Mathf.Max(_effects[i].remainingTime, effect.duration);
                _effects[i].value         = Mathf.Max(_effects[i].value, effect.value); // keep the stronger magnitude on refresh
                _effects[i].source        = effect.source;
                return;
            }
        }
        _effects.Add(effect);
        onEffectAdded?.Invoke(effect.type);
    }

    // Purge Protocol: remove every effect instantly.
    public void RemoveAll()
    {
        _effects.Clear();
        SyncStunFlag();   // clears the networked stun flag
        onAllEffectsCleared?.Invoke();
    }

    public bool HasEffect(StatusEffectType type)
    {
        foreach (var e in _effects)
            if (e.type == type) return true;
        return false;
    }

    // Effect types Dark Harvest can detonate. Bound (the Ironclad's leash) is
    // positional control rather than detonatable decay, so it's excluded — a
    // Shadowblade's Dark Harvest shouldn't break the Ironclad's chain.
    public static bool IsDebuff(StatusEffectType t)
    {
        switch (t)
        {
            case StatusEffectType.Slow:
            case StatusEffectType.Stagger:
            case StatusEffectType.Silenced:
            case StatusEffectType.Cursed:
            case StatusEffectType.Weakened:
                return true;
            default:
                return false;
        }
    }

    // Returns how many detonatable debuffs are active (consumed by Collapse).
    public int CountDebuffStacks()
    {
        int n = 0;
        foreach (var e in _effects)
            if (IsDebuff(e.type)) n++;
        return n;
    }

    // Collapse consumes all detonatable debuffs for damage — removes them and returns count.
    public int ConsumeDebuffStacks()
    {
        int count = 0;
        for (int i = _effects.Count - 1; i >= 0; i--)
        {
            if (IsDebuff(_effects[i].type))
            {
                count++;
                _effects.RemoveAt(i);
            }
        }
        if (count > 0) onAllEffectsCleared?.Invoke();
        return count;
    }

    public List<StatusEffect> GetAll() => new List<StatusEffect>(_effects);

    // Convenience wrappers used by boss controllers
    public void ApplyRoot(float duration) =>
        AddEffect(new StatusEffect(StatusEffectType.Bound, duration, 0f));

    // Resist helpers: each damage type is its own DR source, so multiple resist
    // flasks (and Siege Mode / Threat Protocol) coexist without clobbering.
    public void AddResist(string damageType, float fraction)
    {
        GetComponent<Health>()?.SetDamageReduction("resist_" + damageType, fraction);
    }

    public void RemoveResist(string damageType, float fraction)
    {
        GetComponent<Health>()?.ClearDamageReduction("resist_" + damageType);
    }

    // Applies a slow (0–1 fraction) to attached PlayerMovement or EnemyAI.
    public float GetSlowFraction()
    {
        float max = 0f;
        foreach (var e in _effects)
            if (e.type == StatusEffectType.Slow) max = Mathf.Max(max, e.value);
        return max;
    }
}
