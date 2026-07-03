#if !UNITY_SERVER
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// InventoryBagUI — Singleton inventory bag panel.
/// Self-bootstrapping; no scene setup required.
///
/// API:
///   InventoryBagUI.Instance.Show()        — open panel
///   InventoryBagUI.Instance.Hide()        — close panel
///   InventoryBagUI.Instance.Toggle()      — toggle open/close
///   InventoryBagUI.Instance.RefreshUI()   — rebuild slot display from InventoryManager
///
/// Keybind: I (hardcoded; reassignable via inspector)
/// Layout: 8×4 grid of InventorySlot tiles (32 slots)
///
/// Copy to: Assets/Game/UI/InventoryBagUI.cs
/// </summary>
public class InventoryBagUI : MonoBehaviour
{
    // ─── Singleton ────────────────────────────────────────────────────────────
    public static InventoryBagUI Instance { get; private set; }

    // ─── Inspector ────────────────────────────────────────────────────────────
    [Header("Grid")]
    public int columns  = 8;
    public int rows     = 4;
    public int slotSize = 64;
    public int padding  = 6;

    [Header("Colors")]
    public Color bgColor    = new Color(0.05f, 0.07f, 0.12f, 0.96f);
    public Color headerColor = new Color(0.12f, 0.16f, 0.28f, 1f);

    // ─── Internal ─────────────────────────────────────────────────────────────
    GameObject  _panel;
    CanvasGroup _cg;
    Transform   _grid;
    bool        _open = false;

    // ─── Bootstrap ────────────────────────────────────────────────────────────
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        var go = new GameObject("[InventoryBagUI]");
        DontDestroyOnLoad(go);
        go.AddComponent<InventoryBagUI>();
    }

    // ─── Lifecycle ────────────────────────────────────────────────────────────
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        BuildUI();
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    void Update()
    {
        // I key toggle
        if (UnityEngine.InputSystem.Keyboard.current != null &&
            UnityEngine.InputSystem.Keyboard.current.iKey.wasPressedThisFrame)
            Toggle();
    }

    // ─── Public API ───────────────────────────────────────────────────────────
    public void Show()
    {
        _panel?.SetActive(true);
        _open = true;
        RefreshUI();
    }

    public void Hide()
    {
        _panel?.SetActive(false);
        _open = false;
        TooltipUI.Hide();
    }

    public void Toggle()
    {
        if (_open) Hide(); else Show();
    }

    /// <summary>Rebuild all slot tiles from InventoryManager data.</summary>
    public void RefreshUI()
    {
        if (_grid == null || InventoryManager.Instance == null) return;

        // Clear existing tiles
        foreach (Transform child in _grid)
            Destroy(child.gameObject);

        var slots = InventoryManager.Instance.GetSlots();
        int total = columns * rows;

        for (int i = 0; i < total; i++)
        {
            var slot = slots.Find(s => s.slot_index == i);
            BuildSlotTile(i, slot);
        }
    }

    // ─── UI Construction ──────────────────────────────────────────────────────
    void BuildUI()
    {
        // Root canvas
        var canvasGo = new GameObject("BagCanvas");
        canvasGo.transform.SetParent(transform);
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode  = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 20;
        canvasGo.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        // Panel
        _panel = new GameObject("BagPanel");
        _panel.transform.SetParent(canvasGo.transform, false);

        var panelImg = _panel.AddComponent<Image>();
        panelImg.color = bgColor;

        var panelRect = _panel.GetComponent<RectTransform>();
        float pw = columns * (slotSize + padding) + padding;
        float ph = rows    * (slotSize + padding) + padding + 40f;
        panelRect.sizeDelta  = new Vector2(pw, ph);
        panelRect.anchoredPosition = Vector2.zero;

        // Header
        var header = new GameObject("Header");
        header.transform.SetParent(_panel.transform, false);
        var hi = header.AddComponent<Image>();
        hi.color = headerColor;
        var hr = header.GetComponent<RectTransform>();
        hr.anchorMin = new Vector2(0, 1);
        hr.anchorMax = Vector2.one;
        hr.offsetMin = Vector2.zero;
        hr.offsetMax = new Vector2(0, 0);
        hr.sizeDelta = new Vector2(0, 38);
        hr.anchoredPosition = new Vector2(0, -19);

        var titleGo = new GameObject("Title");
        titleGo.transform.SetParent(header.transform, false);
        var tmp = titleGo.AddComponent<TextMeshProUGUI>();
        tmp.text      = "INVENTORY";
        tmp.fontSize  = 13;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color     = new Color(0.75f, 0.85f, 1f);
        var tr = titleGo.GetComponent<RectTransform>();
        tr.anchorMin  = Vector2.zero;
        tr.anchorMax  = Vector2.one;
        tr.offsetMin  = tr.offsetMax = Vector2.zero;

        // Close button
        var closeGo = new GameObject("CloseBtn");
        closeGo.transform.SetParent(header.transform, false);
        var cb  = closeGo.AddComponent<Button>();
        var cbi = closeGo.AddComponent<Image>();
        cbi.color = new Color(0.7f, 0.2f, 0.2f, 0.8f);
        var cr = closeGo.GetComponent<RectTransform>();
        cr.anchorMin = cr.anchorMax = new Vector2(1, 0.5f);
        cr.sizeDelta = new Vector2(24, 24);
        cr.anchoredPosition = new Vector2(-16, 0);
        cb.onClick.AddListener(Hide);

        // Grid container
        var gridGo = new GameObject("Grid");
        gridGo.transform.SetParent(_panel.transform, false);
        _grid = gridGo.transform;
        var gridRect = gridGo.AddComponent<RectTransform>();
        gridRect.anchorMin = Vector2.zero;
        gridRect.anchorMax = new Vector2(1, 1);
        gridRect.offsetMin = new Vector2(padding, padding);
        gridRect.offsetMax = new Vector2(-padding, -40);
        var glg = gridGo.AddComponent<GridLayoutGroup>();
        glg.cellSize    = new Vector2(slotSize, slotSize);
        glg.spacing     = new Vector2(padding, padding);
        glg.constraint  = GridLayoutGroup.Constraint.FixedColumnCount;
        glg.constraintCount = columns;

        _panel.SetActive(false);
    }

    void BuildSlotTile(int slotIndex, InventoryManager.InventorySlot data)
    {
        var go = new GameObject($"Slot{slotIndex}");
        go.transform.SetParent(_grid, false);

        var img = go.AddComponent<Image>();
        img.color = new Color(0.10f, 0.13f, 0.20f, 1f);

        if (data != null)
        {
            // Item label
            var labelGo = new GameObject("Label");
            labelGo.transform.SetParent(go.transform, false);
            var tmp = labelGo.AddComponent<TextMeshProUGUI>();
            tmp.text     = ItemCatalogManager.Instance != null
                             ? ItemCatalogManager.Instance.GetDisplayName(data.item_id)
                             : data.item_id;
            tmp.fontSize = 9;
            tmp.color    = Color.white;
            tmp.alignment = TextAlignmentOptions.BottomLeft;
            var lr = labelGo.GetComponent<RectTransform>();
            lr.anchorMin = Vector2.zero;
            lr.anchorMax = Vector2.one;
            lr.offsetMin = new Vector2(3, 3);
            lr.offsetMax = new Vector2(-3, -3);

            // Qty badge
            if (data.quantity > 1)
            {
                var qGo = new GameObject("Qty");
                qGo.transform.SetParent(go.transform, false);
                var qt = qGo.AddComponent<TextMeshProUGUI>();
                qt.text      = data.quantity.ToString();
                qt.fontSize  = 11;
                qt.fontStyle = FontStyles.Bold;
                qt.color     = new Color(1f, 0.9f, 0.4f);
                qt.alignment  = TextAlignmentOptions.BottomRight;
                var qr = qGo.GetComponent<RectTransform>();
                qr.anchorMin = Vector2.zero;
                qr.anchorMax = Vector2.one;
                qr.offsetMin = new Vector2(2, 2);
                qr.offsetMax = new Vector2(-2, -2);
            }

            // Tooltip trigger
            var trigger = go.AddComponent<UnityEngine.EventSystems.EventTrigger>();
            var entryEnter = new UnityEngine.EventSystems.EventTrigger.Entry
                { eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter };
            string itemId = data.item_id;
            entryEnter.callback.AddListener(_ =>
            {
                string name  = ItemCatalogManager.Instance?.GetDisplayName(itemId) ?? itemId;
                string rarity = ItemCatalogManager.Instance?.GetRarity(itemId) ?? "";
                TooltipUI.Show(name, rarity, Input.mousePosition);
            });
            trigger.triggers.Add(entryEnter);

            var entryExit = new UnityEngine.EventSystems.EventTrigger.Entry
                { eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit };
            entryExit.callback.AddListener(_ => TooltipUI.Hide());
            trigger.triggers.Add(entryExit);
        }
    }
}
#endif
