using System.Collections;
using UnityEngine;
using Mirror;

/// <summary>
/// NullArchitectArenaStarter — scene object in VoidDungeon.
///
/// On server start, spawns the Null Architect boss prefab after a short delay.
/// The boss prefab must be registered in RodNetworkManager → spawnPrefabs.
///
/// Inspector assignments (required before play):
///   Boss Prefab     — Assets/Game/Prefabs/NullArchitect_Boss.prefab
///   Spawn Delay     — default 3s (players see the room before the boss materialises)
///
/// Architecture: this is a NetworkBehaviour with NetworkIdentity on the scene object.
/// The BOSS itself has no scene placement — it is server-spawned here, so it obeys the
/// project rule: gameplay objects with NetworkIdentity are never hand-placed in scenes.
/// </summary>
public class NullArchitectArenaStarter : NetworkBehaviour
{
    [Header("Boss Prefab — must be in NetworkManager.spawnPrefabs")]
    public GameObject bossPrefab;

    [Header("Delay before boss materialises (seconds)")]
    public float spawnDelay = 3f;

    private bool _spawned = false;

    public override void OnStartServer()
    {
        base.OnStartServer();
        StartCoroutine(SpawnBossAfterDelay());
    }

    [Server]
    IEnumerator SpawnBossAfterDelay()
    {
        yield return new WaitForSeconds(spawnDelay);

        if (_spawned) yield break;
        _spawned = true;

        if (bossPrefab == null)
        {
            Debug.LogError("[NullArchitect] bossPrefab not assigned on NullArchitectArenaStarter! " +
                           "Run BCE/Setup/6, save as prefab, assign here, and register in NetworkManager.");
            yield break;
        }

        var boss = Instantiate(bossPrefab, Vector3.zero, Quaternion.identity);
        ZoneScene.PlaceWith(boss, gameObject);   // keep the boss in this starter's zone
        NetworkServer.Spawn(boss);

        Debug.Log("[NullArchitect] Boss spawned via NullArchitectArenaStarter.");
    }
}
