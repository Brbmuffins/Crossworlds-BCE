using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Physics queries scoped to the caller's Unity scene.
///
/// Server zones are loaded with LocalPhysicsMode.Physics3D, so Physics.* queries
/// only search the default container scene and cannot see zone combatants.
/// Always supply an object that belongs to the same zone as the query.
/// </summary>
public static class ZonePhysics
{
    const int InitialCapacity = 64;
    const int MaximumCapacity = 4096;

    public static Collider[] OverlapSphere(
        GameObject context,
        Vector3 position,
        float radius,
        int layerMask = Physics.AllLayers,
        QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
    {
        PhysicsScene physicsScene = Resolve(context);
        int capacity = InitialCapacity;

        while (true)
        {
            var results = new Collider[capacity];
            int count = physicsScene.OverlapSphere(
                position, radius, results, layerMask, queryTriggerInteraction);
            if (count < capacity || capacity >= MaximumCapacity)
                return Trim(results, count);

            capacity = Mathf.Min(capacity * 2, MaximumCapacity);
        }
    }

    public static int OverlapSphereNonAlloc(
        GameObject context,
        Vector3 position,
        float radius,
        Collider[] results,
        int layerMask = Physics.AllLayers,
        QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
    {
        if (results == null)
            throw new System.ArgumentNullException(nameof(results));

        return Resolve(context).OverlapSphere(
            position, radius, results, layerMask, queryTriggerInteraction);
    }

    public static Collider[] OverlapBox(
        GameObject context,
        Vector3 center,
        Vector3 halfExtents,
        Quaternion orientation,
        int layerMask = Physics.AllLayers,
        QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
    {
        PhysicsScene physicsScene = Resolve(context);
        int capacity = InitialCapacity;

        while (true)
        {
            var results = new Collider[capacity];
            int count = physicsScene.OverlapBox(
                center, halfExtents, results, orientation, layerMask, queryTriggerInteraction);
            if (count < capacity || capacity >= MaximumCapacity)
                return Trim(results, count);

            capacity = Mathf.Min(capacity * 2, MaximumCapacity);
        }
    }

    public static Collider[] OverlapCapsule(
        GameObject context,
        Vector3 point0,
        Vector3 point1,
        float radius,
        int layerMask = Physics.AllLayers,
        QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
    {
        PhysicsScene physicsScene = Resolve(context);
        int capacity = InitialCapacity;

        while (true)
        {
            var results = new Collider[capacity];
            int count = physicsScene.OverlapCapsule(
                point0, point1, radius, results, layerMask, queryTriggerInteraction);
            if (count < capacity || capacity >= MaximumCapacity)
                return Trim(results, count);

            capacity = Mathf.Min(capacity * 2, MaximumCapacity);
        }
    }

    public static RaycastHit[] RaycastAll(
        GameObject context,
        Ray ray,
        float maxDistance,
        int layerMask = Physics.AllLayers,
        QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
    {
        return RaycastAll(
            context, ray.origin, ray.direction, maxDistance, layerMask, queryTriggerInteraction);
    }

    public static int RaycastNonAlloc(
        GameObject context,
        Ray ray,
        RaycastHit[] results,
        float maxDistance,
        int layerMask = Physics.AllLayers,
        QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
    {
        return RaycastNonAlloc(
            context, ray.origin, ray.direction, results, maxDistance,
            layerMask, queryTriggerInteraction);
    }

    public static int RaycastNonAlloc(
        GameObject context,
        Vector3 origin,
        Vector3 direction,
        RaycastHit[] results,
        float maxDistance,
        int layerMask = Physics.AllLayers,
        QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
    {
        if (results == null)
            throw new System.ArgumentNullException(nameof(results));

        return Resolve(context).Raycast(
            origin, direction, results, maxDistance, layerMask, queryTriggerInteraction);
    }

    public static RaycastHit[] RaycastAll(
        GameObject context,
        Vector3 origin,
        Vector3 direction,
        float maxDistance,
        int layerMask = Physics.AllLayers,
        QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
    {
        PhysicsScene physicsScene = Resolve(context);
        int capacity = InitialCapacity;

        while (true)
        {
            var results = new RaycastHit[capacity];
            int count = physicsScene.Raycast(
                origin, direction, results, maxDistance, layerMask, queryTriggerInteraction);
            if (count < capacity || capacity >= MaximumCapacity)
                return Trim(results, count);

            capacity = Mathf.Min(capacity * 2, MaximumCapacity);
        }
    }

    public static bool Raycast(
        GameObject context,
        Ray ray,
        out RaycastHit hit,
        float maxDistance,
        int layerMask = Physics.AllLayers,
        QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
    {
        return Resolve(context).Raycast(
            ray.origin, ray.direction, out hit, maxDistance, layerMask, queryTriggerInteraction);
    }

    public static bool Raycast(
        GameObject context,
        Vector3 origin,
        Vector3 direction,
        out RaycastHit hit,
        float maxDistance,
        int layerMask = Physics.AllLayers,
        QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
    {
        return Resolve(context).Raycast(
            origin, direction, out hit, maxDistance, layerMask, queryTriggerInteraction);
    }

    public static RaycastHit[] SphereCastAll(
        GameObject context,
        Vector3 origin,
        float radius,
        Vector3 direction,
        float maxDistance,
        int layerMask = Physics.AllLayers,
        QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
    {
        PhysicsScene physicsScene = Resolve(context);
        int capacity = InitialCapacity;

        while (true)
        {
            var results = new RaycastHit[capacity];
            int count = physicsScene.SphereCast(
                origin, radius, direction, results, maxDistance,
                layerMask, queryTriggerInteraction);
            if (count < capacity || capacity >= MaximumCapacity)
                return Trim(results, count);

            capacity = Mathf.Min(capacity * 2, MaximumCapacity);
        }
    }

    static PhysicsScene Resolve(GameObject context)
    {
        if (context != null)
        {
            Scene scene = context.scene;
            if (scene.IsValid() && scene.isLoaded)
            {
                PhysicsScene physicsScene = scene.GetPhysicsScene();
                if (physicsScene.IsValid())
                    return physicsScene;
            }
        }

        return Physics.defaultPhysicsScene;
    }

    static Collider[] Trim(Collider[] results, int count)
    {
        count = Mathf.Clamp(count, 0, results.Length);
        if (count == results.Length) return results;
        var trimmed = new Collider[count];
        System.Array.Copy(results, trimmed, count);
        return trimmed;
    }

    static RaycastHit[] Trim(RaycastHit[] results, int count)
    {
        count = Mathf.Clamp(count, 0, results.Length);
        if (count == results.Length) return results;
        var trimmed = new RaycastHit[count];
        System.Array.Copy(results, trimmed, count);
        return trimmed;
    }
}
