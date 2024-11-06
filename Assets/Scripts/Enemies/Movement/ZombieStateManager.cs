using System.Collections;
using Pathfinding;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class ZombieStateManager : MonoBehaviour
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
    public ZombieBaseState currentState;

    public Ragdoll ragdoll = new Ragdoll();
    public Chasing chasing = new Chasing();
    public Roaming roaming = new Roaming();
    public Idle idle = new Idle();


    // detection variables
    private bool PlayerDetected;

    // ragdoll
    public GameObject rig;
    //public Collider mainCollider;
    protected Collider[] ragdollColliders;
    protected Rigidbody[] ragdollRigidbodies;

    // health 
    public int maxHealth;
    public int health;
    public bool isDead;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        anim = GetComponent<Animator>();

        aiPath = GetComponentInParent<AIPath>();
        destinationSetter = GetComponentInParent<AIDestinationSetter>();
        patrol = GetComponentInParent<Patrol>();
        agent = GetComponentInParent<IAstarAI>();

        GetRagdollBits();
        RagdollModeOff();

        SwitchState(roaming);
    }

    // Update is called once per frame
    void Update()
    {
        UpdateCurrentSpeed();
        currentState.UpdateState(this);
        //Debug.Log(currentState);

        if (Input.GetKey(KeyCode.Space))
        {
            SwitchState(ragdoll);
        }


        if (Input.GetKey(KeyCode.F))
        {
            SwitchState(roaming);
        }
    }

    public void SwitchState(ZombieBaseState state)
    {
        currentState = state;
        currentState.EnterState(this);
    }

    protected void GetRagdollBits()
    {
        ragdollColliders = rig.GetComponentsInChildren<Collider>();
        ragdollRigidbodies = rig.GetComponentsInChildren<Rigidbody>();
    }
    public void RagdollModeOn()
    {
        //foreach (Collider col in ragdollColliders)
        //{
        //    col.enabled = true;
        //}

        foreach (Rigidbody rigid in ragdollRigidbodies)
        {
            rigid.isKinematic = false;
        }

        anim.enabled = false;

         

    }

    public void RagdollModeOff()
    {
        
        isDead = false;

        health = maxHealth;

        //foreach (Collider col in ragdollColliders)
        //{
        //   // col.enabled = false;
        //}

        foreach (Rigidbody rigid in ragdollRigidbodies)
        {
            rigid.isKinematic = true;
        }

        anim.enabled = true;

         

        //bloodVisualEffect.enabled = false;

    }

    protected virtual void PlayerDestroyZombie()
    {
        StartCoroutine(DelayDestruction(3f));

    }

    protected virtual IEnumerator DelayDestruction(float delay)
    {

        yield return new WaitForSeconds(delay);
        RagdollModeOff();
        gameObject.SetActive(false);

    }

    public virtual void TakeDamage(int damage, Vector3 bloodSpeed, bool explosion, float force)
    {
        health -= damage;
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
