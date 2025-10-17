using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameTimer : MonoBehaviour
{
    [Tooltip("Second")]
    public float totalTime = 60f;

    private float currentTime;
    private bool isRunning = true;

    [Tooltip("UI")]
    public TextMeshProUGUI timerText;

    public float CurrentTime
    {
        get { return currentTime; }
    }

    void Start()
    {
        currentTime = totalTime;
        UpdateTimerDisplay();
    }

    void Update()
    {
        if (!isRunning) return;

        currentTime -= Time.deltaTime;

        if (currentTime <= 0f)
        {
            currentTime = 0f;
            isRunning = false;
            TimerEnded();
        }

        UpdateTimerDisplay();
    }

    void UpdateTimerDisplay()
    {
        int minutes = Mathf.FloorToInt(currentTime / 60f);
        int seconds = Mathf.FloorToInt(currentTime % 60f);
        timerText.text = $"{minutes:D2}:{seconds:D2}";
    }

    void TimerEnded()
    {
        Debug.Log("Time out!");
        // Здесь можно завершить игру, показать окно и т.д.
    }
}
