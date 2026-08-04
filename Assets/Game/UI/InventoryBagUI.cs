#if UNITY_EDITOR || !UNITY_SERVER
﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.EventSystems;
using TMPro;
using Mirror;

/// <summary>
/// InventoryBagUI — toggled with B key (client-only).
/// Fetches GET /api/inventory/:characterId, renders a 4×6 slot grid.
/// Left-click a slot → equip/unequip (POST /api/inventory/equip).
/// Self-bootstrapping via RuntimeInitializeOnLoadMethod.
///
/// Requires: PlayerIdentity on local player to get characterId.
/// Auth token is read from PlayerPrefs key "jwt_token" (set by LoginManager).
/// Server IP read from PlayerPrefs key "serverIP".
/// </summary>
public class InventoryBagUI : MonoBehaviour
{
    // ── Singleton bootstrap ───────────────────────────────────────────────────
    private static InventoryBagUI _instance;
    public static InventoryBagUI Instance => _instance;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Bootstrap()
    {
        if (_instance != null) return;
        var go = new GameObject("[InventoryBagUI]");
        DontDestroyOnLoad(go);
        _instance = go.AddComponent<InventoryBagUI>();
    }

    // ── Layout ────────────────────────────────────────────────────────────────
    const int COLS        = 4;
    const int ROWS        = 6;
    const int TOTAL_SLOTS = COLS * ROWS;
    const float SLOT_SIZE = 64f;
    const float SLOT_GAP  = 6f;

    // ── UI references ─────────────────────────────────────────────────────────
    Canvas          _canvas;
    GameObject      _panel;
    SlotWidget[]    _slots;
    TextMeshProUGUI _titleText;
    TextMeshProUGUI _statusText;
    bool            _open;
    bool            _loading;

    // ── Data ──────────────────────────────────────────────────────────────────
    InventorySlotData[] _data = new InventorySlotData[TOTAL_SLOTS];

    // ── Lifecycle ─────────────────────────────────────────────────────────────
    void Awake() { BuildUI(); HidePanel(); }

    void Update()
    {
        var kb = UnityEngine.InputSystem.Keyboard.current;
        if (kb == null) return;

        // Don't open if chat or another input is focused
        if (kb.bKey.wasPressedThisFrame && !AnyInputFocused())
            Toggle();
    }

    // ── Public ────────────────────────────────────────────────────────────────
    public static void Refresh() => _instance?.StartCoroutine(_instance?.FetchInventory());

    // ── Toggle ────────────────────────────────────────────────────────────────
    void Toggle()
    {
        // Rebuild if the canvas was destroyed (e.g. scene change before the
        // DontDestroyOnLoad fix, or external cleanup) instead of throwing.
        if (_panel == null) { BuildUI(); HidePanel(); }

        _open = !_open;
        _panel.SetActive(_open);
        if (_open) StartCoroutine(FetchInventory());
    }

    void HidePanel() { _open = false; if (_panel != null) _panel.SetActive(false); }

    // ── API: Fetch ────────────────────────────────────────────────────────────
    IEnumerator FetchInventory()
    {
        if (_loading) yield break;
        _loading = true;
        SetStatus("Loading...");

        string charId = GetCharacterId();
        if (string.IsNullOrEmpty(charId)) { SetStatus("No character loaded."); _loading = false; yield break; }

        string token  = !string.IsNullOrEmpty(AuthManager.Token) ? AuthManager.Token : PlayerPrefs.GetString("jwt_token", "");
        string url    = $"{ServerConfig.AuthBaseUrl}/api/inventory/{charId}";

        using var req = UnityWebRequest.Get(url);
        req.SetRequestHeader("Authorization", $"Bearer {token}");
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            SetStatus($"Error: {req.error}");
            _loading = false;
            yield break;
        }

        try
        {
            var resp = JsonUtility.FromJson<InventoryResponse>(req.downloadHandler.text);
            if (resp.success)
            {
                _data = new InventorySlotData[TOTAL_SLOTS];
                foreach (var s in resp.data)
                    if (s.slot_index >= 0 && s.slot_index < TOTAL_SLOTS)
                        _data[s.slot_index] = s;
                RenderSlots();
                SetStatus("");
            }
            else
            {
                SetStatus($"Server: {resp.error}");
            }
        }
        catch (Exception e)
        {
            SetStatus($"Parse error: {e.Message}");
        }

        _loading = false;
    }

    // ── API: Equip/Unequip ────────────────────────────────────────────────────
    IEnumerator PostEquip(int slotIndex, bool equip)
    {
        string charId = GetCharacterId();
        if (string.IsNullOrEmpty(charId)) yield break;

        string token = !string.IsNullOrEmpty(AuthManager.Token) ? AuthManager.Token : PlayerPrefs.GetString("jwt_token", "");
        string url   = $"{ServerConfig.AuthBaseUrl}/api/inventory/equip";

        string body = $"{{\"characterId\":{charId},\"slot_index\":{slotIndex},\"equipped\":{(equip ? 1 : 0)}}}";
        using var req = new UnityWebRequest(url, "POST");
        req.uploadHandler   = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(body));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        req.SetRequestHeader("Authorization", $"Bearer {token}");
        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            // Update local state immediately, then refresh
            if (_data[slotIndex] != null) _data[slotIndex].equipped = equip ? 1 : 0;
            RenderSlots();
            StartCoroutine(FetchInventory()); // sync with server
        }
        else
        {
            SetStatus($"Equip failed: {req.error}");
        }
    }

    // ── Render ────────────────────────────────────────────────────────────────
    void RenderSlots()
    {
        for (int i = 0; i < TOTAL_SLOTS; i++)
        {
            var d = _data[i];
            if (d == null || string.IsNullOrEmpty(d.item_id))
                _slots[i].SetEmpty();
            else
                _slots[i].SetItem(d.item_id, d.quantity, d.equipped == 1);
        }
    }

    void OnSlotClicked(int index)
    {
        var d = _data[index];
        if (d == null || string.IsNullOrEmpty(d.item_id)) return;

        // Consumables are used, not equipped.
        if (ConsumableEffect.IsConsumable(d.item_id))
        {
            StartCoroutine(UseConsumable(index));
            return;
        }

        bool nowEquip = d.equipped == 0;
        StartCoroutine(PostEquip(index, nowEquip));
    }

    // ── Use a consumable ────────────────────────────────────────────────────────
    // Applies the item's effect to the local player, decrements one from the stack,
    // and persists the change. NOTE: effects that mutate Health (hp_regen, resist)
    // only take hold on the server/host — a pure remote client needs a [Command]
    // bridge to apply them server-side; speed/damage buffs work client-side today.
    IEnumerator UseConsumable(int index)
    {
        var d = _data[index];
        if (d == null || string.IsNullOrEmpty(d.item_id)) yield break;

        var player = NetworkClient.localPlayer != null ? NetworkClient.localPlayer.gameObject : null;
        if (player == null) { SetStatus("No player to use item on."); yield break; }

        if (!ConsumableEffect.Apply(d.item_id, player))
        {
            SetStatus("That item can't be used.");
            yield break;
        }

        // Decrement one; clear the slot if depleted.
        d.quantity -= 1;
        if (d.quantity <= 0) _data[index] = null;
        RenderSlots();

        yield return StartCoroutine(PostSaveAll());
        StartCoroutine(FetchInventory()); // re-sync with server
    }

    // Full-inventory save (used after consuming an item). The server treats this as
    // an authoritative replace, matching InventoryManager.SaveInventory.
    IEnumerator PostSaveAll()
    {
        string charId = GetCharacterId();
        if (string.IsNullOrEmpty(charId) || !int.TryParse(charId, out int cid)) yield break;

        string token = !string.IsNullOrEmpty(AuthManager.Token) ? AuthManager.Token : PlayerPrefs.GetString("jwt_token", "");

        var slots = new List<InventorySlotData>();
        for (int i = 0; i < TOTAL_SLOTS; i++)
            if (_data[i] != null && !string.IsNullOrEmpty(_data[i].item_id))
                slots.Add(_data[i]);

        string body = JsonUtility.ToJson(new SavePayload { characterId = cid, slots = slots });

        using var req = new UnityWebRequest($"{ServerConfig.AuthBaseUrl}/api/inventory/save", "POST");
        req.uploadHandler   = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(body));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        req.SetRequestHeader("Authorization", $"Bearer {token}");
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
            SetStatus($"Save failed: {req.error}");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    static string GetCharacterId()
    {
        if (AuthManager.CharacterId > 0) return AuthManager.CharacterId.ToString();
        foreach (var id in FindObjectsByType<PlayerIdentity>(FindObjectsInactive.Exclude))
            if (id.isLocalPlayer) return id.characterId.ToString();
        return null;
    }

    void SetStatus(string msg)
    {
        if (_statusText != null) _statusText.text = msg;
    }

    static bool AnyInputFocused()
    {
        if (RodChatManager.Instance != null && RodChatManager.Instance.IsOpen)
            return true;

        foreach (var f in FindObjectsByType<TMP_InputField>(FindObjectsInactive.Exclude))
            if (f.isFocused) return true;
        return false;
    }

    // ── Build UI ──────────────────────────────────────────────────────────────
    void BuildUI()
    {
        // Canvas
        var cgo = new GameObject("InventoryCanvas",
            typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        // Must persist with the DontDestroyOnLoad singleton — otherwise scene
        // changes destroy the canvas and Toggle() hits a dead _panel.
        DontDestroyOnLoad(cgo);
        _canvas = cgo.GetComponent<Canvas>();
        _canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = 120;
        var scaler = cgo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight  = 0.5f;

        float panelW = COLS * (SLOT_SIZE + SLOT_GAP) + SLOT_GAP + 16f;
        float panelH = ROWS * (SLOT_SIZE + SLOT_GAP) + SLOT_GAP + 60f;

        // Panel
        _panel = new GameObject("BagPanel", typeof(RectTransform), typeof(Image));
        _panel.transform.SetParent(_canvas.transform, false);
        var img = _panel.GetComponent<Image>();
        img.color = new Color(0.04f, 0.03f, 0.10f, 0.95f);
        var pRt = _panel.GetComponent<RectTransform>();
        pRt.anchorMin = pRt.anchorMax = pRt.pivot = new Vector2(1f, 0.5f);
        pRt.anchoredPosition = new Vector2(-20f, 0f);
        pRt.sizeDelta = new Vector2(panelW, panelH);

        // Title bar
        var titleBar = MakeRect("TitleBar", pRt, Vector2.zero, Vector2.one,
            new Vector2(0f, panelH - 36f), new Vector2(0f, 0f));
        titleBar.GetComponent<RectTransform>().anchorMin = new Vector2(0f, 1f);
        titleBar.GetComponent<RectTransform>().anchorMax = Vector2.one;
        titleBar.GetComponent<RectTransform>().offsetMin = new Vector2(0f, -36f);
        titleBar.GetComponent<RectTransform>().offsetMax = Vector2.zero;
        titleBar.AddComponent<Image>().color = new Color(0.08f, 0.05f, 0.20f, 1f);

        var titleGO = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
        titleGO.transform.SetParent(titleBar.transform, false);
        _titleText = titleGO.GetComponent<TextMeshProUGUI>();
        _titleText.text      = "INVENTORY  <size=9><color=#475569>B to close · click to equip · consumables to use</color></size>";
        _titleText.fontSize  = 12f;
        _titleText.color     = new Color(0.7f, 0.6f, 1f);
        _titleText.fontStyle = FontStyles.Bold;
        _titleText.alignment = TextAlignmentOptions.Left;
        var tRt = titleGO.GetComponent<RectTransform>();
        tRt.anchorMin = Vector2.zero; tRt.anchorMax = Vector2.one;
        tRt.offsetMin = new Vector2(8f, 0f); tRt.offsetMax = Vector2.zero;

        // Status text
        var statusGO = new GameObject("Status", typeof(RectTransform), typeof(TextMeshProUGUI));
        statusGO.transform.SetParent(pRt, false);
        _statusText = statusGO.GetComponent<TextMeshProUGUI>();
        _statusText.fontSize  = 10f;
        _statusText.color     = new Color(1f, 0.6f, 0.3f);
        _statusText.alignment = TextAlignmentOptions.Center;
        var sRt = statusGO.GetComponent<RectTransform>();
        sRt.anchorMin = new Vector2(0f, 0f);
        sRt.anchorMax = new Vector2(1f, 0f);
        sRt.offsetMin = new Vector2(0f, 4f);
        sRt.offsetMax = new Vector2(0f, 20f);

        // Slot grid
        _slots = new SlotWidget[TOTAL_SLOTS];
        // Slots use a top-left anchor and pivot, so their Y positions must be
        // negative offsets measured down from the top of the panel. The old
        // calculation used bottom-up panel coordinates, which placed the grid
        // above the visible bag layout.
        const float gridTopInset = 44f;

        for (int i = 0; i < TOTAL_SLOTS; i++)
        {
            int col = i % COLS;
            int row = i / COLS;
            float x = SLOT_GAP + col * (SLOT_SIZE + SLOT_GAP);
            float y = -(gridTopInset + SLOT_GAP + row * (SLOT_SIZE + SLOT_GAP));

            int captured = i;
            _slots[i] = new SlotWidget(pRt, x, y, SLOT_SIZE, () => OnSlotClicked(captured));
        }
    }

    static GameObject MakeRect(string name, RectTransform parent,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
        rt.offsetMin = offsetMin; rt.offsetMax = offsetMax;
        return go;
    }

    // ── Slot widget ───────────────────────────────────────────────────────────
    class SlotWidget
    {
        readonly Image          _bg;
        readonly Image          _icon;
        readonly TextMeshProUGUI _qty;
        readonly Image          _equipBadge;

        // Tracks the item currently in this slot so hover events can read it
        string _currentItemId;

        static readonly Color EmptyColor   = new Color(0.08f, 0.06f, 0.16f, 1f);
        static readonly Color FilledColor  = new Color(0.14f, 0.10f, 0.28f, 1f);
        static readonly Color EquippedColor= new Color(0.15f, 0.35f, 0.15f, 1f);
        static readonly Color HoverColor   = new Color(0.22f, 0.18f, 0.40f, 1f);

        public SlotWidget(RectTransform parent, float x, float y, float size, Action onClick)
        {
            var go = new GameObject("Slot", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot     = new Vector2(0f, 1f);
            rt.anchoredPosition = new Vector2(x, y);
            rt.sizeDelta = new Vector2(size, size);

            _bg = go.GetComponent<Image>();
            _bg.color = EmptyColor;

            var btn = go.GetComponent<Button>();
            btn.onClick.AddListener(() => onClick());
            var colors = btn.colors;
            colors.normalColor      = Color.white;
            colors.highlightedColor = HoverColor;
            colors.pressedColor     = new Color(0.5f, 0.4f, 0.8f);
            colors.selectedColor    = Color.white;
            btn.colors = colors;

            // Tooltip on hover — show ItemTooltipUI near the cursor
            var et = go.AddComponent<EventTrigger>();
            var enterEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
            enterEntry.callback.AddListener(ed =>
            {
#if UNITY_EDITOR || !UNITY_SERVER
                if (!string.IsNullOrEmpty(_currentItemId))
                    ItemTooltipUI.Instance?.Show(_currentItemId, ((PointerEventData)ed).position);
#endif
            });
            et.triggers.Add(enterEntry);
            var exitEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
            exitEntry.callback.AddListener(_ =>
            {
#if UNITY_EDITOR || !UNITY_SERVER
                ItemTooltipUI.Instance?.Hide();
#endif
            });
            et.triggers.Add(exitEntry);

            // Icon placeholder (tinted white square — swap with real sprite atlas later)
            var iconGO = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            iconGO.transform.SetParent(go.transform, false);
            _icon = iconGO.GetComponent<Image>();
            _icon.color = new Color(1f, 1f, 1f, 0f); // invisible until item set
            var iRt = iconGO.GetComponent<RectTransform>();
            iRt.anchorMin = new Vector2(0.1f, 0.2f);
            iRt.anchorMax = new Vector2(0.9f, 0.9f);
            iRt.offsetMin = iRt.offsetMax = Vector2.zero;

            // Quantity label
            var qGO = new GameObject("Qty", typeof(RectTransform), typeof(TextMeshProUGUI));
            qGO.transform.SetParent(go.transform, false);
            _qty = qGO.GetComponent<TextMeshProUGUI>();
            _qty.fontSize  = 9f;
            _qty.color     = new Color(0.9f, 0.9f, 0.7f);
            _qty.alignment = TextAlignmentOptions.BottomRight;
            var qRt = qGO.GetComponent<RectTransform>();
            qRt.anchorMin = Vector2.zero; qRt.anchorMax = Vector2.one;
            qRt.offsetMin = new Vector2(2f, 2f); qRt.offsetMax = new Vector2(-2f, -2f);

            // Equipped badge (green dot top-right)
            var eGO = new GameObject("EquipBadge", typeof(RectTransform), typeof(Image));
            eGO.transform.SetParent(go.transform, false);
            _equipBadge = eGO.GetComponent<Image>();
            _equipBadge.color = new Color(0.2f, 0.9f, 0.3f);
            var eRt = eGO.GetComponent<RectTransform>();
            eRt.anchorMin = eRt.anchorMax = new Vector2(1f, 1f);
            eRt.pivot     = new Vector2(1f, 1f);
            eRt.anchoredPosition = new Vector2(-3f, -3f);
            eRt.sizeDelta = new Vector2(10f, 10f);
        }

        public void SetEmpty()
        {
            _currentItemId    = null;
            _bg.color         = EmptyColor;
            _icon.color       = new Color(1f, 1f, 1f, 0f);
            _icon.sprite      = null;
            _qty.text         = "";
            _equipBadge.gameObject.SetActive(false);
#if UNITY_EDITOR || !UNITY_SERVER
            ItemTooltipUI.Instance?.Hide();
#endif
        }

        public void SetItem(string itemId, int qty, bool equipped)
        {
            _currentItemId = itemId;
            _bg.color = equipped ? EquippedColor : FilledColor;
            LootItemDefinition definition = LootItemCatalog.Find(itemId);
            _icon.sprite = definition != null ? definition.inventoryIcon : null;
            _icon.preserveAspect = true;
            _icon.color = _icon.sprite != null ? Color.white : RarityColor(itemId);
            _qty.text   = qty > 1 ? qty.ToString() : "";
            _equipBadge.gameObject.SetActive(equipped);
        }

        static Color RarityColor(string itemId)
        {
            if (itemId.Contains("_mythic"))  return new Color(1.0f, 0.5f, 0.1f);
            if (itemId.Contains("_rare"))    return new Color(0.3f, 0.6f, 1.0f);
            if (itemId.Contains("_uncommon"))return new Color(0.3f, 0.9f, 0.4f);
            return new Color(0.75f, 0.75f, 0.75f); // common
        }
    }

    // ── JSON shapes ───────────────────────────────────────────────────────────
    [Serializable] class InventoryResponse
    {
        public bool                  success;
        public string                error;
        public InventorySlotData[]   data;
    }

    [Serializable] class InventorySlotData
    {
        public int    slot_index;
        public string item_id;
        public int    quantity;
        public int    equipped;   // 0 or 1
    }

    [Serializable] class SavePayload
    {
        public int                     characterId;
        public List<InventorySlotData> slots;
    }
}
#endif
