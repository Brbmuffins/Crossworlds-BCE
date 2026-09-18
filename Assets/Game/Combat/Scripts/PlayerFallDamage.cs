using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Server-side landing damage from observed player positions, not a client damage request.</summary>
public sealed class PlayerFallDamage : MonoBehaviour
{
    // Initial tuning: normal jumps are harmless; a 22 m fall costs 12% max HP.
    public const float SafeDistance = 12f;
    public const float HealthFractionPerMetre = 0.012f;
    const float TeleportDistancePerTick = 8f;
    const float GroundProbeMargin = 0.35f;
    const float MinAirTime = 0.15f;

    readonly RaycastHit[] _groundHits = new RaycastHit[16];
    Health _health;
    Collider _body;
    NetworkIdentity _identity;
    Vector3 _previousPosition;
    float _peakY;
    float _airTime;
    bool _wasGrounded;
    bool _inWater;

    void Awake()
    {
        _health = GetComponent<Health>();
        _body = GetComponent<Collider>();
        _identity = GetComponent<NetworkIdentity>();
        ResetTracking();
    }

    public static float CalculateDamage(float fallDistance, float maxHealth) =>
        Mathf.Clamp01((fallDistance - SafeDistance) * HealthFractionPerMetre) * Mathf.Max(0f, maxHealth);

    public void ResetTracking()
    {
        _previousPosition = transform.position;
        _peakY = _previousPosition.y;
        _airTime = 0f;
        _wasGrounded = false;
    }

    void FixedUpdate()
    {
        if (!NetworkServer.active || _health == null || !_health.isPlayer) return;

        Vector3 position = transform.position;
        bool protectedMovement = _health.IsDowned || _health.currentHealth <= 0f ||
            _health.zoneTransitionProtected || _inWater || IsGmFlying();
        Vector3 step = position - _previousPosition;
        // An unexpected downward jump must not erase the fall being measured.
        bool unexpectedTeleport = step.y > TeleportDistancePerTick ||
            new Vector2(step.x, step.z).sqrMagnitude > TeleportDistancePerTick * TeleportDistancePerTick;
        if (protectedMovement || unexpectedTeleport)
        {
            ResetTracking();
            return;
        }

        bool grounded = IsOnWalkableGround();
        if (grounded)
        {
            if (!_wasGrounded && _airTime >= MinAirTime)
            {
                float damage = CalculateDamage(_peakY - position.y, _health.maxHealth);
                if (damage > 0f) _health.TakeDamage(damage);
            }
            _peakY = position.y;
            _airTime = 0f;
        }
        else
        {
            if (_wasGrounded) _peakY = Mathf.Max(_previousPosition.y, position.y);
            else _peakY = Mathf.Max(_peakY, position.y);
            _airTime += Time.fixedDeltaTime;
        }

        _wasGrounded = grounded;
        _previousPosition = position;
    }

    bool IsGmFlying()
    {
        var auth = _identity != null ? _identity.connectionToClient?.authenticationData as RodPlayerAuth : null;
        return auth != null && auth.gmActive && auth.gmFlyEnabled;
    }

    bool IsOnWalkableGround()
    {
        if (_body == null) return false;
        Bounds bounds = _body.bounds;
        Vector3 origin = new(bounds.center.x, bounds.min.y + GroundProbeMargin, bounds.center.z);
        PhysicsScene physicsScene = gameObject.scene.GetPhysicsScene();
        if (!physicsScene.IsValid()) return false;
        int count = physicsScene.Raycast(origin, Vector3.down, _groundHits,
            GroundProbeMargin * 2f, ~0, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < count; i++)
        {
            Collider hit = _groundHits[i].collider;
            if (hit == null || hit.transform.IsChildOf(transform)) continue;
            if (_groundHits[i].normal.y >= 0.45f) return true;
        }
        return false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (NetworkServer.active && other.CompareTag("Water")) { _inWater = true; ResetTracking(); }
    }

    void OnTriggerExit(Collider other)
    {
        if (NetworkServer.active && other.CompareTag("Water")) { _inWater = false; ResetTracking(); }
    }
}
