using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameWin : MonoBehaviour
{
    [SerializeField] private GameObject gameWinPanel;  // Ссылка на панель победы;
    [SerializeField] private GameInput gameInput;
    [SerializeField] private Button restartButton;

    void Start()
    {
        gameWinPanel.SetActive(false);
    }

    public void GameWinPanel()
    {
        // Остановка игрового процесса
        Time.timeScale = 0;

        // Воспроизведение звука выигрыша
        Sounds.Instance.PlayWin();

        // Показать панель выйгрыша
        gameWinPanel.SetActive(true);

        restartButton.onClick.RemoveListener(RestartGameWin); 
        restartButton.onClick.AddListener(RestartGameWin);
    }

    private void RestartGameWin()
    {
        gameInput.DisablePlayerInputActions();
        gameWinPanel.SetActive(false);
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
