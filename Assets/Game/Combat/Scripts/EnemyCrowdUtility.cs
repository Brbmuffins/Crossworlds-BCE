using UnityEngine;
using UnityEngine.AI;

public static class EnemyCrowdUtility
{
    const float GoldenAngle = 2.3999631f;

    public static float Stable01(Component owner, int salt = 0)
    {
        if (owner == null)
            return 0.5f;

        unchecked
        {
            uint value = (uint)owner.GetInstanceID();
            value ^= (uint)salt * 0x9E3779B9u;
            value ^= value >> 16;
            value *= 0x7FEB352Du;
            value ^= value >> 15;
            value *= 0x846CA68Bu;
            value ^= value >> 16;
            return (value & 0x00FFFFFF) / 16777216f;
        }
    }

    public static float FirstAttackDelay(Component owner, float interval)
    {
        return Mathf.Min(0.12f, Mathf.Max(0f, interval) * 0.12f) * Stable01(owner, 103);
    }

    public static float ReadyCountUpAttackTimer(Component owner, float interval)
    {
        return Mathf.Max(0f, interval - FirstAttackDelay(owner, interval));
    }

    public static Vector3 ChaseSlot(Transform self, Transform target, float slotRadius, float radiusJitter = 0.45f)
    {
        if (self == null)
            return target != null ? target.position : Vector3.zero;

        if (target == null)
            return self.position;

        uint id = unchecked((uint)self.GetInstanceID());
        float angle = ((id % 32u) * GoldenAngle) + (Stable01(self, 17) - 0.5f) * 0.35f;
        float radiusOffset = (Stable01(self, 29) - 0.5f) * radiusJitter;
        float radius = Mathf.Max(0.45f, slotRadius + radiusOffset);
        Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;
        Vector3 desired = target.position + offset;

        if (NavMesh.SamplePosition(desired, out NavMeshHit hit, 1.75f, NavMesh.AllAreas))
            return hit.position;

        return desired;
    }

    public static bool ShouldMoveToMeleeSlot(Transform self, Transform target, Vector3 slot, float attackRange)
    {
        if (self == null || target == null)
            return false;

        return HorizontalDistance(self.position, target.position) > MeleeAttackReach(attackRange)
            || HorizontalDistance(self.position, slot) > MeleeSlotTolerance(attackRange);
    }

    public static bool CanMeleeAttack(Transform self, Transform target, Vector3 slot, float attackRange)
    {
        return !ShouldMoveToMeleeSlot(self, target, slot, attackRange);
    }

    public static float MeleeSlotRadius(float attackRange)
    {
        return Mathf.Max(0.55f, attackRange * 0.42f);
    }

    public static float MeleeSlotTolerance(float attackRange)
    {
        return Mathf.Max(0.6f, attackRange * 0.38f);
    }

    public static float MeleeAttackReach(float attackRange)
    {
        return Mathf.Max(attackRange, attackRange + MeleeSlotRadius(attackRange) + 0.65f);
    }

    public static void ApplyAgentCrowdSettings(NavMeshAgent agent, Component owner)
    {
        if (agent == null)
            return;

        agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
        agent.radius = Mathf.Min(agent.radius, 0.25f);
        agent.avoidancePriority = Mathf.RoundToInt(Mathf.Lerp(35f, 75f, Stable01(owner, 43)));
    }

    public static void DesyncAnimator(Animator animator, Component owner)
    {
        if (animator == null)
            return;

        animator.speed *= Mathf.Lerp(0.94f, 1.06f, Stable01(owner, 53));
        animator.Update(Mathf.Lerp(0.04f, 0.45f, Stable01(owner, 59)));
    }

    static float HorizontalDistance(Vector3 a, Vector3 b)
    {
        a.y = 0f;
        b.y = 0f;
        return Vector3.Distance(a, b);
    }
}
