using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOver : MonoBehaviour
{
    [SerializeField] private GameObject gameOverPanel;  // Ссылка на панель проигрыша;
    [SerializeField] private GameInput gameInput;
    [SerializeField] private Button restartButton;
    private bool isGameOver = false;

    void Start()
    {
        gameOverPanel.SetActive(false);
        Time.timeScale = 1;
    }
    
    public void GameOverPanel()
    {
        if (isGameOver) return; 
        isGameOver = true;
        // Остановка игрового процесса
        gameOverPanel.SetActive(false);
        Time.timeScale = 0;

        // Воспроизведение звука проигрыша
        Sounds.Instance.PlayLose();

        // Показать панель проигрыша
        gameOverPanel.SetActive(true);

        restartButton.onClick.RemoveListener(RestartGameOver); 
        restartButton.onClick.AddListener(RestartGameOver);
    }

    private void RestartGameOver()
    {
        gameInput.DisablePlayerInputActions();
        gameOverPanel.SetActive(false);
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    private void OnDestroy()
    {
        if (restartButton != null)
            restartButton.onClick.RemoveListener(RestartGameOver);
    }
}
