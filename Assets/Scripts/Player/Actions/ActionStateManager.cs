using System.Collections;
using Assets.Scripts.Game_Manager;
using Assets.Scripts.Player.Weapon;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.InputSystem;

namespace Assets.Scripts.Player.Actions
{
    public class ActionStateManager : MonoBehaviour
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



        void Awake()
        {
            inputSystemActions = new InputSystem_Actions();
            inputSystemActions.Player.Reload.performed += OnReloadPerformed;
            inputSystemActions.Player.Grenade.performed += OnThrowGrenadePerformed;
            inputSystemActions.Player.Inventory.performed += OnInventoryPerformed;
            inputSystemActions.Player.Attack.performed += OnFirePerformed;
            inputSystemActions.Player.Scroll.performed += OnScrollPerformed;

            inputSystemActions.Player.Interact.started += OnInteractPerformed;


        }
    
        void Start()
        {
            gameManager = GameManager.Instance;
            anim = GetComponent<Animator>();
            AimStateManager = GetComponent<AimStateManager>();
            WeaponManager = GetComponent<WeaponManager>();
            LeftHandIKConstraint = GetComponentInChildren<TwoBoneIKConstraint>();
            SwitchState(Default);
        }

        // Update is called once per frame
        void Update()
        {
            //Debug.Log(CurrentState);
            CurrentState.UpdateState(this);
        }

        public void SwitchState(ActionStateBase state)
        {
            CurrentState = state;
            state.EnterState(this);
        }

        public void TransitionToReload()
        {
            if (!AimStateManager.isTransitioning)
            {
                StartCoroutine(FadeIntoReload());
            }
        }

        private IEnumerator FadeIntoReload()
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

            anim.SetLayerWeight(1,1);
            LeftHandIKConstraint.weight = isProgressChecked ? LeftHandIKConstraint.weight : 0f;  // Retain weight if progress checked
            LeftHandIKConstraint.data.hintWeight = 0;
            RightHandAimConstraint.weight = isProgressChecked ? RightHandAimConstraint.weight : 0f;

            if (!isProgressChecked)
            {
                StartCoroutine(CheckAnimationProgress());
            }
      

            AimStateManager.isTransitioning = false;

        }

        private IEnumerator CheckAnimationProgress()
        {
            while (anim.GetCurrentAnimatorStateInfo(1).IsName("Reloading"))
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
            inputSystemActions.Enable();
        }
        private void OnDisable()
        {
            inputSystemActions.Disable();
        }
    }
}
