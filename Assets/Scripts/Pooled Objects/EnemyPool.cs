using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyPool : MonoBehaviour
{
    [Tooltip("Zombie prefabs; every pooled zombie is a random pick from this list.")]
    [SerializeField] private List<GameObject> enemyPrefabs;

    [Tooltip("Zombies created when the level loads. Size it for the usual crowd (alive + corpses) so the pool rarely has to grow mid-game.")]
    [SerializeField] private int poolSize;
    [Tooltip("Hard cap the pool can grow to when every zombie is alive or still a corpse. Also caps how many zombies can be on the map at once.")]
    [SerializeField] private int maxPoolSize = 150;
    [Tooltip("Every pooled zombie (runtime).")]
    [SerializeField] private List<GameObject> enemyList;

    // scanning starts after the last zombie handed out, so a free one is usually found right away
    private int nextIndex;

    void Start()
    {
        AddEnemiesToPool(poolSize);
    }

    private void AddEnemiesToPool(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            GameObject enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Count)];
            GameObject enemy = Instantiate(enemyPrefab);
            enemy.SetActive(false);
            enemyList.Add(enemy);
            enemy.transform.parent = transform;
        }
    }

    // returns null only when the pool is at its cap and every zombie is in use
    public GameObject RequestEnemy()
    {
        for (int i = 0; i < enemyList.Count; i++)
        {
            int index = (nextIndex + i) % enemyList.Count;

            if (!enemyList[index].activeSelf)
            {
                nextIndex = index + 1;
                return enemyList[index];
            }
        }

        // grow one at a time so the instantiation cost is spread over spawns instead of one big hitch
        if (enemyList.Count < maxPoolSize)
        {
            AddEnemiesToPool(1);
            return enemyList[enemyList.Count - 1];
        }

        return null;
    }
}
