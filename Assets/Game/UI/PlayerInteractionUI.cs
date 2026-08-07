#if UNITY_EDITOR || !UNITY_SERVER
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Client-only action card opened by clicking a remote player's nameplate.
/// Feature systems subscribe to InteractionRequested and perform their own
/// server-authoritative request/validation; this UI never grants game state.
/// </summary>
public sealed class PlayerInteractionUI : MonoBehaviour
{
    public enum Action
    {
        Trade,
        Duel
    }

    public static event System.Action<PlayerIdentity, Action> InteractionRequested;

    static PlayerInteractionUI _instance;
    static PlayerIdentity _target;

    CanvasGroup _group;
    TextMeshProUGUI _name;
    TextMeshProUGUI _class;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        if (_instance != null) return;
        var go = new GameObject("PlayerInteractionUI");
        DontDestroyOnLoad(go);
        _instance = go.AddComponent<PlayerInteractionUI>();
    }

    public static void Show(PlayerIdentity target)
    {
        if (target == null || target.isLocalPlayer) return;
        if (_instance == null) Bootstrap();
        _target = target;
        _instance.RefreshTarget();
        _instance.SetVisible(true);
    }

    public static void Hide()
    {
        if (_instance != null) _instance.SetVisible(false);
        _target = null;
    }

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        BuildUI();
        SetVisible(false);
    }

    void Update()
    {
        if (_group == null || _group.alpha <= 0f) return;
        if (_target == null || !_target.isActiveAndEnabled)
        {
            Hide();
            return;
        }

        if (UnityEngine.InputSystem.Keyboard.current?.escapeKey.wasPressedThisFrame == true)
            Hide();
    }

    void RefreshTarget()
    {
        if (_target == null) return;
        _name.text = string.IsNullOrWhiteSpace(_target.playerName)
            ? "Player"
            : _target.playerName;
        _class.text = _target.ClassName;
    }

    void Request(Action action)
    {
        if (_target == null) return;

        var handler = InteractionRequested;
        if (handler == null)
        {
            RodChatManager.Instance?.AddSystemMessage(
                $"{action} requests are not connected yet.");
            return;
        }

        handler.Invoke(_target, action);
        Hide();
    }

    void SetVisible(bool visible)
    {
        if (_group == null) return;
        _group.alpha = visible ? 1f : 0f;
        _group.interactable = visible;
        _group.blocksRaycasts = visible;
    }

    void BuildUI()
    {
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 160;

        var scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        gameObject.AddComponent<GraphicRaycaster>();
        _group = gameObject.AddComponent<CanvasGroup>();

        RectTransform root = canvas.GetComponent<RectTransform>();
        var panel = new GameObject("PlayerActionCard", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(root, false);
        var panelRt = panel.GetComponent<RectTransform>();
        panelRt.anchorMin = panelRt.anchorMax = new Vector2(0.5f, 0.5f);
        panelRt.pivot = new Vector2(0.5f, 0.5f);
        panelRt.anchoredPosition = new Vector2(0f, 40f);
        panelRt.sizeDelta = new Vector2(300f, 210f);
        panel.GetComponent<Image>().color = new Color(0.035f, 0.025f, 0.09f, 0.97f);

        _name = MakeLabel(panelRt, "PlayerName", new Vector2(0.07f, 0.73f),
            new Vector2(0.93f, 0.94f), 22f, FontStyles.Bold);
        _class = MakeLabel(panelRt, "ClassName", new Vector2(0.07f, 0.61f),
            new Vector2(0.93f, 0.76f), 13f, FontStyles.Normal);
        _class.color = new Color(0.45f, 0.75f, 1f);

        MakeButton(panelRt, "Trade", new Vector2(0.08f, 0.34f),
            new Vector2(0.92f, 0.55f), () => Request(Action.Trade));
        MakeButton(panelRt, "Duel", new Vector2(0.08f, 0.10f),
            new Vector2(0.92f, 0.31f), () => Request(Action.Duel));
        MakeButton(panelRt, "×", new Vector2(0.87f, 0.82f),
            new Vector2(0.97f, 0.96f), Hide, transparent: true);
    }

    static TextMeshProUGUI MakeLabel(RectTransform parent, string name,
        Vector2 anchorMin, Vector2 anchorMax, float size, FontStyles style)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        var label = go.GetComponent<TextMeshProUGUI>();
        label.fontSize = size;
        label.fontStyle = style;
        label.alignment = TextAlignmentOptions.Center;
        label.color = Color.white;
        label.raycastTarget = false;
        return label;
    }

    static void MakeButton(RectTransform parent, string text, Vector2 anchorMin,
        Vector2 anchorMax, UnityAction onClick, bool transparent = false)
    {
        var go = new GameObject(text + "Button", typeof(RectTransform),
            typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        go.GetComponent<Image>().color = transparent
            ? Color.clear
            : new Color(0.13f, 0.10f, 0.27f, 1f);
        go.GetComponent<Button>().onClick.AddListener(onClick);

        var label = MakeLabel(rt, "Label", Vector2.zero, Vector2.one,
            transparent ? 20f : 14f, FontStyles.Bold);
        label.text = text;
    }
}
#endif
