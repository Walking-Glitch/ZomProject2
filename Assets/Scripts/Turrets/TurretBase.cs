using Assets.Scripts.Game_Manager;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;


public class TurretBase : NetworkBehaviour
{
    // weapon stats
    [Header("Weapon Stats")]
    public int WeaponDamage;
    public float MinWeaponRange;  
    public float MaxWeaponRange;  
    public float WeaponFireRate;
    public float AimingSpeed;

    // offset var
    protected float minSpreadX;
    protected float maxSpreadX;
    protected float minSpreadY;
    protected float maxSpreadY;

    // firing variables    
    protected float fireRateTimer;


    //  layers
    [Header("Raycasting Layers")]
    public LayerMask ZombieLayer;
    public LayerMask ShootMask;

    // game manager reference
    protected GameManager gameManager;

    // list of enemies in firing area
    [Header("Enemy Tracker")]
    public ZombieStateManager currentEnemy;
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
    
    // spot light 
    [Header("Weapon VFX")]
    public Light WeaponSpotLight;

    // muzzle flash 
    public Transform ParentMuzzleVFX;
    [SerializeField] private List<Transform> muzzleFlashList = new List<Transform>();

    //[SerializeField] protected Light muzzleFlashLight;
    //[SerializeField] protected GameObject [] muzzleFlashParticleObject;
    protected float lightIntensity;
    [SerializeField] private float maxLightIntensity;
    [SerializeField] private float minLightIntensity;
    [SerializeField] private float lightReturnSpeed = 20;


    // audio
    [Header("Weapon SFX")]
    protected AudioSource turretAudioSource;  
    public AudioClip [] fireSound;


    // barrel rotation variables
    [SerializeField] protected bool rotatoryBarrel;

    

    // optimization attempts
    private float checkInterval = 0.2f;
    private float checkTimer;

    [Header("Networked Variables")]
    public NetworkVariable<ulong> targetEnemyId = new NetworkVariable<ulong>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    protected NetworkVariable<Vector3> panRotation = new NetworkVariable<Vector3>();
    protected NetworkVariable<Vector3> pitchRotation = new NetworkVariable<Vector3>();

    private NetworkVariable<bool> laserNetworkReady = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<Vector3> laserEndPosition = new NetworkVariable<Vector3>(Vector3.zero, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<Vector3> laserStartPosition = new NetworkVariable<Vector3>(Vector3.zero, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<Vector3> aimNetworkPosition = new NetworkVariable<Vector3>(Vector3.zero, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);


    protected virtual void Start()
    {
        gameManager = GameManager.Instance;

        turretAudioSource = GetComponent<AudioSource>();

        CollectMuzzleFlashChildObjects(ParentMuzzleVFX);

        SetAimDispersion();

    }

    protected virtual void SetAimDispersion()
    {
        minSpreadX = -0.4f;
        maxSpreadX = 0.4f;
        minSpreadY = 1f;
        maxSpreadY = 2f;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            // The server will own turret logic and update clients
            FindEnemiesInRange();
        }

        // Sync rotations on clients when they change
        panRotation.OnValueChanged += (oldRot, newRot) =>
        {
            PanTransform.rotation = Quaternion.Euler(newRot);
        };

        pitchRotation.OnValueChanged += (oldRot, newRot) =>
        {
            PitchTransform.rotation = Quaternion.Euler(newRot);
        };

        targetEnemyId.OnValueChanged += (oldId, newId) =>
        {
            if (newId == 0)
            {
                currentEnemy = null;
            }
            else
            {
                currentEnemy = NetworkManager.Singleton.SpawnManager.SpawnedObjects[newId].GetComponent<ZombieStateManager>();
            }
        };

        laserNetworkReady.OnValueChanged += (prev, curr) =>
        {
            laserLine.enabled = curr;
        };

        laserEndPosition.OnValueChanged += (prev, curr) =>
        {
            laserLine.SetPosition(1, curr);
        };
        laserStartPosition.OnValueChanged += (prev, curr) =>
        {
            laserLine.SetPosition(0, curr);
        };

        aimNetworkPosition.OnValueChanged += (prev, curr) =>
        {
            LaserAimTransform.position = curr;
        };
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if (!IsServer) return;

        fireRateTimer += Time.deltaTime;
        

        checkTimer += Time.deltaTime;

        if (checkTimer >= checkInterval)
        {
            checkTimer = 0;
            FindEnemiesInRange();
        }

        AimAtTarget();

        RotateToDefaultPosition();

        CanFire();

        RotateBarrel(rotatoryBarrel);

        DisplayLaser();
    }

    
    protected virtual void DisplayLaser()
    {
        if (IsServer)
        {
            if (currentEnemy != null)
            {

                Vector3 laserDirection = GunEndTransform.forward;


                Ray ray = new Ray(GunEndTransform.position, laserDirection);

                if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, ShootMask))
                {
                    LaserAimTransform.gameObject.GetComponent<MeshRenderer>().enabled = true;
                    //LaserAimTransform.position = hit.point;
                    aimNetworkPosition.Value = hit.point;

                }
                laserNetworkReady.Value = true;
                laserStartPosition.Value = laserOrigin.position;
                laserEndPosition.Value = hit.point;
                


                if (WeaponSpotLight != null)
                {
                    WeaponSpotLight.gameObject.SetActive(true);
                    WeaponSpotLight.transform.LookAt(LaserAimTransform.position);
                }

            }
            else
            {
                LaserAimTransform.gameObject.GetComponent<MeshRenderer>().enabled = false;
                laserNetworkReady.Value = false;

                if (WeaponSpotLight != null) WeaponSpotLight.gameObject.SetActive(false);
            }

        }
        else
        {
            if (currentEnemy != null)
            {

                Vector3 laserDirection = GunEndTransform.forward;


                Ray ray = new Ray(GunEndTransform.position, laserDirection);

                if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, ShootMask))
                {
                    LaserAimTransform.gameObject.GetComponent<MeshRenderer>().enabled = true;
                    LaserAimTransform.position = hit.point;
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
                LaserAimTransform.gameObject.GetComponent<MeshRenderer>().enabled = false;
                laserLine.enabled = false;

                if (WeaponSpotLight != null) WeaponSpotLight.gameObject.SetActive(false);
            }

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
        if (!IsServer) return;

        if (enemies.Count > 0 && gameManager.EconomyManager.CheckEnoughFuel())
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
                PanTransform.rotation = Quaternion.Slerp(PanTransform.rotation, horizontalRotation, Time.deltaTime * AimingSpeed);

                // Sync with clients
                panRotation.Value = PanTransform.rotation.eulerAngles;
            }

            if (PitchTransform != null)
            {
                Vector3 targetPosition = currentEnemy.transform.parent.position + Vector3.up * 1.1f;
                Vector3 barrelDirection = (targetPosition - PitchTransform.position).normalized;

                Quaternion verticalRotation = Quaternion.LookRotation(barrelDirection);
                PitchTransform.rotation = Quaternion.Slerp(PitchTransform.rotation, verticalRotation, Time.deltaTime * AimingSpeed);

                // Lock the barrel's horizontal rotation to match the turret base
                PitchTransform.rotation = Quaternion.Euler(PitchTransform.rotation.eulerAngles.x, PanTransform.rotation.eulerAngles.y, 0);


                // Sync with clients
                pitchRotation.Value = PitchTransform.rotation.eulerAngles;
            }


            // Check alignment for firing
            float horizontalAlignment = Vector3.Dot(PanTransform.forward, horizontalDirection);
            float verticalAlignment = Vector3.Dot(PitchTransform.up, verticalDirection);

         
            if (horizontalAlignment >= 0.99f && CanFire())
            {
                Fire(false);
                //Debug.Log("FIRING");
            }
            else
            {
                //Debug.Log("NOT ALIGNED");
                //Debug.Log(horizontalAlignment + " " + verticalAlignment);
            }
        }

        else
        {
            currentEnemy = null;          
        }

    }
       

    protected virtual void FindEnemiesInRange()
    {
        if (!IsServer) return;

        enemies.Clear();

        Collider[] colliders = Physics.OverlapSphere(transform.position, MaxWeaponRange, ZombieLayer);

        foreach (Collider col in colliders)
        {
            float distance = Vector3.Distance(transform.position, col.transform.position);

            if (distance >= MinWeaponRange && distance <= MaxWeaponRange)
            {
                ZombieStateManager zombie = col.gameObject.GetComponentInParent<ZombieStateManager>();

                if (zombie != null)
                {
                    if (col.gameObject.GetComponentInParent<ZombieStateManager>().health > 0 && !enemies.Contains(zombie))
                        enemies.Add(col.GetComponentInParent<ZombieStateManager>());
                }
            }
  
        }

        if (enemies.Count > 0)
        {
            currentEnemy = enemies[0];

            // Sync target to clients
            if (currentEnemy != null)
            {
                targetEnemyId.Value = currentEnemy.GetComponentInParent<NetworkObject>().NetworkObjectId;
            }
        }
        else
        {
            currentEnemy = null;
            targetEnemyId.Value = 0;
        }
    }

    protected virtual bool CanFire()
    {
        if (!IsServer) return false;

        fireRateTimer += Time.deltaTime;       

        if (fireRateTimer < WeaponFireRate) return false;   
        if(!gameManager.EconomyManager.CheckEnoughAmmo()) return false;
        if(!gameManager.EconomyManager.CheckEnoughFuel()) return false;
        if (currentEnemy != null) return true;

        return false;       
    }
   
    protected virtual void Fire(bool hasRecoil)
    {
        if (!IsServer) return; // 

        fireRateTimer = 0;

        // Sync firing state
        //isFiring.Value = true;

        if(!IsServer && !IsClient)
        {
            TriggerMuzzleFlash();
        }
        else
        {
            TriggerMuzzleFlashServerRpc();
        }

        PlaySfx();

        gameManager.EconomyManager.SpendAmmoFromPooledResources(1);

        if (hasRecoil)
        {
            AddRecoil();
        }

        Vector3 enemyMediumHeight = currentEnemy.transform.parent.position + Vector3.up * Random.Range(minSpreadY, maxSpreadY) + Vector3.right * Random.Range(minSpreadX, maxSpreadX);
        Vector3 direction = (enemyMediumHeight - GunEndTransform.position).normalized;

        if (Physics.Raycast(GunEndTransform.position, direction.normalized, out RaycastHit hit, Mathf.Infinity,
                ShootMask))
        {
            if (hit.collider.CompareTag("Ground") || hit.collider.CompareTag("Environment"))
            {
                Quaternion decalRotation = Quaternion.LookRotation(hit.normal);
                if (!IsServer && !IsClient) gameManager.DecalManager.SpawnGroundHitDecal(hit.point, decalRotation);

                else gameManager.DecalManager.SpawnGroundHitDecalServerRpc(hit.point, decalRotation);
            }

            else if (hit.collider.CompareTag("Metal"))
            {
                Quaternion decalRotation = Quaternion.LookRotation(hit.normal);

                if (!IsServer && !IsClient) gameManager.DecalManager.SpawnMetalHitDecal(hit.point, decalRotation);

                else gameManager.DecalManager.SpawnMetalHitDecalServerRpc(hit.point, decalRotation);
            }

            else if (hit.collider.CompareTag("Wood"))
            {
                Quaternion decalRotation = Quaternion.LookRotation(hit.normal);

                if (!IsServer && !IsClient) gameManager.DecalManager.SpawnWoodHitDecal(hit.point, decalRotation);

                else gameManager.DecalManager.SpawnWoodHitDecalServerRpc(hit.point, decalRotation);
            }

            else if (hit.collider.CompareTag("Concrete"))
            {
                Quaternion decalRotation = Quaternion.LookRotation(hit.normal);

                if (!IsServer && !IsClient) gameManager.DecalManager.SpawnConcreteHitDecal(hit.point, decalRotation);

                else gameManager.DecalManager.SpawnConcreteHitDecalServerRpc(hit.point, decalRotation);
            }

            else if (hit.collider.CompareTag("Zombie"))
            {
                Quaternion decalRotation = Quaternion.LookRotation(hit.normal);


                if (!IsServer && !IsClient) gameManager.DecalManager.SpawnBloodHitDecal(hit.point, decalRotation);

              else gameManager.DecalManager.SpawnBloodHitDecalServerRpc(hit.point, decalRotation);


                ZombieStateManager zombieStateManager;

                Limbs limb = hit.collider.GetComponent<Limbs>();

                ShotForceDir = hit.normal * -1;

                if (limb != null)
                {
                    
                    float finalDamage = WeaponDamage * limb.damageMultiplier;

                    if (limb.limbName == "head")
                    {
                        zombieStateManager = hit.collider.GetComponentInParent<ZombieStateManager>();
                        zombieStateManager.TakeDamageServerRpc((int)finalDamage, limb.limbName, false, true ,0);
                    }

                    else if (limb.limbName == "torso" || limb.limbName == "belly")
                    {
                        zombieStateManager = hit.collider.GetComponentInParent<ZombieStateManager>();
                        zombieStateManager.TakeDamageServerRpc((int)finalDamage, limb.limbName, false, true, 0);
                    }

                    else
                    {
                        zombieStateManager = hit.collider.GetComponentInParent<ZombieStateManager>();
                        zombieStateManager.TakeDamageServerRpc((int)finalDamage, limb.limbName, false, true, 0);
                    }


                    float baseLimbDmg = 100f;
                    float finalLimbDmg = baseLimbDmg * limb.limbDamageMultiplier;

                    limb.LimbTakeDamageServerRpc((int)finalLimbDmg);

                    if (limb.limbReplacement != null && limb.limbHealth <= 0)
                    {

                        Rigidbody rb = limb.limbReplacement.GetComponent<Rigidbody>();
                        Transform trans = limb.limbReplacement.transform;
                        Vector3 backwardForce = direction * 10f;
                        rb.AddForce(backwardForce, ForceMode.Impulse);
                        //Debug.Log("force added");
                        //Debug.Log("Force direction: " + direction);
                        //Debug.DrawRay(trans.position, direction, Color.red, 2f);
                    }
                    //Debug.Log(limb.limbName);
                }

            }

            else
            {
                //Debug.Log(hit.distance);
            }
        }


        //
        //isFiring.Value = false;

    }

    protected virtual void RotateBarrel(bool rotatoryBarrel)
    {
        if (rotatoryBarrel && BarrelTransform != null)
        {
            float rotationSpeed = 100f;
            BarrelTransform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
        }
       
    }

    protected void CollectMuzzleFlashChildObjects(Transform parentMuzzleFlash)
    {
        if(parentMuzzleFlash != null)
        {
            foreach (Transform muzzle in parentMuzzleFlash)
            {
                muzzleFlashList.Add(muzzle.transform);
            }
        }
    }
    protected void TriggerMuzzleFlash()
    {
        int index = Random.Range(0, muzzleFlashList.Count);
        muzzleFlashList[index].gameObject.SetActive(true);

        lightIntensity = Random.Range(minLightIntensity, maxLightIntensity);

        FPSLightCurves lightCurves = GetComponentInChildren<FPSLightCurves>();

        if (lightCurves != null)
        {
            lightCurves.GraphIntensityMultiplier = lightIntensity;
        } 
    }

    [ClientRpc]
    protected void TriggerMuzzleFlashClientRpc()
    {
        int index = Random.Range(0, muzzleFlashList.Count);
        muzzleFlashList[index].gameObject.SetActive(true);

        lightIntensity = Random.Range(minLightIntensity, maxLightIntensity);

        FPSLightCurves lightCurves = GetComponentInChildren<FPSLightCurves>();

        if (lightCurves != null)
        {
            lightCurves.GraphIntensityMultiplier = lightIntensity;
        }
    }

    [ServerRpc]
    public void TriggerMuzzleFlashServerRpc()
    {
        TriggerMuzzleFlashClientRpc();
    }


    protected void PlaySfx()
    {
        if(fireSound.Length > 0)
        {
            int index = Random.Range(0, fireSound.Length);
            turretAudioSource.PlayOneShot(fireSound[index]);
        }
       
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
