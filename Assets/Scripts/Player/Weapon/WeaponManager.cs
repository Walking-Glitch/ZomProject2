using Unity.Burst.Intrinsics;
using UnityEngine;
using static UnityEngine.InputSystem.LowLevel.InputStateHistory;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

public class WeaponManager : MonoBehaviour
{
    // ray casting variables 
    [SerializeField] private LayerMask shootMask;
    public Transform GunEndTransform;
    public Transform TargetTransform;
    public Transform weaponTransform;

    //reference to laser 
    [HideInInspector] public WeaponLaser laser;

    // input system
    private InputSystem_Actions inputSystemActions;

    // reference to state managers
    private AimStateManager aimStateManager;
    private MovementStateManager moveStateManager;
    private ActionStateManager actionStateManager;

    // decals 
    public GameObject hitGroundDecal;

    // audio variables
    public AudioSource audioSource;
    public AudioClip [] gunShots;

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

    void Awake()
    {
        inputSystemActions = new InputSystem_Actions();
        inputSystemActions.Player.Attack.performed += OnFirePerformed;
    }

    void Start()
    {
        anim = GetComponent<Animator>();
        aimStateManager = GetComponent<AimStateManager>();
        moveStateManager = GetComponent<MovementStateManager>();
        actionStateManager = GetComponent<ActionStateManager>();
        laser = GetComponent<WeaponLaser>();
    }

    // Update is called once per frame
    void Update()
    {
        fireRateTimer += Time.deltaTime;

    }




    void Fire()
    {
        anim.SetTrigger("Firing");
        fireRateTimer = 0;
        audioSource.PlayOneShot(gunShots[Random.Range(0, gunShots.Length)]);
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

                //hit.collider.gameObject.SetActive(false);
                hit.collider.GetComponentInParent<Rigidbody>().AddForce(hit.normal*-1, ForceMode.Impulse);
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
        //if (ammo.currentAmmo == 0)
        //{
        //    if (Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKey(KeyCode.Mouse0))
        //    {
        //        if (!playedEmptySound)
        //        {
        //            EmptyAudioSource.PlayOneShot(emptyClip);
        //            playedEmptySound = true;
        //        }
        //    }
        //    else
        //    {
        //        playedEmptySound = false;
        //    }
        //    return false;
        //}
        if (moveStateManager.currentState == moveStateManager.Run) return false;
        if (actionStateManager.CurrentState == actionStateManager.Reload) return false;
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

    //void Fire()
    //{
    //    fireRateTimer = 0;
    //    barrelPos.LookAt(aim.aimPos);
    //    //barrelPos.localEulerAngles = bloom.BloomAngle(barrelPos);
    //    cameraAdjustment.transform.localEulerAngles = bloom.BloomAngle(cameraAdjustment.transform);

    //    audioSource.PlayOneShot(gunShot);

    //    recoil.TriggerRecoil();
    //    TriggerMuzzleFlash();
    //    ammo.currentAmmo--;
    //    for (int i = 0; i < bulletsPershot; i++)
    //    {

    //        Vector2 screenCentre = new Vector2(Screen.width / 2, Screen.height / 2);
    //        Ray ray = Camera.main.ScreenPointToRay(screenCentre);

    //        Vector3 shootingDirection = (ray.direction).normalized;

    //        // Set the ray origin to the camera adjustment's position
    //        RaycastHit hit;
    //        // Does the ray intersect any objects excluding the player layer
    //        if (Physics.Raycast(cameraAdjustment.transform.position, shootingDirection, out hit, Mathf.Infinity, aimMask))
    //        {
    //            shootingDirection = (hit.point - barrelPos.position).normalized;
    //            Debug.DrawRay(cameraAdjustment.transform.position, shootingDirection * hit.distance, Color.yellow);
    //            //Debug.Log("Did Hit");
    //        }

    //        GameObject currentBullet = gameManager.BulletPool.RequestBullet();
    //        currentBullet.transform.position = barrelPos.position;
    //        currentBullet.transform.rotation = Quaternion.LookRotation(shootingDirection);
    //        currentBullet.SetActive(true);


    //        Rigidbody rb = currentBullet.GetComponent<Rigidbody>();
    //        rb.velocity = Vector3.zero;
    //        rb.angularVelocity = Vector3.zero;
    //        rb.AddForce(shootingDirection * bulletVelocity, ForceMode.Impulse);
    //    }

    //    RefreshDisplay(ammo.currentAmmo, ammo.extraAmmo);

    //}
}
