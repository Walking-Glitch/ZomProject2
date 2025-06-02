using UnityEngine;

public interface IAttackable
{
    Transform GetTransform();

    int GetHealth();
    int GetPriority();
    void TakeDamage(int amount);
    void Death();
}
