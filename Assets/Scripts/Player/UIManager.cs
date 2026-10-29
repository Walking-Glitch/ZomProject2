using System.Collections.Generic;
using Assets.Scripts.Game_Manager;
using Assets.Scripts.Player;
using Assets.Scripts.Player.Actions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// player HUD. Every element except the originals is optional, so the HUD works with any subset;
// Tools > ZomProject > Build HUD creates and wires the full layout in the scene.
public class UIManager : MonoBehaviour
{
    [Header("Player")]
    public Image HealthFill;
    public PlayerStatus player;
    [Tooltip("Health number next to the bar. Turns cyan while the player is invulnerable after a respawn.")]
    public TextMeshProUGUI HealthText;

    [Header("Cabins")]
    [Tooltip("Row container for the cabin health bars (one row is cloned from the template per cabin).")]
    public RectTransform CabinList;
    [Tooltip("Inactive row with children \"Name\" (text), \"Bar/Fill\" (rect) and \"Value\" (text).")]
    public GameObject CabinRowTemplate;

    [Header("Day Cycle")]
    [Tooltip("\"Day N\". The clock itself is written by DayCycle's Time Text.")]
    public TextMeshProUGUI Level;
    [Tooltip("\"Night in m:ss\" / \"Dawn in m:ss\" in real seconds.")]
    public TextMeshProUGUI PhaseText;
    [Tooltip("Living zombies on the map.")]
    public TextMeshProUGUI ZombieText;

    [Header("Defenses")]
    [Tooltip("Fill rect of the shared turret fuel bar (its right anchor is the fill amount).")]
    public RectTransform TurretFuelFill;
    public TextMeshProUGUI TurretFuelText;
    [Tooltip("Fill rect of the shared turret ammo bar.")]
    public RectTransform TurretAmmoFill;
    public TextMeshProUGUI TurretAmmoText;

    [Header("Resources")]
    public TextMeshProUGUI Money;
    public TextMeshProUGUI SparePartsText;
    public TextMeshProUGUI FuelBarrelsText;
    public TextMeshProUGUI GeneratorsText;

    [Header("Weapon")]
    [Tooltip("\"magazine / reserve\".")]
    public TextMeshProUGUI AmmoText;
    public TextMeshProUGUI FireModeText;

    [Header("Messages")]
    public TextMeshProUGUI Interact;
    [Tooltip("Short warnings: cabin under attack or destroyed, night falls, turret fuel/ammo low.")]
    public TextMeshProUGUI ToastText;
    [Tooltip("Build controls, shown only in build mode.")]
    public TextMeshProUGUI BuildHintText;
    [Tooltip("FPS counter, hidden by default and toggled with F3.")]
    public GameObject FpsText;

    [Header("Settings")]
    [Tooltip("Turret fuel/ammo bars turn red and warn below this share (0-1) of the most they've held.")]
    [Range(0f, 1f)] public float LowResourceFraction = 0.2f;
    [Tooltip("Seconds a warning stays on screen.")]
    public float ToastDuration = 3f;
    [Tooltip("Seconds between \"under attack\" warnings for the same cabin.")]
    public float AttackWarningCooldown = 10f;
    [Tooltip("Seconds a cabin's bar keeps flashing after a hit.")]
    public float CabinFlashTime = 1.5f;
    [Tooltip("Seconds between HUD text refreshes. Lower = smoother numbers, more string work.")]
    public float RefreshInterval = 0.1f;

    private static readonly Color Normal = Color.white;
    private static readonly Color Warning = new Color(1f, 0.33f, 0.33f);
    private static readonly Color Invulnerable = new Color(0.4f, 0.9f, 1f);
    private static readonly Color Faded = new Color(0.55f, 0.55f, 0.55f);
    private static readonly Color BarNormal = new Color(0.35f, 0.8f, 0.35f);

    private class CabinRow
    {
        public Structure Cabin;
        public string Name;
        public TextMeshProUGUI NameText;
        public RectTransform Fill;
        public Image FillImage;
        public TextMeshProUGUI ValueText;
        public float LastWarningTime = -100f;
    }

    private readonly List<CabinRow> cabinRows = new List<CabinRow>();

    private GameManager gameManager;
    private ActionStateManager playerActions;
    private float refreshTimer;
    private float toastHideTime;
    private float maxTurretFuel;
    private int maxTurretAmmo;
    private bool fuelWarned;
    private bool ammoWarned;

    void Start()
    {
        gameManager = GameManager.Instance;
        playerActions = gameManager.PlayerGameObject.GetComponent<ActionStateManager>();

        BuildCabinRows();

        if (ToastText != null) ToastText.gameObject.SetActive(false);
        if (BuildHintText != null)
        {
            BuildHintText.text = "1 Turrets  ·  2 Barriers  ·  Scroll Select  ·  Q/E Rotate  ·  Fire Place  ·  Inventory Exit";
            BuildHintText.gameObject.SetActive(false);
        }
        if (FpsText != null) FpsText.SetActive(false);

        UpdateHealthUI();
        UpdateLevelUI(gameManager.DifficultyManager.Day);
        UpdateMoneyUI(gameManager.EconomyManager.CurrentMoney);
        Refresh();
    }

    private void OnEnable()
    {
        Structure.OnStructureDamaged += HandleCabinDamaged;
        Structure.OnStructureDestroyed += HandleCabinDestroyed;
        DayCycle.OnNightTimeChanged += HandleNightChanged;
    }

    private void OnDisable()
    {
        Structure.OnStructureDamaged -= HandleCabinDamaged;
        Structure.OnStructureDestroyed -= HandleCabinDestroyed;
        DayCycle.OnNightTimeChanged -= HandleNightChanged;
    }

    void Update()
    {
        ToggleOffInteractText();

        if (Input.GetKeyDown(KeyCode.F3) && FpsText != null) FpsText.SetActive(!FpsText.activeSelf);

        if (BuildHintText != null && playerActions != null && BuildHintText.gameObject.activeSelf != playerActions.IsBuilding)
        {
            BuildHintText.gameObject.SetActive(playerActions.IsBuilding);
        }

        if (ToastText != null && ToastText.gameObject.activeSelf && Time.time >= toastHideTime)
        {
            ToastText.gameObject.SetActive(false);
        }

        // bars move every frame, text only refreshes a few times a second
        UpdateCabinRows();

        refreshTimer -= Time.deltaTime;
        if (refreshTimer <= 0f)
        {
            refreshTimer = RefreshInterval;
            Refresh();
        }
    }

    private void Refresh()
    {
        if (HealthText != null) HealthText.color = player.IsInvulnerable ? Invulnerable : Normal;

        DayCycle dayCycle = gameManager.DayCycle;
        if (PhaseText != null && dayCycle != null)
        {
            int seconds = Mathf.CeilToInt(dayCycle.SecondsUntilPhaseChange());
            PhaseText.text = (dayCycle.IsNightTime ? "Dawn in " : "Night in ") + (seconds / 60) + ":" + (seconds % 60).ToString("00");
            PhaseText.color = !dayCycle.IsNightTime && seconds <= 30 ? Warning : Normal;
        }

        if (ZombieText != null) ZombieText.text = "Zombies: " + ZombieStateManager.AliveCount;

        EconomyManager economy = gameManager.EconomyManager;

        maxTurretFuel = Mathf.Max(maxTurretFuel, economy.Fuel);
        maxTurretAmmo = Mathf.Max(maxTurretAmmo, economy.Ammo);
        float fuelFraction = maxTurretFuel > 0f ? economy.Fuel / maxTurretFuel : 0f;
        float ammoFraction = maxTurretAmmo > 0 ? (float)economy.Ammo / maxTurretAmmo : 0f;

        SetBar(TurretFuelFill, TurretFuelText, "Turret fuel  " + Mathf.CeilToInt(economy.Fuel).ToString("N0"), fuelFraction);
        SetBar(TurretAmmoFill, TurretAmmoText, "Turret ammo  " + economy.Ammo.ToString("N0"), ammoFraction);
        WarnWhenLow(fuelFraction, ref fuelWarned, "Turret fuel low!");
        WarnWhenLow(ammoFraction, ref ammoWarned, "Turret ammo low!");

        if (SparePartsText != null) SparePartsText.text = "Spare parts: " + economy.SpareParts.ToString("N0");
        if (FuelBarrelsText != null) FuelBarrelsText.text = "Fuel barrels: " + economy.FuelBarrels.ToString("N0");
        if (GeneratorsText != null) GeneratorsText.text = "Generators: " + economy.Generators.ToString("N0");

        WeaponAmmo weaponAmmo = gameManager.WeaponAmmo;
        if (AmmoText != null && weaponAmmo != null)
        {
            AmmoText.text = weaponAmmo.currentAmmo + " / " + weaponAmmo.extraAmmo;
            AmmoText.color = weaponAmmo.currentAmmo > 0 ? Normal : Warning;
        }

        if (FireModeText != null && gameManager.WeaponManager != null)
        {
            bool empty = weaponAmmo != null && weaponAmmo.currentAmmo <= 0;
            FireModeText.text = empty ? (weaponAmmo.extraAmmo > 0 ? "RELOAD" : "NO AMMO") : (gameManager.WeaponManager.IsSemiAuto ? "SEMI" : "AUTO");
            FireModeText.color = empty ? Warning : Normal;
        }
    }

    private void SetBar(RectTransform fill, TextMeshProUGUI label, string text, float fraction)
    {
        fraction = Mathf.Clamp01(fraction);
        bool low = fraction < LowResourceFraction;

        if (fill != null)
        {
            fill.anchorMax = new Vector2(fraction, 1f);

            Image image = fill.GetComponent<Image>();
            if (image != null) image.color = low ? Warning : BarNormal;
        }

        if (label != null)
        {
            label.text = text;
            label.color = low ? Warning : Normal;
        }
    }

    private void WarnWhenLow(float fraction, ref bool warned, string message)
    {
        if (fraction < LowResourceFraction)
        {
            if (!warned) ShowToast(message);
            warned = true;
        }
        else
        {
            warned = false;
        }
    }

    #region Cabins

    private void BuildCabinRows()
    {
        if (CabinList == null || CabinRowTemplate == null) return;

        CabinRowTemplate.SetActive(false);

        List<Structure> cabins = new List<Structure>(Structure.Active);
        cabins.Sort((a, b) => string.CompareOrdinal(a.name, b.name));

        for (int i = 0; i < cabins.Count; i++)
        {
            GameObject rowObject = Instantiate(CabinRowTemplate, CabinList);
            rowObject.name = "Cabin " + (char)('A' + i);
            rowObject.SetActive(true);

            Transform fill = rowObject.transform.Find("Bar/Fill");

            CabinRow row = new CabinRow
            {
                Cabin = cabins[i],
                Name = rowObject.name,
                NameText = FindText(rowObject.transform, "Name"),
                Fill = fill as RectTransform,
                FillImage = fill != null ? fill.GetComponent<Image>() : null,
                ValueText = FindText(rowObject.transform, "Value"),
            };

            if (row.NameText != null) row.NameText.text = row.Name;
            cabinRows.Add(row);
        }
    }

    private static TextMeshProUGUI FindText(Transform parent, string childName)
    {
        Transform child = parent.Find(childName);
        return child != null ? child.GetComponent<TextMeshProUGUI>() : null;
    }

    private void UpdateCabinRows()
    {
        foreach (CabinRow row in cabinRows)
        {
            bool standing = row.Cabin != null && row.Cabin.IsAlive();
            float fraction = standing && row.Cabin.MaxHealth > 0 ? (float)row.Cabin.Health / row.Cabin.MaxHealth : 0f;

            // flash while being hit
            bool flashing = standing && Time.time - row.Cabin.LastDamagedTime < CabinFlashTime && Mathf.Repeat(Time.time, 0.3f) < 0.15f;

            if (row.Fill != null) row.Fill.anchorMax = new Vector2(fraction, 1f);
            if (row.FillImage != null) row.FillImage.color = flashing || fraction < 0.25f ? Warning : BarNormal;
            if (row.NameText != null) row.NameText.color = !standing ? Faded : flashing ? Warning : Normal;

            if (row.ValueText != null)
            {
                string value = standing ? Mathf.CeilToInt(fraction * 100f) + "%" : "LOST";
                if (row.ValueText.text != value) row.ValueText.text = value;
                row.ValueText.color = standing ? Normal : Faded;
            }
        }
    }

    private CabinRow FindRow(Structure cabin)
    {
        foreach (CabinRow row in cabinRows)
        {
            if (row.Cabin == cabin) return row;
        }

        return null;
    }

    private void HandleCabinDamaged(Structure cabin)
    {
        CabinRow row = FindRow(cabin);
        if (row == null || Time.time - row.LastWarningTime < AttackWarningCooldown) return;

        row.LastWarningTime = Time.time;
        ShowToast(row.Name + " is under attack!");
    }

    private void HandleCabinDestroyed(Structure cabin)
    {
        CabinRow row = FindRow(cabin);
        ShowToast((row != null ? row.Name : "A cabin") + " has fallen!");
    }

    #endregion

    private void HandleNightChanged(bool isNight)
    {
        ShowToast(isNight ? "Night falls. The horde is coming." : "Dawn. The zombies slow down.");
    }

    public void ShowToast(string message)
    {
        if (ToastText == null) return;

        ToastText.text = message;
        ToastText.gameObject.SetActive(true);
        toastHideTime = Time.time + ToastDuration;
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
        Money.text = "$" + money.ToString("N0");
    }
    public void UpdateLevelUI(int level)
    {
        // Day counts mornings survived, so the first day shows as Day 1
        Level.text = "Day " + (level + 1);
    }
    public void UpdateHealthUI()
    {
        HealthFill.fillAmount = player.MaxHealth > 0 ? (float)player.Health / player.MaxHealth : 0f;

        if (HealthText != null) HealthText.text = player.Health + " / " + player.MaxHealth;
    }
}
