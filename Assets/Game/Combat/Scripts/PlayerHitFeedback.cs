using Mirror;
using UnityEngine;

/// <summary>
/// PlayerHitFeedback — local player receive-hit feedback.
/// Attach to the player prefab alongside Health.
///
/// Fires on the LOCAL client only (isLocalPlayer guard) so remote players
/// don't shake the wrong camera. Hooks into Health.onDamageTaken which fires
/// after server-side damage is applied and the SyncVar updates the client.
///
/// Layers delivered:
///   3 — Sound (incoming damage SFX)  [TODO: assign playerHurtSFX]
///   4 — Damage number in screen-space (red flash alternative — see below)
///   5 — Screen shake (trauma proportional to damage fraction)
/// </summary>
[RequireComponent(typeof(Health))]
public class PlayerHitFeedback : NetworkBehaviour
{
    [Header("SFX")]
    public AudioClip playerHurtSFX;       // assign: short pain grunt / impact
    public AudioClip playerShieldHitSFX;  // assign: metallic clank

    Health _health;

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (!isLocalPlayer) return;   // only the owning client registers feedback

        _health = GetComponent<Health>();
        if (_health == null) return;

        _health.onDamageTaken.AddListener(OnDamageTaken);
        _health.onDownedChanged.AddListener(OnDownedChanged);
    }

    void OnDestroy()
    {
        if (_health == null) return;
        _health.onDamageTaken.RemoveListener(OnDamageTaken);
        _health.onDownedChanged.RemoveListener(OnDownedChanged);
    }

    // ── Feedback handlers ─────────────────────────────────────────────────────

    void OnDamageTaken(float amount)
    {
        if (_health == null) return;

#if UNITY_EDITOR || !UNITY_SERVER
        // Trauma proportional to damage fraction (20 dmg on 100 HP = 0.2 trauma)
        float fraction = _health.maxHealth > 0f ? amount / _health.maxHealth : 0f;
        float trauma   = Mathf.Lerp(0.10f, 0.50f, Mathf.Clamp01(fraction * 3f));
        ScreenShake.AddTrauma(trauma);

        // Sound
        if (playerHurtSFX != null)
            AudioSource.PlayClipAtPoint(playerHurtSFX,
                Camera.main?.transform.position ?? Vector3.zero,
                SfxVolumeSettings.Scale(0.6f));

        // Hitstop on heavy hits (>20% HP in one hit)
        if (fraction >= 0.20f)
            HitstopManager.Freeze(HitstopManager.Weight.Medium);
#endif
    }

    void OnDownedChanged(bool downed)
    {
        if (!downed) return;
#if UNITY_EDITOR || !UNITY_SERVER
        // Player just went down — large shake
        ScreenShake.PlayerDowned();
        HitstopManager.Freeze(HitstopManager.Weight.KillBlow);
#endif
    }
}
