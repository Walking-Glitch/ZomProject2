using System.Collections;
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



        void Awake()
        {
            inputSystemActions = new InputSystem_Actions();
            inputSystemActions.Player.Reload.performed += OnReloadPerformed;
            inputSystemActions.Player.Grenade.performed += OnThrowGrenadePerformed;


        }
    
        void Start()
        {
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
            grenadeClone = Instantiate(GrenadePrefab, WeaponManager.GrenadeSpawnTransform.position, Quaternion.identity);
            grenadeClone.transform.SetParent(WeaponManager.GrenadeSpawnTransform);
            grenadeClone.transform.SetLocalPositionAndRotation(new Vector3(0, 0, 0), Quaternion.Euler(60f, -246.455f, 53.591f));
        }
        public void GrenadeToss()
        {
           Vector3 direction = (WeaponManager.TargetTransform.position - WeaponManager.RightHandTransform.position).normalized;
            grenadeClone.transform.SetParent(null);
            grenadeClone.GetComponent<Rigidbody>().isKinematic = false;
            grenadeClone.GetComponent<Rigidbody>().AddForce(direction * 100f, ForceMode.Impulse);
        }

        public void GrenadeTossCompleted()
        {
            SwitchState(Default);
        }

      
        private void OnReloadPerformed(InputAction.CallbackContext context)
        {
            if (AimStateManager.CurrentState == AimStateManager.AimingState) SwitchState(Reload);
        }

        private void OnThrowGrenadePerformed(InputAction.CallbackContext context)
        {
            if (AimStateManager.CurrentState == AimStateManager.AimingState && CurrentState == Default) SwitchState(Grenade); 
        }

        public void WeaponReloaded()
        {
            // call reload function *to do
            SwitchState(Default);
       
        }

        public void MagOut()
        {
            // play oneshot
        }

        public void MagIn()
        {
            // play oneshot
        }

        public void ReleaseSlideSound()
        {
            // play oneshot
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
