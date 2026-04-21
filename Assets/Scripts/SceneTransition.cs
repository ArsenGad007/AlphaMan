using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
    public static bool dontOpenNextScene = false;
    [SerializeField] private float speed = 5f;

    private static SceneTransition instance;
    private Material mat;
    private RawImage overlay;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {       
        overlay = GetComponentInChildren<RawImage>(true);   // true — находит неактивные
        overlay.gameObject.SetActive(true);

        mat = Instantiate(overlay.material);                // создаём копию
        overlay.material = mat;

        mat.SetFloat("_Aspect", (float)Screen.width / Screen.height);

        if (!dontOpenNextScene)
        {
            mat.SetFloat("_Radius", 0f);
            StartCoroutine(Open());
        }
        else
        {
            dontOpenNextScene = false;
            mat.SetFloat("_Radius", 0.8f);
            overlay.gameObject.SetActive(false);         
        }
    }

    /// <summary>
    /// Загрузка новой сцены с переходом
    /// </summary>
    /// <param name="sceneName"></param>
    public static void Load(string sceneName) => instance.GoTo(sceneName);

    private void GoTo(string sceneName) => StartCoroutine(Go(sceneName));

    IEnumerator Open()
    {
        for (float t = 0; t < 1; t += Time.deltaTime * speed)
        {
            mat.SetFloat("_Radius", t * 0.8f);
            yield return null;
        }
        overlay.gameObject.SetActive(false);
    }

    IEnumerator Go(string sceneName)
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