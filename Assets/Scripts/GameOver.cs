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
    }
    
    public void GameOverPanel()
    {
        // Остановка игрового процесса
        Time.timeScale = 0;

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
}
