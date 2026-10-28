using Assets.Scripts.Game_Manager;
using UnityEngine;

namespace Assets.Scripts.Player
{
    public class PlayerStatus : MonoBehaviour, IAttackable
    {
        [Tooltip("Player starting health. Zombies hit for their Attack Damage (10) per swing.")]
        public int MaxHealth;
        [Tooltip("Current health (runtime). Damage clamps it to Max Health, so raise Max Health for a tankier test run.")]
        public int Health;

        [Tooltip("True while the player is next to something they can interact with, like the truck (runtime).")]
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

        public Vector3 GetClosestPoint(Vector3 from)
        {
            // the player is reached through the swing triggers, so the pivot is enough
            return transform.position;
        }

        public int GetHealth()
        {
            return Health;
        }

        public int GetPriority()
        {
            return TargetPriority.Diversion;
        }

        public void TakeDamage(int amount)
        {
            PlayerTakeDamage(amount);
        }

        public void Death()
        {
            // game over is still pending; at 0 health zombies simply stop targeting the player
        }

    }
}
