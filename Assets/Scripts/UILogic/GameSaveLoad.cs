using UnityEngine;
using UnityEngine.UI;

public class GameSaveLoad : MonoBehaviour
{

    [SerializeField] private GameObject[] levelbuttons;
    public static int LoadScore() => PlayerPrefs.GetInt("player_level", 0);

    void Start()
    {
        int level = LoadScore();

        for (int i = 0; i < levelbuttons.Length; i++)
            levelbuttons[i].SetActive(i <= level);
    }
}
