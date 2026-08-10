#if UNITY_EDITOR || !UNITY_SERVER
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>Moves the Character window from its header and remembers its position per account.</summary>
public sealed class CharacterWindowDragHandle : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    const float LeftMargin = 40f;
    const float ReachableHeader = 42f;
    const float HeaderHeight = 115f;
    const string PositionKeyPrefix = "CharacterWindow.Position.";

    public RectTransform panel;
    Canvas _canvas;
    RectTransform _canvasRect;
    bool _dragging;

    public void ApplySavedPosition()
    {
        if (!ResolveCanvas()) return;
        Canvas.ForceUpdateCanvases();
        string key = AccountPositionKey();
        if (PlayerPrefs.HasKey(key + ".X") && PlayerPrefs.HasKey(key + ".Y"))
        {
            panel.anchoredPosition = new Vector2(PlayerPrefs.GetFloat(key + ".X"), PlayerPrefs.GetFloat(key + ".Y"));
        }
        else
        {
            Rect bounds = _canvasRect.rect;
            panel.anchoredPosition = new Vector2(bounds.xMin + panel.rect.width * .5f + LeftMargin, 0f);
        }
        ClampToCanvas();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!ResolveCanvas()) return;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(panel, eventData.position, eventData.pressEventCamera, out Vector2 point);
        _dragging = point.y >= panel.rect.yMax - HeaderHeight;
        if (_dragging) panel.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!_dragging || panel == null || _canvas == null) return;
        panel.anchoredPosition += eventData.delta / _canvas.scaleFactor;
        ClampToCanvas();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!_dragging || panel == null) return;
        _dragging = false;
        string key = AccountPositionKey();
        PlayerPrefs.SetFloat(key + ".X", panel.anchoredPosition.x);
        PlayerPrefs.SetFloat(key + ".Y", panel.anchoredPosition.y);
        PlayerPrefs.Save();
    }

    bool ResolveCanvas()
    {
        if (panel == null) panel = transform as RectTransform;
        if (_canvas == null) _canvas = panel != null ? panel.GetComponentInParent<Canvas>() : null;
        if (_canvasRect == null && _canvas != null) _canvasRect = _canvas.transform as RectTransform;
        return panel != null && _canvas != null && _canvasRect != null;
    }

    void ClampToCanvas()
    {
        Rect bounds = _canvasRect.rect;
        Vector2 half = panel.rect.size * .5f;
        Vector2 position = panel.anchoredPosition;
        position.x = Mathf.Clamp(position.x, bounds.xMin - half.x + ReachableHeader, bounds.xMax + half.x - ReachableHeader);
        position.y = Mathf.Clamp(position.y, bounds.yMin - half.y + ReachableHeader, bounds.yMax - half.y);
        panel.anchoredPosition = position;
    }

    static string AccountPositionKey()
    {
        string username = PlayerPrefs.GetString("username", "").Trim().ToLowerInvariant();
        return PositionKeyPrefix + (string.IsNullOrEmpty(username) ? "guest" : username);
    }
}
#endif
