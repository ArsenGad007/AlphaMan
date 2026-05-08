using System.Collections;
using System.Linq;
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
            panelDeletePastSaves.SetActive(true);

            yesButton.onClick.RemoveListener(YesButton);
            yesButton.onClick.AddListener(YesButton);

            noButton.onClick.RemoveListener(SaveVersion);
            noButton.onClick.AddListener(SaveVersion);        
        }
        else if (checkVersion)
            PlayVideo();
        else
            StartCoroutine(LoadStartScene());
    }

    void OnDestroy()
    {
        videoPlayer.loopPointReached -= OnVideoFinished;
    }

    private void YesButton()
    {
        panelDeletePastSaves.SetActive(false);
        Debug.Log("YesButton");
        SavesLogic.DeleteSaves();
        Resolution standart_res = Screen.resolutions.Last();
        Screen.SetResolution(standart_res.width, standart_res.height, FullScreenMode.FullScreenWindow);
        PlayVideo();
    }

    private void PlayVideo()
    {
        videoPlayer.loopPointReached += OnVideoFinished;
        videoPlayer.Play();
    }

    private void OnVideoFinished(VideoPlayer vp) => SaveVersion();

    private void SaveVersion()
    {
        panelDeletePastSaves.SetActive(false);

        if (checkVersion)
            SavesLogic.Set("version", Application.version);

        StartCoroutine(LoadStartScene());
    }

    IEnumerator LoadStartScene()
    {
        if (SceneTransition.IsTransitionGo)
            yield return null;

        SceneTransition.Load("StartMenu");
    }
}
