using System;
using UnityEngine;
using UnityEngine.UI;

public class GameExit : MonoBehaviour
{
    [SerializeField] private GameObject gameExitPanel;  // Ссылка на панель выхода;
    [SerializeField] private Button exitButton;
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;

    public static event Action OnMenuOpened;
    public static event Action OnMenuClosed;

    void Start()
    {
        gameExitPanel.SetActive(false);

        exitButton.onClick.RemoveListener(ExitPanel);
        exitButton.onClick.AddListener(ExitPanel);
    }

    private void ExitPanel()
    {
        // Остановка игрового процесса
        Time.timeScale = 0;

        // Показать панель выхода
        gameExitPanel.SetActive(true);

        // Поставить фоновую музыку на паузу
        OnMenuOpened?.Invoke();

        yesButton.onClick.RemoveListener(YesButton);
        yesButton.onClick.AddListener(YesButton);

        noButton.onClick.RemoveListener(NoButton);
        noButton.onClick.AddListener(NoButton);
    }

    private void YesButton()
    {
        Debug.Log("Выход из игры");

        // Для редактора Unity
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #endif

        // Для собранного приложения
        Application.Quit();
    }

    private void NoButton()
    {
        // Продолжение игрового процесса
        Time.timeScale = 1;

        // Убрать панель выхода
        gameExitPanel.SetActive(false);

        OnMenuClosed?.Invoke(); // Снять с паузы музыку
    }
}
