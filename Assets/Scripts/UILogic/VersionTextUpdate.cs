using TMPro;
using UnityEngine;

public class VersionText : MonoBehaviour
{
    private TextMeshProUGUI text;
    void Start()
    {
        text = GetComponent<TextMeshProUGUI>();
        text.text = $"v{Application.version}";
    }
}