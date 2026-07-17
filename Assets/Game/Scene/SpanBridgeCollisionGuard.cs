using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class SpanBridgeCollisionGuard : MonoBehaviour
{
    private const float Clearance = -0.60f;
    private const float HeightPrecision = 0.01f;

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
        foreach (var filter in GetComponentsInChildren<MeshFilter>(true))
        {
            if (!filter.gameObject.name.Contains("brige_road") || filter.sharedMesh == null)
                continue;

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
        }

        if (!float.IsNegativeInfinity(deckHeight))
        {
            continuousSurface.transform.position += Vector3.up * (deckHeight + Clearance - continuousSurface.bounds.max.y);
        }
    }

}