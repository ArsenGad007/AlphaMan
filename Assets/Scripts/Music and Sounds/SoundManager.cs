using System.Collections;
using UnityEngine;

/// <summary>
/// Единый менеджер звука: фоновая музыка + все SFX.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class SoundManager : Singleton<SoundManager>, ISpeedUpgradable
{
    [Header("Фоновая музыка")]
    [SerializeField] private AudioSource musicSource; 

    [Header("Ходьба")]
    [SerializeField] private AudioClip[] walkSteps;
    [Range(0f, 2f)][SerializeField] private float walkStepVolume = 1.5f;
    [Tooltip("Фиксированный интервал между шагами ходьбы в секундах")]
    [Min(0f)][SerializeField] private float walkStepInterval = 0.5f;

    [Header("Бег")]
    [SerializeField] private AudioClip[] runSteps;
    [Range(0f, 2f)][SerializeField] private float runStepVolume = 1f;
    [Tooltip("Фиксированный интервал между шагами бега в секундах")]
    [Min(0f)][SerializeField] private float maxRunStepInterval = 0.43f;
    [Min(0f)][SerializeField] private float minRunStepInterval = 0.33f;

    [Header("Подбор предметов")]
    [SerializeField] private AudioClip itemPickupSound;
    [Range(0f, 2f)][SerializeField] private float itemPickupVolume = 1.4f;

    [Header("Подбор монет")]
    [SerializeField] private AudioClip coinPickupSound;
    [Range(0f, 2f)][SerializeField] private float coinPickupVolume = 0.15f;

    [Header("Победа / Поражение")]
    [SerializeField] private AudioClip winSound;
    [SerializeField] private AudioClip loseSound;
    [Range(0f, 2f)][SerializeField] private float loseWinVolume = 0.1f;

    private AudioSource sfxSource;
    private float lastStepTime = 0f;

    protected override void Awake()
    {
        base.Awake();
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
    private void PlayRandomStep(AudioClip[] clips, float interval, float volume)
    {
        if (clips == null || clips.Length == 0) return;
        if (Time.time - lastStepTime < interval) return;

        lastStepTime = Time.time;
        AudioClip clip = clips[Random.Range(0, clips.Length)];
        PlaySFX(clip, volume);
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
        sfxSource.PlayOneShot(clip, loseWinVolume);

        yield return new WaitForSeconds(clip.length);
        ResumeMusic();
    }

    //////////////////////// Публичные методы ////////////////////////

    public void SpeedProgressUpdate()
    {
        float step = (maxRunStepInterval - minRunStepInterval) / SavesLogic.Get("progress_bar_size", 4);
        SavesLogic.Set("run_step_sound", SavesLogic.Get("run_step_sound", maxRunStepInterval) - step);
        Debug.Log($"run_step_sound: {SavesLogic.Get("run_step_sound", maxRunStepInterval)}");
    }

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
    public static void PlayWalk() => Instance.PlayRandomStep(Instance.walkSteps, Instance.walkStepInterval, Instance.walkStepVolume);

    /// <summary>
    /// Воспроизводит звук бега
    /// </summary>
    public static void PlayRun() => Instance.PlayRandomStep(Instance.runSteps, SavesLogic.Get("run_step_sound", Instance.maxRunStepInterval), Instance.runStepVolume);

    /// <summary>
    /// Воспроизводит звук вз ятия предмета
    /// </summary>    
    public static void PlayItemPickup() => Instance.PlaySFX(Instance.itemPickupSound, Instance.itemPickupVolume);

    /// <summary>
    /// Воспроизводит звук взятия предмета
    /// </summary>    
    public static void PlayCoinPickup() => Instance.PlaySFX(Instance.coinPickupSound, Instance.coinPickupVolume);

    /// <summary>
    /// Воспроизводит звук победы
    /// </summary>   
    public static void PlayWin() => Instance.StartCoroutine(Instance.PlayImportant(Instance.winSound));

    /// <summary>
    /// Воспроизводит звук поражения
    /// </summary>   
    public static void PlayLose() => Instance.StartCoroutine(Instance.PlayImportant(Instance.loseSound));
}