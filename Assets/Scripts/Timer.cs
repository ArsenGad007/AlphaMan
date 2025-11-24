using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    [SerializeField] private int gameTimeSec = 60;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private GameObject gameOverPanel;  // Ссылка на панель проигрыша;
    [SerializeField] private Button restartButton;
    [SerializeField] private GameInput gameInput;

    private float timeRemaining;
    private bool isGameOver = false;

    void Start()
    {
        timeRemaining = gameTimeSec;
        gameOverPanel.SetActive(false);
        restartButton.onClick.AddListener(RestartGame);
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
            GameOver();
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

    private void GameOver()
    {
        // Остановка игрового процесса
        Time.timeScale = 0;

        // Показать панель проигрыша
        gameOverPanel.SetActive(true);

        // Разблокировать курсор (если была блокировка)
        Cursor.lockState = CursorLockMode.None;
    }

    private void RestartGame()
    {
        gameInput.DisablePlayerInputActions();
        gameOverPanel.SetActive(false);
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);   
    }
}
