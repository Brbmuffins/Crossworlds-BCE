using System.Collections.Generic;
using Mirror;
using UnityEngine;

// ═══════════════════════════════════════════════════════════════════════════
//  CrossworldsInterestManagement (ROADMAP 6.5 + 6.6)
//
//  Mirror's SceneDistanceInterestManagement does what the multi-zone world
//  needs: scope observers by scene first, then by distance within a zone. This
//  subclass adds one exception — objects marked GlobalNetworkObject are observed
//  by EVERY connected player regardless of scene.
//
//  Without that exception the DontDestroyOnLoad ChatManager matches no player's
//  scene, gets zero observers, and chat dies for the whole server the moment
//  interest management is enabled. Chat is global by owner decision (ROADMAP
//  open question 9), so the single DDOL instance stays and gets exempted rather
//  than being split per zone.
//
//  Only ONE interest management component may exist on a NetworkManager. This is
//  it — do not also add SceneInterestManagement or DistanceInterestManagement.
// ═══════════════════════════════════════════════════════════════════════════

[AddComponentMenu("BCE/Network/Crossworlds Interest Management")]
public class CrossworldsInterestManagement : SceneDistanceInterestManagement
{
    // Cached rather than looked up per check: OnCheckObserver runs per object per
    // connection, and TryGetComponent in that loop is real cost at scale. Same
    // approach the base class uses for its custom-range lookups.
    readonly HashSet<NetworkIdentity> _global = new HashSet<NetworkIdentity>();

    [ServerCallback]
    public override void OnSpawned(NetworkIdentity identity)
    {
        if (identity != null && identity.TryGetComponent(out GlobalNetworkObject _))
        {
            _global.Add(identity);
            Debug.Log($"[IM] '{identity.name}' is world-global — observed by every player.");
        }

        base.OnSpawned(identity);
    }

    [ServerCallback]
    public override void OnDestroyed(NetworkIdentity identity)
    {
        _global.Remove(identity);
        base.OnDestroyed(identity);
    }

    [ServerCallback]
    public override void ResetState()
    {
        _global.Clear();
        base.ResetState();
    }

    public override bool OnCheckObserver(NetworkIdentity identity, NetworkConnectionToClient newObserver)
    {
        if (_global.Contains(identity)) return true;

        // Guard the base implementation: it dereferences newObserver.identity, which
        // is null for a connection that has authenticated but not yet spawned a player.
        // That window is wider now — the spawn waits on an async additive scene load.
        if (newObserver == null || newObserver.identity == null) return false;

        return base.OnCheckObserver(identity, newObserver);
    }

    public override void OnRebuildObservers(NetworkIdentity identity, HashSet<NetworkConnectionToClient> newObservers)
    {
        if (_global.Contains(identity))
        {
            foreach (NetworkConnectionToClient conn in NetworkServer.connections.Values)
                if (conn != null && conn.isAuthenticated && conn.identity != null)
                    newObservers.Add(conn);
            return;
        }

        base.OnRebuildObservers(identity, newObservers);
    }
}
