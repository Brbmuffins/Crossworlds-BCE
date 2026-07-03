using Mirror;
using UnityEngine;

/// <summary>
/// ArenaAutoStarter — server-side bridge that calls WaveSpawner.StartWaves()
/// and ArenaSessionController.BeginSession() when the arena scene activates.
/// Attach to a NetworkIdentity GameObject in Arena scenes (ArenaSceneBuilder does this).
/// </summary>
public class ArenaAutoStarter : NetworkBehaviour
{
    public override void OnStartServer()
    {
        base.OnStartServer();

        ArenaSessionController.Instance?.BeginSession();

        var spawner = FindAnyObjectByType<WaveSpawner>();
        if (spawner != null)
            spawner.StartWaves();
        else
            Debug.LogWarning("[ArenaAutoStarter] No WaveSpawner found in scene — waves will not start.");
    }
}
