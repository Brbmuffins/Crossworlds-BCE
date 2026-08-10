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
    CharacterWindowView _view;
    CharacterModelPreview _modelPreview;
    CharacterWindowDragHandle _dragHandle;
    readonly Dictionary<CharacterEquipmentSlot, InventoryBagUI.EquippedItemSnapshot> _equipped = new();
    PlayerProgressManager _progress;
    InventoryBagUI _inventory;
    PlayerIdentity _identity;
    bool _open;
    float _nextLiveRefresh;

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
    }

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
        _view.Initialize(Hide, OnEquipmentClicked, OnEquipmentEnter, () => ItemTooltipUI.Instance?.Hide());
        RectTransform panel = _view.transform.Find("Panel") as RectTransform;
        if (panel != null)
        {
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

        foreach (CharacterEquipmentSlot slot in System.Enum.GetValues(typeof(CharacterEquipmentSlot)))
        {
            bool disabled = slot == CharacterEquipmentSlot.Shoulder;
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
        GameObject player = NetworkClient.localPlayer != null ? NetworkClient.localPlayer.gameObject : null;
        _modelPreview?.Refresh(player, true);
    }

    void OnEquipmentClicked(CharacterEquipmentSlot slot)
    {
        if (_equipped.TryGetValue(slot, out var item))
            InventoryBagUI.Instance?.UnequipInventorySlot(item.InventorySlotIndex);
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
