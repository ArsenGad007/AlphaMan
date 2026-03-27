using UnityEngine;
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
        //SceneManager.LoadScene(nameNextScene);
        SceneTransition.Load(nameNextScene);
    }
}
