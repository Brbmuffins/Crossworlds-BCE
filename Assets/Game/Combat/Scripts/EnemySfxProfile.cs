using UnityEngine;

/// <summary>Prefab-specific positional SFX authored by Enemy Forge.</summary>
public sealed class EnemySfxProfile : MonoBehaviour
{
    [Header("Events")]
    public AudioClip aggro;
    public AudioClip attack1;
    public AudioClip attack2;
    public AudioClip attack3;
    public AudioClip attack4;
    public AudioClip attackImpact;
    public AudioClip getHit;
    public AudioClip death;

    [Header("Playback")]
    [Range(0f, 1f)] public float volume = 0.8f;
    [Range(0f, 0.35f)] public float pitchVariation = 0.05f;
    [Min(0f)] public float minDistance = 2f;
    [Min(0.1f)] public float maxDistance = 25f;

    public bool PlayAggro() => Play(aggro);
    public bool PlayAttack(int variant)
    {
        AudioClip clip = variant switch
        {
            1 => attack2,
            2 => attack3,
            3 => attack4,
            _ => attack1
        };
        return Play(clip);
    }
    public bool PlayImpact() => Play(attackImpact);
    public bool PlayGetHit() => Play(getHit);
    public bool PlayDeath() => Play(death);

    bool Play(AudioClip clip)
    {
#if UNITY_SERVER && !UNITY_EDITOR
        return clip != null;
#else
        if (clip == null) return false;
        var oneShot = new GameObject($"SFX_{clip.name}");
        oneShot.transform.position = transform.position;
        var source = oneShot.AddComponent<AudioSource>();
        source.clip = clip;
        source.volume = SfxVolumeSettings.Scale(volume);
        source.pitch = 1f + Random.Range(-pitchVariation, pitchVariation);
        source.spatialBlend = 1f;
        source.rolloffMode = AudioRolloffMode.Logarithmic;
        source.minDistance = Mathf.Max(0f, minDistance);
        source.maxDistance = Mathf.Max(source.minDistance + 0.1f, maxDistance);
        source.playOnAwake = false;
        source.Play();
        Destroy(oneShot, clip.length / Mathf.Max(0.01f, Mathf.Abs(source.pitch)) + 0.1f);
        return true;
#endif
    }
}
