#if UNITY_EDITOR || !UNITY_SERVER
using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;

/// <summary>Client inventory behaviour. Presentation lives in Resources/Inventory/InventoryWindow.prefab.</summary>
public sealed class InventoryBagUI : MonoBehaviour
{
    const int TotalSlots = 24;
    static InventoryBagUI _instance;
    public static InventoryBagUI Instance => _instance;

    InventoryBagView _view;
    InventorySlotData[] _data = new InventorySlotData[TotalSlots];
    InventoryFilter _filter;
    bool _open;
    bool _loading;
    bool _progressSubscribed;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Bootstrap()
    {
        if (_instance != null) return;
        var go = new GameObject("[InventoryBagUI]");
        DontDestroyOnLoad(go);
        _instance = go.AddComponent<InventoryBagUI>();
    }

    void Awake()
    {
        if (_instance != null && _instance != this) { Destroy(gameObject); return; }
        _instance = this;
        CreateView();
        HidePanel();
    }

    void OnDisable()
    {
        if (_progressSubscribed && PlayerProgressManager.Local != null)
            PlayerProgressManager.Local.OnDataRefreshed -= RefreshGold;
        _progressSubscribed = false;
    }

    void Update()
    {
        if (!_progressSubscribed && PlayerProgressManager.Local != null)
        {
            PlayerProgressManager.Local.OnDataRefreshed += RefreshGold;
            _progressSubscribed = true;
            RefreshGold();
        }

        var keyboard = UnityEngine.InputSystem.Keyboard.current;
        if (keyboard == null) return;
        if (keyboard.bKey.wasPressedThisFrame && !AnyInputFocused()) Toggle();
        else if (_open && keyboard.escapeKey.wasPressedThisFrame && !AnyInputFocused()) HidePanel();
    }

    public static void Refresh()
    {
        if (_instance != null) _instance.StartCoroutine(_instance.FetchInventory());
    }

    void CreateView()
    {
        var prefab = Resources.Load<InventoryBagView>("Inventory/InventoryWindow");
        if (prefab == null)
        {
            Debug.LogError("[INVENTORY] Missing Resources/Inventory/InventoryWindow prefab. Run BCE/Setup/Rebuild Inventory UI.");
            return;
        }
        _view = Instantiate(prefab, transform);
        _view.name = "InventoryWindow";
        _view.Initialize(HidePanel, SetFilter, OnSlotClicked, OnSlotEnter, OnSlotExit);
    }

    void Toggle()
    {
        if (_view == null) CreateView();
        if (_view == null) return;
        _open = !_open;
        _view.gameObject.SetActive(_open);
        if (_open)
        {
            RefreshGold();
            StartCoroutine(FetchInventory());
        }
        else ItemTooltipUI.Instance?.Hide();
    }

    void HidePanel()
    {
        _open = false;
        ItemTooltipUI.Instance?.Hide();
        if (_view != null) _view.gameObject.SetActive(false);
    }

    void SetFilter(InventoryFilter filter)
    {
        _filter = filter;
        _view.SetActiveFilter(filter);
        RenderSlots();
    }

    IEnumerator FetchInventory()
    {
        if (_loading || _view == null) yield break;
        _loading = true;
        _view.SetStatus("Loading...");
        string characterId = GetCharacterId();
        if (string.IsNullOrEmpty(characterId))
        {
            _view.SetStatus("No character loaded.");
            _loading = false;
            yield break;
        }

        string token = !string.IsNullOrEmpty(AuthManager.Token) ? AuthManager.Token : PlayerPrefs.GetString("jwt_token", "");
        using var request = UnityWebRequest.Get($"{ServerConfig.AuthBaseUrl}/api/inventory/{characterId}");
        request.SetRequestHeader("Authorization", $"Bearer {token}");
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
            _view.SetStatus($"Error: {request.error}");
        else
        {
            try
            {
                var response = JsonUtility.FromJson<InventoryResponse>(request.downloadHandler.text);
                if (!response.success) _view.SetStatus(response.error);
                else
                {
                    _data = new InventorySlotData[TotalSlots];
                    if (response.data != null)
                        foreach (var slot in response.data)
                            if (slot.slot_index >= 0 && slot.slot_index < TotalSlots) _data[slot.slot_index] = slot;
                    RenderSlots();
                    _view.SetStatus("");
                }
            }
            catch (Exception exception) { _view.SetStatus($"Parse error: {exception.Message}"); }
        }
        _loading = false;
    }

    void RenderSlots()
    {
        if (_view == null) return;
        var visible = new List<InventorySlotData>(TotalSlots);
        foreach (var slot in _data)
            if (slot != null && MatchesFilter(slot.item_id)) visible.Add(slot);

        for (int i = 0; i < TotalSlots; i++)
        {
            if (i >= visible.Count) _view.SetSlot(i, null, 0, false, Color.clear);
            else
            {
                var slot = visible[i];
                var definition = LootItemCatalog.Find(slot.item_id);
                _view.SetSlot(i, definition != null ? definition.inventoryIcon : null, slot.quantity, slot.equipped == 1,
                    definition != null ? LootItemCatalog.RarityColor(definition.rarity) : ItemCatalogManager.GetRarityColor(slot.item_id));
            }
        }
    }

    bool MatchesFilter(string itemId)
    {
        if (_filter == InventoryFilter.All) return true;
        var definition = LootItemCatalog.Find(itemId);
        string type = definition != null ? definition.databaseItemType.ToString() : ItemCatalogManager.Instance?.GetTemplate(itemId)?.item_type;
        bool material = !string.IsNullOrEmpty(type) && type.IndexOf("material", StringComparison.OrdinalIgnoreCase) >= 0;
        if (_filter == InventoryFilter.Materials) return material;
        if (string.IsNullOrEmpty(type)) return false;
        string normalized = type.Replace("_", "").ToLowerInvariant();
        return normalized.Contains("weapon") || normalized.Contains("armor") || normalized.Contains("offhand") ||
               normalized.Contains("ring") || normalized.Contains("trinket") || normalized.Contains("gear");
    }

    InventorySlotData VisibleSlot(int visibleIndex)
    {
        int cursor = 0;
        foreach (var slot in _data)
        {
            if (slot == null || !MatchesFilter(slot.item_id)) continue;
            if (cursor++ == visibleIndex) return slot;
        }
        return null;
    }

    void OnSlotClicked(int visibleIndex)
    {
        var slot = VisibleSlot(visibleIndex);
        if (slot == null) return;
        if (ConsumableEffect.IsConsumable(slot.item_id)) StartCoroutine(UseConsumable(slot));
        else StartCoroutine(PostEquip(slot, slot.equipped == 0));
    }

    void OnSlotEnter(int visibleIndex, PointerEventData eventData)
    {
        var slot = VisibleSlot(visibleIndex);
        if (slot != null) ItemTooltipUI.Instance?.Show(slot.item_id, eventData.position);
    }

    void OnSlotExit() => ItemTooltipUI.Instance?.Hide();

    IEnumerator PostEquip(InventorySlotData slot, bool equip)
    {
        string characterId = GetCharacterId();
        if (string.IsNullOrEmpty(characterId)) yield break;
        string token = !string.IsNullOrEmpty(AuthManager.Token) ? AuthManager.Token : PlayerPrefs.GetString("jwt_token", "");
        string body = $"{{\"characterId\":{characterId},\"slot_index\":{slot.slot_index},\"equipped\":{(equip ? 1 : 0)}}}";
        using var request = new UnityWebRequest($"{ServerConfig.AuthBaseUrl}/api/inventory/equip", "POST");
        request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(body));
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", $"Bearer {token}");
        yield return request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.Success)
        {
            slot.equipped = equip ? 1 : 0;
            RenderSlots();
            StartCoroutine(FetchInventory());
        }
        else _view.SetStatus($"Equip failed: {request.error}");
    }

    IEnumerator UseConsumable(InventorySlotData slot)
    {
        var player = NetworkClient.localPlayer != null ? NetworkClient.localPlayer.gameObject : null;
        if (player == null) { _view.SetStatus("No player to use item on."); yield break; }
        if (!ConsumableEffect.Apply(slot.item_id, player)) { _view.SetStatus("That item can't be used."); yield break; }
        slot.quantity--;
        if (slot.quantity <= 0) _data[slot.slot_index] = null;
        RenderSlots();
        yield return StartCoroutine(PostSaveAll());
        StartCoroutine(FetchInventory());
    }

    IEnumerator PostSaveAll()
    {
        string characterId = GetCharacterId();
        if (!int.TryParse(characterId, out int parsedId)) yield break;
        var slots = new List<InventorySlotData>();
        foreach (var slot in _data) if (slot != null && !string.IsNullOrEmpty(slot.item_id)) slots.Add(slot);
        string body = JsonUtility.ToJson(new SavePayload { characterId = parsedId, slots = slots });
        string token = !string.IsNullOrEmpty(AuthManager.Token) ? AuthManager.Token : PlayerPrefs.GetString("jwt_token", "");
        using var request = new UnityWebRequest($"{ServerConfig.AuthBaseUrl}/api/inventory/save", "POST");
        request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(body));
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", $"Bearer {token}");
        yield return request.SendWebRequest();
        if (request.result != UnityWebRequest.Result.Success) _view.SetStatus($"Save failed: {request.error}");
    }

    void RefreshGold()
    {
        if (_view != null) _view.SetGold(PlayerProgressManager.Local != null ? PlayerProgressManager.Local.Gold : 0);
    }

    static string GetCharacterId()
    {
        if (AuthManager.CharacterId > 0) return AuthManager.CharacterId.ToString();
        foreach (var identity in FindObjectsByType<PlayerIdentity>(FindObjectsInactive.Exclude))
            if (identity.isLocalPlayer) return identity.characterId.ToString();
        return null;
    }

    static bool AnyInputFocused()
    {
        if (RodChatManager.Instance != null && RodChatManager.Instance.IsOpen) return true;
        foreach (var field in FindObjectsByType<TMP_InputField>(FindObjectsInactive.Exclude)) if (field.isFocused) return true;
        return false;
    }

    [Serializable] sealed class InventoryResponse { public bool success; public string error; public InventorySlotData[] data; }
    [Serializable] sealed class InventorySlotData { public int slot_index; public string item_id; public int quantity; public int equipped; }
    [Serializable] sealed class SavePayload { public int characterId; public List<InventorySlotData> slots; }
}
#endif
