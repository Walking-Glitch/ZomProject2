using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Assets.Scripts.Game_Manager;
using Assets.Scripts.Player;
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

    [Header("Movement")]
    [Tooltip("Current movement speed (read-only, for debugging).")]
    public float currentSpeed;
    [Tooltip("Max pathing speed during the day (zombies walk).")]
    [SerializeField] private float walkSpeed = 1;
    [Tooltip("Max pathing speed at night (zombies run).")]
    [SerializeField] private float runSpeed = 3;

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
    private bool HurtByExplosion;
    private bool HurtByTurret;
    // ragdoll
    public GameObject rig;
    protected Collider[] ragdollColliders;
    protected Rigidbody[] ragdollRigidbodies;

    // health
    [Header("Health")]
    [Tooltip("Health the zombie spawns with. Rifle body shots deal ~40 x limb multiplier (head x10).")]
    public int maxHealth;
    [Tooltip("Current health (runtime). Editing it does not kill; only damage triggers death.")]
    public int health;
    [Tooltip("True from the moment the zombie dies until it returns to the pool (runtime).")]
    public bool isDead;

    [Header("Combat")]
    [Tooltip("Damage dealt per swing to the player, turrets, barriers and cabins.")]
    public int AttackDamage = 10;
    [Tooltip("Money paid to the player when this zombie is killed.")]
    public int Reward = 50;
    [Tooltip("Seconds a corpse stays on the ground before the zombie returns to the pool.")]
    public float CorpseDespawnDelay = 3f;

    // alerted
    public bool alerted;

    // legs
    [Tooltip("True once a leg limb is destroyed; the zombie ragdolls when it dies (runtime).")]
    public bool isCrippled;
    public Limbs [] LeftLeg;
    public Limbs [] RightLeg;

    // game manager reference
    private GameManager gameManager;

    //skin mesh object reference
    public GameObject SkinRig;
    private SkinnedMeshRenderer[] zombieMeshRenderers;
    private Limbs[] zombieLimbs;
    private Collider torsoCollider;

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

    // targeting: march on the nearest cabin, divert to nearby player/turrets/barriers
    [Header("Targeting")]
    [Tooltip("Lower bound of the random aggro range rolled on each spawn. Player/turrets/barriers inside it divert the zombie from the cabin.")]
    public float MinAggroRange = 10f;
    [Tooltip("Upper bound of the random aggro range rolled on each spawn.")]
    public float MaxAggroRange = 20f;
    [Tooltip("Chance (0-1) a spawned zombie ignores diversions and walks straight to the cabin.")]
    [Range(0f, 1f)] public float BeelineChance = 0.2f;
    [Tooltip("Seconds between target re-evaluations. Lower = more responsive, more CPU.")]
    public float RetargetInterval = 0.5f;
    [Tooltip("Seconds a zombie commits to a target before switching to a closer one of the same priority.")]
    public float TargetStickiness = 3f;
    [Tooltip("Layers scanned for attackable targets. Empty = Default, Player, Obstacles, Turret.")]
    public LayerMask AttackableLayer;

    // pathfinding keeps zombies a margin away from obstacles, beyond the small swing triggers
    [Tooltip("How close (m) the zombie must be to a barrier/turret/cabin surface to hit it. Raise if zombies stand next to obstacles without attacking.")]
    public float AttackReach = 1.5f;

    [HideInInspector] public IAttackable currentTarget;
    private IAttackable attackTarget;
    private float aggroRange;
    private bool beeline;
    private float retargetTimer;
    private float targetLockedUntil;
    private bool isAttacking;

    // attackables currently touching the attack and damage triggers
    private readonly HashSet<IAttackable> attackablesInReach = new HashSet<IAttackable>();
    private readonly HashSet<IAttackable> attackablesInDamageArea = new HashSet<IAttackable>();

    // shared scan buffer, zombies update on the main thread one at a time
    private static readonly Collider[] targetScanBuffer = new Collider[256];

    private void OnEnable()
    {
        RollTargetingTraits();

        DayCycle.OnNightTimeChanged += NightTimeMode;

        // pooled zombies miss day/night events while disabled, so read the current time on every respawn
        GameManager gm = GameManager.Instance;
        if (gm != null && gm.DayCycle != null) night = gm.DayCycle.IsNightTime;
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

        // player, barriers and cabins (Default), turrets
        if (AttackableLayer.value == 0) AttackableLayer = LayerMask.GetMask("Default", "Player", "Obstacles", "Turret");
    }
    void Start()
    {
        

        gameManager = GameManager.Instance;

        PlayerTransform = gameManager.PlayerGameObject.transform;

        health = maxHealth;

        zombieParent = transform.parent.gameObject;

        night = gameManager.DayCycle.IsNightTime;
        NightTimeMode(night);

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
        UpdateAttackTarget();

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

        foreach (Limbs limb in zombieLimbs)
        {
            if (limb.limbName == "torso")
            {
                torsoCollider = limb.GetComponent<Collider>();
                break;
            }
        }
    }

    // center of the torso, so turrets aim at the body whether the zombie stands, crawls or is a giant
    public Vector3 GetAimPoint()
    {
        // a disabled collider reports empty bounds at the world origin
        if (torsoCollider != null && torsoCollider.enabled && torsoCollider.gameObject.activeInHierarchy)
            return torsoCollider.bounds.center;

        return transform.parent.position + Vector3.up * 1.1f;
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
        StartCoroutine(DelayDestruction(CorpseDespawnDelay));

    }

    protected virtual IEnumerator DelayDestruction(float delay)
    {

        yield return new WaitForSeconds(delay);
       
        RagdollModeOff();

        gameManager.EconomyManager.CollectMoney(Reward);

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

    

    public virtual void TakeDamage(int damage, string limbName, bool explosion, bool turret , float force)
    {
        // corpse stays hittable until DelayDestruction resets it; ignore hits so death isn't processed twice
        if (isDead) return;

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

    public void DealDamage() // called by the attack animation events
    {
        if (!attackTarget.IsAlive()) return;

        // the player can dodge a swing by leaving the damage area; static targets only need to be within reach
        bool connects = attackablesInDamageArea.Contains(attackTarget) || (!UsesSwingTriggers(attackTarget) && IsWithinReach(attackTarget));

        if (connects)
        {
            attackTarget.TakeDamage(AttackDamage);
        }
    }

    private static bool UsesSwingTriggers(IAttackable target)
    {
        return target is PlayerStatus;
    }

    private bool IsWithinReach(IAttackable target)
    {
        Vector3 closest = target.GetClosestPoint(transform.position);

        return (closest - transform.position).sqrMagnitude <= AttackReach * AttackReach;
    }

    private void RollTargetingTraits()
    {
        aggroRange = Random.Range(MinAggroRange, MaxAggroRange);
        beeline = Random.value < BeelineChance;

        // spread target scans across frames instead of every zombie scanning at once
        retargetTimer = Random.Range(0f, RetargetInterval);

        currentTarget = null;
        attackTarget = null;
        attackablesInReach.Clear();
        attackablesInDamageArea.Clear();
    }

    public void TickTargeting()
    {
        retargetTimer -= Time.deltaTime;
        if (retargetTimer > 0f) return;

        retargetTimer = RetargetInterval;
        UpdateTarget();
    }

    public void UpdateTarget()
    {
        // beeline zombies ignore diversions and head straight for the cabins
        IAttackable best = beeline ? null : FindNearestDiversion();

        if (best == null) best = FindNearestStructure();

        if (best == null && gameManager.PlayerStats.IsAlive()) best = gameManager.PlayerStats;

        if (ShouldSwitchTarget(best))
        {
            currentTarget = best;
            targetLockedUntil = Time.time + TargetStickiness;
        }

        if (!currentTarget.IsAlive())
        {
            destinationSetter.target = null;
        }
        else if (UsesSwingTriggers(currentTarget))
        {
            destinationSetter.target = currentTarget.GetTransform();
        }
        else
        {
            // walk to the near side: the centre of a barrier or cabin sits in unwalkable space
            destinationSetter.target = null;
            aiPath.destination = currentTarget.GetClosestPoint(transform.position);
        }
    }

    private bool ShouldSwitchTarget(IAttackable best)
    {
        if (!currentTarget.IsAlive()) return true;

        if (best == null || best == currentTarget) return false;

        // heading for a cabin and something worth attacking turned up nearby: divert right away
        if (currentTarget.GetPriority() == TargetPriority.Objective && best.GetPriority() == TargetPriority.Diversion) return true;

        // otherwise commit to the current target for a while so zombies don't flip back and forth
        return Time.time >= targetLockedUntil;
    }

    private IAttackable FindNearestDiversion()
    {
        int count = Physics.OverlapSphereNonAlloc(transform.position, aggroRange, targetScanBuffer, AttackableLayer, QueryTriggerInteraction.Ignore);

        IAttackable nearest = null;
        float nearestSqrDistance = float.MaxValue;

        for (int i = 0; i < count; i++)
        {
            IAttackable candidate = targetScanBuffer[i].GetComponentInParent<IAttackable>();

            if (candidate == null || candidate.GetPriority() != TargetPriority.Diversion || !candidate.IsAlive()) continue;

            float sqrDistance = (candidate.GetTransform().position - transform.position).sqrMagnitude;
            if (sqrDistance < nearestSqrDistance)
            {
                nearest = candidate;
                nearestSqrDistance = sqrDistance;
            }
        }

        return nearest;
    }

    private IAttackable FindNearestStructure()
    {
        Structure nearest = null;
        float nearestSqrDistance = float.MaxValue;

        foreach (Structure structure in Structure.Active)
        {
            if (!structure.IsAlive()) continue;

            float sqrDistance = (structure.transform.position - transform.position).sqrMagnitude;
            if (sqrDistance < nearestSqrDistance)
            {
                nearest = structure;
                nearestSqrDistance = sqrDistance;
            }
        }

        return nearest;
    }

    private void UpdateAttackTarget()
    {
        attackTarget = null;

        if (!isDead)
        {
            attackablesInReach.RemoveWhere(target => !target.IsAlive());

            // prefer what we were heading for; otherwise hit whatever is blocking the way
            if (attackablesInReach.Contains(currentTarget))
            {
                attackTarget = currentTarget;
            }
            else if (currentTarget.IsAlive() && !UsesSwingTriggers(currentTarget) && IsWithinReach(currentTarget))
            {
                attackTarget = currentTarget;
            }
            else
            {
                foreach (IAttackable target in attackablesInReach)
                {
                    attackTarget = target;
                    break;
                }
            }
        }

        bool attacking = attackTarget != null;
        if (attacking != isAttacking)
        {
            isAttacking = attacking;
            anim.SetBool("IsAttacking", attacking);
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
        anim.SetBool("CanMove", canMove);
    }

    public void SetAttackableInReach(IAttackable attackable, bool inReach)
    {
        if (inReach) attackablesInReach.Add(attackable);
        else attackablesInReach.Remove(attackable);
    }

    public bool IsAttackableInAttackArea()
    {
        return attackTarget != null;
    }

    // lets turrets prioritize zombies that are hitting a cabin
    public bool IsAttackingObjective()
    {
        return attackTarget != null && attackTarget.GetPriority() == TargetPriority.Objective;
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

    public void SetAttackableInDamageArea(IAttackable attackable, bool inArea)
    {
        if (inArea) attackablesInDamageArea.Add(attackable);
        else attackablesInDamageArea.Remove(attackable);
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
