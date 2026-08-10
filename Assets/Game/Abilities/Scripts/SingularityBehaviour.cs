using System.Collections;
using UnityEngine;

// Arcanist — Void Maw / Collapsing Void
// Phase 1: pull all enemies in radius toward center for pullDuration.
// Phase 2: burst AoE damage, then destroy.
// VFX: brbmuffins Dark Arts/Fantasy Pack/Prefabs/Effects normal/Death magic circle.prefab (cast)
//      brbmuffins Technologies/Particle Pack/Legacy Particles/PlasmaExplosionEffect.prefab (burst)
//      brbmuffins Technologies/Particle Pack/Misc Effects/HeatDistortion.prefab (ambient)
public class SingularityBehaviour : MonoBehaviour
{
    [Header("Pull Phase")]
    public float pullRadius   = 8f;
    public float pullDuration = 3f;
    public float pullForce    = 12f;
    public string enemyTag    = "Enemy";

    [Header("Burst Phase")]
    public float burstDamage  = 20f;
    public float burstRadius  = 8f;

    [Header("Weakened Debuff (Collapsing Void only)")]
    public bool  applyExposed        = false;
    public float exposedDuration     = 8f;

    [Header("Phase Relay bonus")]
    // Increased by PhaseRelayDeployable if one is nearby
    public float pullDurationBonus   = 0f;

    [Header("VFX")]
    // Assign: brbmuffins Dark Arts/.../Death magic circle.prefab
    public GameObject ambientVFX;
    // Assign: brbmuffins Technologies/.../PlasmaExplosionEffect.prefab
    public GameObject burstVFX;

    // Set by AbilityCaster
    [HideInInspector] public GameObject owner;

    private GameObject _ambientInstance;

    void Start()
    {
        if (ambientVFX != null)
            _ambientInstance = Instantiate(ambientVFX, transform.position, Quaternion.identity, transform);

        StartCoroutine(Run());
    }

    private IEnumerator Run()
    {
        // Server owns the pull + burst; client copies render the replicated vortex
        // (ambient VFX spawned in Start) while NetworkTransform holds its position.
        if (!DeployableNet.IsAuthority) yield break;

        float total = pullDuration + pullDurationBonus;
        float elapsed = 0f;

        while (elapsed < total)
        {
            elapsed += Time.fixedDeltaTime;

            Collider[] hits = ZonePhysics.OverlapSphere(gameObject, transform.position, pullRadius);
            var pulled = new System.Collections.Generic.HashSet<Health>();
            foreach (var col in hits)
            {
                if (!PvpCombatRules.MatchesTarget(owner, col, enemyTag, out Health health) ||
                    !pulled.Add(health))
                    continue;
                Rigidbody rb = health.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    Vector3 toward = (transform.position - health.transform.position).normalized;
                    rb.AddForce(toward * pullForce, ForceMode.Acceleration);
                }
                else
                {
                    // Fallback: move via transform
                    Vector3 dir = (transform.position - health.transform.position).normalized;
                    health.transform.position += dir * 3f * Time.fixedDeltaTime;
                }
            }

            yield return new WaitForFixedUpdate();
        }

        // Burst
        Collider[] finalHits = ZonePhysics.OverlapSphere(gameObject, transform.position, burstRadius);
        var damaged = new System.Collections.Generic.HashSet<Health>();
        foreach (var col in finalHits)
        {
            if (!PvpCombatRules.MatchesTarget(owner, col, enemyTag, out Health h) ||
                !damaged.Add(h))
                continue;
            h?.TakeDamage(burstDamage, owner);

            // Collapsing Void: apply Weakened debuff
            if (applyExposed)
            {
                var sem = h.GetComponent<StatusEffectManager>();
                sem?.AddEffect(new StatusEffect(StatusEffectType.Weakened, exposedDuration));
            }
        }

        if (burstVFX != null)
        {
            GameObject fx = Instantiate(burstVFX, transform.position, Quaternion.identity);
            Destroy(fx, 4f);
        }

        DeployableNet.Despawn(gameObject);
    }
}
