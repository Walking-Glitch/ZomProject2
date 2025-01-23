using UnityEngine;

public class RotateBuildCanvas : MonoBehaviour
{
    private Transform camTransform;

    private void Start()
    {
        camTransform = Camera.main.transform;
    }

    private void LateUpdate()
    {
        // Make the Canvas face the camera
        transform.LookAt(transform.position + camTransform.rotation * Vector3.forward,
                         camTransform.rotation * Vector3.up);
    }
}
