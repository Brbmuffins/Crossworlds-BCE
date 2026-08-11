using UnityEngine;

public enum LootDatabaseItemType
{
    Material,
    Weapon,
    ArmorHead,
    ArmorChest,
    ArmorLegs,
    ArmorFeet,
    ArmorHands,
    Offhand,
    Ring,
    Trinket,
    // Keep this value last so existing serialized enum indices never change.
    Unspecified
}

public enum LootEquipmentSlot
{
    None,
    Head,
    Chest,
    Legs,
    Feet,
    Hands,
    MainHand,
    OffHand,
    Ring,
    Trinket
}

/// <summary>
/// Client-facing world presentation for one backend inventory item. Drop tables
/// reference this asset so an item's model and rarity are authored only once.
/// </summary>
[CreateAssetMenu(
    fileName = "LootItemDefinition",
    menuName = "BCE/Loot Item Definition")]
public class LootItemDefinition : ScriptableObject
{
    [Tooltip("Stable backend inventory item ID awarded when this pickup is collected.")]
    public string itemId;

    [Tooltip("Player-facing name stored in the items database and shown by inventory UI.")]
    public string displayName;

    [Tooltip("Database inventory category. Must match the live items.item_type enum.")]
    public LootDatabaseItemType databaseItemType = LootDatabaseItemType.Unspecified;

    [Tooltip("Required for equipment. Determines the paper-doll and authoritative database slot.")]
    public LootEquipmentSlot equipmentSlot = LootEquipmentSlot.None;

    [Tooltip("Controls the pickup glow and attached loot-beam color.")]
    public ItemRarity rarity = ItemRarity.Common;

    [Tooltip("Sprite shown in the player's inventory bag.")]
    public Sprite inventoryIcon;

    [HideInInspector] public string iconId;

    [Min(0), Tooltip("Base database sell value.")]
    public int sellValue;

    [Tooltip("Whether the database should mark this item as craft-produced.")]
    public bool crafted;

    [Tooltip("Allow multiple copies of this item to share one inventory slot.")]
    public bool stackable = true;

    [Min(1), Tooltip("Maximum quantity held in one slot. Ignored when Stackable is disabled.")]
    public int maxStackSize = 99;

    [Header("Equipped Item")]
    [Tooltip("Client-side model attached to the equipped player and paper-doll preview. May reuse World Visual Prefab for simple weapons.")]
    public GameObject equippedVisualPrefab;

    [Tooltip("Reusable bone and transform defaults for this equipment family, such as Two-Handed Sword.")]
    public EquipmentAttachmentProfile attachmentProfile;

    [Tooltip("Use this item's bone/position/rotation/scale instead of its attachment profile defaults.")]
    public bool overrideAttachmentProfile;

    [Tooltip("Reserves both hands. Enable directly when no attachment profile supplies this rule.")]
    public bool twoHanded;

    [Tooltip("Optional exact skeleton transform name. Leave empty to use slot-aware hand/head/body aliases.")]
    public string attachmentBoneName;
    public Vector3 equippedLocalPosition;
    public Vector3 equippedLocalEulerAngles;
    public Vector3 equippedLocalScale = Vector3.one;
    [HideInInspector] public EquipmentAttachmentClassOverride[] classAttachmentOverrides =
        System.Array.Empty<EquipmentAttachmentClassOverride>();

    public bool IsTwoHanded => attachmentProfile != null ? attachmentProfile.twoHanded : twoHanded;
    public string EffectiveAttachmentBoneName =>
        attachmentProfile != null && !overrideAttachmentProfile
            ? attachmentProfile.attachmentBoneName : attachmentBoneName;
    public Vector3 EffectiveEquippedLocalPosition =>
        attachmentProfile != null && !overrideAttachmentProfile
            ? attachmentProfile.localPosition : equippedLocalPosition;
    public Vector3 EffectiveEquippedLocalEulerAngles =>
        attachmentProfile != null && !overrideAttachmentProfile
            ? attachmentProfile.localEulerAngles : equippedLocalEulerAngles;
    public Vector3 EffectiveEquippedLocalScale =>
        attachmentProfile != null && !overrideAttachmentProfile
            ? attachmentProfile.localScale : equippedLocalScale;

    public string EffectiveAttachmentBoneNameForClass(int classIndex)
    {
        EquipmentAttachmentClassOverride value = FindClassOverride(classIndex);
        return value != null && !string.IsNullOrWhiteSpace(value.attachmentBoneName)
            ? value.attachmentBoneName : EffectiveAttachmentBoneName;
    }

    public Vector3 EffectiveEquippedLocalPositionForClass(int classIndex) =>
        FindClassOverride(classIndex)?.localPosition ?? EffectiveEquippedLocalPosition;

    public Vector3 EffectiveEquippedLocalEulerAnglesForClass(int classIndex) =>
        FindClassOverride(classIndex)?.localEulerAngles ?? EffectiveEquippedLocalEulerAngles;

    public Vector3 EffectiveEquippedLocalScaleForClass(int classIndex)
    {
        EquipmentAttachmentClassOverride value = FindClassOverride(classIndex);
        return value != null && value.localScale.sqrMagnitude > 0.0001f
            ? value.localScale : EffectiveEquippedLocalScale;
    }

    public EquipmentAttachmentClassOverride GetOrCreateClassOverride(int classIndex)
    {
        if (attachmentProfile != null && !overrideAttachmentProfile)
            return attachmentProfile.GetOrCreateClassOverride(classIndex);
        EquipmentAttachmentClassOverride existing = FindItemClassOverride(classIndex);
        if (existing != null) return existing;
        var created = new EquipmentAttachmentClassOverride
        {
            classIndex = classIndex,
            attachmentBoneName = attachmentBoneName,
            localPosition = equippedLocalPosition,
            localEulerAngles = equippedLocalEulerAngles,
            localScale = equippedLocalScale.sqrMagnitude > 0.0001f
                ? equippedLocalScale : Vector3.one
        };
        int length = classAttachmentOverrides?.Length ?? 0;
        System.Array.Resize(ref classAttachmentOverrides, length + 1);
        classAttachmentOverrides[length] = created;
        return created;
    }

    EquipmentAttachmentClassOverride FindClassOverride(int classIndex) =>
        attachmentProfile != null && !overrideAttachmentProfile
            ? attachmentProfile.FindClassOverride(classIndex)
            : FindItemClassOverride(classIndex);

    EquipmentAttachmentClassOverride FindItemClassOverride(int classIndex)
    {
        if (classAttachmentOverrides == null) return null;
        foreach (EquipmentAttachmentClassOverride value in classAttachmentOverrides)
            if (value != null && value.classIndex == classIndex) return value;
        return null;
    }

    [Header("Server-authoritative stat bonuses")]
    [Min(0)] public int bonusStrength;
    [Min(0)] public int bonusAgility;
    [Min(0)] public int bonusIntelligence;
    [Min(0)] public int bonusVitality;

    [Tooltip("Optional item-specific model shown on the world pickup, such as a sword or chest. Leave empty for inventory-only items to use the enemy's assigned generic pickup prefab. Colliders on this visual are disabled when attached.")]
    public GameObject worldVisualPrefab;
}
