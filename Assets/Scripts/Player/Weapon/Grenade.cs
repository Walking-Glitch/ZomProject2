using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations.Rigging;

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
    void OnEnable()
    {
      elapsed = 0;
    }

    private void OnDisable()
    {
        
    }
    // Update is called once per frame
    void Update()
    {
     
       GrenadeTimer();
        
    }

    public void PlayReleasePinSfx()
    {
        GrenadeAudioSource.PlayOneShot(GrenadeReleasePin);
    }

    public void PlayExplosionSfx()
    {
        GrenadeAudioSource.PlayOneShot(GrenadeExplosion);
    }

    public void ReleaseGrenadeLever()
    {
      GrenadeLever.transform.SetParent(null);
        GrenadeLever.GetComponent<Rigidbody>().isKinematic = false;
        GrenadeLever.GetComponent<Rigidbody>().AddForce(GrenadeLever.transform.up.normalized * 3f, ForceMode.Impulse);
        //GrenadeLever.GetComponent<Rigidbody>().AddForce(GrenadeLever.transform.forward * 0.2f, ForceMode.Impulse);
        GrenadeLever.GetComponent<Rigidbody>().AddRelativeTorque(new Vector3(10, 0, 0), ForceMode.Impulse);
    }

    public void GrenadeTimer()
    {
        if (!exploded)
        {
            if (elapsed < timeToexplode)
            {
                elapsed += Time.deltaTime;
                Debug.Log(elapsed);
            }

            else
            {
                FindEnemies();
                PlayExplosionSfx();
                GrenadeBody.SetActive(false);
                exploded = true;
            }
        }
        
    }
    public void KillEnemiesInBlastRadius()
    {
        //ExplosionParticleSystem.Play();

           

        foreach (ZombieStateManager zombie in enemies)
        { 

            if (zombie != null && zombie.health > 0)
            {
                zombie.SetExplosionDirection(transform.position, zombie.transform.position);
                zombie.TakeDamage(100, "all", true, 500f);
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
        // Draw a wire sphere in the editor to visualize the detection radius
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, explosionRadius); //* Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z));
    }
}
