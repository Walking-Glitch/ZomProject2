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
    private bool AttackPlayer;
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

    // private reference to night just for initialization
    [SerializeField]private bool night;

    // reward 
    [HideInInspector] public int Reward;

    //Network variables 
    public NetworkVariable<int> NetworkHealth = new NetworkVariable<int>(100,
       NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public NetworkVariable<bool> NetworkIsDead = new NetworkVariable<bool>(false,
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public NetworkVariable<bool> NetworkIsCrippled = new NetworkVariable<bool>(false,
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private void OnEnable()
    {
        DayCycle.OnNightTimeChanged += NightTimeMode;
        NightTimeMode(night);

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
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        NetworkHealth.OnValueChanged += (curr, prev) =>
        {
            Debug.Log($"zombie health changed: {prev} ? {curr}");
            health = curr;
        };

        NetworkIsCrippled.OnValueChanged += (curr, prev) =>
        {
            Debug.Log($"zombie crippled changed: {prev} ? {curr}");
            //Debug.Log($"zombie crippled changed: {prev} ? {curr} \n{System.Environment.StackTrace}");
            isCrippled = curr;
        };

        NetworkIsDead.OnValueChanged += (curr, prev) =>
        {
            Debug.Log($"zombie is dead changed: {prev} ? {curr}");
            isDead = curr; 
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
        zombieParent = transform.parent.gameObject;
        night = gameManager.DayCycle.IsNightTime;
        NightTimeMode(night);
        zombieAudioSource = GetComponent<AudioSource>();
        Reward = 50;

        GetSkinMeshBits();
        GetZombieLimbs();
        GetRagdollBits();
        RagdollModeOffClientRpc();
        SwitchState(chasing);
    }


    // Update is called once per frame
    void Update()
    {
        
        currentState.UpdateState(this);
       
         
        CheckIfCrippled();

        //Debug.Log(currentState);
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
               
                NetworkIsCrippled.Value = isCrippled;
                //Debug.Log($"NetworkIsCrippled.Value: {NetworkIsCrippled.Value}");
                //Debug.Log(isCrippled);
            }
        }

        foreach (Limbs limb in RightLeg)
        {
            if (limb.limbHealth <= 0)
            {
                isCrippled = true;
                NetworkIsCrippled.Value = isCrippled;
                //Debug.Log($"NetworkIsCrippled.Value: {NetworkIsCrippled.Value}");
                //Debug.Log(isCrippled);
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
     
        foreach (Rigidbody rigid in ragdollRigidbodies)
        {
            rigid.isKinematic = false;
        }

        anim.enabled = false;

    }
    [ClientRpc]
    public void RagdollModeOffClientRpc()
    {
        //isDead = false;
        NetworkIsDead.Value = true;

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
       
        RagdollModeOffClientRpc();

        gameManager.EconomyManager.CollectMoney(Reward);

        gameManager.EnemyManager.DecreaseEnemyCtr();

        //health = maxHealth;
        NetworkHealth.Value = maxHealth;

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
        NetworkIsCrippled.Value = isCrippled;

        SetIsAlerted(false);

        SwitchState(idle);

        zombieParent.SetActive(false);

        

    }
    [ClientRpc]
    public void DismembermentByExplosionClientRpc()
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

    

    public virtual void TakeDamage(int damage, string limbName, bool explosion, bool turret , float force)
    {
        if (!IsServer) return;

        health -= damage;

       
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


    public void SetCanMove(bool canMove)
    { 
        aiPath.canMove = canMove;
        PlayZombieAnimationBoolClientRpc("CanMove", canMove);
        //anim.SetBool("CanMove", canMove);
    }

    public void SetPlayerAttackStatus(bool isInAttackArea)
    {
        AttackPlayer = isInAttackArea;
        PlayZombieAnimationBoolClientRpc("IsAttacking", isInAttackArea);
        //anim.SetBool("IsAttacking", isInAttackArea);
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
        PlayZombieAnimationBoolClientRpc("IsAlerted", isAlerted);
        //anim.SetBool("IsAlerted", alerted);
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

    public void SetKilledByTurret(bool isHurtByTurret)
    {
        HurtByTurret = isHurtByTurret;
    }

    public bool IsKilledByTurret()
    {
        return HurtByTurret;
    }
}
