using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Game_Manager;
using Assets.Scripts.Player;
using Assets.Scripts.Player.Actions;
using Assets.Scripts.Player.Weapon;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// first pass of the lose rules: dying respawns the player at a cabin for a money penalty,
// the run ends when the last cabin falls, and a results screen shows how it went.
// Creates itself (and its UI) when the scene has none; add it to a scene object to tune the values.
public class GameOverManager : MonoBehaviour
{
    public static GameOverManager Instance { get; private set; }

    [Header("Player Respawn")]
    [Tooltip("Seconds between dying and respawning at the nearest standing cabin.")]
    public float RespawnDelay = 5f;
    [Tooltip("Share of current money (0-1) lost on each death. 0.25 = lose a quarter.")]
    [Range(0f, 1f)] public float MoneyPenalty = 0.25f;
    [Tooltip("Seconds the player can't be damaged after respawning, so zombies at the cabin can't kill them right away.")]
    public float RespawnInvulnerability = 3f;
    [Tooltip("How far (m) in front of the cabin's wall the player respawns. A child object named \"RespawnPoint\" on a cabin overrides this.")]
    public float RespawnDistance = 3f;
    [Tooltip("Layers the respawn point is dropped onto. Empty = Ground.")]
    public LayerMask GroundMask;

    [Header("Run Stats (runtime)")]
    [Tooltip("Zombies killed this run, by the player or the defenses.")]
    public int Kills;
    [Tooltip("Times the player died this run.")]
    public int Deaths;
    [Tooltip("True once the last cabin has fallen.")]
    public bool IsGameOver;

    private GameManager gameManager;
    private bool respawning;
    private float runStartTime;
    private int cabinsAtStart;

    // player pieces switched off while dead
    private Behaviour[] playerControls;
    private CharacterController playerController;
    private readonly List<Renderer> hiddenRenderers = new List<Renderer>();

    // UI
    private GameObject deathPanel;
    private TextMeshProUGUI deathText;
    private GameObject resultsPanel;
    private TextMeshProUGUI resultsText;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        EnsureInstance();
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        EnsureInstance();
    }

    // only gameplay scenes (the ones with a GameManager) get one
    private static void EnsureInstance()
    {
        if (FindFirstObjectByType<GameOverManager>() != null) return;
        if (FindFirstObjectByType<GameManager>() == null) return;

        new GameObject("GameOverManager").AddComponent<GameOverManager>();
    }

    private void Awake()
    {
        Instance = this;

        if (GroundMask.value == 0) GroundMask = LayerMask.GetMask("Ground");

        BuildUI();
    }

    private void OnEnable()
    {
        PlayerStatus.OnPlayerDied += HandlePlayerDied;
        Structure.OnAllStructuresDestroyed += HandleAllStructuresDestroyed;
        ZombieStateManager.OnZombieKilled += HandleZombieKilled;
    }

    private void OnDisable()
    {
        PlayerStatus.OnPlayerDied -= HandlePlayerDied;
        Structure.OnAllStructuresDestroyed -= HandleAllStructuresDestroyed;
        ZombieStateManager.OnZombieKilled -= HandleZombieKilled;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void Start()
    {
        gameManager = GameManager.Instance;
        runStartTime = Time.time;
        cabinsAtStart = Structure.Active.Count;

        GameObject player = gameManager.PlayerGameObject;
        playerController = player.GetComponent<CharacterController>();

        List<Behaviour> controls = new List<Behaviour>();
        AddIfFound(controls, player.GetComponentInChildren<MovementStateManager>());
        AddIfFound(controls, player.GetComponentInChildren<AimStateManager>());
        AddIfFound(controls, player.GetComponentInChildren<ActionStateManager>());
        AddIfFound(controls, player.GetComponentInChildren<WeaponManager>());
        AddIfFound(controls, player.GetComponentInChildren<WeaponLaser>());
        playerControls = controls.ToArray();
    }

    private static void AddIfFound(List<Behaviour> list, Behaviour behaviour)
    {
        if (behaviour != null) list.Add(behaviour);
    }

    private void HandleZombieKilled()
    {
        Kills++;
    }

    #region Player death

    private void HandlePlayerDied()
    {
        if (IsGameOver || respawning) return;

        StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine()
    {
        respawning = true;
        Deaths++;

        EconomyManager economy = gameManager.EconomyManager;
        int moneyLost = Mathf.FloorToInt(economy.CurrentMoney * MoneyPenalty);
        economy.LoseMoney(moneyLost);

        SetPlayerAlive(false);
        deathPanel.SetActive(true);

        for (float timeLeft = RespawnDelay; timeLeft > 0f; timeLeft -= Time.deltaTime)
        {
            deathText.text = "YOU DIED\n<size=50%>Respawning in " + Mathf.CeilToInt(timeLeft) + "</size>"
                + (moneyLost > 0 ? "\n<size=40%><color=#FF6060>-$" + moneyLost + "</color></size>" : "");
            yield return null;
        }

        Structure cabin = FindNearestCabin(gameManager.PlayerGameObject.transform.position);

        // no cabin left to respawn at: the game over screen takes it from here
        if (cabin == null) yield break;

        TeleportPlayer(GetRespawnPosition(cabin));

        PlayerStatus player = gameManager.PlayerStats;
        player.Revive();
        player.SetInvulnerable(RespawnInvulnerability);

        SetPlayerAlive(true);
        deathPanel.SetActive(false);
        respawning = false;
    }

    private void SetPlayerAlive(bool alive)
    {
        SetPlayerControls(alive);

        if (playerController != null) playerController.enabled = alive;

        if (!alive)
        {
            // leave the truck first, as its own exit does
            TruckController truck = gameManager.Truck;
            if (truck != null && truck.isPlayerIn)
            {
                truck.isPlayerIn = false;
                gameManager.PlayerGameObject.GetComponent<ActionStateManager>().anim.SetBool("IsDriving", false);
            }

            // no death animation yet, so the body just disappears
            hiddenRenderers.Clear();
            foreach (Renderer rend in gameManager.PlayerGameObject.GetComponentsInChildren<Renderer>())
            {
                if (!rend.enabled) continue;

                rend.enabled = false;
                hiddenRenderers.Add(rend);
            }
        }
        else
        {
            foreach (Renderer rend in hiddenRenderers)
            {
                if (rend != null) rend.enabled = true;
            }
            hiddenRenderers.Clear();

            MovementStateManager movement = gameManager.PlayerGameObject.GetComponentInChildren<MovementStateManager>();
            if (movement != null) movement.moveInput = Vector2.zero;
        }
    }

    private void SetPlayerControls(bool enabled)
    {
        if (playerControls == null) return;

        foreach (Behaviour control in playerControls)
        {
            if (control != null) control.enabled = enabled;
        }
    }

    private static Structure FindNearestCabin(Vector3 from)
    {
        Structure nearest = null;
        float nearestSqrDistance = float.MaxValue;

        foreach (Structure cabin in Structure.Active)
        {
            if (!cabin.IsAlive()) continue;

            float sqrDistance = (cabin.transform.position - from).sqrMagnitude;
            if (sqrDistance < nearestSqrDistance)
            {
                nearest = cabin;
                nearestSqrDistance = sqrDistance;
            }
        }

        return nearest;
    }

    private Vector3 GetRespawnPosition(Structure cabin)
    {
        Vector3 position;
        Transform respawnPoint = cabin.transform.Find("RespawnPoint");

        if (respawnPoint != null)
        {
            position = respawnPoint.position;
        }
        else
        {
            // just outside the wall on the cabin's front side
            Vector3 forward = cabin.transform.forward;
            Vector3 wall = cabin.GetClosestPoint(cabin.transform.position + forward * 50f);
            position = wall + forward * RespawnDistance;
        }

        if (Physics.Raycast(position + Vector3.up * 20f, Vector3.down, out RaycastHit hit, 50f, GroundMask, QueryTriggerInteraction.Ignore))
        {
            position = hit.point + Vector3.up * 0.1f;
        }

        return position;
    }

    private void TeleportPlayer(Vector3 position)
    {
        // a CharacterController overrides transform moves while it's enabled; it's off while dead
        gameManager.PlayerGameObject.transform.position = position;
        Physics.SyncTransforms();
    }

    #endregion

    #region Game over

    private void HandleAllStructuresDestroyed()
    {
        if (IsGameOver) return;

        IsGameOver = true;
        StopAllCoroutines();
        respawning = false;

        deathPanel.SetActive(false);
        SetPlayerControls(false);
        if (gameManager.BuildManager != null) gameManager.BuildManager.enabled = false;

        Time.timeScale = 0f;
        AudioListener.pause = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        resultsText.text = BuildResults();
        resultsPanel.SetActive(true);
    }

    private string BuildResults()
    {
        int seconds = Mathf.FloorToInt(Time.time - runStartTime);
        int days = gameManager.DifficultyManager != null ? gameManager.DifficultyManager.Day : 0;

        return "Days survived: <b>" + days + "</b>\n"
            + "Zombies killed: <b>" + Kills + "</b>\n"
            + "Money earned: <b>$" + gameManager.EconomyManager.TotalMoneyEarned + "</b>\n"
            + "Deaths: <b>" + Deaths + "</b>\n"
            + "Cabins lost: <b>" + cabinsAtStart + "</b>\n"
            + "Time played: <b>" + (seconds / 60) + ":" + (seconds % 60).ToString("00") + "</b>";
    }

    private void Restart()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void Quit()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    #endregion

    #region UI

    private void BuildUI()
    {
        GameObject canvasObject = new GameObject("GameOverCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(transform, false);

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        // death overlay
        deathPanel = CreatePanel(canvasObject.transform, "DeathPanel", new Color(0.35f, 0f, 0f, 0.35f));
        deathText = CreateText(deathPanel.transform, "", 110, Vector2.zero, new Vector2(1200, 500));
        deathPanel.SetActive(false);

        // results screen
        resultsPanel = CreatePanel(canvasObject.transform, "ResultsPanel", new Color(0f, 0f, 0f, 0.85f));

        TextMeshProUGUI title = CreateText(resultsPanel.transform, "GAME OVER", 120, new Vector2(0, 330), new Vector2(1200, 160));
        title.color = new Color(0.85f, 0.1f, 0.1f);
        TextMeshProUGUI subtitle = CreateText(resultsPanel.transform, "All cabins have fallen", 44, new Vector2(0, 230), new Vector2(1200, 60));
        subtitle.color = new Color(0.8f, 0.8f, 0.8f);

        resultsText = CreateText(resultsPanel.transform, "", 46, new Vector2(0, -10), new Vector2(900, 380));
        resultsText.lineSpacing = 20;

        CreateButton(resultsPanel.transform, "Restart", new Vector2(-180, -320), Restart);
        CreateButton(resultsPanel.transform, "Quit", new Vector2(180, -320), Quit);

        resultsPanel.SetActive(false);

        EnsureEventSystem();
    }

    private static GameObject CreatePanel(Transform parent, string name, Color color)
    {
        GameObject panel = new GameObject(name, typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(parent, false);

        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        panel.GetComponent<Image>().color = color;

        return panel;
    }

    private static TextMeshProUGUI CreateText(Transform parent, string text, float size, Vector2 position, Vector2 boxSize)
    {
        GameObject textObject = new GameObject("Text", typeof(RectTransform));
        textObject.transform.SetParent(parent, false);

        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = boxSize;

        TextMeshProUGUI label = textObject.AddComponent<TextMeshProUGUI>();
        label.text = text;
        label.fontSize = size;
        label.alignment = TextAlignmentOptions.Center;
        label.color = Color.white;
        label.raycastTarget = false;

        return label;
    }

    private static void CreateButton(Transform parent, string label, Vector2 position, UnityEngine.Events.UnityAction onClick)
    {
        GameObject buttonObject = new GameObject(label + "Button", typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(300, 90);

        buttonObject.GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f, 1f);

        Button button = buttonObject.GetComponent<Button>();
        ColorBlock colors = button.colors;
        colors.highlightedColor = new Color(0.55f, 0.15f, 0.15f, 1f);
        colors.pressedColor = new Color(0.4f, 0.1f, 0.1f, 1f);
        button.colors = colors;
        button.onClick.AddListener(onClick);

        CreateText(buttonObject.transform, label, 44, Vector2.zero, rect.sizeDelta);
    }

    private static void EnsureEventSystem()
    {
        if (FindFirstObjectByType<EventSystem>() != null) return;

        new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
    }

    #endregion
}
