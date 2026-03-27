using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[CustomEditor(typeof(TMP_Dropdown), true)]
public class ResolutionDropdown : MonoBehaviour
{
    private TMP_Dropdown dropdown;
    private Resolution[] resolutions;

    void Start()
    {
        dropdown = GetComponent<TMP_Dropdown>();
        resolutions = Screen.resolutions;

        dropdown.ClearOptions();

        List<string> options = new List<string>();

        int savedIndex = PlayerPrefs.GetInt("ResolutionIndex", 0);

        for (int i = resolutions.Length - 1; i >= 0; i--)   // Обратный порядок
            options.Add(resolutions[i].width + "x" + resolutions[i].height);

        dropdown.AddOptions(options);

        // Устанавливаем сохранённое значение
        dropdown.value = savedIndex;
        dropdown.RefreshShownValue();

        // Применяем его сразу
        ChangeResolution(savedIndex);

        dropdown.onValueChanged.AddListener(ChangeResolution);
    }

    void ChangeResolution(int index)
    {
        int real_index = resolutions.Length - 1 - index;    // Обратный порядок
        Resolution res = resolutions[real_index];   
        Screen.SetResolution(res.width, res.height, PlayerPrefs.GetInt("Fullscreen", 1) == 1);

        Debug.Log($"Изменено разрешение: {res.width}x{res.height}");
        // Сохраняем выбор
        PlayerPrefs.SetInt("ResolutionIndex", index);
        PlayerPrefs.Save();
    }
}
