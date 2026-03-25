using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[System.Serializable]
public class ButtonStringPair
{
    public Button button;   
    public string sceneName;    // имя сцены
}

public class GameChooseLevel : MonoBehaviour
{
    [SerializeField] private List<ButtonStringPair> buttonPairs;
    void Start()
    {
        foreach (var pair in buttonPairs)
            if (pair.button != null)
                pair.button.onClick.AddListener(() => LoadSceneByName(pair.sceneName));
    }

    public void LoadSceneByName(string name)
    {
        SceneManager.LoadScene(name);
    }
}
