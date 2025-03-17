using UnityEngine;

public class SetAttackStatus : MonoBehaviour
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
    //    if (other.CompareTag("Player") && !_manager.isDead)
    //    {

    //        float distance = Vector3.Distance(this.transform.position, other.transform.position);
    //        if (distance < 0.8f)
    //        {
    //            //Debug.Log("inside 0.95f");
    //            _manager.SetPlayerAttackStatus(true);
    //        }

    //    }
    //}


    //private void OnTriggerExit(Collider other)
    //{
    //    if (other.CompareTag("Player"))
    //    {
    //        _manager.SetPlayerAttackStatus(false);
    //    }
    //}

    private void OnTriggerStay(Collider other)
    {
        IAttackable attackable = other.GetComponentInParent<IAttackable>();

        if (attackable != null && !_manager.isDead)
        {
            float distance = Vector3.Distance(transform.position, attackable.GetTransform().position);
            if (distance < 0.8f)
            {
                //Debug.Log("inside 0.95f");
                _manager.SetPlayerAttackStatus(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        IAttackable attackable = other.GetComponentInParent<IAttackable>();

        if (attackable != null)
        {
            _manager.SetPlayerAttackStatus(false);
        }
    }

}
