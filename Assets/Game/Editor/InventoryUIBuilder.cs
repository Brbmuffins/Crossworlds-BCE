#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Creates the reusable inventory prefab from the approved visual assets.</summary>
[InitializeOnLoad]
public static class InventoryUIBuilder
{
    const string PrefabPath = "Assets/Game/UI/Resources/Inventory/InventoryWindow.prefab";
    const string MarblePath = "Assets/Game/UI/Resources/Inventory/marble-gold-border.png";
    const string ElvenPath = "Assets/Game/UI/Resources/Inventory/elven-inventory-background.png";
    const string CoinsPath = "Assets/Game/UI/Resources/Inventory/gold-coins.png";

    static InventoryUIBuilder()
    {
        EditorApplication.delayCall += EnsurePrefab;
    }

    [MenuItem("BCE/Setup/Rebuild Inventory UI")]
    public static void Rebuild()
    {
        ConfigureSprite(MarblePath);
        ConfigureSprite(ElvenPath);
        ConfigureSprite(CoinsPath);
        AssetDatabase.ImportAsset(MarblePath, ImportAssetOptions.ForceUpdate);
        AssetDatabase.ImportAsset(ElvenPath, ImportAssetOptions.ForceUpdate);
        AssetDatabase.ImportAsset(CoinsPath, ImportAssetOptions.ForceUpdate);

        var root = new GameObject("InventoryWindow", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(InventoryBagView));
        var rootRect = root.GetComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = rootRect.offsetMax = Vector2.zero;
        var canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 120;
        var scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        var panel = Rect("Panel", root.transform, new Vector2(0.5f, 0.5f), new Vector2(500f, 620f));
        var marble = panel.gameObject.AddComponent<Image>();
        marble.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(MarblePath);
        marble.type = Image.Type.Simple;
        marble.color = Color.white;

        var inner = Rect("ElvenCrest", panel, new Vector2(0.5f, 1f), new Vector2(58f, 58f));
        inner.anchoredPosition = new Vector2(0f, -54f);
        var innerImage = inner.gameObject.AddComponent<Image>();
        innerImage.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(ElvenPath);
        innerImage.type = Image.Type.Simple;
        innerImage.color = new Color(1f, 1f, 1f, 0.16f);
        innerImage.raycastTarget = false;

        var wash = Stretch("IvoryWash", panel, 12f);
        wash.gameObject.AddComponent<Image>().color = new Color32(255, 251, 240, 72);

        var dragArea = Rect("DragArea", panel, new Vector2(0.5f, 1f), new Vector2(420f, 76f));
        dragArea.anchoredPosition = new Vector2(-18f, -48f);
        dragArea.gameObject.AddComponent<Image>().color = new Color(1f, 1f, 1f, 0f);
        dragArea.gameObject.AddComponent<InventoryWindowDragHandle>().panel = panel;

        var title = Text("Title", panel, "Inventory", 31f, FontStyles.Bold, new Color32(77, 58, 29, 255),
            new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(52f, -82f), new Vector2(-52f, -30f), TextAlignmentOptions.Center);
        title.raycastTarget = false;
        var close = Button("Close", panel, "×", new Vector2(1f, 1f), new Vector2(-46f, -48f), new Vector2(34f, 34f));

        var all = Tab("All", panel, new Vector2(55f, -112f), 110f);
        var gear = Tab("Gear", panel, new Vector2(195f, -112f), 110f);
        var materials = Tab("Materials", panel, new Vector2(335f, -112f), 110f);

        var slots = new InventoryBagView.Slot[24];
        const float size = 91f;
        const float gap = 8f;
        for (int i = 0; i < slots.Length; i++)
        {
            int column = i % 4;
            int row = i / 4;
            var slot = Rect($"Slot_{i:00}", panel, new Vector2(0f, 1f), new Vector2(size, 61f));
            slot.pivot = new Vector2(0f, 1f);
            slot.anchoredPosition = new Vector2(55f + column * (size + gap), -146f - row * 64f);
            var bg = slot.gameObject.AddComponent<Image>();
            bg.color = new Color32(25, 38, 35, 185);
            var button = slot.gameObject.AddComponent<Button>();
            button.targetGraphic = bg;

            var icon = Stretch("Icon", slot, 7f).gameObject.AddComponent<Image>();
            icon.color = Color.clear;
            icon.raycastTarget = false;
            var qty = Text("Quantity", slot, "", 14f, FontStyles.Bold, Color.white, Vector2.zero, Vector2.one,
                new Vector2(5f, 3f), new Vector2(-6f, -3f), TextAlignmentOptions.BottomRight);
            qty.raycastTarget = false;
            var equipped = Rect("Equipped", slot, new Vector2(1f, 1f), new Vector2(14f, 14f));
            equipped.pivot = new Vector2(1f, 1f);
            equipped.anchoredPosition = new Vector2(-5f, -5f);
            equipped.gameObject.AddComponent<Image>().color = new Color32(48, 184, 87, 255);
            equipped.gameObject.SetActive(false);
            slots[i] = new InventoryBagView.Slot { button = button, background = bg, icon = icon, equippedMarker = equipped.GetComponent<Image>(), quantity = qty };
        }

        var footerLine = Rect("FooterLine", panel, new Vector2(0.5f, 0f), new Vector2(390f, 1f));
        footerLine.anchoredPosition = new Vector2(0f, 59f);
        footerLine.gameObject.AddComponent<Image>().color = new Color32(166, 132, 68, 150);
        var coins = Rect("GoldCoins", panel, Vector2.zero, new Vector2(48f, 48f));
        coins.anchoredPosition = new Vector2(70f, 34f);
        var coinImage = coins.gameObject.AddComponent<Image>();
        coinImage.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(CoinsPath);
        coinImage.preserveAspect = true;
        coinImage.raycastTarget = false;
        var gold = Text("Gold", panel, "0", 20f, FontStyles.Bold, new Color32(113, 76, 18, 255),
            Vector2.zero, Vector2.zero, new Vector2(91f, 19f), new Vector2(228f, 51f), TextAlignmentOptions.Left);
        var status = Text("Status", panel, "", 13f, FontStyles.Italic, new Color32(115, 70, 51, 255),
            Vector2.zero, Vector2.zero, new Vector2(220f, 20f), new Vector2(442f, 50f), TextAlignmentOptions.Right);

        var view = root.GetComponent<InventoryBagView>();
        view.closeButton = close;
        view.allTab = all.button; view.allTabImage = all.image;
        view.gearTab = gear.button; view.gearTabImage = gear.image;
        view.materialsTab = materials.button; view.materialsTabImage = materials.image;
        view.statusText = status;
        view.goldText = gold;
        view.slots = slots;

        PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        Object.DestroyImmediate(root);
        AssetDatabase.SaveAssets();
        Debug.Log($"[INVENTORY] Rebuilt {PrefabPath}");
    }

    static void EnsurePrefab()
    {
        Rebuild();
    }

    static void ConfigureSprite(string path)
    {
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null) return;
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;
        importer.SaveAndReimport();
    }

    static RectTransform Rect(string name, Transform parent, Vector2 anchor, Vector2 size)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = anchor;
        rt.sizeDelta = size;
        return rt;
    }

    static RectTransform Stretch(string name, Transform parent, float inset)
    {
        var rt = Rect(name, parent, Vector2.zero, Vector2.zero);
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(inset, inset); rt.offsetMax = new Vector2(-inset, -inset);
        return rt;
    }

    static TextMeshProUGUI Text(string name, Transform parent, string value, float size, FontStyles style, Color color,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, TextAlignmentOptions alignment)
    {
        var rt = Rect(name, parent, Vector2.zero, Vector2.zero);
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax; rt.offsetMin = offsetMin; rt.offsetMax = offsetMax;
        var text = rt.gameObject.AddComponent<TextMeshProUGUI>();
        text.text = value; text.fontSize = size; text.fontStyle = style; text.color = color; text.alignment = alignment;
        return text;
    }

    static Button Button(string name, Transform parent, string label, Vector2 anchor, Vector2 position, Vector2 size)
    {
        var rt = Rect(name, parent, anchor, size); rt.anchoredPosition = position;
        var image = rt.gameObject.AddComponent<Image>(); image.color = new Color32(92, 65, 25, 220);
        var button = rt.gameObject.AddComponent<Button>(); button.targetGraphic = image;
        Text("Label", rt, label, 22f, FontStyles.Bold, Color.white, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, TextAlignmentOptions.Center).raycastTarget = false;
        return button;
    }

    static (Button button, Image image) Tab(string label, Transform parent, Vector2 position, float width)
    {
        var rt = Rect(label + "Tab", parent, new Vector2(0f, 1f), new Vector2(width, 32f));
        rt.pivot = new Vector2(0f, 1f); rt.anchoredPosition = position;
        var image = rt.gameObject.AddComponent<Image>(); image.color = new Color32(232, 222, 195, 235);
        var button = rt.gameObject.AddComponent<Button>(); button.targetGraphic = image;
        Text("Label", rt, label, 15f, FontStyles.Bold, new Color32(75, 55, 29, 255), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, TextAlignmentOptions.Center).raycastTarget = false;
        return (button, image);
    }

}
#endif
