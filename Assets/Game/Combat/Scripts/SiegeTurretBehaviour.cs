using UnityEngine;
using Mirror;

/// <summary>
/// Phase 2 Siege Turret for The Iron Warden encounter.
/// Spawned by IronWardenController.SpawnSiegeTurrets() at two flanking positions.
///
/// Fires orange energy bolts at nearby players every 3 s (10 dmg per bolt).
/// When health reaches 0, calls warden.OnTurretDestroyed() so the boss can track
/// simultaneous destruction for the immunity mechanic.
/// </summary>
public class SiegeTurretBehaviour : NetworkBehaviour
{
    [Header("Stats")]
    public int maxHealth   = 300;
    public int boltDamage  = 10;
    public float fireRate  = 3f;
    public float fireRange = 15f;

    [HideInInspector] public IronWardenController warden;

    [SyncVar] int _health;

    Coroutine _fireLoop;

    public override void OnStartServer()
    {
        _health    = maxHealth;
        _fireLoop  = StartCoroutine(FireLoop());
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
                var hp = t.GetComponent<HealthComponent>();
                hp?.TakeDamage(boltDamage, gameObject);
                RpcFireVfx(t.transform.position);
                break; // one target per burst
            }
        }
    }

    [Server]
    public void TakeDamage(int amount)
    {
        if (_health <= 0) return;
        _health = Mathf.Max(0, _health - amount);
        if (_health <= 0) OnDeath();
    }

    [Server]
    void OnDeath()
    {
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
