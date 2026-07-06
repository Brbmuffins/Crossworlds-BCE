using UnityEngine;

/// <summary>
/// FeelConfig — single ScriptableObject holding all game-feel tuning values.
/// Create via: Assets → Create → Crossworlds → FeelConfig
///
/// Referenced by HitstopManager, ScreenShake, CombatAudio for overrides.
/// If no instance is loaded, each system falls back to its own Inspector defaults.
/// </summary>
[CreateAssetMenu(fileName = "FeelConfig", menuName = "Crossworlds/FeelConfig")]
public class FeelConfig : ScriptableObject
{
    // ── Singleton (loaded from Resources/FeelConfig) ──────────────────────────
    static FeelConfig _instance;
    public static FeelConfig Instance
    {
        get
        {
            if (_instance == null)
                _instance = Resources.Load<FeelConfig>("FeelConfig");
            return _instance;
        }
    }

    // ── Hitstop ───────────────────────────────────────────────────────────────
    [Header("Hitstop (seconds)")]
    public float hitstopLight    = 0.030f;
    public float hitstopMedium   = 0.050f;
    public float hitstopHeavy    = 0.080f;
    public float hitstopKillBlow = 0.110f;

    public float HitstopDuration(HitstopManager.Weight w)
    {
        return w switch
        {
            HitstopManager.Weight.Light    => hitstopLight,
            HitstopManager.Weight.Medium   => hitstopMedium,
            HitstopManager.Weight.Heavy    => hitstopHeavy,
            HitstopManager.Weight.KillBlow => hitstopKillBlow,
            _                              => hitstopMedium,
        };
    }

    // ── Screen shake ──────────────────────────────────────────────────────────
    [Header("Screen Shake")]
    public float ShakeMaxOffset   = 0.18f;
    public float ShakeMaxRoll     = 3.5f;
    public float ShakeFrequency   = 18f;
    public float MaxTrauma        = 1.0f;

    // ── Audio ─────────────────────────────────────────────────────────────────
    [Header("Audio Pitch Randomization")]
    [Tooltip("±pitch variance on melee/ranged hit sounds (0.07 = ±7%)")]
    public float pitchVariance    = 0.07f;

    // ── Floating damage text ──────────────────────────────────────────────────
    [Header("Damage Numbers")]
    public float dmgTextLifetime  = 1.0f;
    public float dmgTextRiseSpeed = 1.6f;
    public float critTextLifetime = 1.4f;
    public float critTextRiseSpeed = 2.5f;
}
