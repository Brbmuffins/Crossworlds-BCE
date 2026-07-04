#if !UNITY_SERVER
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// TooltipUI — Static singleton item/ability tooltip.
/// Call Show() on pointer-enter, Hide() on pointer-exit.
///
/// API:
///   TooltipUI.Show(title, subtitle, screenPos)
///   TooltipUI.Show(title, subtitle, description, screenPos)
///   TooltipUI.Hide()
///
/// Self-bootstrapping — no scene object needed.
/// Copy to: Assets/Game/UI/TooltipUI.cs
///
/// NOTE: If a TooltipUI.cs already exists in Assets/Game/Items/Scripts/
/// and causes a CS0101 conflict, delete that older file — this is the
/// authoritative version.
/// </summary>
public class TooltipUI : MonoBehaviour
{
    // ─── Singleton ────────────────────────────────────────────────────────────
    static TooltipUI _instance;

    // ─── UI refs ──────────────────────────────────────────────────────────────
    Canvas          _canvas;
    RectTransform   _panel;
    TextMeshProUGUI _title;
    TextMeshProUGUI _subtitle;
    TextMeshProUGUI _desc;

    static readonly Color RarityCommon   = new Color(0.75f, 0.75f, 0.75f);
    static readonly Color RarityUncommon = new Color(0.12f, 0.85f, 0.12f);
    static readonly Color RarityRare     = new Color(0.20f, 0.50f, 1.00f);
    static readonly Color RarityEpic     = new Color(0.65f, 0.12f, 0.90f);
    static readonly Color RarityLegend  = new Color(1.00f, 0.50f, 0.00f);

    // ─── Bootstrap ────────────────────────────────────────────────────────────
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        var go = new GameObject("[TooltipUI]");
        DontDestroyOnLoad(go);
        _instance = go.AddComponent<TooltipUI>();
        _instance.Build();
    }

    // ─── Public API ───────────────────────────────────────────────────────────
    public static void Show(string title, string subtitle, Vector3 screenPos)
        => Show(title, subtitle, "", screenPos);

    public static void Show(string title, string subtitle, string description, Vector3 screenPos)
    {
        if (_instance == null) return;
        _instance._title.text    = title;
        _instance._subtitle.text = subtitle;
        _instance._desc.text     = description;
        _instance._desc.gameObject.SetActive(!string.IsNullOrEmpty(description));

        // Rarity color on subtitle
        _instance._subtitle.color = GetRarityColor(subtitle);

        _instance._panel.gameObject.SetActive(true);
        _instance.PositionAt(screenPos);
    }

    public static void Hide()
    {
        if (_instance == null) return;
        _instance._panel.gameObject.SetActive(false);
    }

    // ─── Position ─────────────────────────────────────────────────────────────
    void PositionAt(Vector3 screenPos)
    {
        // Offset so tooltip doesn't sit under cursor
        Vector2 pos = new Vector2(screenPos.x + 14f, screenPos.y + 14f);

        // Keep on screen
        float w = _panel.sizeDelta.x;
        float h = _panel.sizeDelta.y;
        if (pos.x + w > Screen.width)  pos.x = screenPos.x - w - 6f;
        if (pos.y + h > Screen.height) pos.y = screenPos.y - h - 6f;

        _panel.position = pos;
    }

    // ─── Build ────────────────────────────────────────────────────────────────
    void Build()
    {
        // Overlay canvas
        var cvs = gameObject.AddComponent<Canvas>();
        cvs.renderMode  = RenderMode.ScreenSpaceOverlay;
        cvs.sortingOrder = 100;   // always on top
        _canvas = cvs;
        gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        // Panel
        var panelGo = new GameObject("Panel");
        panelGo.transform.SetParent(transform, false);
        var bg = panelGo.AddComponent<Image>();
        bg.color = new Color(0.04f, 0.06f, 0.11f, 0.97f);
        _panel = panelGo.GetComponent<RectTransform>();
        _panel.sizeDelta = new Vector2(200f, 80f);
        _panel.pivot     = Vector2.zero;

        // Border
        var border = panelGo.AddComponent<Outline>();
        border.effectColor    = new Color(0.25f, 0.35f, 0.55f, 1f);
        border.effectDistance = new Vector2(1, -1);

        // Vertical layout
        var vlg = panelGo.AddComponent<VerticalLayoutGroup>();
        vlg.padding           = new RectOffset(10, 10, 8, 8);
        vlg.spacing           = 3;
        vlg.childForceExpandWidth  = true;
        vlg.childForceExpandHeight = false;
        vlg.childControlHeight     = true;

        var csf = panelGo.AddComponent<ContentSizeFitter>();
        csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        csf.verticalFit   = ContentSizeFitter.FitMode.PreferredSize;

        // Title
        _title    = MakeText(panelGo, "Title", 14, FontStyles.Bold, Color.white);
        _subtitle = MakeText(panelGo, "Subtitle", 11, FontStyles.Normal, RarityCommon);
        _desc     = MakeText(panelGo, "Desc", 11, FontStyles.Normal, new Color(0.7f, 0.75f, 0.8f));

        _panel.gameObject.SetActive(false);
    }

    static TextMeshProUGUI MakeText(GameObject parent, string name, float size, FontStyles style, Color col)
    {
        var go  = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.fontSize  = size;
        tmp.fontStyle = style;
        tmp.color     = col;
        tmp.enableWordWrapping = true;
        return tmp;
    }

    static Color GetRarityColor(string rarity)
    {
        if (string.IsNullOrEmpty(rarity)) return RarityCommon;
        switch (rarity.ToLower())
        {
            case "uncommon": return RarityUncommon;
            case "rare":     return RarityRare;
            case "epic":     return RarityEpic;
            case "legendary":return RarityLegend;
            default:         return RarityCommon;
        }
    }
}
#endif
