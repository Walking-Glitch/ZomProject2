using UnityEngine;
using Unity.Netcode;
using Assets.Scripts.Game_Manager;

public class Obstacle : NetworkBehaviour, IAttackable
{
    public int MaxHealth;
    public int Health;

    private GameManager gameManager;

    void Start()
    {
        gameManager = GameManager.Instance;
    }
    public Transform GetTransform()
    {
        return transform;
    }
    public int GetPriority()
    {
        return 1;
    }

    public void TakeDamage(int amount) // to be called
    {
        Health -= amount;
        Health = Mathf.Clamp(Health, 0, MaxHealth);
        gameManager.UIManager.UpdateHealthUI();

        if (Health <= 0)
        {
            Debug.Log("inside if health < 0");
            Death();
        }

        Debug.Log(Health);
    }

    public void Death()
    {
        Debug.Log("dead called");
        gameObject.SetActive(false);
    }
}
