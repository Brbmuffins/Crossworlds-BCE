using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Animator))]
public class OgreAnimationDriver : MonoBehaviour
{
    public string speedParameter = "Speed";
    public string attackTrigger = "Attack";
    public string attack2Trigger = "Attack2";
    public string attack3Trigger = "Attack3";
    public string stunnedTrigger = "Stunned";
    public string deathTrigger = "Death";
    public string legacyDieTrigger = "Die";
    public string isDeadParameter = "IsDead";

    [Tooltip("World-space speed above this value drives the walking animation.")]
    public float walkSpeedThreshold = 0.05f;

    [Tooltip("World-space speed above this value drives the running animation.")]
    public float runSpeedThreshold = 1.2f;

    [Tooltip("Rotate through Swing1, Swing2, and Swing3 when PlayAttack is broadcast.")]
    public bool cycleAttackVariants = true;

    [Tooltip("Keep the ogre root fixed in place while the death animation plays.")]
    public bool pinRootOnDeath = true;

    Animator _animator;
    Health _health;
    StatusEffectManager _status;
    Rigidbody _rigidbody;
    Vector3 _lastPosition;
    Vector3 _deathPosition;
    bool _deathTriggered;
    bool _stunnedTriggered;
    int _attackIndex;

    bool _hadRigidbody;
    bool _originalUseGravity;
    bool _originalIsKinematic;
    RigidbodyConstraints _originalConstraints;

    bool _hasSpeed;
    bool _hasAttack;
    bool _hasAttack2;
    bool _hasAttack3;
    bool _hasStunned;
    bool _hasDeath;
    bool _hasLegacyDie;
    bool _hasIsDead;

    void Awake()
    {
        _animator = GetComponent<Animator>();
        _health = GetComponentInParent<Health>();
        _status = GetComponentInParent<StatusEffectManager>();
        _rigidbody = GetComponentInParent<Rigidbody>();
        _lastPosition = transform.position;
        _deathPosition = _lastPosition;

        if (_animator != null)
            _animator.applyRootMotion = false;

        if (_rigidbody != null)
        {
            _hadRigidbody = true;
            _originalUseGravity = _rigidbody.useGravity;
            _originalIsKinematic = _rigidbody.isKinematic;
            _originalConstraints = _rigidbody.constraints;
        }

        CacheAnimatorParameters();

        if (_health != null)
            _health.onDeath.AddListener(PlayDeath);
    }

    void OnDestroy()
    {
        if (_health != null)
            _health.onDeath.RemoveListener(PlayDeath);
    }

    void Update()
    {
        if (_animator == null)
            return;

        if (_deathTriggered)
        {
            SetSpeed(0f);
            return;
        }

        float actualSpeed = (transform.position - _lastPosition).magnitude / Mathf.Max(Time.deltaTime, 0.0001f);
        _lastPosition = transform.position;

        SetSpeed(ToAnimatorSpeed(actualSpeed));

        if (_status != null)
        {
            if (_status.IsStaggered && !_stunnedTriggered)
            {
                _stunnedTriggered = true;
                Trigger(stunnedTrigger, _hasStunned);
            }
            else if (!_status.IsStaggered)
            {
                _stunnedTriggered = false;
            }
        }

        if (_health != null && !_health.IsAlive)
            PlayDeath();
    }

    void LateUpdate()
    {
        if (!_deathTriggered || !pinRootOnDeath)
            return;

        transform.position = _deathPosition;

        if (_rigidbody != null)
            _rigidbody.position = _deathPosition;
    }

    public void PlayAttack()
    {
        if (_animator == null || (_health != null && !_health.IsAlive))
            return;

        if (!cycleAttackVariants)
        {
            Trigger(attackTrigger, _hasAttack);
            return;
        }

        string[] triggers = { attackTrigger, attack2Trigger, attack3Trigger };
        bool[] available = { _hasAttack, _hasAttack2, _hasAttack3 };

        for (int i = 0; i < triggers.Length; i++)
        {
            int index = (_attackIndex + i) % triggers.Length;
            if (!available[index])
                continue;

            _attackIndex = index + 1;
            Trigger(triggers[index], true);
            return;
        }
    }

    public void PlayDeath()
    {
        if (_animator == null || _deathTriggered)
            return;

        _deathTriggered = true;
        _deathPosition = transform.position;
        FreezeBodyForDeath();
        SetSpeed(0f);

        if (_hasIsDead)
            _animator.SetBool(isDeadParameter, true);

        if (_hasDeath)
            Trigger(deathTrigger, true);
        else
            Trigger(legacyDieTrigger, _hasLegacyDie);
    }

    void OnEnemyRespawned()
    {
        _deathTriggered = false;
        _stunnedTriggered = false;
        _lastPosition = transform.position;
        RestoreBodyAfterRespawn();

        if (_animator == null)
            return;

        ResetTrigger(deathTrigger, _hasDeath);
        ResetTrigger(legacyDieTrigger, _hasLegacyDie);
        ResetTrigger(stunnedTrigger, _hasStunned);

        if (_hasIsDead)
            _animator.SetBool(isDeadParameter, false);

        SetSpeed(0f);
    }

    void CacheAnimatorParameters()
    {
        if (_animator == null || _animator.runtimeAnimatorController == null)
            return;

        foreach (AnimatorControllerParameter parameter in _animator.parameters)
        {
            if (parameter.name == speedParameter && parameter.type == AnimatorControllerParameterType.Float)
                _hasSpeed = true;
            else if (parameter.name == attackTrigger && parameter.type == AnimatorControllerParameterType.Trigger)
                _hasAttack = true;
            else if (parameter.name == attack2Trigger && parameter.type == AnimatorControllerParameterType.Trigger)
                _hasAttack2 = true;
            else if (parameter.name == attack3Trigger && parameter.type == AnimatorControllerParameterType.Trigger)
                _hasAttack3 = true;
            else if (parameter.name == stunnedTrigger && parameter.type == AnimatorControllerParameterType.Trigger)
                _hasStunned = true;
            else if (parameter.name == deathTrigger && parameter.type == AnimatorControllerParameterType.Trigger)
                _hasDeath = true;
            else if (parameter.name == legacyDieTrigger && parameter.type == AnimatorControllerParameterType.Trigger)
                _hasLegacyDie = true;
            else if (parameter.name == isDeadParameter && parameter.type == AnimatorControllerParameterType.Bool)
                _hasIsDead = true;
        }
    }

    void SetSpeed(float speed)
    {
        if (_hasSpeed)
            _animator.SetFloat(speedParameter, speed, 0.12f, Time.deltaTime);
    }

    float ToAnimatorSpeed(float worldSpeed)
    {
        if (worldSpeed < walkSpeedThreshold)
            return 0f;

        return worldSpeed >= runSpeedThreshold ? 1f : 0.5f;
    }

    void FreezeBodyForDeath()
    {
        if (!_hadRigidbody || _rigidbody == null)
            return;

        if (!_rigidbody.isKinematic)
        {
            _rigidbody.linearVelocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;
        }

        _rigidbody.useGravity = false;
        _rigidbody.constraints = RigidbodyConstraints.FreezeAll;
        _rigidbody.isKinematic = true;
    }

    void RestoreBodyAfterRespawn()
    {
        if (!_hadRigidbody || _rigidbody == null)
            return;

        _rigidbody.useGravity = _originalUseGravity;
        _rigidbody.constraints = _originalConstraints;
        _rigidbody.isKinematic = _originalIsKinematic;

        if (!_rigidbody.isKinematic)
        {
            _rigidbody.linearVelocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;
        }
    }

    void Trigger(string triggerName, bool hasTrigger)
    {
        if (hasTrigger)
            _animator.SetTrigger(triggerName);
    }

    void ResetTrigger(string triggerName, bool hasTrigger)
    {
        if (hasTrigger)
            _animator.ResetTrigger(triggerName);
    }
}
