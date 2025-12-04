using UnityEngine;

public class Music : MonoBehaviour
{
    private AudioSource music;

    private void Start()
    {
        music = GetComponent<AudioSource>();

        Sounds.Instance.OnImportantSoundStarted += PauseMusic;
        Sounds.Instance.OnImportantSoundEnded += ResumeMusic;

        GameExit.OnMenuOpened += PauseMusic;
        GameExit.OnMenuClosed += ResumeMusic;
    }

    private void PauseMusic()
    {
        music.Pause();
    }

    private void ResumeMusic()
    {
        music.UnPause();
    }
}
