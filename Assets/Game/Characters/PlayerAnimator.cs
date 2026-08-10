#if UNITY_EDITOR || !UNITY_SERVER
using UnityEngine;
using Mirror;

/// <summary>
/// PlayerAnimator — Combat animation bridge for the local player.
///
/// Works alongside PlayerMovement (which already drives isMoving / isSprinting /
/// isBackwards / dodge / Jump) and CastAnimator (which fires CastDamage /
/// CastHeal / CastSupport).  This script handles everything else:
///
///   float  Speed         0 = idle, 1 = run, 1.5 = sprint — for blend trees
///   bool   IsInCombat    true when an enemy is within combatCheckRadius →
///                        switches the resting pose from Idle to IdleCombat
///   trigger GetHit       fired when the local player takes damage
///   trigger Death        fired when the local player becomes downed
///   bool    IsDead       true while the local player is downed
///
/// Required animator parameters on the controller assigned to this prefab
/// (create with BCE/Setup/5a ▶ Create Player AnimController):
///   float   Speed
///   bool    IsInCombat
///   bool    isBackwards   (set by PlayerMovement)
///   trigger GetHit
///   trigger Death
///   bool    IsDead
///   trigger CastDamage    (set by CastAnimator)
///   trigger CastHeal
///   trigger CastSupport
///   trigger dodge         (set by PlayerMovement)
///
/// Attach to: the root GameObject of each class prefab (same as PlayerMovement).
/// </summary>
[RequireComponent(typeof(Health))]
public class PlayerAnimator : MonoBehaviour
{
    [Header("Combat Detection")]
    [Tooltip("Radius used to check for live enemies — sets IsInCombat pose")]
    public float combatCheckRadius   = 10f;
    [Tooltip("How often (seconds) to check for nearby enemies")]
    public float combatCheckInterval = 0.5f;

    [Header("Speed Normalisation")]
    [Tooltip("Your PlayerMovement.moveSpeed value — used to normalise Speed to 0-1")]
    public float baseMoveSpeed = 9f;
    [Tooltip("Your PlayerMovement.sprintSpeed value — Speed will reach 1.5 at sprint")]
    public float baseSprintSpeed = 9f;

    // ── Private ───────────────────────────────────────────────────────────────────

    private Animator  _anim;
    private Rigidbody _rb;
    private Health    _health;
    private float     _combatTimer;
    private bool      _dead;
    private readonly Collider[] _combatHits = new Collider[64];

    private System.Collections.Generic.HashSet<string> _params =
        new System.Collections.Generic.HashSet<string>();

    // ─────────────────────────────────────────────────────────────────────────────

    void Start()
    {
        // Only run on the LOCAL player — remote clones should not re-subscribe events
        var netId = GetComponent<NetworkIdentity>();
        if (netId != null && !netId.isLocalPlayer)
        {
            enabled = false;
            return;
        }

        _anim   = GetComponentInChildren<Animator>();
        _rb     = GetComponent<Rigidbody>();
        _health = GetComponent<Health>();

        // Cache which parameters this controller actually exposes
        if (_anim != null && _anim.runtimeAnimatorController != null)
            foreach (var p in _anim.parameters)
                _params.Add(p.name);

        if (_health != null)
        {
            _health.onDamageTaken.AddListener(OnDamageTaken);
            _health.onDeath.AddListener(OnDeath);
            _health.onDownedChanged.AddListener(OnDownedChanged);
            SetDead(_health.IsDowned);
        }
    }

    void OnDestroy()
    {
        if (_health != null)
        {
            _health.onDamageTaken.RemoveListener(OnDamageTaken);
            _health.onDeath.RemoveListener(OnDeath);
            _health.onDownedChanged.RemoveListener(OnDownedChanged);
        }
    }

    void Update()
    {
        if (_dead || _anim == null) return;

        // ── Speed float for movement blend tree ──────────────────────────────────
        if (_rb != null)
        {
            Vector3 flatVel = _rb.linearVelocity;
            flatVel.y = 0f;
            // Normalise: baseMoveSpeed → 1.0, baseSprintSpeed → 1.5
            float speed = NormalizeLocomotionSpeed(flatVel.magnitude);
            SetFloat("Speed", speed);
        }

        // ── IsInCombat — periodic enemy proximity check ──────────────────────────
        _combatTimer -= Time.deltaTime;
        if (_combatTimer <= 0f)
        {
            _combatTimer = combatCheckInterval;

            bool nearEnemy = false;
            Collider[] cols = _combatHits;
            int hitCount = ZonePhysics.OverlapSphereNonAlloc(
                gameObject, transform.position, combatCheckRadius, _combatHits);
            if (hitCount == _combatHits.Length)
            {
                cols = ZonePhysics.OverlapSphere(gameObject, transform.position, combatCheckRadius);
                hitCount = cols.Length;
            }

            for (int i = 0; i < hitCount; i++)
            {
                Collider c = cols[i];
                if (c.CompareTag("Enemy"))
                {
                    // Only count live enemies
                    var h = c.GetComponent<Health>();
                    if (h == null || h.IsAlive) { nearEnemy = true; break; }
                }
            }

            SetBool("IsInCombat", nearEnemy);
        }
    }

    // ── Health event handlers ─────────────────────────────────────────────────────

    float NormalizeLocomotionSpeed(float worldSpeed)
    {
        float runSpeed = Mathf.Max(0.01f, baseMoveSpeed);
        float authoredSprintSpeed = Mathf.Max(runSpeed, baseSprintSpeed);

        if (worldSpeed <= runSpeed || Mathf.Approximately(authoredSprintSpeed, runSpeed))
            return Mathf.Clamp(worldSpeed / runSpeed, 0f, 1.5f);

        return Mathf.Lerp(
            1f,
            1.5f,
            Mathf.InverseLerp(runSpeed, authoredSprintSpeed, worldSpeed));
    }

    void OnDamageTaken(float _)
    {
        SetTrigger("GetHit");
    }

    void OnDeath()
    {
        SetDead(true);
    }

    void OnDownedChanged(bool isDowned)
    {
        SetDead(isDowned);
    }

    void SetDead(bool isDead)
    {
        bool wasDead = _dead;
        _dead = isDead;
        SetBool("IsDead", isDead);

        if (isDead && !wasDead)
        {
            SetTrigger("Death");
            CombatAudio.Instance?.PlayDeath();
        }
        else if (!isDead && _anim != null && _params.Contains("Death"))
        {
            _anim.ResetTrigger("Death");
        }
    }

    // ── Safe animator helpers (no-op if param doesn't exist) ─────────────────────

    void SetFloat(string p, float v)
    {
        if (_anim != null && _params.Contains(p)) _anim.SetFloat(p, v);
    }

    void SetBool(string p, bool v)
    {
        if (_anim != null && _params.Contains(p)) _anim.SetBool(p, v);
    }

    void SetTrigger(string p)
    {
        if (_anim != null && _params.Contains(p)) _anim.SetTrigger(p);
    }
}
#endif
