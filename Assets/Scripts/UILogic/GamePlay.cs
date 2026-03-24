using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GamePlay : MonoBehaviour
{
    [SerializeField] private Button playButton;
    [SerializeField] private string nameNextScence = "Level0";
    void Start()
    {
        playButton.onClick.RemoveListener(LoadSceneByName);
        playButton.onClick.AddListener(LoadSceneByName);
    }

    public void LoadSceneByName()
    {
        SceneManager.LoadScene(nameNextScence);
    }
}
