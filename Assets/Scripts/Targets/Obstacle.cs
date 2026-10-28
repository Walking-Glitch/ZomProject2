using UnityEngine;

// barriers (concrete, sandbags, hedgehogs) that zombies can break through
public class Obstacle : MonoBehaviour, IAttackable
{
    public int MaxHealth = 100;
    public int Health;

    private Collider[] solidColliders;

    void Awake()
    {
        Health = MaxHealth;

        solidColliders = Targeting.GetSolidColliders(gameObject);
    }

    public Transform GetTransform()
    {
        return transform;
    }

    public Vector3 GetClosestPoint(Vector3 from)
    {
        return Targeting.ClosestPointOnColliders(solidColliders, from, transform.position);
    }

    public int GetHealth()
    {
        return Health;
    }

    public int GetPriority()
    {
        return TargetPriority.Diversion;
    }

    public void TakeDamage(int amount)
    {
        if (Health <= 0) return;

        Health -= amount;
        Health = Mathf.Clamp(Health, 0, MaxHealth);

        if (Health <= 0)
        {
            Death();
        }
    }

    public void Death()
    {
        TargetCleanup.RemoveAndRefreshGraph(gameObject, false);
    }
}
