using Assets.Scripts.Game_Manager;
using Assets.Scripts.Player.Actions;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

namespace Assets.Scripts.Player.Weapon
{
    public class WeaponManager : MonoBehaviour
    {
        // Weapon model reference
        public GameObject rifle;
        private Transform cachedParent;
        private Vector3 cachedPosition;
        private Quaternion cachedRotation;

        // ray casting variables 
        [SerializeField] private LayerMask shootMask;
        public Transform GunEndTransform;
        public Transform TargetTransform;
        public Transform weaponTransform;
        public Transform RightHandTransform;
        public Transform LeftHandTransform;
        public Transform GrenadeSpawnTransform;

        // reference to laser 
        [HideInInspector] public WeaponLaser laser;

        // input system
        private InputSystem_Actions inputSystemActions;

        // reference to state managers
        private AimStateManager aimStateManager;
        private MovementStateManager moveStateManager;
        private ActionStateManager actionStateManager;

        // decals 
        public GameObject hitGroundDecal;
        public GameObject fleshDecal;

        // audio variables
        public AudioSource RifleAudioSource;
        public AudioClip [] gunShots;
        public AudioClip emptyClip;
        
        private bool playedEmptySound = false;


        // firing variables
        [Header("Fire Rate")]
        [SerializeField] float fireRate;
        private float fireRateTimer;

        private Light muzzleFlashLight;
        ParticleSystem muzzleFlashParticleSystem;
        private float lightIntensity;
        [SerializeField] private float lightReturnSpeed = 20;

        // animator
        private Animator anim;

        // zombie reference
        ZombieStateManager zombieStateManager;

        // game manager
        private GameManager gameManager;

        void Awake()
        {
            inputSystemActions = new InputSystem_Actions();
            inputSystemActions.Player.Attack.performed += OnFirePerformed;
        }

        void Start()
        {
            gameManager = GameManager.Instance;
            anim = GetComponent<Animator>();
            aimStateManager = GetComponent<AimStateManager>();
            moveStateManager = GetComponent<MovementStateManager>();
            actionStateManager = GetComponent<ActionStateManager>();
            laser = GetComponent<WeaponLaser>();

            cachedParent = rifle.transform.parent;
            cachedPosition = rifle.transform.localPosition;
            cachedRotation = rifle.transform.localRotation;
    }

        // Update is called once per frame
        void Update()
        {
            fireRateTimer += Time.deltaTime;

        }

        public void AdjustParentedHand()
        {
            if (actionStateManager.CurrentState == actionStateManager.Grenade)
            {
                rifle.transform.SetParent(LeftHandTransform);
            }

            else if (actionStateManager.CurrentState == actionStateManager.Reload)
            {
                rifle.transform.SetParent(cachedParent);
                rifle.transform.localPosition = cachedPosition;
                rifle.transform.localRotation = cachedRotation;
            }

            else if (actionStateManager.CurrentState == actionStateManager.Default)
            {
                rifle.transform.SetParent(cachedParent);
                rifle.transform.localPosition = cachedPosition;
                rifle.transform.localRotation = cachedRotation;
            }
        }


        void Fire()
        {
            anim.SetTrigger("Firing");
            rifle.GetComponent<Animation>().Play();
            gameManager.CasingManager.SpawnBulletCasing();
            fireRateTimer = 0;
            RifleAudioSource.PlayOneShot(gunShots[Random.Range(0, gunShots.Length)]);
            gameManager.WeaponAmmo.currentAmmo--;
            //TriggerMuzzleFlash();

            Vector3 direction = TargetTransform.position - GunEndTransform.position;
            if (Physics.Raycast(GunEndTransform.position, direction.normalized, out RaycastHit hit, Mathf.Infinity,
                    shootMask))
            {
                if (hit.collider.CompareTag("Ground"))
                {
                    Quaternion decalRotation = Quaternion.LookRotation(hit.normal);

                    Instantiate(hitGroundDecal, hit.point, decalRotation);
                }

                else if (hit.collider.CompareTag("Zombie"))
                {
                    Quaternion decalRotation = Quaternion.LookRotation(hit.normal);
                    Instantiate(hitGroundDecal, hit.point, decalRotation);

                
                    Limbs limb = hit.collider.GetComponent<Limbs>();

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

                        Rigidbody rb = hit.collider.GetComponentInParent<Rigidbody>();

                        if (rb != null)
                        {
                            rb.AddForce(hit.normal * -1 * 100f, ForceMode.Impulse); 
                        }
                        Debug.Log(limb.limbName);
                    }
               
                }

                else
                {
                    Debug.Log(hit.distance);
                }
            }
        }

        bool CanFire()
        {
        
            if (fireRateTimer < fireRate) return false;
            if (gameManager.WeaponAmmo.currentAmmo == 0)
            {
                if (!playedEmptySound)
                 {
                   RifleAudioSource.PlayOneShot(emptyClip);
                   playedEmptySound = true;
                 }         
                else
                {
                    playedEmptySound = false;
                }
                return false;
            }
            if (moveStateManager.currentState == moveStateManager.Run) return false;
            if (actionStateManager.CurrentState == actionStateManager.Reload) return false;
            if (actionStateManager.CurrentState == actionStateManager.Grenade) return false;
            if (aimStateManager.CurrentState == aimStateManager.AimingState) return true;
            //if (!semiAuto && Input.GetKey(KeyCode.Mouse0)) return true;
            return false;
        }

        private void OnFirePerformed(InputAction.CallbackContext context)
        {
            if(CanFire()) Fire();
        }

        void TriggerMuzzleFlash()
        {
            muzzleFlashParticleSystem.Play();
            muzzleFlashLight.intensity = lightIntensity;
        }

        void OnEnable()
        {
            inputSystemActions.Enable();
        }

        void OnDisable()
        {
            inputSystemActions.Disable();
        }

     
    }
}
