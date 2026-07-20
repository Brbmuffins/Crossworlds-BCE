using UnityEngine;

/// <summary>
/// DynamicDifficultyScaler — data-driven, per-wave + co-op difficulty layer.
///
/// Turns the "chaos curve" (more identical weak enemies each wave) into a
/// "tension curve" (enemies also get tougher) and bounds concurrency so a
/// 4-player / 20+ enemy late wave stays readable and perf-safe. See
/// Docs/combat/aiming_cone_combat_feel_2026-07-19.md §6 and §4.
///
/// This is a MULTIPLICATIVE overlay: it never replaces the zone's base
/// tuning. WaveSpawner starts from the original ZoneConfig combat-root
/// multipliers and folds this scaler in on top, so a zone's authored
/// difficulty is preserved and the dynamic curve rides on it.
///
/// Server-authoritative: only WaveSpawner (a [Server] context) reads this.
/// Create via: Assets > Create > BCE > Dynamic Difficulty Scaler
/// </summary>
[CreateAssetMenu(menuName = "BCE/Dynamic Difficulty Scaler", fileName = "DynamicScaler_New")]
public class DynamicDifficultyScaler : ScriptableObject
{
    [Header("Per-Wave Escalation (tension curve, not chaos curve)")]
    [Tooltip("Extra enemy HP per wave beyond wave 1, as a fraction. 0.08 = +8% HP each wave.")]
    [Min(0f)] public float healthPerWave = 0.08f;
    [Tooltip("Extra enemy damage per wave beyond wave 1, as a fraction. 0.05 = +5% dmg each wave.")]
    [Min(0f)] public float damagePerWave = 0.05f;
    [Tooltip("Hard cap on the per-wave component so late waves don't become unkillable.")]
    [Min(1f)] public float maxWaveMultiplier = 4f;

    [Header("Co-op Player Scaling")]
    [Tooltip("Extra enemy HP per additional player beyond the first. 0.5 = +50% HP for each extra player.")]
    [Min(0f)] public float healthPerExtraPlayer = 0.5f;
    [Tooltip("Extra enemy damage per additional player beyond the first.")]
    [Min(0f)] public float damagePerExtraPlayer = 0.15f;

    [Header("Concurrency / Aggro Cap")]
    [Tooltip("Max enemies allowed alive at once (perf + readability). 0 = uncapped. " +
             "WaveSpawner holds spawns while at the cap and drains the wave over time.")]
    [Min(0)] public int maxConcurrentAlive = 24;
    [Tooltip("Reserved: intended cap on enemies actively chasing a single player " +
             "(readability). 0 = uncapped. Not yet enforced — the enforcement point is " +
             "EnemyController.TickIdle target acquisition (see note there).")]
    [Min(0)] public int maxAggroPerPlayer = 6;

    /// <summary>Resolved multipliers for a given wave + live player count.</summary>
    public struct Scaling
    {
        public float healthMult;
        public float damageMult;
    }

    /// <summary>
    /// Compute the dynamic overlay multipliers. wave is 1-based; playerCount is
    /// the number of live players (clamped to >= 1). Returns 1.0x at wave 1 solo.
    /// </summary>
    public Scaling GetScaling(int wave, int playerCount)
    {
        int wavesPast = Mathf.Max(1, wave) - 1;
        int extra     = Mathf.Max(0, playerCount - 1);

        float hp  = 1f + healthPerWave * wavesPast + healthPerExtraPlayer * extra;
        float dmg = 1f + damagePerWave * wavesPast + damagePerExtraPlayer * extra;

        if (maxWaveMultiplier > 1f)
        {
            // Cap only the wave component; co-op scaling is intentionally uncapped
            // so a full party always feels the extra bodies.
            float hpWave  = 1f + healthPerWave * wavesPast;
            float dmgWave = 1f + damagePerWave * wavesPast;
            hp  -= Mathf.Max(0f, hpWave  - maxWaveMultiplier);
            dmg -= Mathf.Max(0f, dmgWave - maxWaveMultiplier);
        }

        return new Scaling { healthMult = Mathf.Max(0.1f, hp), damageMult = Mathf.Max(0.1f, dmg) };
    }
}
