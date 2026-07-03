#if !UNITY_SERVER
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// GatheringHUD — minimal screen-space HUD shown while AFK gathering.
///
/// Layout (Canvas sortOrder 150 — above world, below tooltip):
///   [icon]  Mining — Copper Vein          [STOP]
///           ore_copper  Next: 3.2s  ████░░
///
/// AfkGatheringStation manages this object's lifetime:
///   GatheringHUD hud = GatheringHUD.Show(...)
///   hud.SetProgress(0–1, secondsLeft)
///   hud.Pulse(qty)
///   hud.FlashLevelUp(newLevel)
///   hud.Hide()
/// </summary>
public class GatheringHUD : MonoBehaviour
{
    // ── Factory ───────────────────────────────────────────────────────────────────

    public static GatheringHUD Show(string stationName, string itemId, float tickInterval)
    {
        var go  = new GameObject("GatheringHUD");
        var hud = go.AddComponent<GatheringHUD>();
        hud._stationName  = stationName;
        hud._itemId       = itemId;
        hud._tickInterval = tickInterval;
        hud.Build();
        return hud;
    }

    // ── State ─────────────────────────────────────────────────────────────────────

    string _stationName;
    string _itemId;
    float  _tickInterval;

    Canvas    _canvas;
    Image     _fillBar;
    Text      _timerText;
    Text      _itemText;
    Text      _statusText;

    Coroutine _pulseRoutine;
    Coroutine _flashRoutine;

    // ─────────────────────────────────────────────────────────────────────────────

    void Build()
    {
        // Canvas
        _canvas = gameObject.AddComponent<Canvas>();
        _canvas.renderMode  = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = 150;
        gameObject.AddComponent<CanvasScaler>().uiScaleMode =
            CanvasScaler.ScaleMode.ScaleWithScreenSize;

        // Root panel — bottom-center
        var panel = MakeImage("Panel", _canvas.transform,
            new Color(0.05f, 0.05f, 0.08f, 0.88f));
        var rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0f);
        rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot     = new Vector2(0.5f, 0f);
        rt.anchoredPosition = new Vector2(0f, 80f);
        rt.sizeDelta        = new Vector2(320f, 72f);

        // Station name
        var title = MakeText("Title", panel.transform,
            $"⛏  {_stationName}", 14, FontStyle.Bold, Color.white);
        SetRect(title, 10f, 44f, 260f, 22f);

        // Item name
        _itemText = MakeText("Item", panel.transform,
            _itemId, 11, FontStyle.Normal, new Color(0.8f, 0.9f, 0.7f));
        SetRect(_itemText, 10f, 26f, 200f, 18f);

        // Timer
        _timerText = MakeText("Timer", panel.transform,
            "Next: 5.0s", 11, FontStyle.Normal, new Color(0.6f, 0.8f, 1f));
        SetRect(_timerText, 220f, 26f, 90f, 18f);

        // Progress bar track
        var track = MakeImage("Track", panel.transform, new Color(0.15f, 0.15f, 0.2f, 1f));
        SetRect(track, 10f, 8f, 300f, 14f);

        // Progress bar fill
        var fillGO = MakeImage("Fill", track.transform, new Color(0.3f, 0.8f, 0.4f, 1f));
        SetRect(fillGO, 0f, 0f, 300f, 14f);
        _fillBar = fillGO.GetComponent<Image>();
        _fillBar.type = Image.Type.Filled;
        _fillBar.fillMethod    = Image.FillMethod.Horizontal;
        _fillBar.fillOrigin    = 0;
        _fillBar.fillAmount    = 0f;

        // Status / level-up flash text
        _statusText = MakeText("Status", panel.transform,
            "", 11, FontStyle.Italic, new Color(1f, 0.9f, 0.3f));
        SetRect(_statusText, 10f, 44f, 300f, 22f);
        _statusText.enabled = false;

        // Stop button
        var stopBtn = MakeImage("StopBtn", panel.transform, new Color(0.55f, 0.12f, 0.12f));
        SetRect(stopBtn, 272f, 48f, 40f, 18f);
        var stopText = MakeText("StopText", stopBtn.transform,
            "STOP", 10, FontStyle.Bold, Color.white);
        SetRect(stopText, 0f, 0f, 40f, 18f);

        // Stop button click: simulate Escape keypress can't be done cleanly here,
        // so instead destroy the HUD — AfkGatheringStation detects _hud == null via null check.
        // The station will call Hide() explicitly on cancel. Button just self-destructs.
        var btn = stopBtn.gameObject.AddComponent<Button>();
        btn.onClick.AddListener(() =>
        {
            RodChatManager.Instance?.AddSystemMessage("You stopped gathering.");
            Hide();
        });
    }

    // ── Public API ────────────────────────────────────────────────────────────────

    /// progress 0–1 within current tick. secondsLeft = time until next reward.
    public void SetProgress(float progress, float secondsLeft)
    {
        if (_fillBar   != null) _fillBar.fillAmount = Mathf.Clamp01(progress);
        if (_timerText != null) _timerText.text     = $"Next: {secondsLeft:F1}s";
    }

    /// Flash the fill bar green and show qty reward briefly.
    public void Pulse(int qty)
    {
        if (_pulseRoutine != null) StopCoroutine(_pulseRoutine);
        _pulseRoutine = StartCoroutine(DoPulse(qty));
    }

    public void FlashLevelUp(int newLevel)
    {
        if (_flashRoutine != null) StopCoroutine(_flashRoutine);
        _flashRoutine = StartCoroutine(DoFlashLevelUp(newLevel));
    }

    public void Hide()
    {
        if (_pulseRoutine != null) StopCoroutine(_pulseRoutine);
        if (_flashRoutine != null) StopCoroutine(_flashRoutine);
        Destroy(gameObject);
    }

    // ── Coroutines ────────────────────────────────────────────────────────────────

    IEnumerator DoPulse(int qty)
    {
        if (_fillBar != null) _fillBar.color = new Color(0.9f, 1f, 0.3f);
        if (_itemText != null) _itemText.text = $"+ {qty}× {_itemId}";
        yield return new WaitForSeconds(0.6f);
        if (_fillBar  != null) _fillBar.color  = new Color(0.3f, 0.8f, 0.4f);
        if (_itemText != null) _itemText.text   = _itemId;
    }

    IEnumerator DoFlashLevelUp(int newLevel)
    {
        if (_statusText == null) yield break;

        int profId = FindObjectOfType<AfkGatheringStation>()?.professionId ?? 2;
        string name = profId < ProfessionManager.ProfessionNames.Length
            ? ProfessionManager.ProfessionNames[profId]
            : "Profession";

        _statusText.text    = $"🎉  {name} level {newLevel}!";
        _statusText.enabled = true;

        for (int i = 0; i < 5; i++)
        {
            _statusText.color = new Color(1f, 0.9f, 0.3f);
            yield return new WaitForSeconds(0.2f);
            _statusText.color = Color.white;
            yield return new WaitForSeconds(0.2f);
        }
        _statusText.enabled = false;
    }

    // ── UI builder helpers ────────────────────────────────────────────────────────

    static GameObject MakeImage(string name, Transform parent, Color color)
    {
        var go  = new GameObject(name);
        go.transform.SetParent(parent, false);
        var img   = go.AddComponent<Image>();
        img.color = color;
        return go;
    }

    static Text MakeText(string name, Transform parent, string text,
        int size, FontStyle style, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var t = go.AddComponent<Text>();
        t.text      = text;
        t.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.fontSize  = size;
        t.fontStyle = style;
        t.color     = color;
        t.alignment = TextAnchor.MiddleLeft;
        return t;
    }

    static void SetRect(Component c, float x, float y, float w, float h)
    {
        var rt       = c.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.zero;
        rt.pivot     = Vector2.zero;
        rt.anchoredPosition = new Vector2(x, y);
        rt.sizeDelta        = new Vector2(w, h);
    }
}
#endif
