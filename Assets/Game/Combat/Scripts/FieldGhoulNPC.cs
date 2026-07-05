using System.Collections;
using Mirror;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// FieldGhoulNPC — passive wandering NPC for the Hub world.
/// Picks random points on the NavMesh within wanderRadius and walks to them,
/// pausing briefly between trips. Never aggros on players.
///
/// Setup (editor steps):
///   1. Duplicate an Enemy_Grunt prefab → rename "FieldGhoul_NPC".
///   2. Remove EnemyController; add this script + NetworkTransform.
///   3. Set wanderRadius in Inspector (default 8).
///   4. Bake the NavMesh if not already done (Window → AI → Navigation → Bake).
///   5. Drag prefab into the Hub scene where you want the ghoul to stand.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class FieldGhoulNPC : NetworkBehaviour
{
    [Header("Wander")]
    public float wanderRadius  = 8f;
    public float minWaitTime   = 2f;
    public float maxWaitTime   = 5f;
    public float moveSpeed     = 2f;

    [Header("Animations (optional)")]
    [Tooltip("Animator bool set true while moving — wire to your walk animation")]
    public string walkBoolParam = "isMoving";

    private NavMeshAgent _agent;
    private Animator     _anim;
    private Vector3      _origin;

    void Awake()
    {
        _agent       = GetComponent<NavMeshAgent>();
        _anim        = GetComponentInChildren<Animator>();
        _agent.speed = moveSpeed;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        _origin = transform.position;
        StartCoroutine(WanderLoop());
    }

    [Server]
    IEnumerator WanderLoop()
    {
        while (true)
        {
            // Pick a random point within the wander radius
            Vector2   circle  = Random.insideUnitCircle * wanderRadius;
            Vector3   dest    = _origin + new Vector3(circle.x, 0f, circle.y);
            NavMeshHit hit;

            if (NavMesh.SamplePosition(dest, out hit, wanderRadius, NavMesh.AllAreas))
                _agent.SetDestination(hit.position);

            // Walk until close enough
            yield return null;  // let the agent accept the destination
            while (_agent.pathPending || _agent.remainingDistance > _agent.stoppingDistance + 0.3f)
            {
                RpcSetWalking(true);
                yield return new WaitForSeconds(0.25f);
            }

            RpcSetWalking(false);

            // Idle pause before next wander
            yield return new WaitForSeconds(Random.Range(minWaitTime, maxWaitTime));
        }
    }

    // Sync walk animation to all clients
    [ClientRpc]
    void RpcSetWalking(bool walking)
    {
#if UNITY_EDITOR || !UNITY_SERVER
        if (_anim != null && _anim.runtimeAnimatorController != null)
            _anim.SetBool(walkBoolParam, walking);
#endif
    }
}
