using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class SpanBridgeCollisionGuard : MonoBehaviour
{
    private const float Clearance = -0.60f;
    private const float HeightPrecision = 0.01f;
    private const float SideMargin = 0.10f;
    private const float EndMargin = 0.50f;

    private void Awake()
    {
        BoxCollider continuousSurface = null;
        foreach (var collider in GetComponentsInChildren<BoxCollider>(true))
        {
            if (collider.gameObject.name == "SpanBridgeCollisionSurface")
            {
                continuousSurface = collider;
                break;
            }
        }

        if (continuousSurface == null)
        {
            Debug.LogError("Span bridge collision surface is missing.", this);
            return;
        }

        foreach (var collider in GetComponentsInChildren<Collider>(true))
            collider.enabled = collider == continuousSurface;

        var deckHeight = float.NegativeInfinity;
        var hasRoadBounds = false;
        var roadMin = new Vector3(float.PositiveInfinity, 0f, float.PositiveInfinity);
        var roadMax = new Vector3(float.NegativeInfinity, 0f, float.NegativeInfinity);

        foreach (var filter in GetComponentsInChildren<MeshFilter>(true))
        {
            if (!filter.gameObject.name.Contains("brige_road") || filter.sharedMesh == null)
                continue;

            if (!filter.sharedMesh.isReadable)
            {
                Debug.LogWarning(
                    $"Span bridge road mesh '{filter.sharedMesh.name}' is not readable; leaving existing bridge collision bounds unchanged.",
                    filter);
                continue;
            }

            var bounds = filter.sharedMesh.bounds;
            var minimumDeckHeight = bounds.min.y + (bounds.size.y * 0.2f);
            var samples = new Dictionary<int, int>();
            foreach (var vertex in filter.sharedMesh.vertices)
            {
                if (vertex.y < minimumDeckHeight)
                    continue;

                var key = Mathf.RoundToInt(filter.transform.TransformPoint(vertex).y / HeightPrecision);
                samples.TryGetValue(key, out var count);
                samples[key] = count + 1;
            }

            var bestCount = 0;
            foreach (var sample in samples)
            {
                if (sample.Value > bestCount)
                {
                    bestCount = sample.Value;
                    deckHeight = Mathf.Max(deckHeight, sample.Key * HeightPrecision);
                }
            }

            for (var x = 0; x <= 1; x++)
            for (var y = 0; y <= 1; y++)
            for (var z = 0; z <= 1; z++)
            {
                var point = new Vector3(
                    x == 0 ? bounds.min.x : bounds.max.x,
                    y == 0 ? bounds.min.y : bounds.max.y,
                    z == 0 ? bounds.min.z : bounds.max.z);
                point = continuousSurface.transform.InverseTransformPoint(filter.transform.TransformPoint(point));
                roadMin = Vector3.Min(roadMin, point);
                roadMax = Vector3.Max(roadMax, point);
                hasRoadBounds = true;
            }
        }

        if (!float.IsNegativeInfinity(deckHeight))
            continuousSurface.transform.position += Vector3.up * (deckHeight + Clearance - continuousSurface.bounds.max.y);

        if (hasRoadBounds)
        {
            continuousSurface.center = new Vector3(
                (roadMin.x + roadMax.x) * 0.5f,
                continuousSurface.center.y,
                (roadMin.z + roadMax.z) * 0.5f);
            continuousSurface.size = new Vector3(
                roadMax.x - roadMin.x + SideMargin,
                continuousSurface.size.y,
                roadMax.z - roadMin.z + EndMargin);
        }
    }
}
