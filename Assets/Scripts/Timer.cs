using TMPro;
using UnityEngine;

public class Timer : MonoBehaviour
{
    [SerializeField] private int gameTimeSec = 60;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private GameOver gameOver;

    private float timeRemaining;
    private bool isGameOver = false;

    void Start()
    {
        timeRemaining = gameTimeSec;
        UpdateTimerDisplay();
    }

    void Update()
    {
        if (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
            UpdateTimerDisplay();
        }
        else if (!isGameOver)
        {
            isGameOver = true;
            gameOver.GameOverPanel();
        }        
    }

    private void UpdateTimerDisplay()
    {
        if (timeRemaining < 0)
            timeRemaining = 0;

        // Форматирование времени в минуты и секунды
        int minutes = Mathf.FloorToInt(timeRemaining / 60);
        int seconds = Mathf.FloorToInt(timeRemaining % 60);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}
