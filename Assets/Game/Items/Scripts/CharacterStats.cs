using Mirror;
using UnityEngine;
using UnityEngine.Events;

// Central hub for character power modifiers.
// The old local Equipment/Inventory path has been retired; live bonuses now come
// from inspector baselines, mastery, and temporary effects until server-backed
// gear stats are wired in.
[RequireComponent(typeof(Health))]
public class CharacterStats : NetworkBehaviour
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

    [Header("Base Resource Stats")]
    [Tooltip("Maximum mana available for casting spells.")]
    [SerializeField, Min(0f)] private float baseMaxMana = 100f;

    [Tooltip("Health regenerated every 5 seconds.")]
    [SerializeField, Min(0f)] private float baseHp5 = 0f;

    [Tooltip("Mana regenerated every 5 seconds.")]
    [SerializeField, Min(0f)] private float baseMp5 = 5f;

    [Header("Base Utility Stats")]
    [Tooltip("Movement speed bonus. 0.10 = +10% move speed.")]
    [SerializeField] private float baseMoveSpeedBonusPct = 0f;

    [Tooltip("Ability cooldown reduction. 0.10 = 10% shorter cooldowns.")]
    [SerializeField, Range(0f, MaxCooldownReduction)] private float baseCooldownReduction = 0f;

    [Tooltip("Healing output bonus. 0.10 = +10% healing.")]
    [SerializeField] private float baseHealBonusPct = 0f;

    [Header("Level Progression Tuning")]
    [Tooltip("Outgoing damage gained per point in the class primary stat above its starting value.")]
    [SerializeField, Min(0f)] private float primaryStatDamagePctPerPoint = 0.01f;
    [Tooltip("Additional outgoing damage gained per Strength point above the starting value of 5.")]
    [SerializeField, Min(0f)] private float strengthDamagePctPerPoint = 0.005f;
    [Tooltip("Flat max health gained per Vitality point above the starting value of 10.")]
    [SerializeField, Min(0f)] private float maxHealthPerVitality = 5f;
    [Tooltip("Maximum mana gained per Intelligence point above the starting value of 5.")]
    [SerializeField, Min(0f)] private float maxManaPerIntelligence = 2f;
    [Tooltip("Movement speed gained per Agility point above the starting value of 5.")]
    [SerializeField, Min(0f)] private float moveSpeedPctPerAgility = 0.0025f;

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

    [Tooltip("Current max mana after base and future item bonuses.")]
    [SerializeField] private float effectiveMaxMana = 100f;

    [Tooltip("Current mana. Runtime only; initialized to max mana when play starts.")]
    [SerializeField] private float currentMana = 0f;

    [Tooltip("Current health regenerated every 5 seconds.")]
    [SerializeField] private float effectiveHp5 = 0f;

    [Tooltip("Current mana regenerated every 5 seconds.")]
    [SerializeField] private float effectiveMp5 = 5f;

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
    public float MaxMana             { get; private set; } = 100f;
    public float CurrentMana         => currentMana;
    public float ManaFraction        => MaxMana > 0f ? Mathf.Clamp01(currentMana / MaxMana) : 0f;
    public float Hp5                 { get; private set; }
    public float Mp5                 { get; private set; } = 5f;
    public float MoveSpeedMultiplier { get; private set; } = 1f;
    public float CooldownReduction   { get; private set; }
    public float HealMultiplier      { get; private set; } = 1f;

    [HideInInspector] public UnityEvent<float, float> onManaChanged = new UnityEvent<float, float>();

    // Mastery overlay, driven by HeroMasteryManager.
    private float _masteryDmgPct;
    private float _masteryHealPct;
    private float _masteryCdrPct;
    private float _masteryMaxHpPct;

    // Persistent level/stat overlay. Values are supplied by the authenticated character
    // record, never by the client's class-selection message.
    [SyncVar(hook = nameof(OnProgressionValueChanged))] private int _progressionClassIndex;
    [SyncVar(hook = nameof(OnProgressionValueChanged))] private int _progressionLevel = 1;
    [SyncVar(hook = nameof(OnProgressionValueChanged))] private int _progressionStr = 5;
    [SyncVar(hook = nameof(OnProgressionValueChanged))] private int _progressionAgi = 5;
    [SyncVar(hook = nameof(OnProgressionValueChanged))] private int _progressionInt = 5;
    [SyncVar(hook = nameof(OnProgressionValueChanged))] private int _progressionVit = 10;
    [SyncVar(hook = nameof(OnProgressionValueChanged))] private int _equipmentStr;
    [SyncVar(hook = nameof(OnProgressionValueChanged))] private int _equipmentAgi;
    [SyncVar(hook = nameof(OnProgressionValueChanged))] private int _equipmentInt;
    [SyncVar(hook = nameof(OnProgressionValueChanged))] private int _equipmentVit;
    private float _progressionDamagePct;
    private float _progressionMaxHealth;
    private float _progressionMaxMana;
    private float _progressionMoveSpeedPct;

    // Temporary channels, driven by active effects.
    private float _temporaryCDR;
    private float _temporaryDmgPct;
    private bool _manaInitialized;

    public float EffectiveCooldownReduction =>
        Mathf.Clamp(CooldownReduction + _temporaryCDR, 0f, MaxCooldownReduction);
    public int EffectiveStrength => _progressionStr + _equipmentStr;
    public int EffectiveAgility => _progressionAgi + _equipmentAgi;
    public int EffectiveIntelligence => _progressionInt + _equipmentInt;
    public int EffectiveVitality => _progressionVit + _equipmentVit;

    public void SetMasteryBonuses(float dmgPct, float healPct, float cdrPct, float maxHpPct)
    {
        _masteryDmgPct   = dmgPct;
        _masteryHealPct  = healPct;
        _masteryCdrPct   = cdrPct;
        _masteryMaxHpPct = maxHpPct;
        Recalculate();
    }

    /// <summary>Apply primary stats loaded from the authenticated account character.</summary>
    [Server]
    public void SetProgressionStats(int classIndex, int level, int strength, int agility,
        int intelligence, int vitality)
    {
        _progressionClassIndex = Mathf.Clamp(classIndex, 0, 4);
        _progressionLevel = Mathf.Max(1, level);
        _progressionStr = Mathf.Max(0, strength);
        _progressionAgi = Mathf.Max(0, agility);
        _progressionInt = Mathf.Max(0, intelligence);
        _progressionVit = Mathf.Max(0, vitality);

        RecalculateProgressionBonuses();
        Recalculate();
    }

    [Server]
    public void SetEquipmentStatBonuses(int strength, int agility, int intelligence, int vitality)
    {
        _equipmentStr = Mathf.Max(0, strength);
        _equipmentAgi = Mathf.Max(0, agility);
        _equipmentInt = Mathf.Max(0, intelligence);
        _equipmentVit = Mathf.Max(0, vitality);
        RecalculateProgressionBonuses();
        Recalculate();
    }

    void OnProgressionValueChanged(int _, int __)
    {
        RecalculateProgressionBonuses();
        Recalculate();
    }

    void RecalculateProgressionBonuses()
    {
        int primaryValue = _progressionClassIndex switch
        {
            0 => EffectiveIntelligence, // Marauder (legacy Engineer)
            1 => EffectiveVitality, // Ironclad (legacy Guardian)
            2 => EffectiveAgility, // Shadowblade / Night Hunter
            3 => EffectiveIntelligence, // Cleric
            4 => EffectiveIntelligence, // Arcanist
            _ => EffectiveStrength
        };
        int primaryBaseline = _progressionClassIndex == 1 ? 10 : 5;

        _progressionDamagePct =
            Mathf.Max(0, primaryValue - primaryBaseline) * primaryStatDamagePctPerPoint +
            Mathf.Max(0, EffectiveStrength - 5) * strengthDamagePctPerPoint;
        _progressionMaxHealth = Mathf.Max(0, EffectiveVitality - 10) * maxHealthPerVitality;
        _progressionMaxMana = Mathf.Max(0, EffectiveIntelligence - 5) * maxManaPerIntelligence;
        _progressionMoveSpeedPct = Mathf.Max(0, EffectiveAgility - 5) * moveSpeedPctPerAgility;
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
        if (onManaChanged == null)
            onManaChanged = new UnityEvent<float, float>();

        _health = GetComponent<Health>();
    }

    void Start()
    {
        Recalculate();
    }

    void Update()
    {
        TickRegeneration();
    }

    public void Recalculate()
    {
        if (_health == null)
            _health = GetComponent<Health>();

        float masteryHpFlat = _health != null ? _health.BaseMaxHealth * _masteryMaxHpPct : 0f;

        MaxHealthBonus = Mathf.Max(0f, baseMaxHealthBonus) + masteryHpFlat + _progressionMaxHealth;
        DamageMultiplier = Mathf.Max(0f, 1f + baseDamageBonusPct + _progressionDamagePct + _masteryDmgPct + _temporaryDmgPct);
        CriticalStrikeChance = Mathf.Clamp01(baseCriticalStrikeChance);
        CriticalStrikeDamageMultiplier = Mathf.Max(1f, baseCriticalStrikeDamageMultiplier);
        DamageReduction = Mathf.Clamp01(baseDamageReduction);
        MaxMana = Mathf.Max(0f, baseMaxMana + _progressionMaxMana);
        Hp5 = Mathf.Max(0f, baseHp5);
        Mp5 = Mathf.Max(0f, baseMp5);
        MoveSpeedMultiplier = Mathf.Max(0f, 1f + baseMoveSpeedBonusPct + _progressionMoveSpeedPct);
        CooldownReduction = Mathf.Clamp(baseCooldownReduction + _masteryCdrPct, 0f, MaxCooldownReduction);
        HealMultiplier = Mathf.Max(0f, 1f + baseHealBonusPct + _masteryHealPct);

        ClampOrInitializeMana();

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
        baseMaxMana = Mathf.Max(0f, baseMaxMana);
        baseHp5 = Mathf.Max(0f, baseHp5);
        baseMp5 = Mathf.Max(0f, baseMp5);
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
        effectiveMaxMana = Mathf.Max(0f, baseMaxMana);
        currentMana = Mathf.Clamp(currentMana <= 0f ? effectiveMaxMana : currentMana, 0f, effectiveMaxMana);
        effectiveHp5 = Mathf.Max(0f, baseHp5);
        effectiveMp5 = Mathf.Max(0f, baseMp5);
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
        effectiveMaxMana = MaxMana;
        effectiveHp5 = Hp5;
        effectiveMp5 = Mp5;
        effectiveMoveSpeedMultiplier = MoveSpeedMultiplier;
        effectiveCooldownReduction = EffectiveCooldownReduction;
        effectiveHealMultiplier = HealMultiplier;
    }

    void TickRegeneration()
    {
        if (!Application.isPlaying)
            return;

        if (Mp5 > 0f && MaxMana > 0f && currentMana < MaxMana)
            RestoreMana((Mp5 / 5f) * Time.deltaTime);

        if (Hp5 <= 0f || _health == null || _health.currentHealth <= 0f || _health.currentHealth >= _health.maxHealth)
            return;

        // Health is server-authoritative in networked play. Mana is predicted locally
        // for HUD responsiveness, but HP should only mutate on the server/host.
        if (!NetworkClient.active || NetworkServer.active)
            _health.Heal((Hp5 / 5f) * Time.deltaTime, false, false);
    }

    void ClampOrInitializeMana()
    {
        float oldMana = currentMana;
        if (!_manaInitialized)
        {
            currentMana = MaxMana;
            _manaInitialized = true;
        }
        else
        {
            currentMana = Mathf.Clamp(currentMana, 0f, MaxMana);
        }

        if (!Mathf.Approximately(oldMana, currentMana))
            onManaChanged?.Invoke(currentMana, MaxMana);
    }

    public bool HasMana(float amount)
    {
        amount = Mathf.Max(0f, amount);
        if (amount <= 0f) return true;
        return currentMana + 0.001f >= amount;
    }

    public bool TrySpendMana(float amount)
    {
        amount = Mathf.Max(0f, amount);
        if (!HasMana(amount))
            return false;

        if (amount <= 0f)
            return true;

        SetCurrentMana(currentMana - amount);
        return true;
    }

    public void RestoreMana(float amount)
    {
        amount = Mathf.Max(0f, amount);
        if (amount <= 0f || MaxMana <= 0f)
            return;

        SetCurrentMana(currentMana + amount);
    }

    public void RefillMana()
    {
        SetCurrentMana(MaxMana);
    }

    void SetCurrentMana(float value)
    {
        float clamped = Mathf.Clamp(value, 0f, MaxMana);
        if (Mathf.Approximately(currentMana, clamped))
            return;

        currentMana = clamped;
        onManaChanged?.Invoke(currentMana, MaxMana);
    }
}
