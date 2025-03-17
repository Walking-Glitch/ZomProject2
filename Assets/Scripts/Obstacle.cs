using UnityEngine;
using Unity.Netcode;

public class Obstacle : NetworkBehaviour, IAttackable
{
    public Transform GetTransform()
    {
        return transform;
    }
    public int GetPriority()
    {
        return 1;
    }

}
