#if UNITY_EDITOR || !UNITY_SERVER
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// ItemTooltipUI — self-bootstrapping singleton tooltip panel.
/// Appears near the cursor when Show() is called; hides on Hide().
///
/// Usage:
///   ItemTooltipUI.Instance.Show("material_copper_shard", Input.mousePosition);
///   ItemTooltipUI.Instance.Show("Custom text", Input.mousePosition);
///   ItemTooltipUI.Instance.Hide();
///
/// Reads ItemCatalogManager for name/rarity if available.
/// Falls back to formatting the item ID itself.
/// Sort order 200 — above all other HUD layers.
/// </summary>
public class ItemTooltipUI : MonoBehaviour
{
    // ── Bootstrap ─────────────────────────────────────────────────────────────
    public static ItemTooltipUI Instance { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Bootstrap()
    {
        if (Instance != null) return;
        var go = new GameObject("[ItemTooltipUI]");
        DontDestroyOnLoad(go);
        Instance = go.AddComponent<ItemTooltipUI>();
    }

    // ── Colours ───────────────────────────────────────────────────────────────
    static readonly Color ColBg      = new Color(0.04f, 0.03f, 0.08f, 0.95f);
    static readonly Color ColBorder  = new Color(0.30f, 0.25f, 0.45f, 1.00f);
    static readonly Color ColText    = new Color(0.95f, 0.90f, 0.80f, 1.00f);
    static readonly Color ColSubtext = new Color(0.72f, 0.62f, 0.76f, 0.95f);

    // ── UI refs ───────────────────────────────────────────────────────────────
    RectTransform   _panelRT;
    Image           _border;
    TextMeshProUGUI _nameTxt;
    TextMeshProUGUI _rarityTxt;
    TextMeshProUGUI _slotTxt;
    TextMeshProUGUI _detailsTxt;
    Image           _slotTopDivider;
    Image           _slotBottomDivider;

    // Tooltip sizing
    const float PanelW   = 300f;
    const float MinimumPanelH = 92f;
    const float Offset   = 16f; // cursor offset

    // ── Lifecycle ─────────────────────────────────────────────────────────────
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        BuildUI();
        _panelRT.gameObject.SetActive(false);
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>Show a tooltip for an item ID, near the given screen position.</summary>
    public void Show(string itemId, Vector2 screenPos)
    {
        if (string.IsNullOrEmpty(itemId)) { Hide(); return; }

        var catalog = ItemCatalogManager.Instance;
        var def     = catalog?.GetTemplate(itemId);
        LootItemDefinition lootDefinition = LootItemCatalog.Find(itemId);

        string displayName = !string.IsNullOrWhiteSpace(lootDefinition?.displayName)
            ? lootDefinition.displayName : def?.name ?? FormatId(itemId);
        string typeLine = lootDefinition != null
            ? FormatId(lootDefinition.databaseItemType.ToString())
            : def?.item_type != null
            ? System.Globalization.CultureInfo.CurrentCulture.TextInfo
                   .ToTitleCase(def.item_type.Replace('_', ' '))
            : "";
        Color rarityColor = lootDefinition != null
            ? LootItemCatalog.RarityColor(lootDefinition.rarity)
            : ItemCatalogManager.GetRarityColor(itemId);
        string rarityName = lootDefinition != null
            ? FormatRarity(lootDefinition.rarity) : "";

        _nameTxt.text = displayName;
        _nameTxt.color = rarityColor;
        _rarityTxt.text = string.IsNullOrWhiteSpace(rarityName)
            ? typeLine : string.IsNullOrWhiteSpace(typeLine)
                ? rarityName : $"{rarityName} {typeLine}";
        _border.color = rarityColor;

        string slotName = lootDefinition != null
            ? FormatEquipmentSlot(lootDefinition.equipmentSlot) : "";
        _slotTxt.gameObject.SetActive(!string.IsNullOrWhiteSpace(slotName));
        _slotTopDivider.gameObject.SetActive(!string.IsNullOrWhiteSpace(slotName));
        _slotBottomDivider.gameObject.SetActive(!string.IsNullOrWhiteSpace(slotName));
        Color dividerColor = Color.Lerp(ColBorder, rarityColor, 0.5f);
        dividerColor.a = 0.8f;
        _slotTopDivider.color = dividerColor;
        _slotBottomDivider.color = dividerColor;
        _slotTxt.text = string.IsNullOrWhiteSpace(slotName)
            ? "" : $"Equipment Slot                 <color=#D8AD52>{slotName}</color>";

        var stats = new List<string>();
        AddStat(stats, "Strength", lootDefinition?.bonusStrength ?? def?.stat_str ?? 0);
        AddStat(stats, "Agility", lootDefinition?.bonusAgility ?? def?.stat_agi ?? 0);
        AddStat(stats, "Intelligence", lootDefinition?.bonusIntelligence ?? def?.stat_int ?? 0);
        AddStat(stats, "Vitality", lootDefinition?.bonusVitality ?? def?.stat_vit ?? 0);

        var details = new StringBuilder();
        if (stats.Count > 0)
        {
            details.AppendLine("<color=#A99A82>STAT MODIFICATIONS</color>");
            foreach (string stat in stats)
                details.AppendLine($"<color=#64F29C>{stat}</color>");
        }
        if (lootDefinition?.spellBonusDescriptions != null)
        {
            bool headingAdded = false;
            foreach (string bonus in lootDefinition.spellBonusDescriptions)
            {
                if (string.IsNullOrWhiteSpace(bonus)) continue;
                if (!headingAdded)
                {
                    if (details.Length > 0) details.AppendLine();
                    details.AppendLine("<color=#A99A82>SPELL BONUSES</color>");
                    headingAdded = true;
                }
                details.AppendLine($"<color=#78B9FF>{bonus.Trim()}</color>");
            }
        }
        _detailsTxt.text = details.ToString().TrimEnd();
        _detailsTxt.gameObject.SetActive(details.Length > 0);
        ResizePanel(!string.IsNullOrWhiteSpace(slotName));

        PositionNearCursor(screenPos);
        _panelRT.gameObject.SetActive(true);
    }

    public void Hide()
    {
        if (_panelRT != null) _panelRT.gameObject.SetActive(false);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    void PositionNearCursor(Vector2 screenPos)
    {
        // Keep panel on screen
        float x = screenPos.x + Offset;
        float y = screenPos.y - Offset;
        if (x + PanelW > Screen.width)  x = screenPos.x - PanelW - Offset;
        float panelH = _panelRT.sizeDelta.y;
        if (y - panelH < 0)             y = screenPos.y + panelH + Offset;
        _panelRT.position = new Vector3(x, y, 0f);
    }

    static string FormatId(string id)
    {
        if (string.IsNullOrEmpty(id)) return "Unknown";
        var parts = id.Split('_');
        for (int i = 0; i < parts.Length; i++)
            if (parts[i].Length > 0)
                parts[i] = char.ToUpper(parts[i][0]) + parts[i].Substring(1);
        return string.Join(" ", parts);
    }

    static string FormatRarity(ItemRarity rarity) => rarity switch
    {
        ItemRarity.Uncommon => "Uncommon",
        ItemRarity.Rare => "Rare",
        ItemRarity.Epic => "Epic",
        ItemRarity.Legendary => "Legendary",
        _ => "Common"
    };

    static string FormatEquipmentSlot(LootEquipmentSlot slot) => slot switch
    {
        LootEquipmentSlot.MainHand => "Main Hand",
        LootEquipmentSlot.OffHand => "Off-Hand",
        LootEquipmentSlot.Head => "Head",
        LootEquipmentSlot.Chest => "Chest",
        LootEquipmentSlot.Legs => "Legs",
        LootEquipmentSlot.Feet => "Feet",
        LootEquipmentSlot.Hands => "Hands",
        LootEquipmentSlot.Ring => "Ring",
        LootEquipmentSlot.Trinket => "Trinket",
        _ => ""
    };

    static void AddStat(List<string> values, string label, int amount)
    {
        if (amount != 0) values.Add($"{(amount > 0 ? "+" : "")}{amount} {label}");
    }

    void ResizePanel(bool hasSlot)
    {
        float detailsTop = hasSlot ? 112f : 74f;
        float detailsHeight = _detailsTxt.gameObject.activeSelf
            ? Mathf.Max(20f, _detailsTxt.GetPreferredValues(
                _detailsTxt.text, PanelW - 28f, 0f).y) : 0f;
        SetTopRect(_detailsTxt.rectTransform, 14f, detailsTop, PanelW - 28f, detailsHeight);
        _panelRT.sizeDelta = new Vector2(
            PanelW, Mathf.Max(MinimumPanelH, detailsTop + detailsHeight + 14f));
    }

    // ── Build UI ──────────────────────────────────────────────────────────────
    void BuildUI()
    {
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 200;
        var scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight  = 0.5f;
        var cg = gameObject.AddComponent<CanvasGroup>();
        cg.blocksRaycasts = false;
        cg.interactable   = false;

        // Panel root
        var panelGO = new GameObject("TooltipPanel");
        panelGO.transform.SetParent(transform, false);
        _panelRT = panelGO.AddComponent<RectTransform>();
        _panelRT.anchorMin = _panelRT.anchorMax = _panelRT.pivot = new Vector2(0f, 1f);
        _panelRT.sizeDelta = new Vector2(PanelW, MinimumPanelH);

        // Border (slightly larger bg)
        var borderGO = new GameObject("Border", typeof(RectTransform));
        borderGO.transform.SetParent(panelGO.transform, false);
        _border = borderGO.AddComponent<Image>();
        _border.color = ColBorder;
        Stretch(borderGO.GetComponent<RectTransform>(), -1f, 1f);

        // Background
        var bgGO = new GameObject("BG", typeof(RectTransform));
        bgGO.transform.SetParent(panelGO.transform, false);
        var bgImg = bgGO.AddComponent<Image>();
        bgImg.color = ColBg;
        Stretch(bgGO.GetComponent<RectTransform>(), 0f, 0f);

        _nameTxt = MakeTopTMP("Name", panelGO.transform, 14f, 12f, PanelW - 28f,
            27f, 20f, FontStyles.Bold, ColText);
        _rarityTxt = MakeTopTMP("RarityAndType", panelGO.transform, 14f, 40f,
            PanelW - 28f, 22f, 15f, FontStyles.Normal, ColSubtext);
        _slotTopDivider = MakeDivider("EquipmentSlotTopDivider", panelGO.transform, 68f);
        _slotTxt = MakeTopTMP("EquipmentSlot", panelGO.transform, 14f, 70f,
            PanelW - 28f, 30f, 15f, FontStyles.Normal, ColText);
        _slotBottomDivider = MakeDivider("EquipmentSlotBottomDivider", panelGO.transform, 105f);
        _detailsTxt = MakeTopTMP("Details", panelGO.transform, 14f, 112f,
            PanelW - 28f, 80f, 15f, FontStyles.Normal, ColText);
        _detailsTxt.enableWordWrapping = true;
    }

    static void Stretch(RectTransform rt, float inset, float outset)
    {
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(inset,  inset);
        rt.offsetMax = new Vector2(outset, outset);
    }

    static TextMeshProUGUI MakeTopTMP(string name, Transform parent,
        float left, float top, float width, float height,
        float size, FontStyles style, Color col)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        SetTopRect(rt, left, top, width, height);
        var t = go.AddComponent<TextMeshProUGUI>();
        t.fontSize  = size;
        t.fontStyle = style;
        t.color     = col;
        t.alignment = TextAlignmentOptions.Left;
        return t;
    }

    static Image MakeDivider(string name, Transform parent, float top)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        SetTopRect(rect, 14f, top, PanelW - 28f, 1f);
        var image = go.GetComponent<Image>();
        image.raycastTarget = false;
        return image;
    }

    static void SetTopRect(RectTransform rt, float left, float top, float width, float height)
    {
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = new Vector2(left, -top);
        rt.sizeDelta = new Vector2(width, height);
    }
}
#endif
