using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ButtonDeleteSaves : MonoBehaviour
{
    private Button button;

    private string startMenu = "StartMenu";
    private string exitLevel = "LevelSelect";

    void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        SavesLogic.DeleteLevelSaves();
        if (SceneManager.GetActiveScene().name == exitLevel)
        {        
            var chooseLevel = FindFirstObjectByType<LevelsButtonsDisplay>();
            chooseLevel.UpdateLevelButtons();
        }
        else if (SceneManager.GetActiveScene().name != startMenu)
        {
            Time.timeScale = 1;
            SceneTransition.Load(exitLevel);
        }         
    }   
}
