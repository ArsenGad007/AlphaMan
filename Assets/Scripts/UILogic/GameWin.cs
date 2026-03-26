using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameWin : MonoBehaviour
{
    [SerializeField] private GameObject gameWinPanel;  // Ссылка на панель победы;
    [SerializeField] private GameInput gameInput;
    [SerializeField] private Button continueButton;
    [SerializeField] private int nextLevel;

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

        continueButton.onClick.RemoveListener(ContinueGameWin);
        continueButton.onClick.AddListener(ContinueGameWin);
    }

    private void ContinueGameWin()
    {
        PlayerPrefs.SetInt("player_level", nextLevel);
        PlayerPrefs.Save();     // Сохранение результата
        
        gameInput.DisablePlayerInputActions();
        gameWinPanel.SetActive(false);
        Time.timeScale = 1;
        SceneManager.LoadScene("LevelSelect");
    }
}
