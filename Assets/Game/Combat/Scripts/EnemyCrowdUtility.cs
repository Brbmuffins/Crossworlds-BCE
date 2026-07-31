using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public static class EnemyCrowdUtility
{
    const int ChaseSlotSectorCount = 12;
    static readonly HashSet<NavMeshAgent> RegisteredAgents = new HashSet<NavMeshAgent>();

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

        // Pick the closest member of a per-enemy slot ring to the side from
        // which this enemy is approaching. A fully absolute slot can be on the
        // opposite side of the target and make the agent path through it.
        Vector3 approach = self.position - target.position;
        approach.y = 0f;
        if (approach.sqrMagnitude < 0.0001f)
            approach = self.forward;

        float approachAngle = Mathf.Atan2(approach.z, approach.x);
        float sectorAngle = Mathf.PI * 2f / ChaseSlotSectorCount;
        float phase = Stable01(self, 17) * sectorAngle;
        float nearestSector = Mathf.Round((approachAngle - phase) / sectorAngle);
        // Do not send every enemy that shares an approach vector down the same
        // direct path. A stable lane offset fans a group across nearby sectors
        // (at most 60 degrees either side), allowing them to surround closely
        // without re-forming one overlapping column after forced movement.
        int approachLane = Mathf.FloorToInt(Stable01(self, 71) * 5f) - 2;
        float angle = phase + (nearestSector + approachLane) * sectorAngle;
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

        float targetDistance = HorizontalDistance(self.position, target.position);
        if (targetDistance > MeleeAttackReach(attackRange))
            return true;

        // Once an enemy has reached a usable position, do not make it circle
        // through the target just to match its crowd slot exactly. Reposition
        // only when it is genuinely overlapping the target.
        float overlapDistance = Mathf.Max(0.35f, MeleeSlotRadius(attackRange) * 0.55f);
        return targetDistance < overlapDistance
            && HorizontalDistance(self.position, slot) > MeleeSlotTolerance(attackRange);
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
        ApplyRoamingCrowdSettings(agent, owner);
    }

    public static void ApplyRoamingCrowdSettings(NavMeshAgent agent, Component owner)
    {
        if (agent == null)
            return;

        RegisteredAgents.Add(agent);
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
        agent.autoBraking = true;
        agent.avoidancePriority = Mathf.RoundToInt(Mathf.Lerp(35f, 75f, Stable01(owner, 43)));
    }

    public static void ApplyCombatCrowdSettings(NavMeshAgent agent, Component owner)
    {
        if (agent == null)
            return;

        RegisteredAgents.Add(agent);
        // Combat ignores inter-agent personal space so mobs can collapse into
        // their chase slots as tightly as possible. NavMesh pathfinding remains
        // active; only local crowd avoidance is disabled. Roaming and patrol
        // movement retain high-quality avoidance.
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
        agent.avoidancePriority = Mathf.RoundToInt(Mathf.Lerp(35f, 75f, Stable01(owner, 43)));
    }

    public static bool TryGetCombatUnstackPosition(
        NavMeshAgent owner, Component ownerComponent, out Vector3 destination)
    {
        destination = owner != null ? owner.transform.position : Vector3.zero;
        if (owner == null || !owner.enabled || !owner.gameObject.activeInHierarchy ||
            !owner.isOnNavMesh)
            return false;

        RegisteredAgents.Add(owner);
        RegisteredAgents.RemoveWhere(agent => agent == null);

        // Combatants are intentionally allowed to pack tightly. Use their
        // authored agent radii to distinguish tight formation from severe
        // model overlap; a fixed center distance was too small for large mobs.
        Vector3 separation = Vector3.zero;
        int overlapCount = 0;
        float largestSevereDistance = 0f;

        foreach (NavMeshAgent other in RegisteredAgents)
        {
            if (other == null || other == owner || !other.enabled ||
                !other.gameObject.activeInHierarchy ||
                other.gameObject.scene != owner.gameObject.scene)
                continue;

            Vector3 away = owner.transform.position - other.transform.position;
            away.y = 0f;
            float distance = away.magnitude;
            float severeOverlapDistance = Mathf.Max(
                0.5f,
                (owner.radius + other.radius) * 0.95f);
            if (distance >= severeOverlapDistance)
                continue;

            float penetrationWeight = 1f +
                (severeOverlapDistance - distance) / severeOverlapDistance;
            largestSevereDistance = Mathf.Max(largestSevereDistance, severeOverlapDistance);

            if (distance > 0.001f)
            {
                separation += away / distance * penetrationWeight;
            }
            else
            {
                // Give coincident agents stable, different escape headings so
                // they do not all choose the same point every behavior tick.
                float angle = Stable01(ownerComponent, 197) * Mathf.PI * 2f;
                separation += new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) *
                    penetrationWeight;
            }

            overlapCount++;
        }

        if (overlapCount == 0)
            return false;

        if (separation.sqrMagnitude < 0.0001f)
        {
            float angle = Stable01(ownerComponent, 211) * Mathf.PI * 2f;
            separation = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
        }

        // Move far enough that a large model does not take many behavior ticks
        // to escape the same cluster, while retaining unrestricted close combat
        // once its center is outside the severe-overlap threshold.
        float moveDistance = Mathf.Max(
            1f,
            Mathf.Max(owner.radius * 1.75f, largestSevereDistance * 1.1f));
        Vector3 desired = owner.transform.position + separation.normalized * moveDistance;
        float sampleDistance = Mathf.Max(1f, owner.radius * 2f);
        if (!NavMesh.SamplePosition(desired, out NavMeshHit hit, sampleDistance, owner.areaMask))
            return false;

        destination = hit.position;
        return true;
    }

    public static bool IsRoamingDestinationClear(
        NavMeshAgent owner, Vector3 candidate, float padding = 0.25f)
    {
        if (owner == null)
            return true;

        RegisteredAgents.Add(owner);
        RegisteredAgents.RemoveWhere(agent => agent == null);

        foreach (NavMeshAgent other in RegisteredAgents)
        {
            if (other == null || other == owner || !other.enabled ||
                !other.gameObject.activeInHierarchy || other.gameObject.scene != owner.gameObject.scene)
                continue;

            float clearance = Mathf.Max(0.1f, owner.radius + other.radius + padding);
            float clearanceSqr = clearance * clearance;
            Vector3 offset = other.transform.position - candidate;
            offset.y = 0f;
            if (offset.sqrMagnitude < clearanceSqr)
                return false;

            if (!other.isOnNavMesh || !other.hasPath)
            {
                if (DistancePointToSegmentXZ(
                        other.transform.position, owner.transform.position, candidate) < clearance)
                    return false;
                continue;
            }

            offset = other.destination - candidate;
            offset.y = 0f;
            if (offset.sqrMagnitude < clearanceSqr)
                return false;

            if (DistanceBetweenSegmentsXZ(
                    owner.transform.position, candidate,
                    other.transform.position, other.destination) < clearance)
                return false;
        }

        return true;
    }

    static float DistanceBetweenSegmentsXZ(Vector3 a0, Vector3 a1, Vector3 b0, Vector3 b1)
    {
        Vector2 av0 = new Vector2(a0.x, a0.z);
        Vector2 av1 = new Vector2(a1.x, a1.z);
        Vector2 bv0 = new Vector2(b0.x, b0.z);
        Vector2 bv1 = new Vector2(b1.x, b1.z);
        if (SegmentsIntersect(av0, av1, bv0, bv1)) return 0f;

        return Mathf.Min(
            DistancePointToSegment(av0, bv0, bv1),
            DistancePointToSegment(av1, bv0, bv1),
            DistancePointToSegment(bv0, av0, av1),
            DistancePointToSegment(bv1, av0, av1));
    }

    static float DistancePointToSegmentXZ(Vector3 point, Vector3 start, Vector3 end)
    {
        return DistancePointToSegment(
            new Vector2(point.x, point.z),
            new Vector2(start.x, start.z),
            new Vector2(end.x, end.z));
    }

    static float DistancePointToSegment(Vector2 point, Vector2 start, Vector2 end)
    {
        Vector2 segment = end - start;
        float lengthSqr = segment.sqrMagnitude;
        if (lengthSqr < 0.0001f) return Vector2.Distance(point, start);
        float t = Mathf.Clamp01(Vector2.Dot(point - start, segment) / lengthSqr);
        return Vector2.Distance(point, start + segment * t);
    }

    static bool SegmentsIntersect(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
    {
        Vector2 r = b - a;
        Vector2 s = d - c;
        float denominator = r.x * s.y - r.y * s.x;
        if (Mathf.Abs(denominator) < 0.0001f) return false;
        Vector2 delta = c - a;
        float t = (delta.x * s.y - delta.y * s.x) / denominator;
        float u = (delta.x * r.y - delta.y * r.x) / denominator;
        return t >= 0f && t <= 1f && u >= 0f && u <= 1f;
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
