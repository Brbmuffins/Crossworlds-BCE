#if UNITY_EDITOR || !UNITY_SERVER
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>Drag handle for the inventory header. Keeps the window reachable on-screen.</summary>
public sealed class InventoryWindowDragHandle : MonoBehaviour, IBeginDragHandler, IDragHandler
{
    public RectTransform panel;
    Canvas _canvas;
    RectTransform _canvasRect;

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (panel == null) return;
        _canvas = panel.GetComponentInParent<Canvas>();
        _canvasRect = _canvas != null ? _canvas.transform as RectTransform : null;
        panel.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (panel == null || _canvas == null || _canvasRect == null) return;
        panel.anchoredPosition += eventData.delta / _canvas.scaleFactor;

        Rect bounds = _canvasRect.rect;
        Vector2 half = panel.rect.size * 0.5f;
        const float reachableHeader = 42f;
        Vector2 position = panel.anchoredPosition;
        position.x = Mathf.Clamp(position.x, bounds.xMin - half.x + reachableHeader, bounds.xMax + half.x - reachableHeader);
        position.y = Mathf.Clamp(position.y, bounds.yMin - half.y + reachableHeader, bounds.yMax - half.y);
        panel.anchoredPosition = position;
    }
}
#endif
