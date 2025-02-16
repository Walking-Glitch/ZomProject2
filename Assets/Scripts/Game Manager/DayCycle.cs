using Assets.Scripts.Game_Manager;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.UI;

public class DayCycle : NetworkBehaviour
{


    public float dayDuration = 24f;  
    [SerializeField]private float timeOfDay;  
    public AnimationCurve lightIntensityCurve;  
    public Gradient lightColorGradient;  
    public Light directionalLight; // Reference to the directional light
    public Transform lightTransform;  

    public TextMeshProUGUI timeText;


    public float daySpeedMultiplier = 2f; // Speed multiplier for daytime
    public float nightSpeedMultiplier = 0.5f; // Speed multiplier for nighttime
    public float dayStartHour = 6f; // Start of daytime (6 AM)
    public float nightStartHour = 19f; // Start of nighttime (6 PM)

    public float startHour = 8f;


    public delegate void NightTimeChanged(bool isNight);
    public static event NightTimeChanged OnNightTimeChanged;
    public bool IsNightTime { get; private set; }

    //public int currentDay = 1;  
    private bool hasPassed6AM = false; // 

    private GameManager gameManager;

    private NetworkVariable<float> networkTimeOfDay = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    void Start()
    {
        gameManager = GameManager.Instance;
        timeOfDay = startHour / 24f;

        if(IsServer)
        {
            networkTimeOfDay.Value = timeOfDay;
        }
    }
    void Update()
    {
        if (IsServer)
        {
            UpdateTimeOnServer();
        }

        ApplyTimeLocally(networkTimeOfDay.Value);
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            networkTimeOfDay.Value = startHour / 24f; 
        }
    }

    private void UpdateTimeOnServer()
    {
        float currentHour = timeOfDay * 24;

        // Adjust time speed based on the time of day
        float timeSpeedMultiplier = (currentHour >= dayStartHour && currentHour < nightStartHour)
            ? daySpeedMultiplier
            : nightSpeedMultiplier;

        bool newNightTimeState = DetermineIfNightTime(currentHour);

        if (newNightTimeState != IsNightTime)
        {
            IsNightTime = newNightTimeState;
            OnNightTimeChanged?.Invoke(IsNightTime);
        }

        // Check for day cycle completion (crossing 6 AM)
        if (timeOfDay >= 0.25f && !hasPassed6AM)
        {
            hasPassed6AM = true; // Prevent multiple increments
            gameManager.DifficultyManager.IncreaseLevel();

        }

        if (timeOfDay < 0.25f)
        {
            hasPassed6AM = false;
        }

        // Increment time based on the multiplier
        timeOfDay += Time.deltaTime / dayDuration * timeSpeedMultiplier;

        if (timeOfDay >= 1f)
        {
            timeOfDay -= 1f;
        }

        networkTimeOfDay.Value = timeOfDay;
      
    }

    private void ApplyTimeLocally(float syncedTime)
    {
        // Update the light's rotation
        float sunAngle = Mathf.Lerp(-90f, 270f, syncedTime); // Map 0-1 time to -90° (sunrise) to 270° (next sunrise)
        lightTransform.rotation = Quaternion.Euler(sunAngle, 0f, 0f);

        // Use timeOfDay to drive light intensity and color
        directionalLight.intensity = lightIntensityCurve.Evaluate(syncedTime);
        directionalLight.color = lightColorGradient.Evaluate(syncedTime);

        UpdateTimeDisplay(syncedTime);
    }

    void UpdateTimeDisplay(float syncedTime)
    {
        // Calculate hours and minutes
        int hours = Mathf.FloorToInt(syncedTime * 24); // Total hours in a day
        int minutes = Mathf.FloorToInt((syncedTime * 1440) % 60); // Total minutes in a day

        // Format time as HH:MM
        string timeString = string.Format("{0:00}:{1:00}", hours, minutes);

        // Update the UI text
        timeText.text = timeString;
    }

    private bool DetermineIfNightTime(float currentHour)
    {
        return currentHour < dayStartHour || currentHour >= nightStartHour;
    }
}