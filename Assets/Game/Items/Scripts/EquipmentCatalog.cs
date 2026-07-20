using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// EquipmentCatalog — the registry of every equippable item, keyed by serverItemId
/// (which matches items.id in the auth DB). One asset lives at
/// Resources/EquipmentCatalog so BOTH the dedicated server (needs slot lookup to
/// validate an equip Command) and clients (need model + stats) can load it without
/// any scene reference.
///
/// The dedicated server only ever reads GetSlot(); it never instantiates worldModelPrefab.
/// </summary>
[CreateAssetMenu(fileName = "EquipmentCatalog", menuName = "Inventory/Equipment Catalog")]
public class EquipmentCatalog : ScriptableObject
{
    public ItemData[] items;

    static EquipmentCatalog _instance;
    Dictionary<string, ItemData> _byId;

    public static EquipmentCatalog Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<EquipmentCatalog>("EquipmentCatalog");
                if (_instance == null)
                    Debug.LogWarning("[EQUIP] No EquipmentCatalog found at Resources/EquipmentCatalog — " +
                                     "equip visuals and server-side slot validation are disabled.");
                else
                    _instance.Build();
            }
            return _instance;
        }
    }

    void Build()
    {
        _byId = new Dictionary<string, ItemData>();
        if (items == null) return;
        foreach (var it in items)
            if (it != null && !string.IsNullOrEmpty(it.serverItemId))
                _byId[it.serverItemId] = it;
    }

    public ItemData Get(string serverItemId)
    {
        if (_byId == null) Build();
        return (!string.IsNullOrEmpty(serverItemId) && _byId.TryGetValue(serverItemId, out var it))
            ? it : null;
    }

    /// Server-safe: returns the slot an item occupies without touching its model.
    public EquipmentSlotType GetSlot(string serverItemId)
    {
        var it = Get(serverItemId);
        return it != null ? it.equipSlot : EquipmentSlotType.None;
    }
}
