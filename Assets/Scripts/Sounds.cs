using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sounds : MonoBehaviour
{
    public static Sounds Instance;
    private AudioSource audioSrc;

    private void Awake()
    {
        Instance = this;

        if (!TryGetComponent(out audioSrc))
            audioSrc = gameObject.AddComponent<AudioSource>();
    }

    [Header("Одиночные звуки (One Shot)")]
    [SerializeField] private AudioClip walkSound;
    [SerializeField] private AudioClip runSound;
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private AudioClip winSound;
    [SerializeField] private AudioClip loseSound;

    [Header("Наборы случайных звуков")]
    [SerializeField] private AudioClip[] randomSteps;

    // Отделльные звуки
    public void PlayWalk() => PlayOne(walkSound);
    public void PlayRun() => PlayOne(runSound);
    public void PlayPickup() => PlayOne(pickupSound);
    public void PlayWin() => PlayOne(winSound);
    public void PlayLose() => PlayOne(loseSound);

    /// <summary>
    /// Рандомные звуки шагов
    /// </summary>
    public void PlayRandomStep()
    {
        PlayOne(randomSteps[Random.Range(0, randomSteps.Length)]);
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
}
