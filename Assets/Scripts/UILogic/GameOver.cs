using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOver : MonoBehaviour
{
    [SerializeField] private GameObject gameOverPanel;  // —сылка на панель проигрыша;
    [SerializeField] private Button restartButton;
    PlayerController playerController;

    private bool isGameOver = false;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        playerController = playerObj.GetComponent<PlayerController>();
        gameOverPanel.SetActive(false);
        Time.timeScale = 1;
    }
    
    /// <summary>
    /// ¬ыводит на экран панель проигрыша
    /// </summary>
    public void GameOverPanel()
    {
        if (isGameOver) return; 
        isGameOver = true;

        Time.timeScale = 0;
        SoundManager.PlayLose();
        playerController.StopAllComponents();//останавливаем перед проигрышем игрока(чтоб никуда не убежал)
        gameOverPanel.SetActive(true);  // ѕоказать панель проигрыша

        restartButton.onClick.RemoveListener(RestartGameOver); 
        restartButton.onClick.AddListener(RestartGameOver);
    }

    private void RestartGameOver()
    {
        Time.timeScale = 1;
        gameOverPanel.SetActive(false);

        // SceneTransition.Load(SceneManager.GetActiveScene().name);
        SceneTransition.Load(SceneManager.GetActiveScene().name, true);
    }
}
