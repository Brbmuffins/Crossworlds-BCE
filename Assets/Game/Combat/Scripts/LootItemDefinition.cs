using UnityEngine;

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

    [Tooltip("Controls the pickup glow and attached loot-beam color.")]
    public ItemRarity rarity = ItemRarity.Common;

    [Tooltip("Model shown in the world for this item, such as a sword, material, or chest. Colliders on this visual are disabled when attached to the pickup.")]
    public GameObject worldVisualPrefab;
}
