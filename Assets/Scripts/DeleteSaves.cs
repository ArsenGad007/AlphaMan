using UnityEngine;

public class DeleteSaves : MonoBehaviour
{
    void Start()
    {
        PlayerPrefs.DeleteAll();
    }
}
