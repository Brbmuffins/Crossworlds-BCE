using UnityEngine;

/// <summary>Author-approved area in which a zone's dynamic ore nodes may appear.</summary>
[DisallowMultipleComponent]
public sealed class OreSpawnVolume : MonoBehaviour
{
    public Vector3 size = new(80f, 30f, 80f);
    [Min(0.1f)] public float groundSearchHeight = 20f;

    public Vector3 RandomRayOrigin()
    {
        Vector3 local = new(
            Random.Range(-size.x * 0.5f, size.x * 0.5f),
            size.y * 0.5f + groundSearchHeight,
            Random.Range(-size.z * 0.5f, size.z * 0.5f));
        return transform.TransformPoint(local);
    }

    public float RayDistance => Mathf.Max(1f, size.y + groundSearchHeight * 2f);

    void OnValidate()
    {
        size.x = Mathf.Max(1f, size.x);
        size.y = Mathf.Max(1f, size.y);
        size.z = Mathf.Max(1f, size.z);
        groundSearchHeight = Mathf.Max(0.1f, groundSearchHeight);
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.85f, 0.52f, 0.12f, 0.28f);
        Matrix4x4 previous = Gizmos.matrix;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawCube(Vector3.zero, size);
        Gizmos.color = new Color(1f, 0.7f, 0.2f, 0.9f);
        Gizmos.DrawWireCube(Vector3.zero, size);
        Gizmos.matrix = previous;
    }
#endif
}
