using UnityEngine;
using UnityEngine.UI;

public class GameWin : MonoBehaviour
{
    [SerializeField] private GameObject gameWinPanel;  // —сылка на панель победы;
    [SerializeField] private Button continueButton;
    [SerializeField] private int numLevel;

    private string playerLevelKey = "player_level";
    private string winExitLevel = "LevelSelect";

    void Start()
    {
        gameWinPanel.SetActive(false);
    }

    public void GameWinPanel()
    {
        Time.timeScale = 0;
        SoundManager.PlayWin();
        gameWinPanel.SetActive(true);

        if (SavesLogic.Get(playerLevelKey, 0) <= numLevel)
        {
            SavesLogic.Set(playerLevelKey, numLevel + 1);
            SavesLogic.Set("coins_total", SavesLogic.Get("coins_total", 0) + CoinTextCount.CoinCount);
        }
            
        continueButton.onClick.RemoveListener(ContinueGameWin);
        continueButton.onClick.AddListener(ContinueGameWin);
    }

    private void ContinueGameWin()
    {
        Time.timeScale = 1;
        gameWinPanel.SetActive(false);

        SceneTransition.Load(winExitLevel); 
    }
}
