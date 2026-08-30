using UnityEngine;
using Mirror;
using System.Collections;

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
    private int _criticalCombustionBonus;

    /// Called immediately after instantiation on the server to configure the shot.
    public void Init(
        float damage,
        Vector3 origin,
        GameObject owner = null,
        int criticalCombustionBonus = 0)
    {
        _damage = damage;
        _origin = origin;
        _owner = owner;
        _criticalCombustionBonus = Mathf.Max(0, criticalCombustionBonus);
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
        {
            bool wasCritical = false;
            CharacterStats stats = source != null
                ? source.GetComponent<CharacterStats>()
                : null;
            float finalDamage = stats != null
                ? stats.ApplyCriticalStrike(_damage, out wasCritical)
                : Mathf.Max(0f, _damage);

            float healthBefore = health.currentHealth;
            health.TakeDamage(finalDamage, source, wasCritical);
            bool dealtHealthDamage = health.currentHealth < healthBefore;

            if (dealtHealthDamage && _criticalCombustionBonus > 0 && source != null)
            {
                AbilityCaster caster = source.GetComponent<AbilityCaster>();
                caster?.AwardCombustionFromDamage(
                    _criticalCombustionBonus,
                    wasCritical);
                _criticalCombustionBonus = 0;
            }
        }

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
        speed = 0f;

        Collider hitbox = GetComponent<Collider>();
        if (hitbox != null)
            hitbox.enabled = false;

        if (NetworkServer.active)
        {
            RpcHitEffect(transform.position);
            StartCoroutine(DestroyAfterRpcFlush());
        }
        else
        {
            SpawnHitEffect(transform.position);
            DestroyProjectile();
        }
    }

    IEnumerator DestroyAfterRpcFlush()
    {
        // Keep the NetworkIdentity alive through one network update so Mirror can
        // deliver RpcHitEffect before the projectile's destroy message.
        yield return null;
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
        foreach (ParticleSystem particles in
            effect.GetComponentsInChildren<ParticleSystem>(true))
        {
            particles.Play(true);
        }
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
