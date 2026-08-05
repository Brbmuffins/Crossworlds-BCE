using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Client/server lookup for Loot Forge definitions bundled in Resources.</summary>
public static class LootItemCatalog
{
    static Dictionary<string, LootItemDefinition> _byId;

    public static LootItemDefinition Find(string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId)) return null;
        EnsureLoaded();
        return _byId.TryGetValue(itemId, out LootItemDefinition definition)
            ? definition : null;
    }

    public static IReadOnlyCollection<LootItemDefinition> All
    {
        get { EnsureLoaded(); return _byId.Values; }
    }

    public static Color RarityColor(ItemRarity rarity) => rarity switch
    {
        ItemRarity.Uncommon => new Color(0.2f, 0.9f, 0.2f),
        ItemRarity.Rare => new Color(0.2f, 0.5f, 1f),
        ItemRarity.Epic => new Color(0.7f, 0.1f, 1f),
        ItemRarity.Legendary => new Color(1f, 0.5f, 0.1f),
        _ => new Color(0.75f, 0.75f, 0.75f)
    };

    static void EnsureLoaded()
    {
        if (_byId != null) return;
        _byId = new Dictionary<string, LootItemDefinition>(StringComparer.OrdinalIgnoreCase);
        foreach (LootItemDefinition definition in
                 Resources.LoadAll<LootItemDefinition>("LootForge/Items"))
        {
            if (definition == null || string.IsNullOrWhiteSpace(definition.itemId)) continue;
            if (_byId.ContainsKey(definition.itemId))
                Debug.LogError($"[LOOT FORGE] Duplicate item ID '{definition.itemId}'.", definition);
            else _byId.Add(definition.itemId, definition);
        }
    }
}
