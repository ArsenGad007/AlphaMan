using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MenuMusic : Singleton<MenuMusic>
{
    private AudioSource musicSource;

    protected override bool IsDestroyOnLoad => false;

    protected override void Awake()
    {
        base.Awake();
        musicSource = GetComponent<AudioSource>();
    }

    public static void Play() => Instance.musicSource.Play();
    public static void Stop() => Instance.musicSource.Stop();
    public static bool IsPlay() => Instance.musicSource.isPlaying;
}
