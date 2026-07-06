#if UNITY_EDITOR || !UNITY_SERVER
using UnityEngine;

/// <summary>
/// CombatAudio — Singleton 2D audio manager for combat SFX.
/// Attach to any persistent GameObject in the arena scene (e.g. ArenaManager or WaveSpawner).
///
/// Assign clips from Assets/Retro Sci-Fi Pack/Uncompressed/ in the Inspector:
///   meleeHit    → "Medium swell 02.wav"
///   rangedHit   → "Notification 01.wav"
///   deathSFX    → "Deep swell 01.wav"
///   shieldSFX   → "Shield energy [loop] 01.wav"  (short burst — not looped)
///   waveAlert   → "Notification 02.wav"
///   abilityCast → "Medium swell 01.wav"
///   healSFX     → "Notification 03 slow.wav"
///
/// Usage (from any client script):
///   CombatAudio.Instance?.PlayMeleeHit();
///   CombatAudio.Instance?.PlayAt(clip, worldPos);
/// </summary>
public class CombatAudio : MonoBehaviour
{
    public static CombatAudio Instance { get; private set; }

    [Header("Combat SFX — assign from Retro Sci-Fi Pack")]
    public AudioClip meleeHit;
    public AudioClip rangedHit;
    public AudioClip deathSFX;
    public AudioClip shieldSFX;
    public AudioClip waveAlert;
    public AudioClip abilityCast;
    public AudioClip healSFX;

    [Header("Volume")]
    [Range(0f, 1f)] public float masterVolume = 0.75f;

    private AudioSource _src;

    // ─────────────────────────────────────────────────────────────────────────────

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        _src                = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
        _src.playOnAwake    = false;
        _src.spatialBlend   = 0f;   // 2D — HUD-level audio, not positional
        _src.loop           = false;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    // ── Convenience wrappers ──────────────────────────────────────────────────────

    public void PlayMeleeHit()    => Play(meleeHit);
    public void PlayRangedHit()   => Play(rangedHit);
    public void PlayDeath()       => Play(deathSFX);
    public void PlayShield()      => Play(shieldSFX);
    public void PlayWaveAlert()   => Play(waveAlert);
    public void PlayAbilityCast() => Play(abilityCast);
    public void PlayHeal()        => Play(healSFX);

    /// <summary>Play a clip at a world position (3D spatialised one-shot).</summary>
    public void PlayAt(AudioClip clip, Vector3 position, float volumeScale = 1f)
    {
        if (clip == null) return;
        AudioSource.PlayClipAtPoint(clip, position, masterVolume * volumeScale);
    }

    // ── Internal ──────────────────────────────────────────────────────────────────

    void Play(AudioClip clip)
    {
        if (clip == null || _src == null) return;
        // Pitch randomization prevents repetitive sound — ±pitchVariance from FeelConfig
        float variance   = FeelConfig.Instance != null ? FeelConfig.Instance.pitchVariance : 0.07f;
        _src.pitch       = 1f + Random.Range(-variance, variance);
        _src.PlayOneShot(clip, masterVolume);
    }
}
#endif
