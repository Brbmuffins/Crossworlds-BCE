#if UNITY_EDITOR || !UNITY_SERVER
using UnityEngine;
using UnityEngine.EventSystems;

public sealed class ChatWindowHandle : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    const string PositionKey = "ChatWindow.Position";
    const string SizeKey = "ChatWindow.Size";
    public RectTransform panel;
    public bool resize;
    public Vector2 minimumSize = new(360f, 190f);
    public Vector2 maximumSize = new(1100f, 700f);
    Canvas _canvas;
    Vector2 _startPointer, _startPosition, _startSize;

    void Awake() { _canvas = GetComponentInParent<Canvas>(); Restore(); }
    public void OnBeginDrag(PointerEventData e)
    {
        if (panel == null) return;
        _startPointer = e.position;
        _startPosition = panel.anchoredPosition;
        _startSize = panel.rect.size;
    }
    public void OnDrag(PointerEventData e)
    {
        if (panel == null) return;
        float scale = _canvas != null ? Mathf.Max(0.01f, _canvas.scaleFactor) : 1f;
        Vector2 delta = (e.position - _startPointer) / scale;
        if (resize)
        {
            panel.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,
                Mathf.Clamp(_startSize.x + delta.x, minimumSize.x, maximumSize.x));
            panel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,
                Mathf.Clamp(_startSize.y - delta.y, minimumSize.y, maximumSize.y));
        }
        else panel.anchoredPosition = _startPosition + delta;
        KeepOnScreen();
    }
    public void OnEndDrag(PointerEventData e) => Save();

    void KeepOnScreen()
    {
        if (panel?.parent is not RectTransform parent) return;
        Vector3[] p = new Vector3[4], r = new Vector3[4];
        panel.GetWorldCorners(p); parent.GetWorldCorners(r);
        Vector3 correction = Vector3.zero;
        if (p[0].x < r[0].x) correction.x += r[0].x - p[0].x;
        if (p[2].x > r[2].x) correction.x -= p[2].x - r[2].x;
        if (p[0].y < r[0].y) correction.y += r[0].y - p[0].y;
        if (p[2].y > r[2].y) correction.y -= p[2].y - r[2].y;
        panel.position += correction;
    }
    void Restore()
    {
        if (panel == null) return;
        if (TryRead(SizeKey, out Vector2 size))
        {
            panel.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,
                Mathf.Clamp(size.x, minimumSize.x, maximumSize.x));
            panel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,
                Mathf.Clamp(size.y, minimumSize.y, maximumSize.y));
        }
        if (TryRead(PositionKey, out Vector2 position)) panel.anchoredPosition = position;
        KeepOnScreen();
    }
    static bool TryRead(string key, out Vector2 value)
    {
        value = default;
        if (!PlayerPrefs.HasKey(key)) return false;
        string[] parts = PlayerPrefs.GetString(key).Split('|');
        return parts.Length == 2 && float.TryParse(parts[0], out value.x) &&
               float.TryParse(parts[1], out value.y);
    }
    void Save()
    {
        if (panel == null) return;
        PlayerPrefs.SetString(PositionKey, $"{panel.anchoredPosition.x}|{panel.anchoredPosition.y}");
        PlayerPrefs.SetString(SizeKey, $"{panel.rect.width}|{panel.rect.height}");
        PlayerPrefs.Save();
    }
}
#endif
