#if UNITY_EDITOR
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class CharacterUIBuilder
{
    const string PrefabPath = "Assets/Game/UI/Resources/Character/CharacterWindow.prefab";
    const string FramePath = "Assets/Game/UI/Resources/Inventory/inventory-gothic-frame-transparent.png";

    [MenuItem("BCE/Setup/Rebuild Character UI")]
    public static void Rebuild()
    {
        EnsureFolder("Assets/Game/UI/Resources/Character");

        var root = new GameObject("CharacterWindow", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(CharacterWindowView));
        Stretch(root.GetComponent<RectTransform>(), 0f);
        var canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 115;
        var scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        var panel = Rect("Panel", root.transform, new Vector2(0.5f, 0.5f), new Vector2(1000f, 760f));

        var interior = Rect("Interior", panel, new Vector2(0.5f, 0.5f), new Vector2(850f, 600f));
        interior.anchoredPosition = new Vector2(0f, -30f);
        interior.gameObject.AddComponent<Image>().color = new Color32(14, 10, 16, 245);

        HeaderBackplate("NameBackplate", panel, new Vector2(150f, -65f), new Vector2(280f, 42f));
        HeaderBackplate("LevelBackplate", panel, new Vector2(570f, -65f), new Vector2(280f, 42f));

        var name = Text("PlayerName", panel, "Player", 18f, FontStyles.Bold, new Color32(225, 206, 154, 255),
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(158f, -103f), new Vector2(422f, -72f), TextAlignmentOptions.Center);
        var level = Text("PlayerLevel", panel, "Level 1", 18f, FontStyles.Bold, new Color32(225, 206, 154, 255),
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(578f, -103f), new Vector2(842f, -72f), TextAlignmentOptions.Center);
        name.raycastTarget = level.raycastTarget = false;

        var close = Button("Close", panel, "×", new Vector2(1f, 0f), new Vector2(-92f, 62f), new Vector2(38f, 34f));

        Label("Equipment", panel, new Vector2(95f, -150f), 175f);
        Label("Attributes", panel, new Vector2(704f, -150f), 170f);

        CharacterEquipmentSlot[] slotOrder =
        {
            CharacterEquipmentSlot.Head, CharacterEquipmentSlot.Shoulder,
            CharacterEquipmentSlot.Chest, CharacterEquipmentSlot.Hands,
            CharacterEquipmentSlot.MainHand, CharacterEquipmentSlot.OffHand,
            CharacterEquipmentSlot.Legs, CharacterEquipmentSlot.Feet,
            CharacterEquipmentSlot.RingLeft, CharacterEquipmentSlot.RingRight,
            CharacterEquipmentSlot.Trinket
        };
        string[] labels = { "Head", "Shoulder", "Chest", "Hands", "Main Hand", "Off Hand", "Legs", "Feet", "Ring I", "Ring II", "Trinket" };
        var equipmentViews = new CharacterWindowView.EquipmentSlotView[slotOrder.Length];
        for (int i = 0; i < slotOrder.Length; i++)
        {
            int column = i % 2;
            int row = i / 2;
            var slot = Rect(labels[i], panel, new Vector2(0f, 1f), new Vector2(76f, 66f));
            slot.pivot = new Vector2(0f, 1f);
            slot.anchoredPosition = new Vector2(98f + column * 86f, -174f - row * 78f);
            var bg = slot.gameObject.AddComponent<Image>();
            bg.color = new Color32(17, 14, 18, 235);
            var outline = slot.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color32(83, 65, 39, 180);
            outline.effectDistance = new Vector2(1f, -1f);
            var button = slot.gameObject.AddComponent<Button>();
            button.targetGraphic = bg;
            var icon = StretchChild("Icon", slot, 7f).gameObject.AddComponent<Image>();
            icon.color = Color.clear; icon.raycastTarget = false;
            var qty = Text("Quantity", slot, "", 12f, FontStyles.Bold, Color.white, Vector2.zero, Vector2.one,
                new Vector2(3f, 3f), new Vector2(-4f, -3f), TextAlignmentOptions.BottomRight);
            qty.raycastTarget = false;
            var slotLabel = Text("Label", slot, labels[i], 10f, FontStyles.Normal, new Color32(196, 181, 151, 255),
                Vector2.zero, Vector2.one, new Vector2(2f, 2f), new Vector2(-2f, -44f), TextAlignmentOptions.Bottom);
            slotLabel.raycastTarget = false;
            var disabled = StretchChild("Disabled", slot, 0f);
            disabled.gameObject.AddComponent<Image>().color = new Color32(25, 20, 28, 190);
            Text("DisabledLabel", disabled, "Unavailable", 9f, FontStyles.Italic, new Color32(105, 95, 110, 255),
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, TextAlignmentOptions.Center).raycastTarget = false;
            disabled.gameObject.SetActive(slotOrder[i] == CharacterEquipmentSlot.Shoulder);
            equipmentViews[i] = new CharacterWindowView.EquipmentSlotView
            {
                slot = slotOrder[i], button = button, background = bg, icon = icon,
                label = slotLabel, quantity = qty, disabledOverlay = disabled.gameObject
            };
        }

        var doll = Rect("PaperDoll", panel, new Vector2(0f, 1f), new Vector2(380f, 490f));
        doll.pivot = new Vector2(0f, 1f); doll.anchoredPosition = new Vector2(290f, -155f);
        doll.gameObject.AddComponent<Image>().color = new Color32(13, 10, 16, 220);
        var dollOutline = doll.gameObject.AddComponent<Outline>();
        dollOutline.effectColor = new Color32(79, 57, 91, 190); dollOutline.effectDistance = new Vector2(1f, -1f);
        var previewRect = Rect("CharacterPreview", doll, new Vector2(.5f, .5f), new Vector2(360f, 420f));
        previewRect.anchoredPosition = new Vector2(0f, 18f);
        var preview = previewRect.gameObject.AddComponent<RawImage>();
        preview.color = Color.white; preview.raycastTarget = false;
        var className = Text("ClassName", doll, "—", 18f, FontStyles.Bold, new Color32(191, 158, 93, 255),
            new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(20f, 18f), new Vector2(-20f, 52f), TextAlignmentOptions.Center);

        var attributes = new TextMeshProUGUI[4];
        string[] attributeNames = { "STR", "AGI", "INT", "VIT" };
        for (int i = 0; i < 4; i++)
        {
            int column = i % 2; int row = i / 2;
            var box = Rect(attributeNames[i], panel, new Vector2(0f, 1f), new Vector2(76f, 58f));
            box.pivot = new Vector2(0f, 1f); box.anchoredPosition = new Vector2(708f + column * 86f, -174f - row * 67f);
            box.gameObject.AddComponent<Image>().color = new Color32(23, 18, 25, 245);
            Text("Label", box, attributeNames[i], 10f, FontStyles.Normal, new Color32(145, 130, 109, 255),
                new Vector2(0f, .55f), Vector2.one, Vector2.zero, Vector2.zero, TextAlignmentOptions.Center).raycastTarget = false;
            attributes[i] = Text("Value", box, "—", 19f, FontStyles.Bold, new Color32(227, 200, 117, 255),
                Vector2.zero, new Vector2(1f, .62f), Vector2.zero, Vector2.zero, TextAlignmentOptions.Center);
        }

        Label("Combat", panel, new Vector2(704f, -325f), 170f);
        string[] combatNames = { "Health", "Mana", "Damage", "Crit Chance", "Crit Damage", "Reduction", "HP5", "MP5", "Move Speed", "Cooldown" };
        var combatValues = new List<TextMeshProUGUI>();
        for (int i = 0; i < combatNames.Length; i++)
        {
            float y = -349f - i * 29f;
            Text("CombatLabel" + i, panel, combatNames[i], 12f, FontStyles.Normal, new Color32(166, 153, 134, 255),
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(700f, y - 22f), new Vector2(794f, y), TextAlignmentOptions.Left).raycastTarget = false;
            combatValues.Add(Text("CombatValue" + i, panel, "—", 12f, FontStyles.Bold, new Color32(229, 216, 189, 255),
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(794f, y - 22f), new Vector2(878f, y), TextAlignmentOptions.Right));
        }

        var frame = StretchChild("GothicFrame", panel, 0f);
        var frameImage = frame.gameObject.AddComponent<Image>();
        frameImage.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(FramePath);
        frameImage.preserveAspect = false; frameImage.raycastTarget = false;

        var view = root.GetComponent<CharacterWindowView>();
        view.closeButton = close; view.playerName = name; view.playerLevel = level; view.className = className;
        view.characterPreview = preview;
        view.strValue = attributes[0]; view.agiValue = attributes[1]; view.intValue = attributes[2]; view.vitValue = attributes[3];
        view.combatValues = combatValues.ToArray(); view.equipmentSlots = equipmentViews;

        PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        Object.DestroyImmediate(root);
        AssetDatabase.SaveAssets();
        Debug.Log($"[CHARACTER] Rebuilt {PrefabPath}");
    }

    static void HeaderBackplate(string name, Transform parent, Vector2 position, Vector2 size)
    {
        var rt = Rect(name, parent, new Vector2(0f, 1f), size);
        rt.pivot = new Vector2(0f, 1f); rt.anchoredPosition = position;
        var image = rt.gameObject.AddComponent<Image>(); image.color = new Color32(25, 17, 29, 248);
        var outline = rt.gameObject.AddComponent<Outline>();
        outline.effectColor = new Color32(141, 104, 48, 230); outline.effectDistance = new Vector2(1f, -1f);
    }

    static void Label(string value, Transform parent, Vector2 position, float width)
    {
        var rt = Rect(value + "Title", parent, new Vector2(0f, 1f), new Vector2(width, 28f)); rt.pivot = new Vector2(0f, 1f); rt.anchoredPosition = position;
        Text("Text", rt, value.ToUpperInvariant(), 13f, FontStyles.Bold, new Color32(199, 168, 93, 255), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, TextAlignmentOptions.Center).raycastTarget = false;
    }

    static Button Button(string name, Transform parent, string label, Vector2 anchor, Vector2 position, Vector2 size)
    {
        var rt = Rect(name, parent, anchor, size); rt.anchoredPosition = position;
        var image = rt.gameObject.AddComponent<Image>(); image.color = new Color32(26, 19, 28, 240);
        var button = rt.gameObject.AddComponent<Button>(); button.targetGraphic = image;
        Text("Label", rt, label, 22f, FontStyles.Bold, new Color32(210, 191, 151, 255), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, TextAlignmentOptions.Center).raycastTarget = false;
        return button;
    }

    static TextMeshProUGUI Text(string name, Transform parent, string value, float size, FontStyles style, Color color,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, TextAlignmentOptions alignment)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI)); go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>(); rt.anchorMin = anchorMin; rt.anchorMax = anchorMax; rt.offsetMin = offsetMin; rt.offsetMax = offsetMax;
        var text = go.GetComponent<TextMeshProUGUI>(); text.text = value; text.fontSize = size; text.fontStyle = style; text.color = color; text.alignment = alignment;
        return text;
    }

    static RectTransform Rect(string name, Transform parent, Vector2 anchor, Vector2 size)
    {
        var go = new GameObject(name, typeof(RectTransform)); go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>(); rt.anchorMin = rt.anchorMax = anchor; rt.sizeDelta = size; return rt;
    }

    static RectTransform StretchChild(string name, Transform parent, float inset)
    {
        var rt = Rect(name, parent, Vector2.zero, Vector2.zero); Stretch(rt, inset); return rt;
    }

    static void Stretch(RectTransform rt, float inset)
    {
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = new Vector2(inset, inset); rt.offsetMax = new Vector2(-inset, -inset);
    }

    static void EnsureFolder(string path)
    {
        string[] parts = path.Split('/'); string current = parts[0];
        for (int i = 1; i < parts.Length; i++) { string next = current + "/" + parts[i]; if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(current, parts[i]); current = next; }
    }

}
#endif
