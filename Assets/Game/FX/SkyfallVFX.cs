using UnityEngine;

[DisallowMultipleComponent]
[DefaultExecutionOrder(-1000)]
public sealed class SkyfallVFX : MonoBehaviour
{
    [Header("Skyfall Travel")]
    [SerializeField, Min(0f)]
    [Tooltip("How far above the prefab's spawn position the effect begins.")]
    float skyHeight = 12f;

    [SerializeField, Min(0.01f)]
    [Tooltip("Seconds taken to travel from the sky to the spawn position.")]
    float travelDuration = 0.7f;

    [SerializeField, Range(1f, 4f)]
    [Tooltip("Higher values make the effect accelerate more strongly as it falls. One is constant speed.")]
    float acceleration = 2f;

    [SerializeField]
    [Tooltip("Restart child particle systems after moving the effect to its sky position, preventing a one-frame flash on the ground.")]
    bool restartParticlesOnEnable = true;

    Vector3 impactLocalPosition;
    Vector3 startLocalPosition;
    float elapsed;
    bool hasLanded;

    public float TravelDuration => Mathf.Max(0.01f, travelDuration);

    void OnEnable()
    {
        impactLocalPosition = transform.localPosition;
        startLocalPosition = impactLocalPosition + Vector3.up * Mathf.Max(0f, skyHeight);
        transform.localPosition = startLocalPosition;
        elapsed = 0f;
        hasLanded = skyHeight <= 0f;

        if (hasLanded)
            transform.localPosition = impactLocalPosition;

        if (restartParticlesOnEnable)
            RestartParticles();
    }

    void Update()
    {
        if (hasLanded)
            return;

        elapsed += Time.deltaTime;
        float normalizedTime = Mathf.Clamp01(elapsed / TravelDuration);
        float easedTime = Mathf.Pow(normalizedTime, Mathf.Max(1f, acceleration));
        transform.localPosition = Vector3.LerpUnclamped(
            startLocalPosition,
            impactLocalPosition,
            easedTime);

        if (normalizedTime < 1f)
            return;

        transform.localPosition = impactLocalPosition;
        hasLanded = true;
    }

    void RestartParticles()
    {
        foreach (ParticleSystem particles in GetComponentsInChildren<ParticleSystem>(true))
        {
            if (!particles.gameObject.activeInHierarchy)
                continue;

            particles.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
            particles.Play(false);
        }
    }
}
