using Mirror;
using UnityEngine;

/// <summary>
/// CombatZoneStarter — Attach to the Hub's CombatZone trigger collider.
/// First player to enter the zone starts the WaveSpawner.
/// Waves continue indefinitely (WaveSpawner.maxWaves = 0).
/// Server-only: only the server starts waves.
/// </summary>
[RequireComponent(typeof(Collider))]
public class CombatZoneStarter : MonoBehaviour
{
    [Tooltip("Waves auto-start when first player enters. Uncheck for manual start.")]
    public bool autoStart = true;

    private WaveSpawner _spawner;
    private bool        _started;

    void Start()
    {
        _spawner = Object.FindFirstObjectByType<WaveSpawner>();
        if (_spawner == null)
            Debug.LogWarning("[CombatZone] No WaveSpawner found in scene.");
    }

    void OnTriggerEnter(Collider other)
    {
        if (!autoStart || _started || !other.CompareTag("Player")) return;
        if (!NetworkServer.active) return;   // server only

        _started = true;
        _spawner?.StartWaves();
        Debug.Log("[CombatZone] First player entered — waves started.");
    }
}
