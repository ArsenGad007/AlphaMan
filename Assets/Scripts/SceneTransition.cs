using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
    [SerializeField] private GameObject RawImage;
    [SerializeField] private float speed = 5f;

    Material mat;
    RawImage overlay;

    void Start()
    {
        RawImage.SetActive(true);
        overlay = RawImage.GetComponent<RawImage>();
        mat = overlay.material;
        mat.SetFloat("_Aspect", (float)Screen.width / Screen.height);
        StartCoroutine(Open());
    }

    public static void Load(string sceneName) => FindFirstObjectByType<SceneTransition>().GoTo(sceneName);
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