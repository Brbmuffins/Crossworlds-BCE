using Mirror;
using UnityEngine;

/// <summary>
/// BossTrigger — child of a WorldBossController. The first player to enter its
/// sphere starts the fight.
///
/// Lives in the RUNTIME assembly on purpose. It used to be declared inside
/// Assets/Game/Editor/WorldBossBuilder.cs behind `#if UNITY_EDITOR`, which put it in
/// the Editor assembly: the component existed while authoring in the editor, but was
/// stripped from every player/dedicated-server build. Online, the boss could never
/// start its fight and the trigger showed up as a missing script. Keep this file out
/// of any Editor/ folder.
///
/// Server-authoritative: OnTriggerEnter fires on clients too (they run physics
/// locally), so every path is gated on NetworkServer.active. StartFight is [Server].
/// </summary>
[RequireComponent(typeof(SphereCollider))]
public class BossTrigger : MonoBehaviour
{
    [Tooltip("Tag that counts as a player entering the arena.")]
    public string playerTag = "Player";

    bool _triggered;

    void OnTriggerEnter(Collider other)
    {
        // Server only. Clients run physics locally and would fire this too, but
        // WorldBossController.StartFight is [Server] — calling it from a client just logs a
        // warning and no-ops. Clients learn the fight started from the currentPhase SyncVar.
        if (_triggered || !NetworkServer.active) return;
        if (other == null || !other.CompareTag(playerTag)) return;

        var boss = GetComponentInParent<WorldBossController>();
        if (boss == null) return;

        _triggered = true;
        boss.StartFight();

        // Disable rather than Destroy: the boss is one NetworkIdentity, so destroying a
        // child locally would not replicate and would desync the server's view of it.
        var col = GetComponent<Collider>();
        if (col != null) col.enabled = false;
        enabled = false;
    }
}
