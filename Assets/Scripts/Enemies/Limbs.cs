using UnityEngine;

public class Limbs : MonoBehaviour
{
    public string limbName;
    public float damageMultiplier;

    public float limbDamageMultiplier;

    public int limbHealth;
    public int limbMaxHealth;

    private Collider limbCollider;
    public SkinnedMeshRenderer limbMesh;

    public bool isDestructible;
    void Start()
    {
        limbHealth = limbMaxHealth;
        limbCollider = GetComponent<Collider>();
    }

    public void LimbTakeDamage(int damage)
    {
        limbHealth -= damage;

        if (limbHealth <= 0 && isDestructible)
        {
            DestroyLimb();
        }
          
        
    }

    public void DestroyLimb()
    {
        if (limbCollider != null) limbCollider.enabled = false;
        if (limbMesh != null) limbMesh.enabled = false;
    }
}
