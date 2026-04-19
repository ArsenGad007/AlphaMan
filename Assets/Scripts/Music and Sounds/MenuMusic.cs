using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MenuMusic : MonoBehaviour
{
    public static MenuMusic instance { get; private set; }

    private AudioSource musicSource;

    private void Awake()
    {
        if(instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        musicSource = GetComponent<AudioSource>();
    }

    public static void Play() => instance.musicSource.Play();
    public static void Stop() => instance.musicSource.Stop();
    public static bool IsPlay()
    {
        if (instance == null)
        {
            GameObject prefab = Resources.Load<GameObject>("MenuMusic"); // имя префаба
            GameObject obj = Instantiate(prefab);
            instance = obj.GetComponent<MenuMusic>();
            DontDestroyOnLoad(obj);
        }
        return instance.musicSource.isPlaying;
    }
}
