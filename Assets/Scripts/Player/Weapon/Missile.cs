using Assets.Scripts.Game_Manager;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;

public class Missile : MonoBehaviour
{
    // audio variables
    public AudioSource MissileAudioSource;
    public AudioClip MissilePropulsion;
    public AudioClip MissileExplosion;
     
    // game object references     
    public GameObject MissileBody;
    private Vector3 originalMissileTransform;

    //checks
    [SerializeField] private bool exploded;
    private Vector3 explosionPosition;

    // target tracking
    private ZombieStateManager target;
    private Vector3 targetPoint;
    private bool launched;

    // zombies in area of effect 
    [SerializeField] protected List<ZombieStateManager> enemies = new List<ZombieStateManager>();

    // missile stats
    [Header("Missile Stats")]
    [Tooltip("Flight speed in m/s. The AntiTank can't fire again until the missile lands, so slower = longer reload.")]
    [SerializeField] float missileSpeed;
    [Tooltip("Damage to every zombie inside the explosion radius.")]
    [SerializeField] int explosionDamage = 100;

    //game manager
    private GameManager gameManager;

    // flash
    private Light missileFlash;
    [SerializeField] private float flashIntensity;
    [SerializeField] private float lightReturnSpeed = 20;


    // antitank reference
    private TurretAntiTank turretAntiTank;

    // area of effect and detection variables
    [Tooltip("Explosion radius in m. Every zombie inside takes Explosion Damage and is blown apart if it dies.")]
    public float explosionRadius;
    public LayerMask ZombieLayerMask;

    //Explosion particle system
    public ParticleSystem ExplosionVFX;
    private void Awake()
    {
       gameManager = GameManager.Instance;
         
    }
    private void Start()
    {
        turretAntiTank = GetComponentInParent<TurretAntiTank>();
        originalMissileTransform = transform.position;

        missileFlash = GetComponentInChildren<Light>();
        flashIntensity = missileFlash.intensity;
        missileFlash.intensity = 0;
    }

    private void OnEnable()
    {
        exploded = false;
        MissileBody.SetActive(true); // Ensure the missile is visible again        
    }
    private void OnDisable()
    {
        exploded = false;
        launched = false;
        target = null;
    }

    public void Launch(ZombieStateManager zombie)
    {
        target = zombie;
        targetPoint = zombie.GetAimPoint();
        launched = true;
    }
    // Update is called once per frame
    void Update()
    {
        MissileInterpolation();

        missileFlash.intensity = Mathf.Lerp(missileFlash.intensity, 0, lightReturnSpeed * Time.deltaTime);

    }


    public void PlayExplosionSfx()
    {
        MissileAudioSource.PlayOneShot(MissileExplosion);

    }

    public void PlayExplosionVfx()
    {
        if (missileFlash != null)
        {
            ExplosionVFX.Play();
            missileFlash.intensity = flashIntensity;
        }
    }

    public void MissileInterpolation()
    {
        if (!launched) return;

        if (!exploded)
        {
            // follow the zombie while it lives; once it dies it goes back to the pool and
            // would respawn elsewhere, so keep flying to the spot where it fell
            if (target != null)
            {
                if (target.gameObject.activeInHierarchy && !target.isDead)
                    targetPoint = target.GetAimPoint();
                else
                    target = null;
            }

            transform.position = Vector3.MoveTowards(transform.position, targetPoint, missileSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, targetPoint) < 1f)
            {
                exploded = true;
                explosionPosition = transform.position; 
                FindEnemies();
                PlayExplosionVfx();
                PlayExplosionSfx();
                MissileBody.SetActive(false);
                StartCoroutine(WaitForAudioToEndAndDisable());
            }
        }
        else
        {
            // Keep the missile in place after explosion
            transform.position = explosionPosition;
        }
    }

    public void KillEnemiesInBlastRadius()
    { 

        foreach (ZombieStateManager zombie in enemies)
        {
            if (zombie != null && zombie.health > 0)
            {
                zombie.SetExplosionDirection(transform.position, zombie.transform.position);
                zombie.TakeDamage(explosionDamage, "all", true, false, 500f);
            }
        }

        turretAntiTank.MissileTraveling = false; 
    }

    protected virtual void FindEnemies()
    {
        enemies.Clear();

        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius, ZombieLayerMask);

        foreach (Collider col in colliders)
        {
            ZombieStateManager zombie = col.gameObject.GetComponentInParent<ZombieStateManager>();

            if (zombie != null)
            {
                if (col.gameObject.GetComponentInParent<ZombieStateManager>().health > 0 && !enemies.Contains(zombie))
                    enemies.Add(col.GetComponentInParent<ZombieStateManager>());
            }
            
        }

        KillEnemiesInBlastRadius();

    }

    IEnumerator WaitForAudioToEndAndDisable()
    {
        yield return new WaitWhile(() => MissileAudioSource.isPlaying);

        transform.position = originalMissileTransform;

        gameObject.SetActive(false);
    }
 

    


    private void OnDrawGizmosSelected()
    {
        // Draw a wire sphere in the editor to visualize the detection dimensions
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, explosionRadius); //* Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z));
    }
}
