using Assets.Scripts.Game_Manager;
using Assets.Scripts.Player.Actions;
using UnityEngine;
using UnityEngine.InputSystem;

public class TruckController : MonoBehaviour
{
    [SerializeField] private WheelCollider frontRight;
    [SerializeField] private WheelCollider frontLeft;
    [SerializeField] private WheelCollider BackRight;
    [SerializeField] private WheelCollider BackLeft;


    [SerializeField] private Transform frontRightTransform;
    [SerializeField] private Transform frontLeftTransform;
    [SerializeField] private Transform backRightTransform;
    [SerializeField] private Transform backLeftTransform;


  

    public float MaxAcceleration;
    public float OutFuelAcceleration;

    public float acceleration;
    public float brakingForce;
    public float maxTurnAngle;

    private float currentAcceleration = 0f;
    private float currentBrakeForce = 0f;
    private float currentTurnAngle = 0f;

    private bool isBraking;

   


   
    public bool isPlayerIn;


    public bool IsGrounded { get; set; }
    public bool IsInvincible { get; set; }

    public MeshCollider CarMeshCollider;

    public Vector3 adjustVectorFrontWheel;
    public Vector3 adjustVectorBackWheel;

    public Rigidbody CarRigidbody;





    private InputSystem_Actions inputSystemActions;

    private GameManager gameManager;

    void Awake()
    {
        inputSystemActions = new InputSystem_Actions();        

        inputSystemActions.Player.Interact.started += OnInteractPerformed;
       
    }

    private void OnEnable()
    {
        inputSystemActions.Enable();
    }
    private void OnDisable()
    {
        inputSystemActions.Disable();
    }
    void Start()
    {
        gameManager = GameManager.Instance;
        
        CarMeshCollider = GetComponent<MeshCollider>();
        CarRigidbody = GetComponent<Rigidbody>();
    }
    void Update()
    { 
    }
    void FixedUpdate()
    { 

        if (isPlayerIn)
        {
            currentAcceleration = acceleration * Input.GetAxis("Vertical");
            currentTurnAngle = maxTurnAngle * Input.GetAxis("Horizontal");



            if (Input.GetKey(KeyCode.Space))
            {
                currentBrakeForce = brakingForce;
                isBraking = true;
            }

            else
            {
                currentBrakeForce = 0f;
                isBraking = false;
            }


            //add acceleration to front wheels
            frontRight.motorTorque = currentAcceleration;
            frontLeft.motorTorque = currentAcceleration;
            BackRight.motorTorque = currentAcceleration;
            BackLeft.motorTorque = currentAcceleration;

            // brake all wheels
            frontRight.brakeTorque = currentBrakeForce;
            frontLeft.brakeTorque = currentBrakeForce;
            BackRight.brakeTorque = currentBrakeForce;
            BackLeft.brakeTorque = currentBrakeForce;


            // steering

            frontRight.steerAngle = currentTurnAngle;
            frontLeft.steerAngle = currentTurnAngle;

            UpdateWheel(frontRight, frontRightTransform);
            UpdateFrontLeftWheels(frontLeft, frontLeftTransform);
            UpdateWheel(BackRight, backRightTransform);
            UpdateBackLeftWheels(BackLeft, backLeftTransform);


         
        }
        
    }

    void UpdateWheel(WheelCollider col, Transform transform)
    {
        Vector3 position;
        Quaternion rotation;

        col.GetWorldPose(out position, out rotation);

        transform.position = position;
        transform.rotation = rotation;
    }

    void UpdateFrontLeftWheels(WheelCollider col, Transform transform)
    {
        Vector3 position;
        Quaternion rotation;

        col.GetWorldPose(out position, out rotation);

        transform.position = position + adjustVectorFrontWheel;
        transform.rotation = rotation;
    }

    void UpdateBackLeftWheels(WheelCollider col, Transform transform)
    {
        Vector3 position;
        Quaternion rotation;

        col.GetWorldPose(out position, out rotation);

        transform.position = position + adjustVectorBackWheel;
        transform.rotation = rotation;
    }
    public void SetIsGrounded(bool isGrounded)
    {
        IsGrounded = isGrounded;
    }

    public void SetIsInvincible(bool isInvincible)
    {
        IsInvincible = isInvincible;
    }

    private void OnInteractPerformed(InputAction.CallbackContext context)
    {
        if (isPlayerIn)
        {
            gameManager.Truck.isPlayerIn = false;
            gameManager.PlayerGameObject.GetComponent<ActionStateManager>().anim.SetBool("IsDriving", false);
            //gameManager.PlayerGameObject.SetActive(true);

        }
    }

    private void OnCollisionEnter(Collision collision)
    {

    }
}
