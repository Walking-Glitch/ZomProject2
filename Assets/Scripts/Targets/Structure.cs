using System.Collections.Generic;
using UnityEngine;

// cabins the player defends; zombies march toward the nearest one
public class Structure : MonoBehaviour, IAttackable
{
    // standing structures, so zombies can find an objective without scanning the whole map
    public static readonly List<Structure> Active = new List<Structure>();

    public delegate void AllStructuresDestroyed();
    public static event AllStructuresDestroyed OnAllStructuresDestroyed;

    // HUD warnings
    public static event System.Action<Structure> OnStructureDamaged;
    public static event System.Action<Structure> OnStructureDestroyed;

    [Tooltip("Time.time of the last hit (runtime). The HUD flashes the cabin's bar while it's being hit.")]
    public float LastDamagedTime = -100f;

    [Tooltip("Cabin health. Zombies march to the nearest cabin and hit it for their Attack Damage (10) per swing.")]
    public int MaxHealth = 2000;
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
        LastDamagedTime = Time.time;

        if (Health <= 0)
        {
            Death();
        }
        else
        {
            OnStructureDamaged?.Invoke(this);
        }
    }

    public void Death()
    {
        TargetCleanup.RemoveAndRefreshGraph(gameObject, false);

        OnStructureDestroyed?.Invoke(this);

        if (Active.Count == 0)
        {
            // GameOverManager ends the run
            OnAllStructuresDestroyed?.Invoke();
        }
    }
}
