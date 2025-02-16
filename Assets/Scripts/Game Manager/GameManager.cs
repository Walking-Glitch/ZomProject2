using Assets.Scripts.Player;
using Assets.Scripts.Player.Weapon;
using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Assets.Scripts.Game_Manager
{
    public class GameManager : NetworkBehaviour
    {
        #region References

        [Header("Player references")]
        public PlayerStatus PlayerStats;
        public GameObject PlayerGameObject;

        [Header("Pooled objects references")]
        public EnemyPool EnemyPool;
        public GrenadePool GrenadePool;
        public DecalPool DecalPool;
        public BulletCasingPool BulletCasingPool;

        [Header("Manager references")]
        public EnemyManager EnemyManager;
        public WeaponManager WeaponManager;
        public CasingManager CasingManager;
        public DecalManager DecalManager;
        public BuildManager BuildManager;
        public UIManager UIManager;
        public EconomyManager EconomyManager;

        [Header("Weapon related references")]
        public WeaponAmmo WeaponAmmo;
        public WeaponLaser WeaponLaser;

        [Header("Difficulty references")]
        public DayCycle DayCycle;
        public DifficultyManager DifficultyManager;

        [Header("Vehicle references")]
        public TruckController Truck;

        #endregion


        #region Singleton
        private static GameManager instance;



        public static GameManager Instance
        {
            get
            {
                if (instance == null)
                {
                    Debug.LogError("GameManager is NULL. Make sure it exists in the scene.");
                }
                return instance;
            }
        }
        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Debug.LogWarning("Multiple GameManager instances found! Destroying the duplicate.");
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        #endregion

        #region Multiplayer Initialization

        public override void OnNetworkSpawn()
        {
            if (IsClient)
            {
                StartCoroutine(FindAndAssignLocalPlayer());
            }
        }

        private IEnumerator FindAndAssignLocalPlayer()
        {
            while (PlayerStats == null)
            {
                yield return new WaitForSeconds(0.2f);

                foreach (var playerStats in FindObjectsOfType<PlayerStatus>())
                {
                    if (playerStats.IsOwner) // Assign only the local player's components
                    {
                        PlayerGameObject = playerStats.gameObject;
                        PlayerStats = playerStats;
                        WeaponManager = PlayerGameObject.GetComponent<WeaponManager>();                        
                        WeaponAmmo = PlayerGameObject.GetComponent<WeaponAmmo>();
                        WeaponLaser = PlayerGameObject.GetComponent<WeaponLaser>();

                        Debug.Log("GameManager: Local player assigned.");

                        break;
                    }
                }
            }
        }
        #endregion

    }


}



 