using System.Collections;
using Mirror;
using UnityEngine;

/// <summary>
/// ClericHealVFX — networked particle burst played when the Cleric casts a heal.
///
/// Wire-up (auto-done by BCE/Setup/4v — Healing VFX Builder):
///   healBurstPrefab  → brbmuffins Magic Pack / Hits and explosions / Holy hit.prefab
///   healAuraPrefab   → brbmuffins Magic Pack / Character auras / Healing.prefab
///
/// Manual setup:
///   1. Attach to Cleric prefab root (alongside AbilityCaster).
///   2. Assign healBurstPrefab and healAuraPrefab in Inspector.
///   3. Drag this component into AbilityCaster.healVFX in Inspector.
///
/// AbilityCaster calls TriggerHealVFX() on the server; the [ClientRpc] broadcasts
/// the particle burst to all clients. Prefabs are instantiated at cast time and
/// auto-destroyed to keep hierarchy clean.
/// </summary>
public class ClericHealVFX : NetworkBehaviour
{
    [Header("VFX Prefabs (assign via BCE/Setup/4v or manually)")]
    [Tooltip("Short radial burst — plays once at cast origin.\n" +
             "Best pick: brbmuffins Magic Pack / Hits and explosions / Holy hit.prefab")]
    public GameObject healBurstPrefab;

    [Tooltip("Soft looping aura — plays briefly then auto-stops.\n" +
             "Best pick: brbmuffins Magic Pack / Character auras / Healing.prefab")]
    public GameObject healAuraPrefab;

    [Tooltip("Secondary rising cross effect (optional extra layer).\n" +
             "Best pick: brbmuffins Free VFX / FX_PinkCross_Up.prefab")]
    public GameObject healCrossPrefab;

    [Header("Timing")]
    [Tooltip("How long the aura prefab lives before being destroyed (seconds).")]
    public float auraDuration = 1.8f;

    [Tooltip("Delay before the cross effect appears (seconds, 0 = simultaneous).")]
    public float crossDelay = 0.15f;

    // Legacy direct-ParticleSystem refs — kept for backwards-compat if anyone
    // assigned them before the prefab-based refactor.
    [Header("Direct Particle Systems (legacy — prefer Prefab fields above)")]
    public ParticleSystem healBurst;
    public ParticleSystem healAura;

    // ── Server entry point ─────────────────────────────────────────────────────

    [Server]
    public void TriggerHealVFX()
    {
        RpcPlayHealVFX();
    }

    // ── Client visual ──────────────────────────────────────────────────────────

    [ClientRpc]
    void RpcPlayHealVFX()
    {
        PlayVisual();
    }

    void PlayVisual()
    {
        Vector3 pos = transform.position;

        // ── Layer 1: radial burst (Holy hit) ──────────────────────────────────
        if (healBurstPrefab != null)
        {
            var go = Instantiate(healBurstPrefab, pos, Quaternion.identity);
            Destroy(go, 3f);
        }
        else if (healBurst != null)
        {
            healBurst.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            healBurst.Play();
        }

        // ── Layer 2: looping aura (Healing.prefab character aura) ────────────
        if (healAuraPrefab != null)
        {
            var go = Instantiate(healAuraPrefab, pos, Quaternion.identity);
            Destroy(go, auraDuration);
        }
        else if (healAura != null)
        {
            healAura.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            healAura.Play();
            StartCoroutine(StopAura());
        }

        // ── Layer 3: rising cross (optional extra polish) ─────────────────────
        if (healCrossPrefab != null)
            StartCoroutine(SpawnCross(pos));
    }

    IEnumerator StopAura()
    {
        yield return new WaitForSeconds(auraDuration);
        if (healAura != null) healAura.Stop();
    }

    IEnumerator SpawnCross(Vector3 pos)
    {
        if (crossDelay > 0f) yield return new WaitForSeconds(crossDelay);
        var go = Instantiate(healCrossPrefab, pos + Vector3.up * 0.2f, Quaternion.identity);
        Destroy(go, 2f);
    }
}
