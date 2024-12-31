using Assets.Scripts.Game_Manager;
using Unity.VisualScripting;
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
    public SkinnedMeshRenderer [] limbMesh;

    [SerializeField] private Limbs[] NestedLimbs;

    [Header("Body replacement")]
    public GameObject limbReplacement;
    private GameObject limbClone;
    private Transform limbReplacementTransform;
    private Vector3 cachedLimbPosition;
    private Quaternion cachedLimbRotation;

    private GameManager gameManager;

    private ZombieStateManager ZombieStateManager;
    void Start()
    {
        gameManager = GameManager.Instance;

        ZombieStateManager = GetComponentInParent<ZombieStateManager>();

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
        else if(limbName == "torso")
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

    public void InstantiateReplacementTransform()
    {
        cachedLimbPosition = transform.position;
        cachedLimbRotation = transform.rotation;

        limbClone = Instantiate(limbReplacement, transform.position, transform.rotation);



        limbClone.transform.position = cachedLimbPosition;
        limbClone.transform.rotation = cachedLimbRotation;

        //limbReplacement.SetActive(true);
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
                    Debug.Log(Neslimb.limbName);
                    Neslimb.LimbTakeDamage(100);
                }
               
               
            }
        }


        if (limbReplacement != null)
        {
            
            if (ZombieStateManager.IsKilledByExplosion())
            {
                InstantiateReplacementTransform();
                limbClone.GetComponent<Rigidbody>().AddForce(ZombieStateManager.GetExplosionDirection() * 3000f, ForceMode.Impulse);
            }
              
        }
    }
}
