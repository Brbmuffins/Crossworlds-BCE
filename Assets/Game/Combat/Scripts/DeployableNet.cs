using Mirror;
using UnityEngine;

// ═══════════════════════════════════════════════════════════════════════════
//  DeployableNet
//  Shared helpers for player-spawned deployables (mines, walls, zones, wisps,
//  singularities, beacons, turrets) so their gameplay stays server-authoritative
//  once they are NetworkServer.Spawn'd and replicated to every client.
//
//  Pattern: the deployable prefab carries a NetworkIdentity, so the server spawn
//  message replicates the object (and its Start()-spawned ambient/idle child VFX)
//  to all clients. The server alone runs damage/heal/movement/despawn; client
//  copies render only. Guard every gameplay method with `if (!IsAuthority) return;`
//  and despawn via Despawn() so removal replicates.
// ═══════════════════════════════════════════════════════════════════════════
public static class DeployableNet
{
    /// <summary>
    /// True where this object's gameplay logic is authoritative:
    /// offline/solo play (no network), a host, or a dedicated server.
    /// False only on a pure client copy of a server-spawned deployable.
    /// </summary>
    public static bool IsAuthority => !NetworkClient.active || NetworkServer.active;

    /// <summary>
    /// Network-safe despawn. Unspawns across all clients when running on the
    /// server; plain Destroy when offline; no-op on a pure client (the server's
    /// despawn message will remove the local copy).
    /// </summary>
    public static void Despawn(GameObject go)
    {
        if (go == null) return;

        if (NetworkServer.active) NetworkServer.Destroy(go);
        else if (!NetworkClient.active) Object.Destroy(go); // offline / solo
        // pure client: do nothing — server drives removal.
    }
}
