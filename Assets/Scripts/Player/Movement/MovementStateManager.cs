using Assets.Scripts.Player.Actions;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;


public class MovementStateManager : NetworkBehaviour
{
    // movement variables
    [HideInInspector] public Vector2 moveInput;
    [HideInInspector] public Vector3 direction; 

    private CharacterController characterController;
    private InputSystem_Actions actionSystem;
    public float currentSpeed;

    public bool isRunning;

    //Speed variables

    public float walkSpeed = 3, walkBackSpeed = 2;
    public float runSpeed = 7, runBackSpeed = 5;


    // Jump & grounded variables

    [SerializeField] private float groundYoffSet;
    [SerializeField] private LayerMask groundMask;
    private Vector3 spherePos;
    [SerializeField]private Vector3 velocity;

    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float jumpForce; 

    // Animator variables
    [HideInInspector] public Animator anim;

    // state variables

    public MovementBaseState previousState;
    public MovementBaseState currentState;

    public IdleState Idle = new IdleState();
    public WalkState Walk = new WalkState();
    public RunState Run = new RunState();
    public JumpState Jump = new JumpState();

    // reference to other managers
    [HideInInspector] public AimStateManager aimStateManager;
    [HideInInspector] public ActionStateManager actionStateManager;

     


    void OnEnable()
    {
        if (actionSystem != null)
            actionSystem.Enable();
    }

    void OnDisable()
    {
        if (actionSystem != null)
            actionSystem.Disable();
    }

    public override void OnNetworkSpawn()
    {
        Initialize();
    }

    void Start()
    {
        if (!IsServer && !IsClient)  
        {
            Initialize();
        }
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log($"Player {NetworkObjectId} | IsOwner: {IsOwner}");

        if (!IsOwner) return; //

        Move();
        Gravity();

        currentState.UpdateState(this);

    }

    private void Initialize()
    {
        characterController = GetComponent<CharacterController>();
        actionSystem = new InputSystem_Actions();

        actionSystem.Player.Move.performed += OnMovePerformed;
        actionSystem.Player.Move.canceled += OnMoveCancelled;
        actionSystem.Player.Sprint.performed += OnRunPerformed;
        actionSystem.Player.Sprint.canceled += OnRunCancelled;

        actionSystem.Enable();

        currentSpeed = walkSpeed;
        anim = GetComponent<Animator>();
        aimStateManager = GetComponent<AimStateManager>();
        actionStateManager = GetComponent<ActionStateManager>();

        SwitchState(Idle);
    }
    public void SwitchState(MovementBaseState state)
    {
        currentState = state;
        currentState.EnterState(this);
    }

    public void OnMovePerformed(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    private void OnMoveCancelled(InputAction.CallbackContext context)
    {
        moveInput = Vector2.zero;
    }

    public void OnRunPerformed(InputAction.CallbackContext context)
    {
        if(actionStateManager.CurrentState == actionStateManager.Default)
        SwitchState(Run);
    }

    public void OnRunCancelled(InputAction.CallbackContext context)
    {
       SwitchState(Walk);
    }

    private void Move()
    {
        direction = transform.forward * moveInput.y + transform.right * moveInput.x;

        characterController.Move(direction.normalized * currentSpeed * Time.deltaTime);


        anim.SetFloat("hInput", moveInput.x, dampTime: 0.12f, Time.deltaTime);
        anim.SetFloat("vInput", moveInput.y, dampTime: 0.12f, Time.deltaTime);
    }

    public bool IsGrounded()
    {
        spherePos = new Vector3(transform.position.x, transform.position.y - groundYoffSet, transform.position.z);

        if (Physics.CheckSphere(spherePos, characterController.radius - 0.05f, groundMask))
        {
            return true;
        }

        return false;
    }

    private void Gravity()
    {
        if (!IsGrounded())
        {
            velocity.y += gravity * Time.deltaTime;
        }
        else if (velocity.y < 0)
        {
            velocity.y = -2f; 
        }
        characterController.Move((velocity * Time.deltaTime));
    }

    public void Falling()
    {
        anim.SetBool("Falling", !IsGrounded()); 
    }

    public void JumpForce()
    {
        velocity.y += jumpForce; 
    }

    void OnDrawGizmos()
    {
        if (characterController == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(spherePos, characterController.radius -0.05f);
    }
}
