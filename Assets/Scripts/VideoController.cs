using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoController : MonoBehaviour
{
    [SerializeField] private GameObject panelDeletePastSaves;
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;

    private VideoPlayer videoPlayer;
    private bool checkVersion;

    void Awake()
    {
        videoPlayer = GetComponent<VideoPlayer>();
    }

    void Start()
    {
        panelDeletePastSaves.SetActive(false);
        checkVersion = SavesLogic.Get("version", "") != Application.version;

        if (checkVersion && SavesLogic.Get("player_level", 0) != 0)
        {
            yesButton.onClick.RemoveListener(PlayVideo);
            yesButton.onClick.AddListener(PlayVideo);

            noButton.onClick.RemoveListener(LoadStartScene);
            noButton.onClick.AddListener(LoadStartScene);

            panelDeletePastSaves.SetActive(true);
        }
        else if (checkVersion)
            PlayVideo();
        else
            LoadStartScene();
    }

    void OnDestroy()
    {
        videoPlayer.loopPointReached -= OnVideoFinished;
    }

    private void PlayVideo()
    {
        videoPlayer.loopPointReached += OnVideoFinished;
        videoPlayer.Play();
    }

    private void OnVideoFinished(VideoPlayer vp) => LoadStartScene();

    private void LoadStartScene()
    {
        if (checkVersion)
            SavesLogic.Set("version", Application.version);

        SceneTransition.Load("StartMenu");
    }
}
