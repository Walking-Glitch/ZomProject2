using Assets.Scripts.Player;
using Assets.Scripts.Player.Weapon;
using UnityEngine;

namespace Assets.Scripts.Game_Manager
{
    public class GameManager : MonoBehaviour
    {
        #region References

        public PlayerStatus PlayerStats;
        public GameObject PlayerGameObject;
        public EnemyPool EnemyPool;
        public GrenadePool GrenadePool;
        public EnemyManager EnemyManager;
        public WeaponManager WeaponManager;
        public CasingManager CasingManager;
        public BulletCasingPool BulletCasingPool;
        #endregion

        #region Singleton

        private static GameManager instance;

        private GameManager() {}

        public static GameManager Instance
        {
            get
            {
                if (instance is null)
                    Debug.LogError("Game Manager is Null");
                return instance;
            }

        }
        #endregion

        private void Awake()
        {
            instance = this;
        }
        void Start()
        {
           
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
