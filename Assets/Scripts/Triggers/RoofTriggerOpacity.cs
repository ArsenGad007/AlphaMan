using System.Collections;
using UnityEngine;

public class RoofTriggerOpacity : MonoBehaviour
{
    [SerializeField] private GameObject[] objectsToFade;
    [SerializeField] private BoxCollider[] triggerZones; // массив коллайдеров

    [Header("Fade settings")]
    [SerializeField] private float fadeDuration = 0.4f;
    [SerializeField, Range(0f, 1f)] private float fadeIntensity = 0f;
    [SerializeField] private string playerTag = "Player";

    private int playersInside = 0; // счётчик игроков внутри

    private void Start()
    {
        if (triggerZones == null || triggerZones.Length == 0)
            triggerZones = GetComponents<BoxCollider>();

        foreach (var zone in triggerZones)
            zone.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            playersInside++;
            if (playersInside == 1) 
            {
                foreach (GameObject obj in objectsToFade)
                    if (obj != null)
                        StartCoroutine(FadeObject(obj, fadeIntensity));
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            playersInside--;
            if (playersInside == 0) 
            {
                foreach (GameObject obj in objectsToFade)
                    if (obj != null)
                        StartCoroutine(FadeObject(obj, 1f));
            }
        }
    }

    private IEnumerator FadeObject(GameObject target, float targetAlpha)
    {
        Renderer renderer = target.GetComponent<Renderer>();
        if (renderer == null) yield break;

        Material material = renderer.material;

        if (!material.HasProperty("_Color"))
            yield break;

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
