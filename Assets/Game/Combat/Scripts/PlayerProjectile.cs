using UnityEngine;
using Mirror;

/// <summary>
/// PlayerProjectile — Skill-shot projectile fired by player abilities (SkillShot shape).
/// Travels forward at speed, damages first Enemy hit, self-destructs on timeout or max range.
/// Server-spawned via NetworkServer.Spawn; in offline mode just Instantiated.
/// Assign to AbilityCaster.playerProjectilePrefab.
/// </summary>
public class PlayerProjectile : NetworkBehaviour
{
    [Header("Movement")]
    public float speed    = 20f;
    public float lifetime = 3f;
    public float maxRange = 12f;

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

    void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime, Space.Self);

        // Expire when max travel distance reached
        if (!isServer || _hit) return;
        if (Vector3.Distance(transform.position, _origin) > maxRange)
            SelfDestruct();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!isServer || _hit) return;
        GameObject source = ResolveDamageSource();
        if (!PvpCombatRules.MatchesTarget(source, other, "Enemy", out Health health)) return;

        _hit = true;
        CancelInvoke(nameof(SelfDestruct));

        if (health != null && health.IsAlive)
            health.TakeDamage(_damage, source);

        RpcHitEffect(transform.position);
        NetworkServer.Destroy(gameObject);
    }

    [Server]
    void SelfDestruct()
    {
        if (!_hit) NetworkServer.Destroy(gameObject);
    }

    [ClientRpc]
    void RpcHitEffect(Vector3 pos)
    {
        // Future: instantiate impact VFX prefab at pos
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
