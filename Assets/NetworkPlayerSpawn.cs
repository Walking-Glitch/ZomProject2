 
using Unity.Netcode;
using UnityEngine;

public class NetworkPlayerSpawn : NetworkBehaviour
{
    private static Transform[] spawnPoints;

    public override void OnNetworkSpawn()
    {
        if (!IsClient) return;

        // Find all spawn points only once
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            GameObject[] spawns = GameObject.FindGameObjectsWithTag("SpawnPoint");
            spawnPoints = new Transform[spawns.Length];
            for (int i = 0; i < spawns.Length; i++)
            {
                spawnPoints[i] = spawns[i].transform;
            }
        }

        // Assign a unique spawn point based on the number of connected players
        int playerIndex = (int)NetworkManager.Singleton.LocalClientId % spawnPoints.Length;
        transform.position = spawnPoints[playerIndex].position;
        transform.rotation = spawnPoints[playerIndex].rotation;

        Debug.Log($"Player {NetworkManager.Singleton.LocalClientId} spawned at {spawnPoints[playerIndex].position}");
    }
}

