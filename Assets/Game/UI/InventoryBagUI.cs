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
    public event Action EquipmentChanged;

    public readonly struct EquippedItemSnapshot
    {
        public readonly int InventorySlotIndex;
        public readonly string ItemId;
        public readonly int Quantity;
        public readonly string ServerRarity;

        public EquippedItemSnapshot(int inventorySlotIndex, string itemId, int quantity, string serverRarity)
        {
            InventorySlotIndex = inventorySlotIndex;
            ItemId = itemId;
            Quantity = quantity;
            ServerRarity = serverRarity;
        }
    }

    InventoryBagView _view;
    InventoryWindowDragHandle _dragHandle;
    InventorySlotData[] _data = new InventorySlotData[TotalSlots];
    InventoryFilter _filter;
    bool _open;
    bool _loading;
    bool _refreshQueued;
    bool _progressSubscribed;
    bool _deleting;
    Image _dragIcon;
    GameObject _deleteConfirmation;
    InventorySlotData _pendingDelete;

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
        if (_instance == null) return;
        if (_instance._loading)
        {
            _instance._refreshQueued = true;
            return;
        }
        _instance.StartCoroutine(_instance.FetchInventory());
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
        _dragHandle = _view.GetComponentInChildren<InventoryWindowDragHandle>(true);
        _view.Initialize(HidePanel, SetFilter, OnSlotClicked, OnSlotEnter, OnSlotExit,
            OnSlotBeginDrag, OnSlotDrag, OnSlotEndDrag);
    }

    void Toggle()
    {
        if (_view == null) CreateView();
        if (_view == null) return;
        _open = !_open;
        _view.gameObject.SetActive(_open);
        if (_open)
        {
            _dragHandle?.ApplySavedPosition();
            RefreshGold();
            StartCoroutine(FetchInventory());
        }
        else ItemTooltipUI.Instance?.Hide();
    }

    void HidePanel()
    {
        _open = false;
        ItemTooltipUI.Instance?.Hide();
        ClearDragIcon();
        CloseDeleteConfirmation();
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
                    EquipmentChanged?.Invoke();
                }
            }
            catch (Exception exception) { _view.SetStatus($"Parse error: {exception.Message}"); }
        }
        _loading = false;
        if (_refreshQueued)
        {
            _refreshQueued = false;
            StartCoroutine(FetchInventory());
        }
    }

    public List<EquippedItemSnapshot> GetEquippedItems()
    {
        var result = new List<EquippedItemSnapshot>();
        foreach (var slot in _data)
            if (slot != null && slot.equipped == 1 && !string.IsNullOrEmpty(slot.item_id))
                result.Add(new EquippedItemSnapshot(slot.slot_index, slot.item_id, slot.quantity, slot.rarity));
        result.Sort((a, b) => a.InventorySlotIndex.CompareTo(b.InventorySlotIndex));
        return result;
    }

    public void UnequipInventorySlot(int inventorySlotIndex)
    {
        if (inventorySlotIndex < 0 || inventorySlotIndex >= _data.Length) return;
        var slot = _data[inventorySlotIndex];
        if (slot == null || slot.equipped != 1) return;
        StartCoroutine(PostEquip(slot, false));
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

    void OnSlotBeginDrag(int visibleIndex, PointerEventData eventData)
    {
        if (_deleting || _deleteConfirmation != null) return;
        var slot = VisibleSlot(visibleIndex);
        if (slot == null) return;

        ItemTooltipUI.Instance?.Hide();
        ClearDragIcon();
        var definition = LootItemCatalog.Find(slot.item_id);
        var go = new GameObject("DraggedInventoryItem", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
        go.transform.SetParent(_view.transform, false);
        go.transform.SetAsLastSibling();
        var rect = go.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(58f, 58f);
        _dragIcon = go.GetComponent<Image>();
        _dragIcon.sprite = definition != null ? definition.inventoryIcon : null;
        _dragIcon.preserveAspect = true;
        _dragIcon.color = _dragIcon.sprite != null ? Color.white : LootItemCatalog.RarityColor(ResolveRarity(slot));
        go.GetComponent<CanvasGroup>().blocksRaycasts = false;
        rect.position = eventData.position;
    }

    void OnSlotDrag(int visibleIndex, PointerEventData eventData)
    {
        if (_dragIcon != null) _dragIcon.rectTransform.position = eventData.position;
    }

    void OnSlotEndDrag(int visibleIndex, PointerEventData eventData)
    {
        var slot = VisibleSlot(visibleIndex);
        ClearDragIcon();
        if (slot == null || _dragHandle == null || _dragHandle.panel == null) return;
        if (RectTransformUtility.RectangleContainsScreenPoint(_dragHandle.panel, eventData.position, eventData.pressEventCamera))
            return;

        if (ResolveRarity(slot) == ItemRarity.Common)
            StartCoroutine(DeleteSlot(slot));
        else
            ShowDeleteConfirmation(slot);
    }

    ItemRarity ResolveRarity(InventorySlotData slot)
    {
        var definition = LootItemCatalog.Find(slot.item_id);
        if (definition != null) return definition.rarity;
        return Enum.TryParse(slot.rarity, true, out ItemRarity rarity) ? rarity : ItemRarity.Common;
    }

    void ClearDragIcon()
    {
        if (_dragIcon != null) Destroy(_dragIcon.gameObject);
        _dragIcon = null;
    }

    void ShowDeleteConfirmation(InventorySlotData slot)
    {
        CloseDeleteConfirmation();
        _pendingDelete = slot;

        var overlay = new GameObject("DeleteItemConfirmation", typeof(RectTransform), typeof(Image));
        overlay.transform.SetParent(_view.transform, false);
        overlay.transform.SetAsLastSibling();
        var overlayRect = overlay.GetComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = overlayRect.offsetMax = Vector2.zero;
        overlay.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.62f);
        _deleteConfirmation = overlay;

        var box = new GameObject("Dialog", typeof(RectTransform), typeof(Image));
        box.transform.SetParent(overlay.transform, false);
        var boxRect = box.GetComponent<RectTransform>();
        boxRect.anchorMin = boxRect.anchorMax = new Vector2(0.5f, 0.5f);
        boxRect.sizeDelta = new Vector2(430f, 180f);
        box.GetComponent<Image>().color = new Color(0.055f, 0.045f, 0.07f, 0.98f);

        var message = CreateDialogText("Message", box.transform,
            "Are you sure you want to delete this item?", 22f, FontStyles.Bold);
        var messageRect = message.rectTransform;
        messageRect.anchorMin = new Vector2(0f, 0.42f);
        messageRect.anchorMax = new Vector2(1f, 1f);
        messageRect.offsetMin = new Vector2(28f, 0f);
        messageRect.offsetMax = new Vector2(-28f, -18f);

        CreateDialogButton("Cancel", box.transform, new Vector2(-92f, 30f), CloseDeleteConfirmation,
            new Color(0.22f, 0.20f, 0.25f, 1f));
        CreateDialogButton("Delete", box.transform, new Vector2(92f, 30f), ConfirmDelete,
            new Color(0.58f, 0.12f, 0.12f, 1f));
    }

    TextMeshProUGUI CreateDialogText(string name, Transform parent, string value, float fontSize, FontStyles style)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        var text = go.GetComponent<TextMeshProUGUI>();
        text.text = value;
        text.fontSize = fontSize;
        text.fontStyle = style;
        text.color = new Color(0.95f, 0.82f, 0.42f, 1f);
        text.alignment = TextAlignmentOptions.Center;
        text.raycastTarget = false;
        return text;
    }

    void CreateDialogButton(string label, Transform parent, Vector2 position, Action action, Color color)
    {
        var go = new GameObject(label + "Button", typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(150f, 42f);
        var image = go.GetComponent<Image>();
        image.color = color;
        var button = go.GetComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(() => action());
        var text = CreateDialogText("Label", go.transform, label, 18f, FontStyles.Bold);
        text.color = Color.white;
        text.rectTransform.anchorMin = Vector2.zero;
        text.rectTransform.anchorMax = Vector2.one;
        text.rectTransform.offsetMin = text.rectTransform.offsetMax = Vector2.zero;
    }

    void ConfirmDelete()
    {
        var slot = _pendingDelete;
        CloseDeleteConfirmation();
        if (slot != null) StartCoroutine(DeleteSlot(slot));
    }

    void CloseDeleteConfirmation()
    {
        if (_deleteConfirmation != null) Destroy(_deleteConfirmation);
        _deleteConfirmation = null;
        _pendingDelete = null;
    }

    IEnumerator DeleteSlot(InventorySlotData slot)
    {
        if (_deleting || slot == null) yield break;
        string characterId = GetCharacterId();
        if (string.IsNullOrEmpty(characterId)) yield break;
        _deleting = true;
        _view.SetStatus("Deleting item...");
        string token = !string.IsNullOrEmpty(AuthManager.Token) ? AuthManager.Token : PlayerPrefs.GetString("jwt_token", "");
        string body = $"{{\"characterId\":{characterId},\"slot_index\":{slot.slot_index}}}";
        using var request = new UnityWebRequest($"{ServerConfig.AuthBaseUrl}/api/inventory/delete", "POST");
        request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(body));
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", $"Bearer {token}");
        yield return request.SendWebRequest();
        _deleting = false;

        if (request.result == UnityWebRequest.Result.Success)
        {
            if (slot.slot_index >= 0 && slot.slot_index < _data.Length) _data[slot.slot_index] = null;
            RenderSlots();
            _view.SetStatus("");
            StartCoroutine(FetchInventory());
        }
        else
        {
            string message = request.downloadHandler != null ? request.downloadHandler.text : request.error;
            _view.SetStatus($"Delete failed: {message}");
        }
    }

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
            EquipmentChanged?.Invoke();
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
    [Serializable] sealed class InventorySlotData
    {
        public int slot_index;
        public string item_id;
        public int quantity;
        public int equipped;
        public string rarity;
    }
    [Serializable] sealed class SavePayload { public int characterId; public List<InventorySlotData> slots; }
}
#endif
