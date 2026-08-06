#if UNITY_EDITOR || !UNITY_SERVER
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>Drag handle for the inventory header. Keeps the window reachable on-screen.</summary>
public sealed class InventoryWindowDragHandle : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    const float RightMargin = 40f;
    const float ReachableHeader = 42f;
    const string PositionKeyPrefix = "InventoryWindow.Position.";

    public RectTransform panel;
    Canvas _canvas;
    RectTransform _canvasRect;

    public void ApplySavedPosition()
    {
        if (!ResolveCanvas()) return;

        Canvas.ForceUpdateCanvases();
        string key = AccountPositionKey();
        string xKey = key + ".X";
        string yKey = key + ".Y";
        if (PlayerPrefs.HasKey(xKey) && PlayerPrefs.HasKey(yKey))
        {
            panel.anchoredPosition = new Vector2(
                PlayerPrefs.GetFloat(xKey),
                PlayerPrefs.GetFloat(yKey));
        }
        else
        {
            Rect bounds = _canvasRect.rect;
            panel.anchoredPosition = new Vector2(
                bounds.xMax - panel.rect.width * 0.5f - RightMargin,
                0f);
        }

        ClampToCanvas();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!ResolveCanvas()) return;
        panel.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (panel == null || _canvas == null || _canvasRect == null) return;
        panel.anchoredPosition += eventData.delta / _canvas.scaleFactor;

        ClampToCanvas();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (panel == null) return;
        string key = AccountPositionKey();
        PlayerPrefs.SetFloat(key + ".X", panel.anchoredPosition.x);
        PlayerPrefs.SetFloat(key + ".Y", panel.anchoredPosition.y);
        PlayerPrefs.Save();
    }

    bool ResolveCanvas()
    {
        if (panel == null) return false;
        if (_canvas == null) _canvas = panel.GetComponentInParent<Canvas>();
        if (_canvasRect == null && _canvas != null)
            _canvasRect = _canvas.transform as RectTransform;
        return _canvas != null && _canvasRect != null;
    }

    void ClampToCanvas()
    {
        if (panel == null || _canvasRect == null) return;
        Rect bounds = _canvasRect.rect;
        Vector2 half = panel.rect.size * 0.5f;
        Vector2 position = panel.anchoredPosition;
        position.x = Mathf.Clamp(position.x, bounds.xMin - half.x + ReachableHeader, bounds.xMax + half.x - ReachableHeader);
        position.y = Mathf.Clamp(position.y, bounds.yMin - half.y + ReachableHeader, bounds.yMax - half.y);
        panel.anchoredPosition = position;
    }

    static string AccountPositionKey()
    {
        string username = PlayerPrefs.GetString("username", "").Trim().ToLowerInvariant();
        if (string.IsNullOrEmpty(username)) username = "guest";
        return PositionKeyPrefix + username;
    }
}
#endif
