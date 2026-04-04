using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOver : MonoBehaviour
{
    [SerializeField] private GameObject gameOverPanel;  // Ссылка на панель проигрыша;
    [SerializeField] private Button restartButton;

    private bool isGameOver = false;

    void Start()
    {
        gameOverPanel.SetActive(false);
        Time.timeScale = 1;
    }
    
    /// <summary>
    /// Выводит на экран панель проигрыша
    /// </summary>
    public void GameOverPanel()
    {
        if (isGameOver) return; 
        isGameOver = true;

        Time.timeScale = 0;     
        Sounds.Instance.PlayLose();        
        gameOverPanel.SetActive(true);  // Показать панель проигрыша

        restartButton.onClick.RemoveListener(RestartGameOver); 
        restartButton.onClick.AddListener(RestartGameOver);
    }

    private void RestartGameOver()
    {
        Time.timeScale = 1;
        gameOverPanel.SetActive(false);

        SceneTransition.Load(SceneManager.GetActiveScene().name);
    }
}
