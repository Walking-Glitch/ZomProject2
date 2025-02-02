using Assets.Scripts.Game_Manager;
using UnityEngine;


public class DifficultyManager : MonoBehaviour
{
    
    public int Day;

    private GameManager gameManager;
 
    void Start()
    {
        gameManager = GameManager.Instance;
        Day = 0;
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void IncreaseLevel()
    {
        Day++;
        gameManager.UIManager.UpdateLevelUI(Day);

        int additionalEnemies = Mathf.FloorToInt(Day * 1.5f);

        gameManager.EnemyManager.MaxEnemy += additionalEnemies; 
    }
}
