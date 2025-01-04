using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

public class DayCycle : MonoBehaviour
{


    public float dayDuration = 60f; // Duration of a full day in seconds
    [SerializeField]private float timeOfDay; // Current time of day
    public AnimationCurve lightIntensityCurve; // Intensity over time
    public Gradient lightColorGradient; // Color over time
    public Light directionalLight; // Reference to the directional light
    public Transform lightTransform; // Reference to the light's transform (optional)

    void Update()
    {
        // Increment time
        timeOfDay += Time.deltaTime / dayDuration;

        // Loop back to 0 after a full cycle
        timeOfDay %= 1f;

        // Update the light's rotation
        float sunAngle = Mathf.Lerp(-90f, 270f, timeOfDay); // Map 0-1 time to -90° (sunrise) to 270° (next sunrise)
        lightTransform.rotation = Quaternion.Euler(sunAngle, 0f, 0f);

        // Use timeOfDay to drive light intensity and color
        directionalLight.intensity = lightIntensityCurve.Evaluate(timeOfDay);
        directionalLight.color = lightColorGradient.Evaluate(timeOfDay);
    }
}