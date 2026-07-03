#if !UNITY_SERVER
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// TooltipUI — self-bootstrapping singleton.
/// Auto-creates its own Canvas, DontDestroyOnLoad.
/// No Inspector assignment required.
///
/// Usage:
///   TooltipUI.Instance.Show(item, screenPos);
///   TooltipUI.Instance.Show("Raw text", screenPos);
///   TooltipUI.Instance.Hide();
/// </summary>
public class TooltipUI : MonoBehaviour
{
    public static TooltipUI Instance { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        if (Instance != null) return;
        var go = new GameObject("[TooltipUI]");
        DontDestroyOnLoad(go);
        Instance = go.AddComponent<TooltipUI>();
    }

    // ── UI refs ───────────────────────────────────────────────────────────────
    Canvas          _canvas;
    RectTransform   _panelRect;
    TextMeshProUGUI _label;
    Image           _bgImage;

    // ── Lifecycle ─────────────────────────────────────────────────────────────
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        BuildUI();
        _panelRect.gameObject.SetActive(false);
    }

    // ── Public API ────────────────────────────────────────────────────────────

    public void Show(ItemData item, Vector2 screenPosition, string hintOverride = null)
    {
        if (item == null) return;

        string colorHex = ColorUtility.ToHtmlStringRGB(item.RarityColor);
        string text = $"<b><color=#{colorHex}>{item.itemName}</color></b>";

        if (!string.IsNullOrEmpty(item.description))
            text += $"\n{item.description}";

        string hint = hintOverride ?? GetHint(item);
        if (!string.IsNullOrEmpty(hint))
            text += $"\n<color=#AAAAAA><i>{hint}</i></color>";

        ShowRaw(text, screenPosition);
    }

    public void Show(string text, Vector2 screenPosition) => ShowRaw(text, screenPosition);

    public void Hide()
    {
        if (_panelRect != null) _panelRect.gameObject.SetActive(false);
    }

    // ── Internal ──────────────────────────────────────────────────────────────
    void ShowRaw(string text, Vector2 screenPos)
    {
        _label.text = text;

        // Auto-size
        float lineCount = text.Split('\n').Length;
        _panelRect.sizeDelta = new Vector2(
            Mathf.Clamp(text.Length * 5f + 20f, 130f, 280f),
            lineCount * 20f + 14f
        );

        // Position — keep inside screen
        float w = _panelRect.sizeDelta.x;
        float h = _panelRect.sizeDelta.y;
        float x = Mathf.Clamp(screenPos.x, 0f, Screen.width  - w);
        float y = Mathf.Clamp(screenPos.y, h, Screen.height);
        _panelRect.position = new Vector3(x, y, 0f);

        _panelRect.gameObject.SetActive(true);
    }

    static string GetHint(ItemData item)
    {
        if (item.itemType == ItemType.Consumable) return "Click to use";
        if (item.equippable) return "Click to equip";
        return "";
    }

    // ── Build UI ──────────────────────────────────────────────────────────────
    void BuildUI()
    {
        // Canvas — highest sorting order, overlay
        var cGO = new GameObject("TooltipCanvas");
        cGO.transform.SetParent(transform, false);
        _canvas = cGO.AddComponent<Canvas>();
        _canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = 200; // above everything
        var scaler = cGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight  = 0.5f;
        cGO.AddComponent<GraphicRaycaster>();

        // Panel
        var panelGO = new GameObject("TooltipPanel");
        panelGO.transform.SetParent(cGO.transform, false);
        _panelRect        = panelGO.AddComponent<RectTransform>();
        _panelRect.pivot  = new Vector2(0f, 1f); // anchor top-left of panel to cursor
        _panelRect.sizeDelta = new Vector2(180f, 50f);

        _bgImage       = panelGO.AddComponent<Image>();
        _bgImage.color = new Color(0.04f, 0.04f, 0.08f, 0.92f);

        // Border (child outline image)
        var borderGO = new GameObject("Border");
        borderGO.transform.SetParent(panelGO.transform, false);
        var borderRT = borderGO.AddComponent<RectTransform>();
        borderRT.anchorMin = Vector2.zero;
        borderRT.anchorMax = Vector2.one;
        borderRT.offsetMin = new Vector2(-1f, -1f);
        borderRT.offsetMax = new Vector2( 1f,  1f);
        borderGO.AddComponent<Image>().color = new Color(0.4f, 0.35f, 0.6f, 0.7f);
        borderGO.transform.SetAsFirstSibling();

        // Label
        var labelGO = new GameObject("TooltipText");
        labelGO.transform.SetParent(panelGO.transform, false);
        var labelRT = labelGO.AddComponent<RectTransform>();
        labelRT.anchorMin = Vector2.zero;
        labelRT.anchorMax = Vector2.one;
        labelRT.offsetMin = new Vector2(7f, 5f);
        labelRT.offsetMax = new Vector2(-7f, -5f);
        _label           = labelGO.AddComponent<TextMeshProUGUI>();
        _label.fontSize  = 13f;
        _label.color     = Color.white;
        _label.alignment = TextAlignmentOptions.TopLeft;
        _label.richText  = true;
        _label.enableWordWrapping = false;
    }
}
#endif
