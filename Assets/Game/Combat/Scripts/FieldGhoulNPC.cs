using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// FieldGhoulNPC — passive wandering NPC for the Hub world.
/// Plain MonoBehaviour: wanders on whichever machine runs the scene (server
/// in production, editor in play-mode testing). No Mirror sync needed —
/// these NPCs are ambient and never affect gameplay state.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class FieldGhoulNPC : MonoBehaviour
{
    [Header("Wander")]
    public float wanderRadius  = 8f;
    public float minWaitTime   = 2f;
    public float maxWaitTime   = 5f;

    [Header("Animation (optional)")]
    [Tooltip("Animator bool set true while moving — wire to your walk animation")]
    public string walkBoolParam = "isMoving";

    NavMeshAgent _agent;
    Animator     _anim;
    Vector3      _origin;
    bool         _hasWalkParam;   // controller actually declares walkBoolParam

    void Start()
    {
        _agent  = GetComponent<NavMeshAgent>();
        _anim   = GetComponentInChildren<Animator>();
        _origin = transform.position;

        // Only drive the walk bool if the controller declares it — the Field Goul
        // rig uses Speed/Attack/Scream/Die, so SetBool("isMoving") would spam
        // "Parameter does not exist" every tick otherwise.
        if (_anim != null && _anim.runtimeAnimatorController != null)
            foreach (var p in _anim.parameters)
                if (p.name == walkBoolParam) { _hasWalkParam = true; break; }

        StartCoroutine(WanderLoop());
    }

    IEnumerator WanderLoop()
    {
        while (true)
        {
            Vector2    circle = Random.insideUnitCircle * wanderRadius;
            Vector3    dest   = _origin + new Vector3(circle.x, 0f, circle.y);
            NavMeshHit hit;

            if (NavMesh.SamplePosition(dest, out hit, wanderRadius, NavMesh.AllAreas))
                _agent.SetDestination(hit.position);

            yield return null;
            while (_agent.pathPending || _agent.remainingDistance > _agent.stoppingDistance + 0.3f)
            {
                SetWalking(true);
                yield return new WaitForSeconds(0.25f);
            }

            SetWalking(false);
            yield return new WaitForSeconds(Random.Range(minWaitTime, maxWaitTime));
        }
    }

    void SetWalking(bool walking)
    {
        if (_hasWalkParam)
            _anim.SetBool(walkBoolParam, walking);
    }
}
