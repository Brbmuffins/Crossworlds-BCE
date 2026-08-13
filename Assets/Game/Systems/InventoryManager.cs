#if UNITY_EDITOR || !UNITY_SERVER
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;

/// <summary>
/// InventoryManager — self-bootstrapping singleton.
/// Tracks local player's inventory in memory and syncs to auth server.
///
/// Server URL: http://{PlayerPrefs serverIP}:3000
/// Auth token: PlayerPrefs "jwt_token"
/// Character ID: AuthManager.CharacterId (set by LoginManager on login)
///
/// On scene load this auto-calls LoadInventory().
/// WorldItem.RpcOnPickedUp calls OnItemPickedUp() which immediately POSTs a save.
/// </summary>
public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    // Must match InventoryBagUI (4x6 grid) and the server-side add-item/craft caps.
    // Items placed beyond this are invisible in the bag UI, so keep all three in sync.
    private const int MaxSlots = 24;

    [System.Serializable]
    public class InventorySlot
    {
        public int    slot_index;
        public string item_id;
        public int    quantity;
        public int    equipped;
    }

    private List<InventorySlot> _slots = new List<InventorySlot>();

    // ── Bootstrap ─────────────────────────────────────────────────────────────
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Bootstrap()
    {
        if (Instance != null) return;
        var go = new GameObject("[InventoryManager]");
        DontDestroyOnLoad(go);
        Instance = go.AddComponent<InventoryManager>();
    }

    // ── Lifecycle ─────────────────────────────────────────────────────────────
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start() => StartCoroutine(LoadInventory());

    void OnDestroy() { if (Instance == this) Instance = null; }

    // ── Public API ────────────────────────────────────────────────────────────

    public void OnItemPickedUp(string itemId, int qty)
    {
        if (string.IsNullOrWhiteSpace(itemId) || qty <= 0) return;

        if (itemId.StartsWith("gold:"))
        {
            if (int.TryParse(itemId.Substring(5), out int gold))
            {
                Debug.Log($"[LOOT] Picked up {gold} gold");
                PlayerProgressManager.Local?.AwardGold(gold);
            }
            return;
        }

        LootItemDefinition definition = LootItemCatalog.Find(itemId);
        bool stackable = definition == null || definition.stackable;
        int maxStack = stackable
            ? Mathf.Max(1, definition != null ? definition.maxStackSize : 99)
            : 1;
        int remaining = qty;

        if (stackable)
        {
            foreach (var slot in _slots)
            {
                if (remaining <= 0) break;
                if (slot.item_id != itemId || slot.equipped != 0 || slot.quantity >= maxStack) continue;
                int added = Mathf.Min(remaining, maxStack - slot.quantity);
                slot.quantity += added;
                remaining -= added;
            }
        }

        while (remaining > 0)
        {
            int next = FindNextFreeSlot();
            if (next < 0)
            {
                Debug.LogWarning($"[LOOT] Inventory full — {remaining}x {itemId} could not be stored");
                break;
            }
            int added = Mathf.Min(remaining, maxStack);
            _slots.Add(new InventorySlot
                { slot_index = next, item_id = itemId, quantity = added, equipped = 0 });
            remaining -= added;
        }

        int stored = qty - remaining;
        if (stored <= 0) return;
        Debug.Log($"[LOOT] Picked up {stored}x {itemId}");
        StartCoroutine(SaveInventory());
    }

    /// <summary>
    /// Persists world loot without rewriting equipped/reserved inventory rows.
    /// Returns the quantity the server actually stored.
    /// </summary>
    public void PersistWorldPickup(string itemId, int qty, Action<int> completed)
    {
        StartCoroutine(PersistWorldPickupRequest(itemId, qty, completed));
    }

    IEnumerator PersistWorldPickupRequest(string itemId, int qty, Action<int> completed)
    {
        int charId = AuthManager.CharacterId;
        string token = !string.IsNullOrEmpty(AuthManager.Token)
            ? AuthManager.Token
            : PlayerPrefs.GetString("jwt_token", "");
        if (charId <= 0 || string.IsNullOrEmpty(token))
        {
            Debug.LogWarning("[LOOT] Pickup persistence skipped: auth not ready");
            completed?.Invoke(0);
            yield break;
        }

        string url = $"{ServerConfig.AuthBaseUrl}/api/inventory/add-item";
        string json = JsonUtility.ToJson(new InventoryAddPayload
            { characterId = charId, itemId = itemId, quantity = Mathf.Max(1, qty) });
        using var req = new UnityWebRequest(url, "POST");
        req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        req.SetRequestHeader("Authorization", $"Bearer {token}");
        req.timeout = 10;
        yield return req.SendWebRequest();

        int stored = 0;
        if (req.result == UnityWebRequest.Result.Success)
        {
            InventoryAddResponse response =
                JsonUtility.FromJson<InventoryAddResponse>(req.downloadHandler.text);
            if (response?.success == true && response.data != null)
                stored = Mathf.Clamp(response.data.stored, 0, Mathf.Max(1, qty));
        }

        if (stored > 0)
        {
            Debug.Log($"[LOOT] Persisted {stored}x {itemId}");
            yield return LoadInventory();
            InventoryBagUI.Refresh();
        }
        else
        {
            string detail = req.downloadHandler?.text;
            Debug.LogWarning(
                $"[LOOT] Pickup persistence failed ({req.responseCode}) for {itemId}: " +
                (string.IsNullOrWhiteSpace(detail) ? req.error : detail));
        }

        completed?.Invoke(stored);
    }

    public List<InventorySlot> GetSlots() => new List<InventorySlot>(_slots);

    /// Total unequipped quantity of an item across all slots (recipe ingredient checks).
    public int GetItemCount(string itemId)
    {
        int total = 0;
        foreach (var s in _slots)
            if (s.item_id == itemId && s.equipped == 0) total += s.quantity;
        return total;
    }

    /// <summary>
    /// Mark an item equipped/unequipped in local cache and POST to /api/inventory/equip.
    /// Used by server-backed bag/equipment flows.
    /// </summary>
    public void OnItemEquipped(string itemId, bool equipped)
    {
        if (string.IsNullOrEmpty(itemId)) return;

        var slot = _slots.Find(s => s.item_id == itemId && (equipped ? s.equipped == 0 : s.equipped == 1));
        if (slot == null)
        {
            Debug.LogWarning($"[LOOT] OnItemEquipped: no matching slot for {itemId} (equipped={equipped})");
            return;
        }

        slot.equipped = equipped ? 1 : 0;
        StartCoroutine(PostEquip(slot.slot_index, slot.equipped));
    }

    IEnumerator PostEquip(int slotIndex, int equippedFlag)
    {
        int charId   = AuthManager.CharacterId;
        string token = AuthManager.Token;
        if (charId <= 0 || string.IsNullOrEmpty(token)) yield break;

        string url  = $"{ServerConfig.AuthBaseUrl}/api/inventory/equip";
        string json = $"{{\"characterId\":{charId},\"slot_index\":{slotIndex},\"equipped\":{equippedFlag}}}";

        using var req = new UnityWebRequest(url, "POST");
        req.uploadHandler   = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        req.SetRequestHeader("Authorization", $"Bearer {token}");
        req.timeout = 8;
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
            Debug.LogWarning($"[LOOT] Equip POST failed: {req.error}");
        else
            Debug.Log($"[LOOT] Slot {slotIndex} equipped={equippedFlag} saved");
    }

    // ── Load ─────────────────────────────────────────────────────────────────

    public IEnumerator LoadInventory()
    {
        int charId   = AuthManager.CharacterId;
        string token = !string.IsNullOrEmpty(AuthManager.Token)
            ? AuthManager.Token
            : PlayerPrefs.GetString("jwt_token", "");

        float waited = 0f;
        while ((charId <= 0 || string.IsNullOrEmpty(token)) && waited < 8f)
        {
            yield return new WaitForSeconds(0.25f);
            waited += 0.25f;
            charId = AuthManager.CharacterId;
            token = !string.IsNullOrEmpty(AuthManager.Token)
                ? AuthManager.Token
                : PlayerPrefs.GetString("jwt_token", "");
        }

        if (charId <= 0 || string.IsNullOrEmpty(token))
        {
            Debug.LogWarning("[LOOT] LoadInventory: auth not ready");
            yield break;
        }

        string url = $"{ServerConfig.AuthBaseUrl}/api/inventory/{charId}";
        using var req = UnityWebRequest.Get(url);
        req.SetRequestHeader("Authorization", $"Bearer {token}");
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        { Debug.LogError($"[LOOT] LoadInventory failed: {req.error}"); yield break; }

        var response = JsonUtility.FromJson<InventoryResponse>(req.downloadHandler.text);
        if (response?.success == true)
        {
            _slots = response.data ?? new List<InventorySlot>();
            Debug.Log($"[LOOT] Loaded {_slots.Count} slots for char#{charId}");
        }
    }

    // ── Save ─────────────────────────────────────────────────────────────────

    public IEnumerator SaveInventory()
    {
        int charId   = AuthManager.CharacterId;
        string token = AuthManager.Token;
        if (charId <= 0 || string.IsNullOrEmpty(token)) { Debug.LogWarning("[LOOT] SaveInventory: auth not ready"); yield break; }

        string url  = $"{ServerConfig.AuthBaseUrl}/api/inventory/save";
        string json = JsonUtility.ToJson(new InventorySavePayload { characterId = charId, slots = _slots });

        using var req = new UnityWebRequest(url, "POST");
        req.uploadHandler   = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        req.SetRequestHeader("Authorization", $"Bearer {token}");
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
            Debug.LogError($"[LOOT] SaveInventory failed: {req.error}");
        else
            Debug.Log($"[LOOT] Inventory saved — {_slots.Count} slots");
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    int FindNextFreeSlot()
    {
        var used = new HashSet<int>();
        foreach (var s in _slots) used.Add(s.slot_index);
        for (int i = 0; i < MaxSlots; i++) if (!used.Contains(i)) return i;
        return -1;
    }

    [System.Serializable] class InventoryResponse  { public bool success; public List<InventorySlot> data; public string error; }
    [System.Serializable] class InventorySavePayload { public int characterId; public List<InventorySlot> slots; }
    [System.Serializable] class InventoryAddPayload { public int characterId; public string itemId; public int quantity; }
    [System.Serializable] class InventoryAddResponse { public bool success; public InventoryAddResult data; public string error; }
    [System.Serializable] class InventoryAddResult { public int stored; public int rejected; }
}

#endif

// ── AuthManager stub ──────────────────────────────────────────────────────────
// Compiled in ALL build targets (client + server) so every script can reference it.
// If you add a real AuthManager, define AUTHMANAGER_EXISTS in Player → Scripting Defines.
#if !AUTHMANAGER_EXISTS
public static class AuthManager
{
    public static string Token       { get; set; } = "";
    public static int    CharacterId { get; set; } = 0;
}
#endif
