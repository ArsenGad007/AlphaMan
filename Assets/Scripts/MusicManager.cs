using UnityEngine;

public class Music : MonoBehaviour
{
    private AudioSource music;

    private void Start()
    {
        music = GetComponent<AudioSource>();

        Sounds.Instance.OnImportantSoundStarted += PauseMusic;
        Sounds.Instance.OnImportantSoundEnded += ResumeMusic;
    }

    //private void PauseMusic()
    //{
    //    if (music.isPlaying)
    //        music.Pause();
    //}
    private void PauseMusic()
    {
        music.Pause();
    }

    private void ResumeMusic()
    {
        music.UnPause();
    }
}
