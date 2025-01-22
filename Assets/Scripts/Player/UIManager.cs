using Assets.Scripts.Game_Manager;
using Assets.Scripts.Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Image HealthFill;
    public PlayerStatus player;

    public TextMeshProUGUI Level;
    public TextMeshProUGUI Money;

    private GameManager gameManager;
    void Start()
    {
        gameManager = GameManager.Instance;

        UpdateHealthUI();
        UpdateLevelUI(gameManager.DifficultyManager.Day);
        UpdateMoneyUI(gameManager.EconomyManager.CurrentMoney);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateMoneyUI(int money)
    {         
        Money.text = money.ToString();
    }
    public void UpdateLevelUI(int level)
    {
        Level.text = level.ToString(); 
    }
    public void UpdateHealthUI()
    {
        HealthFill.fillAmount = (float)(player.Health * 0.01);
    }
}
