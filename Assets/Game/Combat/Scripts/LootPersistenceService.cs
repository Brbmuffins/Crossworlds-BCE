using System;
using System.Collections;
using System.Text;
using Mirror;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>Dedicated-server synchronization for Loot Forge item definitions.</summary>
public static class LootPersistenceService
{
    static LootPersistenceHost _host;
    static string ServiceToken =>
        Environment.GetEnvironmentVariable("CROSSWORLDS_GAME_SERVICE_TOKEN") ?? "";
    static string AuthBaseUrl =>
        (Environment.GetEnvironmentVariable("CROSSWORLDS_AUTH_URL") ??
         "http://127.0.0.1:3000").TrimEnd('/');

    static LootPersistenceHost Host
    {
        get
        {
            if (_host != null) return _host;
            var go = new GameObject("[LootPersistenceService]");
            UnityEngine.Object.DontDestroyOnLoad(go);
            return _host = go.AddComponent<LootPersistenceHost>();
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Bootstrap()
    {
        if (string.IsNullOrWhiteSpace(ServiceToken)) return;
        LootItemDefinition[] definitions =
            Resources.LoadAll<LootItemDefinition>("LootForge/Items");
        if (definitions.Length > 0)
            Host.StartCoroutine(SyncWhenServerStarts(definitions));
    }

    static IEnumerator SyncWhenServerStarts(LootItemDefinition[] definitions)
    {
        while (!NetworkServer.active) yield return null;
        foreach (LootItemDefinition definition in definitions)
        {
            if (definition == null || string.IsNullOrWhiteSpace(definition.itemId)) continue;
            yield return SendDefinition(definition);
        }
    }

    static IEnumerator SendDefinition(LootItemDefinition definition)
    {
        var dto = new LootDefinitionDto
        {
            itemId = definition.itemId.Trim(),
            displayName = definition.displayName?.Trim(),
            rarity = ItemRarityUtility.StorageName(definition.rarity),
            itemType = ItemTypeName(definition.databaseItemType),
            equipmentSlot = EquipmentSlotName(definition.equipmentSlot),
            iconId = string.IsNullOrWhiteSpace(definition.iconId) ? null : definition.iconId,
            sellValue = Mathf.Max(0, definition.sellValue),
            crafted = definition.crafted,
            stackable = definition.stackable,
            maxStackSize = definition.stackable ? Mathf.Max(1, definition.maxStackSize) : 1,
            twoHanded = definition.IsTwoHanded,
            statStr = Mathf.Max(0, definition.bonusStrength),
            statAgi = Mathf.Max(0, definition.bonusAgility),
            statInt = Mathf.Max(0, definition.bonusIntelligence),
            statVit = Mathf.Max(0, definition.bonusVitality)
        };
        string json = JsonUtility.ToJson(dto);
        using var request = new UnityWebRequest(
            AuthBaseUrl + "/api/game/items/definitions", "POST");
        request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("X-Game-Server-Token", ServiceToken);
        request.timeout = 10;
        yield return request.SendWebRequest();
        if (request.result != UnityWebRequest.Result.Success)
            Debug.LogWarning($"[LOOT DB] Sync '{definition.itemId}' failed: " +
                             $"{request.error} {request.downloadHandler.text}");
        else
            Debug.Log($"[LOOT DB] Synced '{definition.itemId}' as " +
                      $"{dto.itemType}/{dto.equipmentSlot ?? "inventory"}.");
    }

    static string ItemTypeName(LootDatabaseItemType type) => type switch
    {
        LootDatabaseItemType.Weapon => "weapon",
        LootDatabaseItemType.ArmorHead => "armor_head",
        LootDatabaseItemType.ArmorChest => "armor_chest",
        LootDatabaseItemType.ArmorLegs => "armor_legs",
        LootDatabaseItemType.ArmorFeet => "armor_feet",
        LootDatabaseItemType.ArmorHands => "armor_hands",
        LootDatabaseItemType.Offhand => "offhand",
        LootDatabaseItemType.Ring => "ring",
        LootDatabaseItemType.Trinket => "trinket",
        LootDatabaseItemType.Consumable => "consumable",
        _ => "material"
    };

    public static string EquipmentSlotName(LootEquipmentSlot slot) => slot switch
    {
        LootEquipmentSlot.Head => "head",
        LootEquipmentSlot.Chest => "chest",
        LootEquipmentSlot.Legs => "legs",
        LootEquipmentSlot.Feet => "feet",
        LootEquipmentSlot.Hands => "hands",
        LootEquipmentSlot.MainHand => "main_hand",
        LootEquipmentSlot.OffHand => "off_hand",
        LootEquipmentSlot.Ring => "ring",
        LootEquipmentSlot.Trinket => "trinket",
        _ => null
    };

    [Serializable]
    sealed class LootDefinitionDto
    {
        public string itemId, displayName, rarity, itemType, equipmentSlot, iconId;
        public int sellValue, maxStackSize, statStr, statAgi, statInt, statVit;
        public bool crafted, stackable, twoHanded;
    }
}

public sealed class LootPersistenceHost : MonoBehaviour { }
