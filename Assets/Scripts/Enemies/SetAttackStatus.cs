using UnityEngine;
using Unity.Netcode;

public class SetAttackStatus : NetworkBehaviour
{
    private ZombieStateManager _manager;
    void Start()
    {
        _manager = GetComponentInParent<ZombieStateManager>();
    }

  
    private void OnTriggerStay(Collider other)
    {
        if (!IsServer) return;

        IAttackable attackable = other.GetComponentInParent<IAttackable>();
     

        if (attackable != null && attackable.GetTransform().gameObject.activeInHierarchy && !_manager.isDead)
        {
            Debug.Log("we detected attackable");
            
            float distance = Vector3.Distance(transform.position, attackable.GetTransform().position);
            if (distance < 0.8f)
            {
                //Debug.Log("inside 0.95f");                
                _manager.SetAttackStatus(true);
            }
        }
        else
        {
            _manager.SetAttackStatus(false);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsServer) return;

        IAttackable attackable = other.GetComponentInParent<IAttackable>();

        if (attackable != null)
        {
            _manager.SetAttackStatus(false);
        }
    }
     

}
