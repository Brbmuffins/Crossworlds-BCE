#if UNITY_EDITOR || !UNITY_SERVER
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tilts only an enemy's visual hierarchy to the ground normal. The networked
/// NavMesh root remains upright so navigation, colliders and Mirror stay stable.
/// </summary>
[DisallowMultipleComponent]
public sealed class EnemySlopeVisualAligner : MonoBehaviour
{
    [SerializeField, Min(0f)] float groundProbeHeight = 1.5f;
    [SerializeField, Min(0.1f)] float groundProbeDistance = 5f;
    [SerializeField, Range(0f, 60f)] float maximumTilt = 35f;
    [SerializeField, Min(0f)] float alignmentSpeed = 8f;

    Health _health;
    Transform _visualRoot;
    Quaternion _baseLocalRotation;

    void Awake()
    {
        _health = GetComponent<Health>();
        _visualRoot = FindVisualRoot();
        if (_visualRoot != null)
            _baseLocalRotation = _visualRoot.localRotation;
    }

    void LateUpdate()
    {
        if (_visualRoot == null) return;

        Quaternion target = _baseLocalRotation;
        if ((_health == null || _health.IsAlive) && TryGetGroundNormal(out Vector3 normal))
        {
            float angle = Vector3.Angle(Vector3.up, normal);
            if (angle > maximumTilt && angle > 0.001f)
                normal = Vector3.Slerp(Vector3.up, normal, maximumTilt / angle).normalized;

            Quaternion uprightWorld = transform.rotation * _baseLocalRotation;
            Quaternion tiltedWorld = Quaternion.FromToRotation(Vector3.up, normal) * uprightWorld;
            target = Quaternion.Inverse(transform.rotation) * tiltedWorld;
        }

        float blend = alignmentSpeed <= 0f
            ? 1f
            : 1f - Mathf.Exp(-alignmentSpeed * Time.deltaTime);
        _visualRoot.localRotation = Quaternion.Slerp(
            _visualRoot.localRotation, target, blend);
    }

    bool TryGetGroundNormal(out Vector3 normal)
    {
        normal = Vector3.up;
        Vector3 origin = transform.position + Vector3.up * groundProbeHeight;
        RaycastHit[] hits = ZonePhysics.RaycastAll(
            gameObject, origin, Vector3.down,
            groundProbeHeight + groundProbeDistance,
            Physics.AllLayers, QueryTriggerInteraction.Ignore);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider == null || hit.collider.transform.IsChildOf(transform)) continue;
            if (hit.normal.y <= 0.25f) continue;
            normal = hit.normal.normalized;
            return true;
        }
        return false;
    }

    Transform FindVisualRoot()
    {
        Animator animator = GetComponentInChildren<Animator>(true);
        if (animator != null && animator.transform != transform)
            return animator.transform;

        var counts = new Dictionary<Transform, int>();
        foreach (Renderer renderer in GetComponentsInChildren<Renderer>(true))
        {
            if (renderer == null) continue;
            Transform candidate = renderer.transform;
            while (candidate.parent != null && candidate.parent != transform)
                candidate = candidate.parent;
            if (candidate == transform || candidate.parent != transform) continue;
            counts[candidate] = counts.TryGetValue(candidate, out int count) ? count + 1 : 1;
        }

        Transform best = null;
        int bestCount = 0;
        foreach (var pair in counts)
        {
            if (pair.Value <= bestCount) continue;
            best = pair.Key;
            bestCount = pair.Value;
        }
        return best;
    }
}
#endif
