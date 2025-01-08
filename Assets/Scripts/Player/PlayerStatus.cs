using UnityEngine;

namespace Assets.Scripts.Player
{
    public class PlayerStatus : MonoBehaviour
    {
        public int MaxHealth;
        public int Health;
        public UIManager UiManager;

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
            UiManager.UpdateHealthUI();
        }

        private void ToggleLight(bool isNight)
        {
            TorsoLight.enabled = isNight;
        }

    }
}
