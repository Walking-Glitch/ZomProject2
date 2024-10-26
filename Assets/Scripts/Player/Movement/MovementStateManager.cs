using UnityEngine;
using UnityEngine.InputSystem;

public class MovementStateManager : MonoBehaviour
{
    // movement variables
    private Vector2 moveInput;

    private Vector3 direction; 

    private CharacterController characterController;
    private InputSystem_Actions actionSystem;
    [SerializeField] private float currentSpeed;
    private float walkingSpeed = 3f;

    // Jump & grounded variables
  
    [SerializeField] private float groundYoffSet;
    [SerializeField] private LayerMask groundMask;
    private Vector3 spherePos;
    [SerializeField]private Vector3 velocity;

    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float jumpForce; 

    // Animator variables
    private Animator anim;

    void Awake()
    {
        characterController = GetComponent<CharacterController>();
        actionSystem = new InputSystem_Actions();
        actionSystem.Player.Move.performed += OnMovePerformed;
        actionSystem.Player.Move.canceled += OnMoveCancelled;
    }

    void OnEnable()
    {
        actionSystem.Enable();
    }

    void OnDisable()
    {
        actionSystem.Disable();
    }

    void Start()
    {
        currentSpeed = walkingSpeed;
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        Move();
        Gravity();
        Falling();
        
    }

    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    private void OnMoveCancelled(InputAction.CallbackContext context)
    {
        moveInput = Vector2.zero;
    }

    private void Move()
    {
        
        direction = transform.forward * moveInput.y + transform.right * moveInput.x;

        characterController.Move(direction.normalized * currentSpeed * Time.deltaTime);

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
