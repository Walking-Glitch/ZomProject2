using System.Collections.Generic;
using UnityEngine;

public class DecalPool : MonoBehaviour
{
    [SerializeField] private List<GameObject> decalPrefabs;
    [SerializeField] private int poolSize;
    [SerializeField] private List<GameObject> decalList;
    void Start()
    {
        AddDecalToPool(poolSize);
    }

    private void AddDecalToPool(int amount)
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject decalPrefab = decalPrefabs[0];
            GameObject decal = Instantiate(decalPrefab);
            decal.SetActive(false);
            decalList.Add(decal);
            decal.transform.parent = transform;
        }
    }

    public GameObject RequestDecal()
    {
        for (int i = 0; i < decalList.Count; i++)
        {
            if (!decalList[i].activeSelf)
            {
                decalList[i].gameObject.SetActive(true);
                return decalList[i];
            }
        }

        return null;
    }
}
