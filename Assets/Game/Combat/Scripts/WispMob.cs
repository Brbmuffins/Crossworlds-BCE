using UnityEngine;

[DisallowMultipleComponent]
public class WispMob : MonoBehaviour
{
    [Header("Wander")]
    public float wanderRadius = 8f;
    public float minHeight = 1.2f;
    public float maxHeight = 4.2f;
    public float minSpeed = 0.8f;
    public float maxSpeed = 1.8f;
    public float targetReachDistance = 0.35f;
    public Vector2 retargetDelay = new Vector2(1.5f, 4f);

    [Header("Motion")]
    public float driftSmoothTime = 0.65f;
    public float bobAmplitude = 0.18f;
    public float bobFrequency = 1.7f;

    [Header("Light")]
    public Color glowColor = new Color(0.48f, 0.9f, 1f, 1f);
    public float pulseAmount = 0.35f;
    public float pulseSpeed = 2.4f;

    private Vector3 _home;
    private Vector3 _target;
    private Vector3 _velocity;
    private float _speed;
    private float _nextRetargetAt;
    private float _baseLightIntensity;
    private Light _light;
    private TrailRenderer _trail;

    void Awake()
    {
        _light = GetComponentInChildren<Light>();
        _trail = GetComponent<TrailRenderer>();
        _baseLightIntensity = _light != null ? _light.intensity : 1f;
    }

    void Start()
    {
        _home = transform.position;
        PickNewTarget();
    }

    void Update()
    {
        PulseLight();

        if (Time.time >= _nextRetargetAt || Vector3.Distance(transform.position, _target) <= targetReachDistance)
            PickNewTarget();

        Vector3 desired = _target;
        desired.y += Mathf.Sin(Time.time * bobFrequency) * bobAmplitude;
        transform.position = Vector3.SmoothDamp(transform.position, desired, ref _velocity, driftSmoothTime, _speed);
    }

    void PickNewTarget()
    {
        Vector2 circle = Random.insideUnitCircle * wanderRadius;
        _target = _home + new Vector3(circle.x, Random.Range(minHeight, maxHeight), circle.y);
        _speed = Random.Range(minSpeed, maxSpeed);
        _nextRetargetAt = Time.time + Random.Range(retargetDelay.x, retargetDelay.y);
    }

    void PulseLight()
    {
        if (_light == null)
            return;

        float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
        _light.color = glowColor;
        _light.intensity = Mathf.Max(0.05f, _baseLightIntensity * pulse);

        if (_trail != null)
        {
            _trail.startColor = glowColor;
            Color tail = glowColor;
            tail.a = 0f;
            _trail.endColor = tail;
        }
    }

    void OnDrawGizmosSelected()
    {
        Vector3 center = Application.isPlaying ? _home : transform.position;
        Gizmos.color = new Color(glowColor.r, glowColor.g, glowColor.b, 0.25f);
        Gizmos.DrawWireSphere(center, wanderRadius);
    }
}
