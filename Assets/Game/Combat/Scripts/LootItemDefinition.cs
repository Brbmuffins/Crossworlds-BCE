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
    public LootDatabaseItemType databaseItemType = LootDatabaseItemType.Material;

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

    [Tooltip("Optional item-specific model shown on the world pickup, such as a sword or chest. Leave empty for inventory-only items to use the enemy's assigned generic pickup prefab. Colliders on this visual are disabled when attached.")]
    public GameObject worldVisualPrefab;
}
