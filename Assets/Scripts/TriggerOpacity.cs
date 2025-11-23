using System.Collections;
using UnityEngine;

public class TriggerOpacity : MonoBehaviour
{
    [Header("Objects to fade")]
    [SerializeField] private GameObject[] objectsToFade;

    [Header("Fade settings")]
    [SerializeField] private float fadeDuration = 0.4f;
    [SerializeField, Range(0f, 1f)] private float fadeIntensity = 0f;
    [SerializeField] private string playerTag = "Player";

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
            foreach (GameObject obj in objectsToFade)
                if (obj != null)
                    StartCoroutine(FadeObject(obj, fadeIntensity));
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
            foreach (GameObject obj in objectsToFade)
                if (obj != null)
                    StartCoroutine(FadeObject(obj, 1f));
    }

    private IEnumerator FadeObject(GameObject target, float targetAlpha)
    {
        Renderer renderer = target.GetComponent<Renderer>();
        if (renderer == null) yield break;

        Material material = renderer.material;
        if (!material.HasProperty("_Color"))
        {
            yield break;
        }
        float startAlpha = material.color.a;

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            Color color = material.color;
            color.a = Mathf.Lerp(startAlpha, targetAlpha, elapsed / fadeDuration);
            material.color = color;

            elapsed += Time.deltaTime;
            yield return null;
        }

        Color finalColor = material.color;
        finalColor.a = targetAlpha;
        material.color = finalColor;
    }
}
