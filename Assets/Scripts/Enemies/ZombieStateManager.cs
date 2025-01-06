using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Assets.Scripts.Game_Manager;
using Pathfinding;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations.Rigging;

[RequireComponent(typeof(AudioSource))]
public class ZombieStateManager : MonoBehaviour
{
    // player reference    
    [SerializeField] public Transform PlayerTransform;

    //Zombie parent obj reference
    private GameObject zombieParent; 

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

    public Hurt hurt = new Hurt();
    public Death Death = new Death();
    public Chasing chasing = new Chasing();
    public Roaming roaming = new Roaming();
    public Idle idle = new Idle();
    public Attack attack = new Attack();


    // detection variables
    private bool PlayerDetected;
    private bool AttackPlayer;
    private bool CanDamagePlayer;
    private bool HurtByExplosion;

    // ragdoll
    public GameObject rig;
    protected Collider[] ragdollColliders;
    protected Rigidbody[] ragdollRigidbodies;

    // health 
    public int maxHealth;
    public int health;
    public bool isDead;

    // alerted
    public bool alerted; 

    // legs
    public bool isCrippled;
    public Limbs [] LeftLeg;
    public Limbs [] RightLeg;

    // game manager reference
    private GameManager gameManager;

    //skin mesh object reference
    public GameObject SkinRig;
    private SkinnedMeshRenderer[] zombieMeshRenderers;
    private Limbs[] zombieLimbs;

    //explosion
    public Vector3 ExpDirection;

    // zombie mesh customization
    public SkinnedMeshRenderer face;

    //zombie sfx    
    public AudioClip[] zombieChasingClips;
    public AudioClip[] zombieHurtClips;
    public AudioClip[] zombieAttackClips;
    [HideInInspector] public AudioSource zombieAudioSource;

    void Start()
    {
        

        gameManager = GameManager.Instance;

        PlayerTransform = gameManager.PlayerGameObject.transform;

        health = maxHealth;

        zombieParent = transform.parent.gameObject;

        anim = GetComponent<Animator>();

        aiPath = GetComponentInParent<AIPath>();
        destinationSetter = GetComponentInParent<AIDestinationSetter>();
        patrol = GetComponentInParent<Patrol>();
        agent = GetComponentInParent<IAstarAI>();

        zombieAudioSource = GetComponent<AudioSource>();


        GetSkinMeshBits();
        GetZombieLimbs();
        GetRagdollBits();
        RagdollModeOff();

        SwitchState(chasing);


    }

    



    // Update is called once per frame
    void Update()
    {
        UpdateCurrentSpeed();

        currentState.UpdateState(this);
        //Debug.Log(currentState);
         
        CheckIfCrippled();

        NightTimeMode();
    }

    public void SwitchState(ZombieBaseState state)
    {
        currentState = state;
        currentState.EnterState(this);
    }

    public void CheckIfCrippled()
    {
        foreach (Limbs limb in LeftLeg)
        {
            if (limb.limbHealth <= 0)
            {
                isCrippled = true;
            }
        }

        foreach (Limbs limb in RightLeg)
        {
            if (limb.limbHealth <= 0)
            {
                isCrippled = true;
            }
        }
    }

    public void NightTimeMode()
    {
        if (gameManager.DayCycle.IsNightTime)
        {
            if (face.material != null && face.materials.Length > 0)
            {
                if (face.material.IsKeywordEnabled("_EMISSION"))
                {
                    //Debug.Log("Keyword _EMISSION enabled");
                }
                else
                {
                    //Debug.Log("Keyword _EMISSION disabled");
                    face.material.EnableKeyword("_EMISSION");

                }
            }
            else
            {
                //Debug.Log("Material is null or no materials assigned.");
            }
        }
        else
        {
            if (face.material != null && face.materials.Length > 0) face.material.DisableKeyword("_EMISSION");
        }
        
    }
    protected void GetRagdollBits()
    {
        ragdollColliders = rig.GetComponentsInChildren<Collider>();
        ragdollRigidbodies = rig.GetComponentsInChildren<Rigidbody>();
    }

    protected void GetSkinMeshBits()
    {
        zombieMeshRenderers = SkinRig.GetComponentsInChildren<SkinnedMeshRenderer>();
    }

    protected void GetZombieLimbs()
    {
        zombieLimbs = rig.GetComponentsInChildren<Limbs>();
    }
    public void RagdollModeOn()
    {
     
        foreach (Rigidbody rigid in ragdollRigidbodies)
        {
            rigid.isKinematic = false;
        }

        anim.enabled = false;

    }

    public void RagdollModeOff()
    {
        isDead = false;

        foreach (Rigidbody rigid in ragdollRigidbodies)
        {
            rigid.isKinematic = true;
        }

        anim.enabled = true;

        //bloodVisualEffect.enabled = false;
    }

    public void PlayerDestroyZombie()
    {
        StartCoroutine(DelayDestruction(3f));

    }

    protected virtual IEnumerator DelayDestruction(float delay)
    {

        yield return new WaitForSeconds(delay);
       
        RagdollModeOff();

        gameManager.EnemyManager.DecreaseEnemyCtr(); 
       
        health = maxHealth;

        foreach (var skin in zombieMeshRenderers)
        {
                skin.enabled = true;
        }

        foreach (var col in ragdollColliders)
        {
            col.enabled = true;
        }

        foreach (var limb in zombieLimbs)
        {
            limb.limbHealth = limb.limbMaxHealth;
            limb.ReattachReplacementLimb();
        }

        isCrippled = false;

        SetIsAlerted(false);

        SwitchState(idle);

        zombieParent.SetActive(false);

        

    }

    public void DismembermentByExplosion()
    {
        foreach (var limbs in zombieLimbs)
        {
            bool shouldDmg = Random.Range(0, 2) == 0;

            if (shouldDmg)
            {
                limbs.LimbTakeDamage(500);
            }
        }
    }

    

    public virtual void TakeDamage(int damage, string limbName, bool explosion, float force)
    {
        health -= damage;

        SetKilledByExplosion(explosion);

        if (health > 0)
        {
            if (limbName == "torso" || limbName == "belly" && !isDead)
            {
                SwitchState(hurt);
            }
            else
            {
                SwitchState(chasing);
            }
        }
        else
        {
            SwitchState(Death);
        }
    }

    public void SetExplosionDirection(Vector3 grenade, Vector3 zombie)
    {
        Vector3 direction = (zombie - grenade).normalized;

        ExpDirection = direction;   
    }

    public Vector3 GetExplosionDirection()
    {
        return ExpDirection;
    }

    public void DealDamage()
    {
        if (IsPlayerInDamageArea())
        {
            if (gameManager.PlayerStats == null)
            {
                Debug.Log("player is null");
            }
            else
            {
                gameManager.PlayerStats.PlayerTakeDamage(10);
            }
        }
    }

    public void PlayZombieChasingSfx()
    {
       
        if (!zombieAudioSource.isPlaying && zombieChasingClips.Length > 0)
        {
            if (currentState == chasing)
            {
                int randomIndex = Random.Range(0, zombieChasingClips.Length);

                zombieAudioSource.clip = zombieChasingClips[randomIndex];
                zombieAudioSource.Play();
            }
        }
    }

    public void PlayZombieHurtSfx()
    {

        if (!zombieAudioSource.isPlaying && zombieHurtClips.Length > 0)
        {
            if (currentState == hurt)
            {
                int randomIndex = Random.Range(0, zombieHurtClips.Length);

                zombieAudioSource.clip = zombieHurtClips[randomIndex];
                zombieAudioSource.Play();
            }
        }
    }

    public void PlayZombieAttackSfx()
    {

        if (!zombieAudioSource.isPlaying && zombieAttackClips.Length > 0)
        {
            if (currentState == attack)
            {
                int randomIndex = Random.Range(0, zombieAttackClips.Length);

                zombieAudioSource.clip = zombieAttackClips[randomIndex];
                zombieAudioSource.Play();
            }
        }
    }


    private void UpdateCurrentSpeed()
    {
        currentSpeed = agent.velocity.magnitude;
        anim.SetFloat("Speed", currentSpeed);
    }

    public void SetPlayerAttackStatus(bool isInAttackArea)
    {
        AttackPlayer = isInAttackArea;
    }

    public bool IsPlayerInAttackArea()
    {
        return AttackPlayer;
    }
    public void SetPlayerDetectionStatus(bool isDetected)
    {
        PlayerDetected = isDetected;
    }

    public bool IsPlayerInDetectionArea()
    {
        return PlayerDetected;
    }

    public void SetIsAlerted(bool isAlerted)
    {
        alerted = isAlerted;
        anim.SetBool("IsAlerted", alerted);
    }

    public bool IsZombieAlerted()
    {
        return alerted;
    }

    public void SetIsInDamageArea(bool isInDamageArea)
    {
        CanDamagePlayer = isInDamageArea;
    }

    public bool IsPlayerInDamageArea()
    {
        return CanDamagePlayer;
    }

    public void SetKilledByExplosion(bool isHurtByExplosion)
    {
        HurtByExplosion = isHurtByExplosion;
    }

    public bool IsKilledByExplosion()
    {
        return HurtByExplosion;
    }
}
