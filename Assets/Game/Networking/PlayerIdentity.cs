using Mirror;
using UnityEngine;

// ═══════════════════════════════════════════════════════════════════════════
//  PlayerIdentity
//  Add to every class prefab (Engineer, Guardian, Wraith, Medic).
//  Syncs the player's name and class to all clients.
//
//  Usage: read playerName / classIndex anywhere on the player object.
// ═══════════════════════════════════════════════════════════════════════════

public class PlayerIdentity : NetworkBehaviour
{
    [SyncVar] public string playerName  = "Player";
    [SyncVar] public int    classIndex  = 0;

    static readonly string[] ClassNames = { "Engineer", "Guardian", "Wraith", "Medic" };

    public string ClassName => classIndex >= 0 && classIndex < ClassNames.Length
        ? ClassNames[classIndex]
        : "Unknown";

    public override void OnStartLocalPlayer()
    {
        // Tag this as the local player object so other scripts can find it
        gameObject.name = playerName + " (Local)";
    }

    public override void OnStartClient()
    {
        // Update display name for remote players
        if (!isLocalPlayer)
            gameObject.name = playerName;
    }
}
