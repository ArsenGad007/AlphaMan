using System;
using UnityEngine;
using UnityEngine.UI;

public class GameSettings : MonoBehaviour
{
    [SerializeField] private GameObject gameSettingsPanel;  // Ссылка на панель настроек;
    [SerializeField] private Button enterSettingsButton;
    [SerializeField] private Button exitSettingsButton;

    public static event Action OnMenuOpened;
    public static event Action OnMenuClosed;

    void Start()
    {
        gameSettingsPanel.SetActive(false);

        enterSettingsButton.onClick.RemoveListener(EnterSettings);
        enterSettingsButton.onClick.AddListener(EnterSettings);
    }

    private void EnterSettings()
    {
        Time.timeScale = 0;     // Остановка игрового процесса
        OnMenuOpened?.Invoke(); // Поставить фоновую музыку на паузу

        gameSettingsPanel.SetActive(true);

        exitSettingsButton.onClick.RemoveListener(ExitSettings);
        exitSettingsButton.onClick.AddListener(ExitSettings);
    }

    private void ExitSettings()
    {
        exitSettingsButton.onClick.RemoveListener(ExitSettings);
        gameSettingsPanel.SetActive(false);
        
        Time.timeScale = 1;     // Продолжение игрового процесса
        OnMenuClosed?.Invoke(); // Снять с паузы музыку
    }
}
