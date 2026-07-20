using UnityEngine;

/// <summary>
/// CombatBalanceConfig — single ScriptableObject holding every combat BALANCE
/// tunable (caps, clamps, CC durations, healing spill, wave scaling).
/// Sibling to FeelConfig, same pattern.
/// Create via: Assets → Create → Crossworlds → CombatBalanceConfig
/// Place the asset in a Resources/ folder named "CombatBalanceConfig".
///
/// DESIGN INTENT: no value below should ever be hardcoded in a system script.
/// Systems read CombatBalanceConfig.Instance.&lt;field&gt; and fall back to their own
/// Inspector default only if no asset is loaded. This keeps the whole balance
/// surface editable in the Unity Inspector with zero recompiles — designers tweak
/// the asset, press Play.
///
/// SERVER-SAFE: this is plain data (no client-only types), so it compiles into the
/// dedicated server build too. Because combat is server-authoritative, THIS asset in
/// the server build is the source of truth for balance at runtime.
/// </summary>
[CreateAssetMenu(fileName = "CombatBalanceConfig", menuName = "Crossworlds/CombatBalanceConfig")]
public class CombatBalanceConfig : ScriptableObject
{
    // ── Singleton (loaded from Resources/CombatBalanceConfig) ─────────────────
    static CombatBalanceConfig _instance;
    public static CombatBalanceConfig Instance
    {
        get
        {
            if (_instance == null)
                _instance = Resources.Load<CombatBalanceConfig>("CombatBalanceConfig");
            return _instance;
        }
    }

    // ── Power band (reference only — used by tooling / telemetry checks) ───────
    [Header("Power Band (base → BiS target, reference only)")]
    [Tooltip("Target throughput multiplier from base gear to fully geared. " +
             "Rarity roll tables should be derived so no channel exceeds this.")]
    public float targetPowerBandMin = 2.0f;
    public float targetPowerBandMax = 2.5f;

    // ── Damage Reduction caps & clamp ─────────────────────────────────────────
    [Header("Damage Reduction")]
    [Tooltip("Cap on additive gear DR sources (pipeline layer 4).")]
    [Range(0f, 0.95f)] public float gearDrCap = 0.80f;
    [Tooltip("FINAL clamp on TOTAL effective DR after ability DR (layer 3) AND gear DR " +
             "(layer 4) multiply together. Closes the multiplicative stacking hole. " +
             "0.85 = 15% damage always gets through (~6.7x EHP ceiling).")]
    [Range(0.5f, 1f)] public float totalDrClamp = 0.85f;

    // ── Cooldown Reduction caps ───────────────────────────────────────────────
    [Header("Cooldown Reduction")]
    [Tooltip("Cap on CDR reachable from GEAR + mastery. 0.40 = abilities fire 1.67x " +
             "as often. Keep this lower than the Overdrive cap so 'always casting' " +
             "stays an earned burst state, not a passive baseline.")]
    [Range(0f, 0.9f)] public float gearCdrCap = 0.40f;
    [Tooltip("Cap on TOTAL CDR including the temporary Overdrive window.")]
    [Range(0f, 0.9f)] public float overdriveCdrCap = 0.60f;

    // ── Healing / overheal spill ──────────────────────────────────────────────
    [Header("Healing")]
    [Tooltip("Fraction of overheal (healing beyond max HP) converted to shield absorb. " +
             "Makes HealMultiplier never wasted on a full-HP ally.")]
    [Range(0f, 1f)] public float overhealToShieldRate = 0.25f;
    [Tooltip("Max shield absorb an overheal spill can contribute (matches Sacred Aegis ceiling).")]
    public float overhealShieldCap = 80f;

    // ── Crowd Control ─────────────────────────────────────────────────────────
    [Header("Crowd Control — Stun")]
    public float stunBaseDuration = 1.5f;
    [Tooltip("Each repeat Stun on the same target within the window applies at this " +
             "fraction of the previous duration (diminishing returns).")]
    [Range(0f, 1f)] public float stunRepeatFalloff = 0.5f;
    [Tooltip("Repeat-Stun tracking window (seconds).")]
    public float stunFalloffWindow = 8f;
    [Tooltip("After this many stuns in the window, target becomes Stun-immune.")]
    public int stunImmunityThreshold = 3;
    [Tooltip("Duration of the Stun immunity granted after the threshold (seconds).")]
    public float stunImmunityDuration = 4f;

    [Header("Crowd Control — Gear Channels")]
    [Tooltip("Cap on Tenacity (reduces duration of CC applied TO you).")]
    [Range(0f, 0.95f)] public float tenacityCap = 0.60f;
    [Tooltip("Cap on ControlPower (extends duration of CC YOU apply).")]
    [Range(0f, 2f)] public float controlPowerCap = 0.50f;

    /// <summary>
    /// Authoritative CC duration formula. Call server-side from StatusEffectManager
    /// when applying Stun / Slow / Silenced / Bound (NOT DoTs like Cursed, NOT the
    /// Weakened amplifier). tenacity/controlPower are already clamped by CharacterStats.
    /// </summary>
    public static float CCDuration(float baseDuration, float targetTenacity, float casterControlPower)
    {
        return baseDuration * (1f - targetTenacity) * (1f + casterControlPower);
    }

    /// <summary>Applies the final total-DR clamp. Call at the end of TakeDamage's DR math.</summary>
    public float ClampTotalDR(float computedTotalDR)
    {
        return Mathf.Min(computedTotalDR, totalDrClamp);
    }

    // ── Wave scaling (prerequisite: gives healing a job) ──────────────────────
    [Header("Wave Scaling")]
    [Tooltip("HP multiplier added per wave. 0.08 = +8% per wave.")]
    public float hpScalePerWave = 0.08f;
    [Tooltip("Damage multiplier added per wave. 0.05 = +5% per wave.")]
    public float damageScalePerWave = 0.05f;
    [Tooltip("Max enemies that can simultaneously target one player. 0 = unlimited.")]
    public int globalAggroCapPerPlayer = 3;

    // ── Attunement / enchant shared tuning ────────────────────────────────────
    // Per-attunement magnitudes live on AttunementDef ScriptableObjects (one asset
    // per enchant, so each is Inspector-editable). Values shared across all
    // attunements live here.
    [Header("Attunements (Enchants)")]
    [Tooltip("Max attunement sockets per gear piece (Phase 1 = 1).")]
    public int maxAttunementSlotsPerItem = 1;
}
