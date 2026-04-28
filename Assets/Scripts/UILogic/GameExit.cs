using System;
using UnityEngine;
using UnityEngine.UI;

public enum GameExitPlace
{
    Menu,
    Output
}

/// <summary>
/// Отвечает за отображение выхода из игры
/// </summary>
public class GameExit : MonoBehaviour
{
    [SerializeField] private GameInput gameInput;
    [SerializeField] private GameExitPlace gameExitPlace;   // Куда нужно выйти;
    [SerializeField] private GameObject gameExitPanel;      // Ссылка на панель выхода;
    [SerializeField] private Button exitButton;
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;

    public static event Action OnMenuOpened;
    public static event Action OnMenuClosed;

    private string exitLevel = "LevelSelect";

    void Start()
    {
        gameExitPanel.SetActive(false);

        exitButton.onClick.RemoveListener(ExitPanel);
        exitButton.onClick.AddListener(ExitPanel);

        GameInput.IsExit -= ExitPanel;
        GameInput.IsExit += ExitPanel;
    }

    private void ExitPanel()
    {
        Time.timeScale = 0;      
        OnMenuOpened?.Invoke();     // Поставить фоновую музыку на паузу
        gameExitPanel.SetActive(true);

        yesButton.onClick.RemoveListener(YesButton);
        yesButton.onClick.AddListener(YesButton);

        noButton.onClick.RemoveListener(NoButton);
        noButton.onClick.AddListener(NoButton);
    }

    private void YesButton()
    {
        if(gameExitPlace == GameExitPlace.Menu)
        {
            Time.timeScale = 1;
            gameExitPanel.SetActive(false);
            SceneTransition.Load(exitLevel);
            return;
        }

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
        Time.timeScale = 1;
        gameExitPanel.SetActive(false);
        OnMenuClosed?.Invoke();         // Снять с паузы музыку
    }
}
