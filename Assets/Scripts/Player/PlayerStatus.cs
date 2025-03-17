using Assets.Scripts.Game_Manager;
using Unity.Netcode;
using UnityEngine;

namespace Assets.Scripts.Player
{
    public class PlayerStatus : NetworkBehaviour, IAttackable
    {
        public int MaxHealth;
        public int Health;

        public bool isInInteractableRange;

        private GameManager gameManager;

        //torso light 
        public Light TorsoLight;

        private void OnEnable()
        {
            DayCycle.OnNightTimeChanged += ToggleLight;

        }
        private void OnDisable()
        {
            DayCycle.OnNightTimeChanged -= ToggleLight;
        }
        void Awake()
        {
            Health = MaxHealth;
        }
        void Start()
        {
            gameManager = GameManager.Instance;
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.H))
            {
                PlayerTakeDamage(10);
            }
        }

        public void PlayerTakeDamage(int damage)
        {
            Health -= damage;
            Health = Mathf.Clamp(Health, 0, MaxHealth);
            gameManager.UIManager.UpdateHealthUI();
        }

        private void ToggleLight(bool isNight)
        {
            TorsoLight.enabled = isNight;
        }

        public void setIsInInteractableRange(bool isInRange)
        {
            isInInteractableRange = isInRange;
        }

        public Transform GetTransform()
        {
            return transform;
        }

        public int GetPriority()
        {
            return 1;
        }

        public void TakeDamage(int amount) // to be called
        {
            Health -= amount;
            Health = Mathf.Clamp(Health, 0, MaxHealth);
            gameManager.UIManager.UpdateHealthUI();

            if (Health <= 0)
            {
                Death();
            }
        }

        public void Death()
        {
            Debug.Log("dead called");
            //
        }
    }
}
