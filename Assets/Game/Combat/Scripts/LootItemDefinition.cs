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

    [Tooltip("Model shown in the world for this item, such as a sword, material, or chest. Colliders on this visual are disabled when attached to the pickup.")]
    public GameObject worldVisualPrefab;
}
