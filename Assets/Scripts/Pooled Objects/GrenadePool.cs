using System.Collections.Generic;
using UnityEngine;

public class GrenadePool : MonoBehaviour
{
    [SerializeField] private List<GameObject> grenadePrefabs;
    [SerializeField] private int poolSize;
    [SerializeField] private List<GameObject> grenadeList;
    void Start()
    {
        AddGrenadesToPool(poolSize);
    }

    private void AddGrenadesToPool(int amount)
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject grenadePrefab = grenadePrefabs[0];
            GameObject grenade = Instantiate(grenadePrefab);
            grenade.SetActive(false);
            grenadeList.Add(grenade);
            grenade.transform.parent = transform;
        }
    }

    public GameObject RequestGrenade()
    {
        for (int i = 0; i < grenadeList.Count; i++)
        {
            if (!grenadeList[i].activeSelf)
            {
                return grenadeList[i];
            }
        }

        return null;
    }
}
