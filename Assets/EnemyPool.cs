using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyPool : MonoBehaviour
{
    [SerializeField] private List<GameObject> enemyPrefabs;

    [SerializeField] private int poolSize;
    [SerializeField] private List<GameObject> enemyList;

    void Start()
    {
        AddEnemiesToPool(poolSize);
    }

    private void AddEnemiesToPool(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            //if ((i + 1) % 7 == 0)
            //{
            //    GameObject enemyPrefab = enemyPrefabs[2];
            //    GameObject enemy = Instantiate(enemyPrefab);
            //    enemy.SetActive(false);
            //    enemyList.Add(enemy);
            //    enemy.transform.parent = transform;
            //}

            //else
            //{
                GameObject enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Count - 1)];
                GameObject enemy = Instantiate(enemyPrefab);
                enemy.SetActive(false);
                enemyList.Add(enemy);
                enemy.transform.parent = transform;
           // }

        }
    }

    public GameObject RequestEnemy()
    {
        //for (int i = Random.Range(0, EnemyList.Count - 1); i < EnemyList.Count; i++)
        for (int i = 0; i < enemyList.Count; i++)
        {
            if (!enemyList[i].activeSelf)
            {
                //EnemyList[i].SetActive(true);
                //enemyList[i].GetComponent<NavMeshAgent>().enabled = false;
                return enemyList[i];
            }

        }
        return null;

    }
}
