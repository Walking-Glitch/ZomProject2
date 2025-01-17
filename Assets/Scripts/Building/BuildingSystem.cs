using UnityEngine;

public class BuildingSystem : MonoBehaviour
{

    public GameObject[] turretPrefabs; // Array of turret prefabs
    public LayerMask placementLayer; // Define valid layers for placement
    public Material validMaterial;
    public Material invalidMaterial;

    private GameObject currentPreview;
    private GameObject selectedTurret;
    private bool isPlacing = false;

    void Update()
    {
        if (isPlacing && currentPreview != null)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, placementLayer))
            {
                currentPreview.transform.position = hit.point;
                ValidatePlacement(hit.point);
            }

            if (Input.GetMouseButtonDown(0) && IsPlacementValid())
            {
                PlaceTurret(hit.point);
            }

            if (Input.GetMouseButtonDown(1)) // Cancel placement
            {
                CancelPlacement();
            }
        }
    }

    public void StartPlacement(GameObject turretPrefab)
    {
        selectedTurret = turretPrefab;
        currentPreview = Instantiate(turretPrefab);
        SetPreviewAppearance(currentPreview, validMaterial);
        isPlacing = true;
    }

    private void ValidatePlacement(Vector3 position)
    {
        // Implement logic to check for valid placement
        bool valid = true; // Replace with your logic
        SetPreviewAppearance(currentPreview, valid ? validMaterial : invalidMaterial);
    }

    private bool IsPlacementValid()
    {
        // Implement logic to validate the final placement
        return true; // Replace with your logic
    }

    private void PlaceTurret(Vector3 position)
    {
        Instantiate(selectedTurret, position, Quaternion.identity);
        Destroy(currentPreview);
        isPlacing = false;
    }

    private void CancelPlacement()
    {
        Destroy(currentPreview);
        isPlacing = false;
    }

    private void SetPreviewAppearance(GameObject preview, Material material)
    {
        foreach (Renderer renderer in preview.GetComponentsInChildren<Renderer>())
        {
            renderer.material = material;
        }
    }
}
