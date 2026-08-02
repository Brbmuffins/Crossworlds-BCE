using System.Collections;
using System.Collections.Generic;
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
    public const float TravelFadeOutSeconds = 0.75f;

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
    private AudioClip _pendingTrack;
    private bool _trackTransitionPending;
    private readonly List<AudioClip> _playlist = new List<AudioClip>();
    private Coroutine _playlistRoutine;
    private bool _playlistActive;
    private float _playlistMinSilence;
    private float _playlistMaxSilence;
    private int _lastPlaylistIndex = -1;
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

    void Update()
    {
        if (!_playlistActive || _source == null || _source.isPlaying ||
            _fadeRoutine != null || _playlistRoutine != null)
            return;

        _playlistRoutine = StartCoroutine(PlayNextPlaylistTrackAfterDelay());
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
        _fadeRoutine = StartCoroutine(PlayWhenReadyRoutine(_source.clip, fadeSeconds, loopTrack));
    }

    private IEnumerator PlayWhenReadyRoutine(AudioClip clip, float fadeSeconds, bool loop)
    {
        EnsureGlobalAudioEnabled();
        yield return EnsureClipLoaded(clip);

        if (clip != null && clip.loadState == AudioDataLoadState.Failed)
            Debug.LogWarning($"[MusicController] Could not load music clip '{clip.name}'. Check the audio import settings.");

        if (_source == null || clip == null || _source.clip != clip || clip.loadState == AudioDataLoadState.Failed)
        {
            _fadeRoutine = null;
            yield break;
        }

        _source.loop = loop;

        if (fadeSeconds <= 0f)
        {
            ApplyVolumeImmediate();
            _source.Play();
            _fadeRoutine = null;
            yield break;
        }

        _source.volume = 0f;
        _source.Play();
        yield return FadeVolumeRoutine(0f, EffectiveVolume(), fadeSeconds);
        _fadeRoutine = null;
    }

    public void Stop()
    {
        Stop(fadeOutSeconds);
    }

    public void Stop(float fadeSeconds)
    {
        StopPlaylist();

        if (_source == null || !_source.isPlaying)
        {
            // A track may still be loading or waiting to begin. Cancel that work
            // so travel cannot start music from the zone we just left.
            StopFade();
            if (_source != null)
            {
                _source.Stop();
                ApplyVolumeImmediate();
            }
            return;
        }

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
        StopPlaylist();

        if (_source == null)
            return;

        // Additive zone activation can request the same destination track several
        // times while cameras, lighting and old scenes settle. Do not cancel and
        // restart an identical fade before it reaches the clip-swap point.
        if (_trackTransitionPending && _pendingTrack == nextTrack && !restartIfSameTrack)
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
        _pendingTrack = nextTrack;
        _trackTransitionPending = true;
        _fadeRoutine = StartCoroutine(FadeToTrackRoutine(nextTrack, Mathf.Max(0f, fadeSeconds)));
    }

    /// <summary>
    /// Starts an ambient playlist. Tracks play once, followed by a randomized
    /// period of silence. Re-selecting the active playlist does not restart it.
    /// </summary>
    public void SetPlaylist(IList<AudioClip> tracks, float minSilenceSeconds,
        float maxSilenceSeconds, float fadeSeconds, bool restartIfAlreadyPlaying = false)
    {
        if (_source == null)
            return;

        var validTracks = new List<AudioClip>();
        if (tracks != null)
        {
            for (int i = 0; i < tracks.Count; i++)
                if (tracks[i] != null && !validTracks.Contains(tracks[i]))
                    validTracks.Add(tracks[i]);
        }

        if (validTracks.Count == 0)
        {
            Stop(fadeSeconds);
            return;
        }

        bool samePlaylist = _playlistActive && PlaylistsMatch(validTracks);
        _playlistMinSilence = Mathf.Max(0f, minSilenceSeconds);
        _playlistMaxSilence = Mathf.Max(_playlistMinSilence, maxSilenceSeconds);

        if (samePlaylist && !restartIfAlreadyPlaying)
            return;

        StopPlaylist();
        _playlist.AddRange(validTracks);
        _playlistActive = true;
        _lastPlaylistIndex = -1;

        AudioClip firstTrack = ChooseNextPlaylistTrack();
        StartPlaylistTrack(firstTrack, fadeSeconds);
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

    private IEnumerator PlayNextPlaylistTrackAfterDelay()
    {
        float delay = Random.Range(_playlistMinSilence, _playlistMaxSilence);
        if (delay > 0f)
            yield return new WaitForSecondsRealtime(delay);

        _playlistRoutine = null;
        if (!_playlistActive || _source == null || _source.isPlaying)
            yield break;

        StartPlaylistTrack(ChooseNextPlaylistTrack(), fadeInSeconds);
    }

    private void StartPlaylistTrack(AudioClip track, float fadeSeconds)
    {
        if (track == null)
            return;

        StopFade();
        _source.Stop();
        _source.clip = track;
        _source.loop = false;
        _fadeRoutine = StartCoroutine(PlayWhenReadyRoutine(track, Mathf.Max(0f, fadeSeconds), false));
    }

    private AudioClip ChooseNextPlaylistTrack()
    {
        if (_playlist.Count == 0)
            return null;

        int index = Random.Range(0, _playlist.Count);
        if (_playlist.Count > 1 && index == _lastPlaylistIndex)
            index = (index + Random.Range(1, _playlist.Count)) % _playlist.Count;

        _lastPlaylistIndex = index;
        return _playlist[index];
    }

    private bool PlaylistsMatch(IList<AudioClip> tracks)
    {
        if (_playlist.Count != tracks.Count)
            return false;

        for (int i = 0; i < tracks.Count; i++)
            if (!_playlist.Contains(tracks[i]))
                return false;

        return true;
    }

    private void StopPlaylist()
    {
        _playlistActive = false;
        _playlist.Clear();
        _lastPlaylistIndex = -1;

        if (_playlistRoutine != null)
        {
            StopCoroutine(_playlistRoutine);
            _playlistRoutine = null;
        }
    }

    private void LoadPreferences()
    {
        _volume = defaultVolume;
        _muted = startMuted;

        if (!loadSavedPreferences)
            return;

        _volume = Mathf.Clamp01(PlayerPrefs.GetFloat(VolumePrefKey, defaultVolume));
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

    private void EnsureGlobalAudioEnabled()
    {
        AudioListener.pause = false;
        if (AudioListener.volume <= 0.001f)
            AudioListener.volume = 1f;
    }

    private IEnumerator EnsureClipLoaded(AudioClip clip)
    {
        if (clip == null)
            yield break;

        if (clip.loadState == AudioDataLoadState.Unloaded)
            clip.LoadAudioData();

        while (clip.loadState == AudioDataLoadState.Loading)
            yield return null;
    }

    private void StopFade()
    {
        if (_fadeRoutine != null)
        {
            StopCoroutine(_fadeRoutine);
            _fadeRoutine = null;
        }

        _pendingTrack = null;
        _trackTransitionPending = false;
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
            yield return EnsureClipLoaded(nextTrack);
            if (nextTrack.loadState == AudioDataLoadState.Failed)
            {
                Debug.LogWarning($"[MusicController] Could not load music clip '{nextTrack.name}'. Check the audio import settings.");
                ApplyVolumeImmediate();
                _pendingTrack = null;
                _trackTransitionPending = false;
                _fadeRoutine = null;
                yield break;
            }

            _source.volume = 0f;
            _source.Play();
            yield return FadeVolumeRoutine(0f, EffectiveVolume(), fadeInSeconds);
        }
        else
        {
            ApplyVolumeImmediate();
        }

        _pendingTrack = null;
        _trackTransitionPending = false;
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
