using System.Collections.Generic;
using System;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Dropdown))]
public class ResolutionDropdown : MonoBehaviour
{
    private TMP_Dropdown dropdown;
    private Resolution[] resolutions;

    void Start()
    {
        dropdown = GetComponent<TMP_Dropdown>();
        resolutions = Screen.resolutions;

        List<string> options = new List<string>();

        for (int i = resolutions.Length - 1; i >= 0; i--)   // Обратный порядок
            options.Add($"{resolutions[i].width}x{resolutions[i].height} ({(int)Math.Round(resolutions[i].refreshRateRatio.value)} Hz)");

        dropdown.ClearOptions();
        dropdown.AddOptions(options);

        int savedIndex = PlayerPrefs.GetInt("ResolutionIndex", 0);

        dropdown.value = savedIndex;            // Устанавливаем сохранённое значение
        dropdown.RefreshShownValue();

        ChangeResolution(savedIndex);           // Применяем его сразу

        dropdown.onValueChanged.AddListener(ChangeResolution);
    }

    void ChangeResolution(int index)
    {
        int real_index = resolutions.Length - 1 - index;    // Обратный порядок
        Resolution res = resolutions[real_index];

        FullScreenMode fullscreen_mode = (PlayerPrefs.GetInt("Fullscreen", 1) == 1) ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
        Screen.SetResolution(res.width, res.height, fullscreen_mode, res.refreshRateRatio);

        Debug.Log($"Разрешение установлено: {res.width}x{res.height} @ {(int)Math.Round(res.refreshRateRatio.value)} Hz");

        PlayerPrefs.SetInt("ResolutionIndex", index);       // Сохраняем выбор
        PlayerPrefs.Save();
    }
}
