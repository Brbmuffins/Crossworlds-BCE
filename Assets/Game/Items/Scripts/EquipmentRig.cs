using UnityEngine;

/// <summary>
/// EquipmentRig — placed on every hero prefab. Exposes the transform each equipment slot's
/// world model attaches to. Assign these in the editor to the appropriate rig bones
/// (right hand for the sword, left hand/forearm for the shield, head bone for helmets, etc.).
///
/// PlayerEquipment reads GetMount(slot) to know where to parent an equipped item's model.
/// A null mount just means "no visual for that slot on this hero" — the item still equips
/// and applies stats, it simply won't render a model.
/// </summary>
public class EquipmentRig : MonoBehaviour
{
    [Header("Attach mounts (assign to rig bones)")]
    public Transform headMount;
    public Transform chestMount;
    public Transform feetMount;
    public Transform handsMount;
    [Tooltip("Sword / primary weapon — usually the right-hand bone.")]
    public Transform mainHandMount;
    [Tooltip("Shield / off-hand — usually the left-hand or left-forearm bone.")]
    public Transform offHandMount;

    public Transform GetMount(EquipmentSlotType slot)
    {
        switch (slot)
        {
            case EquipmentSlotType.Head:     return headMount;
            case EquipmentSlotType.Chest:    return chestMount;
            case EquipmentSlotType.Feet:     return feetMount;
            case EquipmentSlotType.Hands:    return handsMount;
            case EquipmentSlotType.MainHand:
            case EquipmentSlotType.Weapon:   return mainHandMount;   // legacy alias
            case EquipmentSlotType.OffHand:  return offHandMount;
            default:                         return null;
        }
    }
}
