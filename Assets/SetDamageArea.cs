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
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _manager.SetIsInDamageArea(true);
        }
    }


    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _manager.SetIsInDamageArea(false);
        }
    }
}
