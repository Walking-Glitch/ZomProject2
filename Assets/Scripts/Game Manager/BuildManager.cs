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

    //public LineRenderer Lazer;
    

    private void Start()
    {
        gameManager = GameManager.Instance;
    }
    void Update()
    {
      


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
            currentPreview = Instantiate(turretPrefabs[0], AimTransform.position, Quaternion.identity);
            currentPreview.transform.SetParent(AimTransform);
            isPlacing = true;
        }
        
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