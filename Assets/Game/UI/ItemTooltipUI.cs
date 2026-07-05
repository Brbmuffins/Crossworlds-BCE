#if UNITY_EDITOR || !UNITY_SERVER
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
    static readonly Color ColText    = new Color(0.95f, 0.95f, 1.00f, 1.00f);
    static readonly Color ColSubtext = new Color(0.65f, 0.65f, 0.75f, 0.90f);

    // ── UI refs ───────────────────────────────────────────────────────────────
    RectTransform   _panelRT;
    Image           _border;
    TextMeshProUGUI _nameTxt;
    TextMeshProUGUI _typeTxt;
    TextMeshProUGUI _valueTxt;

    // Tooltip sizing
    const float PanelW   = 180f;
    const float PanelH   = 70f;
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

        string displayName = def?.name ?? FormatId(itemId);
        string typeLine    = def?.item_type != null
            ? System.Globalization.CultureInfo.CurrentCulture.TextInfo
                   .ToTitleCase(def.item_type.Replace('_', ' '))
            : "";
        string valueLine   = def != null && def.sell_value > 0
            ? $"Sell: {def.sell_value}g"
            : "";

        _nameTxt.text  = displayName;
        _nameTxt.color = ColText;
        _typeTxt.text  = typeLine;
        _valueTxt.text = valueLine;

        // Rarity-tint the border
        _border.color = ItemCatalogManager.GetRarityColor(itemId);

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
        if (y - PanelH < 0)             y = screenPos.y + PanelH + Offset;
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
        _panelRT.sizeDelta = new Vector2(PanelW, PanelH);

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

        // Name line (top 40%)
        _nameTxt = MakeTMP("Name", panelGO.transform,
            new Vector2(0f, 0.58f), new Vector2(1f, 1f), 11f, FontStyles.Bold, ColText);

        // Type line (middle 30%)
        _typeTxt = MakeTMP("Type", panelGO.transform,
            new Vector2(0f, 0.28f), new Vector2(1f, 0.58f), 9f, FontStyles.Normal, ColSubtext);

        // Sell value line (bottom 28%)
        _valueTxt = MakeTMP("Value", panelGO.transform,
            new Vector2(0f, 0f), new Vector2(1f, 0.28f), 9f, FontStyles.Normal, ColSubtext);
    }

    static void Stretch(RectTransform rt, float inset, float outset)
    {
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(inset,  inset);
        rt.offsetMax = new Vector2(outset, outset);
    }

    static TextMeshProUGUI MakeTMP(string name, Transform parent,
        Vector2 anchMin, Vector2 anchMax, float size, FontStyles style, Color col)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchMin; rt.anchorMax = anchMax;
        rt.offsetMin = new Vector2(6f, 0f); rt.offsetMax = new Vector2(-6f, 0f);
        var t = go.AddComponent<TextMeshProUGUI>();
        t.fontSize  = size;
        t.fontStyle = style;
        t.color     = col;
        t.alignment = TextAlignmentOptions.Left;
        return t;
    }
}
#endif
