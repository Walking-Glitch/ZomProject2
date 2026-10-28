using System.Collections.Generic;
using UnityEngine;

// cabins the player defends; zombies march toward the nearest one
public class Structure : MonoBehaviour, IAttackable
{
    // standing structures, so zombies can find an objective without scanning the whole map
    public static readonly List<Structure> Active = new List<Structure>();

    public delegate void AllStructuresDestroyed();
    public static event AllStructuresDestroyed OnAllStructuresDestroyed;

    [Tooltip("Cabin health. Zombies march to the nearest cabin and hit it for their Attack Damage (10) per swing.")]
    public int MaxHealth = 1000;
    [Tooltip("Current health (runtime). Editing it does not kill; only damage triggers death.")]
    public int Health;

    private Collider[] solidColliders;

    void Awake()
    {
        Health = MaxHealth;

        solidColliders = Targeting.GetSolidColliders(gameObject);
    }

    private void OnEnable()
    {
        Active.Add(this);
    }

    private void OnDisable()
    {
        Active.Remove(this);
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
        return TargetPriority.Objective;
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

        if (Active.Count == 0)
        {
            // game over is still pending; for now just report it
            Debug.Log("All structures destroyed");
            OnAllStructuresDestroyed?.Invoke();
        }
    }
}
