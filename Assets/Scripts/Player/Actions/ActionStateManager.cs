using System.Collections;
using Assets.Scripts.Game_Manager;
using Assets.Scripts.Player.Weapon;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.InputSystem;

namespace Assets.Scripts.Player.Actions
{
    public class ActionStateManager : NetworkBehaviour
    {
        // states
        [HideInInspector] public ActionStateBase CurrentState;
        [HideInInspector] public ReloadState Reload = new ReloadState();
        [HideInInspector] public GrenadeState Grenade = new GrenadeState();
        [HideInInspector] public DefaultState Default = new DefaultState();
        [HideInInspector] public BuildState Build = new BuildState();

        // animator
        [HideInInspector] public Animator anim;
        public bool isProgressChecked;
        public float blendDuration;

        // constraint variables
        [HideInInspector] private TwoBoneIKConstraint LeftHandIKConstraint;
        public MultiAimConstraint RightHandAimConstraint;

        private InputSystem_Actions inputSystemActions;

        // audio 
        public AudioSource audioSource;

        // reference to aim state manager
        [HideInInspector] public AimStateManager AimStateManager;

        // reference to weapon manager
        [HideInInspector] public WeaponManager WeaponManager;

        //Grenades
        public GameObject GrenadePrefab;
        private GameObject grenadeClone;

        //game manager
        public GameManager gameManager;

        // player spawned check 
        private bool isInitialized;

        // network variables
        private NetworkVariable<float> layer1Weight = new NetworkVariable<float>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        private NetworkVariable<float> rightHandAimWeight = new NetworkVariable<float>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        private NetworkVariable<float> leftHandHintWeight = new NetworkVariable<float>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        private NetworkVariable<float> leftHandHintWeightData = new NetworkVariable<float>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        public override void OnNetworkSpawn()
        {
            Initialize();

            
        }

        void Start()
        {
            if (!IsServer && !IsClient)
            {
                Initialize();
            }
        }
        // Update is called once per frame
        void Update()
        {
            if (!IsOwner || !isInitialized) return;
            //Debug.Log(CurrentState);
            CurrentState.UpdateState(this);
        }

        public void SwitchState(ActionStateBase state)
        {
            CurrentState = state;
            state.EnterState(this);
        }

        private void Initialize()
        {
            gameManager = GameManager.Instance;
            StartCoroutine(WaitForPlayer());           
        }

        private IEnumerator WaitForPlayer()
        {
            while (gameManager.PlayerGameObject == null)
            {
                yield return null;
            }

            inputSystemActions = new InputSystem_Actions();

            inputSystemActions.Player.Reload.performed += OnReloadPerformed;
            inputSystemActions.Player.Grenade.performed += OnThrowGrenadePerformed;
            inputSystemActions.Player.Inventory.performed += OnInventoryPerformed;
            inputSystemActions.Player.Attack.performed += OnFirePerformed;
            inputSystemActions.Player.Scroll.performed += OnScrollPerformed;
            inputSystemActions.Player.Interact.started += OnInteractPerformed;

            inputSystemActions.Enable();

            anim = GetComponent<Animator>();
            AimStateManager = GetComponent<AimStateManager>();
            WeaponManager = GetComponent<WeaponManager>();
            LeftHandIKConstraint = GetComponentInChildren<TwoBoneIKConstraint>();

            

            if (IsOwner)
            {
                UpdateLayerWieghtServerRpc(anim.GetLayerWeight(1));
                UpdateLeftHintWeightServerRpc(LeftHandIKConstraint.weight);
                UpdateRightHandAimWeightServerRpc(RightHandAimConstraint.weight);
                UpdateLeftHintWeightDataServerRpc(LeftHandIKConstraint.data.hintWeight);
            }

            // Now apply the network variables to update the animation weights
            anim.SetLayerWeight(1, layer1Weight.Value);
            LeftHandIKConstraint.data.hintWeight = leftHandHintWeight.Value;
            RightHandAimConstraint.weight = rightHandAimWeight.Value;

            layer1Weight.OnValueChanged += (prev, curr) =>
            {
                //Debug.Log($"Layer1Weight changed from {prev} to {curr}");
                anim.SetLayerWeight(1, curr);
            };
            leftHandHintWeight.OnValueChanged += (prev, curr) =>
            {
                //Debug.Log($"LeftHandHintWeight changed from {prev} to {curr}");
                LeftHandIKConstraint.weight = curr;
            };
            rightHandAimWeight.OnValueChanged += (prev, curr) =>
            {
                //Debug.Log($"RightHandAimWeight changed from {prev} to {curr}");
                RightHandAimConstraint.weight = curr;
            };

            leftHandHintWeightData.OnValueChanged += (prev, curr) =>
            {
                //Debug.Log($"HintWeight changed from {prev} to {curr}");
                LeftHandIKConstraint.data.hintWeight = curr;
            };

            SwitchState(Default);

            isInitialized = true;
        }
        public void TransitionToReload()
        {
            if (!AimStateManager.isTransitioning)
            {
                StartCoroutine(FadeIntoReload());
            }
        }

        [ServerRpc]
        private void UpdateLayerWieghtServerRpc(float layerWeight)
        {
            layer1Weight.Value = layerWeight;
        }

        [ServerRpc]
        private void UpdateLeftHintWeightServerRpc(float leftHandHintWeightValue)
        {
            leftHandHintWeight.Value = leftHandHintWeightValue;
        }

        [ServerRpc]
        private void UpdateLeftHintWeightDataServerRpc(float leftHandHintWeightDataValue)
        {
            leftHandHintWeightData.Value = leftHandHintWeightDataValue;
        }

        [ServerRpc]
        private void UpdateRightHandAimWeightServerRpc(float rightHandWeight)
        {
            rightHandAimWeight.Value = rightHandWeight;
        }

        private IEnumerator FadeIntoReload()
        {
            if (!IsServer && !IsClient)
            {

                AimStateManager.isTransitioning = true;
                float startWeight = anim.GetLayerWeight(1);
                float leftHandStartIkWeight = LeftHandIKConstraint.weight;
                float leftHandStartHintWeight = LeftHandIKConstraint.data.hintWeight;
                float rightHandStartWeight = RightHandAimConstraint.weight;
                float elapsed = 0;

                while (elapsed < blendDuration)
                {
                    elapsed += Time.deltaTime;
                    float newWeight = Mathf.Lerp(startWeight, 1, elapsed / blendDuration);
                    anim.SetLayerWeight(1, newWeight);

                    if (!isProgressChecked)
                    {
                        float newWeightTwoBoneIk = Mathf.Lerp(leftHandStartIkWeight, 0f, elapsed / blendDuration);
                        LeftHandIKConstraint.weight = newWeightTwoBoneIk;


                        float newWeighRightHand = Mathf.Lerp(rightHandStartWeight, 0, elapsed / blendDuration);
                        RightHandAimConstraint.weight = newWeighRightHand;
                    }

                    float newHintWeighT = Mathf.Lerp(leftHandStartHintWeight, 0, elapsed / blendDuration);
                    LeftHandIKConstraint.data.hintWeight = newHintWeighT;


                    yield return null;
                }

                anim.SetLayerWeight(1, 1);
                LeftHandIKConstraint.weight = isProgressChecked ? LeftHandIKConstraint.weight : 0f;  // Retain weight if progress checked
                LeftHandIKConstraint.data.hintWeight = 0;
                RightHandAimConstraint.weight = isProgressChecked ? RightHandAimConstraint.weight : 0f;

                if (!isProgressChecked)
                {
                    StartCoroutine(CheckAnimationProgress());
                }


                AimStateManager.isTransitioning = false;

            }

            else if (IsOwner)
            {
                Debug.Log("we are transitioning from shoot to reload");
                AimStateManager.isTransitioning = true;
                float startWeight = layer1Weight.Value;
                float leftHandStartIkWeight = leftHandHintWeight.Value;
                float leftHandStartHintWeight = leftHandHintWeightData.Value;
                float rightHandStartWeight = rightHandAimWeight.Value;
                float elapsed = 0;

                while (elapsed < blendDuration)
                {
                    elapsed += Time.deltaTime;
                    float newWeight = Mathf.Lerp(startWeight, 1, elapsed / blendDuration);
                    UpdateLayerWieghtServerRpc(newWeight);                  
                     

                    if (!isProgressChecked)
                    {
                        float newWeightTwoBoneIk = Mathf.Lerp(leftHandStartIkWeight, 0f, elapsed / blendDuration);                       
                        UpdateLeftHintWeightServerRpc(newWeightTwoBoneIk);
                      

                        float newWeighRightHand = Mathf.Lerp(rightHandStartWeight, 0, elapsed / blendDuration);                        
                        UpdateRightHandAimWeightServerRpc(newWeighRightHand);                    

                    }

                    float newHintWeighT = Mathf.Lerp(leftHandStartHintWeight, 0, elapsed / blendDuration);
                    UpdateLeftHintWeightDataServerRpc(newHintWeighT);

                    yield return null;
                }

                UpdateLayerWieghtServerRpc(1);
               
                if (!isProgressChecked)
                {
                    UpdateLeftHintWeightServerRpc(0);
                    UpdateLeftHintWeightDataServerRpc(0);
                    UpdateRightHandAimWeightServerRpc(0);
                    StartCoroutine(CheckAnimationProgress());
                }
               
                AimStateManager.isTransitioning = false;
            }
        }

        private IEnumerator CheckAnimationProgress()
        {
            if (!IsServer && !IsClient)
            {
                while (anim.GetCurrentAnimatorStateInfo(1).IsName("Reloading") || anim.GetCurrentAnimatorStateInfo(1).IsName("Toss Grenade"))
                {
                    float progress = anim.GetCurrentAnimatorStateInfo(1).normalizedTime % 1;

                    if (progress > 0.7f && !isProgressChecked)
                    {
                        float elapsed = 0;
                        isProgressChecked = true;

                        while (elapsed < blendDuration)
                        {
                            elapsed += Time.deltaTime;
                            float newWeightTwoBoneIk = Mathf.Lerp(0, 1f, elapsed / blendDuration);
                            LeftHandIKConstraint.weight = newWeightTwoBoneIk;


                            float newWeighRightHand = Mathf.Lerp(0, 1f, elapsed / blendDuration);
                            RightHandAimConstraint.weight = newWeighRightHand;

                            yield return null;
                        }

                        Debug.Log("Progress reached 0.6f - LeftHandIKConstraint.weight set to 1");
                        yield break;
                    }

                    yield return null;

                }

                isProgressChecked = false;
                yield return null;
            }

            else if (IsOwner)
            {
                Debug.Log("Inside new if");
                while (anim.GetCurrentAnimatorStateInfo(1).IsName("Reloading") || anim.GetCurrentAnimatorStateInfo(1).IsName("Toss Grenade"))
                {
                    float progress = anim.GetCurrentAnimatorStateInfo(1).normalizedTime % 1;

                    if (progress > 0.7f && !isProgressChecked)
                    {
                        float elapsed = 0;
                        isProgressChecked = true;

                        while (elapsed < blendDuration)
                        {
                            elapsed += Time.deltaTime;
                            float newWeightTwoBoneIk = Mathf.Lerp(0, 1f, elapsed / blendDuration);                           
                            UpdateLeftHintWeightServerRpc(newWeightTwoBoneIk);

                            Debug.Log(leftHandHintWeight.Value);

                            float newWeighRightHand = Mathf.Lerp(0, 1f, elapsed / blendDuration);                           
                            UpdateRightHandAimWeightServerRpc(newWeighRightHand);

                            yield return null;
                        }

                        Debug.Log("Progress reached 0.6f - LeftHandIKConstraint.weight set to 1");
                        yield break;
                    }

                    yield return null;

                } 
                isProgressChecked = false;
                yield return null;
            }
        } 

        public void GrenadeInstantiate()
        {
            grenadeClone = gameManager.GrenadePool.RequestGrenade();
            grenadeClone.transform.SetParent(WeaponManager.GrenadeSpawnTransform);
            grenadeClone.transform.SetLocalPositionAndRotation(new Vector3(0, 0, 0), Quaternion.Euler(36f, -20f, -68f));
            grenadeClone.GetComponent<Grenade>().CodeToRunWhenObjectRequested();


            grenadeClone.SetActive(true);

        }
        public void GrenadeToss()
        {
            Vector3 direction = (WeaponManager.TargetTransform.position - WeaponManager.RightHandTransform.position).normalized;
            grenadeClone.transform.SetParent(null);
            grenadeClone.GetComponent<Rigidbody>().isKinematic = false;
            grenadeClone.GetComponent<Rigidbody>().AddForce(direction * 100f, ForceMode.Impulse);
            grenadeClone.GetComponent<Rigidbody>().AddForce(new Vector3(0, 0.5f, 0) * 50f, ForceMode.Impulse);
        }

        public void PlayGrenadeReleasePin()
        {
            grenadeClone.GetComponent<Grenade>().PlayReleasePinSfx();
        }

        public void LeverRelease()
        {
            grenadeClone.GetComponent<Grenade>().ReleaseGrenadeLever();
        }

        public void GrenadeTossCompleted()
        {
            SwitchState(Default);
        }


        private void OnReloadPerformed(InputAction.CallbackContext context)
        {
            if (AimStateManager.CurrentState == AimStateManager.AimingState && CurrentState == Default) SwitchState(Reload);
        }

        private void OnThrowGrenadePerformed(InputAction.CallbackContext context)
        {
            if (AimStateManager.CurrentState == AimStateManager.AimingState && CurrentState == Default) SwitchState(Grenade);
        }

        private void OnInventoryPerformed(InputAction.CallbackContext context)
        {
            if (AimStateManager.CurrentState == AimStateManager.AimingState && CurrentState == Default) SwitchState(Build);
            else if (AimStateManager.CurrentState == AimStateManager.AimingState && CurrentState == Build)
                CurrentState?.OnInventory(this);
        }

        private void OnScrollPerformed(InputAction.CallbackContext context)
        {
            float scrollDelta = context.ReadValue<Vector2>().y;

            CurrentState?.OnScroll(this, scrollDelta);
        }

        private void OnFirePerformed(InputAction.CallbackContext context)
        {
            CurrentState?.OnFire(this);
        }

        private void OnInteractPerformed(InputAction.CallbackContext context)
        {
            //Debug.Log("E pressed");
            if (gameManager.PlayerStats.isInInteractableRange)
            {
                //gameManager.PlayerGameObject.SetActive(false);
                anim.SetBool("IsDriving", true);
                gameManager.Truck.isPlayerIn = true;
            }
        }

        public void WeaponReloaded()
        {
            gameManager.WeaponAmmo.Reload();
            SwitchState(Default);
        }

        public void MagOut()
        {
            WeaponManager.RemoveMag();
            audioSource.PlayOneShot(gameManager.WeaponAmmo.magOutClip);
        }
        public void MagIn()
        {
            WeaponManager.AttachMag();
            audioSource.PlayOneShot(gameManager.WeaponAmmo.magInClip);
        }
        public void ReleaseSlideSound()
        {
            audioSource.PlayOneShot(gameManager.WeaponAmmo.releaseSlideClip);
        }
        private void OnEnable()
        {
            if (inputSystemActions != null)
                inputSystemActions.Enable();
        }
        private void OnDisable()
        {
            if (inputSystemActions != null)
                inputSystemActions.Disable();
        }
    }
}
