using Assets.Scripts.Game_Manager;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

public class TurretBase : MonoBehaviour
{
    // weapon stats
    public int WeaponDamage;
    public int WeaponFireRate;
    public float WeaponRange;
  

    //  layers
    public LayerMask ZombieLayer;
    public LayerMask ShootMask;

    // game manager reference
    protected GameManager gameManager;

    // list of enemies in firing area
    [SerializeField] protected List<ZombieStateManager> enemies = new List<ZombieStateManager>();

    // turret transforms  
    public Transform PitchTransform;
    public Transform PanTransform;
    public Transform BarrelTransform;
    public Transform GunEndTransform;
    public float RotationSpeed;

    public ZombieStateManager currentEnemy;

    // hit decals
    public GameObject hitGroundDecal;
    [HideInInspector] public Vector3 ShotForceDir;

    // laser variables
    [SerializeField] private bool laserReady;
    public LineRenderer laserLine;
    public Transform laserOrigin;

    // firing variables
    [Header("Fire Rate")]
    [SerializeField] float fireRate;
    private float fireRateTimer;

    // muzzle flash 
    [SerializeField] private Light muzzleFlashLight;
    ParticleSystem muzzleFlashParticleSystem;
    private float lightIntensity;
    [SerializeField] private float maxLightIntensity;
    [SerializeField] private float minLightIntensity;
    [SerializeField] private float lightReturnSpeed = 20;
    public float range;

    // audio
    private AudioSource turretAudioSource;
    public AudioClip fireSound;
     

    void Start()
    {
        gameManager = GameManager.Instance;

        turretAudioSource = GetComponent<AudioSource>();

        lightIntensity = muzzleFlashLight.intensity;
        muzzleFlashLight.intensity = 0;

    }

    // Update is called once per frame
    void Update()
    {
        FindEnemiesInRange();

        AimAtTarget();

        RotateToDefaultPosition();

        CanFire();

        RotateBarrel();

        muzzleFlashLight.intensity = Mathf.Lerp(muzzleFlashLight.intensity, 0, lightReturnSpeed * Time.deltaTime);

        fireRateTimer += Time.deltaTime;

         

        if (currentEnemy != null)
        {
            laserLine.enabled = true;
            laserLine.SetPosition(0, laserOrigin.position);
            laserLine.SetPosition(1, currentEnemy.transform.position + Vector3.up * 1f);
        }
        else
        {
            laserLine.enabled = false;
        }
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
                PanTransform.rotation = Quaternion.Slerp(PanTransform.rotation, horizontalRotation, Time.deltaTime * RotationSpeed);
            }

            
            if (PitchTransform != null)
            {
                Quaternion neutralPitchRotation = Quaternion.Euler(0, PitchTransform.rotation.eulerAngles.y, 0);
                PitchTransform.rotation = Quaternion.Slerp(PitchTransform.rotation, neutralPitchRotation, Time.deltaTime * RotationSpeed);
            }
        }
    }
    protected virtual void AimAtTarget()
    {
        if(enemies.Count > 0)
        {
            if (currentEnemy == null || !enemies.Contains(currentEnemy))
            {
                currentEnemy = enemies[0];
            }

            Vector3 direction = (currentEnemy.transform.position - transform.position).normalized;

            Vector3 horizontalDirection = new Vector3(direction.x, 0, direction.z);

            Vector3 verticalDirection = new Vector3(0, direction.y, 0);


            if (horizontalDirection != Vector3.zero)
            {
                Quaternion horizontalRotation = Quaternion.LookRotation(horizontalDirection);
                PanTransform.rotation = Quaternion.Slerp(PanTransform.rotation, horizontalRotation, Time.deltaTime * RotationSpeed);
            }

            if (PitchTransform != null)
            {
                Vector3 targetPosition = currentEnemy.transform.parent.position + Vector3.up * 1f;
                Vector3 barrelDirection = targetPosition - PitchTransform.position;

                Quaternion verticalRotation = Quaternion.LookRotation(barrelDirection);
                PitchTransform.rotation = Quaternion.Slerp(PitchTransform.rotation, verticalRotation, Time.deltaTime * RotationSpeed);

                // Lock the barrel's horizontal rotation to match the turret base
                PitchTransform.rotation = Quaternion.Euler(PitchTransform.rotation.eulerAngles.x, PanTransform.rotation.eulerAngles.y, 0);
            }


            // Check alignment for firing
            float horizontalAlignment = Vector3.Dot(PanTransform.forward, horizontalDirection);
            float verticalAlignment = Vector3.Dot(PitchTransform.up, verticalDirection);

         
            if (horizontalAlignment >= 0.7f && CanFire())
            {
                Fire();
                Debug.Log("FIRING");
            }
            else
            {
                Debug.Log("NOT ALIGNED");
                Debug.Log(horizontalAlignment + " " + verticalAlignment);
            }
        }

        else
        {
            currentEnemy = null;          
        }

    }
       

    protected virtual void FindEnemiesInRange()
    {
        enemies.Clear();

        Collider[] colliders = Physics.OverlapSphere(transform.position, WeaponRange, ZombieLayer);

        foreach (Collider col in colliders)
        {
            ZombieStateManager zombie = col.gameObject.GetComponentInParent<ZombieStateManager>();

            if (col.gameObject.GetComponentInParent<ZombieStateManager>().health > 0 && !enemies.Contains(zombie))
                enemies.Add(col.GetComponentInParent<ZombieStateManager>());
        }
    }

    protected virtual bool CanFire()
    {
        fireRateTimer += Time.deltaTime;
        if (fireRateTimer < fireRate) return false;

        if (currentEnemy != null) return true;

        return false;       
    }
    void Fire()
    {
        TriggerMuzzleFlash();

        PlaySfx();

        fireRateTimer = 0;

        Vector3 enemyMediumHeight = currentEnemy.transform.parent.position + Vector3.up * 1f;
        Vector3 direction = (enemyMediumHeight - GunEndTransform.position).normalized;

        if (Physics.Raycast(GunEndTransform.position, direction.normalized, out RaycastHit hit, Mathf.Infinity,
                ShootMask))
        {
            if (hit.collider.CompareTag("Ground") || hit.collider.CompareTag("Environment"))
            {
                Quaternion decalRotation = Quaternion.LookRotation(hit.normal);

                Instantiate(hitGroundDecal, hit.point, decalRotation);
            }

            else if (hit.collider.CompareTag("Zombie"))
            {
                Quaternion decalRotation = Quaternion.LookRotation(hit.normal);
                Instantiate(hitGroundDecal, hit.point, decalRotation);

                ZombieStateManager zombieStateManager;

                Limbs limb = hit.collider.GetComponent<Limbs>();

                ShotForceDir = hit.normal * -1;

                if (limb != null)
                {
                    float baseDamage = 10;
                    float finalDamage = baseDamage * limb.damageMultiplier;

                    if (limb.limbName == "head")
                    {
                        zombieStateManager = hit.collider.GetComponentInParent<ZombieStateManager>();
                        zombieStateManager.TakeDamage((int)finalDamage, limb.limbName, false, 0);
                    }

                    else if (limb.limbName == "torso" || limb.limbName == "belly")
                    {
                        zombieStateManager = hit.collider.GetComponentInParent<ZombieStateManager>();
                        zombieStateManager.TakeDamage((int)finalDamage, limb.limbName, false, 0);
                    }

                    else
                    {
                        zombieStateManager = hit.collider.GetComponentInParent<ZombieStateManager>();
                        zombieStateManager.TakeDamage((int)finalDamage, limb.limbName, false, 0);
                    }


                    float baseLimbDmg = 100f;
                    float finalLimbDmg = baseLimbDmg * limb.limbDamageMultiplier;

                    limb.LimbTakeDamage((int)finalLimbDmg);

                    if (limb.limbReplacement != null && limb.limbHealth <= 0)
                    {

                        Rigidbody rb = limb.limbReplacement.GetComponent<Rigidbody>();
                        Transform trans = limb.limbReplacement.transform;
                        Vector3 backwardForce = direction * 10f;
                        rb.AddForce(backwardForce, ForceMode.Impulse);
                        Debug.Log("force added");
                        Debug.Log("Force direction: " + direction);
                        Debug.DrawRay(trans.position, direction, Color.red, 2f);
                    }
                    Debug.Log(limb.limbName);
                }

            }

            else
            {
                Debug.Log(hit.distance);
            }
        }

        //aimStateManager.AddRecoil();
    }

    void RotateBarrel()
    {
        float rotationSpeed = 100f;
        BarrelTransform.Rotate(0,0, rotationSpeed * Time.deltaTime) ; 
    }
    void TriggerMuzzleFlash()
    {
        //muzzleFlashParticleSystem.Play();
        lightIntensity = Random.Range(minLightIntensity, maxLightIntensity);
        muzzleFlashLight.intensity = lightIntensity;
    }

    void PlaySfx()
    {
        turretAudioSource.PlayOneShot(fireSound);
    }


    private void OnDrawGizmosSelected()
    {
        // Draw a wire sphere in the editor to visualize the detection radius
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, WeaponRange);
    }

}
