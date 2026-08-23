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

    public static Color RarityColor(ItemRarity rarity) => ItemRarityUtility.Color(rarity);

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
