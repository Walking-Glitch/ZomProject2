using System.Collections;
using Assets.Scripts.Player.Actions;
using Assets.Scripts.Player.Weapon;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.InputSystem;
public class AimStateManager : MonoBehaviour 
{

    // recoil test 
    [Header("Recoil")]
    [Tooltip("How far the aim point kicks per rifle shot. Higher = less accurate full-auto.")]
    [SerializeField] private float recoilAmount = 0.1f; // Magnitude of the recoil
    [Tooltip("How fast the aim point settles back after a shot.")]
    [SerializeField] private float recoilDecaySpeed = 5f; // Speed at which recoil dissipates
    private Vector3 recoilOffset; // Current recoil offset


    // camera rotation
    private Vector2 lookInput;
    [Header("Aim")]
    [Tooltip("How fast the character body turns to face where the camera aims.")]
    [SerializeField] private float aimRotationSpeed;

    private InputSystem_Actions actionSystem;

    // camera aim
    public Transform aimPos;
    public Transform laserPos;
    [Tooltip("What the crosshair aim point can land on: ground, scenery, defenses and zombies. Not the player.")]
    [SerializeField] private LayerMask aimMask;
    [Tooltip("How far (m) the crosshair ray reaches; aiming at the sky puts the aim point this far out.")]
    [SerializeField] private float maxAimDistance = 150f;
    [Tooltip("Closest distance (m) from the camera the arms and gun aim at. Higher = less gun swing on close objects, but the laser leaves the muzzle at more of an angle. Bullets and the laser dot still use the real aim point.")]
    [SerializeField] private float minPoseDistance = 8f;
    [Tooltip("Laser dot diameter (m). Matches the visible laser beam, so like the beam it looks bigger up close and smaller far away.")]
    [SerializeField] private float laserDotSize = 0.008f;
    [Tooltip("Smallest the laser dot gets on screen, as diameter per metre from the camera, so it stays visible at range.")]
    [SerializeField] private float laserDotMinSizePerMetre = 0.002f;
    // what's really under the crosshair; bullets, grenades and the laser use this, the rig uses aimPos
    public Vector3 AimPoint { get; private set; }
    [SerializeField] private LayerMask laserMask;

    // camera zoom
    [Tooltip("How fast the aim point follows the crosshair. Lower = smoother but laggier aim.")]
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
    public CinemachineCamera DefaultCamera;
    public CinemachineCamera ZoomedCamera;
    public CinemachinePanTilt cineDefaultTilt;
    public CinemachinePanTilt cineZoomedTilt;

    // blend variables
    public float blendDuration; // Duration for blending layers

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

        // aimPos can sit past what the crosshair is on, so only the laser dot is drawn
        AimPoint = aimPos.position;
        aimPos.GetComponent<MeshRenderer>().enabled = false;
        laserPos.GetComponent<MeshRenderer>().enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        SwitchState(AimIdleState);
    }

    private void Update()
    {
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

    public void AdjustConstraintWeight()
    {
        if (isTransitioning) return;

        if (actionStateManager.CurrentState == actionStateManager.Reload || actionStateManager.CurrentState == actionStateManager.Grenade)
        {
            // the reload blend re-runs while the animation plays (its hand IK timing relies on it)
            lastPoseBlend = PoseBlend.None;
            actionStateManager.TransitionToReload();
            return;
        }

        // hip and aim poses only blend when the target changes, instead of restarting a no-op blend every frame
        PoseBlend target = CurrentState == AimingState ? PoseBlend.Shooting : PoseBlend.Main;
        if (target == lastPoseBlend) return;

        lastPoseBlend = target;

        if (target == PoseBlend.Shooting) TransitionFromMainToShootingLayer();
        else TransitionFromShootingToMainLayer();
    }

    private enum PoseBlend { None, Main, Shooting }
    private PoseBlend lastPoseBlend = PoseBlend.None;

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
        Vector2 screenCentre = new Vector2(Screen.width / 2, Screen.height / 2);
        Ray ray = Camera.main.ScreenPointToRay(screenCentre);
        
        // the aim point lands on whatever is under the crosshair (zombies, defenses, scenery, ground); the gun sits
        // lower than the camera, so aiming past a zombie at the ground behind it made close shots miss high or low
        Vector3 aimPoint = Physics.Raycast(ray, out RaycastHit hit, maxAimDistance, aimMask, QueryTriggerInteraction.Ignore)
            ? hit.point
            : ray.GetPoint(maxAimDistance); // sky: aim far along the crosshair instead of keeping a stale point

        AimPoint = Vector3.Lerp(AimPoint, aimPoint + recoilOffset, aimSmoothSpeed * Time.deltaTime);

        // the rig aims at aimPos: same direction from the camera, but never close enough to swing the gun toward near objects
        Vector3 fromCamera = AimPoint - ray.origin;
        aimPos.position = ray.origin + fromCamera.normalized * Mathf.Max(fromCamera.magnitude, minPoseDistance);

        // the laser stops at the aim point, or earlier if something blocks the muzzle's line to it
        Vector3 laserDirection = AimPoint - weaponLaser.laserOrigin.position;
        Ray ray2 = new Ray(weaponLaser.laserOrigin.position, laserDirection);
        float laserLength = laserDirection.magnitude + 0.1f;

        laserPos.position = Physics.Raycast(ray2, out RaycastHit hit2, laserLength, laserMask, QueryTriggerInteraction.Ignore)
            ? hit2.point
            : AimPoint;

        // a fixed real-world size like the beam, until distance would shrink it too small to see
        float laserDotDistance = Vector3.Distance(ray.origin, laserPos.position);
        laserPos.localScale = Vector3.one * Mathf.Max(laserDotSize, laserDotDistance * laserDotMinSizePerMetre);

        // Gradually reduce the recoil offset over time
        recoilOffset = Vector3.Lerp(recoilOffset, Vector3.zero, recoilDecaySpeed * Time.deltaTime);
    }
     
    private void CharacterRotation()
    {
        // the camera follows the aim state, so looking at the sky can't leave it stuck zoomed in or out
        bool zoomed = CurrentState != AimIdleState || actionStateManager.CurrentState == actionStateManager.Reload || actionStateManager.CurrentState == actionStateManager.Grenade;

        if (zoomed) SwitchToZoomedCamera();
        else SwitchToDefaultCamera();

        // Cinemachine keeps resetting the input smoothing of the camera that isn't live, so its axes fall behind: copy them from the live one
        CinemachinePanTilt liveTilt = zoomed ? cineZoomedTilt : cineDefaultTilt;
        CinemachinePanTilt standbyTilt = zoomed ? cineDefaultTilt : cineZoomedTilt;
        standbyTilt.PanAxis.Value = liveTilt.PanAxis.Value;
        standbyTilt.TiltAxis.Value = liveTilt.TiltAxis.Value;

        Quaternion cameraYaw = Quaternion.Euler(0, liveTilt.PanAxis.Value, 0);

        // the cameras sit relative to the holster: facing the view keeps them behind it, instead of swinging around after the body
        DefaultCamera.Target.TrackingTarget.rotation = cameraYaw;

        // the body smoothly turns to face the same way as the camera
        transform.rotation = Quaternion.Slerp(transform.rotation, cameraYaw, (zoomed ? aimRotationSpeed : 5) * Time.deltaTime);
    }

    // call after moving the player instantly, so the cameras jump with it instead of damping across the map
    public void NotifyTeleported(Vector3 positionDelta)
    {
        CinemachineCore.OnTargetObjectWarped(DefaultCamera.Target.TrackingTarget, positionDelta);
    }

    private void SetLookInputEnabled(bool isEnabled)
    {
        foreach (CinemachineCamera cinemachineCamera in new[] { DefaultCamera, ZoomedCamera })
        {
            if (cinemachineCamera != null && cinemachineCamera.TryGetComponent(out CinemachineInputAxisController lookInputController))
            {
                lookInputController.enabled = isEnabled;
            }
        }
    }

    public void SwitchToDefaultCamera()
    {
        DefaultCamera.Priority = 10;
        ZoomedCamera.Priority = 5;
    }

    public void SwitchToZoomedCamera()
    {
       
        DefaultCamera.Priority = 5;
        ZoomedCamera.Priority = 10;
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
        SetLookInputEnabled(true);
    }

    // these read input on their own and ignore time scale, so the view would keep turning while dead or on the results screen
    void OnDisable()
    {
        actionSystem.Disable();
        SetLookInputEnabled(false);
    }

}
