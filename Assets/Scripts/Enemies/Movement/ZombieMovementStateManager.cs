using Pathfinding;
using UnityEngine;

public class ZombieMovementStateManager : MonoBehaviour
{
    // navigation variables
    [HideInInspector] public AIPath aiPath;
    [HideInInspector] public AIDestinationSetter destinationSetter;
    [HideInInspector] public Patrol patrol;
    [HideInInspector] public IAstarAI agent;

    public float currentSpeed; 

    // animator
    [HideInInspector] public Animator anim;

    // state references
    public ZombieMovementBaseState currentState;

    public Chasing chasing = new Chasing();
    public Roaming roaming = new Roaming();
    public Idle idle = new Idle();

    // detection variables
    private bool PlayerDetected;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        anim = GetComponent<Animator>();

        aiPath = GetComponentInParent<AIPath>();
        destinationSetter = GetComponentInParent<AIDestinationSetter>();
        patrol = GetComponentInParent<Patrol>();
        agent = GetComponentInParent<IAstarAI>();
        
        SwitchState(roaming);
    }

    // Update is called once per frame
    void Update()
    {
        UpdateCurrentSpeed();
        currentState.UpdateState(this);
        //Debug.Log(currentState);
    }

    public void SwitchState(ZombieMovementBaseState state)
    {
        currentState = state;
        currentState.EnterState(this);
    }

    private void UpdateCurrentSpeed()
    {
        currentSpeed = agent.velocity.magnitude;
        anim.SetFloat("Speed", currentSpeed);
    }

    public void SetPlayerDetectionStatus(bool isDetected)
    {
        PlayerDetected = isDetected;
    }

    public bool IsPlayerInDetectionArea()
    {
        return PlayerDetected;
    }
}
