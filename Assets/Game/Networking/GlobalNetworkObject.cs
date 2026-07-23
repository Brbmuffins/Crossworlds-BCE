using UnityEngine;

// ═══════════════════════════════════════════════════════════════════════════
//  GlobalNetworkObject — marks a NetworkIdentity as world-global (ROADMAP 6.5)
//
//  Interest management scopes observers by scene. That is exactly what we want
//  for enemies, drops and players — and exactly wrong for the handful of objects
//  that belong to the whole world rather than to any one zone.
//
//  The ChatManager is the motivating case. RodNetworkManager marks it
//  DontDestroyOnLoad, which moves it into Unity's DDOL pseudo-scene. That scene
//  matches no player's scene, so under plain scene-based interest management it
//  would get ZERO observers and chat would go silent for everyone the moment
//  interest management was switched on.
//
//  Attach this to any NetworkIdentity that every connected player must observe
//  regardless of where they are standing. Use sparingly: each global object is
//  broadcast to every client, which is the cost interest management exists to
//  avoid.
// ═══════════════════════════════════════════════════════════════════════════

[DisallowMultipleComponent]
[AddComponentMenu("BCE/Network/Global Network Object")]
public class GlobalNetworkObject : MonoBehaviour
{
    [Tooltip("Why this object is world-global. Documentation only.")]
    [TextArea(2, 4)]
    public string rationale = "Observed by every player regardless of zone.";
}
