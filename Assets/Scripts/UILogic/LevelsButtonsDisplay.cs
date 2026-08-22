using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class ButtonStringPair
{
    public Button button;
    public string sceneName;   
    public GameObject panelLevel;   
}

public class LevelsButtonsDisplay : Singleton<LevelsButtonsDisplay>
{
    [SerializeField] private List<ButtonStringPair> buttonPairs;

    private string playerLevelKey = "player_level";
    public string selectedPanelName { get; private set; }

    void Start()
    {
        if(!MenuMusic.IsPlay())
            MenuMusic.Play();

        foreach (var pair in buttonPairs)
            if (pair.button != null)
                pair.button.onClick.AddListener(() => ShowPanelLevel(pair));
        UpdateLevelButtons();
    }

    private void ShowPanelLevel(ButtonStringPair bsp)
    {
        selectedPanelName = bsp.sceneName;
        bsp.panelLevel.SetActive(true);
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
}
