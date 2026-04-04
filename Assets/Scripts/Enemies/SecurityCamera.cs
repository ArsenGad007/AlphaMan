using UnityEngine;
using UnityEngine.SceneManagement;

public class SecurityCamera : MonoBehaviour
{
    [SerializeField] private SecurityCameraFOV fieldOfView;
    [SerializeField] private GameOver gameOver;
    private bool isTriggered = false;


    void Update()
    {
        if (fieldOfView != null && !isTriggered)
        {
            OnPlayerDetected();
        }
    }
    private void OnPlayerDetected()
    {
        if (fieldOfView.IsPlayerVisible())
        {
            TriggerGameOver();
        }
    }
    private void TriggerGameOver()
    {
        if (isTriggered) return;
        isTriggered = true;
        Debug.Log("камера");

        if (gameOver != null)
            gameOver.GameOverPanel();

        Time.timeScale = 0f;
    }
}