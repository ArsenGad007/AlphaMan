using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class ButtonStringPair
{
    public Button button;
    public string sceneName;   
}

public class LevelsButtonsDisplay : MonoBehaviour
{
    [SerializeField] private List<ButtonStringPair> buttonPairs;

    private string playerLevelKey = "player_level";

    void Start()
    {
        foreach (var pair in buttonPairs)
            if (pair.button != null)
                pair.button.onClick.AddListener(() => LoadSceneByName(pair.sceneName));
        UpdateLevelButtons();
    }

    /// <summary>
    /// Обновление отображения кнопок уровней
    /// </summary>
    public void UpdateLevelButtons()
    {
        int level = SavesLogic.Get(playerLevelKey, 0);

        for (int i = 0; i < buttonPairs.Count; i++)
            if (buttonPairs[i].button != null)
                buttonPairs[i].button.gameObject.SetActive(i <= level);
    }

    public void LoadSceneByName(string name)
    {
        SceneTransition.Load(name);
    }
}
