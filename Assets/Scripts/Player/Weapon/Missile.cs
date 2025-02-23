using Assets.Scripts.Game_Manager;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Missile : NetworkBehaviour
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

    // zombies in area of effect 
    [SerializeField] protected List<ZombieStateManager> enemies = new List<ZombieStateManager>();

    // missile stats
    [SerializeField] float missileSpeed;

    //game manager
    private GameManager gameManager;

    // flash
    private Light missileFlash;
    [SerializeField] private float flashIntensity;
    [SerializeField] private float lightReturnSpeed = 20;


    // antitank reference
    private TurretAntiTank turretAntiTank;

    // area of effect and detection variables
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
        //MissileBody.SetActive(true); // Ensure the missile is visible again
       
        
        turretAntiTank.missileBodyActive.Value = true; 
    }
    private void OnDisable()
    {
        exploded = false;  
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
           // Debug.Log("WE HAVE LIGHT" + flashIntensity);
        }
        else
        {
            Debug.Log("NULL LIGHT");
        }
    }

    public void MissileInterpolation()
    {
        if (turretAntiTank.CurrentMissileTarget == null) return;

        Vector3 currentTarget = turretAntiTank.CurrentMissileTarget.transform.position + Vector3.up * 1.1f;

        if (!exploded)
        {
            //transform.position = Vector3.MoveTowards(transform.position, currentTarget, missileSpeed * Time.deltaTime);
            turretAntiTank.missilePosition.Value = Vector3.MoveTowards(transform.position, currentTarget, missileSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, currentTarget) < 1f)
            {
                exploded = true;
                explosionPosition = transform.position; 
                FindEnemies();
                //PlayExplosionVfx();
                turretAntiTank.PlayExplosionSfxClientRpc();
                PlayExplosionSfx();                
                turretAntiTank.missileBodyActive.Value = false;
                StartCoroutine(WaitForAudioToEndAndDisable());
            }
        }
        else
        {
            // Keep the missile in place after explosion
            turretAntiTank.missilePosition.Value = explosionPosition;
        }
    }

    public void KillEnemiesInBlastRadius()
    { 

        foreach (ZombieStateManager zombie in enemies)
        {
            Limbs[] limbs = zombie.GetComponentsInChildren<Limbs>();

            if (zombie != null && zombie.health > 0)
            {
                zombie.SetExplosionDirection(transform.position, zombie.transform.position);
                zombie.TakeDamage(100, "all", true, false, 500f);
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
        //Debug.Log("Audio clip has finished playing.");

        //transform.position = originalMissileTransform;
        turretAntiTank.missilePosition.Value = originalMissileTransform;

        turretAntiTank.missileActive.Value = false;
        //gameObject.SetActive(false);
        //Debug.Log("Wait for audio done");
    }
 

    


    private void OnDrawGizmosSelected()
    {
        // Draw a wire sphere in the editor to visualize the detection dimensions
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, explosionRadius); //* Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z));
    }
}
