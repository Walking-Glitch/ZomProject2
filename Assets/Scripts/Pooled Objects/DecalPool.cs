using System.Collections.Generic;
using UnityEngine;

public class DecalPool : MonoBehaviour
{
    [SerializeField] private List<GameObject> groundDecalPrefabs;
    [SerializeField] private List<GameObject> bloodDecalPrefabs;
    [SerializeField] private List<GameObject> metalDecalPrefabs;
    [SerializeField] private List<GameObject> woodDecalPrefabs;
    [SerializeField] private List<GameObject> concreteDecalPrefabs;
    [SerializeField] private int poolSize;
    [SerializeField] private List<GameObject> groundDecalList;
    [SerializeField] private List<GameObject> bloodDecalList;
    [SerializeField] private List<GameObject> metalDecalList;
    [SerializeField] private List<GameObject> woodDecalist;
    [SerializeField] private List<GameObject> concreteDecalist;

    public Transform GroundDecalPool;
    public Transform BloodDecalPool;
    public Transform MetalDecalPool;
    public Transform WoodDecalPool;
    public Transform ConcreteDecalPool;

    void Start()
    {
        AddGroundDecalToPool(poolSize);
        AddBloodDecalToPool(poolSize);
        AddMetalDecalToPool(poolSize);
        AddWoodDecalToPool(poolSize);
        AddConcreteDecalToPool(poolSize);
    }

    private void AddGroundDecalToPool(int amount)
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject decalPrefab = groundDecalPrefabs[0];
            GameObject decal = Instantiate(decalPrefab);
            decal.SetActive(false);
            groundDecalList.Add(decal);
            decal.transform.parent = GroundDecalPool;
        }
    }

    private void AddBloodDecalToPool(int amount)
    {
        for (int i = 0; i < poolSize; i++)
        {
            int index = Random.Range(0, bloodDecalPrefabs.Count) ;
            GameObject bloodDecalPrefab = bloodDecalPrefabs[index];
            GameObject bloodDecal = Instantiate(bloodDecalPrefab);
            bloodDecal.SetActive(false);
            bloodDecalList.Add(bloodDecal);
            bloodDecal.transform.parent = BloodDecalPool;
        }
    }

    private void AddMetalDecalToPool(int amount)
    {
        for (int i = 0; i < poolSize; i++)
        {
            int index = Random.Range(0, metalDecalPrefabs.Count);
            GameObject metalDecalPrefab = metalDecalPrefabs[index];
            GameObject metalDecal = Instantiate(metalDecalPrefab);
            metalDecal.SetActive(false);
            metalDecalList.Add(metalDecal);
            metalDecal.transform.parent = MetalDecalPool;
        }
    }

    private void AddWoodDecalToPool(int amount)
    {
        for (int i = 0; i < poolSize; i++)
        {
            int index = Random.Range(0, woodDecalPrefabs.Count);
            GameObject woodDecalPrefab = woodDecalPrefabs[index];
            GameObject woodDecal = Instantiate(woodDecalPrefab);
            woodDecal.SetActive(false);
            woodDecalist.Add(woodDecal);
            woodDecal.transform.parent = WoodDecalPool;
        }
    }

    private void AddConcreteDecalToPool(int amount)
    {
        for (int i = 0; i < poolSize; i++)
        {
            int index = Random.Range(0, concreteDecalPrefabs.Count);
            GameObject concreteDecalPrefab = concreteDecalPrefabs[index];
            GameObject concreteDecal = Instantiate(concreteDecalPrefab);
            concreteDecal.SetActive(false);
            concreteDecalist.Add(concreteDecal);
            concreteDecal.transform.parent = ConcreteDecalPool;
        }
    }

    public GameObject RequestGroundDecal()
    {
        for (int i = 0; i < groundDecalList.Count; i++)
        {
            if (!groundDecalList[i].activeSelf)
            {
                groundDecalList[i].gameObject.SetActive(true);
                return groundDecalList[i];
            }
        }

        return null;
    }

    public GameObject RequestBloodDecal()
    {
        for (int i = 0; i < bloodDecalList.Count; i++)
        {
            if (!bloodDecalList[i].activeSelf)
            {
                bloodDecalList[i].gameObject.SetActive(true);
                return bloodDecalList[i];
            }
        }
        return null;
    }
    public GameObject RequestMetalDecal()
    {
        for (int i = 0; i < metalDecalList.Count; i++)
        {
            if (!metalDecalList[i].activeSelf)
            {
                metalDecalList[i].gameObject.SetActive(true);
                return metalDecalList[i];
            }
        }
        return null;
    }

    public GameObject RequestWoodDecal()
    {
        for (int i = 0; i < woodDecalist.Count; i++)
        {
            if (!woodDecalist[i].activeSelf)
            {
                woodDecalist[i].gameObject.SetActive(true);
                return woodDecalist[i];
            }
        }
        return null;
    }

    public GameObject RequestConcreteDecal()
    {
        for (int i = 0; i < concreteDecalist.Count; i++)
        {
            if (!concreteDecalist[i].activeSelf)
            {
                concreteDecalist[i].gameObject.SetActive(true);
                return concreteDecalist[i];
            }
        }
        return null;
    }
}
