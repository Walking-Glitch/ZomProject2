using System.Collections.Generic;
using UnityEngine;

// anything zombies can target and damage: player, turrets, barriers, cabins
public interface IAttackable
{
    Transform GetTransform();

    // nearest point on the target's surface, used to approach and reach large targets
    Vector3 GetClosestPoint(Vector3 from);

    int GetHealth();
    int GetPriority();
    void TakeDamage(int amount);
    void Death();
}

public static class TargetPriority
{
    // picked up on the way when within a zombie's aggro range
    public const int Diversion = 1;

    // main objective zombies march toward
    public const int Objective = 3;
}

public static class Targeting
{
    public static bool IsAlive(this IAttackable target)
    {
        // Unity's == also catches destroyed objects that a plain null check misses
        if (target == null || (target as UnityEngine.Object) == null) return false;

        return target.GetTransform().gameObject.activeInHierarchy && target.GetHealth() > 0;
    }

    public static Collider[] GetSolidColliders(GameObject target)
    {
        List<Collider> solid = new List<Collider>();

        foreach (Collider col in target.GetComponentsInChildren<Collider>())
        {
            if (!col.isTrigger) solid.Add(col);
        }

        return solid.ToArray();
    }

    // bounds-based so it also works on non-convex mesh colliders, where Collider.ClosestPoint doesn't
    public static Vector3 ClosestPointOnColliders(Collider[] colliders, Vector3 from, Vector3 fallback)
    {
        if (colliders == null) return fallback;

        Vector3 closest = fallback;
        float closestSqrDistance = float.MaxValue;

        foreach (Collider col in colliders)
        {
            if (col == null || !col.enabled) continue;

            Vector3 point = col.ClosestPointOnBounds(from);
            float sqrDistance = (point - from).sqrMagnitude;

            if (sqrDistance < closestSqrDistance)
            {
                closest = point;
                closestSqrDistance = sqrDistance;
            }
        }

        return closest;
    }

    // trigger events need a rigidbody on one side; barriers and cabins have none,
    // so give zombie triggers a kinematic body of their own when they aren't under one
    public static void EnsureTriggerBody(GameObject trigger)
    {
        if (trigger.GetComponentInParent<Rigidbody>() != null) return;

        Rigidbody body = trigger.AddComponent<Rigidbody>();
        body.isKinematic = true;
        body.useGravity = false;
    }
}

public static class TargetCleanup
{
    // removes a destroyed target and reopens its space in the A* graph
    public static void RemoveAndRefreshGraph(GameObject target, bool destroy)
    {
        // read bounds before disabling: colliders on an inactive object report empty bounds
        Bounds bounds = new Bounds(target.transform.position, Vector3.zero);
        bool hasBounds = false;

        foreach (Collider col in target.GetComponentsInChildren<Collider>())
        {
            if (col.isTrigger) continue;

            if (!hasBounds)
            {
                bounds = col.bounds;
                hasBounds = true;
            }
            else
            {
                bounds.Encapsulate(col.bounds);
            }
        }

        target.SetActive(false);

        if (destroy) Object.Destroy(target);

        if (hasBounds && AstarPath.active != null) AstarPath.active.UpdateGraphs(bounds);
    }
}
