using Assets.Scripts.Game_Manager;
using UnityEngine;


public class DifficultyManager : MonoBehaviour
{
    [Tooltip("Current day (runtime). Goes up every morning at 6 AM.")]
    public int Day;

    [Tooltip("Each morning Max Enemy grows by floor(Day x this). 1.5 = +1 on day 1, +3 on day 2, +4 on day 3...")]
    [SerializeField] private float enemiesPerDayMultiplier = 1.5f;

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

        int additionalEnemies = Mathf.FloorToInt(Day * enemiesPerDayMultiplier);

        gameManager.EnemyManager.MaxEnemy += additionalEnemies;
    }
}
