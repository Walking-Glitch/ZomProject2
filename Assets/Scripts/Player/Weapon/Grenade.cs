using Assets.Scripts.Game_Manager;
using Assets.Scripts.Player.Weapon;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.Audio;

public class Grenade : MonoBehaviour
{
    // audio variables
    public AudioSource GrenadeAudioSource;
    public AudioClip GrenadeReleasePin;
    public AudioClip GrenadeExplosion;

    // area of effect and detection variables
    public float explosionRadius;
    public LayerMask ZombieLayerMask;

    // game object references
    public GameObject GrenadePin;
    public GameObject GrenadePullPin;
    public GameObject GrenadeLever;
    public GameObject GrenadeBody;

    // colliders found
    [SerializeField] Collider[] colliders;

    // timer variables
    private float elapsed;
    private float timeToexplode = 5f;

    // zombies in area of effect 
    [SerializeField] protected List<ZombieStateManager> enemies = new List<ZombieStateManager>();

    //checks
    private bool exploded;

    //Rigidbodies
    [HideInInspector] public Rigidbody GrenadeRb;
    [HideInInspector] public Rigidbody GrenadeLeverRb;

    //game manager
    private GameManager gameManager;

    // flash
    private Light grenadeFlash;
    [SerializeField] private float flashIntensity;
    [SerializeField] private float lightReturnSpeed = 30;

    //Explosion particle system
    public ParticleSystem ExplosionVFX;


    private void Awake()
    {
        gameManager = GameManager.Instance;
        GrenadeRb = GetComponent<Rigidbody>();
        GrenadeLeverRb = GrenadeLever.GetComponent<Rigidbody>();

        CodeToRunWhenObjectRequested();
    }
    private void Start()
    {
        grenadeFlash = GetComponentInChildren<Light>();
        flashIntensity = grenadeFlash.intensity;
        grenadeFlash.intensity = 0;
    }
    // Update is called once per frame
    void Update()
    {
        GrenadeTimer();

        if (grenadeFlash != null)
        {
            //Debug.Log("WE HAVE LIGHT" + flashIntensity);
        }
        else
        {
            //Debug.Log("NULL LIGHT");
        }

        grenadeFlash.intensity = Mathf.Lerp(grenadeFlash.intensity, 0, lightReturnSpeed * Time.deltaTime);
        
    }

    public void CodeToRunWhenObjectRequested()
    {
        elapsed = 0;
        exploded = false;

        GrenadeRb.isKinematic = true;
        GrenadeLeverRb.isKinematic = true;
    }

    public void CodeToRunWhenObjectDisabled()
    {
        GrenadeRb.isKinematic = true;
        GrenadeLeverRb.isKinematic = true;
        GrenadeBody.SetActive(true);
        
    }
    public void PlayReleasePinSfx()
    {
        GrenadeAudioSource.PlayOneShot(GrenadeReleasePin);
    }

    public void PlayExplosionSfx()
    {
        GrenadeAudioSource.PlayOneShot(GrenadeExplosion);
       
    }

    public void PlayExplosionVfx()
    {
        GrenadeRb.isKinematic = true;

        if (grenadeFlash != null)
        {
            ExplosionVFX.Play();
            grenadeFlash.intensity = flashIntensity;
            Debug.Log("WE HAVE LIGHT" + flashIntensity);
        }
        else
        {
            Debug.Log("NULL LIGHT");
        }
    }

    IEnumerator WaitForAudioToEndAndDisable()
    {
        yield return new WaitWhile(() => GrenadeAudioSource.isPlaying);
        Debug.Log("Audio clip has finished playing.");
      
        GrenadeLever.transform.SetParent(GrenadeBody.transform);
        GrenadeLever.transform.SetLocalPositionAndRotation(new Vector3(0, 0, 0), Quaternion.Euler(0, 0, 0));

        CodeToRunWhenObjectDisabled();

        gameObject.SetActive(false);
    }

    public void ReleaseGrenadeLever()
    {
      GrenadeLever.transform.SetParent(null);
        GrenadeLever.GetComponent<Rigidbody>().isKinematic = false;
        GrenadeLever.GetComponent<Rigidbody>().AddForce(GrenadeLever.transform.up.normalized * 3f, ForceMode.Impulse);
        GrenadeLever.GetComponent<Rigidbody>().AddRelativeTorque(new Vector3(10, 0, 0), ForceMode.Impulse);
    }

    public void GrenadeTimer()
    {
        if (!exploded)
        {
            if (elapsed < timeToexplode)
            {
                elapsed += Time.deltaTime;
                //Debug.Log(elapsed);
            }

            else
            {
                FindEnemies();
                PlayExplosionVfx();
                PlayExplosionSfx();                
                GrenadeBody.SetActive(false);
                StartCoroutine(WaitForAudioToEndAndDisable());

                exploded = true;

                
            }
        }
        
    }
    public void KillEnemiesInBlastRadius()
    {
        //ExplosionParticleSystem.Play();

        foreach (ZombieStateManager zombie in enemies)
        {
            Limbs[] limbs = zombie.GetComponentsInChildren<Limbs>(); 

            if (zombie != null && zombie.health > 0)
            {
                zombie.SetExplosionDirection(transform.position, zombie.transform.position);
                zombie.TakeDamage(100, "all", true, false, 500f);

            }
        }      
    }

    protected virtual void FindEnemies()
    {
        enemies.Clear();

        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius, ZombieLayerMask);

        foreach (Collider col in colliders)
        {
            ZombieStateManager zombie = col.gameObject.GetComponentInParent<ZombieStateManager>();

            if (col.gameObject.GetComponentInParent<ZombieStateManager>().health > 0 && !enemies.Contains(zombie))
                enemies.Add(col.GetComponentInParent<ZombieStateManager>());
        }

        KillEnemiesInBlastRadius();

    }

    

    private void OnDrawGizmosSelected()
    {
        // Draw a wire sphere in the editor to visualize the detection dimensions
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, explosionRadius); //* Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z));
    }
}
