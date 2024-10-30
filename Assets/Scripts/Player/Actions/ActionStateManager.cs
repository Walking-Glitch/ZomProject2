using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.InputSystem;

public class ActionStateManager : MonoBehaviour
{
    // states
    [HideInInspector] public ActionStateBase CurrentState;
    [HideInInspector] public ReloadState Reload = new ReloadState();
    [HideInInspector] public DefaultState Default = new DefaultState();

    // animator
    [HideInInspector] public Animator anim;

    // constraint variables
    [HideInInspector] private TwoBoneIKConstraint LeftHandIKConstraint;
    public MultiAimConstraint RightHandAimConstraint;

    private InputSystem_Actions inputSystemActions;

    // audio 
    public AudioSource audioSource;

    void Awake()
    {
        inputSystemActions = new InputSystem_Actions();
        inputSystemActions.Player.Reload.performed += OnReloadPerformed; 
    }
    
    void Start()
    {
        anim = GetComponent<Animator>();
        LeftHandIKConstraint = GetComponentInChildren<TwoBoneIKConstraint>();
        SwitchState(Default);
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(CurrentState);
        CurrentState.UpdateState(this);
    }

    public void SwitchState(ActionStateBase state)
    {
        CurrentState = state;
        state.EnterState(this);
    }

    public void AdjustConstraintWeight()
    {
        if (CurrentState == Reload)
        {
            
            anim.SetLayerWeight(1, 1);
            LeftHandIKConstraint.weight = 0;
            LeftHandIKConstraint.data.hintWeight = 0;
            RightHandAimConstraint.weight = 0;
        }

        
    }
    private void OnReloadPerformed(InputAction.CallbackContext context)
    {
        SwitchState(Reload);
    }

    public void WeaponReloaded()
    {
        // call reload function *to do
        SwitchState(Default);
    }

    public void MagOut()
    {
        // play oneshot
    }

    public void MagIn()
    {
        // play oneshot
    }

    public void ReleaseSlideSound()
    {
        // play oneshot
    }
    private void OnEnable()
    {
        inputSystemActions.Enable();
    }
    private void OnDisable()
    {
        inputSystemActions.Disable();
    }
}
