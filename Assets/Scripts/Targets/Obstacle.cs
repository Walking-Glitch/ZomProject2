using UnityEngine;

// barriers (concrete, sandbags, hedgehogs) that zombies can break through
public class Obstacle : MonoBehaviour, IAttackable
{
    [Tooltip("Barrier health. Zombies that bump into it stop and hit it for their Attack Damage (10) per swing.")]
    public int MaxHealth = 100;
    [Tooltip("Current health (runtime). Editing it does not kill; only damage triggers death.")]
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
