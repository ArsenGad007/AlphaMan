using UnityEngine;

public class CoinTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        SoundManager.PlayCoinPickup();// звук взятия монеты
        gameObject.SetActive(false);
        CoinTextCount.UpdateCoinCountText(CoinTextCount.CoinCount + 1);
    }
}
