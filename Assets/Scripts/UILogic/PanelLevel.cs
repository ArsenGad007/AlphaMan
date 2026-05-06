using UnityEngine;
using UnityEngine.UI;

public class PanelLevel : MonoBehaviour
{
    [SerializeField] private Button exitPanelLevel;
    [SerializeField] private Button playPanelLevel;

    void Start()
    {
        exitPanelLevel.onClick.RemoveListener(ExitPanelLevel);
        exitPanelLevel.onClick.AddListener(ExitPanelLevel);

        playPanelLevel.onClick.RemoveListener(PlayPanelLevel);
        playPanelLevel.onClick.AddListener(PlayPanelLevel);
    }

    private void ExitPanelLevel()
    {
        gameObject.SetActive(false);
    }

    private void PlayPanelLevel()
    {
        MenuMusic.Stop();
        SceneTransition.Load(LevelsButtonsDisplay.Instance.selectedPanelName, true);
    }
}
