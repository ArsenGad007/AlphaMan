using System.Collections;
using UnityEngine;

public class ElevatorSound : MonoBehaviour
{
    [SerializeField] private AudioClip audioClip;
    [SerializeField] [Range(0,1)] private float volume = 1f;
    [SerializeField] [Min(0)] private float delaySec;

    void Start()
    {
        StartCoroutine(Delay());
    }

    IEnumerator Delay()
    {
        yield return new WaitForSeconds(delaySec);
        SoundManager.PlayOneShotAt(audioClip, gameObject.transform.position, volume);
    }
}
