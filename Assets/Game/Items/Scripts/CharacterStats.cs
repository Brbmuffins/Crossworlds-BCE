using UnityEngine;

// Central hub for character power modifiers.
// The old local Equipment/Inventory path has been retired; live bonuses now come
// from mastery and temporary effects until server-backed gear stats are wired in.
//
// Pushes Max Health + Damage Reduction into Health, and exposes
// DamageMultiplier / CooldownReduction / MoveSpeedMultiplier / HealMultiplier
// for AbilityCaster, PlayerMovement, and healing code to consume.
[RequireComponent(typeof(Health))]
public class CharacterStats : MonoBehaviour
{
    private Health _health;

    public float MaxHealthBonus      { get; private set; }
    public float DamageMultiplier    { get; private set; } = 1f;
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
        Mathf.Clamp(CooldownReduction + _temporaryCDR, 0f, 0.6f);

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
        _temporaryCDR = Mathf.Clamp(_temporaryCDR + delta, -0.6f, 0.6f);
    }

    public void AddTemporaryDamagePct(float delta)
    {
        _temporaryDmgPct = Mathf.Clamp(_temporaryDmgPct + delta, -1f, 2f);
        Recalculate();
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
        float masteryHpFlat = _health != null ? _health.BaseMaxHealth * _masteryMaxHpPct : 0f;

        MaxHealthBonus      = masteryHpFlat;
        DamageMultiplier    = Mathf.Max(0f, 1f + _masteryDmgPct + _temporaryDmgPct);
        DamageReduction     = 0f;
        MoveSpeedMultiplier = 1f;
        CooldownReduction   = Mathf.Clamp(_masteryCdrPct, 0f, 0.6f);
        HealMultiplier      = Mathf.Max(0f, 1f + _masteryHealPct);

        if (_health != null)
        {
            _health.SetGearMaxHealthBonus(MaxHealthBonus);
            _health.SetGearDamageReduction(DamageReduction);
        }
    }
}
