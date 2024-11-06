using System.Collections;
using System.ComponentModel;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.InputSystem;
using UnityEngine.Windows;
using static UnityEditor.SceneView;

public class AimStateManager : MonoBehaviour 
{
     

    // camera rotation
    private Vector2 lookInput;
    private Vector3 direction;
    [SerializeField] private float aimRotationSpeed;

    private bool cameraAcheck;
    private bool cameraBcheck;

    private InputSystem_Actions actionSystem;

    // camera aim
    public Transform aimPos;
    public Transform laserPos;
    [SerializeField] private LayerMask aimMask;
    [SerializeField] private LayerMask laserMask;
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
    [HideInInspector] public bool isTransitioning = false;
     

    // Constraint bones 
    [HideInInspector] public TwoBoneIKConstraint LeftHandIKConstraint;
    public MultiAimConstraint HeadAimConstraint;
    public MultiAimConstraint TorsoAimConstraint;
    public MultiAimConstraint RightHandAimConstraint;

    // reference to action state manager
    [HideInInspector] public ActionStateManager actionStateManager;

    // weapon variables
    [HideInInspector] public WeaponManager WeaponManager;

    // cinemachine references
    private CinemachineBrain brain;
    public CinemachineCamera DefaultCamera;
    public CinemachineCamera ZoomedCamera;
    public CinemachinePanTilt cineDefaultTilt;
    public CinemachinePanTilt cineZoomedTilt;

    // blend variables
    public float blendDuration; // Duration for blending layers

    // bool to set laser
    [HideInInspector] public bool IsOnTarget;
    private WeaponLaser weaponLaser;

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
        weaponLaser = GetComponent<WeaponLaser>();
        anim = GetComponent<Animator>();
        WeaponManager = GetComponent<WeaponManager>();
        actionStateManager = GetComponent<ActionStateManager>();
        movementStateManager = GetComponent<MovementStateManager>();
        LeftHandIKConstraint = GetComponentInChildren<TwoBoneIKConstraint>();

        brain = Camera.main.GetComponent<CinemachineBrain>();
        
        Cursor.lockState = CursorLockMode.Locked;
        SwitchState(AimIdleState);
    }

    private void Update()
    {
        MoveAimReference();
        CharacterRotation();
        CurrentState.UpdateState(this);
        AdjustConstraintWeight();
    }

    public void SwitchState(AimStateBase state)
    {
        CurrentState = state;
        state.EnterState(this);
    }

    public void AdjustConstraintWeight()
    {
        if (isTransitioning) return;

        if (actionStateManager.CurrentState == actionStateManager.Reload)
        {
            actionStateManager.TransitionToReload();
        }
        
        else if (CurrentState == AimingState && actionStateManager.CurrentState != actionStateManager.Reload)
        {
            TransitionFromMainToShootingLayer();
        }

        else 
        {
            TransitionFromShootingToMainLayer();
        }
      
    }

    public void TransitionFromShootingToMainLayer()
    {
        if (!isTransitioning)
        {
            StartCoroutine(FadeIntoMainLayer());
        }
    }

    public void TransitionFromMainToShootingLayer()
    {
        if (!isTransitioning)
        {
            StartCoroutine(FadeIntoUpperBodyLayer());
        }
    }


    private IEnumerator FadeIntoMainLayer()
    {
        isTransitioning = true;  // Set the flag to true
        float startWeight = anim.GetLayerWeight(1);
        float leftHandStartWeight = LeftHandIKConstraint.data.hintWeight;
        float rightHandStartWeight = RightHandAimConstraint.weight;
        float elapsed = 0;

        while (elapsed < blendDuration)
        {
            elapsed += Time.deltaTime;
            float newWeight = Mathf.Lerp(startWeight, 0, elapsed / blendDuration);
            anim.SetLayerWeight(1, newWeight);

            float newHintWeight = Mathf.Lerp(leftHandStartWeight, 0, elapsed / blendDuration);
            LeftHandIKConstraint.data.hintWeight = newHintWeight;

            float newWeighRightHand = Mathf.Lerp(rightHandStartWeight, 0, elapsed / blendDuration);
            RightHandAimConstraint.weight = newWeighRightHand;

            yield return null;
        }

        anim.SetLayerWeight(1, 0); // Fully transitioned to main layer
        LeftHandIKConstraint.weight = 1;
        LeftHandIKConstraint.data.hintWeight = 0;
        RightHandAimConstraint.weight = 0f;

        isTransitioning = false;
    }

    private IEnumerator FadeIntoUpperBodyLayer()
    {
        isTransitioning = true;  // Set the flag to true
        float startWeight = anim.GetLayerWeight(1);
        float leftHandStartWeight = LeftHandIKConstraint.data.hintWeight;
        float rightHandStartWeight = RightHandAimConstraint.weight;
        float elapsed = 0;

        while (elapsed < blendDuration)
        {
            elapsed += Time.deltaTime;
            float newWeight = Mathf.Lerp(startWeight, 1, elapsed / blendDuration);
            anim.SetLayerWeight(1, newWeight);

            float newWeighTwoBoneIk = Mathf.Lerp(leftHandStartWeight, 1, elapsed / blendDuration);
            LeftHandIKConstraint.data.hintWeight = newWeighTwoBoneIk;

            float newWeighRightHand = Mathf.Lerp(rightHandStartWeight, 1, elapsed / blendDuration);
            RightHandAimConstraint.weight = newWeighRightHand;

            yield return null; // Wait for the next frame
        }

        // Set final values
        anim.SetLayerWeight(1, 1);
        LeftHandIKConstraint.weight = 1;
        LeftHandIKConstraint.data.hintWeight = 1;
        RightHandAimConstraint.weight = 1;

        isTransitioning = false;  // Reset the flag when done
    }

    private void MoveAimReference()
    {
        Vector2 screenCentre = new Vector2(Screen.width / 2, Screen.height / 2);
        Ray ray = Camera.main.ScreenPointToRay(screenCentre);
        
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, aimMask))
        {
            aimPos.position = Vector3.Lerp(aimPos.position, hit.point, aimSmoothSpeed * Time.deltaTime);
        }

        //if (Physics.Raycast(ray, out RaycastHit hit2, Mathf.Infinity, laserMask))
        //{
        //    aimPos.gameObject.GetComponent<MeshRenderer>().enabled = false;

        //    laserPos.gameObject.GetComponent<MeshRenderer>().enabled = true;
        //    laserPos.position = hit2.point;
        //    IsOnTarget = true;
        //}

        Vector3 laserDirection = (aimPos.position - weaponLaser.laserOrigin.position);
        Ray ray2 = new Ray(weaponLaser.laserOrigin.position, laserDirection );

        if (Physics.Raycast( ray2  , out RaycastHit hit2, Mathf.Infinity, laserMask))
        {
            aimPos.gameObject.GetComponent<MeshRenderer>().enabled = false;

            laserPos.gameObject.GetComponent<MeshRenderer>().enabled = true;
            laserPos.position = hit2.point;
            IsOnTarget = true;
        }
        else
        {
            aimPos.gameObject.GetComponent<MeshRenderer>().enabled = true;

            laserPos.gameObject.GetComponent<MeshRenderer>().enabled = false;
            IsOnTarget = false;
        }
    }
     
    private void CharacterRotation()
    {
        {
            Vector2 screenCentre = new Vector2(Screen.width / 2, Screen.height / 2);
            Ray ray = Camera.main.ScreenPointToRay(screenCentre);

            // rotate when not aiming 
            if (Physics.Raycast(ray, out RaycastHit hit3, Mathf.Infinity, allMask) && CurrentState == AimIdleState && actionStateManager.CurrentState != actionStateManager.Reload)
            {
                SwitchToDefaultCamera();

                // Calculate the direction from the character to the hit point
                direction = hit3.point - transform.position;
                direction.y = 0; // Keep the rotation on the y-axis only

                // Calculate the target rotation and smoothly rotate the character towards it
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 5 * Time.deltaTime);

            }

            // Rotate the character during aiming 
            else
            {
                if (Physics.Raycast(ray, out RaycastHit hit2, Mathf.Infinity, allMask))

                {
                    SwitchToZoomedCamera();

                    // Calculate the direction from the character to the hit point
                    direction = hit3.point - transform.position;
                    direction.y = 0; // Keep the rotation on the y-axis only

                    // Calculate the target rotation and smoothly rotate the character towards it
                    Quaternion targetRotation = Quaternion.LookRotation(direction);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation,
                        aimRotationSpeed * Time.deltaTime);
                }
            }
        }
    }

    public void SwitchToDefaultCamera()
    {
        DefaultCamera.Priority = 10;
        ZoomedCamera.Priority = 5;
        if (brain.IsBlending)
        {
            cineDefaultTilt.PanAxis = cineZoomedTilt.PanAxis;
        }
    }

    public void SwitchToZoomedCamera()
    {
       
        DefaultCamera.Priority = 5;
        ZoomedCamera.Priority = 10;
        
        if (brain.IsBlending)
        {
            cineZoomedTilt.PanAxis = cineDefaultTilt.PanAxis;
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
