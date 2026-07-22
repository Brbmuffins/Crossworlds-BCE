using UnityEngine;

// Central hub for character power modifiers.
// The old local Equipment/Inventory path has been retired; live bonuses now come
// from inspector baselines, mastery, and temporary effects until server-backed
// gear stats are wired in.
[RequireComponent(typeof(Health))]
public class CharacterStats : MonoBehaviour
{
    const float MaxCooldownReduction = 0.6f;

    [Header("Base Combat Stats")]
    [Tooltip("Flat max health added before mastery bonuses.")]
    [SerializeField, Min(0f)] private float baseMaxHealthBonus = 0f;

    [Tooltip("Outgoing damage bonus. 0.10 = +10% damage.")]
    [SerializeField] private float baseDamageBonusPct = 0f;

    [Tooltip("Chance for this character's ability damage to critically strike. 0.25 = 25%.")]
    [SerializeField, Range(0f, 1f)] private float baseCriticalStrikeChance = 0f;

    [Tooltip("Damage multiplier used when a critical strike happens. 1.5 = 150% damage.")]
    [SerializeField, Min(1f)] private float baseCriticalStrikeDamageMultiplier = 1.5f;

    [Tooltip("Incoming damage reduction. 0.10 = 10% less damage taken.")]
    [SerializeField, Range(0f, 0.95f)] private float baseDamageReduction = 0f;

    [Header("Base Utility Stats")]
    [Tooltip("Movement speed bonus. 0.10 = +10% move speed.")]
    [SerializeField] private float baseMoveSpeedBonusPct = 0f;

    [Tooltip("Ability cooldown reduction. 0.10 = 10% shorter cooldowns.")]
    [SerializeField, Range(0f, MaxCooldownReduction)] private float baseCooldownReduction = 0f;

    [Tooltip("Healing output bonus. 0.10 = +10% healing.")]
    [SerializeField] private float baseHealBonusPct = 0f;

    [Header("Runtime Readouts")]
    [Tooltip("Current flat max health bonus after base and mastery bonuses.")]
    [SerializeField] private float effectiveMaxHealthBonus = 0f;

    [Tooltip("Current outgoing damage multiplier after base, mastery, and temporary bonuses.")]
    [SerializeField] private float effectiveDamageMultiplier = 1f;

    [Tooltip("Current critical strike chance after all bonuses.")]
    [SerializeField] private float effectiveCriticalStrikeChance = 0f;

    [Tooltip("Current critical strike damage multiplier.")]
    [SerializeField] private float effectiveCriticalStrikeDamageMultiplier = 1.5f;

    [Tooltip("Current incoming damage reduction after all bonuses.")]
    [SerializeField] private float effectiveDamageReduction = 0f;

    [Tooltip("Current movement speed multiplier after all bonuses.")]
    [SerializeField] private float effectiveMoveSpeedMultiplier = 1f;

    [Tooltip("Current cooldown reduction after base, mastery, and temporary bonuses.")]
    [SerializeField] private float effectiveCooldownReduction = 0f;

    [Tooltip("Current healing multiplier after base and mastery bonuses.")]
    [SerializeField] private float effectiveHealMultiplier = 1f;

    private Health _health;

    public float MaxHealthBonus      { get; private set; }
    public float DamageMultiplier    { get; private set; } = 1f;
    public float CriticalStrikeChance { get; private set; }
    public float CriticalStrikeDamageMultiplier { get; private set; } = 1.5f;
    public float DamageReduction     { get; private set; }
    public float MoveSpeedMultiplier { get; private set; } = 1f;
    public float CooldownReduction   { get; private set; }
    public float HealMultiplier      { get; private set; } = 1f;

    // Mastery overlay, driven by HeroMasteryManager.
    private float _masteryDmgPct;
    private float _masteryHealPct;
    private float _masteryCdrPct;
    private float _masteryMaxHpPct;

    // Temporary channels, driven by active effects.
    private float _temporaryCDR;
    private float _temporaryDmgPct;

    public float EffectiveCooldownReduction =>
        Mathf.Clamp(CooldownReduction + _temporaryCDR, 0f, MaxCooldownReduction);

    public void SetMasteryBonuses(float dmgPct, float healPct, float cdrPct, float maxHpPct)
    {
        _masteryDmgPct   = dmgPct;
        _masteryHealPct  = healPct;
        _masteryCdrPct   = cdrPct;
        _masteryMaxHpPct = maxHpPct;
        Recalculate();
    }

    public void AddTemporaryCDR(float delta)
    {
        _temporaryCDR = Mathf.Clamp(_temporaryCDR + delta, -MaxCooldownReduction, MaxCooldownReduction);
        UpdateReadouts();
    }

    public void AddTemporaryDamagePct(float delta)
    {
        _temporaryDmgPct = Mathf.Clamp(_temporaryDmgPct + delta, -1f, 2f);
        Recalculate();
    }

    public float ApplyCriticalStrike(float damage, out bool wasCritical)
    {
        wasCritical = false;
        float finalDamage = Mathf.Max(0f, damage);

        if (finalDamage <= 0f
            || CriticalStrikeChance <= 0f
            || CriticalStrikeDamageMultiplier <= 1f)
            return finalDamage;

        if (Random.value >= CriticalStrikeChance)
            return finalDamage;

        wasCritical = true;
        return finalDamage * CriticalStrikeDamageMultiplier;
    }

    void Awake()
    {
        _health = GetComponent<Health>();
    }

    void Start()
    {
        Recalculate();
    }

    public void Recalculate()
    {
        if (_health == null)
            _health = GetComponent<Health>();

        float masteryHpFlat = _health != null ? _health.BaseMaxHealth * _masteryMaxHpPct : 0f;

        MaxHealthBonus = Mathf.Max(0f, baseMaxHealthBonus) + masteryHpFlat;
        DamageMultiplier = Mathf.Max(0f, 1f + baseDamageBonusPct + _masteryDmgPct + _temporaryDmgPct);
        CriticalStrikeChance = Mathf.Clamp01(baseCriticalStrikeChance);
        CriticalStrikeDamageMultiplier = Mathf.Max(1f, baseCriticalStrikeDamageMultiplier);
        DamageReduction = Mathf.Clamp01(baseDamageReduction);
        MoveSpeedMultiplier = Mathf.Max(0f, 1f + baseMoveSpeedBonusPct);
        CooldownReduction = Mathf.Clamp(baseCooldownReduction + _masteryCdrPct, 0f, MaxCooldownReduction);
        HealMultiplier = Mathf.Max(0f, 1f + baseHealBonusPct + _masteryHealPct);

        UpdateReadouts();

        if (_health != null)
        {
            _health.SetGearMaxHealthBonus(MaxHealthBonus);
            _health.SetGearDamageReduction(DamageReduction);
        }
    }

    void OnValidate()
    {
        baseMaxHealthBonus = Mathf.Max(0f, baseMaxHealthBonus);
        baseCriticalStrikeChance = Mathf.Clamp01(baseCriticalStrikeChance);
        baseCriticalStrikeDamageMultiplier = Mathf.Max(1f, baseCriticalStrikeDamageMultiplier);
        baseDamageReduction = Mathf.Clamp01(baseDamageReduction);
        baseCooldownReduction = Mathf.Clamp(baseCooldownReduction, 0f, MaxCooldownReduction);

        if (Application.isPlaying)
            Recalculate();
        else
            PreviewInspectorReadouts();
    }

    void PreviewInspectorReadouts()
    {
        effectiveMaxHealthBonus = Mathf.Max(0f, baseMaxHealthBonus);
        effectiveDamageMultiplier = Mathf.Max(0f, 1f + baseDamageBonusPct);
        effectiveCriticalStrikeChance = Mathf.Clamp01(baseCriticalStrikeChance);
        effectiveCriticalStrikeDamageMultiplier = Mathf.Max(1f, baseCriticalStrikeDamageMultiplier);
        effectiveDamageReduction = Mathf.Clamp01(baseDamageReduction);
        effectiveMoveSpeedMultiplier = Mathf.Max(0f, 1f + baseMoveSpeedBonusPct);
        effectiveCooldownReduction = Mathf.Clamp(baseCooldownReduction, 0f, MaxCooldownReduction);
        effectiveHealMultiplier = Mathf.Max(0f, 1f + baseHealBonusPct);
    }

    void UpdateReadouts()
    {
        effectiveMaxHealthBonus = MaxHealthBonus;
        effectiveDamageMultiplier = DamageMultiplier;
        effectiveCriticalStrikeChance = CriticalStrikeChance;
        effectiveCriticalStrikeDamageMultiplier = CriticalStrikeDamageMultiplier;
        effectiveDamageReduction = DamageReduction;
        effectiveMoveSpeedMultiplier = MoveSpeedMultiplier;
        effectiveCooldownReduction = EffectiveCooldownReduction;
        effectiveHealMultiplier = HealMultiplier;
    }
}
