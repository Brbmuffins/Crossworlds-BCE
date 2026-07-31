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
    Vector3 pullDestination;
    float pullEndTime;
    float pullSpeed;
    float pullStopDistance;

    public void BeginPull(
        Vector3 destination,
        float duration,
        float speed,
        float stopDistance,
        GameObject source)
    {
        health = GetComponent<Health>();
        if (health == null || !health.IsAlive)
            return;

        pullDestination = destination;
        pullEndTime = Mathf.Max(
            pullEndTime,
            Time.time + Mathf.Max(0.05f, duration));
        pullSpeed = Mathf.Max(0.1f, speed);
        pullStopDistance = Mathf.Max(0f, stopDistance);

        StatusEffectManager status =
            GetComponent<StatusEffectManager>();
        status?.AddEffect(new StatusEffect(
            StatusEffectType.Stagger,
            duration,
            0f,
            source));

        // A persistent vortex refreshes this call several times per second.
        // Update the active pull in place so movement state is captured once and
        // the enemy is not repeatedly released/restarted between scan ticks.
        if (activePull != null)
            return;

        CaptureMovementState();
        activePull = StartCoroutine(PullRoutine());
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

    IEnumerator PullRoutine()
    {
        var wait = new WaitForFixedUpdate();

        while (Time.time < pullEndTime &&
               health != null &&
               health.IsAlive)
        {
            Vector3 current = transform.position;
            Vector3 toDestination = pullDestination - current;
            toDestination.y = 0f;
            float distance = toDestination.magnitude;
            if (distance > pullStopDistance + 0.01f)
            {
                float step = Mathf.Min(
                    pullSpeed * Time.fixedDeltaTime,
                    distance - pullStopDistance);
                Vector3 desired =
                    current + toDestination.normalized * step;
                MoveToPulledPosition(desired, step);
            }
            else if (body != null)
            {
                body.linearVelocity = Vector3.zero;
            }

            yield return wait;
        }

        activePull = null;
        pullEndTime = 0f;
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

        pullEndTime = 0f;
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
