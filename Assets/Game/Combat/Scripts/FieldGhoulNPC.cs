using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Passive wandering NPC for ambient enemies in the hub/world scenes.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class FieldGhoulNPC : MonoBehaviour
{
    [Header("Wander")]
    public float wanderRadius = 8f;
    public float minWaitTime = 2f;
    public float maxWaitTime = 5f;

    [Header("Animation (optional)")]
    [Tooltip("Animator bool set true while moving. Only used if the controller declares this parameter.")]
    public string walkBoolParam = "isMoving";

    NavMeshAgent _agent;
    Animator _anim;
    Health _health;
    Vector3 _origin;
    Coroutine _wanderRoutine;
    bool _hasWalkParam;
    bool _stoppedForDeath;

    void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        _anim = GetComponentInChildren<Animator>();
        _health = GetComponent<Health>();
        _origin = transform.position;

        if (_health != null)
        {
            _health.onDeath.AddListener(StopForDeath);
            _health.onHealthChanged.AddListener(OnHealthChanged);

            if (!_health.IsAlive)
            {
                StopForDeath();
                return;
            }
        }

        if (_anim != null && _anim.runtimeAnimatorController != null)
        {
            foreach (var parameter in _anim.parameters)
            {
                if (parameter.name == walkBoolParam)
                {
                    _hasWalkParam = true;
                    break;
                }
            }
        }

        _wanderRoutine = StartCoroutine(WanderLoop());
    }

    void OnDisable()
    {
        if (_health != null)
        {
            _health.onDeath.RemoveListener(StopForDeath);
            _health.onHealthChanged.RemoveListener(OnHealthChanged);
        }
    }

    IEnumerator WanderLoop()
    {
        while (!_stoppedForDeath)
        {
            Vector2 circle = Random.insideUnitCircle * wanderRadius;
            Vector3 dest = _origin + new Vector3(circle.x, 0f, circle.y);

            if (_agent != null
                && _agent.enabled
                && _agent.isOnNavMesh
                && NavMesh.SamplePosition(dest, out NavMeshHit hit, wanderRadius, NavMesh.AllAreas))
            {
                _agent.SetDestination(hit.position);
            }

            yield return null;

            while (!_stoppedForDeath
                && _agent != null
                && _agent.enabled
                && _agent.isOnNavMesh
                && (_agent.pathPending || _agent.remainingDistance > _agent.stoppingDistance + 0.3f))
            {
                SetWalking(true);
                yield return new WaitForSeconds(0.25f);
            }

            SetWalking(false);
            yield return new WaitForSeconds(Random.Range(minWaitTime, maxWaitTime));
        }
    }

    void OnHealthChanged(float current, float max)
    {
        if (current <= 0f)
            StopForDeath();
    }

    void StopForDeath()
    {
        if (_stoppedForDeath)
            return;

        _stoppedForDeath = true;
        SetWalking(false);

        if (_wanderRoutine != null)
        {
            StopCoroutine(_wanderRoutine);
            _wanderRoutine = null;
        }

        if (_agent != null && _agent.enabled)
        {
            if (_agent.isOnNavMesh)
            {
                _agent.ResetPath();
                _agent.velocity = Vector3.zero;
                _agent.isStopped = true;
            }

            _agent.enabled = false;
        }
    }

    void SetWalking(bool walking)
    {
        if (_hasWalkParam && _anim != null)
            _anim.SetBool(walkBoolParam, walking);
    }
}
