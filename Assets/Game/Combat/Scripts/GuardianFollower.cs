using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(TurretController))]
public class GuardianFollower : MonoBehaviour
{
    const string DefaultOwnerHomeChildName = "GuardianLocation";

    [Header("Follow Target")]
    [Tooltip("Optional. If assigned, the guardian moves to this exact point. Useful for scene-placed guardians.")]
    public Transform homeLocation;
    [Tooltip("For spawned guardian prefabs, this child empty on the owner/player becomes the guardian's home slot.")]
    public string ownerHomeChildName = DefaultOwnerHomeChildName;
    [Tooltip("Optional. If empty, the guardian follows the turret owner assigned by AbilityCaster.")]
    public Transform followTarget;
    [Tooltip("Use the TurretController owner as the follow target when Follow Target is empty.")]
    public bool followOwner = true;
    [Tooltip("Used when Home Location is empty.")]
    public Vector3 followOffset = new Vector3(-1.6f, 0f, -1.4f);
    [Tooltip("When enabled, Follow Offset rotates with the target so the guardian keeps the same relative side/back slot.")]
    public bool offsetUsesTargetRotation = true;

    [Header("Movement")]
    public bool followEnabled = true;
    [Min(0f)] public float stopDistance = 0.2f;
    [Min(0f)] public float moveSpeed = 4.5f;
    [Min(0f)] public float catchUpDistance = 6f;
    [Min(0f)] public float catchUpSpeed = 9f;
    [Tooltip("If the guardian gets farther than this, it snaps back to its follow slot. Set to 0 to disable.")]
    [Min(0f)] public float teleportDistance = 24f;

    [Header("Physics")]
    [Tooltip("Recommended for floating guardians so physics does not push them away from their follow slot.")]
    public bool keepRigidbodyKinematic = true;
    [Tooltip("Recommended for floating guardians.")]
    public bool disableGravity = true;
    [Tooltip("Prevents the guardian from colliding with and being pushed by its owner/player.")]
    public bool ignoreOwnerCollisions = true;

    [Header("Rotation")]
    [Tooltip("Usually off for guardians because TurretController rotates the turret toward enemies.")]
    public bool rotateTowardMovement = false;
    [Min(0f)] public float rotationSpeed = 12f;

    [Header("Grounding")]
    [Tooltip("Turn on for ground guardians. Leave off for hovering/floating guardians.")]
    public bool snapToGround = false;
    [Min(0f)] public float groundRayHeight = 4f;
    [Min(0f)] public float groundRayDistance = 12f;
    public float groundOffset = 0.05f;
    public LayerMask groundMask = ~0;

    TurretController _turret;
    Rigidbody _rb;
    readonly RaycastHit[] _groundHits = new RaycastHit[64];
    float _nextOwnerResolveTime;
    float _nextTeleportTime;

    void Awake()
    {
        _turret = GetComponent<TurretController>();
        _rb = GetComponent<Rigidbody>();
        ConfigureRigidbody();
    }

    void OnEnable()
    {
        ResolveOwnerTarget(true);
    }

    void Update()
    {
        if (!DeployableNet.IsAuthority || !followEnabled)
            return;

        if (homeLocation == null && followTarget == null && followOwner && Time.time >= _nextOwnerResolveTime)
            ResolveOwnerTarget(false);
    }

    void FixedUpdate()
    {
        if (!DeployableNet.IsAuthority || !followEnabled)
            return;

        if (homeLocation == null && followTarget == null && followOwner && Time.time >= _nextOwnerResolveTime)
            ResolveOwnerTarget(false);

        if (!TryGetDestination(out Vector3 destination))
            return;

        MoveToward(destination, Time.fixedDeltaTime);
    }

    public void SetHomeLocation(Transform newHomeLocation)
    {
        homeLocation = newHomeLocation;
    }

    public void SetFollowTarget(Transform newFollowTarget)
    {
        followTarget = newFollowTarget;
    }

    public void BindToOwner(Transform ownerTransform)
    {
        if (ownerTransform == null)
            return;

        followTarget = ownerTransform;

        string childName = string.IsNullOrEmpty(ownerHomeChildName)
            ? DefaultOwnerHomeChildName
            : ownerHomeChildName;

        Transform ownerHome = FindChildRecursive(ownerTransform, childName);
        if (ownerHome != null)
            homeLocation = ownerHome;

        IgnoreOwnerCollisions(ownerTransform);
    }

    bool TryGetDestination(out Vector3 destination)
    {
        if (homeLocation != null)
        {
            destination = homeLocation.position;
        }
        else if (followTarget != null)
        {
            destination = offsetUsesTargetRotation
                ? followTarget.TransformPoint(followOffset)
                : followTarget.position + followOffset;
        }
        else
        {
            destination = transform.position;
            return false;
        }

        if (snapToGround)
            destination = ProjectToGround(destination);

        return true;
    }

    void MoveToward(Vector3 destination, float deltaTime)
    {
        Vector3 current = transform.position;
        Vector3 toDestination = destination - current;
        float distance = toDestination.magnitude;

        if (teleportDistance > 0f && distance > teleportDistance && Time.time >= _nextTeleportTime)
        {
            TeleportTo(destination);
            _nextTeleportTime = Time.time + 0.5f;
            return;
        }

        if (distance <= stopDistance)
            return;

        float speed = distance >= catchUpDistance ? catchUpSpeed : moveSpeed;
        Vector3 next = Vector3.MoveTowards(current, destination, speed * deltaTime);
        SetPosition(next);

        if (rotateTowardMovement)
            RotateToward(next - current, deltaTime);
    }

    void SetPosition(Vector3 position)
    {
        if (_rb != null)
            _rb.MovePosition(position);
        else
            transform.position = position;
    }

    void TeleportTo(Vector3 position)
    {
        if (_rb != null)
        {
            _rb.position = position;
            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
        }

        transform.position = position;
    }

    void ConfigureRigidbody()
    {
        if (_rb == null)
            return;

        if (keepRigidbodyKinematic)
            _rb.isKinematic = true;

        if (disableGravity)
            _rb.useGravity = false;
    }

    void RotateToward(Vector3 direction, float deltaTime)
    {
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.0001f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * deltaTime);
    }

    Vector3 ProjectToGround(Vector3 point)
    {
        Vector3 origin = point + Vector3.up * groundRayHeight;
        RaycastHit[] hits = _groundHits;
        int hitCount = ZonePhysics.RaycastNonAlloc(
            gameObject,
            origin,
            Vector3.down,
            _groundHits,
            groundRayHeight + groundRayDistance,
            groundMask,
            QueryTriggerInteraction.Ignore);

        if (hitCount == 0)
            return point;

        if (hitCount == _groundHits.Length)
        {
            hits = ZonePhysics.RaycastAll(
                gameObject, origin, Vector3.down, groundRayHeight + groundRayDistance,
                groundMask, QueryTriggerInteraction.Ignore);
            hitCount = hits.Length;
        }

        bool found = false;
        RaycastHit nearest = default;
        for (int i = 0; i < hitCount; i++)
        {
            RaycastHit hit = hits[i];
            if (hit.collider == null)
                continue;

            Transform hitTransform = hit.collider.transform;
            if (hitTransform == transform || hitTransform.IsChildOf(transform))
                continue;

            if (!found || hit.distance < nearest.distance)
            {
                found = true;
                nearest = hit;
            }
        }

        return found ? nearest.point + Vector3.up * groundOffset : point;
    }

    void ResolveOwnerTarget(bool force)
    {
        if (!followOwner || (!force && Time.time < _nextOwnerResolveTime))
            return;

        _nextOwnerResolveTime = Time.time + 1f;
        if (_turret == null || _turret.owner == null)
            return;

        BindToOwner(_turret.owner.transform);
    }

    void IgnoreOwnerCollisions(Transform ownerTransform)
    {
        if (!ignoreOwnerCollisions || ownerTransform == null)
            return;

        Collider[] guardianColliders = GetComponentsInChildren<Collider>(true);
        Collider[] ownerColliders = ownerTransform.GetComponentsInChildren<Collider>(true);

        foreach (Collider guardianCollider in guardianColliders)
        foreach (Collider ownerCollider in ownerColliders)
        {
            if (guardianCollider == null || ownerCollider == null)
                continue;

            Physics.IgnoreCollision(guardianCollider, ownerCollider, true);
        }
    }

    static Transform FindChildRecursive(Transform root, string childName)
    {
        if (root == null || string.IsNullOrEmpty(childName))
            return null;

        foreach (Transform child in root)
        {
            if (child.name == childName)
                return child;

            Transform nested = FindChildRecursive(child, childName);
            if (nested != null)
                return nested;
        }

        return null;
    }

    void OnDrawGizmosSelected()
    {
        Transform anchor = homeLocation != null ? homeLocation : followTarget;
        if (anchor == null)
            return;

        Vector3 destination = homeLocation != null
            ? homeLocation.position
            : (offsetUsesTargetRotation ? anchor.TransformPoint(followOffset) : anchor.position + followOffset);

        Gizmos.color = new Color(0.25f, 0.75f, 1f, 0.85f);
        Gizmos.DrawWireSphere(destination, Mathf.Max(0.1f, stopDistance));
        Gizmos.DrawLine(transform.position, destination);
    }
}
