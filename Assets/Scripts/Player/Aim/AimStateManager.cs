using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

public class AimStateManager : MonoBehaviour 
{
     

    // camera rotation
    private Vector2 lookInput;
    public float mouseSense = 1.0f;
    [SerializeField] private float aimRotationSpeed;

    private InputSystem_Actions actionSystem;

    // camera aim
    public Transform aimPos;
    [SerializeField] private LayerMask aimMask;
    [SerializeField] private LayerMask allMask;

    // camera zoom
    [SerializeField] private float aimSmoothSpeed;

    // states
    public AimStateBase CurrentState;
    public AimHipState AimIdleState = new AimHipState();
    public AimingState AimingState = new AimingState();

    [HideInInspector] public MovementStateManager movementStateManager;

    // animator
    [HideInInspector] public Animator anim;

    // Constraint bones 
    [HideInInspector] public TwoBoneIKConstraint LeftHandIKConstraint;
    public MultiAimConstraint HeadAimConstraint;
    public MultiAimConstraint TorsoAimConstraint;
    public MultiAimConstraint RightHandAimConstraint;


    private void Awake()
    {
        actionSystem = new InputSystem_Actions();
        actionSystem.Player.Look.performed += OnLookPerformed;
        actionSystem.Player.Look.canceled += OnLookCancelled;

        actionSystem.Player.Aim.performed += OnAimingPerformed;
        actionSystem.Player.Aim.canceled += OnAimingCancelled;
        actionSystem.Enable();
    }

    private void Start()
    {
        anim = GetComponent<Animator>();
        movementStateManager = GetComponent<MovementStateManager>();
        LeftHandIKConstraint = GetComponentInChildren<TwoBoneIKConstraint>();

        Cursor.lockState = CursorLockMode.Locked;
        SwitchState(AimIdleState);
    }

    private void Update()
    {
        MoveAimReferenceAndCharacterRotation();
        CurrentState.UpdateState(this);
    }

    public void SwitchState(AimStateBase state)
    {
        CurrentState = state;
        state.EnterState(this);
    }

    public void AdjustConstraintWeight()
    {
        if (CurrentState == AimIdleState)
        {
            anim.SetLayerWeight(1, 0);
            LeftHandIKConstraint.weight = 1;
            LeftHandIKConstraint.data.hintWeight = 0;
            TorsoAimConstraint.weight = 0;
            RightHandAimConstraint.weight = 0.3f;
        }

        else if (CurrentState == AimingState)
        {
            anim.SetLayerWeight(1, 1);
            LeftHandIKConstraint.weight = 1;
            LeftHandIKConstraint.data.hintWeight = 1;
            TorsoAimConstraint.weight = 1; 
            RightHandAimConstraint.weight = 1;
        }
    }
    private void MoveAimReferenceAndCharacterRotation()
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

    private void OnAimingPerformed(InputAction.CallbackContext context)
    {
        if(movementStateManager.currentState != movementStateManager.Run) SwitchState(AimingState);
    }

    private void OnAimingCancelled(InputAction.CallbackContext context)
    {
        SwitchState(AimIdleState);
    }
    void OnEnable()
    {
        actionSystem.Enable();
    }

    void OnDisable()
    {
        actionSystem.Disable();
    }

}
