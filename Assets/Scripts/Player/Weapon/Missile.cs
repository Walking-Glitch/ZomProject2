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

    // zombies in area of effect 
    [SerializeField] protected List<ZombieStateManager> enemies = new List<ZombieStateManager>();

    // missile stats
    [SerializeField] float missileSpeed;

    //game manager
    private GameManager gameManager;

    // flash
    private Light missileFlash;
    [SerializeField] private float flashIntensity;
    [SerializeField] private float lightReturnSpeed = 30;


    // antitank reference
    private TurretAntiTank turretAntiTank;

    // area of effect and detection variables
    public float explosionRadius;
    public LayerMask ZombieLayerMask;

    private void Awake()
    {
        gameManager = GameManager.Instance;
         
    }
    private void Start()
    {
        turretAntiTank = GetComponentInParent<TurretAntiTank>();
        originalMissileTransform = transform.position;
    }

    private void OnEnable()
    {
        exploded = false;
        MissileBody.SetActive(true); // Ensure the missile is visible again
        //transform.position = originalMissileTransform; // Reset position to its starting point
    }
    private void OnDisable()
    {
        exploded = false;  
    }
    // Update is called once per frame
    void Update()
    {
        {
            if(!exploded) MissileInterpolation();
        }
    }

   

     
    public void PlayReleasePinSfx()
    {
        //MissileAudioSource.PlayOneShot(MissilePropulsion);
    }

    public void PlayExplosionSfx()
    {
        //MissileAudioSource.PlayOneShot(MissileExplosion);

    }

    public void PlayExplosionVfx()
    {

        
    }

    public void MissileInterpolation()
    {
        Vector3 currentTarget = turretAntiTank.CurrentMissileTarget.transform.position + Vector3.up * 1.1f; 
        transform.position = Vector3.MoveTowards(transform.position, currentTarget, missileSpeed * Time.deltaTime);
        
        if (Vector3.Distance( transform.position, currentTarget) < 0.5f)
        {
            exploded = true;
            FindEnemies();
            KillEnemiesInBlastRadius();
            MissileBody.SetActive(false);
            transform.position = originalMissileTransform; 
            gameObject.SetActive(false);
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
        Debug.Log("Audio clip has finished playing.");
         
        gameObject.SetActive(false);
    }
 

    


    private void OnDrawGizmosSelected()
    {
        // Draw a wire sphere in the editor to visualize the detection dimensions
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, explosionRadius); //* Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z));
    }
}
