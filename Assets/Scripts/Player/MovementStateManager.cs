using UnityEngine;
using UnityEngine.InputSystem;

public class MovementStateManager : MonoBehaviour
{
    
    private Vector2 moveInput;

    private Vector3 direction; 

    private CharacterController characterController;
    private InputSystem_Actions actionSystem;
    [SerializeField] private float currentSpeed;
    private float walkingSpeed = 3f;

    void Awake()
    {
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
        characterController = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        Move();
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
}
