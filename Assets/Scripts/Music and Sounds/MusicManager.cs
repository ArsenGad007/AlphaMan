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

        GameSettings.OnMenuOpened += PauseMusic;
        GameSettings.OnMenuClosed += ResumeMusic;
    }

    private void PauseMusic()
    {
        if (music == null) return;  // защита от ошибки
        music.Pause();
    }

    private void ResumeMusic()
    {
        if (music == null) return;  // защита от ошибки
        music.UnPause();
    }
}
