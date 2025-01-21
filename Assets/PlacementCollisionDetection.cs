using Assets.Scripts.Game_Manager;
using UnityEngine;

public class PlacementCollisionDetection : MonoBehaviour
{
    private GameManager gameManager;
    void Start()
    {
        gameManager = GameManager.Instance;
    }

    // Update is called once per frame
    void Update()
    {

    }

    //private void OnTriggerStay(Collider other)
    //{
    //    if (!other.CompareTag("Ground"))
    //    {
    //        gameManager.BuildManager.SetIsValidPlacement(false);
    //        return;

    //    }
    //    gameManager.BuildManager.SetIsValidPlacement(true);
    //}

    //private void OnTriggerEnter(Collider other)
    //{
    //    if (!other.CompareTag("Ground"))
    //    {
    //        gameManager.BuildManager.SetIsValidPlacement(false);          
    //    }
    //}

    //private void OnTriggerExit(Collider other)
    //{
    //    if (!other.CompareTag("Ground"))
    //    {
    //        gameManager.BuildManager.SetIsValidPlacement(true);
    //    }
    //}


}
