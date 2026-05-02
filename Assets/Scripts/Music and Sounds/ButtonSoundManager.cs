using UnityEngine;

public class ButtonSoundManager : MonoBehaviour
{
    public static ButtonSoundManager Instance { get; private set; }

    [SerializeField] private AudioSource clickButtonSource;
    [SerializeField] private AudioSource enterAriaButtonSource;
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

    public void PlayClickSound() => clickButtonSource.PlayOneShot(clickSound);
    public void PlayEnterAriaSound() => enterAriaButtonSource.PlayOneShot(enterAriaSound);
}
