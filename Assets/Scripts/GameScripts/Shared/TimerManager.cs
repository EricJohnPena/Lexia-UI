using UnityEngine;
using UnityEngine.UI;

public class TimerManager : MonoBehaviour
{
    public Text timerText; // Assign the Text UI element in the Inspector
    public float elapsedTime;
    private bool isRunning;

    void Update()
    {
        if (isRunning)
        {
            elapsedTime += Time.deltaTime;
            int minutes = Mathf.FloorToInt(elapsedTime / 60);
            int seconds = Mathf.FloorToInt(elapsedTime % 60);

            if (timerText != null)
            {
                timerText.text = $"{minutes:00}:{seconds:00}";
            }
            else
            {
                Debug.LogWarning("Timer Text UI is not assigned!");
            }
        }
    }

    public void StartTimer()
    {
        elapsedTime = 0f;
        isRunning = true;

        if (timerText != null)
        {
            timerText.text = "00:00"; // Reset the timer text
            Debug.Log("Timer started!"); // Debug log for timer start
        }
        else
        {
            Debug.LogWarning("Timer Text UI is not assigned!");
        }
    }

    public void StopTimer()
    {
        isRunning = false;
    }

    public void ResetTimer()
    {
        elapsedTime = 0f;
        if (timerText != null)
        {
            timerText.text = "00:00";
        }
        else
        {
            Debug.LogWarning("Timer Text UI is not assigned!");
        }
    }
}
