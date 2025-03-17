using UnityEngine;

public class SetDamageArea : MonoBehaviour
{
    private ZombieStateManager _manager;
    void Start()
    {
        _manager = GetComponentInParent<ZombieStateManager>();
    }

    void Update()
    {
    }
    //private void OnTriggerStay(Collider other)
    //{
    //    if (other.CompareTag("Player"))
    //    {
    //        _manager.SetIsInDamageArea(true);
    //    }
    //}


    //private void OnTriggerExit(Collider other)
    //{
    //    if (other.CompareTag("Player"))
    //    {
    //        _manager.SetIsInDamageArea(false);
    //    }
    //}

    private void OnTriggerStay(Collider other)
    {
        IAttackable attackable = other.GetComponentInParent<IAttackable>();

        if (attackable != null)
        {
            _manager.SetIsInDamageArea(true, attackable); // Pass the attackable target
        }
    }

    private void OnTriggerExit(Collider other)
    {
        IAttackable attackable = other.GetComponentInParent<IAttackable>();

        if (attackable != null)
        {
            _manager.SetIsInDamageArea(false, attackable);
        }
    }
}
