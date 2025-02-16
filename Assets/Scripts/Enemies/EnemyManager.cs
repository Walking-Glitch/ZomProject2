using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Assets.Scripts.Game_Manager;
using Unity.Netcode;
using UnityEngine;

public class EnemyManager : NetworkBehaviour
{
    public Transform parentSpawnPoint;

    [SerializeField] private List<Transform> spawnPointsList = new List<Transform>();

    public int EnemyCtr = 0;
    public int MaxEnemy;
    public float Delay;

    private bool isSpawning;

    private GameManager gameManager;

    private bool isInitialized = false;


    void Start()
    {
        gameManager = GameManager.Instance;

        StartCoroutine(WaitForPlayer());
    }

    private IEnumerator WaitForPlayer()
    {
        while (gameManager.PlayerGameObject == null)
        {
            yield return null;
        }

        CollectChildObjects(parentSpawnPoint);

        isInitialized = true;
    }
    // Update is called once per frame
    void Update()
    {
        if(!isInitialized) return;

        if(!IsServer && !IsClient)
        {
            if (!isSpawning && EnemyCtr < MaxEnemy)
            {
                //StartCoroutine(SpawnEnemies());
            }
        }
        else
        {
            if (!isSpawning && EnemyCtr < MaxEnemy)
            {
                StartCoroutine(NetworkSpawnEnemies());
            }
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

    private IEnumerator NetworkSpawnEnemies()
    {
        if (!IsServer) yield break; // Ensure only the server spawns enemies

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
            isSpawning = false;
            yield break;
        }

        // Request an enemy from the synchronized pool
        NetworkObject tempEnemy = gameManager.EnemyPool.RequestNetworkEnemy();

        if (tempEnemy == null)
        {
            Debug.LogError("No available enemies in pool!");
            isSpawning = false;
            yield break;
        }

        tempEnemy.transform.position = selectedSpawnPoint.position;
        tempEnemy.transform.rotation = selectedSpawnPoint.rotation;

        tempEnemy.gameObject.SetActive(true); // Activate on the server

        // Tell all clients to activate this specific enemy
        ActivateEnemyClientRpc(tempEnemy.NetworkObjectId);

        EnemyCtr++;

        yield return new WaitForSeconds(Delay);
        isSpawning = false;
    }

    [ClientRpc]
    private void ActivateEnemyClientRpc(ulong enemyId)
    {
        NetworkObject enemyNetworkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[enemyId];
        enemyNetworkObject.gameObject.SetActive(true);
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
