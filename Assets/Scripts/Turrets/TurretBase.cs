using Assets.Scripts.Game_Manager;
using System.Collections.Generic;
using UnityEngine;


public class TurretBase : MonoBehaviour, IAttackable
{
    // health, zombies can destroy turrets
    [Header("Health")]
    [Tooltip("Health the turret is built with. Zombies hit for their Attack Damage (10) per swing.")]
    public int MaxHealth = 400;
    [Tooltip("Current health (runtime). Editing it does not kill; only damage triggers death.")]
    public int Health;
    private Collider[] solidColliders;

    // weapon stats
    [Header("Weapon Stats")]
    [Tooltip("Damage per hit before the limb multiplier (head x10, torso x2, belly x1.5, legs x0.8, arms x0.5, hands/feet x0.2).")]
    public int WeaponDamage;
    [Tooltip("Zombies closer than this (m) are ignored; the long barrel can't aim at its own base.")]
    public float MinWeaponRange;
    [Tooltip("Zombies farther than this (m) are ignored.")]
    public float MaxWeaponRange;
    [Tooltip("Seconds between shots.")]
    public float WeaponFireRate;
    [Tooltip("How fast the turret swings toward a far-off target. Higher = snappier turn-in.")]
    public float AimingSpeed;
    [Tooltip("Minimum turn speed in degrees/second once close to the target. Lets the barrel land exactly on a moving zombie and stay on it; must beat how fast zombies cross the turret's view.")]
    public float TrackingSpeed = 90f;
    [Tooltip("The turret only fires when the barrel points within this many degrees of the target.")]
    public float FireAngleTolerance = 2f;
    [Tooltip("Random miss offset in meters around the zombie's torso for each shot (realism). 0 = every shot dead center.")]
    public float ShotSpread = 0.5f;
    [Tooltip("Seconds between target scans. Lower = reacts faster to new zombies, costs more CPU.")]
    [SerializeField] private float checkInterval = 0.2f;

    // firing variables    
    protected float fireRateTimer;


    //  layers
    [Header("Raycasting Layers")]
    [Tooltip("Layers scanned for zombies in range.")]
    public LayerMask ZombieLayer;
    [Tooltip("Layers that stop bullets and block line of sight (Ground, Environment, Zombie). Layers left out are shot through.")]
    public LayerMask ShootMask;

    // game manager reference
    protected GameManager gameManager;

    // list of enemies in firing area
    [Header("Enemy Tracker")]
    [Tooltip("Zombie the turret is currently aiming at (runtime).")]
    public ZombieStateManager currentEnemy;
    [Tooltip("Live zombies inside the weapon range from the last scan (runtime).")]
    [SerializeField] protected List<ZombieStateManager> enemies = new List<ZombieStateManager>();
    

    // turret transforms  
    [Header("Weapon Transforms")] 
    public Transform PitchTransform;
    public Transform PanTransform;
    public Transform BarrelTransform;
    public Transform GunEndTransform;



    // hit decals
    [Header("Impact Decals")]
    public GameObject hitGroundDecal;
    [HideInInspector] public Vector3 ShotForceDir;

    // laser variables
    [Header("Weapon Laser")]    
    public LineRenderer laserLine;
    public Transform laserOrigin;
    public Transform LaserAimTransform;
    private MeshRenderer laserAimRenderer;
    private bool isOnTarget;

    // spot light 
    [Header("Weapon VFX")]
    public Light WeaponSpotLight;

    // muzzle flash 
    public Transform ParentMuzzleVFX;
    [SerializeField] private List<Transform> muzzleFlashList = new List<Transform>();
    private List<FPSLightCurves> muzzleLightCurves = new List<FPSLightCurves>();

    //[SerializeField] protected Light muzzleFlashLight;
    //[SerializeField] protected GameObject [] muzzleFlashParticleObject;
    protected float lightIntensity;
    [Tooltip("Brightest muzzle flash light (random between min and max per shot).")]
    [SerializeField] private float maxLightIntensity;
    [Tooltip("Dimmest muzzle flash light.")]
    [SerializeField] private float minLightIntensity;
    [SerializeField] private float lightReturnSpeed = 20;


    // tracers
    [Header("Tracers")]
    [Tooltip("Draw a tracer streak for every shot.")]
    public bool ShowTracers = true;
    [Tooltip("Tracer material. Empty = a neutral copy of the laser's glow material, colored by Tracer Color. A custom material's own tint/texture multiplies Tracer Color, so keep its tint white/grey.")]
    public Material TracerMaterial;
    [Tooltip("Tracer color; alpha sets brightness.")]
    public Color TracerColor = new Color(1f, 0.75f, 0.35f, 1f);
    [Tooltip("Tracer thickness in meters.")]
    public float TracerWidth = 0.04f;
    [Tooltip("How fast the streak flies, in m/s. Visual only; damage is still instant.")]
    public float TracerSpeed = 300f;
    [Tooltip("Length of the streak in meters.")]
    public float TracerLength = 4f;

    // audio
    [Header("Weapon SFX")]
    protected AudioSource turretAudioSource;  
    public AudioClip [] fireSound;


    // barrel rotation variables
    [Header("Rotary Barrel")]
    [Tooltip("Spin the barrel continuously (Vulcan).")]
    [SerializeField] protected bool rotatoryBarrel;
    [Tooltip("Barrel spin speed in degrees per second.")]
    [SerializeField] protected float barrelSpinSpeed = 100f;

    // optimization attempts
    private float checkTimer;
    private System.Comparison<ZombieStateManager> targetOrder;

    protected virtual void Start()
    {
        gameManager = GameManager.Instance;

        targetOrder = CompareTargets;

        Health = MaxHealth;
        solidColliders = Targeting.GetSolidColliders(gameObject);

        turretAudioSource = GetComponent<AudioSource>();

        laserAimRenderer = LaserAimTransform.GetComponent<MeshRenderer>();

        CollectMuzzleFlashChildObjects(ParentMuzzleVFX);
         

    }

    // Update is called once per frame
    protected virtual void Update()
    {
        fireRateTimer += Time.deltaTime;
        

        checkTimer += Time.deltaTime;

        if (checkTimer >= checkInterval)
        {
            checkTimer = 0;
            FindEnemiesInRange();
        }

        AimAtTarget();

        RotateToDefaultPosition();

        RotateBarrel(rotatoryBarrel);

        DisplayLaser();
    }

    
    protected virtual void DisplayLaser()
    {
       
        if (currentEnemy != null)
        {

            // the laser shows where the barrel really points; tracking keeps that on the zombie
            Ray ray = new Ray(GunEndTransform.position, GunEndTransform.forward);

            if (Physics.Raycast(ray, out RaycastHit hit, MaxWeaponRange * 2f, ShootMask, QueryTriggerInteraction.Ignore))
            {
                laserAimRenderer.enabled = true;
                LaserAimTransform.position = hit.point;
            }
            else
            {
                // nothing hit (sky): end the beam in the air instead of leaving the dot at an old spot
                laserAimRenderer.enabled = false;
                LaserAimTransform.position = ray.GetPoint(MaxWeaponRange);
            }

            laserLine.enabled = true;
            laserLine.SetPosition(0, laserOrigin.position);
            laserLine.SetPosition(1, LaserAimTransform.position);

            if (WeaponSpotLight != null)
            {
                WeaponSpotLight.gameObject.SetActive(true);
                WeaponSpotLight.transform.LookAt(LaserAimTransform.position);
            }

        }
        else
        {
            laserAimRenderer.enabled = false;
            laserLine.enabled = false;

            if (WeaponSpotLight != null) WeaponSpotLight.gameObject.SetActive(false);
        }
    

}

protected virtual void AddRecoil()
    {
        PitchTransform.Rotate(-25, 0, 0);
    }

    protected virtual void RotateToDefaultPosition()
    {
        if (currentEnemy == null)
        {
            // Define the default direction or rotation
            Vector3 defaultDirection = transform.forward;  
            Vector3 horizontalDefaultDirection = new Vector3(defaultDirection.x, 0, defaultDirection.z).normalized;

            if (horizontalDefaultDirection != Vector3.zero)
            {
                Quaternion horizontalRotation = Quaternion.LookRotation(horizontalDefaultDirection);
                PanTransform.rotation = Quaternion.Slerp(PanTransform.rotation, horizontalRotation, Time.deltaTime * AimingSpeed);
            }

            
            if (PitchTransform != null)
            {
                Quaternion neutralPitchRotation = Quaternion.Euler(0, PitchTransform.rotation.eulerAngles.y, 0);
                PitchTransform.rotation = Quaternion.Slerp(PitchTransform.rotation, neutralPitchRotation, Time.deltaTime * AimingSpeed);
            }
        }
    }
    protected virtual void AimAtTarget()
    {
        if(enemies.Count > 0 && gameManager.EconomyManager.CheckEnoughFuel())
        {
            // target died between scans
            if (!IsValidTarget(currentEnemy))
            {
                SelectTarget();
                if (currentEnemy == null) return;
            }

            if (TrackTarget(currentEnemy) && CanFire())
            {
                Fire(false);
            }
        }

        else
        {
            currentEnemy = null;          
        }

    }
       

    // pans and tilts toward the zombie's torso; returns true once the barrel is on target
    protected bool TrackTarget(ZombieStateManager target)
    {
        Vector3 aimPoint = AimPoint(target);

        Vector3 toTarget = aimPoint - PanTransform.position;
        Vector3 horizontalDirection = new Vector3(toTarget.x, 0, toTarget.z);

        if (horizontalDirection != Vector3.zero)
        {
            PanTransform.rotation = TurnTowards(PanTransform.rotation, Quaternion.LookRotation(horizontalDirection));
        }

        float panYaw = PanTransform.rotation.eulerAngles.y;

        if (PitchTransform == null)
        {
            return Vector3.Angle(PanTransform.forward, horizontalDirection) <= FireAngleTolerance;
        }

        // pitch only; the yaw always follows the pan so the two rotations never fight
        Vector3 toTargetFromPitch = aimPoint - PitchTransform.position;
        float horizontalDistance = new Vector2(toTargetFromPitch.x, toTargetFromPitch.z).magnitude;
        float pitchAngle = -Mathf.Atan2(toTargetFromPitch.y, horizontalDistance) * Mathf.Rad2Deg;

        Quaternion currentPitch = Quaternion.Euler(PitchTransform.rotation.eulerAngles.x, panYaw, 0);
        PitchTransform.rotation = TurnTowards(currentPitch, Quaternion.Euler(pitchAngle, panYaw, 0));

        // also waits for the barrel to settle after recoil
        return Vector3.Angle(PitchTransform.forward, toTargetFromPitch) <= FireAngleTolerance;
    }

    // swings fast while far off (like a slerp), but never slower than TrackingSpeed, so the barrel
    // lands exactly on the target and keeps up with it instead of trailing a moving zombie
    protected Quaternion TurnTowards(Quaternion from, Quaternion to)
    {
        float angle = Quaternion.Angle(from, to);
        float step = Mathf.Max(angle * AimingSpeed, TrackingSpeed) * Time.deltaTime;

        return Quaternion.RotateTowards(from, to, step);
    }

    protected virtual void FindEnemiesInRange()
    {
        enemies.Clear();

        Collider[] colliders = Physics.OverlapSphere(transform.position, MaxWeaponRange, ZombieLayer);

        foreach (Collider col in colliders)
        {
            float distance = Vector3.Distance(transform.position, col.transform.position);

            if (distance >= MinWeaponRange && distance <= MaxWeaponRange)
            {
                ZombieStateManager zombie = col.gameObject.GetComponentInParent<ZombieStateManager>();

                if (IsValidTarget(zombie) && !enemies.Contains(zombie))
                    enemies.Add(zombie);
            }
        }

        SelectTarget();
    }

    // keeps the current target unless it's gone or hidden, or a zombie hitting a cabin shows up;
    // otherwise takes the closest visible zombie, cabin attackers first
    protected virtual void SelectTarget()
    {
        bool currentValid = IsValidTarget(currentEnemy) && enemies.Contains(currentEnemy) && HasLineOfSight(currentEnemy);

        if (currentValid && currentEnemy.IsAttackingObjective()) return;

        enemies.Sort(targetOrder);

        foreach (ZombieStateManager zombie in enemies)
        {
            // only a cabin attacker is worth pulling the turret off a valid target
            if (currentValid && !zombie.IsAttackingObjective()) return;

            if (!IsValidTarget(zombie) || zombie == currentEnemy) continue;

            if (HasLineOfSight(zombie))
            {
                currentEnemy = zombie;
                return;
            }
        }

        if (!currentValid) currentEnemy = null;
    }

    private int CompareTargets(ZombieStateManager a, ZombieStateManager b)
    {
        bool aObjective = a.IsAttackingObjective();
        bool bObjective = b.IsAttackingObjective();
        if (aObjective != bObjective) return aObjective ? -1 : 1;

        float aDistance = (a.transform.position - transform.position).sqrMagnitude;
        float bDistance = (b.transform.position - transform.position).sqrMagnitude;
        return aDistance.CompareTo(bDistance);
    }

    protected bool IsValidTarget(ZombieStateManager zombie)
    {
        return zombie != null && zombie.gameObject.activeInHierarchy && !zombie.isDead && zombie.health > 0;
    }

    protected Vector3 AimPoint(ZombieStateManager zombie)
    {
        return zombie.GetAimPoint();
    }

    // terrain and props block the shot; another zombie in the way still takes the bullet
    protected bool HasLineOfSight(ZombieStateManager zombie)
    {
        Vector3 origin = PitchTransform != null ? PitchTransform.position : GunEndTransform.position;
        Vector3 toTarget = AimPoint(zombie) - origin;

        if (!Physics.Raycast(origin, toTarget, out RaycastHit hit, toTarget.magnitude, ShootMask, QueryTriggerInteraction.Ignore))
            return true;

        return (ZombieLayer.value & (1 << hit.collider.gameObject.layer)) != 0;
    }

    protected virtual bool CanFire()
    {
        if (fireRateTimer < WeaponFireRate) return false;
        if(!gameManager.EconomyManager.CheckEnoughAmmo()) return false;
        if(!gameManager.EconomyManager.CheckEnoughFuel()) return false;
        if (currentEnemy != null) return true;

        return false;       
    }
    protected virtual void Fire(bool hasRecoil)
    {

        fireRateTimer = 0;

        TriggerMuzzleFlash();

        PlaySfx();

        gameManager.EconomyManager.SpendAmmoFromPooledResources(1);

        if (hasRecoil)
        {
            AddRecoil();
        }

        // realism: each shot lands somewhere around the torso, sideways relative to the barrel
        Vector3 shotPoint = AimPoint(currentEnemy)
            + Vector3.up * Random.Range(-ShotSpread, ShotSpread)
            + GunEndTransform.right * Random.Range(-ShotSpread, ShotSpread);
        Vector3 direction = (shotPoint - GunEndTransform.position).normalized;

        bool hitSomething = Physics.Raycast(GunEndTransform.position, direction, out RaycastHit hit, Mathf.Infinity,
                ShootMask, QueryTriggerInteraction.Ignore);

        SpawnTracer(hitSomething ? hit.point : GunEndTransform.position + direction * MaxWeaponRange);

        if (hitSomething)
        {
            if (hit.collider.CompareTag("Ground") || hit.collider.CompareTag("Environment"))
            {
                Quaternion decalRotation = Quaternion.LookRotation(hit.normal);
                gameManager.DecalManager.SpawnGroundHitDecal(hit.point, decalRotation);
            }

            else if (hit.collider.CompareTag("Metal"))
            {
                Quaternion decalRotation = Quaternion.LookRotation(hit.normal);

                gameManager.DecalManager.SpawnMetalHitDecal(hit.point, decalRotation);
            }

            else if (hit.collider.CompareTag("Wood"))
            {
                Quaternion decalRotation = Quaternion.LookRotation(hit.normal);

                gameManager.DecalManager.SpawnWoodHitDecal(hit.point, decalRotation);
            }

            else if (hit.collider.CompareTag("Concrete"))
            {
                Quaternion decalRotation = Quaternion.LookRotation(hit.normal);

                gameManager.DecalManager.SpawnConcreteHitDecal(hit.point, decalRotation);
            }

            else if (hit.collider.CompareTag("Zombie"))
            {
                Quaternion decalRotation = Quaternion.LookRotation(hit.normal);
                gameManager.DecalManager.SpawnBloodHitDecal(hit.point, decalRotation);

                ZombieStateManager zombieStateManager;

                Limbs limb = hit.collider.GetComponent<Limbs>();

                ShotForceDir = hit.normal * -1;

                if (limb != null)
                {
                    
                    float finalDamage = WeaponDamage * limb.damageMultiplier;

                    if (limb.limbName == "head")
                    {
                        zombieStateManager = hit.collider.GetComponentInParent<ZombieStateManager>();
                        zombieStateManager.TakeDamage((int)finalDamage, limb.limbName, false, true ,0);
                    }

                    else if (limb.limbName == "torso" || limb.limbName == "belly")
                    {
                        zombieStateManager = hit.collider.GetComponentInParent<ZombieStateManager>();
                        zombieStateManager.TakeDamage((int)finalDamage, limb.limbName, false, true, 0);
                    }

                    else
                    {
                        zombieStateManager = hit.collider.GetComponentInParent<ZombieStateManager>();
                        zombieStateManager.TakeDamage((int)finalDamage, limb.limbName, false, true, 0);
                    }


                    float baseLimbDmg = 100f;
                    float finalLimbDmg = baseLimbDmg * limb.limbDamageMultiplier;

                    limb.LimbTakeDamage((int)finalLimbDmg);

                    if (limb.limbReplacement != null && limb.limbHealth <= 0)
                    {

                        Rigidbody rb = limb.limbReplacement.GetComponent<Rigidbody>();
                        Vector3 backwardForce = direction * 10f;
                        rb.AddForce(backwardForce, ForceMode.Impulse);
                    }
                    //Debug.Log(limb.limbName);
                }

            }

            else
            {
                //Debug.Log(hit.distance);
            }
        }

         
    }

    protected void SpawnTracer(Vector3 end)
    {
        if (!ShowTracers) return;

        // falls back to a neutral copy of the laser's glow material so tracers work without extra setup
        Material material = TracerMaterial != null
            ? TracerMaterial
            : TracerPool.GetDefaultMaterial(laserLine != null ? laserLine.sharedMaterial : null);

        TracerPool.Spawn(GunEndTransform.position, end, material, TracerColor, TracerWidth, TracerSpeed, TracerLength);
    }

    protected virtual void RotateBarrel(bool rotatoryBarrel)
    {
        if (rotatoryBarrel && BarrelTransform != null)
        {
            BarrelTransform.Rotate(0, 0, barrelSpinSpeed * Time.deltaTime);
        }
       
    }

    protected void CollectMuzzleFlashChildObjects(Transform parentMuzzleFlash)
    {
        if(parentMuzzleFlash != null)
        {
            foreach (Transform muzzle in parentMuzzleFlash)
            {
                muzzleFlashList.Add(muzzle.transform);

                // flashes start inactive, so include inactive children
                muzzleLightCurves.Add(muzzle.GetComponentInChildren<FPSLightCurves>(true));
            }
        }
    }
    protected void TriggerMuzzleFlash()
    {
        int index = Random.Range(0, muzzleFlashList.Count);
        muzzleFlashList[index].gameObject.SetActive(true);

        lightIntensity = Random.Range(minLightIntensity, maxLightIntensity);

        FPSLightCurves lightCurves = muzzleLightCurves[index];

        if (lightCurves != null)
        {
            lightCurves.GraphIntensityMultiplier = lightIntensity;
        } 
    }

    protected void PlaySfx()
    {
        if(fireSound.Length > 0)
        {
            int index = Random.Range(0, fireSound.Length);
            turretAudioSource.PlayOneShot(fireSound[index]);
        }

    }

    public Transform GetTransform()
    {
        return transform;
    }

    public Vector3 GetClosestPoint(Vector3 from)
    {
        return Targeting.ClosestPointOnColliders(solidColliders, from, transform.position);
    }

    public int GetHealth()
    {
        return Health;
    }

    public int GetPriority()
    {
        return TargetPriority.Diversion;
    }

    public void TakeDamage(int amount)
    {
        if (Health <= 0) return;

        Health -= amount;
        Health = Mathf.Clamp(Health, 0, MaxHealth);

        if (Health <= 0)
        {
            Death();
        }
    }

    public void Death()
    {
        // stop the destroyed turret from draining fuel
        gameManager.EconomyManager.RemoveTurret(this);

        TargetCleanup.RemoveAndRefreshGraph(gameObject, true);
    }


    private void OnDrawGizmosSelected()
    {
        // Draw the max range in red
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, MaxWeaponRange);

        // Draw the min range in yellow
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, MinWeaponRange);
    }

}
