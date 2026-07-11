using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Animator))]
public class CyclopsAnimationDriver : MonoBehaviour
{
    public string speedParameter = "Speed";
    public string attackTrigger = "Attack";
    public string dieTrigger = "Die";
    public string isDeadParameter = "IsDead";

    [Tooltip("Animator speed value at/above this is considered running.")]
    public float runSpeedThreshold = 0.15f;

    Animator _animator;
    Health _health;
    EnemyController _enemyController;
    Vector3 _lastPosition;
    bool _deathTriggered;

    void Awake()
    {
        _animator = GetComponent<Animator>();
        _health = GetComponentInParent<Health>();
        _enemyController = GetComponentInParent<EnemyController>();
        _lastPosition = transform.position;

        if (_health != null)
            _health.onDeath.AddListener(PlayDeath);
    }

    void Update()
    {
        if (_animator == null)
            return;

        if (_deathTriggered)
        {
            _animator.SetFloat(speedParameter, 0f);
            return;
        }

        float actualSpeed = (transform.position - _lastPosition).magnitude / Mathf.Max(Time.deltaTime, 0.0001f);
        _lastPosition = transform.position;

        float normalizedSpeed = actualSpeed > runSpeedThreshold ? 1f : 0f;
        _animator.SetFloat(speedParameter, normalizedSpeed, 0.12f, Time.deltaTime);

        if (_health != null && !_health.IsAlive && !_deathTriggered)
            PlayDeath();
    }

    public void PlayAttack()
    {
        if (_animator != null && (_health == null || _health.IsAlive))
            _animator.SetTrigger(attackTrigger);
    }

    public void PlayDeath()
    {
        if (_animator == null || _deathTriggered)
            return;

        _deathTriggered = true;
        _animator.SetFloat(speedParameter, 0f);
        _animator.SetBool(isDeadParameter, true);
        _animator.SetTrigger(dieTrigger);

    }

    void OnEnemyRespawned()
    {
        _deathTriggered = false;
        _lastPosition = transform.position;

        if (_animator == null)
            return;

        _animator.ResetTrigger(dieTrigger);
        _animator.SetBool(isDeadParameter, false);
        _animator.SetFloat(speedParameter, 0f);
    }
}
