using Mirror;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Animator))]
public class EnemyWanderAnimationDriver : MonoBehaviour
{
    public string speedParameter = "Speed";
    public string movingBoolParameter = "isMoving";
    public string attackTrigger = "Attack";
    public string alternateAttackTrigger = "Attack2";
    public string thirdAttackTrigger = "Attack3";
    public string kneelTrigger = "Kneel";
    public string rageTrigger = "Rage";
    public string blockTrigger = "Block";
    public string deathTrigger = "Die";
    public string isDeadParameter = "IsDead";

    [Tooltip("World-space movement above this value is considered walking.")]
    public float walkSpeedThreshold = 0.05f;

    [Tooltip("World-space movement above this value is considered running when no EnemyWanderAI state is available.")]
    public float runSpeedThreshold = 1.2f;

    [Tooltip("Alternate between the two attack triggers when both exist.")]
    public bool cycleAttackVariants = true;

    Animator _animator;
    NetworkAnimator _networkAnimator;
    EnemyWanderAI _wanderAI;
    Health _health;
    Vector3 _lastPosition;
    bool _deathTriggered;
    int _attackIndex;

    bool _hasSpeed;
    bool _hasMovingBool;
    bool _hasAttack;
    bool _hasAlternateAttack;
    bool _hasThirdAttack;
    bool _hasKneel;
    bool _hasRage;
    bool _hasBlock;
    bool _hasDeath;
    bool _hasIsDead;

    int _speedHash;
    int _movingBoolHash;
    int _attackHash;
    int _alternateAttackHash;
    int _thirdAttackHash;
    int _kneelHash;
    int _rageHash;
    int _blockHash;
    int _deathHash;
    int _isDeadHash;

    void Awake()
    {
        _animator = GetComponent<Animator>();
        _networkAnimator = GetComponent<NetworkAnimator>();
        _wanderAI = GetComponentInParent<EnemyWanderAI>();
        _health = GetComponentInParent<Health>();
        _lastPosition = transform.position;

        if (_animator != null)
        {
            _animator.applyRootMotion = false;
            CacheAnimatorParameters();
        }
    }

    void OnEnable()
    {
        if (_health == null)
            _health = GetComponentInParent<Health>();

        if (_health != null)
        {
            _health.onDeath.RemoveListener(PlayDeath);
            _health.onDeath.AddListener(PlayDeath);
        }

        _lastPosition = transform.position;
    }

    void OnDisable()
    {
        if (_health != null)
            _health.onDeath.RemoveListener(PlayDeath);
    }

    void Update()
    {
        if (_animator == null)
            return;

        if (!_hasSpeed && !_hasMovingBool)
            CacheAnimatorParameters();

        if (_deathTriggered)
        {
            SetMovement(0f, false);
            return;
        }

        float actualSpeed = (transform.position - _lastPosition).magnitude / Mathf.Max(Time.deltaTime, 0.0001f);
        _lastPosition = transform.position;

        bool moving = actualSpeed > walkSpeedThreshold;
        bool running = moving && (_wanderAI != null ? _wanderAI.HasAggroTarget : actualSpeed > runSpeedThreshold);
        float animatorSpeed = running ? 1f : moving ? 0.5f : 0f;
        SetMovement(animatorSpeed, moving);

        if (_health != null && !_health.IsAlive)
            PlayDeath();
    }

    public void PlayAttack()
    {
        if (_animator == null || _deathTriggered || (_health != null && !_health.IsAlive))
            return;

        TriggerNextAttack();
    }

    public void PlayKneel()
    {
        if (_animator == null || _deathTriggered || (_health != null && !_health.IsAlive))
            return;

        Trigger(_kneelHash, _hasKneel);
    }

    public void PlayRage()
    {
        if (_animator == null || _deathTriggered || (_health != null && !_health.IsAlive))
            return;

        Trigger(_rageHash, _hasRage);
    }

    public void PlayBlock()
    {
        if (_animator == null || _deathTriggered || (_health != null && !_health.IsAlive))
            return;

        Trigger(_blockHash, _hasBlock);
    }

    public void PlayDeath()
    {
        if (_animator == null || _deathTriggered)
            return;

        _deathTriggered = true;
        SetMovement(0f, false);
        SetBool(_isDeadHash, _hasIsDead, true);
        Trigger(_deathHash, _hasDeath);
    }

    void OnEnemyRespawned()
    {
        _deathTriggered = false;
        _attackIndex = 0;
        _lastPosition = transform.position;

        if (_animator == null)
            return;

        ResetTrigger(_attackHash, _hasAttack);
        ResetTrigger(_alternateAttackHash, _hasAlternateAttack);
        ResetTrigger(_thirdAttackHash, _hasThirdAttack);
        ResetTrigger(_kneelHash, _hasKneel);
        ResetTrigger(_rageHash, _hasRage);
        ResetTrigger(_blockHash, _hasBlock);
        ResetTrigger(_deathHash, _hasDeath);
        SetBool(_isDeadHash, _hasIsDead, false);
        SetMovement(0f, false);
    }

    void SetMovement(float speed, bool moving)
    {
        if (_animator == null)
            return;

        if (_hasSpeed)
            _animator.SetFloat(_speedHash, speed, 0.12f, Time.deltaTime);

        SetBool(_movingBoolHash, _hasMovingBool, moving);
    }

    void TriggerNextAttack()
    {
        if (!cycleAttackVariants)
        {
            if (_hasAttack)
                Trigger(_attackHash, _hasAttack);
            else if (_hasAlternateAttack)
                Trigger(_alternateAttackHash, _hasAlternateAttack);
            else
                Trigger(_thirdAttackHash, _hasThirdAttack);

            return;
        }

        for (int attempts = 0; attempts < 3; attempts++)
        {
            int index = _attackIndex % 3;
            _attackIndex++;

            if (index == 0 && _hasAttack)
            {
                Trigger(_attackHash, _hasAttack);
                return;
            }

            if (index == 1 && _hasAlternateAttack)
            {
                Trigger(_alternateAttackHash, _hasAlternateAttack);
                return;
            }

            if (index == 2 && _hasThirdAttack)
            {
                Trigger(_thirdAttackHash, _hasThirdAttack);
                return;
            }
        }
    }

    void Trigger(int hash, bool hasTrigger)
    {
        if (!hasTrigger)
            return;

        if (_networkAnimator != null && NetworkServer.active)
            _networkAnimator.SetTrigger(hash);
        else
            _animator.SetTrigger(hash);
    }

    void ResetTrigger(int hash, bool hasTrigger)
    {
        if (!hasTrigger)
            return;

        if (_networkAnimator != null && NetworkServer.active)
            _networkAnimator.ResetTrigger(hash);
        else if (_animator != null)
            _animator.ResetTrigger(hash);
    }

    void SetBool(int hash, bool hasBool, bool value)
    {
        if (hasBool && _animator != null)
            _animator.SetBool(hash, value);
    }

    void CacheAnimatorParameters()
    {
        _hasSpeed = false;
        _hasMovingBool = false;
        _hasAttack = false;
        _hasAlternateAttack = false;
        _hasThirdAttack = false;
        _hasKneel = false;
        _hasRage = false;
        _hasBlock = false;
        _hasDeath = false;
        _hasIsDead = false;

        _speedHash = Animator.StringToHash(speedParameter);
        _movingBoolHash = Animator.StringToHash(movingBoolParameter);
        _attackHash = Animator.StringToHash(attackTrigger);
        _alternateAttackHash = Animator.StringToHash(alternateAttackTrigger);
        _thirdAttackHash = Animator.StringToHash(thirdAttackTrigger);
        _kneelHash = Animator.StringToHash(kneelTrigger);
        _rageHash = Animator.StringToHash(rageTrigger);
        _blockHash = Animator.StringToHash(blockTrigger);
        _deathHash = Animator.StringToHash(deathTrigger);
        _isDeadHash = Animator.StringToHash(isDeadParameter);

        foreach (AnimatorControllerParameter parameter in _animator.parameters)
        {
            if (parameter.name == speedParameter && parameter.type == AnimatorControllerParameterType.Float)
                _hasSpeed = true;
            else if (parameter.name == movingBoolParameter && parameter.type == AnimatorControllerParameterType.Bool)
                _hasMovingBool = true;
            else if (parameter.name == attackTrigger && parameter.type == AnimatorControllerParameterType.Trigger)
                _hasAttack = true;
            else if (parameter.name == alternateAttackTrigger && parameter.type == AnimatorControllerParameterType.Trigger)
                _hasAlternateAttack = true;
            else if (parameter.name == thirdAttackTrigger && parameter.type == AnimatorControllerParameterType.Trigger)
                _hasThirdAttack = true;
            else if (parameter.name == kneelTrigger && parameter.type == AnimatorControllerParameterType.Trigger)
                _hasKneel = true;
            else if (parameter.name == rageTrigger && parameter.type == AnimatorControllerParameterType.Trigger)
                _hasRage = true;
            else if (parameter.name == blockTrigger && parameter.type == AnimatorControllerParameterType.Trigger)
                _hasBlock = true;
            else if (parameter.name == deathTrigger && parameter.type == AnimatorControllerParameterType.Trigger)
                _hasDeath = true;
            else if (parameter.name == isDeadParameter && parameter.type == AnimatorControllerParameterType.Bool)
                _hasIsDead = true;
        }
    }
}
