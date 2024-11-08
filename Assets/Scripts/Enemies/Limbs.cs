using UnityEngine;

public class Limbs : MonoBehaviour
{
    [Header("ID")]
    public string limbName;

    [Header(("Damage variables"))]
    public float damageMultiplier;

    public float limbDamageMultiplier;

    public int limbHealth;
    public int limbMaxHealth;

    private Collider limbCollider;

    [Header("To hide Skin")]
    public bool isDestructible;
    public SkinnedMeshRenderer limbMesh;

    [SerializeField] private Limbs[] NestedLimbs; 
    void Start()
    {
        limbMaxHealth = 100;
        limbHealth = limbMaxHealth;
        limbCollider = GetComponent<Collider>();

        GetNestedLimbs();
    }

    public void LimbTakeDamage(int damage)
    {
        limbHealth -= damage;
        limbHealth = Mathf.Clamp(limbHealth, 0, limbMaxHealth);
        if (limbHealth <= 0 && isDestructible)
        {
            DestroyLimb();
        }
        
    }

    private void GetNestedLimbs()
    {
        NestedLimbs = GetComponentsInChildren<Limbs>();
    }

    public void DestroyLimb()
    {
        if (limbCollider != null) limbCollider.enabled = false;
        if (limbMesh != null) limbMesh.enabled = false;

        if (NestedLimbs.Length > 0)
        {
            foreach (Limbs Neslimb in NestedLimbs)
            {
                if (Neslimb != NestedLimbs[0])
                {
                    Debug.Log(Neslimb.limbName);
                    Neslimb.LimbTakeDamage(100);
                }
               
               
            }
        }
    }
}
