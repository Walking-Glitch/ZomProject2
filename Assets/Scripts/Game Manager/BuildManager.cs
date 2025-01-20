using Assets.Scripts.Game_Manager;
using UnityEngine;

public class BuildManager : MonoBehaviour
{

    public GameObject[] turretPrefabs; // Array of turret prefabs
    public LayerMask placementLayer; // Define valid layers for placement
    public Material validMaterial;
    public Material invalidMaterial;

    public Transform AimTransform;

    private GameObject currentPreview;
    private GameObject selectedTurret;
    private bool isPlacing = false;
    private bool isPlacementValid; 

    private GameManager gameManager;

    private int currentIndex = 0;
    private GameObject tempPrefab;


    private void Start()
    {
        gameManager = GameManager.Instance;
    }
   
    public void CheckIfOnValidPlacement()
    {
       
          Vector3 direction = AimTransform.position - gameManager.WeaponManager.GunEndTransform.position;

          if (Physics.Raycast(gameManager.WeaponManager.GunEndTransform.position, direction.normalized, out RaycastHit hit, Mathf.Infinity,
                 placementLayer))
          {
            DisplaySelectedPrefab();
            isPlacementValid = true;

          }

            else
        {
            DestroyPreview();
            isPlacementValid = false;
        }

         
    }

    public void DisplaySelectedPrefab()
    {
        if (!isPlacing)
        {
            
            currentPreview = Instantiate(turretPrefabs[currentIndex], AimTransform.position, Quaternion.identity);
            currentPreview.transform.SetParent(AimTransform);
            isPlacing = true;
        }
        
    } 

    public void SwitchSelectedPrefab(float scrollDelta)
    {
        
        if (scrollDelta > 0)
        {
            currentIndex++;
            if(currentIndex >= turretPrefabs.Length)
            {
                currentIndex = 0;               
            }
            
        }
        else if (scrollDelta < 0)
        {
            currentIndex--;
            if(currentIndex < 0)
            {
                currentIndex = turretPrefabs.Length -1;
            }
        }

        isPlacing = false;

    }

    public void PlacePrefab()
    {
        if(isPlacementValid) currentPreview.transform.SetParent(null);
        isPlacing = false;
    }

    public void DestroyPreview()
    {
        Destroy(currentPreview);
    }
   
}