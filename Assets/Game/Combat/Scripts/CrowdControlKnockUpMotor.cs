using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Server-side vertical forced-movement motor used by Spellbook knock-up effects.
/// The target's normal movement is suspended for one deterministic arc, while its
/// existing server-authoritative NetworkTransform carries the result to clients.
/// </summary>
[DisallowMultipleComponent]
public sealed class CrowdControlKnockUpMotor : MonoBehaviour
{
    Coroutine activeKnockUp;
    Health health;
    NavMeshAgent agent;
    Rigidbody body;
    bool hasAgentState;
    bool agentWasStopped;
    bool agentUpdatedPosition;
    bool agentUpdatedRotation;
    bool hasBodyState;
    bool bodyUsedGravity;
    Vector3 groundPosition;

    public void BeginKnockUp(
        float height,
        float duration,
        GameObject source)
    {
        health = GetComponent<Health>();
        if (health == null || !health.IsAlive)
            return;

        GetComponent<CrowdControlPullMotor>()?.CancelPull();
        CancelKnockUp();

        groundPosition = transform.position;
        CaptureMovementState();

        float safeDuration = Mathf.Max(0.1f, duration);
        StatusEffectManager status =
            GetComponent<StatusEffectManager>();
        status?.AddEffect(new StatusEffect(
            StatusEffectType.Stagger,
            safeDuration,
            0f,
            source));

        activeKnockUp = StartCoroutine(KnockUpRoutine(
            Mathf.Max(0.1f, height),
            safeDuration));
    }

    public void CancelKnockUp()
    {
        if (activeKnockUp == null)
            return;

        StopCoroutine(activeKnockUp);
        activeKnockUp = null;
        LandAndRestoreMovement();
    }

    void CaptureMovementState()
    {
        agent = GetComponent<NavMeshAgent>();
        body = GetComponent<Rigidbody>();
        hasAgentState =
            agent != null &&
            agent.enabled &&
            agent.isOnNavMesh;

        if (hasAgentState)
        {
            agentWasStopped = agent.isStopped;
            agentUpdatedPosition = agent.updatePosition;
            agentUpdatedRotation = agent.updateRotation;
            agent.ResetPath();
            agent.velocity = Vector3.zero;
            agent.isStopped = true;
            agent.updatePosition = false;
            agent.updateRotation = false;
        }

        hasBodyState = body != null;
        if (!hasBodyState)
            return;

        bodyUsedGravity = body.useGravity;
        body.useGravity = false;
        if (!body.isKinematic)
            body.linearVelocity = Vector3.zero;
    }

    IEnumerator KnockUpRoutine(float height, float duration)
    {
        var wait = new WaitForFixedUpdate();
        float elapsed = 0f;

        while (elapsed < duration &&
               health != null &&
               health.IsAlive)
        {
            float progress = Mathf.Clamp01(elapsed / duration);
            float verticalOffset =
                4f * height * progress * (1f - progress);
            MoveToPosition(
                groundPosition + Vector3.up * verticalOffset);

            yield return wait;
            elapsed += Time.fixedDeltaTime;
        }

        activeKnockUp = null;
        LandAndRestoreMovement();
    }

    void MoveToPosition(Vector3 position)
    {
        if (body != null && body.gameObject.activeInHierarchy)
        {
            if (!body.isKinematic)
                body.linearVelocity = Vector3.zero;
            body.MovePosition(position);
            return;
        }

        transform.position = position;
    }

    void LandAndRestoreMovement()
    {
        // The launch begins from a valid server position. Return to that exact
        // height instead of re-projecting through NavMesh.SamplePosition, which
        // can introduce vertical drift on agents with a non-zero base offset.
        MoveToPosition(groundPosition);

        if (hasBodyState && body != null)
        {
            if (!body.isKinematic)
                body.linearVelocity = Vector3.zero;
            body.useGravity = bodyUsedGravity;
        }
        hasBodyState = false;

        if (!hasAgentState)
            return;

        hasAgentState = false;
        if (agent == null ||
            !agent.enabled ||
            !agent.isOnNavMesh)
            return;

        agent.nextPosition = groundPosition;
        agent.updatePosition = agentUpdatedPosition;
        agent.updateRotation = agentUpdatedRotation;
        agent.isStopped = agentWasStopped;

        // Let the AI choose its next state immediately after landing. Restoring a
        // stopped attack-state snapshot can otherwise strand an enemy permanently.
        agent.isStopped = false;
    }

    void OnDisable()
    {
        CancelKnockUp();
    }
}
