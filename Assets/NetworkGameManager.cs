using Unity.Netcode;
using UnityEngine;

public class NetworkGameManager : NetworkBehaviour
{
    public static NetworkGameManager Instance;

    [SerializeField] private GameObject playerPrefab; // Assign in Inspector
    [SerializeField] private Transform[] spawnPoints;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }

    private void OnClientConnected(ulong clientId)
    {
        if (IsServer) // Only the server should spawn players
        {
            Debug.Log($"Client {clientId} connected. Spawning player...");
            SpawnPlayer(clientId);
        }
    }

    public void SpawnPlayer(ulong clientId)
    {
        // Pick a spawn point
        Transform spawnPoint = spawnPoints[clientId % (ulong)spawnPoints.Length];

        // Instantiate and spawn the player
        GameObject player = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
        NetworkObject netObj = player.GetComponent<NetworkObject>();
        netObj.SpawnAsPlayerObject(clientId); // Assign ownership immediately
    }
}