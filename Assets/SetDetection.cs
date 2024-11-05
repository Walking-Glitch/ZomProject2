using Unity.VisualScripting;
using UnityEngine;

public class SetDetection : MonoBehaviour
{
    private ZombieMovementStateManager movementManager;
    void Start()
    {
        movementManager = GetComponentInParent<ZombieMovementStateManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            movementManager.SetPlayerDetectionStatus(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            movementManager.SetPlayerDetectionStatus(false);
        }
    }


}
