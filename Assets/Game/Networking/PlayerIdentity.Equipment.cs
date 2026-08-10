using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Mirror;
using UnityEngine;
using UnityEngine.Networking;

public struct EquippedLootState
{
    public int inventorySlotIndex;
    public LootEquipmentSlot equipmentSlot;
    public string itemId;
}

/// <summary>Server-owned equipment state and client visual presentation on every player.</summary>
public partial class PlayerIdentity
{
    public readonly SyncList<EquippedLootState> equippedLoot = new();
    public event Action EquipmentChanged;

    readonly Dictionary<LootEquipmentSlot, GameObject> _equippedVisuals = new();

    static string EquipmentServiceToken =>
        Environment.GetEnvironmentVariable("CROSSWORLDS_GAME_SERVICE_TOKEN") ?? "";
    static string EquipmentAuthBaseUrl =>
        (Environment.GetEnvironmentVariable("CROSSWORLDS_AUTH_URL") ??
         "http://127.0.0.1:3000").TrimEnd('/');

    public override void OnStartServer()
    {
        base.OnStartServer();
        StartCoroutine(ServerLoadEquipmentAfterSpawn());
    }

    [Command]
    public void CmdRefreshEquipment()
    {
        StartCoroutine(ServerLoadEquipment());
    }

    [Server]
    IEnumerator ServerLoadEquipmentAfterSpawn()
    {
        yield return null;
        yield return ServerLoadEquipment();
    }

    [Server]
    IEnumerator ServerLoadEquipment()
    {
        if (characterId <= 0 || string.IsNullOrWhiteSpace(EquipmentServiceToken)) yield break;
        string url = $"{EquipmentAuthBaseUrl}/api/game/equipment/{characterId}";
        using var request = UnityWebRequest.Get(url);
        request.SetRequestHeader("X-Game-Server-Token", EquipmentServiceToken);
        request.timeout = 10;
        yield return request.SendWebRequest();
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogWarning($"[EQUIPMENT] Load failed for char#{characterId}: " +
                             $"{request.error} {request.downloadHandler.text}");
            yield break;
        }

        EquipmentResponse response;
        try { response = JsonUtility.FromJson<EquipmentResponse>(request.downloadHandler.text); }
        catch (Exception exception)
        {
            Debug.LogWarning($"[EQUIPMENT] Invalid response for char#{characterId}: {exception.Message}");
            yield break;
        }
        if (response?.success != true || response.data == null) yield break;

        equippedLoot.Clear();
        if (response.data.items != null)
        {
            foreach (EquippedItemDto item in response.data.items)
            {
                if (!TryParseSlot(item.equipped_slot, out LootEquipmentSlot slot)) continue;
                equippedLoot.Add(new EquippedLootState
                {
                    inventorySlotIndex = item.slot_index,
                    equipmentSlot = slot,
                    itemId = item.item_id
                });
            }
        }

        EquipmentBonusDto bonus = response.data.bonus ?? new EquipmentBonusDto();
        GetComponent<CharacterStats>()?.SetEquipmentStatBonuses(
            bonus.stat_str, bonus.stat_agi, bonus.stat_int, bonus.stat_vit);
        Debug.Log($"[EQUIPMENT] Loaded {equippedLoot.Count} slot(s) for char#{characterId}.");
    }

    void EquipmentOnStartClient()
    {
        equippedLoot.Callback -= OnEquipmentListChanged;
        equippedLoot.Callback += OnEquipmentListChanged;
        RebuildEquipmentVisuals();
    }

    void EquipmentOnStopClient()
    {
        equippedLoot.Callback -= OnEquipmentListChanged;
        ClearEquipmentVisuals();
    }

    void OnEquipmentListChanged(SyncList<EquippedLootState>.Operation operation,
        int index, EquippedLootState oldItem, EquippedLootState newItem)
    {
        RebuildEquipmentVisuals();
        EquipmentChanged?.Invoke();
    }

    public bool TryGetEquipped(LootEquipmentSlot slot, out EquippedLootState state)
    {
        foreach (EquippedLootState candidate in equippedLoot)
        {
            if (candidate.equipmentSlot != slot) continue;
            state = candidate;
            return true;
        }
        state = default;
        return false;
    }

    void RebuildEquipmentVisuals()
    {
#if UNITY_EDITOR || !UNITY_SERVER
        ClearEquipmentVisuals();
        foreach (EquippedLootState state in equippedLoot)
        {
            LootItemDefinition definition = LootItemCatalog.Find(state.itemId);
            if (definition == null) continue;
            GameObject prefab = definition.equippedVisualPrefab != null
                ? definition.equippedVisualPrefab
                : definition.worldVisualPrefab;
            if (prefab == null) continue;

            Transform anchor = ResolveEquipmentAnchor(definition, state.equipmentSlot);
            GameObject visual = Instantiate(prefab, anchor, false);
            visual.name = $"[Equipped] {state.itemId}";
            visual.transform.localPosition = definition.EffectiveEquippedLocalPosition;
            visual.transform.localRotation = Quaternion.Euler(definition.EffectiveEquippedLocalEulerAngles);
            Vector3 scale = definition.EffectiveEquippedLocalScale;
            visual.transform.localScale = scale.sqrMagnitude > 0.0001f ? scale : Vector3.one;
            DisablePickupBehaviour(visual);
            _equippedVisuals[state.equipmentSlot] = visual;
        }
#endif
    }

    void ClearEquipmentVisuals()
    {
#if UNITY_EDITOR || !UNITY_SERVER
        foreach (GameObject visual in _equippedVisuals.Values)
            if (visual != null) Destroy(visual);
        _equippedVisuals.Clear();
#endif
    }

#if UNITY_EDITOR || !UNITY_SERVER
    Transform ResolveEquipmentAnchor(LootItemDefinition definition, LootEquipmentSlot slot)
    {
        string attachmentBoneName = definition.EffectiveAttachmentBoneName;
        if (!string.IsNullOrWhiteSpace(attachmentBoneName))
        {
            Transform exact = FindTransform(transform, attachmentBoneName);
            if (exact != null) return exact;
        }

        string[] aliases = slot switch
        {
            LootEquipmentSlot.MainHand => new[] { "RightHand", "Hand_R", "mixamorig:RightHand", "Bip001 R Hand", "r_hand" },
            LootEquipmentSlot.OffHand => new[] { "LeftHand", "Hand_L", "mixamorig:LeftHand", "Bip001 L Hand", "l_hand" },
            LootEquipmentSlot.Head => new[] { "Head", "mixamorig:Head", "Bip001 Head", "head" },
            LootEquipmentSlot.Chest => new[] { "Chest", "UpperChest", "Spine2", "mixamorig:Spine2" },
            LootEquipmentSlot.Hands => new[] { "RightHand", "Hand_R", "mixamorig:RightHand" },
            LootEquipmentSlot.Legs => new[] { "Hips", "Pelvis", "mixamorig:Hips" },
            LootEquipmentSlot.Feet => new[] { "RightFoot", "Foot_R", "mixamorig:RightFoot" },
            _ => Array.Empty<string>()
        };
        foreach (string alias in aliases)
        {
            Transform found = FindTransform(transform, alias);
            if (found != null) return found;
        }
        Debug.LogWarning($"[EQUIPMENT] No {slot} anchor found on '{name}'; using player root.", this);
        return transform;
    }

    static Transform FindTransform(Transform root, string targetName)
    {
        foreach (Transform candidate in root.GetComponentsInChildren<Transform>(true))
            if (string.Equals(candidate.name, targetName, StringComparison.OrdinalIgnoreCase))
                return candidate;
        return null;
    }

    static void DisablePickupBehaviour(GameObject visual)
    {
        foreach (Collider collider in visual.GetComponentsInChildren<Collider>(true)) collider.enabled = false;
        foreach (Rigidbody body in visual.GetComponentsInChildren<Rigidbody>(true))
        {
            body.isKinematic = true;
            body.detectCollisions = false;
        }
        foreach (NetworkBehaviour behaviour in visual.GetComponentsInChildren<NetworkBehaviour>(true))
            behaviour.enabled = false;
    }
#endif

    static bool TryParseSlot(string value, out LootEquipmentSlot slot)
    {
        slot = value switch
        {
            "head" => LootEquipmentSlot.Head,
            "chest" => LootEquipmentSlot.Chest,
            "legs" => LootEquipmentSlot.Legs,
            "feet" => LootEquipmentSlot.Feet,
            "hands" => LootEquipmentSlot.Hands,
            "main_hand" => LootEquipmentSlot.MainHand,
            "off_hand" => LootEquipmentSlot.OffHand,
            "ring" => LootEquipmentSlot.Ring,
            "trinket" => LootEquipmentSlot.Trinket,
            _ => LootEquipmentSlot.None
        };
        return slot != LootEquipmentSlot.None;
    }

    [Serializable] sealed class EquipmentResponse { public bool success; public EquipmentData data; }
    [Serializable] sealed class EquipmentData
    {
        public EquippedItemDto[] items;
        public EquipmentBonusDto bonus;
    }
    [Serializable] sealed class EquippedItemDto
    {
        public int slot_index;
        public string item_id;
        public string equipped_slot;
    }
    [Serializable] sealed class EquipmentBonusDto
    {
        public int stat_str, stat_agi, stat_int, stat_vit;
    }
}
