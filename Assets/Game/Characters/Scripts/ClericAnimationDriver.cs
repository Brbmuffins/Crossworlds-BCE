#if UNITY_EDITOR || !UNITY_SERVER
using Mirror;
using UnityEngine;

/// <summary>
/// ClericAnimationDriver — drives the Cleric's Animator from velocity and
/// hooks AbilityCaster heal casts to trigger ClericHealVFX.
///
/// Works in tandem with:
///   • CastAnimator  — fires CastHeal / CastDamage / CastSupport triggers
///   • PlayerAnimator — handles GetHit / Death / IsInCombat / Speed
///   • ClericHealVFX  — particle burst on heal (called from here on the server)
///
/// Attach to: Cleric prefab root. Runs only on the local client.
/// </summary>
[RequireComponent(typeof(CastAnimator))]
public class ClericAnimationDriver : NetworkBehaviour
{
    [Header("Animator")]
    [Tooltip("Leave blank — resolved from children at Start")]
    public Animator animator;

    [Header("Speed Tuning")]
    [Tooltip("Walk speed — animator Speed reaches 1.0 here")]
    public float baseMoveSpeed   = 5f;
    [Tooltip("Sprint speed — animator Speed reaches 1.5 here")]
    public float baseSprintSpeed = 9f;

    [Header("References")]
    public AbilityCaster abilityCaster;
    public ClericHealVFX healVFX;

    // ── Private ───────────────────────────────────────────────────────────────
    private Rigidbody _rb;
    private bool      _hooked;
    private bool      _hasSpeedParam;   // controller actually declares "Speed"

    static readonly int SpeedHash     = Animator.StringToHash("Speed");
    static readonly int CastHealHash  = Animator.StringToHash("CastHeal");

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    public override void OnStartLocalPlayer()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();
        _rb = GetComponent<Rigidbody>();

        // Only drive Speed if the assigned controller declares it — otherwise
        // SetFloat(SpeedHash, …) spams "Parameter 'Hash …' does not exist" every frame.
        if (animator != null && animator.runtimeAnimatorController != null)
            foreach (var p in animator.parameters)
                if (p.nameHash == SpeedHash) { _hasSpeedParam = true; break; }

        HookAbilityCaster();
    }

    void OnDestroy() => UnhookAbilityCaster();

    // ── Heal cast hook ────────────────────────────────────────────────────────

    void HookAbilityCaster()
    {
        if (_hooked || abilityCaster == null) return;
        abilityCaster.OnHealCast += HandleHealCast;
        _hooked = true;
    }

    void UnhookAbilityCaster()
    {
        if (!_hooked || abilityCaster == null) return;
        abilityCaster.OnHealCast -= HandleHealCast;
        _hooked = false;
    }

    void HandleHealCast()
    {
        // Trigger animation locally
        if (animator != null) animator.SetTrigger(CastHealHash);

        // Tell server to broadcast VFX to all clients
        if (isLocalPlayer) CmdRequestHealVFX();
    }

    [Command]
    void CmdRequestHealVFX()
    {
        healVFX?.TriggerHealVFX();
    }

    // ── Speed parameter ───────────────────────────────────────────────────────

    void Update()
    {
        if (!isLocalPlayer || animator == null || _rb == null || animator.runtimeAnimatorController == null) return;
        if (!_hasSpeedParam) return;

        float speed = new Vector3(_rb.linearVelocity.x, 0f, _rb.linearVelocity.z).magnitude;
        float normalized = Mathf.Clamp(speed / baseSprintSpeed * 1.5f, 0f, 1.5f);
        animator.SetFloat(SpeedHash, normalized, 0.1f, Time.deltaTime);
    }
}
#endif
