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
    [Tooltip("Layers the preview can be placed on (Ground).")]
    public LayerMask placementLayer;
    [Tooltip("Layers that block placement when they overlap the preview (barriers, turrets, cabins, player, zombies, scenery). Trigger zones never block.")]
    public LayerMask placementObstacle;

    [Header("Placement Rules")]
    [Tooltip("Max distance (m) from the player to the placement point.")]
    public float MaxBuildDistance = 20f;
    [Tooltip("Builds must be within this distance (m) of a standing cabin, like a settlement area. 0 = build anywhere.")]
    public float BuildAreaRadius = 90f;
    [Tooltip("Off = building is only allowed during the day.")]
    public bool AllowBuildingAtNight = true;
    [Tooltip("How fast (degrees/s) Q and E rotate the preview.")]
    public float RotationSpeed = 90f;

    // preview renderers, cached so the valid/invalid tint isn't looked up and reapplied every frame
    private MeshRenderer[] previewRenderers;
    private bool? appliedValidMaterial;

    // build cost of the current preview, read from its SpawnRequirements
    private SpawnRequirements previewRequirements;
    private static readonly Collider[] placementOverlapBuffer = new Collider[64];

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
                SetIsValidPlacement(CanAffordPreview() && IsPlacementValidWithBox(currentPreview, placementObstacle) && IsWithinBuildRules(currentPreview.transform.position));


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

        // Perform overlap check; trigger zones (zombie reach/detection areas) aren't solid, so they don't block
        int count = Physics.OverlapBoxNonAlloc(center, halfExtents, placementOverlapBuffer, prefab.transform.rotation, placementObstacle, QueryTriggerInteraction.Ignore);

        for (int i = 0; i < count; i++)
        {
            // the preview always sits on a terrain; the Environment terrain is in the obstacle layers for its props, not its ground
            if (placementOverlapBuffer[i] is TerrainCollider) continue;

            return false;
        }

        return true;
    }

    // range from the player, settlement area around the cabins, and day-only building
    private bool IsWithinBuildRules(Vector3 point)
    {
        Vector3 player = gameManager.PlayerGameObject.transform.position;
        if ((point - player).sqrMagnitude > MaxBuildDistance * MaxBuildDistance) return false;

        if (!AllowBuildingAtNight && gameManager.DayCycle != null && gameManager.DayCycle.IsNightTime) return false;

        if (BuildAreaRadius <= 0f) return true;

        foreach (Structure cabin in Structure.Active)
        {
            if (!cabin.IsAlive()) continue;

            if ((cabin.GetClosestPoint(point) - point).sqrMagnitude <= BuildAreaRadius * BuildAreaRadius) return true;
        }

        return false;
    }

    public void RotatePreview(GameObject currentPreview)
    {
        if (Input.GetKey(KeyCode.Q))
        {
            currentPreview.transform.Rotate(0, -RotationSpeed * Time.deltaTime, 0);
        }
        else if (Input.GetKey(KeyCode.E))
        {
            currentPreview.transform.Rotate(0, RotationSpeed * Time.deltaTime, 0);
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
            previewRenderers = currentPreview.GetComponentsInChildren<MeshRenderer>();
            appliedValidMaterial = null;
            previewRequirements = currentPreview.GetComponentInChildren<SpawnRequirements>();

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
        if (currentPreview == null || previewRenderers == null) return;

        // only swap when validity changes
        if (appliedValidMaterial == isPlacementValid) return;
        appliedValidMaterial = isPlacementValid;

        Material tint = isPlacementValid ? valid : invalid;

        foreach (MeshRenderer meshRenderer in previewRenderers)
        {
            if (meshRenderer == null) continue;

            // every slot, so multi-material models are tinted completely
            Material[] materials = meshRenderer.sharedMaterials;
            for (int i = 0; i < materials.Length; i++) materials[i] = tint;
            meshRenderer.sharedMaterials = materials;
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
        // affordability is checked again here: resources may have changed since the last placement check
        if (ReturnIsValidPlacement() && CanAffordPreview())
        {
            gameManager.EconomyManager.SpendBuildCost(previewRequirements);

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

    // previews without a SpawnRequirements are free
    private bool CanAffordPreview()
    {
        return previewRequirements == null || gameManager.EconomyManager.CanAfford(previewRequirements);
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