#if !UNITY_SERVER
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// PlayerHealthBar — self-bootstrapping bottom-left HP + shield bar for the local player.
///
/// Layout (bottom-left HUD):
///   [Name]
///   [████████████░░░░░]  ← HP fill + shield overlay (yellow)
///    123 / 200  HP
///
/// Colour shifts: green > 60%, yellow 30–60%, red &lt; 30%.
/// Shield appears as a yellow extension to the right of the HP fill.
/// Pulses when HP is critically low (&lt;20%).
/// Self-bootstrapping via RuntimeInitializeOnLoadMethod.
/// </summary>
public class PlayerHealthBar : MonoBehaviour
{
    // ── Layout ────────────────────────────────────────────────────────────────
    const float BAR_W   = 260f;
    const float BAR_H   = 18f;
    const float MARGIN  = 16f;

    // ── Colours ───────────────────────────────────────────────────────────────
    static readonly Color ColHigh    = new Color(0.20f, 0.82f, 0.35f);
    static readonly Color ColMid     = new Color(0.90f, 0.78f, 0.10f);
    static readonly Color ColLow     = new Color(0.90f, 0.20f, 0.12f);
    static readonly Color ColShield  = new Color(0.30f, 0.70f, 1.00f, 0.88f);
    static readonly Color ColBg      = new Color(0.06f, 0.06f, 0.08f, 0.88f);
    static readonly Color ColText    = new Color(1.00f, 1.00f, 1.00f, 0.90f);
    static readonly Color ColName    = new Color(0.80f, 0.80f, 0.95f, 1.00f);

    // ── UI refs ───────────────────────────────────────────────────────────────
    Image   _hpFill;
    Image   _shieldFill;
    Text    _hpText;
    Text    _nameText;
    RectTransform _barRT;

    // ── Runtime ───────────────────────────────────────────────────────────────
    Health  _health;
    bool    _subscribed;
    bool    _pulsing;

    // ── Bootstrap ─────────────────────────────────────────────────────────────
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        var go = new GameObject("PlayerHealthBar", typeof(RectTransform));
        DontDestroyOnLoad(go);
        go.AddComponent<PlayerHealthBar>();
    }

    // ── Lifecycle ─────────────────────────────────────────────────────────────
    void Awake() { BuildUI(); SetVisible(false); }

    void Update()
    {
        if (!_subscribed)
        {
            TryFindHealth();
            return;
        }

        // Refresh every frame for shield (no event — polling is fine for one bar)
        if (_health != null)
            UpdateShield(_health.ShieldRemaining, _health.maxHealth);
    }

    void OnDestroy()
    {
        if (_health != null)
        {
            _health.onHealthChanged.RemoveListener(OnHealthChanged);
            _health.onDeath.RemoveListener(OnDeath);
        }
    }

    // ── Find local player ─────────────────────────────────────────────────────
    void TryFindHealth()
    {
        foreach (var id in FindObjectsByType<Mirror.NetworkIdentity>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
        {
            if (!id.isLocalPlayer) continue;
            var h = id.GetComponent<Health>();
            if (h == null) continue;

            _health     = h;
            _subscribed = true;
            _health.onHealthChanged.AddListener(OnHealthChanged);
            _health.onDeath.AddListener(OnDeath);

            // Grab name from PlayerIdentity
            var pid = id.GetComponent<PlayerIdentity>();
            if (pid != null && !string.IsNullOrEmpty(pid.playerName))
                _nameText.text = pid.playerName;

            UpdateHP(_health.currentHealth, _health.maxHealth);
            SetVisible(true);
            return;
        }
    }

    // ── Events ────────────────────────────────────────────────────────────────
    void OnHealthChanged(float current, float max)
    {
        UpdateHP(current, max);

        bool crit = max > 0f && (current / max) < 0.20f;
        if (crit && !_pulsing) StartCoroutine(PulseLow());
        else if (!crit) _pulsing = false;
    }

    void OnDeath()
    {
        _hpFill.fillAmount = 0f;
        _hpText.text       = "DEAD";
        _hpText.color      = new Color(1f, 0.3f, 0.3f);
    }

    // ── UI updates ────────────────────────────────────────────────────────────
    void UpdateHP(float current, float max)
    {
        float frac = max > 0f ? Mathf.Clamp01(current / max) : 0f;
        _hpFill.fillAmount = frac;
        _hpFill.color = frac > 0.60f ? ColHigh : frac > 0.30f ? ColMid : ColLow;
        _hpText.text  = $"{Mathf.CeilToInt(current)} / {Mathf.RoundToInt(max)}";
    }

    void UpdateShield(float shield, float maxHp)
    {
        if (shield <= 0f || maxHp <= 0f)
        {
            _shieldFill.fillAmount = 0f;
            return;
        }
        // Shield shown as fraction of maxHp, capped at 0.4 of the bar (generous visual)
        _shieldFill.fillAmount = Mathf.Clamp01(shield / maxHp * 2f) * 0.4f;
    }

    // ── Low-health pulse ──────────────────────────────────────────────────────
    IEnumerator PulseLow()
    {
        _pulsing = true;
        while (_pulsing)
        {
            float t = 0f;
            while (t < 0.5f)
            {
                t += Time.unscaledDeltaTime;
                float a = Mathf.Lerp(0.4f, 1f, t / 0.5f);
                _hpFill.color = new Color(ColLow.r, ColLow.g, ColLow.b, a);
                yield return null;
            }
            t = 0f;
            while (t < 0.5f)
            {
                t += Time.unscaledDeltaTime;
                float a = Mathf.Lerp(1f, 0.4f, t / 0.5f);
                _hpFill.color = new Color(ColLow.r, ColLow.g, ColLow.b, a);
                yield return null;
            }
        }
    }

    // ── UI construction ───────────────────────────────────────────────────────
    void BuildUI()
    {
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 90;
        gameObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        gameObject.AddComponent<GraphicRaycaster>();

        // Anchor: bottom-left
        var root = new GameObject("HPPanel", typeof(RectTransform));
        root.transform.SetParent(transform, false);
        var rootRT = root.GetComponent<RectTransform>();
        rootRT.anchorMin        = Vector2.zero;
        rootRT.anchorMax        = Vector2.zero;
        rootRT.pivot            = Vector2.zero;
        rootRT.sizeDelta        = new Vector2(BAR_W, 54f);
        rootRT.anchoredPosition = new Vector2(MARGIN, MARGIN);

        // Name label
        var nameGO = new GameObject("NameLabel", typeof(RectTransform));
        nameGO.transform.SetParent(root.transform, false);
        var nameRT = nameGO.GetComponent<RectTransform>();
        nameRT.anchorMin        = new Vector2(0f, 1f);
        nameRT.anchorMax        = new Vector2(1f, 1f);
        nameRT.pivot            = new Vector2(0f, 0f);
        nameRT.sizeDelta        = new Vector2(0f, 20f);
        nameRT.anchoredPosition = new Vector2(0f, 4f);
        _nameText = nameGO.AddComponent<Text>();
        _nameText.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        _nameText.fontSize  = 14;
        _nameText.fontStyle = FontStyle.Bold;
        _nameText.color     = ColName;
        _nameText.text      = "—";

        // Bar background
        var bgGO = new GameObject("BarBG", typeof(RectTransform));
        bgGO.transform.SetParent(root.transform, false);
        _barRT = bgGO.GetComponent<RectTransform>();
        _barRT.anchorMin = new Vector2(0f, 0f);
        _barRT.anchorMax = new Vector2(0f, 0f);
        _barRT.pivot     = new Vector2(0f, 0f);
        _barRT.sizeDelta = new Vector2(BAR_W, BAR_H);
        _barRT.anchoredPosition = new Vector2(0f, 22f);
        var bg = bgGO.AddComponent<Image>();
        bg.color = ColBg;

        // HP fill
        var fillGO = new GameObject("HPFill", typeof(RectTransform));
        fillGO.transform.SetParent(bgGO.transform, false);
        var fillRT = fillGO.GetComponent<RectTransform>();
        fillRT.anchorMin = Vector2.zero;
        fillRT.anchorMax = Vector2.one;
        fillRT.offsetMin = new Vector2(2f, 2f);
        fillRT.offsetMax = new Vector2(-2f, -2f);
        _hpFill = fillGO.AddComponent<Image>();
        _hpFill.type       = Image.Type.Filled;
        _hpFill.fillMethod = Image.FillMethod.Horizontal;
        _hpFill.fillAmount = 1f;
        _hpFill.color      = ColHigh;

        // Shield overlay (right side of bar)
        var shieldGO = new GameObject("ShieldFill", typeof(RectTransform));
        shieldGO.transform.SetParent(bgGO.transform, false);
        var shieldRT = shieldGO.GetComponent<RectTransform>();
        shieldRT.anchorMin = new Vector2(1f, 0f);
        shieldRT.anchorMax = new Vector2(1f, 1f);
        shieldRT.pivot     = new Vector2(1f, 0.5f);
        shieldRT.sizeDelta = new Vector2(0f, -4f);
        shieldRT.anchoredPosition = new Vector2(-2f, 0f);
        _shieldFill = shieldGO.AddComponent<Image>();
        _shieldFill.type       = Image.Type.Filled;
        _shieldFill.fillMethod = Image.FillMethod.Horizontal;
        _shieldFill.fillOrigin = (int)Image.OriginHorizontal.Right;
        _shieldFill.fillAmount = 0f;
        _shieldFill.color      = ColShield;
        // Need a width to fill from — tie to barRT width
        _shieldFill.GetComponent<RectTransform>().sizeDelta = new Vector2(BAR_W - 4f, -(4f));

        // HP text
        var textGO = new GameObject("HPText", typeof(RectTransform));
        textGO.transform.SetParent(root.transform, false);
        var textRT = textGO.GetComponent<RectTransform>();
        textRT.anchorMin        = Vector2.zero;
        textRT.anchorMax        = Vector2.zero;
        textRT.pivot            = new Vector2(0f, 1f);
        textRT.sizeDelta        = new Vector2(BAR_W, 18f);
        textRT.anchoredPosition = new Vector2(0f, 20f);
        _hpText = textGO.AddComponent<Text>();
        _hpText.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        _hpText.fontSize  = 11;
        _hpText.color     = ColText;
        _hpText.alignment = TextAnchor.MiddleLeft;
        _hpText.text      = "— / —";
    }

    void SetVisible(bool v)
    {
        foreach (Transform t in transform) t.gameObject.SetActive(v);
    }
}
#endif
