using System.Collections;
using Assets.Scripts.Player.Actions;
using Assets.Scripts.Player.Weapon;
using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.InputSystem;
public class AimStateManager : NetworkBehaviour
{

    // recoil test 
    [SerializeField] private float recoilAmount = 0.1f; // Magnitude of the recoil
    [SerializeField] private float recoilDecaySpeed = 5f; // Speed at which recoil dissipates
    private Vector3 recoilOffset; // Current recoil offset


    // camera rotation
    private Vector2 lookInput;
    private Vector3 direction;
    [SerializeField] private float aimRotationSpeed;

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

    // network variables
    private NetworkVariable<float> layer1Weight = new NetworkVariable<float>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<float> rightHandAimWeight = new NetworkVariable<float>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<float> leftHandHintWeight = new NetworkVariable<float>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<float> leftHandHintWeightData = new NetworkVariable<float>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server); 

    private NetworkVariable<Vector3> aimPosition = new NetworkVariable<Vector3>(Vector3.zero, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private NetworkVariable<Vector3> laserPosition = new NetworkVariable<Vector3>(Vector3.zero, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);



    public override void OnNetworkSpawn()
    {
        Initialize();

        if (IsOwner)
        {
             
            UpdateLayerWieghtServerRpc(anim.GetLayerWeight(1));
            UpdateLeftHintWeightServerRpc(LeftHandIKConstraint.weight);
            UpdateRightHandAimWeightServerRpc(RightHandAimConstraint.weight);
            UpdateLeftHintWeightDataServerRpc(LeftHandIKConstraint.data.hintWeight);

        }


        else
        {           
            DefaultCamera.gameObject.SetActive(false);
            ZoomedCamera.gameObject.SetActive(false);
        }


        //Force update on new players
        anim.SetLayerWeight(1, layer1Weight.Value);
        LeftHandIKConstraint.data.hintWeight = leftHandHintWeight.Value;
        RightHandAimConstraint.weight = rightHandAimWeight.Value;

        layer1Weight.OnValueChanged += (prev, curr) =>
        {
            //Debug.Log($"Layer1Weight changed from {prev} to {curr}");
            anim.SetLayerWeight(1, curr);
        };
        leftHandHintWeight.OnValueChanged += (prev, curr) => 
        {
            //Debug.Log($"LeftHandHintWeight changed from {prev} to {curr}");
            LeftHandIKConstraint.weight = curr;
        };
        rightHandAimWeight.OnValueChanged += (prev, curr) =>
        {
            //Debug.Log($"RightHandAimWeight changed from {prev} to {curr}");
            RightHandAimConstraint.weight = curr;
        };
        leftHandHintWeightData.OnValueChanged += (prev, curr) =>
        {
            //Debug.Log($"LeftHandHintWeightData changed from {prev} to {curr}");
            LeftHandIKConstraint.data.hintWeight = curr;
        };

        aimPosition.OnValueChanged += (prev, curr) =>
        {
            aimPos.position = curr;
        };

        laserPosition.OnValueChanged += (prev, curr) =>
        {
            laserPos.position = curr;  
        };
    }

    private void Start()
    {
        if(!IsServer && !IsClient)
        {
            Initialize();
        }
    }

    private void Update()
    {
        //Debug.Log($"Before IsOwner check: IsClient = {IsClient}, IsServer = {IsServer}, IsOwner = {IsOwner}, Owner of this object: {GetComponent<NetworkObject>().OwnerClientId}, Local Client ID: {NetworkManager.Singleton.LocalClientId}");
        //if (IsOwner)
        //{
        //    Debug.Log("Passed IsOwner check!");             
        //}
        //else
        //{
        //    Debug.Log("Failed IsOwner check.");
        //}

        if (!IsOwner) return;

        MoveAimReference();
        CharacterRotation();
        CurrentState.UpdateState(this);
        AdjustConstraintWeight();
        //Debug.Log(CurrentState);
    }

    public void SwitchState(AimStateBase state)
    {
        CurrentState = state;
        state.EnterState(this);
    }

    private void Initialize()
    {

        actionSystem = new InputSystem_Actions();
        actionSystem.Player.Look.performed += OnLookPerformed;
        actionSystem.Player.Look.canceled += OnLookCancelled;

        actionSystem.Player.Aim.performed += OnAimingPerformed;
        actionSystem.Player.Aim.canceled += OnAimingCancelled;
        actionSystem.Enable();



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
    public void AdjustConstraintWeight() //originall
    {
        if(!IsServer && !IsClient)
        {
            if (isTransitioning) return;

            if (actionStateManager.CurrentState == actionStateManager.Reload || actionStateManager.CurrentState == actionStateManager.Grenade)
            {
                actionStateManager.TransitionToReload();
            }

            else if (CurrentState == AimingState && actionStateManager.CurrentState != actionStateManager.Grenade)
            {
                TransitionFromMainToShootingLayer();
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

        else 
        {
            if (isTransitioning) return;

            if (actionStateManager.CurrentState == actionStateManager.Reload || actionStateManager.CurrentState == actionStateManager.Grenade)
            {
                actionStateManager.TransitionToReload();
            }
                        
            else if (CurrentState == AimingState && actionStateManager.CurrentState == actionStateManager.Default)
            {                
                TransitionFromMainToShootingLayer();
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

     
    [ServerRpc]
    private void UpdateLayerWieghtServerRpc(float layerWeight)
    {
        layer1Weight.Value = layerWeight;
    }

    [ServerRpc]
    private void UpdateLeftHintWeightServerRpc(float leftHandHintWeightValue)
    {
        leftHandHintWeight.Value = leftHandHintWeightValue;
    }

    [ServerRpc]
    private void UpdateLeftHintWeightDataServerRpc(float leftHandHintWeightDataValue)
    {
        leftHandHintWeightData.Value = leftHandHintWeightDataValue;
    }

    [ServerRpc]
    private void UpdateRightHandAimWeightServerRpc(float rightHandWeight)
    {
        rightHandAimWeight.Value = rightHandWeight;
    }


    private IEnumerator FadeIntoMainLayer()
    {
        if (!IsServer && !IsClient) 
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
        else if(IsOwner)
        {
            //Debug.Log((IsClient) +"we areOWNERS and fading to main");
            //Debug.Log("IsOnwer inside fading to main");
            isTransitioning = true;
            float startWeight = layer1Weight.Value;
            float leftHandStartWeightData = leftHandHintWeightData.Value;
            float rightHandStartWeight = rightHandAimWeight.Value;
            float elapsed = 0;

            while (elapsed < blendDuration)
            {
                elapsed += Time.deltaTime;
                float newWeight = Mathf.Lerp(startWeight, 0, elapsed / blendDuration);
                UpdateLayerWieghtServerRpc(newWeight);  

                float newHintWeightData = Mathf.Lerp(leftHandStartWeightData, 0, elapsed / blendDuration);
                UpdateLeftHintWeightDataServerRpc(newHintWeightData);

                float newWeighRightHand = Mathf.Lerp(rightHandStartWeight, 0, elapsed / blendDuration);
                UpdateRightHandAimWeightServerRpc(newWeighRightHand);

              
                yield return null;
            }

            UpdateLayerWieghtServerRpc(0);
            UpdateLeftHintWeightServerRpc(1);
            UpdateLeftHintWeightDataServerRpc(0);
            UpdateRightHandAimWeightServerRpc(0);

            
            isTransitioning = false;
        }

    }


    private IEnumerator FadeIntoUpperBodyLayer()
    {
        if(!IsServer && !IsClient)
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

        else if (IsOwner)
        {
            //Debug.Log(IsClient);
            //Debug.Log("we are transitioning from main to shoot");
            isTransitioning = true;  // Set the flag to true
            float startWeight = layer1Weight.Value;
            float leftHandHintWeightDataStartValue = leftHandHintWeightData.Value;
            float leftHandWeightStartValue = leftHandHintWeight.Value;
            float rightHandStartWeight = rightHandAimWeight.Value;
            float elapsed = 0;

            while (elapsed < blendDuration)
            {
                elapsed += Time.deltaTime;
                float newWeight = Mathf.Lerp(startWeight, 1, elapsed / blendDuration);
                UpdateLayerWieghtServerRpc(newWeight);

                float newWeighTwoBoneIk = Mathf.Lerp(leftHandWeightStartValue, 1, elapsed / blendDuration);
                UpdateLeftHintWeightServerRpc(newWeighTwoBoneIk);

                float newWeighDataTwoBoneIk = Mathf.Lerp(leftHandHintWeightDataStartValue, 1, elapsed / blendDuration);
                UpdateLeftHintWeightDataServerRpc(newWeighDataTwoBoneIk);

                float newWeighRightHand = Mathf.Lerp(rightHandStartWeight, 1, elapsed / blendDuration);
                UpdateRightHandAimWeightServerRpc(newWeighRightHand);

                yield return null; // Wait for the next frame
            }

            // Set final values
            UpdateLayerWieghtServerRpc(1);
            UpdateLeftHintWeightServerRpc(1);
            UpdateLeftHintWeightDataServerRpc(1);
            UpdateRightHandAimWeightServerRpc(1);

            //Debug.Log("we FINISHED THE transitioning from main to shoot");

            UpdateLeftHintWeightServerRpc(1);
            isTransitioning = false;  // Reset the flag when done
        }



    }


    public void AddRecoil()
    {
        //TargetTransform.position += new Vector3(10f, 10f, 0);
        float recoilX = Random.Range(-recoilAmount, recoilAmount);
        float recoilY = Random.Range(0.2f, recoilAmount);
        float recoilZ = Random.Range(-recoilAmount / 2, 0); // Optional: minor backward recoil
        recoilOffset += new Vector3(recoilX, recoilY, recoilZ);
    }
    private void MoveAimReference()
    {
        if (!IsServer && !IsClient)
        {
            Vector2 screenCentre = new Vector2(Screen.width / 2, Screen.height / 2);
            Ray ray = Camera.main.ScreenPointToRay(screenCentre);

            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, aimMask))
            {
                Vector3 targetPosition = hit.point + recoilOffset; // Apply recoil offset
                                                                   //aimPos.position = Vector3.Lerp(aimPos.position, hit.point, aimSmoothSpeed * Time.deltaTime);
                aimPos.position = Vector3.Lerp(aimPos.position, targetPosition, aimSmoothSpeed * Time.deltaTime);
            }

            Vector3 laserDirection = (aimPos.position - weaponLaser.laserOrigin.position);
            Ray ray2 = new Ray(weaponLaser.laserOrigin.position, laserDirection);

            if (Physics.Raycast(ray2, out RaycastHit hit2, Mathf.Infinity, laserMask))
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

            // Gradually reduce the recoil offset over time
            recoilOffset = Vector3.Lerp(recoilOffset, Vector3.zero, recoilDecaySpeed * Time.deltaTime);
        }

        else if (IsOwner)
        {
            Vector2 screenCentre = new Vector2(Screen.width / 2, Screen.height / 2);
            Ray ray = Camera.main.ScreenPointToRay(screenCentre);

            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, aimMask))
            {
                Vector3 targetPosition = hit.point + recoilOffset; // Apply recoil offset
                                                                   //aimPos.position = Vector3.Lerp(aimPos.position, hit.point, aimSmoothSpeed * Time.deltaTime);
                aimPosition.Value = Vector3.Lerp(aimPos.position, targetPosition, aimSmoothSpeed * Time.deltaTime);

                 
            }

            Vector3 laserDirection = (aimPosition.Value - weaponLaser.laserOrigin.position);
            Ray ray2 = new Ray(weaponLaser.laserOrigin.position, laserDirection);

            if (Physics.Raycast(ray2, out RaycastHit hit2, Mathf.Infinity, laserMask))
            {
                aimPos.gameObject.GetComponent<MeshRenderer>().enabled = false;

                laserPos.gameObject.GetComponent<MeshRenderer>().enabled = true;
                laserPosition.Value = hit2.point;
                IsOnTarget = true;
            }
            else
            {
                aimPos.gameObject.GetComponent<MeshRenderer>().enabled = true;

                laserPos.gameObject.GetComponent<MeshRenderer>().enabled = false;
                IsOnTarget = false;
            }

            // Gradually reduce the recoil offset over time
            recoilOffset = Vector3.Lerp(recoilOffset, Vector3.zero, recoilDecaySpeed * Time.deltaTime);
        }
    }
     
    private void CharacterRotation()
    {
        {
            Vector2 screenCentre = new Vector2(Screen.width / 2, Screen.height / 2);
            Ray ray = Camera.main.ScreenPointToRay(screenCentre);

            // rotate when not aiming 
            if (Physics.Raycast(ray, out RaycastHit hit3, Mathf.Infinity, allMask) && CurrentState == AimIdleState && actionStateManager.CurrentState != actionStateManager.Reload && actionStateManager.CurrentState != actionStateManager.Grenade)
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
        if(actionSystem != null)
            actionSystem.Enable();
    }

    void OnDisable()
    {
        if (actionSystem != null)
            actionSystem.Disable();
    }

}
