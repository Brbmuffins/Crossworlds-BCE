using Mirror;
using UnityEngine;

/// <summary>
/// BossArenaTrigger — attach to the Iron Warden arena entry trigger collider.
/// The first player to enter activates the boss (Siege Protocol).
/// Server-only: only the server starts the encounter.
///
/// Mirrors CombatZoneStarter's pattern. Placed by BCE/Setup/9b.
/// </summary>
[RequireComponent(typeof(Collider))]
public class BossArenaTrigger : MonoBehaviour
{
    [Tooltip("Boss activates when the first player enters. Uncheck for manual start.")]
    public bool autoStart = true;

    private IronWardenController _warden;
    private bool                 _started;

    void Start()
    {
        _warden = Object.FindAnyObjectByType<IronWardenController>();
        if (_warden == null)
            Debug.LogWarning("[WardenArena] No IronWardenController found in scene.");
    }

    void OnTriggerEnter(Collider other)
    {
        if (!autoStart || _started || !other.CompareTag("Player")) return;
        if (!NetworkServer.active) return;   // server only

        // Re-resolve in case the boss spawned after this trigger's Start().
        if (_warden == null) _warden = Object.FindAnyObjectByType<IronWardenController>();
        if (_warden == null) return;

        _started = true;
        _warden.Activate();
        Debug.Log("[WardenArena] First player entered — Iron Warden activated.");
    }
}
