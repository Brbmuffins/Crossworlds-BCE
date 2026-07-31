using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Server-side forced-movement motor used by Spellbook pull effects.
/// Keeping the coroutine on the affected target lets a newer pull replace an
/// older one cleanly instead of two casters fighting over the same transform.
/// </summary>
public sealed class CrowdControlPullMotor : MonoBehaviour
{
    Coroutine activePull;
    Health health;
    NavMeshAgent agent;
    Rigidbody body;
    bool hasAgentState;
    bool agentWasStopped;
    bool agentUpdatedPosition;
    bool agentUpdatedRotation;

    public void BeginPull(
        Vector3 destination,
        float duration,
        float speed,
        float stopDistance,
        GameObject source)
    {
        StopActivePull();

        health = GetComponent<Health>();
        if (health == null || !health.IsAlive)
            return;

        StatusEffectManager status =
            GetComponent<StatusEffectManager>();
        status?.AddEffect(new StatusEffect(
            StatusEffectType.Stagger,
            duration,
            0f,
            source));

        CaptureMovementState();
        activePull = StartCoroutine(PullRoutine(
            destination,
            Mathf.Max(0.05f, duration),
            Mathf.Max(0.1f, speed),
            Mathf.Max(0f, stopDistance)));
    }

    void CaptureMovementState()
    {
        agent = GetComponent<NavMeshAgent>();
        body = GetComponent<Rigidbody>();
        hasAgentState =
            agent != null &&
            agent.enabled &&
            agent.isOnNavMesh;

        if (!hasAgentState)
            return;

        agentWasStopped = agent.isStopped;
        agentUpdatedPosition = agent.updatePosition;
        agentUpdatedRotation = agent.updateRotation;
        agent.ResetPath();
        agent.velocity = Vector3.zero;
        agent.isStopped = true;
        agent.updatePosition = false;
    }

    IEnumerator PullRoutine(
        Vector3 destination,
        float duration,
        float speed,
        float stopDistance)
    {
        float elapsed = 0f;
        var wait = new WaitForFixedUpdate();

        while (elapsed < duration &&
               health != null &&
               health.IsAlive)
        {
            Vector3 current = transform.position;
            Vector3 toDestination = destination - current;
            toDestination.y = 0f;
            float distance = toDestination.magnitude;
            if (distance <= stopDistance + 0.01f)
                break;

            float step = Mathf.Min(
                speed * Time.fixedDeltaTime,
                distance - stopDistance);
            Vector3 desired =
                current + toDestination.normalized * step;
            MoveToPulledPosition(desired, step);

            elapsed += Time.fixedDeltaTime;
            yield return wait;
        }

        activePull = null;
        RestoreMovementState();
    }

    void MoveToPulledPosition(Vector3 desired, float step)
    {
        if (hasAgentState &&
            agent != null &&
            agent.enabled &&
            agent.isOnNavMesh)
        {
            float sampleDistance = Mathf.Max(0.5f, step * 2f);
            if (NavMesh.SamplePosition(
                desired,
                out NavMeshHit hit,
                sampleDistance,
                agent.areaMask))
            {
                agent.nextPosition = hit.position;
                transform.position = hit.position;
            }

            // Never fall back to raw physics movement for an active NavMeshAgent;
            // a failed sample means this step would leave its walkable surface.
            return;
        }

        if (body != null && body.gameObject.activeInHierarchy)
        {
            body.linearVelocity = Vector3.zero;
            body.MovePosition(desired);
            return;
        }

        transform.position = desired;
    }

    void StopActivePull()
    {
        if (activePull != null)
        {
            StopCoroutine(activePull);
            activePull = null;
        }

        RestoreMovementState();
    }

    void RestoreMovementState()
    {
        if (!hasAgentState)
            return;

        hasAgentState = false;
        if (agent == null ||
            !agent.enabled ||
            !agent.isOnNavMesh)
            return;

        agent.nextPosition = transform.position;
        agent.updatePosition = agentUpdatedPosition;
        agent.updateRotation = agentUpdatedRotation;
        agent.isStopped = agentWasStopped;
    }

    void OnDisable()
    {
        StopActivePull();
    }
}
