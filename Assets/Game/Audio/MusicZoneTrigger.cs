using Mirror;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Drop this on a trigger volume to change music when the local player enters a zone.
/// The current track keeps playing until a zone trigger requests a different one.
/// </summary>
[RequireComponent(typeof(Collider))]
[AddComponentMenu("BCE/Audio/Music Zone Trigger")]
public class MusicZoneTrigger : MonoBehaviour
{
    [Header("Music")]
    [SerializeField] private AudioClip zoneTrack;
    [Tooltip("Additional tracks for this zone. The original zone track is always included first.")]
    [SerializeField] private List<AudioClip> additionalTracks = new List<AudioClip>();
    [SerializeField] private bool stopMusicInstead = false;
    [SerializeField] private bool restartIfAlreadyPlaying = false;

    [Header("Ambient Timing")]
    [Min(0f)]
    [SerializeField] private float minSilenceSeconds = 30f;
    [Min(0f)]
    [SerializeField] private float maxSilenceSeconds = 120f;

    [Header("Fade")]
    [Min(0f)]
    [SerializeField] private float fadeSeconds = 1.5f;

    [Header("Rules")]
    [SerializeField] private bool localPlayerOnly = true;
    [SerializeField] private bool triggerOnce = false;
    [SerializeField] private float repeatDelay = 1f;

    private bool _triggered;
    private float _lastTriggerTime = -999f;

    void Reset()
    {
        EnsureTrigger();
    }

    void OnValidate()
    {
        EnsureTrigger();
    }

    void OnTriggerEnter(Collider other)
    {
        if (localPlayerOnly && !IsLocalPlayer(other))
            return;

        // Additive travel can leave the old zone loaded briefly. Never let its
        // trigger select music for a player who has already moved to another scene.
        if (!ZoneCameraDirector.IsCurrentLocalZone(gameObject.scene))
            return;

        ApplyTrack(respectTriggerTiming: true);
    }

    /// <summary>
    /// Called by ZoneCameraDirector after the local player's destination scene is
    /// confirmed. Additive scene travel does not reliably produce trigger-entry
    /// callbacks, so zone music must not depend on physics alone.
    /// </summary>
    public void ActivateForLocalZone()
    {
        ApplyTrack(respectTriggerTiming: false);
    }

    void ApplyTrack(bool respectTriggerTiming)
    {
        if (respectTriggerTiming && triggerOnce && _triggered)
            return;

        if (respectTriggerTiming && Time.unscaledTime - _lastTriggerTime < repeatDelay)
            return;

        var controller = MusicController.Instance;
        if (controller == null)
        {
            Debug.LogWarning($"[MusicZoneTrigger] '{name}' fired, but no MusicController exists in the scene.");
            return;
        }

        // Avoid re-triggering only when the correct track is genuinely playing.
        // A stopped AudioSource still retains its clip, so comparing CurrentTrack
        // alone prevented Hub music from restarting after another zone stopped it.
        if (!stopMusicInstead && controller.CurrentTrack == zoneTrack &&
            controller.IsPlaying && !restartIfAlreadyPlaying)
            return;

        if (!stopMusicInstead && zoneTrack == null)
        {
            Debug.LogWarning($"[MusicZoneTrigger] '{name}' has no zoneTrack assigned.");
            return;
        }

        _triggered = true;
        _lastTriggerTime = Time.unscaledTime;

        if (stopMusicInstead)
        {
            controller.Stop(fadeSeconds);
            return;
        }

        var playlist = new List<AudioClip>();
        if (zoneTrack != null)
            playlist.Add(zoneTrack);
        foreach (AudioClip track in additionalTracks)
            if (track != null && !playlist.Contains(track))
                playlist.Add(track);

        controller.SetPlaylist(playlist, minSilenceSeconds, maxSilenceSeconds,
            fadeSeconds, restartIfAlreadyPlaying);
    }

    void OnTriggerStay(Collider other)
    {
        OnTriggerEnter(other);
    }

    private bool IsLocalPlayer(Collider other)
    {
        var identity = other.GetComponentInParent<NetworkIdentity>();
        if (identity != null)
            return identity.isLocalPlayer;

        return other.CompareTag("Player") || other.GetComponentInParent<PlayerMovement>() != null;
    }

    private void EnsureTrigger()
    {
        var col = GetComponent<Collider>();
        if (col != null)
            col.isTrigger = true;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        var col = GetComponent<Collider>();
        if (col == null)
            return;

        Gizmos.color = new Color(1f, 0.55f, 0.2f, 0.18f);
        Gizmos.matrix = transform.localToWorldMatrix;

        if (col is BoxCollider box)
        {
            Gizmos.DrawCube(box.center, box.size);
            Gizmos.color = new Color(1f, 0.55f, 0.2f, 0.85f);
            Gizmos.DrawWireCube(box.center, box.size);
        }
        else
        {
            Gizmos.matrix = Matrix4x4.identity;
            Gizmos.DrawCube(col.bounds.center, col.bounds.size);
            Gizmos.color = new Color(1f, 0.55f, 0.2f, 0.85f);
            Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
        }

        Gizmos.matrix = Matrix4x4.identity;
    }
#endif
}
