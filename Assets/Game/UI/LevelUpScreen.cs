#if !UNITY_SERVER
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// LevelUpScreen — full-screen flash + "LEVEL UP!" burst when the player levels up.
/// Self-bootstrapping via RuntimeInitializeOnLoadMethod.
/// Subscribes to PlayerProgressManager.Local.OnLevelUp.
/// </summary>
public class LevelUpScreen : MonoBehaviour
{
    // ── Class colours (matches CharacterSheetUI + XpBar) ─────────────────────
    static readonly Color[] ClassColours =
    {
        new Color(0.28f, 0.70f, 1.00f), // Engineer  — blue
        new Color(0.95f, 0.80f, 0.20f), // Guardian  — gold
        new Color(0.55f, 0.10f, 0.80f), // Shadowblade — purple
        new Color(0.20f, 0.90f, 0.50f), // Cleric    — green
        new Color(1.00f, 0.35f, 0.10f), // Arcanist  — orange
    };

    // ── UI references ─────────────────────────────────────────────────────────
    Canvas     _canvas;
    Image      _flash;
    Text       _levelText;
    Text       _subtitleText;
    Text       _newLevelNumber;

    // ── State ─────────────────────────────────────────────────────────────────
    bool       _subscribed;
    int        _classIndex;

    // ── Bootstrap ─────────────────────────────────────────────────────────────
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        var go = new GameObject("LevelUpScreen", typeof(RectTransform));
        DontDestroyOnLoad(go);
        go.AddComponent<LevelUpScreen>();
    }

    // ── Lifecycle ─────────────────────────────────────────────────────────────
    void Awake()
    {
        BuildUI();
        Hide();
    }

    void Update()
    {
        // Subscribe once PlayerProgressManager singleton is live
        if (!_subscribed && PlayerProgressManager.Local != null)
        {
            PlayerProgressManager.Local.OnLevelUp += HandleLevelUp;
            _subscribed = true;
        }
    }

    void OnDestroy()
    {
        if (PlayerProgressManager.Local != null)
            PlayerProgressManager.Local.OnLevelUp -= HandleLevelUp;
    }

    // ── UI construction ───────────────────────────────────────────────────────
    void BuildUI()
    {
        // Root canvas — overlay, always on top
        _canvas = gameObject.AddComponent<Canvas>();
        _canvas.renderMode  = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = 200;
        gameObject.AddComponent<CanvasScaler>().uiScaleMode =
            CanvasScaler.ScaleMode.ScaleWithScreenSize;
        gameObject.AddComponent<GraphicRaycaster>();

        // Full-screen flash overlay
        var flashGO = new GameObject("Flash", typeof(RectTransform));
        flashGO.transform.SetParent(transform, false);
        var flashRT = flashGO.GetComponent<RectTransform>();
        flashRT.anchorMin = Vector2.zero;
        flashRT.anchorMax = Vector2.one;
        flashRT.offsetMin = Vector2.zero;
        flashRT.offsetMax = Vector2.zero;
        _flash = flashGO.AddComponent<Image>();
        _flash.color = new Color(1f, 1f, 1f, 0f);
        _flash.raycastTarget = false;

        // Central text container
        var centerGO = new GameObject("Center", typeof(RectTransform));
        centerGO.transform.SetParent(transform, false);
        var centerRT = centerGO.GetComponent<RectTransform>();
        centerRT.anchorMin = new Vector2(0.2f, 0.35f);
        centerRT.anchorMax = new Vector2(0.8f, 0.65f);
        centerRT.offsetMin = Vector2.zero;
        centerRT.offsetMax = Vector2.zero;

        // "LEVEL UP!" banner text
        var lvlGO = new GameObject("LevelUpText", typeof(RectTransform));
        lvlGO.transform.SetParent(centerGO.transform, false);
        var lvlRT = lvlGO.GetComponent<RectTransform>();
        lvlRT.anchorMin = new Vector2(0f, 0.55f);
        lvlRT.anchorMax = Vector2.one;
        lvlRT.offsetMin = Vector2.zero;
        lvlRT.offsetMax = Vector2.zero;
        _levelText = lvlGO.AddComponent<Text>();
        _levelText.text      = "LEVEL UP!";
        _levelText.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        _levelText.fontSize  = 72;
        _levelText.fontStyle = FontStyle.Bold;
        _levelText.alignment = TextAnchor.MiddleCenter;
        _levelText.color     = Color.white;
        _levelText.raycastTarget = false;

        // "Level X" subtitle
        var numGO = new GameObject("LevelNumber", typeof(RectTransform));
        numGO.transform.SetParent(centerGO.transform, false);
        var numRT = numGO.GetComponent<RectTransform>();
        numRT.anchorMin = new Vector2(0f, 0.25f);
        numRT.anchorMax = new Vector2(1f, 0.55f);
        numRT.offsetMin = Vector2.zero;
        numRT.offsetMax = Vector2.zero;
        _newLevelNumber = numGO.AddComponent<Text>();
        _newLevelNumber.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        _newLevelNumber.fontSize  = 48;
        _newLevelNumber.fontStyle = FontStyle.Bold;
        _newLevelNumber.alignment = TextAnchor.MiddleCenter;
        _newLevelNumber.color     = new Color(1f, 0.85f, 0.2f);
        _newLevelNumber.raycastTarget = false;

        // Subtitle flavour line
        var subGO = new GameObject("Subtitle", typeof(RectTransform));
        subGO.transform.SetParent(centerGO.transform, false);
        var subRT = subGO.GetComponent<RectTransform>();
        subRT.anchorMin = new Vector2(0f, 0f);
        subRT.anchorMax = new Vector2(1f, 0.25f);
        subRT.offsetMin = Vector2.zero;
        subRT.offsetMax = Vector2.zero;
        _subtitleText = subGO.AddComponent<Text>();
        _subtitleText.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        _subtitleText.fontSize  = 24;
        _subtitleText.alignment = TextAnchor.MiddleCenter;
        _subtitleText.color     = new Color(0.9f, 0.9f, 0.9f, 0.85f);
        _subtitleText.raycastTarget = false;
    }

    // ── Show / Hide ───────────────────────────────────────────────────────────
    void Hide()
    {
        _flash.color           = new Color(1f, 1f, 1f, 0f);
        _levelText.color       = new Color(1f, 1f, 1f, 0f);
        _newLevelNumber.color  = new Color(1f, 0.85f, 0.2f, 0f);
        _subtitleText.color    = new Color(0.9f, 0.9f, 0.9f, 0f);
    }

    // ── Event handler ─────────────────────────────────────────────────────────
    void HandleLevelUp(int newLevel)
    {
        // Resolve class colour
        _classIndex = PlayerProgressManager.Local?.ClassIndex ?? 0;
        Color accent = ClassColours[Mathf.Clamp(_classIndex, 0, ClassColours.Length - 1)];

        _newLevelNumber.text  = $"Level {newLevel}";
        _subtitleText.text    = LevelFlavour(newLevel);
        _levelText.color      = accent;
        _newLevelNumber.color = new Color(1f, 0.85f, 0.2f, 1f);

        StopAllCoroutines();
        StartCoroutine(PlayAnimation(accent));
    }

    IEnumerator PlayAnimation(Color accent)
    {
        float duration    = 2.8f;
        float holdTime    = 0.9f;
        float fadeInTime  = 0.25f;
        float fadeOutTime = 0.6f;
        float punchTime   = 0.35f;

        // ── Flash background ──────────────────────────────────────────────────
        float elapsed = 0f;
        while (elapsed < fadeInTime)
        {
            elapsed += Time.unscaledDeltaTime;
            float t  = elapsed / fadeInTime;
            _flash.color = new Color(accent.r, accent.g, accent.b, Mathf.Lerp(0f, 0.55f, t));
            yield return null;
        }

        // ── Punch scale the LEVEL UP text ─────────────────────────────────────
        Vector3 baseScale = Vector3.one;
        Vector3 punchScale = Vector3.one * 1.25f;
        elapsed = 0f;
        while (elapsed < punchTime)
        {
            elapsed += Time.unscaledDeltaTime;
            float t  = elapsed / punchTime;
            float s  = Mathf.LerpUnclamped(1f, 1.25f, Mathf.Sin(t * Mathf.PI));
            _levelText.transform.localScale = new Vector3(s, s, 1f);
            // Fade in text
            Color tc = _levelText.color; tc.a = Mathf.Clamp01(elapsed / fadeInTime * 2f);
            _levelText.color = tc;
            Color sc = _subtitleText.color; sc.a = tc.a * 0.85f;
            _subtitleText.color = sc;
            yield return null;
        }
        _levelText.transform.localScale = baseScale;

        // Ensure fully visible
        Color fc = _levelText.color; fc.a = 1f; _levelText.color = fc;
        Color sfc = _subtitleText.color; sfc.a = 0.85f; _subtitleText.color = sfc;

        // ── Hold ──────────────────────────────────────────────────────────────
        yield return new WaitForSecondsRealtime(holdTime);

        // ── Fade out everything ───────────────────────────────────────────────
        elapsed = 0f;
        Color startFlash = _flash.color;
        Color startText  = _levelText.color;
        Color startNum   = _newLevelNumber.color;
        Color startSub   = _subtitleText.color;

        while (elapsed < fadeOutTime)
        {
            elapsed += Time.unscaledDeltaTime;
            float t  = elapsed / fadeOutTime;
            float inv = 1f - t;
            _flash.color          = new Color(startFlash.r, startFlash.g, startFlash.b, startFlash.a * inv);
            _levelText.color      = new Color(startText.r,  startText.g,  startText.b,  startText.a  * inv);
            _newLevelNumber.color = new Color(startNum.r,   startNum.g,   startNum.b,   startNum.a   * inv);
            _subtitleText.color   = new Color(startSub.r,   startSub.g,   startSub.b,   startSub.a   * inv);
            yield return null;
        }

        Hide();
    }

    // ── Flavour text by level tier ────────────────────────────────────────────
    static string LevelFlavour(int level)
    {
        if (level >= 20) return "A force to be reckoned with.";
        if (level >= 15) return "The world trembles.";
        if (level >= 10) return "Growing stronger.";
        if (level >= 5)  return "Finding your footing.";
        return "The journey begins.";
    }
}
#endif
