using UnityEngine;

public class PlayerStatus : MonoBehaviour
{
    public int MaxHealth;
    public int Health;
    void Start()
    {
        Health = MaxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            PlayerTakeDamage(10);
        }
    }

    public void PlayerTakeDamage(int damage)
    {
        Health -= damage;
        Health = Mathf.Clamp(Health, 0, MaxHealth);
    }



}
