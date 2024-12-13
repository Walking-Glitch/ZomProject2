using Unity.VisualScripting;
using UnityEngine;

public class SetDetection : MonoBehaviour
{
    private ZombieStateManager _manager;
    void Start()
    {
        _manager = GetComponentInParent<ZombieStateManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _manager.SetPlayerDetectionStatus(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _manager.SetPlayerDetectionStatus(false);
        }
    }


}
