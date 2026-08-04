#if UNITY_EDITOR || !UNITY_SERVER
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// CharacterSheetUI — C key toggles a side panel showing:
///   Level, XP progress, Gold
///   Str / Agi / Int / Vit
///   Class name
///
/// Reads from PlayerProgressManager.Local and PlayerIdentity (local).
/// Self-bootstrapping.
/// </summary>
public class CharacterSheetUI : MonoBehaviour
{
    // ── Bootstrap ─────────────────────────────────────────────────────────────
    private static CharacterSheetUI _instance;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        if (_instance != null) return;
        var go = new GameObject("[CharacterSheetUI]");
        DontDestroyOnLoad(go);
        _instance = go.AddComponent<CharacterSheetUI>();
    }

    // ── UI refs ───────────────────────────────────────────────────────────────
    Canvas          _canvas;
    GameObject      _panel;
    TextMeshProUGUI _nameText;
    TextMeshProUGUI _classText;
    TextMeshProUGUI _levelText;
    TextMeshProUGUI _xpText;
    TextMeshProUGUI _goldText;
    Image           _xpFill;
    TextMeshProUGUI _strText, _agiText, _intText, _vitText;
    bool            _open;

    // ── Lifecycle ─────────────────────────────────────────────────────────────
    void Awake()  { BuildUI(); _panel.SetActive(false); }

    void OnEnable()  { StartCoroutine(WaitForManager()); }
    void OnDisable()
    {
        if (PlayerProgressManager.Local != null)
            PlayerProgressManager.Local.OnDataRefreshed -= Repaint;
    }

    IEnumerator WaitForManager()
    {
        while (PlayerProgressManager.Local == null) yield return null;
        PlayerProgressManager.Local.OnDataRefreshed -= Repaint;
        PlayerProgressManager.Local.OnDataRefreshed += Repaint;
        Repaint();
    }

    void Update()
    {
        var kb = UnityEngine.InputSystem.Keyboard.current;
        if (kb == null) return;
        if (kb.cKey.wasPressedThisFrame && !AnyInputFocused())
        {
            _open = !_open;
            _panel.SetActive(_open);
            if (_open) Repaint();
        }
    }

    // ── Repaint ───────────────────────────────────────────────────────────────
    void Repaint()
    {
        if (!_open || _panel == null) return;
        var pm = PlayerProgressManager.Local;
        if (pm == null) return;

        // Identity
        var id = FindLocalIdentity();
        _nameText.text  = id != null ? id.playerName : PlayerPrefs.GetString("username", "Player");
        _classText.text = id != null ? id.ClassName  : "—";

        // Progress
        _levelText.text = $"Level  {pm.Level}";
        _xpText.text    = $"{pm.Xp} / {pm.XpToNext} XP";
        _xpFill.fillAmount = pm.XpFraction;

        // Gold
        _goldText.text = $"⬡ {pm.Gold:N0}";

        // Stats
        _strText.text = $"STR   {pm.StatStr}";
        _agiText.text = $"AGI   {pm.StatAgi}";
        _intText.text = $"INT   {pm.StatInt}";
        _vitText.text = $"VIT   {pm.StatVit}";
    }

    // ── Build UI ──────────────────────────────────────────────────────────────
    void BuildUI()
    {
        // Canvas
        var cgo = new GameObject("CharSheetCanvas",
            typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        _canvas = cgo.GetComponent<Canvas>();
        _canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = 110;
        var scaler = cgo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight  = 0.5f;
        var root = _canvas.GetComponent<RectTransform>();

        // Panel — left side, vertically centred
        _panel = new GameObject("CharPanel", typeof(RectTransform), typeof(Image));
        _panel.transform.SetParent(root, false);
        _panel.GetComponent<Image>().color = new Color(0.04f, 0.03f, 0.10f, 0.96f);
        var pRt = _panel.GetComponent<RectTransform>();
        pRt.anchorMin        = new Vector2(0f, 0.3f);
        pRt.anchorMax        = new Vector2(0f, 0.75f);
        pRt.pivot            = new Vector2(0f, 0.5f);
        pRt.anchoredPosition = new Vector2(12f, 0f);
        pRt.sizeDelta        = new Vector2(200f, 0f);

        // Title bar
        var titleBar = MakeStretchChild("TitleBar", pRt,
            new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -36f), Vector2.zero);
        titleBar.AddComponent<Image>().color = new Color(0.08f, 0.05f, 0.20f, 1f);
        var titleT = MakeText("Title", titleBar.GetComponent<RectTransform>(),
            Vector2.zero, Vector2.one, new Vector2(8f, 0f), Vector2.zero);
        titleT.text      = "CHARACTER  <size=8><color=#475569>C to close</color></size>";
        titleT.fontSize  = 11f;
        titleT.fontStyle = FontStyles.Bold;
        titleT.color     = new Color(0.7f, 0.6f, 1f);
        titleT.alignment = TextAlignmentOptions.Left;

        // Body — stacked rows
        float y = -44f;
        float rowH = 20f;
        float gap  = 4f;

        _nameText  = AddRow("Name",  pRt, ref y, rowH, gap, new Color(1f, 1f, 1f));
        _classText = AddRow("Class", pRt, ref y, rowH, gap, new Color(0.7f, 0.6f, 1f));

        // Divider
        AddDivider(pRt, ref y, gap);

        _levelText = AddRow("Level", pRt, ref y, rowH, gap, new Color(0.9f, 0.9f, 1f));

        // XP bar
        y -= gap;
        var xpBarGO = MakeStretchChild("XpBg", pRt,
            new Vector2(0f, 1f), new Vector2(1f, 1f),
            new Vector2(8f, y - 10f), new Vector2(-8f, y));
        xpBarGO.AddComponent<Image>().color = new Color(0.08f, 0.06f, 0.18f);
        var xpFillGO = MakeStretchChild("XpFill", xpBarGO.GetComponent<RectTransform>(),
            Vector2.zero, new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
        _xpFill = xpFillGO.AddComponent<Image>();
        _xpFill.color      = new Color(0.28f, 0.18f, 0.72f);
        _xpFill.type       = Image.Type.Filled;
        _xpFill.fillMethod = Image.FillMethod.Horizontal;
        _xpFill.fillAmount = 0f;
        y -= 14f;

        _xpText = AddRow("XP", pRt, ref y, rowH, gap, new Color(0.6f, 0.6f, 0.9f));
        _xpText.fontSize = 9f;

        AddDivider(pRt, ref y, gap);

        _goldText = AddRow("Gold", pRt, ref y, rowH, gap, new Color(1f, 0.85f, 0.2f));

        AddDivider(pRt, ref y, gap);

        // Stats
        var statsLabel = AddRow("Stats", pRt, ref y, rowH, gap, new Color(0.5f, 0.5f, 0.7f));
        statsLabel.text = "— STATS —";
        statsLabel.alignment = TextAlignmentOptions.Center;

        _strText = AddRow("Str", pRt, ref y, rowH, gap, new Color(1f, 0.5f, 0.4f));
        _agiText = AddRow("Agi", pRt, ref y, rowH, gap, new Color(0.4f, 1f, 0.5f));
        _intText = AddRow("Int", pRt, ref y, rowH, gap, new Color(0.4f, 0.7f, 1f));
        _vitText = AddRow("Vit", pRt, ref y, rowH, gap, new Color(1f, 0.7f, 0.4f));

        // Resize panel to fit content
        pRt.sizeDelta = new Vector2(200f, -(y - gap));
    }

    // ── UI helpers ────────────────────────────────────────────────────────────
    TextMeshProUGUI AddRow(string name, RectTransform parent, ref float y,
        float h, float gap, Color col)
    {
        y -= gap;
        var go = MakeStretchChild(name, parent,
            new Vector2(0f, 1f), new Vector2(1f, 1f),
            new Vector2(10f, y - h), new Vector2(-10f, y));
        y -= h;
        var t = go.AddComponent<TextMeshProUGUI>();
        t.fontSize  = 11f;
        t.color     = col;
        t.alignment = TextAlignmentOptions.Left;
        return t;
    }

    void AddDivider(RectTransform parent, ref float y, float gap)
    {
        y -= gap + 2f;
        var div = MakeStretchChild("Divider", parent,
            new Vector2(0f, 1f), new Vector2(1f, 1f),
            new Vector2(8f, y - 1f), new Vector2(-8f, y));
        div.AddComponent<Image>().color = new Color(0.3f, 0.2f, 0.5f, 0.5f);
        y -= 3f;
    }

    static GameObject MakeStretchChild(string name, RectTransform parent,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
        rt.offsetMin = offsetMin; rt.offsetMax = offsetMax;
        return go;
    }

    static TextMeshProUGUI MakeText(string name, RectTransform parent,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
    {
        var go = MakeStretchChild(name, parent, anchorMin, anchorMax, offsetMin, offsetMax);
        return go.AddComponent<TextMeshProUGUI>();
    }

    static bool AnyInputFocused()
    {
        if (RodChatManager.Instance != null && RodChatManager.Instance.IsOpen)
            return true;

        foreach (var f in FindObjectsByType<TMP_InputField>(FindObjectsInactive.Exclude))
            if (f.isFocused) return true;
        return false;
    }

    static PlayerIdentity FindLocalIdentity()
    {
        foreach (var id in FindObjectsByType<PlayerIdentity>(FindObjectsInactive.Exclude))
            if (id.isLocalPlayer) return id;
        return null;
    }
}
#endif
