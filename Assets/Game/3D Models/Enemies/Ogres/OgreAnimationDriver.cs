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

    [Tooltip("Vertical offset applied to the pinned root while the ogre is dead.")]
    public float deathRootYOffset = -1.6f;

    [Tooltip("After death, adjust the pinned root so the visible corpse rests on the ground.")]
    public bool snapCorpseToGroundOnDeath = true;

    public LayerMask groundMask = ~0;
    public float groundProbeHeight = 5f;
    public float groundProbeDistance = 12f;
    public float corpseGroundOffset = 0.03f;
    public float corpseGroundSnapSeconds = 1.5f;

    Animator _animator;
    Health _health;
    StatusEffectManager _status;
    Rigidbody _rigidbody;
    Vector3 _lastPosition;
    Vector3 _deathPosition;
    bool _deathTriggered;
    bool _stunnedTriggered;
    int _attackIndex;
    float _deathStartedAt;

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

        if (snapCorpseToGroundOnDeath && Time.time - _deathStartedAt <= corpseGroundSnapSeconds)
            SnapCorpseToGround();

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
        _deathStartedAt = Time.time;
        _deathPosition = transform.position + Vector3.up * deathRootYOffset;
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
        _deathStartedAt = 0f;
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
        {
            if (speed <= 0f)
                _animator.SetFloat(speedParameter, 0f);
            else
                _animator.SetFloat(speedParameter, speed, 0.08f, Time.deltaTime);
        }
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

    void SnapCorpseToGround()
    {
        if (!TryFindGroundY(out float groundY) || !TryFindRendererBottomY(out float bottomY))
            return;

        float targetBottomY = groundY + corpseGroundOffset;
        float deltaY = targetBottomY - bottomY;
        if (Mathf.Abs(deltaY) <= 0.01f)
            return;

        _deathPosition.y += deltaY;
    }

    bool TryFindGroundY(out float groundY)
    {
        groundY = 0f;

        Vector3 origin = _deathPosition + Vector3.up * groundProbeHeight;
        float distance = groundProbeHeight + groundProbeDistance;
        RaycastHit[] hits = ZonePhysics.RaycastAll(
            gameObject,
            origin,
            Vector3.down,
            distance,
            groundMask,
            QueryTriggerInteraction.Ignore);

        if (hits == null || hits.Length == 0)
            return false;

        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider == null)
                continue;

            if (hit.collider.transform.IsChildOf(transform))
                continue;

            groundY = hit.point.y;
            return true;
        }

        return false;
    }

    bool TryFindRendererBottomY(out float bottomY)
    {
        bottomY = float.PositiveInfinity;
        bool found = false;

        Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
        foreach (Renderer renderer in renderers)
        {
            if (renderer == null || !renderer.enabled)
                continue;

            if (!(renderer is SkinnedMeshRenderer) && !(renderer is MeshRenderer))
                continue;

            bottomY = Mathf.Min(bottomY, renderer.bounds.min.y);
            found = true;
        }

        return found;
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
