using UnityEngine;

public class SetAttackStatus : MonoBehaviour
{
    private ZombieStateManager _manager;
    void Start()
    {
        _manager = GetComponentInParent<ZombieStateManager>();
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _manager.SetPlayerAttackStatus(true);
        }
    }
     

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _manager.SetPlayerAttackStatus(false);
        }
    }

}
