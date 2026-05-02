using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Класс для сохранения в файл
/// </summary>
public class SavesLogic : MonoBehaviour
{
    private readonly static string[] deleteSaves = { "player_level", "coins_total", "upgrade_speed" };

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
    public static int Get(string key, int? default_value = null)
    {
        if(default_value == null)
            return PlayerPrefs.GetInt(key);
        return PlayerPrefs.GetInt(key, default_value.Value);
    }

    /// <summary>
    /// Удаление сохранения уровней
    /// </summary>
    public static void DeleteSaves()
    {
        foreach (var item in deleteSaves)
            PlayerPrefs.DeleteKey(item);
    }
}
