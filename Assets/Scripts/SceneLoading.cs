using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoading : MonoBehaviour
{
    [SerializeField] private float pauseSeconds = 1f;

    private void Start()
    {
        if (SceneTransition.TargetScene != null)
            StartCoroutine(Load());
    }

    IEnumerator Load()
    {
        AsyncOperation op = SceneManager.LoadSceneAsync(SceneTransition.TargetScene);
        op.allowSceneActivation = false;

        while (op.progress < 0.9f)
            yield return null;

        yield return new WaitForSeconds(pauseSeconds);
        op.allowSceneActivation = true;
    }
}
