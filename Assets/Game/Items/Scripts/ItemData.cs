using UnityEngine;

public enum EquipmentSlotType
{
    None,
    Head,
    Chest,
    Legs,
    Weapon
}

public enum ItemType
{
    Generic,
    Consumable,
    Equipment,
    QuestItem
}

public enum ItemRarity
{
    // Explicit values are required because Unity serializes enums as integers.
    // Append future tiers; never reorder these entries.
    Common = 0,
    Uncommon = 1,
    Rare = 2,
    Epic = 3,
    Legendary = 4,
    Relic = 5
}

/// <summary>Canonical persistence names and presentation for every item rarity.</summary>
public static class ItemRarityUtility
{
    public static string StorageName(ItemRarity rarity) => rarity switch
    {
        ItemRarity.Uncommon => "uncommon",
        ItemRarity.Rare => "rare",
        ItemRarity.Epic => "epic",
        ItemRarity.Legendary => "legendary",
        ItemRarity.Relic => "relic",
        _ => "common"
    };

    public static string DisplayName(ItemRarity rarity) => rarity switch
    {
        ItemRarity.Uncommon => "Uncommon",
        ItemRarity.Rare => "Rare",
        ItemRarity.Epic => "Epic",
        ItemRarity.Legendary => "Legendary",
        ItemRarity.Relic => "Relic",
        _ => "Common"
    };

    public static bool TryParse(string value, out ItemRarity rarity)
    {
        if (System.Enum.TryParse(value?.Trim(), true, out rarity) &&
            System.Enum.IsDefined(typeof(ItemRarity), rarity))
            return true;

        rarity = ItemRarity.Common;
        return false;
    }

    public static Color Color(ItemRarity rarity) => rarity switch
    {
        ItemRarity.Uncommon => new Color(0.2f, 0.9f, 0.2f),
        ItemRarity.Rare => new Color(0.2f, 0.5f, 1f),
        ItemRarity.Epic => new Color(0.7f, 0.1f, 1f),
        ItemRarity.Legendary => new Color(1f, 0.5f, 0.1f),
        ItemRarity.Relic => new Color(1f, 0.05f, 0.18f),
        _ => new Color(0.75f, 0.75f, 0.75f)
    };

    public static ItemRarity InferLegacyItemId(string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId)) return ItemRarity.Common;
        string id = itemId.ToLowerInvariant();
        if (id.Contains("relic")) return ItemRarity.Relic;
        if (id.Contains("legendary")) return ItemRarity.Legendary;
        if (id.Contains("epic")) return ItemRarity.Epic;
        if (id.Contains("rare") || id.Contains("iron")) return ItemRarity.Rare;
        if (id.Contains("uncommon") || id.Contains("bar")) return ItemRarity.Uncommon;
        return ItemRarity.Common;
    }
}

[CreateAssetMenu(fileName = "New Item", menuName = "Legacy/Inventory/Item")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    [TextArea]
    public string description;
    public bool stackable = true;
    public int maxStackSize = 99;

    public ItemType itemType = ItemType.Generic;
    public ItemRarity rarity = ItemRarity.Common;

    public bool equippable = false;
    public EquipmentSlotType equipSlot = EquipmentSlotType.None;

    public float healAmount = 0f;

    [Header("Gear Stats (attunement system)")]
    [Tooltip("Innate stat bonuses granted while this gear is equipped.")]
    public StatModifier[] baseModifiers;

    [Tooltip("How many attunements can be socketed into this gear.")]
    public int attunementSlots = 0;

    [Tooltip("Attunements currently socketed. Should not exceed attunementSlots.")]
    public Attunement[] installedAttunements;

    // Every active modifier: innate gear stats + all socketed attunements.
    public System.Collections.Generic.IEnumerable<StatModifier> AllModifiers()
    {
        if (baseModifiers != null)
            foreach (var m in baseModifiers) yield return m;

        if (installedAttunements != null)
            foreach (var att in installedAttunements)
                if (att != null && att.modifiers != null)
                    foreach (var m in att.modifiers) yield return m;
    }

    public Color RarityColor => ItemRarityUtility.Color(rarity);
}
