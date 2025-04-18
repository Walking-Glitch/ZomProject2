using System.Collections;
 
using Assets.Scripts.Game_Manager;
using Pathfinding;
using UnityEngine;
using Unity.Netcode;  

[RequireComponent(typeof(AudioSource))]
public class ZombieStateManager : NetworkBehaviour
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
    private float walkSpeed = 1;
    private float runSpeed = 3;

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
    private bool AttackAttackable;
    private bool CanDamagePlayer;
    private bool HurtByExplosion;
    private bool HurtByTurret;
    // ragdoll
    public GameObject rig;
    protected Collider[] ragdollColliders;
    protected Rigidbody[] ragdollRigidbodies;

    // health 
    public int maxHealth;
    public int health;
    public bool isDead;
    public bool isActive;

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
    [SerializeField]private SkinnedMeshRenderer[] zombieMeshRenderers;
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

    // private reference to night just for initialization
    [SerializeField]private bool night;

    // reward 
    [HideInInspector] public int Reward;

    // targeting 

    public IAttackable currentTarget;
    private IAttackable targetToDamage;
    private float aggroRange = 15f;    
    private float maxDetectionRange = 1000f;
    public LayerMask AttackableLayer;
    



    //Network variables 
    public NetworkVariable<int> NetworkHealth = new NetworkVariable<int>(0,
       NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public NetworkVariable<bool> NetworkIsDead = new NetworkVariable<bool>(false,
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public NetworkVariable<bool> NetworkIsCrippled = new NetworkVariable<bool>(false,
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public NetworkVariable<bool> NetworkIsActive = new NetworkVariable<bool>(true,
      NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);


    public NetworkVariable<bool> NetworkIsAttacking = new NetworkVariable<bool>(false,
    NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);



    private void OnEnable()
    {
        DayCycle.OnNightTimeChanged += NightTimeMode;
        NightTimeMode(night);

        if(IsServer)
        {
            NetworkIsCrippled.Value = false; // taken out delaydestruction 
            NetworkHealth.Value = maxHealth;
        }
       

    }
    private void OnDisable()
    {
        DayCycle.OnNightTimeChanged -= NightTimeMode;
        NightTimeMode(night);
    }

    private void Awake()
    {
        anim = GetComponent<Animator>();
        aiPath = GetComponentInParent<AIPath>();
        destinationSetter = GetComponentInParent<AIDestinationSetter>();
        patrol = GetComponentInParent<Patrol>();
        agent = GetComponentInParent<IAstarAI>();
    }
    void Start()
    { 
        gameManager = GameManager.Instance;
        
        StartCoroutine(WaitForPlayer());

        //InvokeRepeating(nameof(UpdateTarget), 0f, checkInterval);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        zombieParent = transform.parent.gameObject;

        if (!IsServer) // Clients only
        {
            //Debug.Log($"Client joined. Applying initial state: {NetworkIsActive.Value}");
            zombieParent.SetActive(NetworkIsActive.Value);
        }


        NetworkHealth.OnValueChanged += (prev, curr) =>
        {
            //Debug.Log($"zombie health changed: {prev} ? {curr}");
            health = curr;
        };

        NetworkIsCrippled.OnValueChanged += (prev, curr) =>
        {
            //Debug.Log($"zombie crippled changed: {prev} ? {curr}");          
            isCrippled = curr;
        };

        NetworkIsDead.OnValueChanged += (prev, curr) =>
        {
            //Debug.Log($"zombie is dead changed: {prev} ? {curr}");
            isDead = curr; 
        };

        NetworkIsActive.OnValueChanged += (prev, curr) =>
        {
            //Debug.Log($"zombie is active changed: {prev} ? {curr}");
            isActive = curr;
            SetEnableZombieClientRpc(curr);
        };

        NetworkIsAttacking.OnValueChanged += (prev, curr) =>
        {
            //Debug.Log($"zombie is active changed: {prev} ? {curr}");
            AttackAttackable = curr;
        };


    }

    private IEnumerator WaitForPlayer()
    {
        // Wait until PlayerGameObject is assigned
        while (gameManager.PlayerGameObject == null)
        {
            yield return null; // Waits one frame
        }

        // Now it's safe to assign the player transform
        PlayerTransform = gameManager.PlayerGameObject.transform;

        health = maxHealth;       
        night = gameManager.DayCycle.IsNightTime;
        NightTimeMode(night);
        zombieAudioSource = GetComponent<AudioSource>();
        Reward = 50;
        isActive = true;

        GetSkinMeshBits();
        GetZombieLimbs();
        GetRagdollBits();
        RagdollModeOffServerRpc();
        SwitchState(chasing);
    }


    // Update is called once per frame
    void Update()
    {

        if (currentState == null) return; 
        currentState.UpdateState(this);
       
         
        CheckIfCrippledServerRpc();

        //Debug.Log(currentState);
        //Debug.Log(IsAttackableInAttackArea());
    }

    public void SwitchState(ZombieBaseState state)
    {
        currentState = state;
        currentState.EnterState(this);
    }

    

    [ClientRpc(RequireOwnership = false)]
    public void SetEnableZombieClientRpc(bool x)
    {
        if (zombieParent == null)
        {
            Debug.LogError("zombieParent is NULL on client!");
            return;
        }
        zombieParent.SetActive(x);
        //Debug.Log("enabled set variable to " + x);
    }



    [ServerRpc(RequireOwnership = false)]      
    public void CheckIfCrippledServerRpc()
    {
        foreach (Limbs limb in LeftLeg)
        {
            if (limb.limbHealth <= 0)
            {
                NetworkIsCrippled.Value = true;
            }
        }

        foreach (Limbs limb in RightLeg)
        {
            if (limb.limbHealth <= 0)
            {
                NetworkIsCrippled.Value = true;                
            }
        }
    }

    public void NightTimeMode(bool isNight)
    {
        anim.SetBool("IsNightTime", isNight);

        if (isNight)
        {
            aiPath.maxSpeed = runSpeed;

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

            aiPath.maxSpeed = walkSpeed;
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
    [ClientRpc]
    public void RagdollModeOnClientRpc()
    {

        if(ragdollColliders != null)
        {
            foreach (Rigidbody rigid in ragdollRigidbodies)
            {
                rigid.isKinematic = false;
            }

            anim.enabled = false;
        }
        

    }

    [ServerRpc(RequireOwnership = false)]
    public void RagdollModeOffServerRpc()
    {
        NetworkIsDead.Value = false;
        RagdollModeOffClientRpc();
    }

    [ClientRpc]
    public void RagdollModeOffClientRpc()
    { 
        if (ragdollColliders!=null && ragdollColliders.Length > 0)
        {
            foreach (Rigidbody rigid in ragdollRigidbodies)
            {
                rigid.isKinematic = true;
            }
        
        }

        anim.enabled = true;        
    }


    [ServerRpc]
    public void PlayerDestroyZombieServerRpc()
    {
       
        StartCoroutine(DelayDestruction(3f));

    }
    

    protected virtual IEnumerator DelayDestruction(float delay)
    {

        yield return new WaitForSeconds(delay);
         
        RagdollModeOffServerRpc();

        gameManager.EconomyManager.CollectMoney(Reward);

        gameManager.EnemyManager.DecreaseEnemyCtr();
         
        EnableSkinMeshesClientRpc();

        EnableCollidersClientRpc();

        foreach (var limb in zombieLimbs)
        {
            limb.ReattachReplacementLimbServerRpc();
        }


        SetIsAlerted(false);

        SwitchState(idle);

        NetworkIsActive.Value = false;

    }

  

    [ClientRpc]
    private void EnableCollidersClientRpc()
    {
        if(ragdollColliders != null)
        {
            foreach (var col in ragdollColliders)
            {
                col.enabled = true;
            }
        }
       
    }

    [ClientRpc]
    private void EnableSkinMeshesClientRpc()
    {
        foreach (var skin in zombieMeshRenderers)
        {
            skin.enabled = true;
        }
    }

    [ClientRpc]
    public void DismembermentByExplosionClientRpc()
    {
        foreach (var limbs in zombieLimbs)
        {
            bool shouldDmg = Random.Range(0, 2) == 0;

            if (shouldDmg)
            {
                limbs.LimbTakeDamageServerRpc(500);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public virtual void TakeDamageServerRpc(int damage, string limbName, bool explosion, bool turret , float force)
    {
        if (!IsServer) return;

        NetworkHealth.Value = Mathf.Clamp(NetworkHealth.Value - damage, 0, maxHealth);

        if (health > 0 && currentState != Death)
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
            SetKilledByExplosion(explosion);
            SetKilledByTurret(turret);
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

    public void DealDamage() // called by anim event
    {
        if (IsAttackableInDamageArea())
        {
            if (targetToDamage == null)
            {
                Debug.Log("target is null");
            }
            else
            {
                targetToDamage.TakeDamage(10);
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


    public void SetCanMove(bool canMove)
    { 
        aiPath.canMove = canMove;
        PlayZombieAnimationBoolClientRpc("CanMove", canMove);
        //anim.SetBool("CanMove", canMove);
    }

   


    public void UpdateTarget()
    {         
        if (!IsServer) return;
         
        IAttackable bestTarget = null;

        float closestDistance = Mathf.Infinity;
        int highestPriority = 0;


        Collider[] colliders = Physics.OverlapSphere(transform.position, maxDetectionRange, AttackableLayer);

        foreach (Collider col in colliders)
        {
            IAttackable attackable = col.GetComponentInParent<IAttackable>();
            //Debug.Log($"Overlap detected: {col.gameObject.name}, Layer: {LayerMask.LayerToName(col.gameObject.layer)}");

            if (attackable != null)
            {
                float distance = Vector3.Distance(transform.position, attackable.GetTransform().position);
                int priority = attackable.GetPriority();

                 

                if (priority == 1 && distance <= aggroRange)
                {
                    if (bestTarget == null || distance < closestDistance)
                    {
                        bestTarget = attackable;
                        closestDistance = distance;
                    }
                }

                else if (priority != 1 && (bestTarget == null || distance < closestDistance || priority > highestPriority))
                {
                    // Update best target and priority
                    bestTarget = attackable;
                    highestPriority = priority;
                    closestDistance = distance;
                }

             
  
            }
        }

        // Update the target only once at the end
        if (bestTarget != null && bestTarget != currentTarget)
        {
            currentTarget = bestTarget;
        }

        Debug.Log(currentTarget);

        destinationSetter.target = currentTarget.GetTransform();
    }


    [ClientRpc]
    public void PlayZombieAnimationBoolClientRpc(string animationName, bool setBool)
    {
        if (!IsOwner || IsServer)
            anim.SetBool(animationName, setBool);
    }
    [ClientRpc]
    public void PlayZombieAnimationTriggerClientRpc(string animationName)
    {
        if (!IsOwner || IsServer)
            anim.SetTrigger(animationName);
    }

    public void SetAttackStatus(bool isInAttackArea)
    {
        if (isDead && currentState != attack)
        {
            NetworkIsAttacking.Value = false;
        }
        else
        {
            NetworkIsAttacking.Value = isInAttackArea;
        }
         
    }

    public bool IsAttackableInAttackArea()
    {
        return AttackAttackable;
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
        PlayZombieAnimationBoolClientRpc("IsAlerted", isAlerted);
        //anim.SetBool("IsAlerted", alerted);
    }

    public bool IsZombieAlerted()
    {
        return alerted;
    }

    public void SetIsInDamageArea(bool isInDamageArea, IAttackable attackable)
    {
        CanDamagePlayer = isInDamageArea;
        targetToDamage = attackable;
    }

    public bool IsAttackableInDamageArea()
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

    public void SetKilledByTurret(bool isHurtByTurret)
    {
        HurtByTurret = isHurtByTurret;
    }

    public bool IsKilledByTurret()
    {
        return HurtByTurret;
    }
}
