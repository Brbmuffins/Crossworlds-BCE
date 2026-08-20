#if UNITY_EDITOR
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Builds the prefab-owned Gothic Forge window and recipe row.</summary>
public static class ForgeCraftingUIBuilder
{
    const string DirectoryPath = "Assets/Game/UI/Resources/Forge";
    const string WindowPath = DirectoryPath + "/ForgeWindow.prefab";
    const string RowPath = DirectoryPath + "/ForgeRecipeRow.prefab";
    const string GothicPanelPath = "Assets/Game/UI/Resources/Inventory/inventory-gothic-panel-clean.png";

    [DidReloadScripts]
    static void BuildMissingPrefab()
    {
        if (AssetDatabase.LoadAssetAtPath<GameObject>(WindowPath) != null) return;
        EditorApplication.delayCall += () =>
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(WindowPath) == null) Rebuild();
        };
    }

    [MenuItem("BCE/Setup/Rebuild Forge Crafting UI")]
    public static void Rebuild()
    {
        Directory.CreateDirectory(DirectoryPath);
        ConfigureSprite(GothicPanelPath);
        AssetDatabase.ImportAsset(GothicPanelPath, ImportAssetOptions.ForceUpdate);

        GameObject rowPrefab = BuildRecipeRow();
        BuildWindow(rowPrefab);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[FORGE] Rebuilt {WindowPath} and {RowPath}");
    }

    static GameObject BuildRecipeRow()
    {
        var root = new GameObject("ForgeRecipeRow", typeof(RectTransform), typeof(Image), typeof(CanvasGroup), typeof(RecipeRowUI));
        var rect = root.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(420f, 92f);
        root.GetComponent<Image>().color = new Color32(20, 26, 24, 235);

        var badgeRect = Rect("RarityBadge", root.transform, new Vector2(0f, 0.5f), new Vector2(5f, 80f));
        badgeRect.pivot = new Vector2(0f, 0.5f); badgeRect.anchoredPosition = new Vector2(4f, 0f);
        var badge = badgeRect.gameObject.AddComponent<Image>();

        var iconRect = Rect("Icon", root.transform, new Vector2(0f, 0.5f), new Vector2(62f, 62f));
        iconRect.pivot = new Vector2(0f, 0.5f); iconRect.anchoredPosition = new Vector2(16f, 0f);
        var iconBg = iconRect.gameObject.AddComponent<Image>(); iconBg.color = new Color32(9, 14, 13, 235);
        var icon = Stretch("Artwork", iconRect, 5f).gameObject.AddComponent<Image>();
        icon.color = Color.clear; icon.raycastTarget = false;

        var name = Text("Name", root.transform, "Copper Ingot", 17f, FontStyles.Bold, new Color32(238, 203, 119, 255),
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(90f, -37f), new Vector2(285f, -10f), TextAlignmentOptions.Left);
        var level = Text("Level", root.transform, "Lv 1", 12f, FontStyles.Bold, new Color32(188, 158, 93, 255),
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-128f, -35f), new Vector2(-76f, -10f), TextAlignmentOptions.Right);
        var ingredients = Text("Ingredients", root.transform, "3× Copper Ore", 13f, FontStyles.Normal, Color.white,
            new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(90f, 14f), new Vector2(300f, 44f), TextAlignmentOptions.Left);
        var time = Text("Time", root.transform, "2s", 12f, FontStyles.Normal, new Color32(180, 180, 180, 255),
            new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-126f, 15f), new Vector2(-82f, 42f), TextAlignmentOptions.Right);
        var craft = Button("Craft", root.transform, "CRAFT", new Vector2(1f, 0.5f), new Vector2(-48f, 0f), new Vector2(76f, 38f));

        var ui = root.GetComponent<RecipeRowUI>();
        ui.nameLabel = name; ui.levelLabel = level; ui.ingredientsLabel = ingredients; ui.timeLabel = time;
        ui.craftButton = craft; ui.rarityBadge = badge; ui.icon = icon;

        PrefabUtility.SaveAsPrefabAsset(root, RowPath);
        Object.DestroyImmediate(root);
        return AssetDatabase.LoadAssetAtPath<GameObject>(RowPath);
    }

    static void BuildWindow(GameObject rowPrefab)
    {
        var root = new GameObject("ForgeWindow", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler),
            typeof(GraphicRaycaster), typeof(ForgeCraftingPanel));
        var rootRect = root.GetComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero; rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = rootRect.offsetMax = Vector2.zero;
        var canvas = root.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 124;
        var scaler = root.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f); scaler.matchWidthOrHeight = 0.5f;

        var panel = Rect("Panel", root.transform, new Vector2(0.5f, 0.5f), new Vector2(500f, 620f));
        var background = panel.gameObject.AddComponent<Image>();
        background.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(GothicPanelPath); background.color = Color.white;

        var drag = Rect("DragArea", panel, new Vector2(0.5f, 1f), new Vector2(410f, 68f));
        drag.anchoredPosition = new Vector2(-18f, -47f);
        drag.gameObject.AddComponent<Image>().color = Color.clear;
        drag.gameObject.AddComponent<ForgeWindowDragHandle>().panel = panel;

        Text("Title", panel, "FORGE", 25f, FontStyles.Bold, new Color32(230, 194, 105, 255),
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(-130f, -75f), new Vector2(130f, -37f), TextAlignmentOptions.Center).raycastTarget = false;
        var close = Button("Close", panel, "×", new Vector2(1f, 1f), new Vector2(-46f, -48f), new Vector2(34f, 34f));
        var smeltTab = Button("SmeltTab", panel, "SMELT", new Vector2(0f, 1f), new Vector2(128f, -112f), new Vector2(170f, 34f));
        var craftTab = Button("CraftTab", panel, "CRAFT", new Vector2(0f, 1f), new Vector2(322f, -112f), new Vector2(170f, 34f));

        var smelt = Scroll("SmeltContent", panel, out Transform smeltList);
        var craft = Scroll("CraftContent", panel, out Transform craftList);

        var status = Text("Status", panel, "", 13f, FontStyles.Italic, new Color32(225, 196, 126, 255),
            new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(54f, 45f), new Vector2(-54f, 78f), TextAlignmentOptions.Center);

        var progress = Rect("ProgressOverlay", panel, new Vector2(0.5f, 0.5f), new Vector2(390f, 150f));
        progress.gameObject.AddComponent<Image>().color = new Color(0.025f, 0.02f, 0.035f, 0.98f);
        var progressLabel = Text("ProgressLabel", progress, "Crafting…", 19f, FontStyles.Bold, new Color32(238, 203, 119, 255),
            new Vector2(0f, 0.5f), new Vector2(1f, 1f), new Vector2(20f, -4f), new Vector2(-20f, -12f), TextAlignmentOptions.Center);
        var slider = Slider("ProgressBar", progress);

        var controller = root.GetComponent<ForgeCraftingPanel>();
        controller.smeltTabButton = smeltTab; controller.craftTabButton = craftTab;
        controller.smeltContent = smelt; controller.craftContent = craft;
        controller.smeltListParent = smeltList; controller.craftListParent = craftList;
        controller.recipeRowPrefab = rowPrefab; controller.progressOverlay = progress.gameObject;
        controller.progressBar = slider; controller.progressLabel = progressLabel;
        controller.closeButton = close; controller.statusLabel = status;

        PrefabUtility.SaveAsPrefabAsset(root, WindowPath);
        Object.DestroyImmediate(root);
    }

    static GameObject Scroll(string name, Transform panel, out Transform content)
    {
        var root = Rect(name, panel, new Vector2(0.5f, 1f), new Vector2(420f, 390f));
        root.anchoredPosition = new Vector2(0f, -340f);
        var scroll = root.gameObject.AddComponent<ScrollRect>(); scroll.horizontal = false;
        var viewport = Stretch("Viewport", root, 0f);
        viewport.gameObject.AddComponent<Image>().color = new Color32(7, 11, 10, 205);
        viewport.gameObject.AddComponent<Mask>().showMaskGraphic = true;
        var contentRect = Rect("List", viewport, new Vector2(0f, 1f), new Vector2(0f, 0f));
        contentRect.anchorMin = new Vector2(0f, 1f); contentRect.anchorMax = new Vector2(1f, 1f);
        contentRect.pivot = new Vector2(0.5f, 1f); contentRect.offsetMin = new Vector2(0f, 0f); contentRect.offsetMax = Vector2.zero;
        var layout = contentRect.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 5f; layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true; layout.childControlHeight = false; layout.childForceExpandWidth = true; layout.childForceExpandHeight = false;
        var fitter = contentRect.gameObject.AddComponent<ContentSizeFitter>(); fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        scroll.viewport = viewport; scroll.content = contentRect;
        content = contentRect;
        return root.gameObject;
    }

    static Slider Slider(string name, Transform parent)
    {
        var root = Rect(name, parent, new Vector2(0.5f, 0f), new Vector2(330f, 20f)); root.anchoredPosition = new Vector2(0f, 28f);
        var bg = root.gameObject.AddComponent<Image>(); bg.color = new Color32(22, 30, 28, 255);
        var fillArea = Stretch("FillArea", root, 3f);
        var fill = Stretch("Fill", fillArea, 0f); fill.anchorMax = new Vector2(0f, 1f);
        var fillImage = fill.gameObject.AddComponent<Image>(); fillImage.color = new Color32(205, 137, 39, 255);
        var slider = root.gameObject.AddComponent<Slider>(); slider.fillRect = fill; slider.targetGraphic = bg;
        slider.direction = UnityEngine.UI.Slider.Direction.LeftToRight;
        return slider;
    }

    static void ConfigureSprite(string path)
    {
        if (AssetImporter.GetAtPath(path) is not TextureImporter importer) return;
        importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single;
        importer.mipmapEnabled = false; importer.alphaIsTransparency = true; importer.SaveAndReimport();
    }

    static RectTransform Rect(string name, Transform parent, Vector2 anchor, Vector2 size)
    {
        var go = new GameObject(name, typeof(RectTransform)); go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>(); rt.anchorMin = rt.anchorMax = anchor; rt.sizeDelta = size; return rt;
    }

    static RectTransform Stretch(string name, Transform parent, float inset)
    {
        var rt = Rect(name, parent, Vector2.zero, Vector2.zero); rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(inset, inset); rt.offsetMax = new Vector2(-inset, -inset); return rt;
    }

    static TextMeshProUGUI Text(string name, Transform parent, string value, float size, FontStyles style, Color color,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, TextAlignmentOptions alignment)
    {
        var rt = Rect(name, parent, Vector2.zero, Vector2.zero); rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
        rt.offsetMin = offsetMin; rt.offsetMax = offsetMax;
        var text = rt.gameObject.AddComponent<TextMeshProUGUI>(); text.text = value; text.fontSize = size;
        text.fontStyle = style; text.color = color; text.alignment = alignment; return text;
    }

    static Button Button(string name, Transform parent, string label, Vector2 anchor, Vector2 position, Vector2 size)
    {
        var rt = Rect(name, parent, anchor, size); rt.anchoredPosition = position;
        var image = rt.gameObject.AddComponent<Image>(); image.color = new Color32(92, 65, 25, 235);
        var button = rt.gameObject.AddComponent<Button>(); button.targetGraphic = image;
        Text("Label", rt, label, 15f, FontStyles.Bold, new Color32(244, 225, 178, 255), Vector2.zero, Vector2.one,
            Vector2.zero, Vector2.zero, TextAlignmentOptions.Center).raycastTarget = false;
        return button;
    }
}
#endif
