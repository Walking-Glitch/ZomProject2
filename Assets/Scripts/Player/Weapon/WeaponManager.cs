using Assets.Scripts.Game_Manager;
using Assets.Scripts.Player.Actions;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;
using Random = UnityEngine.Random;

namespace Assets.Scripts.Player.Weapon
{
    public class WeaponManager : NetworkBehaviour
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

         
        // audio variables
        public AudioSource RifleAudioSource;
        public AudioSource ToggleWeaponAudioSource;
        public AudioClip [] gunShots;
        public AudioClip emptyClip;
        
        private bool playedEmptySound = false;


        // firing variables
        [Header("FireRpc Rate")]
        [SerializeField] float fireRate;
        private float fireRateTimer;

        

        public Transform ParentMuzzleVFX;
        [SerializeField] private List<Transform> muzzleFlashList = new List<Transform>();

        
        private float lightIntensity;
        [SerializeField] private float maxLightIntensity;
        [SerializeField] private float minLightIntensity;
         

        // animator
        private Animator anim;

        // zombie reference
        ZombieStateManager zombieStateManager;

        // toggle firing mode
        [SerializeField] private bool semiAuto;
        private bool isFiring = false;

        //casing spawn transform
        public Transform CasingSpawnPoint;
        private float MaxEjectForce = 3.5f;
        private float MinEjectForce = 1.8f;
        private float ejectTorque = 50;

        // game manager
        private GameManager gameManager;


        void Awake()
        {
            inputSystemActions = new InputSystem_Actions();
            inputSystemActions.Player.Attack.performed += OnFirePerformed;
            inputSystemActions.Player.Attack.canceled += OnFireCanceled;


           
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
            gameManager = GameManager.Instance; // moved in from awake
            CollectMuzzleFlashChildObjects(ParentMuzzleVFX);
        }

        public override void OnNetworkSpawn()
        {
            if (IsOwner)
            {
                StartCoroutine(WaitForPlayer());
            }
        }

        private IEnumerator WaitForPlayer()
        {
            while (gameManager == null || gameManager.PlayerGameObject == null)
            {
                yield return null; // Waits one frame
            }

            // Wait until CasingManager is assigned
            while (gameManager.CasingManager == null)
            {
                yield return null;
            }

            gameManager.CasingManager.CasingSpawnTransform = CasingSpawnPoint;
        }


        // Update is called once per frame
        void Update()
        {
            if (!IsOwner) return;

            ToogleFireRate();

            if (!semiAuto && isFiring && CanFire())
            {
                Fire();
            }

            fireRateTimer += Time.deltaTime;

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
            if(!IsServer && !IsClient)
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
                
            else if (IsOwner)
            {
                if (actionStateManager.CurrentState == actionStateManager.Grenade)
                {
                    RequestWeaponParentChangeServerRpc(true);
                }
                else if (actionStateManager.CurrentState == actionStateManager.Reload)
                {
                    RequestWeaponParentChangeServerRpc(false);
                }
                else if (actionStateManager.CurrentState == actionStateManager.Default)
                {
                    RequestWeaponParentChangeServerRpc(false);
                }
            }

         
        }

        [ServerRpc]
        private void RequestWeaponParentChangeServerRpc(bool parentToLeftHand, ServerRpcParams rpcParams = default)
        {
            if (parentToLeftHand)
            {
                rifle.transform.SetParent(LeftHandTransform);

                UpdateWeaponTransformClientRpc(true);
            }

            else
            {
                rifle.transform.SetParent(cachedWeaponParent);
                rifle.transform.localPosition = cachedWeaponPosition;
                rifle.transform.localRotation = cachedWeaponRotation;

                UpdateWeaponTransformClientRpc(false);
            }
        }

        [ClientRpc]
        private void UpdateWeaponTransformClientRpc(bool parentToLeftHand, ClientRpcParams rpcParams = default)
        {
            if (parentToLeftHand)
            {
                rifle.transform.SetParent(LeftHandTransform);
            }
            else
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
           


            fireRateTimer = 0;
            RifleAudioSource.PlayOneShot(gunShots[Random.Range(0, gunShots.Length)]);
            gameManager.WeaponAmmo.currentAmmo--;
            
            if(!IsServer && !IsClient)
            {
                TriggerMuzzleFlash();
                gameManager.CasingManager.SpawnBulletCasing();
            }

            if (IsOwner)  
            {
                TriggerMuzzleFlashServerRpc();

                

                // taken out of server rcp cause it was not updating properly 
                Vector3 spawnPos = CasingSpawnPoint.position;
                Quaternion spawnRot = CasingSpawnPoint.rotation;
 
                Vector3 forceRight = spawnRot * Vector3.left * Random.Range(MinEjectForce, MaxEjectForce);
                Vector3 forceForward = spawnRot * Vector3.forward * Random.Range(MinEjectForce, MaxEjectForce);
                Vector3 torque = new Vector3(10, 0, 0) * ejectTorque;

                // Call ServerRpc with correct forces               

                gameManager.CasingManager.SpawnBulletCasingServerRpc(spawnPos, spawnRot, forceRight, forceForward, torque);
            }

            Vector3 direction = TargetTransform.position - GunEndTransform.position;
            if (Physics.Raycast(GunEndTransform.position, direction.normalized, out RaycastHit hit, Mathf.Infinity,
                    shootMask))
            {
                if (hit.collider.CompareTag("Ground")|| hit.collider.CompareTag("Environment"))
                {
                    Quaternion decalRotation = Quaternion.LookRotation(hit.normal);

                    if (!IsServer && !IsClient) gameManager.DecalManager.SpawnGroundHitDecal(hit.point, decalRotation);


                    if (IsOwner) gameManager.DecalManager.SpawnGroundHitDecalServerRpc(hit.point, decalRotation);
                }

                else if (hit.collider.CompareTag("Metal"))
                {
                    Quaternion decalRotation = Quaternion.LookRotation(hit.normal);

                    if (!IsServer && !IsClient) gameManager.DecalManager.SpawnMetalHitDecal(hit.point, decalRotation);

                    if (IsOwner) gameManager.DecalManager.SpawnMetalHitDecalServerRpc(hit.point, decalRotation);
                }

                else if (hit.collider.CompareTag("Wood"))
                {
                    Quaternion decalRotation = Quaternion.LookRotation(hit.normal);

                    if (!IsServer && !IsClient) gameManager.DecalManager.SpawnWoodHitDecal(hit.point, decalRotation);

                    if (IsOwner) gameManager.DecalManager.SpawnWoodHitDecalServerRpc(hit.point, decalRotation);
                }

                else if (hit.collider.CompareTag("Concrete"))
                {
                    Quaternion decalRotation = Quaternion.LookRotation(hit.normal);

                    if (!IsServer && !IsClient) gameManager.DecalManager.SpawnConcreteHitDecal(hit.point, decalRotation);

                    if (IsOwner) gameManager.DecalManager.SpawnConcreteHitDecalServerRpc(hit.point, decalRotation);
                }

                else if (hit.collider.CompareTag("Zombie"))
                {
                    Quaternion decalRotation = Quaternion.LookRotation(hit.normal);

                    if (!IsServer && !IsClient) gameManager.DecalManager.SpawnBloodHitDecal(hit.point, decalRotation);

                    if (IsOwner) gameManager.DecalManager.SpawnBloodHitDecalServerRpc(hit.point, decalRotation);



                    ShotForceDir = hit.normal * -1;

                    Limbs limb = hit.collider.GetComponent<Limbs>();

                    if (limb != null)
                    {
                        float baseDamage = 40;
                        float finalDamage = baseDamage * limb.damageMultiplier;

                        if (limb.limbName == "head")
                        {
                            zombieStateManager = hit.collider.GetComponentInParent<ZombieStateManager>();
                            zombieStateManager.TakeDamageServerRpc((int)finalDamage, limb.limbName, false, false, 0);
                        }                       
                    
                    else if (limb.limbName == "torso" || limb.limbName == "belly")
                    {
                        zombieStateManager = hit.collider.GetComponentInParent<ZombieStateManager>();
                        zombieStateManager.TakeDamageServerRpc((int)finalDamage, limb.limbName, false, false, 0);
                    }

                    else
                    {
                        zombieStateManager = hit.collider.GetComponentInParent<ZombieStateManager>();
                        zombieStateManager.TakeDamageServerRpc((int)finalDamage, limb.limbName, false, false, 0);
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

        protected void CollectMuzzleFlashChildObjects(Transform parentMuzzleFlash)
        {
            foreach (Transform muzzle in parentMuzzleFlash)
            {
                muzzleFlashList.Add(muzzle.transform);
            }
        }
        void TriggerMuzzleFlash()
        {
            int index = Random.Range(0, muzzleFlashList.Count);
            muzzleFlashList[index].gameObject.SetActive(true);

            lightIntensity = Random.Range(minLightIntensity, maxLightIntensity);

            FPSLightCurves lightCurves = muzzleFlashList[index].GetComponentInChildren<FPSLightCurves>();

            if (lightCurves != null)
            {
                lightCurves.GraphIntensityMultiplier = lightIntensity;
            }
        }

        [ClientRpc]
        private void TriggerMuzzleFlashClientRpc()
        {
            int index = Random.Range(0, muzzleFlashList.Count);
            muzzleFlashList[index].gameObject.SetActive(true);

            lightIntensity = Random.Range(minLightIntensity, maxLightIntensity);

            FPSLightCurves lightCurves = muzzleFlashList[index].GetComponentInChildren<FPSLightCurves>();

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
