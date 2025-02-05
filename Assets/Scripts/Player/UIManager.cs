using Assets.Scripts.Game_Manager;
using Assets.Scripts.Player;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : NetworkBehaviour
{
    public Image HealthFill;
    public PlayerStatus player;

    public TextMeshProUGUI Level;
    public TextMeshProUGUI Money;

    public TextMeshProUGUI Interact;

    //Networking variables

    [SerializeField] private GameObject networkImage;
    [SerializeField] private Button hostButton;
    [SerializeField] private Button clientButton;

    private GameManager gameManager;

    private void Awake()
    {
        hostButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartHost();
            Hide();
        });

        clientButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartClient();
            Hide();
        });
    }

    private void Hide()
    {
        networkImage.SetActive(false);
    }
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
        ToggleOffInteractText();
    }

    public void ToggleOffInteractText()
    {
        if (gameManager.PlayerStats.isInInteractableRange && !gameManager.Truck.isPlayerIn)
        {
            //Debug.Log("we are inside first if");
            Interact.gameObject.SetActive(true);
        }
        else if(!gameManager.PlayerStats.isInInteractableRange )
        {
            //Debug.Log("we are inside second if");
            Interact.gameObject.SetActive(false);
        }
        else if (gameManager.Truck.isPlayerIn)
        {
            //Debug.Log("we are inside third if");
            Interact.gameObject.SetActive(false);
        }
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
