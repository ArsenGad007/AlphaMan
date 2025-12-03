using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOver : MonoBehaviour
{
    [SerializeField] private GameObject gameOverPanel;  // Ссылка на панель проигрыша;
    [SerializeField] private GameInput gameInput;
    [SerializeField] private Button restartButton;

    void Start()
    {
        gameOverPanel.SetActive(false);
        restartButton.onClick.RemoveListener(RestartGameOver); // Удаляем если был
        restartButton.onClick.AddListener(RestartGameOver);
    }
    
    public void GameOverPanel()
    {
        // Остановка игрового процесса
        Time.timeScale = 0;
        
        // Воспроизведение звука проигрыша
        Sounds.Instance.PlayLose();

        // Показать панель проигрыша
        gameOverPanel.SetActive(true);

        // Разблокировать курсор (если была блокировка)
        Cursor.lockState = CursorLockMode.None;
    }

    private void RestartGameOver()
    {
        gameInput.DisablePlayerInputActions();
        gameOverPanel.SetActive(false);
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
