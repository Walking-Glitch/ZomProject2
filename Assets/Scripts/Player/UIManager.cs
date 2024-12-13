using Assets.Scripts.Player;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Image HealthFill;
    public PlayerStatus player;
    void Start()
    {
        UpdateHealthUI();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateHealthUI()
    {
        HealthFill.fillAmount = (float)(player.Health * 0.01);
    }
}
