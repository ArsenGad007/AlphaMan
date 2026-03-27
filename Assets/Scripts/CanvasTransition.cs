using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CanvasTransition : MonoBehaviour
{
    [SerializeField] private GameObject rawImageObj;
    [SerializeField] private float speed = 5f;

    private static CanvasTransition instance;
    private Material mat;
    private RawImage overlay;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);                  // Переживает смену сцен

        rawImageObj.SetActive(true);
        overlay = rawImageObj.GetComponent<RawImage>();
        mat = overlay.material;

        SceneManager.sceneLoaded += OnSceneLoaded;      // Подписываемся — будет вызываться при каждой загрузке сцены 
    }

    private void OnDestroy() => SceneManager.sceneLoaded -= OnSceneLoaded;

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) => StartOpen();

    private void Start() => StartOpen();

    private void StartOpen()
    {
        mat.SetFloat("_Aspect", (float)Screen.width / Screen.height);
        StartCoroutine(Open());
    }

    public static void LoadScene(string sceneName) => instance.CloseTo(sceneName);
    private void CloseTo(string sceneName) => StartCoroutine(Close(sceneName));

    IEnumerator Open()
    {
        for (float t = 0; t < 1; t += Time.deltaTime * speed)
        {
            mat.SetFloat("_Radius", t * 0.8f);
            yield return null;
        }
        overlay.gameObject.SetActive(false);
    }

    IEnumerator Close(string sceneName)
    {
        overlay.gameObject.SetActive(true);
        for (float t = 0; t < 1; t += Time.deltaTime * speed)
        {
            mat.SetFloat("_Radius", (1 - t) * 0.8f - 0.1f);
            yield return null;
        }
        SceneManager.LoadScene(sceneName);
    }
}