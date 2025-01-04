using TMPro;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.UI;

public class DayCycle : MonoBehaviour
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

    void Start()
    {
   
        timeOfDay = startHour / 24f;
    }
    void Update()
    {
        float currentHour = timeOfDay * 24;

        // Adjust time speed based on the time of day
        float timeSpeedMultiplier = (currentHour >= dayStartHour && currentHour < nightStartHour)
            ? daySpeedMultiplier
            : nightSpeedMultiplier;

        // Increment time based on the multiplier
        timeOfDay += Time.deltaTime / dayDuration * timeSpeedMultiplier;

        // Update the light's rotation
        float sunAngle = Mathf.Lerp(-90f, 270f, timeOfDay); // Map 0-1 time to -90° (sunrise) to 270° (next sunrise)
        lightTransform.rotation = Quaternion.Euler(sunAngle, 0f, 0f);

        // Use timeOfDay to drive light intensity and color
        directionalLight.intensity = lightIntensityCurve.Evaluate(timeOfDay);
        directionalLight.color = lightColorGradient.Evaluate(timeOfDay);

        UpdateTimeDisplay();
    }

    void UpdateTimeDisplay()
    {
        // Calculate hours and minutes
        int hours = Mathf.FloorToInt(timeOfDay * 24); // Total hours in a day
        int minutes = Mathf.FloorToInt((timeOfDay * 1440) % 60); // Total minutes in a day

        // Format time as HH:MM
        string timeString = string.Format("{0:00}:{1:00}", hours, minutes);

        // Update the UI text
        timeText.text = timeString;
    }
}