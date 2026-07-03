#if !UNITY_SERVER
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// ShieldValueHUD — world-space canvas shield bar attached to a shielded target.
/// Added by AbilityCaster.CastAdaptiveShield() via ShieldValueHUD.Attach(target).
///
/// Usage:
///   ShieldValueHUD.Attach(targetGameObject, maxShield);
///
/// The HUD reads Health.ShieldRemaining each frame, flashes on hit, and destroys
/// itself with a particle burst when the shield breaks.
/// </summary>
public class ShieldValueHUD : MonoBehaviour
{
    // ── Factory ───────────────────────────────────────────────────────────────
    /// <summary>
    /// Creates and attaches a ShieldValueHUD to a target. Replaces any existing one.
    /// </summary>
    public static ShieldValueHUD Attach(GameObject target, float maxShield = 20f)
    {
        // Remove existing if present
        var existing = target.GetComponentInChildren<ShieldValueHUD>();
        if (existing != null) Destroy(existing.gameObject);

        var go  = new GameObject("[ShieldValueHUD]");
        var hud = go.AddComponent<ShieldValueHUD>();
        hud.Setup(target, maxShield);
        return hud;
    }

    // ── State ─────────────────────────────────────────────────────────────────
    private Health              _health;
    private GameObject          _targetGO;
    private float               _maxShield;
    private float               _lastShield;

    // UI refs
    private Canvas              _canvas;
    private RectTransform       _barFill;
    private Image               _barFillImg;
    private TextMeshProUGUI     _valueLabel;
    private CanvasGroup         _cg;

    // Flash state
    private bool  _flashing   = false;
    private float _flashTimer = 0f;
    const   float FlashDur    = 0.1f;

    // Punch state
    private bool  _punching    = false;
    private float _punchTimer  = 0f;
    const   float PunchDur     = 0.1f;
    const   float PunchScale   = 1.15f;

    static readonly Color BarNormal = new Color(0.42f, 0.71f, 1.0f, 0.9f);
    static readonly Color BarFlash  = Color.white;

    // ── Setup ─────────────────────────────────────────────────────────────────
    void Setup(GameObject target, float maxShield)
    {
        _targetGO  = target;
        _health    = target.GetComponent<Health>();
        _maxShield = Mathf.Max(1f, maxShield);
        _lastShield = _health != null ? _health.ShieldRemaining : maxShield;

        BuildCanvas(target);
        _health?.onDamageTaken.AddListener(OnHit);
    }

    void BuildCanvas(GameObject target)
    {
        // World-space canvas parented to the target
        _canvas = gameObject.AddComponent<Canvas>();
        _canvas.renderMode  = RenderMode.WorldSpace;

        var rt = GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(1.2f, 0.18f);

        // Position 0.3u above the object (EnemyHealthBar sits ~0.1u above head)
        transform.SetParent(target.transform, false);
        transform.localPosition = new Vector3(0f, 2.3f, 0f); // above nameplate
        transform.localRotation = Quaternion.identity;
        transform.localScale    = Vector3.one * 0.01f;        // world-space scale

        _cg = gameObject.AddComponent<CanvasGroup>();
        _cg.alpha = 1f;

        // Bar background
        var bg = new GameObject("BG"); bg.transform.SetParent(transform, false);
        var bgRect = bg.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero; bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = bgRect.offsetMax = Vector2.zero;
        var bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0.1f, 0.15f, 0.2f, 0.8f);

        // Fill
        var fill = new GameObject("Fill"); fill.transform.SetParent(transform, false);
        _barFill = fill.AddComponent<RectTransform>();
        _barFill.anchorMin = new Vector2(0f, 0.05f);
        _barFill.anchorMax = new Vector2(1f, 0.95f);
        _barFill.offsetMin = new Vector2(2f, 0f);
        _barFill.offsetMax = new Vector2(-2f, 0f);
        _barFillImg = fill.AddComponent<Image>();
        _barFillImg.color = BarNormal;
        _barFillImg.type  = Image.Type.Filled;
        _barFillImg.fillMethod    = Image.FillMethod.Horizontal;
        _barFillImg.fillAmount    = 1f;

        // Value label
        var lbl = new GameObject("Label"); lbl.transform.SetParent(transform, false);
        var lblRect = lbl.AddComponent<RectTransform>();
        lblRect.anchorMin = Vector2.zero; lblRect.anchorMax = Vector2.one;
        lblRect.offsetMin = new Vector2(0f, 0f); lblRect.offsetMax = new Vector2(-4f, 0f);
        _valueLabel = lbl.AddComponent<TextMeshProUGUI>();
        _valueLabel.fontSize  = 24f;  // in world-space canvas units
        _valueLabel.color     = Color.white;
        _valueLabel.alignment = TextAlignmentOptions.Right;
        _valueLabel.fontStyle = FontStyles.Bold;

        UpdateVisuals(_lastShield);
    }

    // ── Per-frame ─────────────────────────────────────────────────────────────
    void Update()
    {
        if (_targetGO == null) { Destroy(gameObject); return; }

        // Billboard — face camera
        var cam = Camera.main;
        if (cam != null)
            transform.rotation = Quaternion.LookRotation(
                transform.position - cam.transform.position, cam.transform.up);

        // Check shield
        float current = _health != null ? _health.ShieldRemaining : 0f;

        // Shield broke
        if (current <= 0f && _lastShield > 0f)
        {
            ShieldBurst();
            return;
        }

        // Max grows (AdaptiveShield)
        if (current > _maxShield) _maxShield = current;

        UpdateVisuals(current);
        _lastShield = current;

        // Flash tick
        if (_flashing)
        {
            _flashTimer -= Time.deltaTime;
            if (_flashTimer <= 0f) { _flashing = false; _barFillImg.color = BarNormal; }
        }

        // Punch tick
        if (_punching)
        {
            _punchTimer -= Time.deltaTime;
            float t = 1f - Mathf.Clamp01(_punchTimer / PunchDur);
            float s = Mathf.Lerp(PunchScale, 1f, t);
            _barFill.localScale = new Vector3(s, s, 1f);
            if (_punchTimer <= 0f) { _punching = false; _barFill.localScale = Vector3.one; }
        }
    }

    void UpdateVisuals(float current)
    {
        float fraction = Mathf.Clamp01(current / _maxShield);
        _barFillImg.fillAmount = fraction;
        _valueLabel.text = $"Shield: {Mathf.CeilToInt(current)}";
    }

    void OnHit(float dmg)
    {
        // Flash bar white
        _flashing   = true;
        _flashTimer = FlashDur;
        _barFillImg.color = BarFlash;

        // Punch scale
        _punching   = true;
        _punchTimer = PunchDur;
        _barFill.localScale = Vector3.one * PunchScale;
    }

    // ── Shield break ──────────────────────────────────────────────────────────
    void ShieldBurst()
    {
        _health?.onDamageTaken.RemoveListener(OnHit);

        // 8 blue radial particles
        SpawnBreakParticles();

        // Fade and destroy
        StartCoroutine(FadeOut());
    }

    void SpawnBreakParticles()
    {
        var ps   = new GameObject("ShieldBreak").AddComponent<ParticleSystem>();
        ps.transform.position = transform.position;

        var main        = ps.main;
        main.loop                = false;
        main.duration            = 0.3f;
        main.playOnAwake         = false;
        main.startLifetime       = new ParticleSystem.MinMaxCurve(0.4f, 0.7f);
        main.startSpeed          = new ParticleSystem.MinMaxCurve(2f, 5f);
        main.startSize           = new ParticleSystem.MinMaxCurve(0.05f, 0.15f);
        main.startColor          = new ParticleSystem.MinMaxGradient(
            new Color(0.42f, 0.71f, 1f), new Color(0.8f, 0.9f, 1f));
        main.maxParticles        = 8;

        var emission    = ps.emission;
        emission.enabled = true;
        emission.rateOverTime = 0f;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 8) });

        var shape       = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius    = 0.1f;

        var psr  = ps.GetComponent<ParticleSystemRenderer>();
        var mat  = new Material(Shader.Find("Sprites/Default"));
        mat.color = new Color(0.6f, 0.8f, 1f);
        psr.material = mat;

        ps.Play();
        Destroy(ps.gameObject, 1.5f);
    }

    IEnumerator FadeOut()
    {
        float t = 0f;
        while (t < 0.3f) { t += Time.deltaTime; _cg.alpha = 1f - t / 0.3f; yield return null; }
        _health?.onDamageTaken.RemoveListener(OnHit);
        Destroy(gameObject);
    }

    void OnDestroy()
    {
        _health?.onDamageTaken.RemoveListener(OnHit);
    }
}
#endif
