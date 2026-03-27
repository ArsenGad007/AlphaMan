using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sounds : MonoBehaviour
{
    public static Sounds Instance;
    private AudioSource audioSrc;

    public event System.Action OnImportantSoundStarted;
    public event System.Action OnImportantSoundEnded;

    private void Awake()
    {
        Instance = this;

        if (!TryGetComponent(out audioSrc))
            audioSrc = gameObject.AddComponent<AudioSource>();
    }

    [Header("Одиночные звуки (One Shot)")]
    [SerializeField] private AudioClip walkSound;
    [Range(0f, 1f)] public float walkVolume = 1f;

    [SerializeField] private AudioClip runSound;
    [Range(0f, 1f)] public float runVolume = 1f;

    [SerializeField] private AudioClip pickupSound;
    [Range(0f, 1f)] public float pickupVolume = 1f;

    [SerializeField] private AudioClip winSound;
    [Range(0f, 1f)] public float winVolume = 1f;

    [SerializeField] private AudioClip loseSound;
    [Range(0f, 1f)] public float loseVolume = 1f;

    [Header("Наборы случайных звуков")]
    [SerializeField] private AudioClip[] randomSteps;
    [Range(0f, 1f)] public float randomStepVolume = 1f;

    // --- Важные звуки победы/поражения ---
    public void PlayWin() => PlayImportant(winSound, winVolume);
    public void PlayLose() => PlayImportant(loseSound, loseVolume);


    // --- Обычные ---
    [SerializeField] public AudioClip[] walkSteps;

    [SerializeField] public AudioClip[] runSteps;

    public void PlayWalk() => PlayRandomStep(walkSteps);

    public void PlayRun() => PlayRandomStep(runSteps);
    public void PlayPickup() => PlayOne(pickupSound, pickupVolume);

    /// <summary>
    /// Рандомные звуки шагов
    /// </summary>
    public void PlayRandomStep(AudioClip[] clips) =>
        PlayOne(clips[Random.Range(0, clips.Length)], randomStepVolume);

    private void PlayImportant(AudioClip clip, float volume)
    {
        if (clip == null) return;
        StartCoroutine(PlayImportantRoutine(clip, volume));
    }

    private IEnumerator PlayImportantRoutine(AudioClip clip, float volume)
    {
        OnImportantSoundStarted?.Invoke();

        audioSrc.pitch = 1f;
        audioSrc.PlayOneShot(clip, volume);

        yield return new WaitForSeconds(clip.length);

        OnImportantSoundEnded?.Invoke();
    }

    /// <summary>
    /// Базовый проигрыватель
    /// </summary>
    private void PlayOne(AudioClip clip, float volume = 1f, float pMin = 0.9f, float pMax = 1.1f)
    {
        if (clip == null) return;

        audioSrc.pitch = Random.Range(pMin, pMax);
        audioSrc.PlayOneShot(clip, volume);
    }

    //для 3д проигрывания у охранника
    public void PlayOneShotAt(AudioClip clip, Vector3 position, float volume)
    {
        if (clip == null) return;
        AudioSource.PlayClipAtPoint(clip, position, volume);
    }
}