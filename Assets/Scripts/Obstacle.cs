using UnityEngine;
using Unity.Netcode;
using Assets.Scripts.Game_Manager;

public class Obstacle : NetworkBehaviour, IAttackable
{
    public int MaxHealth;
    public int Health;

    public bool IsActive;

    [SerializeField] NetworkVariable <int> NetworkHealth = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField]  NetworkVariable<bool> NetworkIsActive = new NetworkVariable<bool>(true, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    
    private GameManager gameManager;

    void Start()
    {
        gameManager = GameManager.Instance;

        MaxHealth = 100;
        Health = MaxHealth;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsServer) return;

        NetworkHealth.Value = 100;

        NetworkHealth.OnValueChanged += (prev, curr) =>
        {
            Health = curr;
        };

        NetworkIsActive.OnValueChanged += (prev, curr) =>
        {
            IsActive = curr;

            gameObject.SetActive(IsActive);

            Bounds bounds = GetComponent<MeshCollider>().bounds;
            AstarPath.active.UpdateGraphs(bounds);
        };
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
        if (!IsServer) return;

        NetworkHealth.Value -= amount;
        NetworkHealth.Value = Mathf.Clamp(NetworkHealth.Value, 0, MaxHealth);
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
        NetworkIsActive.Value = false;
    }
}
