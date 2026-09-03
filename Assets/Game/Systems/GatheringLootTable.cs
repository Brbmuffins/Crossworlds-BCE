using System;
using UnityEngine;

[Serializable]
public sealed class GatheringLootEntry
{
    public LootItemDefinition item;
    [Min(0f)] public float weight = 1f;
    [Min(1)] public int minimumQuantity = 1;
    [Min(1)] public int maximumQuantity = 1;
}

/// <summary>Server-rollable weighted rewards shared by Gathering Node definitions.</summary>
[CreateAssetMenu(fileName = "GatheringLootTable", menuName = "BCE/Gathering Loot Table")]
public sealed class GatheringLootTable : ScriptableObject
{
    public string displayName = "Random Resource";
    public GatheringLootEntry[] entries = Array.Empty<GatheringLootEntry>();

    public bool TryRoll(out LootItemDefinition item, out int quantity)
    {
        item = null;
        quantity = 0;
        float total = 0f;
        foreach (GatheringLootEntry entry in entries ?? Array.Empty<GatheringLootEntry>())
            if (IsValid(entry)) total += Mathf.Max(0f, entry.weight);
        if (total <= 0f) return false;

        float roll = UnityEngine.Random.value * total;
        GatheringLootEntry selected = null;
        foreach (GatheringLootEntry entry in entries)
        {
            if (!IsValid(entry)) continue;
            selected = entry;
            roll -= Mathf.Max(0f, entry.weight);
            if (roll < 0f) break;
        }
        if (selected?.item == null) return false;
        item = selected.item;
        quantity = UnityEngine.Random.Range(
            Mathf.Max(1, selected.minimumQuantity),
            Mathf.Max(Mathf.Max(1, selected.minimumQuantity), selected.maximumQuantity) + 1);
        return true;
    }

    static bool IsValid(GatheringLootEntry entry) =>
        entry?.item != null && entry.weight > 0f &&
        entry.item.databaseItemType == LootDatabaseItemType.Material &&
        !string.IsNullOrWhiteSpace(entry.item.itemId);

#if UNITY_EDITOR
    void OnValidate()
    {
        foreach (GatheringLootEntry entry in entries ?? Array.Empty<GatheringLootEntry>())
        {
            if (entry == null) continue;
            entry.weight = Mathf.Max(0f, entry.weight);
            entry.minimumQuantity = Mathf.Max(1, entry.minimumQuantity);
            entry.maximumQuantity = Mathf.Max(entry.minimumQuantity, entry.maximumQuantity);
            if (entry.item != null && entry.item.databaseItemType != LootDatabaseItemType.Material)
                Debug.LogError($"[GATHER TABLE] {name}: {entry.item.name} is not a Material.", this);
        }
    }
#endif
}
