using UnityEngine;

public class GameManager : MonoBehaviour
{
    #region References
    public EnemyPool EnemyPool;
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
    void Start()
    {
        instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
