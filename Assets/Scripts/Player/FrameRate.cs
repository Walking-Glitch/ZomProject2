using UnityEngine;
using UnityEngine.UI;
public class FrameRate : MonoBehaviour
{
    
    [SerializeField] private Text fpsText; // Optional if using Unity UI Text
    [SerializeField] private TMPro.TextMeshProUGUI fpsTextTMP; // Optional if using TextMeshPro
    private float deltaTime;

    void Update()
    {
        // Calculate deltaTime (time between frames)
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;

        // Calculate FPS
        float fps = 1.0f / deltaTime;

        // Update the UI text
        if (fpsText != null)
        {
            fpsText.text = $"FPS: {fps:0.}";
        }
        if (fpsTextTMP != null)
        {
            fpsTextTMP.text = $"FPS: {fps:0.}";
        }
    }
}

