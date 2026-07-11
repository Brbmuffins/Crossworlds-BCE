using System.Collections;
using UnityEngine;

/// <summary>
/// Scene music controller with saved volume/mute preferences.
/// Attach this to a scene GameObject and assign a starting track in the Inspector.
/// Future settings UI can call Instance.SetVolume(...), Instance.SetMuted(...), or Instance.SetTrack(...).
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(AudioSource))]
[AddComponentMenu("BCE/Audio/Music Controller")]
public class MusicController : MonoBehaviour
{
    public const string VolumePrefKey = "music_volume";
    public const string MutedPrefKey = "music_muted";

    public static MusicController Instance { get; private set; }

    [Header("Track")]
    [SerializeField] private AudioClip startingTrack;
    [SerializeField] private bool playOnStart = true;
    [SerializeField] private bool loopTrack = true;

    [Header("Volume")]
    [Range(0f, 1f)]
    [SerializeField] private float defaultVolume = 0.6f;
    [SerializeField] private bool startMuted = false;

    [Header("Fades")]
    [Min(0f)]
    [SerializeField] private float fadeInSeconds = 1.5f;
    [Min(0f)]
    [SerializeField] private float fadeOutSeconds = 0.5f;

    [Header("Settings")]
    [SerializeField] private bool loadSavedPreferences = true;
    [SerializeField] private bool savePreferenceChanges = true;
    [SerializeField] private bool persistAcrossScenes = false;

    private AudioSource _source;
    private Coroutine _fadeRoutine;
    private float _volume;
    private bool _muted;

    public float Volume => _volume;
    public bool Muted => _muted;
    public AudioClip CurrentTrack => _source != null ? _source.clip : null;
    public bool IsPlaying => _source != null && _source.isPlaying;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (persistAcrossScenes)
            DontDestroyOnLoad(gameObject);

        _source = GetComponent<AudioSource>();
        ConfigureSource();
        LoadPreferences();

        if (_source.clip == null)
            _source.clip = startingTrack;

        ApplyVolumeImmediate();
    }

    void Start()
    {
        if (playOnStart && _source.clip != null && !Application.isBatchMode)
            Play(fadeInSeconds);
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    void Reset()
    {
        _source = GetComponent<AudioSource>();
        ConfigureSource();
    }

    public void Play()
    {
        Play(fadeInSeconds);
    }

    public void Play(float fadeSeconds)
    {
        if (_source == null || _source.clip == null)
            return;

        StopFade();
        _source.loop = loopTrack;

        if (fadeSeconds <= 0f)
        {
            ApplyVolumeImmediate();
            _source.Play();
            return;
        }

        _source.volume = 0f;
        _source.Play();
        _fadeRoutine = StartCoroutine(FadeVolumeRoutine(0f, EffectiveVolume(), fadeSeconds));
    }

    public void Stop()
    {
        Stop(fadeOutSeconds);
    }

    public void Stop(float fadeSeconds)
    {
        if (_source == null || !_source.isPlaying)
            return;

        StopFade();

        if (fadeSeconds <= 0f)
        {
            _source.Stop();
            ApplyVolumeImmediate();
            return;
        }

        _fadeRoutine = StartCoroutine(StopAfterFadeRoutine(fadeSeconds));
    }

    public void Pause()
    {
        if (_source != null)
            _source.Pause();
    }

    public void Resume()
    {
        if (_source != null && _source.clip != null)
            _source.UnPause();
    }

    public void SetTrack(AudioClip nextTrack, bool playImmediately = true)
    {
        if (_source == null)
            return;

        if (_source.clip == nextTrack)
        {
            ApplyVolumeImmediate();
            if (playImmediately && !_source.isPlaying && nextTrack != null && !Application.isBatchMode)
                Play(fadeInSeconds);
            return;
        }

        StopFade();
        _source.Stop();
        _source.clip = nextTrack;
        _source.loop = loopTrack;
        ApplyVolumeImmediate();

        if (playImmediately && nextTrack != null && !Application.isBatchMode)
            Play(fadeInSeconds);
    }

    public void FadeToTrack(AudioClip nextTrack)
    {
        FadeToTrack(nextTrack, fadeOutSeconds);
    }

    public void FadeToTrack(AudioClip nextTrack, float fadeSeconds)
    {
        FadeToTrack(nextTrack, fadeSeconds, false);
    }

    public void FadeToTrack(AudioClip nextTrack, float fadeSeconds, bool restartIfSameTrack)
    {
        if (_source == null)
            return;

        if (_source.clip == nextTrack && !restartIfSameTrack)
        {
            if (!_source.isPlaying && nextTrack != null && !Application.isBatchMode)
                Play(fadeInSeconds);
            else
                ApplyVolumeImmediate();
            return;
        }

        StopFade();
        _fadeRoutine = StartCoroutine(FadeToTrackRoutine(nextTrack, Mathf.Max(0f, fadeSeconds)));
    }

    public void SetVolume(float value)
    {
        StopFade();
        _volume = Mathf.Clamp01(value);
        ApplyVolumeImmediate();

        if (savePreferenceChanges)
            PlayerPrefs.SetFloat(VolumePrefKey, _volume);
    }

    public void SetMuted(bool muted)
    {
        StopFade();
        _muted = muted;
        ApplyVolumeImmediate();

        if (savePreferenceChanges)
            PlayerPrefs.SetInt(MutedPrefKey, _muted ? 1 : 0);
    }

    public void ToggleMuted()
    {
        SetMuted(!_muted);
    }

    public void SavePreferences()
    {
        PlayerPrefs.SetFloat(VolumePrefKey, _volume);
        PlayerPrefs.SetInt(MutedPrefKey, _muted ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void ConfigureSource()
    {
        if (_source == null)
            return;

        _source.playOnAwake = false;
        _source.loop = loopTrack;
        _source.spatialBlend = 0f;
    }

    private void LoadPreferences()
    {
        _volume = defaultVolume;
        _muted = startMuted;

        if (!loadSavedPreferences)
            return;

        _volume = PlayerPrefs.GetFloat(VolumePrefKey, defaultVolume);
        _muted = PlayerPrefs.GetInt(MutedPrefKey, startMuted ? 1 : 0) == 1;
    }

    private void ApplyVolumeImmediate()
    {
        if (_source != null)
            _source.volume = EffectiveVolume();
    }

    private float EffectiveVolume()
    {
        return _muted ? 0f : Mathf.Clamp01(_volume);
    }

    private void StopFade()
    {
        if (_fadeRoutine == null)
            return;

        StopCoroutine(_fadeRoutine);
        _fadeRoutine = null;
    }

    private IEnumerator StopAfterFadeRoutine(float fadeSeconds)
    {
        float startVolume = _source.volume;
        yield return FadeVolumeRoutine(startVolume, 0f, fadeSeconds);
        _source.Stop();
        ApplyVolumeImmediate();
        _fadeRoutine = null;
    }

    private IEnumerator FadeToTrackRoutine(AudioClip nextTrack, float fadeSeconds)
    {
        if (_source.isPlaying && fadeSeconds > 0f)
            yield return FadeVolumeRoutine(_source.volume, 0f, fadeSeconds);

        _source.Stop();
        _source.clip = nextTrack;
        _source.loop = loopTrack;

        if (nextTrack != null && !Application.isBatchMode)
        {
            _source.volume = 0f;
            _source.Play();
            yield return FadeVolumeRoutine(0f, EffectiveVolume(), fadeInSeconds);
        }
        else
        {
            ApplyVolumeImmediate();
        }

        _fadeRoutine = null;
    }

    private IEnumerator FadeVolumeRoutine(float from, float to, float seconds)
    {
        if (seconds <= 0f)
        {
            if (_source != null)
                _source.volume = to;
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < seconds && _source != null)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / seconds);
            _source.volume = Mathf.Lerp(from, to, t);
            yield return null;
        }

        if (_source != null)
            _source.volume = to;
    }
}
