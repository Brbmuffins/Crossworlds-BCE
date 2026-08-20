#if UNITY_EDITOR || !UNITY_SERVER
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>Movable Forge header with per-account position persistence.</summary>
public sealed class ForgeWindowDragHandle : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    const string PositionKeyPrefix = "ForgeWindow.Position.";
    const float ReachableHeader = 42f;

    public RectTransform panel;
    Canvas _canvas;
    RectTransform _canvasRect;

    public void ApplySavedPosition()
    {
        if (!ResolveCanvas()) return;
        Canvas.ForceUpdateCanvases();
        string key = AccountPositionKey();
        if (PlayerPrefs.HasKey(key + ".X") && PlayerPrefs.HasKey(key + ".Y"))
            panel.anchoredPosition = new Vector2(PlayerPrefs.GetFloat(key + ".X"), PlayerPrefs.GetFloat(key + ".Y"));
        else
            panel.anchoredPosition = new Vector2(-430f, 0f);
        ClampToCanvas();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (ResolveCanvas()) panel.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!ResolveCanvas()) return;
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
        if (_canvasRect == null && _canvas != null) _canvasRect = _canvas.transform as RectTransform;
        return _canvas != null && _canvasRect != null;
    }

    void ClampToCanvas()
    {
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
        return PositionKeyPrefix + (string.IsNullOrEmpty(username) ? "guest" : username);
    }
}
#endif
