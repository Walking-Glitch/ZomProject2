using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Game_Manager;
using NUnit.Framework;
using UnityEditor.Build.Content;
using UnityEngine;
using UnityEngine.AI;

public class EnemyManager : MonoBehaviour
{
    public Transform parentSpawnPoint;

    [SerializeField] private List<Transform> spawnPointsList = new List<Transform>();

    public int EnemyCtr = 0;
    public int MaxEnemy;
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
        tempEnemy.transform.position = selectedSpawnPoint.position;
        tempEnemy.transform.rotation = selectedSpawnPoint.rotation;
        
        tempEnemy.SetActive(true);
        EnemyCtr++;

        yield return new WaitForSeconds(Delay);
        isSpawning = false;

    }

    Transform GetValidSpawnPoint()
    {
        int i = Random.Range(0, spawnPointsList.Count - 1);
        
        return spawnPointsList[i];
    }

    public void DecreaseEnemyCtr()
    {
        EnemyCtr--;
    }
}
