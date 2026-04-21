using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GamePlay : MonoBehaviour
{
    [SerializeField] private Button playButton;
    [SerializeField] private string nameNextScene = "LevelSelect";
    void Start()
    {
        playButton.onClick.RemoveListener(LoadSceneByName);
        playButton.onClick.AddListener(LoadSceneByName);
    }

    public void LoadSceneByName()
    {
        //SceneTransition.Load(nameNextScene);
        SceneTransition.dontOpenNextScene = true;
        SceneManager.LoadScene(nameNextScene);
    }
}
