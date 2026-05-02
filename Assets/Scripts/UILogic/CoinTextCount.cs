using TMPro;
using UnityEngine;

public class CoinTextCount : MonoBehaviour
{
    public static int CoinCount { get; private set; }

    private static TextMeshProUGUI coinCountText;

    private void Awake()
    {
        coinCountText = GetComponent<TextMeshProUGUI>();
    }

    private void Start()
    {
        UpdateCoinCountText(0);
    }

    public static void UpdateCoinCountText(int num)
    {
        CoinCount = num;
        coinCountText.text = CoinCount.ToString();
    }
}
