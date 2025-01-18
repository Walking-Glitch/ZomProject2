using Assets.Scripts.Game_Manager;
using Assets.Scripts.Player.Actions;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;
using Random = UnityEngine.Random;

namespace Assets.Scripts.Player.Weapon
{
    public class WeaponManager : MonoBehaviour
    {
        [HideInInspector]public Vector3 ShotForceDir;

        // Weapon model reference
        public GameObject rifle;
        public GameObject Mag;
        private Transform cachedWeaponParent;
        private Vector3 cachedWeaponPosition;
        private Quaternion cachedWeaponRotation;

        private Transform cachedMagParent;
        private Vector3 cachedMagPosition;
        private Quaternion cachedMagRotation;

        // ray casting variables 
        [SerializeField] private LayerMask shootMask;
        public Transform GunEndTransform;
        public Transform TargetTransform;

        // position variables
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
        public AudioSource ToggleWeaponAudioSource;
        public AudioClip [] gunShots;
        public AudioClip emptyClip;
        
        private bool playedEmptySound = false;


        // firing variables
        [Header("Fire Rate")]
        [SerializeField] float fireRate;
        private float fireRateTimer;

        [SerializeField] private Light muzzleFlashLight;
        ParticleSystem muzzleFlashParticleSystem;
        private float lightIntensity;
        [SerializeField] private float maxLightIntensity;
        [SerializeField] private float minLightIntensity;
        [SerializeField] private float lightReturnSpeed = 20;

        // animator
        private Animator anim;

        // zombie reference
        ZombieStateManager zombieStateManager;

        // toggle firing mode
        [SerializeField] private bool semiAuto;
        private bool isFiring = false;


        // game manager
        private GameManager gameManager;


        void Awake()
        {
            inputSystemActions = new InputSystem_Actions();
            inputSystemActions.Player.Attack.performed += OnFirePerformed;
            inputSystemActions.Player.Attack.canceled += OnFireCanceled;


            gameManager = GameManager.Instance;
            anim = GetComponent<Animator>();
            aimStateManager = GetComponent<AimStateManager>();
            moveStateManager = GetComponent<MovementStateManager>();
            actionStateManager = GetComponent<ActionStateManager>();
            laser = GetComponent<WeaponLaser>();

            cachedWeaponParent = rifle.transform.parent;
            cachedWeaponPosition = rifle.transform.localPosition;
            cachedWeaponRotation = rifle.transform.localRotation;

            cachedMagParent = Mag.transform.parent;
            cachedMagPosition = Mag.transform.localPosition;
            cachedMagRotation = Mag.transform.localRotation;
        }

        void Start()
        {
            lightIntensity = muzzleFlashLight.intensity;
            muzzleFlashLight.intensity = 0;
        }

        // Update is called once per frame
        void Update()
        {
            ToogleFireRate();

            if (!semiAuto && isFiring && CanFire())
            {
                Fire();
            }

            fireRateTimer += Time.deltaTime;

            muzzleFlashLight.intensity = Mathf.Lerp(muzzleFlashLight.intensity, 0, lightReturnSpeed * Time.deltaTime);

        }

        public void RemoveMag()
        {           
            Mag.transform.SetParent(LeftHandTransform);
            Mag.transform.SetLocalPositionAndRotation(new Vector3(-0.133599997f, 0.0542000011f, -0.0315999985f), Quaternion.Euler(25.4739895f, 230.443344f, 212.954651f));
        }

        public void AttachMag()
        {
                Mag.transform.SetParent(cachedMagParent);
                Mag.transform.localPosition = cachedMagPosition;
                Mag.transform.localRotation = cachedMagRotation;
        }

        public void AdjustWeaponParentedHand()
        {
            if (actionStateManager.CurrentState == actionStateManager.Grenade)
            {
                rifle.transform.SetParent(LeftHandTransform);
            }

            else if (actionStateManager.CurrentState == actionStateManager.Reload)
            {
                rifle.transform.SetParent(cachedWeaponParent);
                rifle.transform.localPosition = cachedWeaponPosition;
                rifle.transform.localRotation = cachedWeaponRotation;
            }

            else if (actionStateManager.CurrentState == actionStateManager.Default)
            {
           
                    rifle.transform.SetParent(cachedWeaponParent);
                    rifle.transform.localPosition = cachedWeaponPosition;
                    rifle.transform.localRotation = cachedWeaponRotation;
                
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
            TriggerMuzzleFlash();

            Vector3 direction = TargetTransform.position - GunEndTransform.position;
            if (Physics.Raycast(GunEndTransform.position, direction.normalized, out RaycastHit hit, Mathf.Infinity,
                    shootMask))
            {
                if (hit.collider.CompareTag("Ground"))
                {
                    Quaternion decalRotation = Quaternion.LookRotation(hit.normal);

                    gameManager.DecalManager.SpawnGroundHitDecal(hit.point, decalRotation);
                }

                else if (hit.collider.CompareTag("Zombie"))
                {
                    Quaternion decalRotation = Quaternion.LookRotation(hit.normal);
                     
                    gameManager.DecalManager.SpawnBloodHitDecal(hit.point, decalRotation);
                     


                    ShotForceDir = hit.normal * -1;

                    Limbs limb = hit.collider.GetComponent<Limbs>();

                    if (limb != null)
                    {
                        float baseDamage = 40;
                        float finalDamage = baseDamage * limb.damageMultiplier;

                        if (limb.limbName == "head")
                        {
                            zombieStateManager = hit.collider.GetComponentInParent<ZombieStateManager>();
                            zombieStateManager.TakeDamage((int)finalDamage, limb.limbName, false, false, 0);
                        }                       
                    
                    else if (limb.limbName == "torso" || limb.limbName == "belly")
                    {
                        zombieStateManager = hit.collider.GetComponentInParent<ZombieStateManager>();
                        zombieStateManager.TakeDamage((int)finalDamage, limb.limbName, false, false, 0);
                    }

                    else
                    {
                        zombieStateManager = hit.collider.GetComponentInParent<ZombieStateManager>();
                        zombieStateManager.TakeDamage((int)finalDamage, limb.limbName, false, false, 0);
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
                        //Debug.Log(limb.limbName);
                    }
               
                }

                else
                {
                    //Debug.Log(hit.distance);
                }
            }

            aimStateManager.AddRecoil();
        }

        bool CanFire()
        {
            fireRateTimer += Time.deltaTime;
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
                    if(semiAuto) playedEmptySound = false;
                }
                return false;


            }
            if (moveStateManager.currentState == moveStateManager.Run) return false;            
            if (actionStateManager.CurrentState != actionStateManager.Default) return false;
            if (aimStateManager.CurrentState == aimStateManager.AimingState) return true;
      
            return false;
        }

        private void OnFirePerformed(InputAction.CallbackContext context)
        {
            if (context.interaction is PressInteraction)
            {
                if (semiAuto)
                {
                    //Debug.Log("Semi-Auto: Quick Press Detected");
                    if (CanFire()) Fire();
                }
                else
                {
                    //Debug.Log("Full-Auto: Button Hold Detected");
                    if (CanFire()) Fire();
                    isFiring = true;
                }
            }
          
        }

        private void OnFireCanceled(InputAction.CallbackContext context)
        {
            // Stop firing when the button is released
            if (!semiAuto)
            {
                //Debug.Log("Full-Auto: Button Released");
                isFiring = false;
                playedEmptySound = false;
            }
        }

        private void ToogleFireRate()
        {
            if (Input.GetKeyDown(KeyCode.X) && actionStateManager.CurrentState == actionStateManager.Default)
            {
                semiAuto = !semiAuto;
                ToggleWeaponAudioSource.PlayOneShot(gameManager.WeaponAmmo.switchFireMode);
            }

            //fireRateText.text = semiAuto ? "SEMI" : "FULL AUTO";
        }

        void TriggerMuzzleFlash()
        {
            //muzzleFlashParticleSystem.Play();
            lightIntensity = Random.Range(minLightIntensity, maxLightIntensity);
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
