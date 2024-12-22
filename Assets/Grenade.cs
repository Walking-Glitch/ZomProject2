using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class Grenade : MonoBehaviour
{
    public float explosionRadius;
    public LayerMask ZombieLayerMask;

    [SerializeField] Collider[] colliders;

    private float elapsed;
    private float timeToexplode = 5f;

    [SerializeField] protected List<ZombieStateManager> enemies = new List<ZombieStateManager>();
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

    public void GrenadeTimer()
    {
        if (elapsed < timeToexplode)
        {
            elapsed += Time.deltaTime;
            Debug.Log(elapsed);
        }

        else {

            FindEnemies();

            gameObject.SetActive(false);
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
