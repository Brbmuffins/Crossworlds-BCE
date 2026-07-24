using Mirror;
using UnityEngine;

[DisallowMultipleComponent]
public class PetFollower : MonoBehaviour
{
    [Header("Follow Target")]
    [Tooltip("Optional. If assigned, the pet moves to this exact point. Parent an empty object to the player and drag it here for a pet slot.")]
    public Transform homeLocation;
    [Tooltip("Optional fallback. If Home Location is empty, the pet follows this target using Follow Offset.")]
    public Transform followTarget;
    [Tooltip("If no target is assigned, find the local player automatically.")]
    public bool autoFindLocalPlayer = true;
    [Tooltip("Used only when Home Location is empty.")]
    public Vector3 followOffset = new Vector3(-1.5f, 0f, -1.5f);

    [Header("Movement")]
    public bool followEnabled = true;
    [Min(0f)] public float stopDistance = 0.15f;
    [Min(0f)] public float moveSpeed = 4f;
    [Min(0f)] public float catchUpDistance = 6f;
    [Min(0f)] public float catchUpSpeed = 8f;
    [Tooltip("If the pet gets farther than this, it snaps to its destination. Set to 0 to disable.")]
    [Min(0f)] public float teleportDistance = 25f;

    [Header("Rotation")]
    public bool rotateTowardMovement = true;
    [Min(0f)] public float rotationSpeed = 12f;

    [Header("Grounding")]
    [Tooltip("Turn this on for ground pets. Leave it off for floating pets or home points with exact height.")]
    public bool snapToGround = false;
    [Min(0f)] public float groundRayHeight = 4f;
    [Min(0f)] public float groundRayDistance = 12f;
    public float groundOffset = 0.05f;
    public LayerMask groundMask = ~0;

    Rigidbody _rb;
    float _nextTargetSearchTime;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    void OnEnable()
    {
        TryFindFollowTarget(true);
    }

    void Update()
    {
        if (!followEnabled)
            return;

        if (homeLocation == null && followTarget == null && autoFindLocalPlayer && Time.time >= _nextTargetSearchTime)
            TryFindFollowTarget(false);

        if (!TryGetDestination(out Vector3 destination))
            return;

        MoveToward(destination, Time.deltaTime);
    }

    public void SetHomeLocation(Transform newHomeLocation)
    {
        homeLocation = newHomeLocation;
    }

    public void SetFollowTarget(Transform newFollowTarget)
    {
        followTarget = newFollowTarget;
    }

    public void StartFollowing()
    {
        followEnabled = true;
        TryFindFollowTarget(true);
    }

    public void StopFollowing()
    {
        followEnabled = false;
    }

    bool TryGetDestination(out Vector3 destination)
    {
        if (homeLocation != null)
        {
            destination = homeLocation.position;
        }
        else if (followTarget != null)
        {
            destination = followTarget.TransformPoint(followOffset);
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

        if (teleportDistance > 0f && distance > teleportDistance)
        {
            SetPosition(destination);
            return;
        }

        if (distance <= stopDistance)
        {
            if (rotateTowardMovement && homeLocation != null)
                RotateToward(homeLocation.forward, deltaTime);
            return;
        }

        float speed = distance >= catchUpDistance ? catchUpSpeed : moveSpeed;
        Vector3 next = Vector3.MoveTowards(current, destination, speed * deltaTime);
        SetPosition(next);

        if (rotateTowardMovement)
            RotateToward(next - current, deltaTime);
    }

    void SetPosition(Vector3 position)
    {
        if (_rb != null && !_rb.isKinematic)
            _rb.MovePosition(position);
        else
            transform.position = position;
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
        RaycastHit[] hits = ZonePhysics.RaycastAll(
            gameObject,
            origin,
            Vector3.down,
            groundRayHeight + groundRayDistance,
            groundMask,
            QueryTriggerInteraction.Ignore);

        if (hits.Length == 0)
            return point;

        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider == null)
                continue;

            Transform hitTransform = hit.collider.transform;
            if (hitTransform == transform || hitTransform.IsChildOf(transform))
                continue;

            return hit.point + Vector3.up * groundOffset;
        }

        return point;
    }

    void TryFindFollowTarget(bool force)
    {
        if (!autoFindLocalPlayer || (!force && Time.time < _nextTargetSearchTime))
            return;

        _nextTargetSearchTime = Time.time + 1f;

        NetworkIdentity[] identities = FindObjectsByType<NetworkIdentity>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        foreach (NetworkIdentity identity in identities)
        {
            if (identity != null && identity.isLocalPlayer)
            {
                followTarget = identity.transform;
                return;
            }
        }

        GameObject taggedPlayer = GameObject.FindGameObjectWithTag("Player");
        if (taggedPlayer != null)
            followTarget = taggedPlayer.transform;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.25f, 0.75f, 1f, 0.85f);
        Transform anchor = homeLocation != null ? homeLocation : followTarget;
        if (anchor == null)
            return;

        Vector3 destination = homeLocation != null ? homeLocation.position : anchor.TransformPoint(followOffset);
        Gizmos.DrawWireSphere(destination, Mathf.Max(0.1f, stopDistance));
        Gizmos.DrawLine(transform.position, destination);
    }
}
