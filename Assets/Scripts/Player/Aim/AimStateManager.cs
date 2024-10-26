using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class AimStateManager : MonoBehaviour 
{
    // camera rotation
    private Vector2 lookInput;
    private float xAxis, yAxis;
    public float mouseSense = 1.0f;

    private InputSystem_Actions actionSystem;

    // camera aim
    public Transform aimPos;
    [SerializeField] private LayerMask aimMask; 

    // camera zoom
    [SerializeField] private float aimSmoothSpeed; 

    //
    private CinemachinePanTilt cinemachinePanTilt;


    private void Awake()
    {
        actionSystem = new InputSystem_Actions();
       // actionSystem.Player.Look.performed += OnLookPerformed;
       //actionSystem.Player.Look.canceled += OnLookCancelled;
        actionSystem.Enable();
    }

    private void Start()
    {
        cinemachinePanTilt = GetComponentInChildren<CinemachinePanTilt>();
    }

    private void Update()
    {
        MoveAimReference();
        RotatePlayer();
    }

     
    private void RotatePlayer()
    {
        Vector3 direction = aimPos.position - transform.position;

        
        Quaternion targetRotation = Quaternion.LookRotation(direction);

         
        transform.rotation = Quaternion.Euler(0, targetRotation.eulerAngles.y, 0);
    }

     
    private void MoveAimReference()
    {
        Vector2 screenCentre = new Vector2(Screen.width / 2, Screen.height / 2);
        Ray ray = Camera.main.ScreenPointToRay(screenCentre);

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, aimMask))
        {
            aimPos.position = Vector3.Lerp(aimPos.position, hit.point, aimSmoothSpeed *Time.deltaTime);
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
