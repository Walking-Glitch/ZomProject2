using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

public class AimStateManager : MonoBehaviour 
{
     

    // camera rotation
    private Vector2 lookInput;
    private float xAxis, yAxis;
    public float mouseSense = 1.0f;
    [SerializeField] private float aimRotationSpeed;

    private InputSystem_Actions actionSystem;

    // camera aim
    public Transform aimPos;
    [SerializeField] private LayerMask aimMask;
    [SerializeField] private LayerMask allMask;

    // camera zoom
    [SerializeField] private float aimSmoothSpeed; 

    //
    private CinemachinePanTilt cinemachinePanTilt;
     

     
     
    private void Awake()
    {
        actionSystem = new InputSystem_Actions();
        actionSystem.Player.Look.performed += OnLookPerformed;
        actionSystem.Player.Look.canceled += OnLookCancelled;
        actionSystem.Enable();
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        cinemachinePanTilt = GetComponentInChildren<CinemachinePanTilt>();
       
    }

    private void Update()
    {
        MoveAimReference();
        
    }

 
     

     
    private void MoveAimReference()
    {
        Vector2 screenCentre = new Vector2(Screen.width / 2, Screen.height / 2);
        Ray ray = Camera.main.ScreenPointToRay(screenCentre);

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, aimMask))
        {
            aimPos.position = Vector3.Lerp(aimPos.position, hit.point, aimSmoothSpeed *Time.deltaTime);
        }

        // Rotate the character to face the hit point
        if  (Physics.Raycast(ray, out RaycastHit hit2, Mathf.Infinity, allMask))
        {
            // Calculate the direction from the character to the hit point
            Vector3 direction = hit2.point - transform.position;
            direction.y = 0; // Keep the rotation on the y-axis only

            // Calculate the target rotation and smoothly rotate the character towards it
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, aimRotationSpeed * Time.deltaTime);
           // transform.rotation = targetRotation;
        }
    }
   

    private void OnLookPerformed(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }

    private void OnLookCancelled(InputAction.CallbackContext context)
    {
        lookInput = Vector2.zero;
    }
}
