using UnityEngine;

public class ToggleMuzzle : MonoBehaviour
{
    [SerializeField] private float elapsed;
    [SerializeField] private float timeToOff;
    void Start()
    {
        
    }

    private void OnDisable()
    {
        elapsed = 0;
    }

    // Update is called once per frame
    void Update()
    {
        elapsed += Time.deltaTime;

        if(elapsed > timeToOff)
        {
            gameObject.SetActive(false);
        }
    }
}
