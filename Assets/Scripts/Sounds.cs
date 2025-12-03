using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sounds : MonoBehaviour
{
    public static Sounds Instance;
    public AudioClip[] sounds;
    public SoundArrays[] randSound;

    private AudioSource audioSrc => GetComponent<AudioSource>();

    public void PlaySound(int ind, float volume = 1f, bool random = false, bool destroyed = false, float p1 = 0.85f, float p2 = 1.2f)
    {
        AudioClip clip = random ? randSound[ind].soundArray[Random.Range(0, randSound[ind].soundArray.Length)] : sounds[ind];
        audioSrc.pitch = Random.Range(p1, p2);

        if (destroyed)
            AudioSource.PlayClipAtPoint(clip, transform.position, volume);
        else
            audioSrc.PlayOneShot(clip, volume);
    }

    [System.Serializable]
    public class SoundArrays
    {
        public AudioClip[] soundArray;
    }
    private void Awake()
    {
        Instance = this;

        if (!TryGetComponent(out AudioSource src))
            gameObject.AddComponent<AudioSource>();
    }
}
