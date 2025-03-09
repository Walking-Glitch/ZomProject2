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
    private Vector3 originalMissilePosition;
    private Quaternion origianlMissileRotation;

    //checks
    [HideInInspector] public bool exploded;
    [HideInInspector] public Vector3 explosionPosition;

    // zombies in area of effect 
    [SerializeField] protected List<ZombieStateManager> enemies = new List<ZombieStateManager>();

    // missile stats
    [SerializeField] float missileSpeed;

    //game manager
    private GameManager gameManager;

    Transform immediateParent;
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
        originalMissilePosition = transform.position;
        origianlMissileRotation = transform.rotation;  

        missileFlash = GetComponentInChildren<Light>();
        flashIntensity = missileFlash.intensity;
        missileFlash.intensity = 0;

        immediateParent = gameObject.transform.parent;
    }

    private void OnEnable()
    {
        //exploded = false;     
        // EnableServerRpc();

        //if (IsServer)
        //{
        turretAntiTank.missileExploded.Value = false;
        turretAntiTank.missileBodyActive.Value = true;
        //}
    }

    //[ServerRpc]
    //private void EnableServerRpc()
    //{
    //    turretAntiTank.missileExploded.Value = false;
    //    turretAntiTank.missileBodyActive.Value = true;

    //}
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
        if(transform.parent != null)
        {
            transform.SetParent(null, true);
        }

        if (turretAntiTank.CurrentMissileTarget == null)
        {
            Debug.Log("MissileInterpolation Stopped! exploded: " + exploded);
            return;
        }

        Vector3 currentTarget = turretAntiTank.CurrentMissileTarget.transform.position + Vector3.up * 1.1f;

        if (!exploded)
        { 
            turretAntiTank.missilePosition.Value = Vector3.MoveTowards(transform.position, currentTarget, missileSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, currentTarget) < 1f)
            {
                Debug.Log("Missile hit target! exploded BEFORE: " + exploded);
               
                turretAntiTank.missileExploded.Value = true;

                Debug.Log("Missile hit target! exploded AFTER: " + exploded);

                turretAntiTank.explosionPosition.Value = turretAntiTank.missilePosition.Value; 
                FindEnemies();
              
                turretAntiTank.PlayExplosionVfxClientRpc();
                turretAntiTank.PlayExplosionSfxClientRpc();
                turretAntiTank.missileBodyActive.Value = false;
                StartCoroutine(WaitForAudioToEndAndDisable());
            }
        }
        else
        {
           // Debug.Log("INSIDE ELSE! Holding Position. exploded: " + exploded);
            // Keep the missile in place after explosion
            turretAntiTank.missilePosition.Value = turretAntiTank.explosionPosition.Value;
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
                zombie.TakeDamageServerRpc(100, "all", true, false, 500f);
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

         
        turretAntiTank.ReparentMissileClientRpc();

        turretAntiTank.missilePosition.Value = originalMissilePosition;

        turretAntiTank.missileActive.Value = false;
        
    }


   
    public void ReparentMissile()
    {
        Debug.Log("is this repeating?");
        transform.SetLocalPositionAndRotation(originalMissilePosition, Quaternion.LookRotation(immediateParent.forward));
        transform.SetParent(immediateParent);

    }


    private void OnDrawGizmosSelected()
    {
        // Draw a wire sphere in the editor to visualize the detection dimensions
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, explosionRadius); //* Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z));
    }
}
