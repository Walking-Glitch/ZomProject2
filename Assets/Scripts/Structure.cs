using UnityEngine;
using Unity.Netcode;
public class Structure : NetworkBehaviour, IAttackable 
{
    public Transform GetTransform()
    {
        return transform;     
    }

    public int GetPriority()
    {
        return 3;
    }

   
}
