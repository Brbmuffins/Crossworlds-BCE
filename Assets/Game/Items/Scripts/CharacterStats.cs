using UnityEngine;

// Central hub for gear-driven character power.
// DESIGN PILLAR: no leveling — every bonus here comes from equipped gear and
// the attunements socketed into it.
//
// Reads Equipment, sums every StatModifier, and:
//   • pushes Max Health + Damage Reduction into Health (which owns those)
//   • exposes DamageMultiplier / CooldownReduction / MoveSpeedMultiplier /
//     HealMultiplier as read-only properties for AbilityCaster, PlayerMovement,
//     and healing code to consume.
//
// Call Recalculate() whenever equipped gear or its attunements change
// (Equipment does this automatically on equip/unequip).
[RequireComponent(typeof(Health))]
public class CharacterStats : MonoBehaviour
{
    private Health    _health;
    private Equipment _equipment;

    // ── Aggregated results (recomputed on every gear change) ──────────
    public float MaxHealthBonus      { get; private set; }        // flat HP added
    public float DamageMultiplier    { get; private set; } = 1f;  // ×outgoing damage
    public float DamageReduction     { get; private set; }        // 0..0.8 fraction
    public float MoveSpeedMultiplier { get; private set; } = 1f;  // ×movement speed
    public float CooldownReduction   { get; private set; }        // 0..0.6 fraction
    public float HealMultiplier      { get; private set; } = 1f;  // ×healing dealt
    public float Tenacity            { get; private set; }        // 0..0.60 — cuts CC duration applied to you
    public float ControlPower        { get; private set; }        // 0..0.50 — extends CC duration you apply

    // ── Mastery overlay (driven by HeroMasteryManager) ───────────
    // Percentages, additive on top of gear values. Updated via SetMasteryBonuses().
    private float _masteryDmgPct   = 0f;   // e.g. 0.08 = +8% damage
    private float _masteryHealPct  = 0f;
    private float _masteryCdrPct   = 0f;
    private float _masteryMaxHpPct = 0f;   // fraction of BaseMaxHealth added as flat HP

    /// <summary>
    /// Called by HeroMasteryManager when mastery data loads or levels change.
    /// All params are additive percentages (0.08 = 8%).
    /// maxHpPct is applied as a fraction of Health.BaseMaxHealth.
    /// </summary>
    public void SetMasteryBonuses(float dmgPct, float healPct, float cdrPct, float maxHpPct)
    {
        _masteryDmgPct   = dmgPct;
        _masteryHealPct  = healPct;
        _masteryCdrPct   = cdrPct;
        _masteryMaxHpPct = maxHpPct;
        Recalculate();
    }

    // ── Temporary CDR bonus (Overdrive ability) ────────────────────
    // Additive on top of gear CDR. Clamped alongside gear CDR to the 0.6 ceiling.
    // Call AddTemporaryCDR(+0.30f) to apply, AddTemporaryCDR(-0.30f) to remove.
    private float _temporaryCDR = 0f;
    public float EffectiveCooldownReduction
    {
        get
        {
            var cfg = CombatBalanceConfig.Instance;
            float cap = cfg != null ? cfg.overdriveCdrCap : 0.6f;
            return Mathf.Clamp(CooldownReduction + _temporaryCDR, 0f, cap);
        }
    }

    public void AddTemporaryCDR(float delta)
    {
        _temporaryCDR = Mathf.Clamp(_temporaryCDR + delta, -0.6f, 0.6f);
    }

    // ── Temporary damage bonus (consumable flasks) ─────────────────
    // Additive percentage folded into DamageMultiplier on Recalculate.
    // Call AddTemporaryDamagePct(+0.15f) to apply, AddTemporaryDamagePct(-0.15f) to remove.
    private float _temporaryDmgPct = 0f;

    public void AddTemporaryDamagePct(float delta)
    {
        _temporaryDmgPct = Mathf.Clamp(_temporaryDmgPct + delta, -1f, 2f);
        Recalculate();
    }

    void Awake()
    {
        _health    = GetComponent<Health>();
        _equipment = GetComponent<Equipment>();
    }

    void Start()
    {
        Recalculate();
    }

    // Re-reads all equipped gear + attunements and re-applies the totals.
    public void Recalculate()
    {
        float flatHealth = 0f;
        float pctDamage = 0f, pctDR = 0f, pctSpeed = 0f, pctCdr = 0f, pctHeal = 0f;
        float pctTenacity = 0f, pctControl = 0f;

        if (_equipment != null)
        {
            foreach (var kvp in _equipment.equippedItems)
            {
                ItemData item = kvp.Value;
                if (item == null) continue;

                foreach (var m in item.AllModifiers())
                {
                    switch (m.stat)
                    {
                        case StatType.MaxHealth:
                            flatHealth += m.kind == ModifierKind.Percent
                                ? _health.BaseMaxHealth * m.value
                                : m.value;
                            break;
                        case StatType.Damage:            pctDamage   += m.value; break;
                        case StatType.DamageReduction:   pctDR       += m.value; break;
                        case StatType.MoveSpeed:         pctSpeed    += m.value; break;
                        case StatType.CooldownReduction: pctCdr      += m.value; break;
                        case StatType.HealPower:         pctHeal     += m.value; break;
                        case StatType.Tenacity:          pctTenacity += m.value; break;
                        case StatType.ControlPower:      pctControl  += m.value; break;
                    }
                }
            }
        }

        float masteryHpFlat = _health != null ? _health.BaseMaxHealth * _masteryMaxHpPct : 0f;
        MaxHealthBonus      = flatHealth + masteryHpFlat;
        DamageMultiplier    = Mathf.Max(0f,   1f + pctDamage + _masteryDmgPct + _temporaryDmgPct);
        var cfg          = CombatBalanceConfig.Instance;
        float drCap      = cfg != null ? cfg.gearDrCap        : 0.8f;
        float cdrCap     = cfg != null ? cfg.gearCdrCap       : 0.6f;
        float tenCap     = cfg != null ? cfg.tenacityCap      : 0.6f;
        float controlCap = cfg != null ? cfg.controlPowerCap  : 0.5f;

        DamageReduction     = Mathf.Clamp(pctDR,  0f, drCap);
        MoveSpeedMultiplier = Mathf.Max(0.1f, 1f + pctSpeed);
        CooldownReduction   = Mathf.Clamp(pctCdr + _masteryCdrPct, 0f, cdrCap);
        HealMultiplier      = Mathf.Max(0f,   1f + pctHeal + _masteryHealPct);
        Tenacity            = Mathf.Clamp(pctTenacity, 0f, tenCap);
        ControlPower        = Mathf.Clamp(pctControl,  0f, controlCap);

        // Hand off the channels Health owns.
        if (_health != null)
        {
            _health.SetGearMaxHealthBonus(MaxHealthBonus);
            _health.SetGearDamageReduction(DamageReduction);
        }
    }
}
