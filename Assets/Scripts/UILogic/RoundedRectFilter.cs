using UnityEngine;

public class RoundedRectFilter : MonoBehaviour, ICanvasRaycastFilter
{
    [SerializeField] private float radius = 30f; // радиус скругления в пикселях
    RectTransform rectTransform;

    void Awake() => rectTransform = GetComponent<RectTransform>();

    public bool IsRaycastLocationValid(Vector2 sp, Camera cam)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, sp, cam, out var p);
        Rect r = rectTransform.rect;

        // Ближайший угол к точке p
        float cx = Mathf.Clamp(p.x, r.xMin + radius, r.xMax - radius);
        float cy = Mathf.Clamp(p.y, r.yMin + radius, r.yMax - radius);

        float dx = p.x - cx;
        float dy = p.y - cy;

        return dx * dx + dy * dy <= radius * radius;
    }
}