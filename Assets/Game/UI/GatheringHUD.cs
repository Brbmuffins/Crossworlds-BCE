#if UNITY_EDITOR || !UNITY_SERVER
using System.Collections;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// GatheringHUD — screen-space overlay shown while AFK gathering.
///
/// Layout (Canvas sortOrder 150):
///   [profession icon]  Mining — Copper Vein          [STOP]
///                      ore_copper  Next: 3.2s  ████░░
///
/// Factory:
///   GatheringHUD hud = GatheringHUD.Show(stationName, itemId, tickInterval,
///                                         professionId, onStop);
///
/// The caller (AfkGatheringStation) passes an onStop callback. The STOP button
/// invokes that callback so the station can cancel cleanly before the HUD destroys itself.
/// </summary>
public class GatheringHUD : MonoBehaviour
{
    // ── Factory ───────────────────────────────────────────────────────────────────

    public static GatheringHUD Show(string stationName, string itemId,
                                    float tickInterval, int professionId,
                                    Action onStop)
    {
        var go  = new GameObject("GatheringHUD");
        var hud = go.AddComponent<GatheringHUD>();
        hud._stationName  = stationName;
        hud._itemId       = itemId;
        hud._tickInterval = tickInterval;
        hud._professionId = professionId;
        hud._onStop       = onStop;
        hud.Build();
        return hud;
    }

    // ── State ─────────────────────────────────────────────────────────────────────

    string _stationName;
    string _itemId;
    float  _tickInterval;
    int    _professionId;
    Action _onStop;

    Canvas             _canvas;
    Image              _fillBar;
    TextMeshProUGUI    _timerText;
    TextMeshProUGUI    _itemText;
    TextMeshProUGUI    _statusText;

    Coroutine _pulseRoutine;
    Coroutine _flashRoutine;

    static readonly string[] ProfessionIcons = { "[Wood]", "[Fish]", "[Mine]" };

    // ── Build ─────────────────────────────────────────────────────────────────────

    void Build()
    {
        _canvas = gameObject.AddComponent<Canvas>();
        _canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = 150;
        var scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        gameObject.AddComponent<GraphicRaycaster>();   // required for the STOP button to receive clicks

        // Root panel — top-center, clear of the action bar and XP display.
        var panel = MakeImage("Panel", _canvas.transform, new Color(0.05f, 0.05f, 0.08f, 0.90f));
        var rt    = panel.GetComponent<RectTransform>();
        rt.anchorMin        = new Vector2(0.5f, 1f);
        rt.anchorMax        = new Vector2(0.5f, 1f);
        rt.pivot            = new Vector2(0.5f, 1f);
        rt.anchoredPosition = new Vector2(0f, -80f);
        rt.sizeDelta        = new Vector2(340f, 76f);

        string icon = _professionId < ProfessionIcons.Length ? ProfessionIcons[_professionId] : "[?]";

        // Station name row
        var title = MakeTMP("Title", panel.transform,
            $"{icon}  {_stationName}", 13, FontStyles.Bold, Color.white);
        SetRect(title, 10f, 48f, 280f, 22f);

        // Item name
        _itemText = MakeTMP("Item", panel.transform,
            _itemId, 11, FontStyles.Normal, new Color(0.8f, 0.9f, 0.7f));
        SetRect(_itemText, 10f, 28f, 200f, 18f);

        // Countdown timer
        _timerText = MakeTMP("Timer", panel.transform,
            $"Next: {_tickInterval:F1}s", 11, FontStyles.Normal, new Color(0.6f, 0.8f, 1f));
        SetRect(_timerText, 225f, 28f, 100f, 18f);

        // Progress bar track
        var track = MakeImage("Track", panel.transform, new Color(0.15f, 0.15f, 0.20f));
        SetRect(track, 10f, 10f, 320f, 14f);

        // Progress bar fill
        var fillGO    = MakeImage("Fill", track.transform, new Color(0.3f, 0.8f, 0.4f));
        SetRect(fillGO, 0f, 0f, 320f, 14f);
        _fillBar      = fillGO.GetComponent<Image>();
        _fillBar.type = Image.Type.Filled;
        _fillBar.fillMethod = Image.FillMethod.Horizontal;
        _fillBar.fillOrigin = 0;
        _fillBar.fillAmount = 0f;

        // Level-up / status text (hidden until needed)
        _statusText = MakeTMP("Status", panel.transform,
            "", 11, FontStyles.Italic, new Color(1f, 0.9f, 0.3f));
        SetRect(_statusText, 10f, 48f, 280f, 22f);
        _statusText.enabled = false;

        // STOP button
        var stopBtn = MakeImage("StopBtn", panel.transform, new Color(0.55f, 0.12f, 0.12f));
        SetRect(stopBtn, 294f, 50f, 38f, 20f);
        var stopLabel = MakeTMP("StopText", stopBtn.transform,
            "STOP", 10, FontStyles.Bold, Color.white);
        SetRect(stopLabel, 0f, 0f, 38f, 20f);

        var btn = stopBtn.gameObject.AddComponent<Button>();
        btn.onClick.AddListener(OnStopClicked);
    }

    void OnStopClicked()
    {
        // Tell the station to cancel first (sets _gathering = false, stops loop)
        _onStop?.Invoke();
        // Station calls Hide() as part of StopGathering; if for any reason it doesn't, self-clean
        if (this != null && gameObject != null)
            Hide();
    }

    // ── Public API ────────────────────────────────────────────────────────────────

    public void SetProgress(float progress, float secondsLeft)
    {
        if (_fillBar   != null) _fillBar.fillAmount = Mathf.Clamp01(progress);
        if (_timerText != null) _timerText.text     = $"Next: {secondsLeft:F1}s";
    }

    public void Pulse(int qty)
    {
        if (_pulseRoutine != null) StopCoroutine(_pulseRoutine);
        _pulseRoutine = StartCoroutine(DoPulse(qty));
    }

    public void Pulse(int qty, string itemId)
    {
        if (!string.IsNullOrWhiteSpace(itemId)) _itemId = itemId;
        Pulse(qty);
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
        if (_fillBar  != null) _fillBar.color  = new Color(0.9f, 1f, 0.3f);
        if (_itemText != null) _itemText.text   = $"+ {qty}x {_itemId}";
        yield return new WaitForSeconds(0.6f);
        if (_fillBar  != null) _fillBar.color  = new Color(0.3f, 0.8f, 0.4f);
        if (_itemText != null) _itemText.text   = _itemId;
    }

    IEnumerator DoFlashLevelUp(int newLevel)
    {
        if (_statusText == null) yield break;
        string profName = _professionId < ProfessionManager.ProfessionNames.Length
            ? ProfessionManager.ProfessionNames[_professionId]
            : "Profession";

        _statusText.text    = $"{profName} level {newLevel}!";
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
        var go  = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color = color;
        return go;
    }

    static TextMeshProUGUI MakeTMP(string name, Transform parent,
        string text, int size, FontStyles style, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var t       = go.AddComponent<TextMeshProUGUI>();
        t.text      = text;
        t.fontSize  = size;
        t.fontStyle = style;
        t.color     = color;
        t.alignment = TextAlignmentOptions.MidlineLeft;
        return t;
    }

    static void SetRect(GameObject go, float x, float y, float w, float h)
    {
        SetRect(go.GetComponent<RectTransform>(), x, y, w, h);
    }

    static void SetRect(Component c, float x, float y, float w, float h)
    {
        var rt              = c.GetComponent<RectTransform>();
        rt.anchorMin        = Vector2.zero;
        rt.anchorMax        = Vector2.zero;
        rt.pivot            = Vector2.zero;
        rt.anchoredPosition = new Vector2(x, y);
        rt.sizeDelta        = new Vector2(w, h);
    }
}
#endif
