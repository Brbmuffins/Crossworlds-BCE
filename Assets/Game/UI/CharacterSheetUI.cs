#if UNITY_EDITOR || !UNITY_SERVER
using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>Prefab-backed client Character window. Uses inventory's authenticated equipment bridge.</summary>
public sealed class CharacterSheetUI : MonoBehaviour
{
    static CharacterSheetUI _instance;
    public static CharacterSheetUI Instance => _instance;
    CharacterWindowView _view;
    CharacterModelPreview _modelPreview;
    CharacterWindowDragHandle _dragHandle;
    RectTransform _scaledPanel;
    readonly Dictionary<CharacterEquipmentSlot, InventoryBagUI.EquippedItemSnapshot> _equipped = new();
    PlayerProgressManager _progress;
    InventoryBagUI _inventory;
    PlayerIdentity _identity;
    bool _open;
    float _nextLiveRefresh;
    UnityEngine.UI.Image _dragIcon;
    GameObject _dragLayer;
    Vector2 _equipmentDragStart;
    Coroutine _pendingPreviewRefresh;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        if (_instance != null) return;
        var go = new GameObject("[CharacterSheetUI]");
        DontDestroyOnLoad(go);
        _instance = go.AddComponent<CharacterSheetUI>();
    }

    void Awake()
    {
        if (_instance != null && _instance != this) { Destroy(gameObject); return; }
        _instance = this;
        CreateView();
        InterfaceScaleSettings.Changed += ApplyInterfaceScale;
        ApplyInterfaceScale(InterfaceScaleSettings.Scale);
        Hide();
    }

    void OnEnable() => StartCoroutine(BindSources());

    void OnDisable()
    {
        if (_progress != null) _progress.OnDataRefreshed -= RefreshAll;
        if (_inventory != null) _inventory.EquipmentChanged -= RefreshEquipment;
        if (_identity != null) _identity.EquipmentChanged -= RefreshEquipment;
        _progress = null;
        _inventory = null;
        _identity = null;
    }

    void OnDestroy()
    {
        InterfaceScaleSettings.Changed -= ApplyInterfaceScale;
        _modelPreview?.Dispose();
        _modelPreview = null;
    }

    IEnumerator BindSources()
    {
        while (PlayerProgressManager.Local == null || InventoryBagUI.Instance == null) yield return null;
        _progress = PlayerProgressManager.Local;
        _inventory = InventoryBagUI.Instance;
        _progress.OnDataRefreshed -= RefreshAll;
        _progress.OnDataRefreshed += RefreshAll;
        _inventory.EquipmentChanged -= RefreshEquipment;
        _inventory.EquipmentChanged += RefreshEquipment;
        BindIdentity(FindLocalIdentity());
        RefreshAll();
    }

    void Update()
    {
        if (!HasGameplayPlayer())
        {
            if (_open) Hide();
            return;
        }

        BindIdentity(FindLocalIdentity());
        var keyboard = UnityEngine.InputSystem.Keyboard.current;
        if (keyboard != null)
        {
            if (keyboard.cKey.wasPressedThisFrame && !AnyInputFocused()) Toggle();
            else if (_open && keyboard.escapeKey.wasPressedThisFrame && !AnyInputFocused()) Hide();
        }

        if (_open && Time.unscaledTime >= _nextLiveRefresh)
        {
            _nextLiveRefresh = Time.unscaledTime + 0.25f;
            RefreshIdentityAndStats();
        }
        if (_open) _modelPreview?.RenderFrame();
    }

    static bool HasGameplayPlayer() => NetworkClient.active && NetworkClient.localPlayer != null;

    void CreateView()
    {
        var prefab = Resources.Load<CharacterWindowView>("Character/CharacterWindow");
        if (prefab == null)
        {
            Debug.LogError("[CHARACTER] Missing Resources/Character/CharacterWindow.prefab. Run BCE/Setup/Rebuild Character UI.");
            return;
        }
        _view = Instantiate(prefab, transform);
        _view.name = "CharacterWindow";
        _view.Initialize(Hide, OnEquipmentClicked, OnEquipmentEnter,
            () => ItemTooltipUI.Instance?.Hide(), OnEquipmentBeginDrag,
            OnEquipmentDrag, OnEquipmentEndDrag);
        RectTransform panel = _view.transform.Find("Panel") as RectTransform;
        if (panel != null)
        {
            _scaledPanel = panel;
            ApplyInterfaceScale(InterfaceScaleSettings.Scale);
            _dragHandle = panel.GetComponent<CharacterWindowDragHandle>() ?? panel.gameObject.AddComponent<CharacterWindowDragHandle>();
            _dragHandle.panel = panel;
        }
        _modelPreview = new CharacterModelPreview(_view.characterPreview);
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
            RefreshAll();
            InventoryBagUI.Refresh();
        }
        else ItemTooltipUI.Instance?.Hide();
    }

    void ApplyInterfaceScale(float scale)
    {
        if (_scaledPanel != null)
            _scaledPanel.localScale = Vector3.one * Mathf.Clamp(scale,
                InterfaceScaleSettings.Minimum, InterfaceScaleSettings.Maximum);
    }

    void Hide()
    {
        _open = false;
        ItemTooltipUI.Instance?.Hide();
        if (_view != null) _view.gameObject.SetActive(false);
    }

    void RefreshAll()
    {
        if (!_open || _view == null) return;
        RefreshIdentityAndStats();
        RefreshEquipment();
    }

    void RefreshIdentityAndStats()
    {
        if (!_open || _view == null) return;
        PlayerIdentity identity = FindLocalIdentity();
        var progress = PlayerProgressManager.Local;
        _view.playerName.text = identity != null ? identity.playerName : PlayerPrefs.GetString("username", "Player");
        _view.playerLevel.text = progress != null ? $"Level {progress.Level}" : "Level —";
        _view.className.text = identity != null ? identity.ClassName : "—";
        GameObject player = NetworkClient.localPlayer != null
            ? NetworkClient.localPlayer.gameObject
            : identity != null ? identity.gameObject : null;
        CharacterStats stats = player != null ? player.GetComponent<CharacterStats>() : null;
        if (stats != null)
        {
            _view.strValue.text = stats.EffectiveStrength.ToString();
            _view.agiValue.text = stats.EffectiveAgility.ToString();
            _view.intValue.text = stats.EffectiveIntelligence.ToString();
            _view.vitValue.text = stats.EffectiveVitality.ToString();
        }
        else if (progress != null)
        {
            _view.strValue.text = progress.StatStr.ToString();
            _view.agiValue.text = progress.StatAgi.ToString();
            _view.intValue.text = progress.StatInt.ToString();
            _view.vitValue.text = progress.StatVit.ToString();
        }

        _modelPreview?.Refresh(player);
        Health health = player != null ? player.GetComponent<Health>() : null;
        SetCombat(0, health != null ? $"{health.maxHealth:0}" : "—");
        SetCombat(1, stats != null ? $"{stats.MaxMana:0}" : "—");
        SetCombat(2, stats != null ? $"{stats.DamageMultiplier * 100f:0}%" : "—");
        SetCombat(3, stats != null ? $"{stats.CriticalStrikeChance * 100f:0.#}%" : "—");
        SetCombat(4, stats != null ? $"{stats.CriticalStrikeDamageMultiplier * 100f:0}%" : "—");
        SetCombat(5, stats != null ? $"{stats.DamageReduction * 100f:0.#}%" : "—");
        SetCombat(6, stats != null ? $"{stats.Hp5:0.#}" : "—");
        SetCombat(7, stats != null ? $"{stats.Mp5:0.#}" : "—");
        SetCombat(8, stats != null ? $"{stats.MoveSpeedMultiplier * 100f:0}%" : "—");
        SetCombat(9, stats != null ? $"{stats.EffectiveCooldownReduction * 100f:0.#}%" : "—");
    }

    void SetCombat(int index, string value)
    {
        if (_view.combatValues != null && index >= 0 && index < _view.combatValues.Length)
            _view.combatValues[index].text = value;
    }

    void RefreshEquipment()
    {
        if (!_open || _view == null) return;
        _equipped.Clear();
        int ringOrdinal = 0;
        if (_identity != null && _identity.equippedLoot.Count > 0)
        {
            foreach (EquippedLootState state in _identity.equippedLoot)
            {
                int ordinal = state.equipmentSlot == LootEquipmentSlot.Ring ? ringOrdinal++ : 0;
                if (!CharacterEquipmentSlotMap.TryMap(state.equipmentSlot, ordinal, out CharacterEquipmentSlot slot))
                    continue;
                _equipped[slot] = new InventoryBagUI.EquippedItemSnapshot(
                    state.inventorySlotIndex, state.itemId, 1, "");
            }
        }
        else
        {
            var snapshots = InventoryBagUI.Instance?.GetEquippedItems();
            if (snapshots != null) foreach (var item in snapshots)
            {
                LootItemDefinition definition = LootItemCatalog.Find(item.ItemId);
                bool ring = definition != null && definition.equipmentSlot != LootEquipmentSlot.None
                    ? definition.equipmentSlot == LootEquipmentSlot.Ring
                    : definition != null
                        ? definition.databaseItemType == LootDatabaseItemType.Ring
                        : string.Equals(ItemCatalogManager.Instance?.GetTemplate(item.ItemId)?.item_type,
                            "ring", System.StringComparison.OrdinalIgnoreCase);
                int ordinal = ring ? ringOrdinal++ : 0;
                if (CharacterEquipmentSlotMap.TryMap(item.ItemId, ordinal, out CharacterEquipmentSlot slot))
                    _equipped[slot] = item;
            }
        }

        bool twoHandedEquipped = _equipped.TryGetValue(CharacterEquipmentSlot.MainHand, out var mainHand) &&
                                LootItemCatalog.Find(mainHand.ItemId)?.IsTwoHanded == true;
        foreach (CharacterEquipmentSlot slot in System.Enum.GetValues(typeof(CharacterEquipmentSlot)))
        {
            bool disabled = slot == CharacterEquipmentSlot.Shoulder ||
                            (slot == CharacterEquipmentSlot.OffHand && twoHandedEquipped);
            if (!_equipped.TryGetValue(slot, out var item))
            {
                _view.SetEquipment(slot, null, 0, Color.clear, disabled);
                continue;
            }
            LootItemDefinition definition = LootItemCatalog.Find(item.ItemId);
            Sprite icon = definition != null ? definition.inventoryIcon : null;
            Color rarity = definition != null ? LootItemCatalog.RarityColor(definition.rarity) : ItemCatalogManager.GetRarityColor(item.ItemId);
            _view.SetEquipment(slot, icon, item.Quantity, rarity, false);
        }
        if (_pendingPreviewRefresh != null) StopCoroutine(_pendingPreviewRefresh);
        _pendingPreviewRefresh = StartCoroutine(RefreshPreviewAfterEquipmentSettles());
    }

    IEnumerator RefreshPreviewAfterEquipmentSettles()
    {
        yield return new WaitForEndOfFrame();
        GameObject player = NetworkClient.localPlayer != null
            ? NetworkClient.localPlayer.gameObject : null;
        _modelPreview?.Refresh(player, true);
        _pendingPreviewRefresh = null;
    }

    void OnEquipmentClicked(CharacterEquipmentSlot slot)
    {
        if (_equipped.TryGetValue(slot, out var item))
        {
            if (item.InventorySlotIndex >= 100)
                InventoryBagUI.Instance?.UnequipEquipmentPosition(
                    item.InventorySlotIndex, item.ItemId);
            else
                InventoryBagUI.Instance?.UnequipInventorySlot(item.InventorySlotIndex);
        }
    }

    void OnEquipmentBeginDrag(CharacterEquipmentSlot slot, PointerEventData eventData)
    {
        if (!_equipped.TryGetValue(slot, out var item)) return;
        ItemTooltipUI.Instance?.Hide();
        ClearDragIcon();
        var definition = LootItemCatalog.Find(item.ItemId);
        _dragLayer = new GameObject("EquipmentDragOverlay", typeof(RectTransform),
            typeof(Canvas), typeof(UnityEngine.UI.CanvasScaler));
        DontDestroyOnLoad(_dragLayer);
        var overlayCanvas = _dragLayer.GetComponent<Canvas>();
        overlayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        overlayCanvas.sortingOrder = 1000;
        var scaler = _dragLayer.GetComponent<UnityEngine.UI.CanvasScaler>();
        scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        var go = new GameObject("DraggedEquippedItem", typeof(RectTransform),
            typeof(CanvasGroup), typeof(UnityEngine.UI.Image));
        go.transform.SetParent(_dragLayer.transform, false);
        var rect = go.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(58f, 58f);
        _dragIcon = go.GetComponent<UnityEngine.UI.Image>();
        _dragIcon.sprite = definition != null ? definition.inventoryIcon : null;
        _dragIcon.preserveAspect = true;
        _dragIcon.color = _dragIcon.sprite != null ? Color.white : Color.clear;
        go.GetComponent<CanvasGroup>().blocksRaycasts = false;
        rect.position = eventData.position;
        _equipmentDragStart = eventData.position;
    }

    void OnEquipmentDrag(CharacterEquipmentSlot slot, PointerEventData eventData)
    {
        if (_dragIcon != null) _dragIcon.rectTransform.position = eventData.position;
    }

    void OnEquipmentEndDrag(CharacterEquipmentSlot slot, PointerEventData eventData)
    {
        bool dragged = _dragIcon != null &&
                       Vector2.Distance(_equipmentDragStart, eventData.position) >= 16f;
        bool returnedToSource = _view != null &&
                                _view.TryGetSlotAt(eventData.position,
                                    eventData.pressEventCamera, out var target) &&
                                target == slot;
        ClearDragIcon();
        if (dragged && !returnedToSource)
            OnEquipmentClicked(slot);
    }

    void ClearDragIcon()
    {
        if (_dragLayer != null) Destroy(_dragLayer);
        else if (_dragIcon != null) Destroy(_dragIcon.gameObject);
        _dragLayer = null;
        _dragIcon = null;
    }

    public bool AcceptsInventoryDrop(string itemId, Vector2 screenPosition, Camera eventCamera)
    {
        if (!_open || _view == null || !_view.TryGetSlotAt(screenPosition, eventCamera, out var target))
            return false;
        if (target == CharacterEquipmentSlot.OffHand &&
            _equipped.TryGetValue(CharacterEquipmentSlot.MainHand, out var mainHand) &&
            LootItemCatalog.Find(mainHand.ItemId)?.IsTwoHanded == true)
            return false;
        int ringOrdinal = target == CharacterEquipmentSlot.RingRight ? 1 : 0;
        return CharacterEquipmentSlotMap.TryMap(itemId, ringOrdinal, out var expected) && expected == target;
    }

    void OnEquipmentEnter(CharacterEquipmentSlot slot, PointerEventData eventData)
    {
        if (_equipped.TryGetValue(slot, out var item))
            ItemTooltipUI.Instance?.Show(item.ItemId, eventData.position);
    }

    void BindIdentity(PlayerIdentity identity)
    {
        if (_identity == identity) return;
        if (_identity != null) _identity.EquipmentChanged -= RefreshEquipment;
        _identity = identity;
        if (_identity != null) _identity.EquipmentChanged += RefreshEquipment;
        RefreshAll();
    }

    static bool AnyInputFocused()
    {
        if (RodChatManager.Instance != null && RodChatManager.Instance.IsOpen) return true;
        foreach (var field in FindObjectsByType<TMP_InputField>(FindObjectsInactive.Exclude)) if (field.isFocused) return true;
        return false;
    }

    static PlayerIdentity FindLocalIdentity()
    {
        foreach (var identity in FindObjectsByType<PlayerIdentity>(FindObjectsInactive.Exclude))
            if (identity.isLocalPlayer) return identity;
        return null;
    }
}
#endif
