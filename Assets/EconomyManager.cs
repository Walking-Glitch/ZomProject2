using Assets.Scripts.Game_Manager;
using UnityEngine;

public class EconomyManager : MonoBehaviour
{
    public int CurrentMoney;

    private GameManager gameManager;
    void Start()
    {
        gameManager = GameManager.Instance;
    }

    public void CollectMoney(int money)
    {
        CurrentMoney += money;
        gameManager.UIManager.UpdateMoneyUI(CurrentMoney);
    }
    
}
