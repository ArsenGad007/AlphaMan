using UnityEngine;
using UnityEngine.SceneManagement;

public class SecurityCamera : MonoBehaviour
{
    [SerializeField] private SecurityCameraFOV fieldOfView;
    [SerializeField] private GameOver gameOver;
    private bool isTriggered = false;
    private Material Green;
    private Material Red;
    private void Awake()
    {
        Green = Resources.Load<Material>("FOV_mat/FOV_Walking");
        Red = Resources.Load<Material>("FOV_mat/FOV_Danger");
    }
    private void Start()
    {
        fieldOfView.SetMaterial(Green);
    }
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


        fieldOfView.SetMaterial(Red);
            gameOver.GameOverPanel();

        Time.timeScale = 0f;
    }

}