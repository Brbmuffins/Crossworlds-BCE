#if !UNITY_SERVER
using System.Collections;
using UnityEngine;

/// <summary>
/// SoulBondTether — draws a pulsing LineRenderer between the Cleric (owner)
/// and a bonded ally. Created at runtime by AbilityCaster.CastTransferProtocol().
///
/// Usage:
///   var tether = Instantiate(SoulBondTether.Prefab);
///   tether.Init(clericTransform, allyTransform, transferProtocolHandler);
///
/// Or create at runtime without a prefab:
///   SoulBondTether.Create(clericTransform, allyTransform, handler);
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class SoulBondTether : MonoBehaviour
{
    // ── Factory ───────────────────────────────────────────────────────────────
    /// <summary>
    /// Spawns a tether at runtime. Call from AbilityCaster.CastTransferProtocol().
    /// </summary>
    public static SoulBondTether Create(Transform owner, Transform target,
                                        TransferProtocolHandler handler)
    {
        var go     = new GameObject("[SoulBondTether]");
        var tether = go.AddComponent<SoulBondTether>();
        tether.Init(owner, target, handler);
        return tether;
    }

    // ── State ─────────────────────────────────────────────────────────────────
    private Transform                _owner;
    private Transform                _target;
    private TransferProtocolHandler  _handler;
    private LineRenderer             _line;

    // Pulse state
    private float _pulseT = 0f;

    // Flash state
    private bool  _flashing    = false;
    private float _flashTimer  = 0f;
    const   float FlashDur     = 0.10f;

    // Colors
    static readonly Color NormalColor = new Color(1.0f, 0.75f, 0.1f, 0.7f);
    static readonly Color FlashColor  = new Color(1.0f, 1.0f, 1.0f, 1.0f);

    // ── Init ──────────────────────────────────────────────────────────────────
    public void Init(Transform owner, Transform target, TransferProtocolHandler handler)
    {
        _owner   = owner;
        _target  = target;
        _handler = handler;

        _line = GetComponent<LineRenderer>();
        _line.positionCount = 2;
        _line.startWidth    = 0.05f;
        _line.endWidth      = 0.05f;
        _line.useWorldSpace = true;

        // Amber/gold material
        var mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = NormalColor;
        _line.material     = mat;
        _line.startColor   = NormalColor;
        _line.endColor     = NormalColor;
        _line.numCapVertices = 4;
        _line.numCornerVertices = 4;

        // Listen for damage redirect events
        if (handler != null)
            handler.onDamageRedirected += OnDamageRedirected;
    }

    // ── Lifecycle ─────────────────────────────────────────────────────────────
    void Update()
    {
        // Destroy self when bond expires
        if (_handler == null || !_handler.IsActive || _owner == null || _target == null)
        {
            Destroy(gameObject);
            return;
        }

        // Update line positions
        _line.SetPosition(0, _owner.position  + Vector3.up * 1.2f);
        _line.SetPosition(1, _target.position + Vector3.up * 1.2f);

        // Pulse alpha on a 1s sine cycle
        if (!_flashing)
        {
            _pulseT += Time.deltaTime * (2f * Mathf.PI); // full cycle per second
            float alpha = Mathf.Lerp(0.4f, 0.8f, (Mathf.Sin(_pulseT) + 1f) * 0.5f);
            Color c = NormalColor;
            c.a = alpha;
            _line.startColor = c;
            _line.endColor   = c;
        }
        else
        {
            _flashTimer -= Time.deltaTime;
            if (_flashTimer <= 0f)
            {
                _flashing = false;
                _line.startColor = NormalColor;
                _line.endColor   = NormalColor;
            }
        }
    }

    void OnDestroy()
    {
        if (_handler != null)
            _handler.onDamageRedirected -= OnDamageRedirected;
    }

    // ── Damage redirect callback ───────────────────────────────────────────────
    void OnDamageRedirected(float amount)
    {
        // Flash white
        _flashing   = true;
        _flashTimer = FlashDur;
        _line.startColor = FlashColor;
        _line.endColor   = FlashColor;

        // Spawn "BOND" floating text at midpoint
        if (_owner != null && _target != null)
        {
            Vector3 mid = (_owner.position + _target.position) * 0.5f + Vector3.up * 1.2f;
            FloatingDamageText.Spawn(mid, amount, FloatingDamageText.DamageType.Shield, "BOND");
        }
    }
}
#endif
