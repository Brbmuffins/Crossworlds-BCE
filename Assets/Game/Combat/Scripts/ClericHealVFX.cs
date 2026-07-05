using Mirror;
using UnityEngine;

/// <summary>
/// ClericHealVFX — networked particle burst played when the Cleric casts a heal.
///
/// Class is NOT wrapped in #if !UNITY_SERVER so the NetworkBehaviour compiles on
/// the server. Particle fields and the visual method are guarded individually.
///
/// Wire-up:
///   1. Attach to Cleric prefab root (alongside AbilityCaster).
///   2. Assign healBurst + healAura particle systems in Inspector (optional — works
///      without them, just no visual).
///   3. Drag this component into AbilityCaster.healVFX in the Inspector.
///
/// AbilityCaster calls TriggerHealVFX() on the server; the [ClientRpc] broadcasts
/// the particle burst to all clients.
/// </summary>
public class ClericHealVFX : NetworkBehaviour
{
#if !UNITY_SERVER
    [Header("Particles")]
    [Tooltip("Short burst — plays once at cast origin (e.g. a gold radial burst)")]
    public ParticleSystem healBurst;

    [Tooltip("Soft aura loop — played briefly then stopped (e.g. a gentle ambient glow)")]
    public ParticleSystem healAura;

    [Header("Timing")]
    [Tooltip("How long the aura plays before being stopped (seconds)")]
    public float auraDuration = 1.2f;
#endif

    // ── Server entry point ─────────────────────────────────────────────────────

    /// <summary>
    /// Call this from the server (e.g. AbilityCaster) to broadcast the heal VFX
    /// to all connected clients.
    /// </summary>
    [Server]
    public void TriggerHealVFX()
    {
        RpcPlayHealVFX();
    }

    // ── Client visual ──────────────────────────────────────────────────────────

    [ClientRpc]
    void RpcPlayHealVFX()
    {
#if !UNITY_SERVER
        PlayVisual();
#endif
    }

#if !UNITY_SERVER
    void PlayVisual()
    {
        if (healBurst != null)
        {
            healBurst.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            healBurst.Play();
        }

        if (healAura != null)
        {
            healAura.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            healAura.Play();
            StartCoroutine(StopAuraAfterDelay());
        }
    }

    System.Collections.IEnumerator StopAuraAfterDelay()
    {
        yield return new WaitForSeconds(auraDuration);
        if (healAura != null) healAura.Stop();
    }
#endif
}
