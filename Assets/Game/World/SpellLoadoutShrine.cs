using System.Collections.Generic;
using Mirror;
using UnityEngine;

/// <summary>
/// Reusable world interaction that grants access to the player's spell loadout.
/// Add it to a shrine, lectern, trainer, or other scene object. A trigger sphere
/// is created automatically when the object has no trigger collider.
/// </summary>
[DisallowMultipleComponent]
[AddComponentMenu("BCE/Spell Loadout Shrine")]
public sealed class SpellLoadoutShrine : MonoBehaviour, INPCInteractable
{
    [Header("Interaction")]
    [Min(0.5f)]
    public float interactionRadius = 3f;

    public string shrineName = "Spell Shrine";

    readonly HashSet<Collider> localPlayerOverlaps = new();

    public string PromptText =>
        $"Press E to attune spells at {shrineName}";

    void Awake()
    {
        EnsureTrigger();
    }

    void OnDisable()
    {
#if UNITY_EDITOR || !UNITY_SERVER
        NPCInteractionManager.Instance?.UnregisterNearby(this);
        PlayerHUD.Instance?.CloseSpellLoadout(this);
#endif
        localPlayerOverlaps.Clear();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!IsLocalPlayer(other) ||
            !localPlayerOverlaps.Add(other))
            return;

#if UNITY_EDITOR || !UNITY_SERVER
        NPCInteractionManager.Instance?.RegisterNearby(this);
#endif
    }

    void OnTriggerExit(Collider other)
    {
        if (!localPlayerOverlaps.Remove(other) ||
            localPlayerOverlaps.Count > 0)
            return;

#if UNITY_EDITOR || !UNITY_SERVER
        NPCInteractionManager.Instance?.UnregisterNearby(this);
        PlayerHUD.Instance?.CloseSpellLoadout(this);
#endif
    }

    public void Interact()
    {
#if UNITY_EDITOR || !UNITY_SERVER
        PlayerHUD hud = PlayerHUD.Instance;
        if (hud == null) return;

        if (hud.IsEditingAt(this))
            hud.CloseSpellLoadout();
        else
            hud.OpenSpellLoadout(this);
#endif
    }

    /// <summary>
    /// Server-side proximity validation for remote equip requests.
    /// </summary>
    public static bool ServerCanEditLoadout(NetworkIdentity player)
    {
        if (player == null) return false;

        foreach (SpellLoadoutShrine shrine in
                 FindObjectsByType<SpellLoadoutShrine>(
                     FindObjectsInactive.Exclude))
        {
            if (shrine == null ||
                shrine.gameObject.scene != player.gameObject.scene)
                continue;

            float allowedDistance =
                Mathf.Max(0.5f, shrine.interactionRadius) + 1.5f;
            if ((shrine.transform.position -
                 player.transform.position).sqrMagnitude <=
                allowedDistance * allowedDistance)
                return true;
        }

        return false;
    }

    void EnsureTrigger()
    {
        SphereCollider sphere = null;
        bool hasTrigger = false;
        foreach (Collider collider in GetComponents<Collider>())
        {
            if (!collider.isTrigger) continue;
            hasTrigger = true;
            sphere = collider as SphereCollider;
            break;
        }

        if (!hasTrigger)
        {
            sphere = gameObject.AddComponent<SphereCollider>();
            sphere.isTrigger = true;
        }

        if (sphere != null)
            sphere.radius = interactionRadius;
    }

    static bool IsLocalPlayer(Collider other)
    {
        NetworkIdentity identity =
            other.GetComponentInParent<NetworkIdentity>();
        if (identity != null)
            return identity.isLocalPlayer;

        return other.CompareTag("Player") ||
               other.GetComponentInParent<PlayerMovement>() != null;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.55f, 0.25f, 1f, 0.28f);
        Gizmos.DrawSphere(
            transform.position,
            Mathf.Max(0.5f, interactionRadius));
    }
}
