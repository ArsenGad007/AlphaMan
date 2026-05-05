using TMPro;
using UnityEngine;

public class CountItems : MonoBehaviour
{
    public static int itemCount { get; private set; }

    private static TextMeshProUGUI itemCountText;

    private void Awake()
    {
        itemCountText = GetComponent<TextMeshProUGUI>();
    }

    public static void UpdateItemCountPanel(int num)
    {
        itemCount = num;
        itemCountText.text = itemCount.ToString();
    }
}
