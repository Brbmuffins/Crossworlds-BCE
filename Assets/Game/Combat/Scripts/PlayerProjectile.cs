using UnityEngine;
using Mirror;

/// <summary>
/// PlayerProjectile — Skill-shot projectile fired by player abilities.
/// Travels forward at speed, damages first Enemy hit, self-destructs on timeout or max range.
/// Server-spawned via NetworkServer.Spawn; in offline mode just Instantiated.
/// Assign through AbilityDef.projectilePrefab with launchProjectile enabled.
/// </summary>
public class PlayerProjectile : NetworkBehaviour
{
    [Header("Movement")]
    public float speed    = 20f;
    public float lifetime = 3f;
    public float maxRange = 12f;

    [Header("Impact")]
    public GameObject impactVFX;
    [Min(0.05f)] public float impactVFXLifetime = 2f;

    private float   _damage;
    private bool    _hit;
    private Vector3 _origin;
    private GameObject _owner;

    /// Called immediately after instantiation on the server to configure the shot.
    public void Init(float damage, Vector3 origin, GameObject owner = null)
    {
        _damage = damage;
        _origin = origin;
        _owner = owner;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        Invoke(nameof(SelfDestruct), lifetime);
    }

    void Start()
    {
        if (!NetworkClient.active && !NetworkServer.active)
            Invoke(nameof(SelfDestruct), lifetime);
    }

    void Update()
    {
        bool offline = !NetworkClient.active && !NetworkServer.active;
        if (!isServer && !offline) return;

        transform.Translate(Vector3.forward * speed * Time.deltaTime, Space.Self);

        // Expire when max travel distance reached
        if (_hit) return;
        if (Vector3.Distance(transform.position, _origin) > maxRange)
            SelfDestruct();
    }

    void OnTriggerEnter(Collider other)
    {
        bool offline = !NetworkClient.active && !NetworkServer.active;
        if ((!isServer && !offline) || _hit) return;
        if (_owner != null && other.transform.root == _owner.transform.root) return;

        GameObject source = ResolveDamageSource();
        if (!PvpCombatRules.MatchesTarget(source, other, "Enemy", out Health health))
        {
            if (!other.isTrigger)
                Impact();
            return;
        }

        if (health != null && health.IsAlive)
            health.TakeDamage(_damage, source);

        Impact();
    }

    void SelfDestruct()
    {
        if (!_hit) DestroyProjectile();
    }

    void Impact()
    {
        _hit = true;
        CancelInvoke(nameof(SelfDestruct));

        if (NetworkServer.active)
            RpcHitEffect(transform.position);
        else
            SpawnHitEffect(transform.position);

        DestroyProjectile();
    }

    void DestroyProjectile()
    {
        if (NetworkServer.active)
            NetworkServer.Destroy(gameObject);
        else
            Destroy(gameObject);
    }

    [ClientRpc]
    void RpcHitEffect(Vector3 pos)
    {
        SpawnHitEffect(pos);
    }

    void SpawnHitEffect(Vector3 pos)
    {
#if UNITY_EDITOR || !UNITY_SERVER
        if (impactVFX == null) return;
        GameObject effect = Instantiate(impactVFX, pos, Quaternion.identity);
        Destroy(effect, Mathf.Max(0.05f, impactVFXLifetime));
#endif
    }

    GameObject ResolveDamageSource()
    {
        if (_owner != null)
            return _owner;

        return connectionToClient != null && connectionToClient.identity != null
            ? connectionToClient.identity.gameObject
            : gameObject;
    }
}
