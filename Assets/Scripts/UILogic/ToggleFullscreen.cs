using UnityEngine;
using UnityEngine.UI;

public class ToggleFullscreen : MonoBehaviour
{
    private Toggle fullscreenToggle;
    private string fullscreenStatusKey = "fullscreen";

    void Start()
    {
        fullscreenToggle = GetComponent<Toggle>();
        bool isFullscreen = SavesLogic.Get(fullscreenStatusKey, 1) == 1;
        Screen.fullScreen = isFullscreen;

        // Синхронизируем Toggle с текущим состоянием
        fullscreenToggle.isOn = isFullscreen;

        // Подписываемся на изменение
        fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
    }

    public void SetFullscreen(bool value)
    {
        Screen.fullScreen = value;
        SavesLogic.Set(fullscreenStatusKey, value ? 1 : 0);

        Debug.Log($"Fullscreen: {value} | Сохранено: {PlayerPrefs.GetInt(fullscreenStatusKey)}");
    }
}
