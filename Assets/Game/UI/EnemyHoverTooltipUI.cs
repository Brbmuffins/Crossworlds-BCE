#if UNITY_EDITOR || !UNITY_SERVER
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Small top-screen enemy tooltip shown by EnemyHoverController.
/// Self-bootstraps so scenes and enemy prefabs do not need manual UI setup.
/// </summary>
public class EnemyHoverTooltipUI : MonoBehaviour
{
    public static EnemyHoverTooltipUI Instance { get; private set; }

    static readonly Color ColBorder = new Color(0.13f, 0.85f, 0.95f, 0.92f);
    static readonly Color ColBg = new Color(0.02f, 0.025f, 0.035f, 0.86f);
    static readonly Color ColName = new Color(0.96f, 0.99f, 1f, 1f);
    static readonly Color ColLevelBg = new Color(0.10f, 0.18f, 0.22f, 0.96f);
    static readonly Color ColLevel = new Color(0.70f, 0.95f, 1f, 1f);

    const float PanelWidth = 340f;
    const float PanelHeight = 42f;

    RectTransform _panel;
    RectTransform _nameRect;
    TextMeshProUGUI _nameText;
    TextMeshProUGUI _levelText;
    GameObject _levelChip;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Bootstrap()
    {
        EnsureInstance();
    }

    public static EnemyHoverTooltipUI EnsureInstance()
    {
        if (Instance != null)
            return Instance;

        var go = new GameObject("[EnemyHoverTooltipUI]");
        DontDestroyOnLoad(go);
        Instance = go.AddComponent<EnemyHoverTooltipUI>();
        return Instance;
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        BuildUI();
        Hide();
    }

    public void Show(Health enemy)
    {
        if (enemy == null)
        {
            Hide();
            return;
        }

        Show(enemy.EnemyHoverDisplayName, enemy.HasEnemyLevel ? enemy.EnemyLevel : 0);
    }

    public void Show(string displayName, int level)
    {
        if (_panel == null)
            BuildUI();

        if (string.IsNullOrWhiteSpace(displayName))
            displayName = "Enemy";

        _nameText.text = displayName.Trim();

        bool hasLevel = level > 0;
        _levelChip.SetActive(hasLevel);
        _levelText.text = hasLevel ? $"Lv {level}" : "";
        if (_nameRect != null)
            _nameRect.offsetMax = new Vector2(hasLevel ? -88f : -14f, 0f);

        if (!_panel.gameObject.activeSelf)
            _panel.gameObject.SetActive(true);
    }

    public void Hide()
    {
        if (_panel != null)
            _panel.gameObject.SetActive(false);
    }

    void BuildUI()
    {
        if (_panel != null)
            return;

        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 220;

        var scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        var group = gameObject.AddComponent<CanvasGroup>();
        group.blocksRaycasts = false;
        group.interactable = false;

        var panelGO = new GameObject("EnemyHoverPanel", typeof(RectTransform));
        panelGO.transform.SetParent(transform, false);
        _panel = panelGO.GetComponent<RectTransform>();
        _panel.anchorMin = _panel.anchorMax = _panel.pivot = new Vector2(0.5f, 1f);
        _panel.anchoredPosition = new Vector2(0f, -22f);
        _panel.sizeDelta = new Vector2(PanelWidth, PanelHeight);

        var borderGO = new GameObject("Border", typeof(RectTransform), typeof(Image));
        borderGO.transform.SetParent(panelGO.transform, false);
        var border = borderGO.GetComponent<Image>();
        border.color = ColBorder;
        Stretch(borderGO.GetComponent<RectTransform>(), -1f, 1f);

        var bgGO = new GameObject("BG", typeof(RectTransform), typeof(Image));
        bgGO.transform.SetParent(panelGO.transform, false);
        var bg = bgGO.GetComponent<Image>();
        bg.color = ColBg;
        Stretch(bgGO.GetComponent<RectTransform>(), 0f, 0f);

        _nameText = MakeText("Name", panelGO.transform, 16f, FontStyles.Bold, ColName);
        _nameRect = _nameText.GetComponent<RectTransform>();
        _nameRect.anchorMin = new Vector2(0f, 0f);
        _nameRect.anchorMax = new Vector2(1f, 1f);
        _nameRect.offsetMin = new Vector2(14f, 0f);
        _nameRect.offsetMax = new Vector2(-88f, 0f);
        _nameText.alignment = TextAlignmentOptions.MidlineLeft;

        _levelChip = new GameObject("LevelChip", typeof(RectTransform), typeof(Image));
        _levelChip.transform.SetParent(panelGO.transform, false);
        var chipRT = _levelChip.GetComponent<RectTransform>();
        chipRT.anchorMin = chipRT.anchorMax = new Vector2(1f, 0.5f);
        chipRT.pivot = new Vector2(1f, 0.5f);
        chipRT.anchoredPosition = new Vector2(-12f, 0f);
        chipRT.sizeDelta = new Vector2(62f, 24f);
        _levelChip.GetComponent<Image>().color = ColLevelBg;

        _levelText = MakeText("Level", _levelChip.transform, 12f, FontStyles.Bold, ColLevel);
        Stretch(_levelText.GetComponent<RectTransform>(), 0f, 0f);
        _levelText.alignment = TextAlignmentOptions.Center;
    }

    static TextMeshProUGUI MakeText(string name, Transform parent, float size, FontStyles style, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);

        var text = go.GetComponent<TextMeshProUGUI>();
        text.fontSize = size;
        text.fontStyle = style;
        text.color = color;
        text.enableWordWrapping = false;
        text.overflowMode = TextOverflowModes.Ellipsis;
        return text;
    }

    static void Stretch(RectTransform rt, float inset, float outset)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(inset, inset);
        rt.offsetMax = new Vector2(outset, outset);
    }
}
#endif
