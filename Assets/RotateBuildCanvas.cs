using TMPro;
using UnityEngine;

public class RotateBuildCanvas : MonoBehaviour
{
    private Transform camTransform;
    public TextMeshProUGUI PrefabId;

     
    private void Start()
    {
        camTransform = Camera.main.transform;
    }

    private void LateUpdate()
    {
        transform.LookAt(transform.position + camTransform.rotation * Vector3.forward,
                         camTransform.rotation * Vector3.up);
    }

    public void DisplayPrefabRequirments()
    {
        SpawnRequirements spawnRequirements = GetComponentInParent<SpawnRequirements>();

        if (spawnRequirements != null)
        {
            PrefabId.text = spawnRequirements.PrefabName;
        }
    }
}
