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

            // the box is never farther than the real surface, so a collider whose box is already
            // farther than the best point so far can be skipped without the more expensive surface check
            Vector3 boundsPoint = col.ClosestPointOnBounds(from);
            if ((boundsPoint - from).sqrMagnitude >= closestSqrDistance) continue;

            Vector3 point = ClosestPointOnSurface(col, from, boundsPoint);
            float sqrDistance = (point - from).sqrMagnitude;

            if (sqrDistance < closestSqrDistance)
            {
                closest = point;
                closestSqrDistance = sqrDistance;
            }
        }

        return closest;
    }

    // exact for boxes, spheres, capsules and convex meshes. Non-convex meshes (barriers, cabins) only offer the
    // axis-aligned box, which on rotated or spiky models sits meters off the real surface (zombies swung from 5 m away),
    // so cast toward the model's centre and take the first point of its surface instead
    private static Vector3 ClosestPointOnSurface(Collider col, Vector3 from, Vector3 boundsPoint)
    {
        if (!(col is MeshCollider mesh) || mesh.convex) return col.ClosestPoint(from);

        Bounds bounds = col.bounds;
        Vector3 toCenter = bounds.center - from;
        float distance = toCenter.magnitude;

        if (distance > 0.0001f && col.Raycast(new Ray(from, toCenter / distance), out RaycastHit hit, distance))
        {
            // keep the surface's horizontal position but the height nearest to us, as the box did:
            // the centre of a raised cabin is meters up, and the hit there shouldn't count as out of reach
            Vector3 point = hit.point;
            point.y = Mathf.Clamp(from.y, bounds.min.y, bounds.max.y);
            return point;
        }

        // no surface between us and the centre: we're inside the mesh (touching), or the cast slipped past a thin part
        return boundsPoint;
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
