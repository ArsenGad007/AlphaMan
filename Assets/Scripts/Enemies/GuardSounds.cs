using UnityEngine;
//пока только кряхтение, 
public class GuardSounds : MonoBehaviour
{
    [SerializeField] public AudioClip[] gruntSounds;
    [Range(0f, 1f)] private float groanVolume = 1f;
    [SerializeField] private float minInterval = 10f;
    [SerializeField] private float maxInterval = 20f;
    private float nextGroanTime;

    void Start()
    {
        NextGroan();
    }

    void Update()
    {
        if (Time.time >= nextGroanTime)
        {
            PlayRandomGroan();
            NextGroan();
        }
    }

    private void PlayRandomGroan()
    {
        if (gruntSounds == null) return;
        var clip = gruntSounds[Random.Range(0, gruntSounds.Length)];
        SoundManager.PlayOneShotAt(clip, transform.position, groanVolume);
    }

    private void NextGroan()
    {
        nextGroanTime = Time.time + Random.Range(minInterval, maxInterval);
    }
}
