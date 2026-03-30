using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class FindMissingScriptsTool : EditorWindow
{
    [MenuItem("Tools/Найти Missing Scripts")]
    public static void ShowWindow()
    {
        GetWindow<FindMissingScriptsTool>("Найти Missing Scripts");
    }

    void OnGUI()
    {
        if (GUILayout.Button("Найти Missing Scripts в текущей сцене"))
            FindMissingInScene();

        if (GUILayout.Button("Найти Missing Scripts во всех префабах"))
            FindMissingInPrefabs();
    }

    private static void FindMissingInScene()
    {
        int missingCount = 0;
        // Ищем все объекты в сцене, включая неактивные
        foreach (GameObject go in FindObjectsByType<GameObject>(FindObjectsSortMode.None))
            missingCount += FindInGO(go);

        Debug.Log($"Поиск завершен. Найдено missing скриптов: {missingCount}");
    }

    private static void FindMissingInPrefabs()
    {
        int missingCount = 0;
        // Ищем все префабы в проекте
        string[] guids = AssetDatabase.FindAssets("t:Prefab");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null)
                missingCount += FindInGO(prefab);
        }

        Debug.Log($"Поиск префабов завершен. Найдено missing скриптов: {missingCount}");
    }

    private static int FindInGO(GameObject go)
    {
        int missingCount = 0;
        Component[] components = go.GetComponents<Component>();
        for (int i = 0; i < components.Length; i++)
            if (components[i] == null)
            {
                missingCount++;
                string fullPath = GetFullPath(go);
                Debug.Log($"Найден Missing Script на объекте: {fullPath}", go);
            }

        return missingCount;
    }

    private static string GetFullPath(GameObject go)
    {
        string path = go.name;
        Transform current = go.transform;
        while (current.parent != null)
        {
            current = current.parent;
            path = current.name + "/" + path;
        }

        return path;
    }
}