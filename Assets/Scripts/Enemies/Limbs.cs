using Assets.Scripts.Game_Manager;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.LightAnchor;

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
    public SkinnedMeshRenderer [] limbMesh;

    [SerializeField] private Limbs[] NestedLimbs;

    [Header("Body replacement")]
    public GameObject limbReplacement;
    private Transform limbReplacementParent;
    private Rigidbody limbReplacementRb;
    
    private GameManager gameManager;

    private ZombieStateManager ZombieStateManager;

    //explosion
    public Vector3 ExpDirection;
    private Vector3 grenadePos;
    void Start()
    {
        gameManager = GameManager.Instance;

        ZombieStateManager = GetComponentInParent<ZombieStateManager>();

        if(limbReplacement != null)
        {
            limbReplacementParent = limbReplacement.transform.parent;
            limbReplacementRb = limbReplacement.GetComponent<Rigidbody>();
        }
            
        limbMaxHealth = 100;
        limbHealth = limbMaxHealth;
        limbCollider = GetComponent<Collider>();

        GetNestedLimbs();
    }

    void OnEnable()
    {
        if (limbName == "head")
        {
            damageMultiplier = 10;

            limbDamageMultiplier = 1;

            isDestructible = true;

        }
        else if (limbName == "leg")
        {
            damageMultiplier = 0.8f;

            limbDamageMultiplier = 0.4f;

            isDestructible = true;
        }
        else if (limbName == "foot")
        {
            damageMultiplier = 0.2f;

            limbDamageMultiplier = 0f;

            isDestructible = true;
        }

        else if (limbName == "hand")
        {
            damageMultiplier = 0.2f;

            limbDamageMultiplier = 1f;

            isDestructible = true;
        }

        else if (limbName == "lowerArm")
        {
            damageMultiplier = 0.5f;

            limbDamageMultiplier = 0.5f;

            isDestructible = true;
        }
        else if (limbName == "torso")
        {
            damageMultiplier = 2f;

            limbDamageMultiplier = 0f;

            isDestructible = false;
        }

        else if (limbName == "belly")
        {
            damageMultiplier = 1.5f;

            limbDamageMultiplier = 0f;

            isDestructible = false;
        }
    }

    public void ActivateAndDetachReplacementLimb()
    {
     
        limbReplacement.SetActive(true);

        limbReplacement.transform.SetParent(null);
    }

    public void ReattachReplacementLimb()
    {
        if(limbReplacement != null)
        {
            limbReplacement.transform.SetParent(limbReplacementParent);

            limbReplacement.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.Euler(0, 0, 0));

            limbReplacement.SetActive(false);
        }
        
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
        if (limbMesh != null)
        {
            foreach (var limbMeshRenderer in limbMesh)
            {
                limbMeshRenderer.enabled = false;
            }
        }
        if (NestedLimbs.Length > 0)
        {
            foreach (Limbs Neslimb in NestedLimbs)
            {
                if (Neslimb != NestedLimbs[0])
                {
                    //Debug.Log(Neslimb.limbName);
                    Neslimb.LimbTakeDamage(100);
                }
               
               
            }
        }


        if (limbReplacement != null)
        {
            ActivateAndDetachReplacementLimb();

            if (ZombieStateManager.IsKilledByExplosion())
            {
                float randomForceMult = Random.Range(2000f, 5000f);
                float randomYtMult = Random.Range(0.05f, 0.3f);
                float randomXMult = Random.Range(-0.3f, 0.3f);
                limbReplacementRb.AddForce((ZombieStateManager.GetExplosionDirection() + new Vector3(randomXMult, randomYtMult, 0)) * randomForceMult, ForceMode.Impulse);
            }

            else
            {
                float randomForceMult = Random.Range(800f, 1000f);
                limbReplacementRb.AddForce(gameManager.WeaponManager.ShotForceDir * randomForceMult, ForceMode.Impulse);
            }
              
        }
    }
}
