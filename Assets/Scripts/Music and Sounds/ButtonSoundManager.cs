using UnityEngine;

public class ButtonSoundManager : Singleton<ButtonSoundManager>
{
    [SerializeField] private AudioSource clickButtonSource;
    [SerializeField] private AudioSource enterAriaButtonSource;
    [SerializeField] private AudioClip clickSound;
    [SerializeField] private AudioClip enterAriaSound;

    protected override bool IsDestroyOnLoad => false;

    public void PlayClickSound() => clickButtonSource.PlayOneShot(clickSound);
    public void PlayEnterAriaSound() => enterAriaButtonSource.PlayOneShot(enterAriaSound);
}
