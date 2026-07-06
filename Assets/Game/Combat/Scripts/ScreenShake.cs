#if UNITY_EDITOR || !UNITY_SERVER
using UnityEngine;

/// <summary>
/// ScreenShake — trauma-based camera shake.
/// Attach to the main camera GameObject.
///
/// Trauma accumulates additively, but shake magnitude is trauma², which means
/// small traumas produce little shake and only large events feel violent.
/// Trauma decays linearly; multiple small hits don't stack into a huge shake.
///
/// Usage:
///   ScreenShake.AddTrauma(0.3f);   // light (player hit)
///   ScreenShake.AddTrauma(0.6f);   // medium (elite hit, player downed)
///   ScreenShake.AddTrauma(1.0f);   // max (boss slam, arena complete)
///
/// All tunable values exposed in FeelConfig if present.
/// </summary>
public class ScreenShake : MonoBehaviour
{
    // ── Singleton ─────────────────────────────────────────────────────────────
    public static ScreenShake Instance { get; private set; }

    // ── Inspector / FeelConfig defaults ──────────────────────────────────────
    [Header("Shake Settings")]
    [Tooltip("Trauma decay per second (1.0 = full trauma gone in 1s).")]
    public float traumaDecay    = 1.2f;
    [Tooltip("Max positional offset at trauma=1 (world units).")]
    public float maxOffset      = 0.18f;
    [Tooltip("Max rotational offset at trauma=1 (degrees).")]
    public float maxRoll        = 3.5f;
    [Tooltip("Perlin seed speed — higher = more chaotic.")]
    public float shakeFrequency = 18f;
    [Tooltip("Trauma cap — AddTrauma will not push above this.")]
    [Range(0f, 1f)]
    public float maxTrauma      = 1f;

    // ── Runtime state ─────────────────────────────────────────────────────────
    float   _trauma;
    float   _seed;
    Vector3 _originLocalPos;
    float   _originLocalRollZ;

    // ── Lifecycle ─────────────────────────────────────────────────────────────
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
        _originLocalPos   = transform.localPosition;
        _originLocalRollZ = transform.localEulerAngles.z;
        _seed             = Random.value * 100f;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    void LateUpdate()
    {
        if (_trauma <= 0f) return;

        _trauma = Mathf.Max(0f, _trauma - traumaDecay * Time.deltaTime);

        float shake = _trauma * _trauma;   // trauma² curve

        // Override with FeelConfig if available
        float mo = FeelConfig.Instance != null ? FeelConfig.Instance.ShakeMaxOffset    : maxOffset;
        float mr = FeelConfig.Instance != null ? FeelConfig.Instance.ShakeMaxRoll      : maxRoll;
        float sf = FeelConfig.Instance != null ? FeelConfig.Instance.ShakeFrequency    : shakeFrequency;

        float t = Time.unscaledTime * sf;   // unscaled so hitstop doesn't freeze the shake curve
        float ox = (Mathf.PerlinNoise(_seed + 0f,  t) * 2f - 1f) * mo * shake;
        float oy = (Mathf.PerlinNoise(_seed + 10f, t) * 2f - 1f) * mo * shake * 0.6f;
        float rz = (Mathf.PerlinNoise(_seed + 20f, t) * 2f - 1f) * mr * shake;

        transform.localPosition    = _originLocalPos + new Vector3(ox, oy, 0f);
        var euler                  = transform.localEulerAngles;
        euler.z                    = _originLocalRollZ + rz;
        transform.localEulerAngles = euler;
    }

    // ── Static API ────────────────────────────────────────────────────────────

    /// <summary>
    /// Add trauma [0–1]. Clamped to maxTrauma.
    /// Small values produce almost no shake; 1.0 is maximum chaos.
    /// </summary>
    public static void AddTrauma(float amount)
    {
        if (Instance == null) return;
        float cap = FeelConfig.Instance != null ? FeelConfig.Instance.MaxTrauma : Instance.maxTrauma;
        Instance._trauma = Mathf.Min(Instance._trauma + amount, cap);
    }

    /// <summary>Preset helpers so call sites are self-documenting.</summary>
    public static void PlayerHit()     => AddTrauma(0.25f);
    public static void PlayerDowned()  => AddTrauma(0.70f);
    public static void EliteHit()      => AddTrauma(0.40f);
    public static void BossSlam()      => AddTrauma(0.90f);
    public static void ArenaClear()    => AddTrauma(0.55f);
}
#endif // UNITY_EDITOR || !UNITY_SERVER
