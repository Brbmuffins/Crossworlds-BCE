using UnityEngine;

/// <summary>
/// Keeps a gravity-driven particle stream's travel distance proportional when
/// its transform is uniformly scaled. Falling distance is proportional to
/// lifetime squared, so lifetime grows by the square root of the scale.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(ParticleSystem))]
public sealed class ScaleAwareParticleLifetime : MonoBehaviour
{
    [Min(0.01f)] public float referenceScale = 1f;
    [Min(0.01f)] public float maximumMultiplier = 12f;

    ParticleSystem _particles;
    ParticleSystem.MinMaxCurve _authoredLifetime;
    Vector3 _lastLossyScale;

    void Awake()
    {
        _particles = GetComponent<ParticleSystem>();
        _authoredLifetime = _particles.main.startLifetime;
        ApplyForCurrentScale();
    }

    void OnEnable()
    {
        if (_particles == null)
        {
            _particles = GetComponent<ParticleSystem>();
            _authoredLifetime = _particles.main.startLifetime;
        }

        ApplyForCurrentScale();
    }

    void LateUpdate()
    {
        if (transform.lossyScale != _lastLossyScale)
            ApplyForCurrentScale();
    }

    void ApplyForCurrentScale()
    {
        _lastLossyScale = transform.lossyScale;
        float largestAxis = Mathf.Max(
            Mathf.Abs(_lastLossyScale.x),
            Mathf.Abs(_lastLossyScale.y),
            Mathf.Abs(_lastLossyScale.z));
        float scaleRatio = Mathf.Max(1f, largestAxis / referenceScale);
        float multiplier = Mathf.Min(Mathf.Sqrt(scaleRatio), maximumMultiplier);

        var adjusted = _authoredLifetime;
        adjusted.constantMin = _authoredLifetime.constantMin * multiplier;
        adjusted.constantMax = _authoredLifetime.constantMax * multiplier;

        var main = _particles.main;
        main.startLifetime = adjusted;
    }
}
