using Assets.Scripts.Game_Manager;
using UnityEngine;
using static Pathfinding.Drawing.Palette;

public class BuildManager : MonoBehaviour
{
    public GameObject[] turretPrefabs; // Array of turret prefabs
    public GameObject[] fakeTurretPrefabs;
    public LayerMask placementLayer;
    public LayerMask placementObstacle;// Define valid layers for placement
    public Material validMaterial;
    public Material invalidMaterial;

    public Transform AimTransform;

    private GameObject currentPreview;
    private GameObject selectedTurret;
    [SerializeField] public bool isPlacing = false;
    [SerializeField] private bool isPlacementValid;

    private GameManager gameManager;

    private int currentIndex = 0;

    [SerializeField] private float radius;

    private void Start()
    {
        gameManager = GameManager.Instance;
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

                DisplaySelectedPrefab();

                 radius = GetPrefabRadius(currentPreview);

                if (currentPreview == null) return;

                
                Collider[] colliders = Physics.OverlapSphere(currentPreview.transform.position, radius, placementObstacle);             

                

                if (colliders.Length > 0)
                {
                    // Placement is invalid if any colliders are found
                    SetIsValidPlacement(false);

                    // Debug the colliders found inside the sphere
                    Debug.Log("Colliders found inside the sphere:");
                    foreach (Collider collider in colliders)
                    {
                        Debug.Log($"Collider Name: {collider.name}, Tag: {collider.tag}, Layer: {LayerMask.LayerToName(collider.gameObject.layer)}");
                    }
                }
                else { 

                    SetIsValidPlacement(true); 
                }
               
            }
            else
            {
                SetIsValidPlacement(false);
            }
        }

        else
        {
            //Debug.Log($"Hit: {hit.collider.name}, Layer: {LayerMask.LayerToName(hit.collider.gameObject.layer)}");

            SetIsValidPlacement(false);
        }


    }

    public void DisplaySelectedPrefab()
    {

        if (!isPlacing)
        {
            currentPreview = Instantiate(fakeTurretPrefabs[currentIndex], AimTransform.position, Quaternion.identity);
            currentPreview.transform.SetParent(AimTransform);

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

    public void SwitchSelectedPrefab(float scrollDelta)
    {

        if (scrollDelta > 0)
        {
            currentIndex++;
            if (currentIndex >= turretPrefabs.Length)
            {
                currentIndex = 0;
            }

        }
        else if (scrollDelta < 0)
        {
            currentIndex--;
            if (currentIndex < 0)
            {
                currentIndex = turretPrefabs.Length - 1;
            }
        }

        Destroy(currentPreview);
        isPlacing = false;

    }

    public void PlacePrefab()
    {
        if (ReturnIsValidPlacement())
        {

            selectedTurret = Instantiate(turretPrefabs[currentIndex], AimTransform.position, Quaternion.identity);
            selectedTurret.transform.SetParent(AimTransform);

            selectedTurret.transform.SetParent(null);
            DestroyPreview();
            isPlacing = false;
        }


    }

    private float GetPrefabRadius(GameObject prefab)
    {
        if (prefab == null)
            return 10f; // Default radius if no prefab exists

        Bounds bounds = new Bounds(prefab.transform.position, Vector3.zero);

        // Combine bounds of all child MeshRenderers
        foreach (var meshRenderer in prefab.GetComponentsInChildren<MeshRenderer>())
        {
            bounds.Encapsulate(meshRenderer.bounds);
        }

        // Use the largest dimension of the bounds as the radius
        return bounds.extents.magnitude*0.7f;
    }

    public void DestroyPreview()
    {
        currentIndex = 0;
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
        Gizmos.DrawWireSphere(currentPreview.transform.position, radius);


    }
}