using UnityEngine;
using Mirror;

// Medic deployable — Restoration Beacon
// Pulsing nanite emitter heals all allies in range every pulseInterval seconds.
// VFX: brbmuffins Dark Arts/Fantasy Pack/Prefabs/Effects normal/Magic circle.prefab (tint green)
//      brbmuffins Dark Arts/Fantasy Pack/Prefabs/Healing buff.prefab (per-heal burst)
public class RestorationBeacon : NetworkBehaviour
{
    [Header("Healing")]
    public float healPerPulse   = 12f;
    public float pulseInterval  = 3f;
    public float radius         = 8f;
    public string playerTag     = "Player";
    public float lifetime       = 30f;

    [Header("VFX")]
    // Assign: brbmuffins Dark Arts/.../Magic circle.prefab
    public GameObject idleVFX;
    // Assign: brbmuffins Dark Arts/.../Healing buff.prefab
    public GameObject pulseVFX;

    [HideInInspector] public UnityEngine.EntityId ownerID;
    [HideInInspector] public GameObject owner;

    private float _pulseTimer;
    private float _lifetimeTimer;

    void Start()
    {
        if (idleVFX != null)
            Instantiate(idleVFX, transform.position, Quaternion.identity, transform);

        // Deployable tracking (limit enforcement) is server-authoritative — only the
        // authority registers, so it never despawns a client's local copy.
        if (DeployableNet.IsAuthority && DeployableManager.Instance != null)
            DeployableManager.Instance.Register(gameObject, ownerID, 1);
    }

    void Update()
    {
        // Server drives healing pulses + lifetime; client copies render the idle VFX.
        if (!DeployableNet.IsAuthority) return;

        _lifetimeTimer += Time.deltaTime;
        if (_lifetimeTimer >= lifetime) { DeployableNet.Despawn(gameObject); return; }

        _pulseTimer += Time.deltaTime;
        if (_pulseTimer < pulseInterval) return;
        _pulseTimer = 0f;
        Pulse();
    }

    void Pulse()
    {
        float mult = DeployableManager.Instance != null
            ? DeployableManager.Instance.GetMultiplier(gameObject)
            : 1f;

        Collider[] hits = ZonePhysics.OverlapSphere(gameObject, transform.position, radius);
        foreach (var col in hits)
        {
            if (!col.CompareTag(playerTag)) continue;
            col.GetComponent<Health>()?.Heal(healPerPulse * mult);
        }

        RpcPlayPulseVFX();
    }

    [ClientRpc]
    void RpcPlayPulseVFX()
    {
        if (pulseVFX != null)
        {
            GameObject fx = Instantiate(pulseVFX, transform.position, Quaternion.identity);
            Destroy(fx, 2f);
        }
    }

    void OnDestroy()
    {
        if (DeployableNet.IsAuthority)
            DeployableManager.Instance?.Unregister(gameObject);
    }
}
