using System.Collections;
using UnityEngine;

/// <summary>
/// Единый менеджер звука: фоновая музыка + все SFX.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class SoundManager : MonoBehaviour
{
    private static SoundManager instance;

    [Header("Фоновая музыка")]
    [SerializeField] private AudioSource musicSource; 

    [Header("Шаги")]
    [SerializeField] private AudioClip[] walkSteps;
    [SerializeField] private AudioClip[] runSteps;
    [Range(0f, 1f)][SerializeField] private float stepVolume = 1f;

    [Tooltip("Фиксированный интервал между шагами в секундах")]
    [SerializeField] private float walkStepInterval = 0.4f;
    [SerializeField] private float runStepInterval = 0.25f;

    [Header("Подбор предметов")]
    [SerializeField] private AudioClip pickupSound;
    [Range(0f, 1f)][SerializeField] private float pickupVolume = 1f;

    [Header("Победа / Поражение")]
    [SerializeField] private AudioClip winSound;
    [SerializeField] private AudioClip loseSound;
    [Range(0f, 1f)][SerializeField] private float importantVolume = 1f;

    private AudioSource sfxSource;
    private float lastStepTime = 0f;

    private void Awake()
    {
        instance = this;
        sfxSource = GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        GameExit.OnMenuOpened += PauseMusic;
        GameExit.OnMenuClosed += ResumeMusic;
        GameSettings.OnMenuOpened += PauseMusic;
        GameSettings.OnMenuClosed += ResumeMusic;
    }

    private void OnDisable()
    {
        GameExit.OnMenuOpened -= PauseMusic;
        GameExit.OnMenuClosed -= ResumeMusic;
        GameSettings.OnMenuOpened -= PauseMusic;
        GameSettings.OnMenuClosed -= ResumeMusic;
    }

    private void PauseMusic() => musicSource?.Pause();
    private void ResumeMusic() => musicSource?.UnPause();

    /// <summary>
    /// Воспроизводит случайный шаг из массива.
    /// Новый шаг разрешён только через интервал
    /// </summary>
    private void PlayRandomStep(AudioClip[] clips, float interval)
    {
        if (clips == null || clips.Length == 0) return;
        if (Time.time - lastStepTime < interval) return;

        lastStepTime = Time.time;
        AudioClip clip = clips[Random.Range(0, clips.Length)];
        PlaySFX(clip, stepVolume);
    }

    private void PlaySFX(AudioClip clip, float volume)
    {
        if (clip == null) return;

        sfxSource.pitch = Random.Range(0.9f, 1.1f);
        sfxSource.PlayOneShot(clip, volume);
    }

    private IEnumerator PlayImportant(AudioClip clip)
    {
        if (clip == null) yield break;

        PauseMusic();
        sfxSource.pitch = 1f;
        sfxSource.PlayOneShot(clip, importantVolume);

        yield return new WaitForSeconds(clip.length);
        ResumeMusic();
    }

    //////////////////////// Публичные методы ////////////////////////

    /// <summary>
    /// Создает звук в точке
    /// </summary>
    /// <param name="clip"></param>
    /// <param name="pos"></param>
    /// <param name="volume"></param>
    public static void PlayOneShotAt(AudioClip clip, Vector3 pos, float volume) =>
        AudioSource.PlayClipAtPoint(clip, pos, volume);

    /// <summary>
    /// Воспроизводит звук ходьбы
    /// </summary>
    public static void PlayWalk() => instance.PlayRandomStep(instance.walkSteps, instance.walkStepInterval);

    /// <summary>
    /// Воспроизводит звук бега
    /// </summary>
    public static void PlayRun() => instance.PlayRandomStep(instance.runSteps, instance.runStepInterval);

    /// <summary>
    /// Воспроизводит звук взятия предмета
    /// </summary>    
    public static void PlayPickup() => instance.PlaySFX(instance.pickupSound, instance.pickupVolume);

    /// <summary>
    /// Воспроизводит звук победы
    /// </summary>   
    public static void PlayWin() => instance.StartCoroutine(instance.PlayImportant(instance.winSound));

    /// <summary>
    /// Воспроизводит звук поражения
    /// </summary>   
    public static void PlayLose() => instance.StartCoroutine(instance.PlayImportant(instance.loseSound));
}