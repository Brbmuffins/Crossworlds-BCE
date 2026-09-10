using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR || !UNITY_SERVER
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public sealed class WaypointMapUI : MonoBehaviour
{
    const float SolidLineThickness = 3f;
    const float DashedLineThickness = 2f;
    const float DashLength = 22f;
    const float DashGap = 16f;

    static WaypointMapUI _instance;

    public static bool IsVisible =>
        _instance != null && _instance._panel != null && _instance._panel.activeSelf;

    Canvas _canvas;
    GameObject _panel;
    RectTransform _mapArea;
    RectTransform _lineLayer;
    RectTransform _nodeLayer;
    TextMeshProUGUI _title;
    TextMeshProUGUI _status;
    Image _mapImage;
    bool _usesIllustratedBackground;
    static Sprite _hotspotRingSprite;
    Action<WaypointMapNode> _onNodeSelected;
    string _currentSceneName;

    public static void Show(
        string title,
        Sprite background,
        WaypointMapNode[] nodes,
        WaypointMapConnection[] connections,
        Action<WaypointMapNode> onNodeSelected)
    {
        EnsureInstance().Open(title, background, nodes, connections, onNodeSelected);
    }

    public static void Hide()
    {
        if (_instance != null)
            _instance.Close();
    }

    public static void SetStatus(string message)
    {
        if (_instance != null)
            _instance.SetStatusText(message);
    }

    static WaypointMapUI EnsureInstance()
    {
        if (_instance != null)
            return _instance;

        var go = new GameObject("WaypointMapUI");
        DontDestroyOnLoad(go);
        _instance = go.AddComponent<WaypointMapUI>();
        return _instance;
    }

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        Build();
        Close();
    }

    void Update()
    {
        if (_panel == null || !_panel.activeSelf)
            return;

        var keyboard = Keyboard.current;
        if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
            Close();
    }

    void Open(
        string title,
        Sprite background,
        WaypointMapNode[] nodes,
        WaypointMapConnection[] connections,
        Action<WaypointMapNode> onNodeSelected)
    {
        EnsureEventSystem();

        _onNodeSelected = onNodeSelected;
        _title.text = string.IsNullOrWhiteSpace(title) ? "WORLD MAP" : title;
        _mapImage.sprite = background;
        _usesIllustratedBackground = background != null;
        _title.gameObject.SetActive(!_usesIllustratedBackground);
        _mapArea.anchorMin = _usesIllustratedBackground
            ? new Vector2(0.03f, 0.14f)
            : new Vector2(0.03f, 0.16f);
        _mapArea.anchorMax = _usesIllustratedBackground
            ? new Vector2(0.97f, 0.97f)
            : new Vector2(0.97f, 0.88f);
        _mapImage.color = background != null
            ? Color.white
            : new Color(0.08f, 0.06f, 0.045f, 1f);
        _mapImage.type = Image.Type.Simple;
        _mapImage.preserveAspect = background != null;

        _panel.SetActive(true);
        ClearLayer(_lineLayer);
        ClearLayer(_nodeLayer);
        SetStatusText("Select a destination.");

        _currentSceneName = PlayerIdentity.Local != null && PlayerIdentity.Local.gameObject.scene.IsValid()
            ? PlayerIdentity.Local.gameObject.scene.name
            : SceneManager.GetActiveScene().name;

        Canvas.ForceUpdateCanvases();
        if (!_usesIllustratedBackground)
        {
            DrawBarrierPlaceholder();
            DrawConnections(nodes, connections);
        }
        DrawCurrentLocation(nodes);
        DrawNodes(nodes);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Close()
    {
        if (_panel != null)
            _panel.SetActive(false);

        _onNodeSelected = null;
    }

    void SetStatusText(string message)
    {
        if (_status != null)
            _status.text = string.IsNullOrWhiteSpace(message) ? "Select a destination." : message;
    }

    void Build()
    {
        var cgo = new GameObject("WaypointMapCanvas",
            typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        cgo.transform.SetParent(transform, false);

        _canvas = cgo.GetComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = 220;

        var scaler = cgo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        RectTransform root = _canvas.GetComponent<RectTransform>();
        _panel = MakeRect("Panel", root, Vector2.zero, Vector2.one);

        GameObject overlay = MakeRect("Overlay", _panel.GetComponent<RectTransform>(), Vector2.zero, Vector2.one);
        Img(overlay, new Color(0f, 0f, 0f, 0.72f));

        GameObject frame = MakeRect("Frame", _panel.GetComponent<RectTransform>(),
            new Vector2(0.06f, 0.06f), new Vector2(0.94f, 0.94f));
        Img(frame, new Color(0.045f, 0.035f, 0.03f, 0.98f));
        RectTransform frameRt = frame.GetComponent<RectTransform>();

        _title = MakeTmp("Title", frameRt, new Vector2(0.03f, 0.90f), new Vector2(0.86f, 0.98f));
        _title.text = "WORLD MAP";
        _title.fontSize = 22f;
        _title.fontStyle = FontStyles.Bold;
        _title.color = new Color(0.95f, 0.88f, 0.68f, 1f);
        _title.alignment = TextAlignmentOptions.MidlineLeft;

        MakeButton("Close", frameRt, new Vector2(0.88f, 0.91f), new Vector2(0.97f, 0.97f),
            new Color(0.25f, 0.09f, 0.08f, 1f), Close);

        GameObject map = MakeRect("MapArea", frameRt, new Vector2(0.03f, 0.16f), new Vector2(0.97f, 0.88f));
        _mapImage = map.AddComponent<Image>();
        _mapImage.color = new Color(0.08f, 0.06f, 0.045f, 1f);
        _mapArea = map.GetComponent<RectTransform>();

        _lineLayer = MakeRect("Lines", _mapArea, Vector2.zero, Vector2.one).GetComponent<RectTransform>();
        _nodeLayer = MakeRect("Nodes", _mapArea, Vector2.zero, Vector2.one).GetComponent<RectTransform>();

        _status = MakeTmp("Status", frameRt, new Vector2(0.03f, 0.04f), new Vector2(0.97f, 0.13f));
        _status.text = "Select a destination.";
        _status.fontSize = 14f;
        _status.color = new Color(0.78f, 0.78f, 0.72f, 1f);
        _status.alignment = TextAlignmentOptions.MidlineLeft;
    }

    void DrawNodes(WaypointMapNode[] nodes)
    {
        if (nodes == null)
            return;

        foreach (WaypointMapNode node in nodes)
        {
            if (node == null)
                continue;

            DrawNode(node);
        }
    }

    void DrawNode(WaypointMapNode node)
    {
        Vector2 local = MapToLocal(node.normalizedPosition);
        Color nodeColor = node.unlocked ? node.color : new Color(0.55f, 0.5f, 0.5f, 1f);

        GameObject buttonGo = MakeRect(node.id + "_Node", _nodeLayer, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        RectTransform buttonRt = buttonGo.GetComponent<RectTransform>();
        buttonRt.anchoredPosition = local;
        buttonRt.sizeDelta = _usesIllustratedBackground
            ? new Vector2(64f, 64f)
            : new Vector2(28f, 28f);

        Image icon = buttonGo.AddComponent<Image>();
        icon.sprite = _usesIllustratedBackground ? GetHotspotRingSprite() : node.icon;
        icon.color = _usesIllustratedBackground
            ? Color.clear
            : node.icon != null ? Color.white : nodeColor;

        Button button = buttonGo.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = _usesIllustratedBackground ? Color.clear : Color.white;
        colors.highlightedColor = _usesIllustratedBackground
            ? new Color(1f, 0.82f, 0.25f, 0.8f)
            : new Color(1.5f, 1.5f, 1.5f, 1f);
        colors.pressedColor = _usesIllustratedBackground
            ? new Color(1f, 0.68f, 0.12f, 1f)
            : new Color(0.7f, 0.7f, 0.7f, 1f);
        colors.selectedColor = colors.highlightedColor;
        button.colors = colors;
        button.onClick.AddListener(() => SelectNode(node));

        string hoverText = node.CanTravel
            ? (string.IsNullOrWhiteSpace(node.description) ? node.displayName : node.description)
            : $"{node.displayName}: coming soon.";
        EventTrigger et = buttonGo.AddComponent<EventTrigger>();
        AddPointerEvent(et, EventTriggerType.PointerEnter, _ => SetStatusText(hoverText));
        AddPointerEvent(et, EventTriggerType.PointerExit, _ => SetStatusText("Select a destination."));

        if (_usesIllustratedBackground)
            return;

        GameObject ring = MakeRect(node.id + "_Ring", buttonRt, Vector2.zero, Vector2.one);
        Image ringImage = ring.AddComponent<Image>();
        ringImage.color = new Color(1f, 1f, 1f, node.unlocked ? 0.22f : 0.12f);
        ringImage.raycastTarget = false;

        TextMeshProUGUI label = MakeTmp(node.id + "_Label", _nodeLayer,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        RectTransform labelRt = label.GetComponent<RectTransform>();
        labelRt.anchoredPosition = local + node.labelOffset;
        labelRt.sizeDelta = new Vector2(220f, 54f);
        label.text = string.IsNullOrWhiteSpace(node.subtitle)
            ? node.displayName
            : $"{node.displayName}\n<size=70%><color=#8f877b>{node.subtitle}</color></size>";
        label.fontSize = 13f;
        label.fontStyle = FontStyles.Bold;
        label.color = node.unlocked ? new Color(1f, 0.92f, 0.72f, 1f) : new Color(0.75f, 0.7f, 0.66f, 1f);
        label.alignment = TextAlignmentOptions.Center;
        label.raycastTarget = false;
    }

    void SelectNode(WaypointMapNode node)
    {
        string detail = string.IsNullOrWhiteSpace(node.description) ? node.displayName : node.description;
        SetStatusText(node.CanTravel ? $"→ {detail}" : $"{node.displayName}: coming soon.");
        _onNodeSelected?.Invoke(node);
    }

    void DrawConnections(WaypointMapNode[] nodes, WaypointMapConnection[] connections)
    {
        if (nodes == null || connections == null)
            return;

        Dictionary<string, WaypointMapNode> byId = new Dictionary<string, WaypointMapNode>();
        foreach (WaypointMapNode node in nodes)
        {
            if (node != null && !string.IsNullOrWhiteSpace(node.id))
                byId[node.id] = node;
        }

        foreach (WaypointMapConnection connection in connections)
        {
            if (connection == null)
                continue;
            if (!byId.TryGetValue(connection.fromNodeId, out WaypointMapNode from))
                continue;
            if (!byId.TryGetValue(connection.toNodeId, out WaypointMapNode to))
                continue;

            Vector2 a = MapToLocal(from.normalizedPosition);
            Vector2 b = MapToLocal(to.normalizedPosition);
            if (connection.dashed)
                DrawDashedLine(a, b, connection.color);
            else
                DrawLine(a, b, connection.color, SolidLineThickness);
        }
    }

    void DrawLine(Vector2 a, Vector2 b, Color color, float thickness)
    {
        Vector2 delta = b - a;
        float length = delta.magnitude;
        if (length <= 0.001f)
            return;

        GameObject line = MakeRect("Line", _lineLayer, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        RectTransform rt = line.GetComponent<RectTransform>();
        rt.anchoredPosition = (a + b) * 0.5f;
        rt.sizeDelta = new Vector2(length, thickness);
        rt.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg);
        Img(line, color);
    }

    void DrawDashedLine(Vector2 a, Vector2 b, Color color)
    {
        Vector2 delta = b - a;
        float totalLength = delta.magnitude;
        if (totalLength <= 0.001f)
            return;

        Vector2 direction = delta / totalLength;
        for (float start = 0f; start < totalLength; start += DashLength + DashGap)
        {
            float end = Mathf.Min(start + DashLength, totalLength);
            Vector2 dashA = a + direction * start;
            Vector2 dashB = a + direction * end;
            DrawLine(dashA, dashB, color, DashedLineThickness);
        }
    }

    void DrawCurrentLocation(WaypointMapNode[] nodes)
    {
        if (nodes == null || string.IsNullOrWhiteSpace(_currentSceneName))
            return;

        foreach (WaypointMapNode node in nodes)
        {
            if (node == null) continue;
            if (!string.Equals(node.sceneName, _currentSceneName, StringComparison.OrdinalIgnoreCase))
                continue;

            Vector2 local = MapToLocal(node.normalizedPosition);

            // Golden pulse ring — drawn before nodes so it sits behind them
            GameObject glow = MakeRect("CurrentLocationGlow", _nodeLayer,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            RectTransform glowRt = glow.GetComponent<RectTransform>();
            glowRt.anchoredPosition = local;
            glowRt.sizeDelta = _usesIllustratedBackground
                ? new Vector2(64f, 64f)
                : new Vector2(52f, 52f);
            Image glowImg = glow.AddComponent<Image>();
            if (_usesIllustratedBackground)
                glowImg.sprite = GetHotspotRingSprite();
            glowImg.color = new Color(1f, 0.92f, 0.32f, 0.55f);
            glowImg.raycastTarget = false;

            if (_usesIllustratedBackground)
                break;

            // "— you are here —" text below the node label
            TextMeshProUGUI hereLabel = MakeTmp("CurrentLocationLabel", _nodeLayer,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            RectTransform hereLabelRt = hereLabel.GetComponent<RectTransform>();
            hereLabelRt.anchoredPosition = local + node.labelOffset + new Vector2(0f, -18f);
            hereLabelRt.sizeDelta = new Vector2(130f, 18f);
            hereLabel.text = "— you are here —";
            hereLabel.fontSize = 9f;
            hereLabel.color = new Color(1f, 0.92f, 0.4f, 0.85f);
            hereLabel.alignment = TextAlignmentOptions.Center;
            hereLabel.raycastTarget = false;
            break;
        }
    }

    static void AddPointerEvent(EventTrigger trigger, EventTriggerType type,
        UnityEngine.Events.UnityAction<BaseEventData> action)
    {
        var entry = new EventTrigger.Entry { eventID = type };
        entry.callback.AddListener(action);
        trigger.triggers.Add(entry);
    }

    static Sprite GetHotspotRingSprite()
    {
        if (_hotspotRingSprite != null)
            return _hotspotRingSprite;

        const int size = 64;
        const float innerRadius = 22f;
        const float outerRadius = 29f;
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false, true)
        {
            name = "WaypointHotspotRing",
            hideFlags = HideFlags.HideAndDontSave,
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp
        };
        var pixels = new Color32[size * size];
        Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float radius = Vector2.Distance(new Vector2(x, y), center);
                byte alpha = radius >= innerRadius && radius <= outerRadius ? (byte)255 : (byte)0;
                pixels[y * size + x] = new Color32(255, 255, 255, alpha);
            }
        }

        texture.SetPixels32(pixels);
        texture.Apply(false, true);
        _hotspotRingSprite = Sprite.Create(texture, new Rect(0f, 0f, size, size),
            new Vector2(0.5f, 0.5f), 100f);
        _hotspotRingSprite.name = "WaypointHotspotRing";
        _hotspotRingSprite.hideFlags = HideFlags.HideAndDontSave;
        return _hotspotRingSprite;
    }

    void DrawBarrierPlaceholder()
    {
        Vector2 local = MapToLocal(new Vector2(0.85f, 0.62f));
        GameObject mark = MakeRect("VolcanoBarrierMark", _lineLayer,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        RectTransform markRt = mark.GetComponent<RectTransform>();
        markRt.anchoredPosition = local + new Vector2(0f, 18f);
        markRt.sizeDelta = new Vector2(58f, 58f);
        markRt.localRotation = Quaternion.Euler(0f, 0f, 45f);
        Img(mark, new Color(0.5f, 0.18f, 0.16f, 0.58f));

        TextMeshProUGUI label = MakeTmp("VolcanoBarrierLabel", _lineLayer,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        RectTransform labelRt = label.GetComponent<RectTransform>();
        labelRt.anchoredPosition = local + new Vector2(0f, -34f);
        labelRt.sizeDelta = new Vector2(190f, 26f);
        label.text = "VOLCANO - BARRIER";
        label.fontSize = 9f;
        label.characterSpacing = 8f;
        label.color = new Color(0.55f, 0.52f, 0.42f, 1f);
        label.alignment = TextAlignmentOptions.Center;
        label.raycastTarget = false;
    }

    Vector2 MapToLocal(Vector2 normalized)
    {
        Rect rect = GetDisplayedMapRect();
        normalized.x = Mathf.Clamp01(normalized.x);
        normalized.y = Mathf.Clamp01(normalized.y);
        return new Vector2(
            rect.xMin + normalized.x * rect.width,
            rect.yMin + normalized.y * rect.height);
    }

    Rect GetDisplayedMapRect()
    {
        Rect rect = _mapArea.rect;
        if (!_usesIllustratedBackground || _mapImage.sprite == null || rect.height <= 0f)
            return rect;

        Rect spriteRect = _mapImage.sprite.rect;
        if (spriteRect.height <= 0f)
            return rect;

        float spriteAspect = spriteRect.width / spriteRect.height;
        float rectAspect = rect.width / rect.height;
        if (rectAspect > spriteAspect)
        {
            float width = rect.height * spriteAspect;
            return new Rect(rect.center.x - width * 0.5f, rect.yMin, width, rect.height);
        }

        float height = rect.width / spriteAspect;
        return new Rect(rect.xMin, rect.center.y - height * 0.5f, rect.width, height);
    }

    static void ClearLayer(RectTransform layer)
    {
        if (layer == null)
            return;

        for (int i = layer.childCount - 1; i >= 0; i--)
            Destroy(layer.GetChild(i).gameObject);
    }

    static GameObject MakeRect(string name, RectTransform parent, Vector2 anchorMin, Vector2 anchorMax)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        return go;
    }

    static void Img(GameObject go, Color color)
    {
        Image image = go.GetComponent<Image>() ?? go.AddComponent<Image>();
        image.color = color;
    }

    static TextMeshProUGUI MakeTmp(string name, RectTransform parent, Vector2 anchorMin, Vector2 anchorMax)
    {
        GameObject go = MakeRect(name, parent, anchorMin, anchorMax);
        return go.AddComponent<TextMeshProUGUI>();
    }

    static void MakeButton(string label, RectTransform parent, Vector2 anchorMin, Vector2 anchorMax, Color color, UnityEngine.Events.UnityAction onClick)
    {
        GameObject go = MakeRect(label + "Button", parent, anchorMin, anchorMax);
        Img(go, color);

        TextMeshProUGUI text = MakeTmp(label + "Label", go.GetComponent<RectTransform>(), Vector2.zero, Vector2.one);
        text.text = label.ToUpperInvariant();
        text.fontSize = 12f;
        text.fontStyle = FontStyles.Bold;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Center;

        Button button = go.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = color;
        colors.highlightedColor = color * 1.25f;
        colors.pressedColor = color * 0.75f;
        button.colors = colors;
        button.onClick.AddListener(onClick);
    }

    static void EnsureEventSystem()
    {
        EventSystem eventSystem = EventSystem.current;
        if (eventSystem == null)
        {
            var go = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule), typeof(SingleEventSystem));
            DontDestroyOnLoad(go);
            return;
        }

        if (eventSystem.GetComponent<BaseInputModule>() == null)
            eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
    }
}
#endif // UNITY_EDITOR || !UNITY_SERVER
