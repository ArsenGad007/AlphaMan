using UnityEngine;
using UnityEngine.UI;

public class Fullscreen : MonoBehaviour
{
    private Toggle fullscreenToggle;

    void Start()
    {
        fullscreenToggle = GetComponent<Toggle>();
        bool isFullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
        Screen.fullScreen = isFullscreen;

        // Синхронизируем Toggle с текущим состоянием
        fullscreenToggle.isOn = isFullscreen;

        // Подписываемся на изменение
        fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
    }

    public void SetFullscreen(bool value)
    {
        Screen.fullScreen = value;
        PlayerPrefs.SetInt("Fullscreen", value ? 1 : 0);
        PlayerPrefs.Save();

        Debug.Log($"Fullscreen: {value} | Сохранено: {PlayerPrefs.GetInt("Fullscreen")}");
    }

    void OnDestroy()
    {
        fullscreenToggle.onValueChanged.RemoveListener(SetFullscreen);
    }
}
