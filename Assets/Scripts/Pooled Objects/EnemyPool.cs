using Assets.Scripts.Game_Manager;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class EnemyPool : NetworkBehaviour
{
    [SerializeField] private List<GameObject> enemyPrefabs;

    [SerializeField] private int poolSize;
    [SerializeField] private List<GameObject> enemyList;

    private GameManager gameManager;
    public bool isInitialized = false;



    [SerializeField] private NetworkList<NetworkObjectReference> networkList = new NetworkList<NetworkObjectReference>();


    void Start()
    {
        gameManager = GameManager.Instance;

       
        //AddEnemiesToPool(poolSize);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
        {
            NetworkAddEnemiesToPool(poolSize);
        } 
    }


    private void AddEnemiesToPool(int amount)
    {

        for (int i = 0; i < amount; i++)
        {
            GameObject enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Count)];
            GameObject enemy = Instantiate(enemyPrefab);

            //If running single-player, do not treat it as a networked object.
            if (NetworkManager.Singleton.IsListening)
            {
                NetworkObject enemyNetObj = enemy.GetComponent<NetworkObject>();
                if (enemyNetObj != null)
                {
                    enemyNetObj.TrySetParent(transform, true);
                }
            }
            else
            {
                enemy.transform.parent = transform; // Regular parenting in single-player
            }

            enemy.SetActive(false);
            enemyList.Add(enemy);
        }
    }

    public void NetworkAddEnemiesToPool(int amount)
    {
        //Debug.Log("inside network pool");
        for (int i = 0; i < amount; i++)
        {
            GameObject enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Count)];
            GameObject enemy = Instantiate(enemyPrefab);
            NetworkObject enemyNetObj = enemy.GetComponent<NetworkObject>();

            if (enemyNetObj == null)
            {
                //Debug.LogError("Enemy prefab is missing a NetworkObject component!");
                return;
            }

            enemyNetObj.Spawn(false);  // Spawn on the server, but keep it disabled for now.
                                       //enemy.SetActive(false);     // Pool is inactive initially.



            disableNetworkZombiesClientRpc(enemyNetObj.NetworkObjectId);
           


            enemy.transform.parent = transform;

        }
    }


    [ClientRpc]   
    private void disableNetworkZombiesClientRpc(ulong enemyNetId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(enemyNetId, out NetworkObject enemyNetObj))
        {
            //Debug.Log("we are disabling network zombie rpc");

            //networkEnemyList.Add(enemyNetObj);
            //enemyNetObj.gameObject.SetActive(false);

            NetworkObjectReference enemyRef = new NetworkObjectReference(enemyNetObj);
            networkList.Add(enemyRef);

            foreach (NetworkObjectReference netObjRef in networkList)
            {
                //Debug.Log("dentro del foreach");

                if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(netObjRef.NetworkObjectId, out NetworkObject enemyNetObj2))
                {
                    //enemyNetObj2.gameObject.SetActive(false);
                    enemyNetObj2.GetComponentInChildren<ZombieStateManager>().NetworkIsActive.Value = false;
                }
                else
                {
                    //Debug.LogWarning($"Failed to find NetworkObject with ID {netObjRef.NetworkObjectId}");
                }

            }
        }
        else
        {
            //Debug.Log("else clause called on disable network zombie rpc");
        }
    }





    public GameObject RequestEnemy()
    {
        for (int i = 0; i < enemyList.Count; i++)
        {
            if (!enemyList[i].activeSelf)
            {
                return enemyList[i];
            }

        }
        return null;
    }

    public NetworkObject RequestNetworkEnemy()
    {
        for (int i = 0; i < networkList.Count; i++)
        {
            NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkList[i].NetworkObjectId, out NetworkObject enemyNetObj);
            if (!enemyNetObj.isActiveAndEnabled)
            {
                return enemyNetObj;
            }


        }
        return null;
    }
}