using UnityEngine;

/// <summary>Player-local master volume applied to gameplay sound effects.</summary>
public static class SfxVolumeSettings
{
    public const string VolumePrefKey = "sfx_volume";
    public const float DefaultVolume = 0.8f;

    static bool _loaded;
    static float _volume;

    public static float Volume
    {
        get
        {
            EnsureLoaded();
            return _volume;
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void LoadBeforeScene()
    {
        _loaded = false;
        EnsureLoaded();
    }

    public static void SetVolume(float value)
    {
        _volume = Mathf.Clamp01(value);
        _loaded = true;
        PlayerPrefs.SetFloat(VolumePrefKey, _volume);
        PlayerPrefs.Save();
    }

    public static float Scale(float authoredVolume)
    {
        return Mathf.Clamp01(authoredVolume) * Volume;
    }

    static void EnsureLoaded()
    {
        if (_loaded) return;
        _volume = Mathf.Clamp01(PlayerPrefs.GetFloat(VolumePrefKey, DefaultVolume));
        _loaded = true;
    }
}
