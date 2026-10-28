using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Game_Manager;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    [Tooltip("Parent object whose children are the zombie spawn points.")]
    public Transform parentSpawnPoint;

    [SerializeField] private List<Transform> spawnPointsList = new List<Transform>();

    [Tooltip("Zombies currently on the map, alive or corpse (runtime).")]
    public int EnemyCtr = 0;
    [Tooltip("Max zombies on the map at once. Grows every morning (see DifficultyManager). Effectively capped by the EnemyPool's Max Pool Size.")]
    public int MaxEnemy;
    [Tooltip("Seconds between spawns while below Max Enemy.")]
    public float Delay;

    private bool isSpawning;

    private GameManager gameManager;

    void Start()
    {
        gameManager = GameManager.Instance;

        CollectChildObjects(parentSpawnPoint);
    }

    // Update is called once per frame
    void Update()
    {
        if (!isSpawning && EnemyCtr < MaxEnemy)
        {
            StartCoroutine(SpawnEnemies());
        }
    }

    void CollectChildObjects(Transform parentSpawnPoint)
    {
        // Iterate through each child of the parent
        foreach (Transform sp in parentSpawnPoint)
        {
            // Add the child object to the list
            spawnPointsList.Add(sp.transform);
        }
    }
    private IEnumerator SpawnEnemies()
    {
        isSpawning = true;

        Transform selectedSpawnPoint = GetValidSpawnPoint();

        int attempts = 0;

        while (selectedSpawnPoint == null && attempts < 10)
        {
            selectedSpawnPoint = GetValidSpawnPoint();
            attempts++;
        }

        if (selectedSpawnPoint == null)
        {
            // Debug.Log("Failed to find a valid spawn point after multiple attempts.");
            isSpawning = false;
            yield break;
        }

        GameObject tempEnemy = gameManager.EnemyPool.RequestEnemy();

        if (tempEnemy == null)
        {
            // pool is at its cap; wait for a corpse to despawn instead of stalling the spawner
            yield return new WaitForSeconds(Delay);
            isSpawning = false;
            yield break;
        }

        tempEnemy.transform.position = selectedSpawnPoint.position;
        tempEnemy.transform.rotation = selectedSpawnPoint.rotation;
        
        tempEnemy.SetActive(true);
        EnemyCtr++;

        yield return new WaitForSeconds(Delay);
        isSpawning = false;

    }

    Transform GetValidSpawnPoint()
    {
        int i = Random.Range(0, spawnPointsList.Count); // int max is exclusive
        
        return spawnPointsList[i];
    }

    public void DecreaseEnemyCtr()
    {
        EnemyCtr--;
    }
}
