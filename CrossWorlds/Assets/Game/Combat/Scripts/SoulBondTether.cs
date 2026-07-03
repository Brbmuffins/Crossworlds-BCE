using UnityEngine;
using Mirror;

/// <summary>
/// SoulBondTether — Client-side LineRenderer tether between a Cleric and
/// their bonded ally. Attach to the Cleric player prefab.
///
/// Copy to: Assets/Game/Combat/Scripts/SoulBondTether.cs
///
/// Usage:
///   SoulBondTether.Local?.Bond(allyTransform);   // set bond target
///   SoulBondTether.Local?.Unbond();               // clear tether
///
/// Visual: amber line (#FF9C1A), 0.05 width, fades beyond 20m.
/// Requires a LineRenderer component on the same GameObject.
/// </summary>
#if !UNITY_SERVER
[RequireComponent(typeof(LineRenderer))]
public class SoulBondTether : NetworkBehaviour
{
    public static SoulBondTether Local { get; private set; }

    [Header("Tether Visual")]
    public Color tetherColor  = new Color(1f, 0.61f, 0.10f, 0.85f);   // amber
    public float tetherWidth  = 0.05f;
    public float maxTetherDist = 20f;   // fades to alpha 0 beyond this

    private LineRenderer _line;
    private Transform    _bondTarget;
    private float        _pulseTime;

    void Awake()
    {
        _line = GetComponent<LineRenderer>();
        _line.positionCount = 2;
        _line.startWidth = tetherWidth;
        _line.endWidth   = tetherWidth;
        _line.useWorldSpace = true;
        _line.enabled = false;

        // Default material if none assigned
        if (_line.sharedMaterial == null)
        {
            _line.material = new Material(Shader.Find("Sprites/Default"));
            _line.startColor = tetherColor;
            _line.endColor   = tetherColor;
        }
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        Local = this;
    }

    void OnDestroy()
    {
        if (Local == this) Local = null;
    }

    // ─── Public API ───────────────────────────────────────────────────────────
    public void Bond(Transform target)
    {
        _bondTarget   = target;
        _line.enabled = (target != null);
    }

    public void Unbond()
    {
        _bondTarget   = null;
        _line.enabled = false;
    }

    // ─── Update (client only) ─────────────────────────────────────────────────
    void Update()
    {
        if (!isLocalPlayer || _bondTarget == null) return;

        _line.SetPosition(0, transform.position + Vector3.up * 1.2f);
        _line.SetPosition(1, _bondTarget.position + Vector3.up * 1.0f);

        // Pulse + fade by distance
        _pulseTime += Time.deltaTime * 2f;
        float dist  = Vector3.Distance(transform.position, _bondTarget.position);
        float alpha = Mathf.Clamp01(1f - dist / maxTetherDist);
        float pulse = 0.7f + 0.3f * Mathf.Sin(_pulseTime);

        Color c      = tetherColor;
        c.a          = alpha * pulse;
        _line.startColor = c;
        _line.endColor   = new Color(c.r, c.g, c.b, c.a * 0.4f);

        // Warn if bond is close to breaking
        if (dist > maxTetherDist * 0.8f)
            Debug.Log("[CLERIC] Soul bond is stretched — move closer to your bonded ally");
    }
}
#endif
