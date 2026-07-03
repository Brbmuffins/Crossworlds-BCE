#if !UNITY_SERVER
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// EnemyHealthBar — world-space health bar that floats above an enemy.
/// Self-contained: reads Health from the same GameObject, builds its own Canvas.
/// Added programmatically by EnemyController.OnStartClient — no prefab setup needed.
///
/// Visual: slim red bar (width ~1 unit in world space) with a dark background.
/// Faces camera via LateUpdate billboard, hidden when enemy is dead.
/// </summary>
[RequireComponent(typeof(Health))]
public class EnemyHealthBar : MonoBehaviour
{
    // ── Config ────────────────────────────────────────────────────────────────
    const float BAR_WIDTH     = 1.2f;    // world-space width of bar
    const float BAR_HEIGHT    = 0.12f;
    const float FLOAT_HEIGHT  = 2.5f;    // above the enemy root
    const float CANVAS_SCALE  = 0.01f;   // world-space canvas scale

    // Colours
    static readonly Color ColBg      = new Color(0.08f, 0.06f, 0.08f, 0.85f);
    static readonly Color ColFill    = new Color(0.85f, 0.15f, 0.15f, 0.92f);
    static readonly Color ColLow     = new Color(1.00f, 0.40f, 0.05f, 0.92f);  // <30%
    static readonly Color ColCritLow = new Color(1.00f, 0.90f, 0.10f, 0.92f);  // <15%

    // ── References ────────────────────────────────────────────────────────────
    Health  _health;
    Image   _fill;
    Canvas  _canvas;

    // ── Setup ─────────────────────────────────────────────────────────────────
    void Awake()
    {
        _health = GetComponent<Health>();
        if (_health == null) { enabled = false; return; }

        BuildCanvas();

        _health.onHealthChanged.AddListener(OnHealthChanged);
        _health.onDeath.AddListener(OnDeath);

        // Set initial fill
        UpdateFill(_health.currentHealth, _health.maxHealth);
    }

    void OnDestroy()
    {
        if (_health != null)
        {
            _health.onHealthChanged.RemoveListener(OnHealthChanged);
            _health.onDeath.RemoveListener(OnDeath);
        }
    }

    // ── World-space canvas ────────────────────────────────────────────────────
    void BuildCanvas()
    {
        var canvasGO = new GameObject("EnemyHPBar");
        canvasGO.transform.SetParent(transform, false);
        canvasGO.transform.localPosition = new Vector3(0f, FLOAT_HEIGHT, 0f);
        canvasGO.transform.localScale    = Vector3.one * CANVAS_SCALE;

        _canvas = canvasGO.AddComponent<Canvas>();
        _canvas.renderMode = RenderMode.WorldSpace;
        _canvas.sortingOrder = 10;

        // Canvas size in canvas-units (scaled by CANVAS_SCALE → world units)
        var canvasRT = canvasGO.GetComponent<RectTransform>();
        float w = BAR_WIDTH  / CANVAS_SCALE;
        float h = BAR_HEIGHT / CANVAS_SCALE;
        canvasRT.sizeDelta = new Vector2(w, h);

        // Background
        var bgGO = new GameObject("BG", typeof(RectTransform));
        bgGO.transform.SetParent(canvasGO.transform, false);
        var bgRT = bgGO.GetComponent<RectTransform>();
        bgRT.anchorMin = Vector2.zero;
        bgRT.anchorMax = Vector2.one;
        bgRT.offsetMin = Vector2.zero;
        bgRT.offsetMax = Vector2.zero;
        var bgImg = bgGO.AddComponent<Image>();
        bgImg.color = ColBg;

        // Fill
        var fillGO = new GameObject("Fill", typeof(RectTransform));
        fillGO.transform.SetParent(canvasGO.transform, false);
        var fillRT = fillGO.GetComponent<RectTransform>();
        fillRT.anchorMin = Vector2.zero;
        fillRT.anchorMax = new Vector2(1f, 1f);
        fillRT.pivot     = new Vector2(0f, 0.5f);
        fillRT.offsetMin = new Vector2(2f, 2f);
        fillRT.offsetMax = new Vector2(-2f, -2f);
        _fill = fillGO.AddComponent<Image>();
        _fill.type       = Image.Type.Filled;
        _fill.fillMethod = Image.FillMethod.Horizontal;
        _fill.fillAmount = 1f;
        _fill.color      = ColFill;
    }

    // ── Billboard — always face camera ────────────────────────────────────────
    void LateUpdate()
    {
        if (_canvas == null) return;
        var cam = Camera.main;
        if (cam == null) return;
        // Face toward camera (not toward target like LookAt, just copy camera rotation)
        _canvas.transform.rotation = cam.transform.rotation;
    }

    // ── Health events ─────────────────────────────────────────────────────────
    void OnHealthChanged(float current, float max) => UpdateFill(current, max);

    void OnDeath()
    {
        if (_canvas != null) _canvas.gameObject.SetActive(false);
    }

    void UpdateFill(float current, float max)
    {
        if (_fill == null) return;
        float fraction = max > 0f ? Mathf.Clamp01(current / max) : 0f;
        _fill.fillAmount = fraction;
        _fill.color = fraction < 0.15f ? ColCritLow
                    : fraction < 0.30f ? ColLow
                    : ColFill;
    }
}
#endif
