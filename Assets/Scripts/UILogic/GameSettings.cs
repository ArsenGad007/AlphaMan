using UnityEngine;
using UnityEngine.UI;

public class GameSettings : MonoBehaviour
{
    [SerializeField] private GameObject gameSettingsPanel;  // —сылка на панель настроек;
    [SerializeField] private Button enterSettingsButton;
    [SerializeField] private Button exitSettingsButton;

    void Start()
    {
        enterSettingsButton.onClick.RemoveListener(EnterSettings);
        enterSettingsButton.onClick.AddListener(EnterSettings);
    }

    private void EnterSettings()
    {
        gameSettingsPanel.SetActive(true);

        exitSettingsButton.onClick.RemoveListener(ExitSettings);
        exitSettingsButton.onClick.AddListener(ExitSettings);
    }

    private void ExitSettings()
    {
        exitSettingsButton.onClick.RemoveListener(ExitSettings);
        gameSettingsPanel.SetActive(false);
    }
}
