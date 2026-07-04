using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Animator))]
public class FieldGoulAnimationDriver : MonoBehaviour
{
    public string speedParameter = "Speed";
    public string attackTrigger = "Attack";
    public string dieTrigger = "Die";
    public string isDeadParameter = "IsDead";

    [Tooltip("Animator speed value at/above this is considered running.")]
    public float runSpeedThreshold = 0.15f;

    private Animator _animator;
    private Health _health;
    private EnemyAI _enemyAI;
    private Vector3 _lastPosition;
    private bool _deathTriggered;

    void Awake()
    {
        _animator = GetComponent<Animator>();
        _health = GetComponentInParent<Health>();
        _enemyAI = GetComponentInParent<EnemyAI>();
        _lastPosition = transform.position;

        if (_health != null)
            _health.onDeath.AddListener(PlayDeath);
    }

    void Update()
    {
        if (_animator == null)
            return;

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
        _animator.SetBool(isDeadParameter, true);
        _animator.SetTrigger(dieTrigger);

        if (_enemyAI != null)
            _enemyAI.enabled = false;
    }
}
