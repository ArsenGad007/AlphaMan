using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameDeleteSaves : MonoBehaviour
{
    private Button button;

    void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        PlayerPrefs.DeleteAll();
        if (SceneManager.GetActiveScene().name == "LevelSelect")
        {        
            var chooseLevel = FindFirstObjectByType<GameSaveLoad>();
            chooseLevel.UpdateButtons();
        }
        else if (SceneManager.GetActiveScene().name != "StartMenu")
        {
            Time.timeScale = 1;
            SceneTransition.Load("LevelSelect");
        }         
    }   
}
