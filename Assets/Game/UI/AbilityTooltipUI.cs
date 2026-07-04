#if !UNITY_SERVER
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

/// <summary>
/// AbilityTooltipUI — self-bootstrapping singleton tooltip for spellbook cards.
/// Call Show(ability, screenPos) from AbilityBar card hover; Hide() on leave.
/// Canvas sort order 201 (above spellbook panel at ~100).
/// </summary>
public class AbilityTooltipUI : MonoBehaviour
{
    // ── Bootstrap ─────────────────────────────────────────────────────────────
    public static AbilityTooltipUI Instance { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Bootstrap()
    {
        if (Instance != null) return;
        var go = new GameObject("[AbilityTooltipUI]");
        DontDestroyOnLoad(go);
        Instance = go.AddComponent<AbilityTooltipUI>();
    }

    // ── Colours ───────────────────────────────────────────────────────────────
    static readonly Color ColBg       = new Color(0.04f, 0.03f, 0.10f, 0.97f);
    static readonly Color ColBorder   = new Color(0.28f, 0.22f, 0.50f, 1.00f);
    static readonly Color ColName     = new Color(0.95f, 0.95f, 1.00f, 1.00f);
    static readonly Color ColStat     = new Color(0.70f, 0.85f, 1.00f, 0.90f);
    static readonly Color ColDesc     = new Color(0.80f, 0.80f, 0.85f, 0.85f);
    static readonly Color ColDmg      = new Color(1.00f, 0.45f, 0.20f, 1.00f);
    static readonly Color ColHeal     = new Color(0.20f, 0.90f, 0.40f, 1.00f);
    static readonly Color ColSupport  = new Color(0.30f, 0.65f, 1.00f, 1.00f);

    // ── UI refs ───────────────────────────────────────────────────────────────
    RectTransform   _panel;
    Image           _border;
    TextMeshProUGUI _nameTxt;
    TextMeshProUGUI _statTxt;
    TextMeshProUGUI _descTxt;

    const float W      = 260f;
    const float H      = 120f;
    const float Offset = 18f;

    // ── Lifecycle ─────────────────────────────────────────────────────────────
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        BuildUI();
        _panel.gameObject.SetActive(false);
    }

    // ── Public API ────────────────────────────────────────────────────────────
    public void Show(AbilityDef ability, Vector2 screenPos)
    {
        if (ability == null) { Hide(); return; }

        _nameTxt.text  = ability.abilityName;
        _nameTxt.color = CategoryColor(ability.category);

        // Stat line: type · damage · CD · range
        string shape = ShapeLabel(ability.shape);
        string dmg   = ability.damage > 0f
            ? (ability.maxChargeDamage > ability.damage
                ? $"{ability.damage:0}–{ability.maxChargeDamage:0} dmg"
                : $"{ability.damage:0} dmg")
            : "";
        string cd    = $"{ability.cooldown:0}s CD";
        string range = ability.range > 0f ? $"{ability.range:0}u range" : "self";
        string stats = string.Join("  ·  ", new[] { shape, dmg, cd, range }
            .Where(s => !string.IsNullOrEmpty(s)));
        _statTxt.text = stats;

        _descTxt.text = BuildDescription(ability);

        Position(screenPos);
        _panel.gameObject.SetActive(true);
    }

    public void Hide()
    {
        if (_panel != null) _panel.gameObject.SetActive(false);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    void Position(Vector2 screenPos)
    {
        float x = screenPos.x + Offset;
        float y = screenPos.y + Offset;
        if (x + W > Screen.width)  x = screenPos.x - W - Offset;
        if (y + H > Screen.height) y = screenPos.y - H - Offset;
        _panel.position = new Vector3(x, y, 0f);
    }

    Color CategoryColor(AbilityCategory cat)
    {
        switch (cat)
        {
            case AbilityCategory.Heal:    return ColHeal;
            case AbilityCategory.Support: return ColSupport;
            default:                      return ColDmg;
        }
    }

    static string ShapeLabel(AbilityShape shape)
    {
        switch (shape)
        {
            case AbilityShape.Cone:       return "Cone";
            case AbilityShape.Rectangle:  return "Line";
            default:                      return "AoE";
        }
    }

    static string BuildDescription(AbilityDef ability)
    {
        if (ability == null) return "No description.";
        if (ability.healAmount > 0f) return $"Restores {ability.healAmount:0} health.";
        if (ability.shieldAbsorb > 0f) return $"Grants a {ability.shieldAbsorb:0} shield for {ability.shieldDuration:0}s.";
        if (ability.damage > 0f) return $"Deals {ability.damage:0} damage to valid targets.";
        if (ability.spawnTurret) return "Deploys a combat support turret.";
        if (ability.abilityName.IndexOf("Step", System.StringComparison.OrdinalIgnoreCase) >= 0) return "Teleports to the targeted location.";
        if (ability.abilityName.IndexOf("Veil", System.StringComparison.OrdinalIgnoreCase) >= 0) return "Enters stealth for a short duration.";
        if (ability.abilityName.IndexOf("Stance", System.StringComparison.OrdinalIgnoreCase) >= 0) return "Raises your defensive presence for a short duration.";
        if (ability.abilityName.IndexOf("Snare", System.StringComparison.OrdinalIgnoreCase) >= 0) return "Slows enemies caught in the effect.";
        if (ability.abilityName.IndexOf("Slam", System.StringComparison.OrdinalIgnoreCase) >= 0) return "Disrupts enemies caught in the effect.";
        return "Support ability.";
    }

    // ── Build UI ──────────────────────────────────────────────────────────────
    void BuildUI()
    {
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 201;
        var scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight  = 0.5f;
        var cg = gameObject.AddComponent<CanvasGroup>();
        cg.blocksRaycasts = false;
        cg.interactable   = false;

        var panelGO = new GameObject("AbilityTooltipPanel");
        panelGO.transform.SetParent(transform, false);
        _panel = panelGO.AddComponent<RectTransform>();
        _panel.anchorMin = _panel.anchorMax = _panel.pivot = new Vector2(0f, 0f);
        _panel.sizeDelta = new Vector2(W, H);

        // Border
        var borderGO = new GameObject("Border");
        borderGO.transform.SetParent(panelGO.transform, false);
        _border = borderGO.AddComponent<Image>();
        _border.color = ColBorder;
        Stretch(borderGO.GetComponent<RectTransform>(), -2f, 2f);

        // Background
        var bgGO = new GameObject("BG");
        bgGO.transform.SetParent(panelGO.transform, false);
        bgGO.AddComponent<Image>().color = ColBg;
        Stretch(bgGO.GetComponent<RectTransform>(), 0f, 0f);

        // Ability name (top 22%)
        _nameTxt = MakeTMP("Name", panelGO.transform,
            new Vector2(0f, 0.78f), new Vector2(1f, 1f), 13f, FontStyles.Bold, ColName);

        // Stat line (next 18%)
        _statTxt = MakeTMP("Stats", panelGO.transform,
            new Vector2(0f, 0.60f), new Vector2(1f, 0.78f), 9f, FontStyles.Normal, ColStat);

        // Description (bottom 60%)
        _descTxt = MakeTMP("Desc", panelGO.transform,
            new Vector2(0f, 0f), new Vector2(1f, 0.60f), 9.5f, FontStyles.Normal, ColDesc);
        _descTxt.enableWordWrapping = true;
        _descTxt.alignment = TextAlignmentOptions.TopLeft;
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
        rt.offsetMin = new Vector2(8f, 2f); rt.offsetMax = new Vector2(-8f, -2f);
        var t = go.AddComponent<TextMeshProUGUI>();
        t.fontSize  = size;
        t.fontStyle = style;
        t.color     = col;
        t.alignment = TextAlignmentOptions.TopLeft;
        return t;
    }
}
#endif
