using UnityEngine;
using Mirror;

/// <summary>
/// Phase 2 Siege Turret for The Iron Warden encounter.
/// Spawned by IronWardenController.SpawnSiegeTurrets() at two flanking positions.
///
/// Fires orange energy bolts at nearby players every 3 s (10 dmg per bolt).
/// Damage is taken through the standard Health component (player attacks hit it
/// like any enemy). On death, calls warden.OnTurretDestroyed() so the boss can
/// track simultaneous destruction for the immunity mechanic.
///
/// Prefab requirements: NetworkIdentity + Health (maxHealth set here) + collider,
/// registered in NetworkManager.spawnPrefabs.
/// </summary>
[RequireComponent(typeof(Health))]
public class SiegeTurretBehaviour : NetworkBehaviour
{
    [Header("Stats")]
    public int maxHealth   = 300;
    public int boltDamage  = 10;
    public float fireRate  = 3f;
    public float fireRange = 15f;

    [HideInInspector] public IronWardenController warden;

    Health    _health;
    Coroutine _fireLoop;
    bool      _died;

    public override void OnStartServer()
    {
        _health = GetComponent<Health>();
        _health.maxHealth     = maxHealth;
        _health.currentHealth = maxHealth;
        _health.onDeath.AddListener(OnDeath);
        _fireLoop = StartCoroutine(FireLoop());
    }

    [Server]
    System.Collections.IEnumerator FireLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(fireRate);
            var targets = Physics.OverlapSphere(transform.position, fireRange);
            foreach (var t in targets)
            {
                if (!t.CompareTag("Player")) continue;
                var hp = t.GetComponent<Health>();
                if (hp == null || !hp.IsAlive) continue;
                hp.TakeDamage(boltDamage, gameObject);
                RpcFireVfx(t.transform.position);
                break; // one target per burst
            }
        }
    }

    [Server]
    void OnDeath()
    {
        if (_died) return;
        _died = true;
        if (_fireLoop != null) StopCoroutine(_fireLoop);
        warden?.OnTurretDestroyed();
        RpcDeathVfx();
        NetworkServer.Destroy(gameObject);
    }

    [ClientRpc]
    void RpcFireVfx(Vector3 target) { /* Orange bolt particle toward target */ }

    [ClientRpc]
    void RpcDeathVfx() { /* Explosion particle at turret position */ }
}
