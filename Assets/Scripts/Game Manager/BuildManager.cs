using Assets.Scripts.Game_Manager;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildManager : MonoBehaviour
{
    // actual list to return
    private List<GameObject> currentFakeList;
    private List<GameObject> currentRealList;

    // list of functional objects to spawn 
    public List<GameObject> turretPrefabs;
    public List<GameObject> barriersPrefabs;

    //list of dummy objects to display when selecting a placement
    public List<GameObject> fakeTurretPrefabs;
    public List<GameObject> fakeBarriersPrefabs;

    //valid layers for placement
    public LayerMask placementLayer;
    public LayerMask placementObstacle; 

    //materials to switch depending on availability  
    public Material validMaterial;
    public Material invalidMaterial;

    //transform to parent the instantiated objects
    public Transform AimTransform;

    // rotation of current preview
    private Vector3 previewEulerAngles;
    private Vector3 previewDefaultStartAngles = Vector3.zero;

    private GameObject currentPreview;
    private GameObject selectedPrefab;
    [SerializeField] public bool isPlacing = false;
    [SerializeField] private bool isPlacementValid;

    //secondary canvas that is attached to the dummy object
    public GameObject BuildCanvas;
    private BuildCanvas buildCanvasX;
    //game manager for references
    private GameManager gameManager;

    // index to iterate through the different lists. 
    private int currentIndex = 0;

    [SerializeField] private float dimensions;
    private Vector3 Center;
    private Vector3 HalfExtents;

    private void Start()
    {
        gameManager = GameManager.Instance;

        currentFakeList = fakeTurretPrefabs;
        currentRealList = turretPrefabs;

        buildCanvasX = BuildCanvas.GetComponent<BuildCanvas>();   
    }

    private void Update()
    {

    }

    public void CheckIfOnValidPlacement()
    {

        Vector3 direction = (AimTransform.position - gameManager.WeaponManager.GunEndTransform.position).normalized;

        if (Physics.Raycast(gameManager.WeaponManager.GunEndTransform.position, direction.normalized, out RaycastHit hit, 40f,
                placementLayer))
        {
            //Debug.Log($"Hit: {hit.collider.name}, Layer: {LayerMask.LayerToName(hit.collider.gameObject.layer)}");

            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
            {

                DisplaySelectedPrefab(AlternateBetweenFakeLists());

                //dimensions = GetPrefabDimensions(currentPreview);

                //if (currentPreview == null) return;

                RotatePreview(currentPreview);

                //Collider[] colliders = Physics.OverlapSphere(currentPreview.transform.position, dimensions, placementObstacle);

                //if (colliders.Length > 0)
                //{
                //    // Placement is invalid if any colliders are found
                //    SetIsValidPlacement(false);

                //    // Debug the colliders found inside the sphere
                //   // Debug.Log("Colliders found inside the sphere:");
                //    foreach (Collider collider in colliders)
                //    {
                //       // Debug.Log($"Collider Name: {collider.name}, Tag: {collider.tag}, Layer: {LayerMask.LayerToName(collider.gameObject.layer)}");
                //    }
                //}
                //else
                //{

                //    SetIsValidPlacement(true);
                //}
                if(IsPlacementValidWithBox(currentPreview, placementObstacle))
                {
                    SetIsValidPlacement(true);
                }
                else
                {
                    SetIsValidPlacement(false);
                }


            }
        }

        else
        {
            //Debug.Log($"Hit: {hit.collider.name}, Layer: {LayerMask.LayerToName(hit.collider.gameObject.layer)}");

            SetIsValidPlacement(false);
        }


    }

    private bool IsPlacementValidWithBox(GameObject prefab, LayerMask placementObstacle)
    {
        if (prefab == null)
        {
            Debug.LogError("Prefab is null!");
            return false;
        }
        Bounds bounds = new Bounds(prefab.transform.position, Vector3.zero);

        foreach (var meshRenderer in prefab.GetComponentsInChildren<MeshRenderer>())
        {
            bounds.Encapsulate(meshRenderer.bounds);
        }

      
        Vector3 center = bounds.center;
        Vector3 halfExtents = bounds.extents; // Extents are already half the size of bounds

        Center = center;
        HalfExtents = halfExtents;

        // Perform overlap check
        Collider[] colliders = Physics.OverlapBox(center, halfExtents, prefab.transform.rotation, placementObstacle);

        // Return true if no overlaps, false otherwise
        return colliders.Length == 0;
    }

    public void RotatePreview(GameObject currentPreview)
    {
        if (Input.GetKey(KeyCode.Q))
        {
            float rotateRate = -30;

            currentPreview.transform.Rotate(0, rotateRate * Time.deltaTime, 0);
        }
        else if (Input.GetKey(KeyCode.E))
        {
            float rotateRate = 30;

            currentPreview.transform.Rotate(0, rotateRate * Time.deltaTime, 0);
        }

        previewEulerAngles = currentPreview.transform.eulerAngles;

    }
    public List<GameObject> AlternateBetweenFakeLists()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {

           
            currentFakeList = fakeTurretPrefabs;
            currentRealList = turretPrefabs;

            //disable previous object while keeping the index
            DestroyPreview();
            isPlacing = false;

        }

        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {

            
            currentFakeList = fakeBarriersPrefabs;
            currentRealList = barriersPrefabs;

            //disable previous object while keeping the index
            DestroyPreview();
            isPlacing = false;
             
        }

        if (currentFakeList == null) Debug.Log("CURRENT LIST IS NULL");

         
        return currentFakeList;
    }
    public List<GameObject> AlternateBetweenLists(List<GameObject> list)
    { 
        currentRealList = list;

        return currentRealList;
    }// this can be deleted later
    public void DisplaySelectedPrefab(List<GameObject> list)
    {

        if (!isPlacing)
        {
            currentPreview = Instantiate(list[currentIndex], AimTransform.position, Quaternion.Euler(previewEulerAngles));
            currentPreview.transform.SetParent(AimTransform);

            if(currentPreview != null)
            {
                EnableCanvas();
                BuildCanvas.transform.SetParent(currentPreview.transform);               
                BuildCanvas.transform.localPosition = Vector3.zero;
                buildCanvasX.DisplayPrefabRequirments();

            }

            isPlacing = true;
        }

    }

    public void AssignMaterial(GameObject currentPreview, Material valid, Material invalid)
    {
        if (currentPreview != null)
        {
            MeshRenderer[] meshRenderers = currentPreview.GetComponentsInChildren<MeshRenderer>();

            if (isPlacementValid)
            {
                foreach (MeshRenderer meshRenderer in meshRenderers)
                {
                    meshRenderer.material = valid;
                }

            }
            else
            {
                foreach (MeshRenderer meshRenderer in meshRenderers)
                {
                    meshRenderer.material = invalidMaterial;
                }
            }
        }
    }

    public void SwitchSelectedPrefab(float scrollDelta, List<GameObject> list)
    {

        if (scrollDelta > 0)
        {
            currentIndex++;
            if (currentIndex >= list.Count)
            {
                currentIndex = 0;
            }

        }
        else if (scrollDelta < 0)
        {
            currentIndex--;
            if (currentIndex < 0)
            {
                currentIndex = list.Count - 1;
            }
        }

        Destroy(currentPreview);
        isPlacing = false;

        previewEulerAngles = previewDefaultStartAngles;

        DisplaySelectedPrefab(AlternateBetweenFakeLists());
    }

    void EnableCanvas()
    {
        Canvas canvas = BuildCanvas.GetComponent<Canvas>();
        CanvasScaler canvasScaler = BuildCanvas.GetComponent<CanvasScaler>();
        GraphicRaycaster graphicRaycaster = BuildCanvas.GetComponent<GraphicRaycaster>();

        if (canvas != null) canvas.enabled = true;
        if (canvasScaler != null) canvasScaler.enabled = true;
        if (graphicRaycaster != null) graphicRaycaster.enabled = true;

        // If you are using any custom scripts like BuildCanvas
        BuildCanvas rotateScript = BuildCanvas.GetComponent<BuildCanvas>();
        if (rotateScript != null) rotateScript.enabled = true;

        
        BuildCanvas.SetActive(true); // Ensure the GameObject itself is active
    }

    void DisableCanvas()
    {
        BuildCanvas.transform.SetParent(null);
        //BuildCanvas.SetActive(false);
    }

    public void PlacePrefab()
    {
        if (ReturnIsValidPlacement())
        {

            selectedPrefab = Instantiate(currentRealList[currentIndex], AimTransform.position, currentPreview.transform.rotation);
            selectedPrefab.transform.SetParent(AimTransform);

            selectedPrefab.transform.SetParent(null);

            gameManager.EconomyManager.AddTurretToEconomyManager(selectedPrefab);
            //
            DisableCanvas();
            Destroy(currentPreview);
            isPlacing = false;

             
            Bounds bounds = selectedPrefab.GetComponent<Collider>().bounds;
            AstarPath.active.UpdateGraphs(bounds);
        }


    }

    private float GetPrefabDimensions(GameObject prefab)
    {
        if (prefab == null)
            return 10f; // Default dimensions if no prefab exists

        Bounds bounds = new Bounds(prefab.transform.position, Vector3.zero);

        // Combine bounds of all child MeshRenderers
        foreach (var meshRenderer in prefab.GetComponentsInChildren<MeshRenderer>())
        {
            bounds.Encapsulate(meshRenderer.bounds);
        }

        
        // Use the largest dimension of the bounds as the dimensions
        return bounds.extents.magnitude;
    }
 
    public void DestroyPreview()
    {
      
        currentIndex = 0;

        DisableCanvas();
        Destroy(currentPreview);
    }

    public void SetIsValidPlacement(bool placementValid)
    {
        isPlacementValid = placementValid;

        if (currentPreview != null)
        {
            AssignMaterial(currentPreview, validMaterial, invalidMaterial);
        }
    }

    private bool ReturnIsValidPlacement()
    {
        return isPlacementValid;
    }

    private void OnDrawGizmosSelected()
    {
        // Draw the max range in red
        Gizmos.color = Color.red;
        if(currentPreview != null)
        //Gizmos.DrawWireSphere(currentPreview.transform.position, dimensions);
        Gizmos.DrawWireCube(Center, HalfExtents * 2);


    }
}