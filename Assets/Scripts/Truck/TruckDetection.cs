using Assets.Scripts.Game_Manager;
using UnityEngine;

public class TruckDetection : MonoBehaviour
{
    private TruckController truck;
    private GameManager gameManager;
    void Start()
    {
        gameManager = GameManager.Instance;
        
        truck = GetComponentInParent<TruckController>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") && !truck.isPlayerIn)
        {
            gameManager.PlayerStats.setIsInInteractableRange(true);
        }


    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            gameManager.PlayerStats.setIsInInteractableRange(false);
        }
    }
}
