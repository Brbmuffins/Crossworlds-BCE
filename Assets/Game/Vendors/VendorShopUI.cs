#if UNITY_EDITOR || !UNITY_SERVER
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class VendorShopUI : MonoBehaviour
{
    static VendorShopUI _instance;
    NetworkVendor _vendor;
    GameObject _window;
    TextMeshProUGUI _title, _subtitle, _status, _gold;
    RectTransform _content;
    bool _sellMode, _pending;

    public static VendorShopUI EnsureInstance()
    {
        if (_instance != null) return _instance;
        var go = new GameObject("[VendorShopUI]");
        DontDestroyOnLoad(go);
        return _instance = go.AddComponent<VendorShopUI>();
    }

    void Awake()
    {
        if (_instance != null && _instance != this) { Destroy(gameObject); return; }
        _instance = this;
        Build();
        _window.SetActive(false);
    }

    void Update()
    {
        var keyboard = UnityEngine.InputSystem.Keyboard.current;
        if (_window.activeSelf && keyboard != null && keyboard.escapeKey.wasPressedThisFrame) Close();
    }

    public void Open(NetworkVendor vendor)
    {
        if (vendor == null || vendor.profile == null) return;
        _vendor = vendor;
        _sellMode = false;
        _pending = false;
        _window.SetActive(true);
        _title.text = vendor.DisplayName;
        _subtitle.text = vendor.Subtitle;
        SetStatus("Select an item to buy.");
        Render();
    }

    public void CompleteTransaction(bool success, string message)
    {
        _pending = false;
        SetStatus(message, success ? new Color(0.55f, 1f, 0.55f) : new Color(1f, 0.5f, 0.45f));
        if (success) StartCoroutine(RefreshAfterTransaction());
    }

    IEnumerator RefreshAfterTransaction()
    {
        if (InventoryManager.Instance != null) yield return InventoryManager.Instance.LoadInventory();
        InventoryBagUI.Refresh();
        PlayerProgressManager.Local?.Refresh();
        yield return null;
        Render();
    }

    void Render()
    {
        if (_content == null || _vendor == null || _vendor.profile == null) return;
        for (int i = _content.childCount - 1; i >= 0; i--) Destroy(_content.GetChild(i).gameObject);
        _gold.text = $"Gold: {(PlayerProgressManager.Local != null ? PlayerProgressManager.Local.Gold : 0):N0}";

        if (!_sellMode)
        {
            foreach (VendorStockEntry entry in _vendor.profile.stock)
            {
                if (entry?.item == null || string.IsNullOrWhiteSpace(entry.item.itemId)) continue;
                LootItemDefinition item = entry.item;
                AddRow(item, item.displayName, item.inventoryIcon, $"Buy  {Mathf.Max(1, entry.buyPrice):N0}g",
                    () => Buy(item.itemId));
            }
            if (_content.childCount == 0) AddMessage("This vendor has no items for sale.");
        }
        else
        {
            if (!_vendor.profile.buysItems) { AddMessage("This vendor does not buy items."); return; }
            var slots = InventoryManager.Instance?.GetSlots();
            if (slots != null)
                foreach (var slot in slots)
                {
                    if (slot == null || slot.equipped != 0 || slot.slot_index < 0 || slot.slot_index >= 24) continue;
                    LootItemDefinition item = LootItemCatalog.Find(slot.item_id);
                    if (item == null || item.sellValue <= 0) continue;
                    int capturedSlot = slot.slot_index;
                    AddRow(item, $"{item.displayName}  x{slot.quantity}", item.inventoryIcon, $"Sell  {item.sellValue:N0}g",
                        () => Sell(capturedSlot));
                }
            if (_content.childCount == 0) AddMessage("You have no sellable items.");
        }
    }

    void Buy(string itemId)
    {
        if (_pending || _vendor == null) return;
        _pending = true;
        SetStatus("Processing purchase...");
        _vendor.RequestBuy(itemId, 1);
    }

    void Sell(int slotIndex)
    {
        if (_pending || _vendor == null) return;
        _pending = true;
        SetStatus("Processing sale...");
        _vendor.RequestSell(slotIndex, 1);
    }

    void SetMode(bool sell)
    {
        if (_pending) return;
        _sellMode = sell;
        SetStatus(sell ? "Select an inventory item to sell." : "Select an item to buy.");
        Render();
    }

    void Close()
    {
        _pending = false;
        _vendor = null;
        ItemTooltipUI.Instance?.Hide();
        _window.SetActive(false);
    }

    void Build()
    {
        var canvasGo = new GameObject("VendorCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasGo.transform.SetParent(transform, false);
        var canvas = canvasGo.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 125;
        var scaler = canvasGo.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1920, 1080); scaler.matchWidthOrHeight = 0.5f;

        _window = Box("VendorWindow", canvasGo.transform, new Color(0.035f, 0.028f, 0.045f, 0.98f));
        var wr = _window.GetComponent<RectTransform>(); wr.anchorMin = wr.anchorMax = new Vector2(0.5f, 0.5f); wr.sizeDelta = new Vector2(720f, 620f);
        _title = Label("Title", _window.transform, "Merchant", 30, FontStyles.Bold, new Color(0.95f, 0.72f, 0.25f));
        SetRect(_title.rectTransform, new Vector2(0f, .92f), new Vector2(1f, 1f), new Vector2(24, 0), new Vector2(-70, -8));
        _subtitle = Label("Subtitle", _window.transform, "", 17, FontStyles.Normal, new Color(.85f, .76f, .57f));
        SetRect(_subtitle.rectTransform, new Vector2(0f, .875f), new Vector2(1f, .93f), new Vector2(24, 0), new Vector2(-70, 0));
        Button("Close", _window.transform, new Vector2(.92f, .91f), new Vector2(.98f, .98f), "×", Close, new Color(.35f, .12f, .12f));
        Button("BuyTab", _window.transform, new Vector2(.08f, .80f), new Vector2(.47f, .88f), "BUY", () => SetMode(false), new Color(.32f, .22f, .08f));
        Button("SellTab", _window.transform, new Vector2(.53f, .80f), new Vector2(.92f, .88f), "SELL", () => SetMode(true), new Color(.18f, .18f, .22f));

        var scrollGo = new GameObject("Scroll", typeof(RectTransform), typeof(Image), typeof(Mask), typeof(ScrollRect));
        scrollGo.transform.SetParent(_window.transform, false); SetRect(scrollGo.GetComponent<RectTransform>(), new Vector2(.06f, .16f), new Vector2(.94f, .78f), Vector2.zero, Vector2.zero);
        scrollGo.GetComponent<Image>().color = new Color(.07f, .065f, .075f, 1f); scrollGo.GetComponent<Mask>().showMaskGraphic = true;
        var contentGo = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        contentGo.transform.SetParent(scrollGo.transform, false); _content = contentGo.GetComponent<RectTransform>();
        _content.anchorMin = new Vector2(0, 1); _content.anchorMax = new Vector2(1, 1); _content.pivot = new Vector2(.5f, 1); _content.sizeDelta = Vector2.zero;
        var layout = contentGo.GetComponent<VerticalLayoutGroup>(); layout.padding = new RectOffset(10, 10, 10, 10); layout.spacing = 6; layout.childControlHeight = false; layout.childForceExpandWidth = true;
        contentGo.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        var scroll = scrollGo.GetComponent<ScrollRect>(); scroll.viewport = scrollGo.GetComponent<RectTransform>(); scroll.content = _content; scroll.horizontal = false;

        _status = Label("Status", _window.transform, "", 17, FontStyles.Normal, Color.white);
        SetRect(_status.rectTransform, new Vector2(.06f, .06f), new Vector2(.72f, .14f), Vector2.zero, Vector2.zero); _status.alignment = TextAlignmentOptions.Left;
        _gold = Label("Gold", _window.transform, "Gold: 0", 19, FontStyles.Bold, new Color(1f, .8f, .16f));
        SetRect(_gold.rectTransform, new Vector2(.72f, .06f), new Vector2(.94f, .14f), Vector2.zero, Vector2.zero); _gold.alignment = TextAlignmentOptions.Right;
    }

    void AddRow(LootItemDefinition item, string name, Sprite icon, string action,
        UnityEngine.Events.UnityAction callback)
    {
        var row = Box("ItemRow", _content, new Color(.12f, .105f, .13f, 1f));
        var rt = row.GetComponent<RectTransform>(); rt.sizeDelta = new Vector2(0, 72); row.AddComponent<LayoutElement>().preferredHeight = 72;
        if (icon != null) { var image = new GameObject("Icon", typeof(RectTransform), typeof(Image)).GetComponent<Image>(); image.transform.SetParent(row.transform, false); image.sprite = icon; image.preserveAspect = true; SetRect(image.rectTransform, new Vector2(.02f,.12f), new Vector2(.11f,.88f), Vector2.zero, Vector2.zero); }
        Color rarityColor = LootItemCatalog.RarityColor(item.rarity);
        var text = Label("Name", row.transform, name, 18, FontStyles.Bold, rarityColor); SetRect(text.rectTransform, new Vector2(.13f,0), new Vector2(.67f,1), Vector2.zero, Vector2.zero); text.alignment = TextAlignmentOptions.Left;
        Button("Action", row.transform, new Vector2(.70f,.18f), new Vector2(.97f,.82f), action, callback, new Color(.42f,.27f,.08f));

        var trigger = row.AddComponent<EventTrigger>();
        var enter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        enter.callback.AddListener(data =>
            ItemTooltipUI.Instance?.Show(item.itemId, ((PointerEventData)data).position));
        trigger.triggers.Add(enter);
        var exit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        exit.callback.AddListener(_ => ItemTooltipUI.Instance?.Hide());
        trigger.triggers.Add(exit);
    }

    void AddMessage(string value) { var text = Label("Message", _content, value, 19, FontStyles.Italic, new Color(.75f,.72f,.68f)); text.gameObject.AddComponent<LayoutElement>().preferredHeight = 64; }
    void SetStatus(string value, Color? color = null) { if (_status != null) { _status.text = value; _status.color = color ?? new Color(.86f,.82f,.75f); } }
    static GameObject Box(string name, Transform parent, Color color) { var go = new GameObject(name, typeof(RectTransform), typeof(Image)); go.transform.SetParent(parent, false); go.GetComponent<Image>().color = color; return go; }
    static TextMeshProUGUI Label(string name, Transform parent, string value, float size, FontStyles style, Color color) { var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI)); go.transform.SetParent(parent, false); var t=go.GetComponent<TextMeshProUGUI>(); t.text=value; t.fontSize=size; t.fontStyle=style; t.color=color; t.alignment=TextAlignmentOptions.Center; t.raycastTarget=false; return t; }
    static void Button(string name, Transform parent, Vector2 min, Vector2 max, string label, UnityEngine.Events.UnityAction action, Color color) { var go=Box(name,parent,color); SetRect(go.GetComponent<RectTransform>(),min,max,Vector2.zero,Vector2.zero); var b=go.AddComponent<Button>(); b.targetGraphic=go.GetComponent<Image>(); b.onClick.AddListener(action); var t=Label("Label",go.transform,label,18,FontStyles.Bold,Color.white); SetRect(t.rectTransform,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero); }
    static void SetRect(RectTransform r, Vector2 min, Vector2 max, Vector2 offMin, Vector2 offMax) { r.anchorMin=min; r.anchorMax=max; r.offsetMin=offMin; r.offsetMax=offMax; }
}
#endif
