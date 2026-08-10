#if UNITY_EDITOR || !UNITY_SERVER
using System;

public enum CharacterEquipmentSlot
{
    Head,
    Shoulder,
    Chest,
    Hands,
    MainHand,
    OffHand,
    Legs,
    Feet,
    RingLeft,
    RingRight,
    Trinket
}

/// <summary>Single mapping from active backend item categories to Character-window slots.</summary>
public static class CharacterEquipmentSlotMap
{
    public static bool TryMap(string itemId, int ringOrdinal, out CharacterEquipmentSlot slot)
    {
        LootItemDefinition definition = LootItemCatalog.Find(itemId);
        if (definition != null) return TryMap(definition.databaseItemType, ringOrdinal, out slot);

        string type = ItemCatalogManager.Instance?.GetTemplate(itemId)?.item_type;
        return TryMapServerType(type, ringOrdinal, out slot);
    }

    public static bool TryMap(LootDatabaseItemType type, int ringOrdinal, out CharacterEquipmentSlot slot)
    {
        switch (type)
        {
            case LootDatabaseItemType.ArmorHead: slot = CharacterEquipmentSlot.Head; return true;
            case LootDatabaseItemType.ArmorChest: slot = CharacterEquipmentSlot.Chest; return true;
            case LootDatabaseItemType.ArmorHands: slot = CharacterEquipmentSlot.Hands; return true;
            case LootDatabaseItemType.ArmorLegs: slot = CharacterEquipmentSlot.Legs; return true;
            case LootDatabaseItemType.ArmorFeet: slot = CharacterEquipmentSlot.Feet; return true;
            case LootDatabaseItemType.Weapon: slot = CharacterEquipmentSlot.MainHand; return true;
            case LootDatabaseItemType.Offhand: slot = CharacterEquipmentSlot.OffHand; return true;
            case LootDatabaseItemType.Ring:
                slot = ringOrdinal == 0 ? CharacterEquipmentSlot.RingLeft : CharacterEquipmentSlot.RingRight;
                return ringOrdinal < 2;
            case LootDatabaseItemType.Trinket: slot = CharacterEquipmentSlot.Trinket; return true;
            default: slot = default; return false;
        }
    }

    static bool TryMapServerType(string type, int ringOrdinal, out CharacterEquipmentSlot slot)
    {
        string normalized = (type ?? "").Replace("_", "").Replace("-", "").ToLowerInvariant();
        switch (normalized)
        {
            case "armorhead": slot = CharacterEquipmentSlot.Head; return true;
            case "armorchest": slot = CharacterEquipmentSlot.Chest; return true;
            case "armorhands": slot = CharacterEquipmentSlot.Hands; return true;
            case "armorlegs": slot = CharacterEquipmentSlot.Legs; return true;
            case "armorfeet": slot = CharacterEquipmentSlot.Feet; return true;
            case "weapon": slot = CharacterEquipmentSlot.MainHand; return true;
            case "offhand": slot = CharacterEquipmentSlot.OffHand; return true;
            case "ring":
                slot = ringOrdinal == 0 ? CharacterEquipmentSlot.RingLeft : CharacterEquipmentSlot.RingRight;
                return ringOrdinal < 2;
            case "trinket": slot = CharacterEquipmentSlot.Trinket; return true;
            default: slot = default; return false;
        }
    }
}
#endif
