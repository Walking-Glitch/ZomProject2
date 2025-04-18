using UnityEngine;

public interface IAttackable
{
    Transform GetTransform();
    int GetPriority();
    void TakeDamage(int amount);
    void Death();
}
