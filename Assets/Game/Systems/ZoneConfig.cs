using UnityEngine;

/// <summary>
/// ZoneConfig — Per-zone tuning asset.
/// Create via: Assets > Create > BCE > Zone Config
/// Assign to WaveSpawner.zoneConfig in the scene.
/// </summary>
[CreateAssetMenu(menuName = "BCE/Zone Config", fileName = "ZoneConfig_New")]
public class ZoneConfig : ScriptableObject
{
    [Header("Identity")]
    public string zoneName        = "Unknown Zone";
    public string sceneName       = "";   // must match Build Settings scene name exactly
    [Tooltip("Recommended player level to enter this zone.")]
    public int    recommendedLevel = 1;

    [Header("Difficulty")]
    [Tooltip("Scales all enemy damage output in this zone.")]
    [Range(0.5f, 5f)]
    public float damageMult        = 1f;
    [Tooltip("Scales all enemy max HP in this zone.")]
    [Range(0.5f, 5f)]
    public float healthMult        = 1f;

    [Header("Waves")]
    public int   baseEnemiesPerWave  = 4;
    public int   enemiesAddedPerWave = 2;
    public int   eliteEveryNWaves    = 3;
    [Tooltip("0 = infinite waves (persistent combat zone).")]
    public int   maxWaves            = 0;
    public float timeBetweenWaves    = 10f;
    public float introDelay          = 5f;

    [Header("Heavy Attack Override")]
    [Tooltip("Override min cooldown on EnemyHeavyAttack for this zone. 0 = use component default.")]
    [Min(0f)]
    public float heavyAttackMinCooldown = 0f;
    [Min(0f)]
    public float heavyAttackMaxCooldown = 0f;

    [Header("Loot")]
    [Tooltip("DB enemy_template IDs to use for kill rewards in this zone. " +
             "Matches enemyTemplateId on EnemyController.")]
    public string[] enemyTemplateIds = new[] { "grunt_basic" };

    [Header("Visual")]
    [Tooltip("Zone accent colour used in HUD zone name display.")]
    public Color accentColor = new Color(0.4f, 0.8f, 1f);

    // ── Convenience ──────────────────────────────────────────────────────────────

    /// <summary>Apply this config's wave settings to a WaveSpawner.</summary>
    public void ApplyTo(WaveSpawner ws)
    {
        if (ws == null) return;
        ws.baseEnemiesPerWave  = baseEnemiesPerWave;
        ws.enemiesAddedPerWave = enemiesAddedPerWave;
        ws.eliteEveryNWaves    = eliteEveryNWaves;
        ws.maxWaves            = maxWaves;
        ws.timeBetweenWaves    = timeBetweenWaves;
        ws.introDelay          = introDelay;
    }
}
