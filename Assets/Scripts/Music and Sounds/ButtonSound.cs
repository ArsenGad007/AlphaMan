using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(EventTrigger))]
public class ButtonSound : MonoBehaviour
{
    public void OnClick() => ButtonSoundManager.Instance.PlayClickSound();
    public void OnAria() => ButtonSoundManager.Instance.PlayEnterAriaSound();
}
