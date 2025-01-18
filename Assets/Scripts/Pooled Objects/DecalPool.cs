using System.Collections.Generic;
using UnityEngine;

public class DecalPool : MonoBehaviour
{
    [SerializeField] private List<GameObject> groundDecalPrefabs;
    [SerializeField] private List<GameObject> bloodDecalPrefabs;
    [SerializeField] private int poolSize;
    [SerializeField] private List<GameObject> groundDecalList;
    [SerializeField] private List<GameObject> bloodDecalList;

    public Transform GroundDecalPool;
    public Transform BloodDecalPool;

    void Start()
    {
        AddGroundDecalToPool(poolSize);
        AddBloodDecalToPool(poolSize);
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
}
