using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Класс для сохранения в файл
/// </summary>
public class SavesLogic : MonoBehaviour
{

    private static string[] settingKeys = { "fullscreen", "resolution" };

    /// <summary>
    /// Сохранение ключа - значения
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public static void Set(string key, int value)
    {
        PlayerPrefs.SetInt(key, value);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Получение значения по ключу
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public static int Get(string key, int default_value = int.MinValue)
    {
        if(default_value == int.MinValue)
            return PlayerPrefs.GetInt(key);
        return PlayerPrefs.GetInt(key, default_value);
    }

    /// <summary>
    /// Удаление сохранения уровней
    /// </summary>
    public static void DeleteLevelSaves()
    {
        List<int> settings_values = new();

        foreach (string key in settingKeys)
            settings_values.Add(Get(key));
        DeleteAllSaves();

        for (int i = 0; i < settings_values.Count; i++)
            Set(settingKeys[i], settings_values[i]);
    }

    /// <summary>
    /// Удаление всех сохранений
    /// </summary>
    public static void DeleteAllSaves()
    {
        PlayerPrefs.DeleteAll();
    }
}
