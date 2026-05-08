using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Переход IRIS на другую сцену
/// </summary>
public class SceneTransition : MonoBehaviour
{
    public static bool dontOpenNextScene = false;
    public static string TargetScene { private set; get; }
    public static bool IsTransitionGo { private set; get; } = false;

    [SerializeField] private float speed = 5f;
   
    private static SceneTransition instance;
    private bool showLoadingScene = false;
    private Material mat;
    private RawImage overlay;

    void Awake()
    {
        instance = this;
        overlay = GetComponentInChildren<RawImage>(true);   // true — находит неактивные
    }

    void Start()
    {          
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
    public static void Load(string sceneName, bool showLoadingScene = false)
    {
        instance.showLoadingScene = showLoadingScene;
        instance.GoTo(sceneName);
    }

    private void GoTo(string sceneName) => StartCoroutine(Go(sceneName));

    IEnumerator Open()
    {
        IsTransitionGo = true;

        for (float t = 0; t < 1; t += Time.deltaTime * speed)
        {
            mat.SetFloat("_Radius", t * 0.8f);
            yield return null;
        }
        
        overlay.gameObject.SetActive(false);
        IsTransitionGo = false;
    }

    IEnumerator Go(string sceneName)
    {
        IsTransitionGo = true;
        overlay.gameObject.SetActive(true);      

        for (float t = 0; t < 1; t += Time.deltaTime * speed)
        {
            mat.SetFloat("_Radius", (1 - t) * 0.8f - 0.1f);
            yield return null;
        }

        IsTransitionGo = false;

        if (!showLoadingScene)
            SceneManager.LoadScene(sceneName);
        else
        {
            TargetScene = sceneName;
            SceneManager.LoadScene("Loading");
        }
    }
}