using UnityEngine;

public class ButtonSoundManager : MonoBehaviour
{
    public static ButtonSoundManager Instance { get; private set; }

    [SerializeField] private AudioSource clickAudioSource;
    [SerializeField] private AudioSource enterAriaAudioSource;
    [SerializeField] private AudioClip clickSound;
    [SerializeField] private AudioClip enterAriaSound;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void PlayClickSound() => clickAudioSource.PlayOneShot(clickSound);
    public void PlayEnterAriaSound() => enterAriaAudioSource.PlayOneShot(enterAriaSound);
}
