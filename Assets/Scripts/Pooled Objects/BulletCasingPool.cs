using System.Collections.Generic;
using UnityEngine;

public class BulletCasingPool : MonoBehaviour
{

    [SerializeField] private GameObject casingPrefab;
    [SerializeField] private int poolSize;
    [SerializeField] private List<GameObject> casingList;
    void Start()
    {
        AddCasingsToPool(poolSize);
    }

    private void AddCasingsToPool(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            GameObject casing = Instantiate(casingPrefab);
            casing.SetActive(false);
            casingList.Add(casing);
            casing.transform.parent = transform;
        }
    }

    public GameObject RequestCasing()
    {
        for (int i = 0; i < casingList.Count; i++)
        {
            if (!casingList[i].activeSelf)
            {
                casingList[i].gameObject.SetActive(true);
                casingList[i].GetComponent<Rigidbody>().isKinematic = true;
                return casingList[i];
            }
        }

        return null;
    }
}
