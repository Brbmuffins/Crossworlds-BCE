#if UNITY_EDITOR || !UNITY_SERVER
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// EnemyHealthBar — world-space health bar that floats above an enemy.
/// Self-contained: reads Health from the same GameObject, builds its own Canvas.
/// Added programmatically by EnemyController.OnStartClient — no prefab setup needed.
///
/// Visual: slim segmented bar (green current HP over red missing HP) with a dark border.
/// Faces camera via LateUpdate billboard, hidden when enemy is dead.
/// </summary>
[RequireComponent(typeof(Health))]
public class EnemyHealthBar : MonoBehaviour
{
    // ── Config ────────────────────────────────────────────────────────────────
    const float BAR_WIDTH       = 1.2f;    // world-space width of bar
    const float BAR_HEIGHT      = 0.12f;
    const float DEFAULT_HEIGHT  = 1.65f;   // fallback above the enemy root
    const float DEFAULT_OFFSET  = 0.25f;   // above visible/collider bounds
    const float MIN_HEIGHT      = 0.2f;
    const float CANVAS_SCALE    = 0.01f;   // world-space canvas scale

    // Colours
    static readonly Color ColBorder = new Color(0.02f, 0.02f, 0.02f, 0.95f);
    static readonly Color ColHealth = new Color(0.15f, 0.85f, 0.20f, 0.95f);
    static readonly Color ColDamage = new Color(0.95f, 0.08f, 0.06f, 0.95f);

    // ── References ────────────────────────────────────────────────────────────
    Health  _health;
    Image   _fill;
    RectTransform _fillRect;
    Canvas  _canvas;
    bool    _canRevealFromHealthChange;
    Camera  _camera;
    readonly List<Renderer> _boundsRenderers = new List<Renderer>(8);
    readonly List<Collider> _boundsColliders = new List<Collider>(4);
    bool _boundsSourcesDirty = true;

    // ── Setup ─────────────────────────────────────────────────────────────────
    void Awake()
    {
        _health = GetComponent<Health>();
        if (_health == null) { enabled = false; return; }

        _health.onHealthChanged.AddListener(OnHealthChanged);
        _health.onDamageTaken.AddListener(OnDamageTaken);
        _health.onDeath.AddListener(OnDeath);
    }

    void Start()
    {
        _canRevealFromHealthChange = true;

        if (_health != null
            && _health.currentHealth > 0f
            && _health.maxHealth > 0f
            && _health.currentHealth < _health.maxHealth)
        {
            Reveal(_health.currentHealth, _health.maxHealth);
        }
    }

    void OnDestroy()
    {
        if (_health != null)
        {
            _health.onHealthChanged.RemoveListener(OnHealthChanged);
            _health.onDamageTaken.RemoveListener(OnDamageTaken);
            _health.onDeath.RemoveListener(OnDeath);
        }
    }

    void OnTransformChildrenChanged()
    {
        _boundsSourcesDirty = true;
    }

    // ── World-space canvas ────────────────────────────────────────────────────
    void BuildCanvas()
    {
        var canvasGO = new GameObject("EnemyHPBar");
        canvasGO.transform.SetParent(transform, false);
        canvasGO.transform.localPosition = new Vector3(0f, GetBarHeight(), 0f);
        canvasGO.transform.localScale    = Vector3.one * CANVAS_SCALE;

        _canvas = canvasGO.AddComponent<Canvas>();
        _canvas.renderMode = RenderMode.WorldSpace;
        _canvas.sortingOrder = 10;

        // Canvas size in canvas-units (scaled by CANVAS_SCALE → world units)
        var canvasRT = canvasGO.GetComponent<RectTransform>();
        float w = BAR_WIDTH  / CANVAS_SCALE;
        float h = BAR_HEIGHT / CANVAS_SCALE;
        canvasRT.sizeDelta = new Vector2(w, h);

        // Border
        var bgGO = new GameObject("BG", typeof(RectTransform));
        bgGO.transform.SetParent(canvasGO.transform, false);
        var bgRT = bgGO.GetComponent<RectTransform>();
        bgRT.anchorMin = Vector2.zero;
        bgRT.anchorMax = Vector2.one;
        bgRT.offsetMin = Vector2.zero;
        bgRT.offsetMax = Vector2.zero;
        var bgImg = bgGO.AddComponent<Image>();
        bgImg.color = ColBorder;

        // Missing-health track. This stays full width; the green fill sits above it.
        var damageGO = new GameObject("MissingHealth", typeof(RectTransform));
        damageGO.transform.SetParent(canvasGO.transform, false);
        var damageRT = damageGO.GetComponent<RectTransform>();
        damageRT.anchorMin = Vector2.zero;
        damageRT.anchorMax = Vector2.one;
        damageRT.offsetMin = new Vector2(2f, 2f);
        damageRT.offsetMax = new Vector2(-2f, -2f);
        var damageImg = damageGO.AddComponent<Image>();
        damageImg.color = ColDamage;

        // Fill
        var fillGO = new GameObject("Fill", typeof(RectTransform));
        fillGO.transform.SetParent(damageGO.transform, false);
        _fillRect = fillGO.GetComponent<RectTransform>();
        _fillRect.anchorMin = Vector2.zero;
        _fillRect.anchorMax = Vector2.one;
        _fillRect.pivot     = new Vector2(0f, 0.5f);
        _fillRect.offsetMin = Vector2.zero;
        _fillRect.offsetMax = Vector2.zero;
        _fill = fillGO.AddComponent<Image>();
        _fill.type  = Image.Type.Simple;
        _fill.color = ColHealth;
    }

    float GetBarHeight()
    {
        float fixedHeight = _health != null ? _health.EnemyHealthBarFixedHeight : -1f;
        if (fixedHeight >= 0f)
            return Mathf.Max(MIN_HEIGHT, fixedHeight);

        float heightOffset = _health != null ? _health.EnemyHealthBarHeightOffset : DEFAULT_OFFSET;
        float top = float.NegativeInfinity;

        RefreshBoundsSourcesIfNeeded();

        foreach (Renderer renderer in _boundsRenderers)
        {
            if (renderer == null || !renderer.enabled || !renderer.gameObject.activeInHierarchy) continue;
            top = Mathf.Max(top, transform.InverseTransformPoint(renderer.bounds.max).y);
        }

        foreach (Collider collider in _boundsColliders)
        {
            if (collider == null || !collider.enabled || !collider.gameObject.activeInHierarchy) continue;
            top = Mathf.Max(top, transform.InverseTransformPoint(collider.bounds.max).y);
        }

        if (float.IsNegativeInfinity(top))
            top = DEFAULT_HEIGHT - heightOffset;

        return Mathf.Max(MIN_HEIGHT, top + heightOffset);
    }

    void RefreshBoundsSourcesIfNeeded()
    {
        if (!_boundsSourcesDirty) return;

        _boundsSourcesDirty = false;
        _boundsRenderers.Clear();
        _boundsColliders.Clear();
        GetComponentsInChildren(true, _boundsRenderers);
        GetComponentsInChildren(true, _boundsColliders);
    }

    // ── Billboard — always face camera ────────────────────────────────────────
    void LateUpdate()
    {
        if (_canvas == null || !_canvas.gameObject.activeSelf) return;
        _canvas.transform.localPosition = new Vector3(0f, GetBarHeight(), 0f);

        if (_camera == null || !_camera.isActiveAndEnabled)
            _camera = Camera.main;
        if (_camera == null) return;
        // Face toward camera (not toward target like LookAt, just copy camera rotation)
        _canvas.transform.rotation = _camera.transform.rotation;
    }

    // ── Health events ─────────────────────────────────────────────────────────
    void OnHealthChanged(float current, float max)
    {
        if (current <= 0f)
        {
            OnDeath();
            return;
        }

        if (_canRevealFromHealthChange && max > 0f && current < max)
            Reveal(current, max);
        else
            UpdateFill(current, max);
    }

    void OnDamageTaken(float amount)
    {
        if (amount <= 0f || _health == null || _health.currentHealth <= 0f) return;
        Reveal(_health.currentHealth, _health.maxHealth);
    }

    void OnDeath()
    {
        if (_canvas != null) _canvas.gameObject.SetActive(false);
    }

    void Reveal(float current, float max)
    {
        if (_canvas == null)
            BuildCanvas();

        if (_canvas != null && !_canvas.gameObject.activeSelf)
            _canvas.gameObject.SetActive(true);

        UpdateFill(current, max);
    }

    void UpdateFill(float current, float max)
    {
        if (_fill == null || _fillRect == null) return;
        float fraction = max > 0f ? Mathf.Clamp01(current / max) : 0f;
        _fillRect.anchorMax = new Vector2(fraction, 1f);
    }
}
#endif
