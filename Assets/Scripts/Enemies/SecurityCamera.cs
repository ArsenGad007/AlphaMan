using UnityEngine;

public class SecurityCamera : FieldOfView
{
    [Header("Смещения")]
    [SerializeField] private float forwardMeshOffset = 0.7f;    // вперед(+) / назад(-)
    [SerializeField] private float upMeshOffset = 0.2f;         // вверх (+) / вниз (-)
    [SerializeField] private float rightMeshOffset = 0f;        // вправо(+) / влево(-)

    [Header("Тест")]
    [SerializeField] private bool debugUpdateFOV = false;

    private bool isTriggered = false;

    private void Start()
    {
        SetGreenMaterial();
        RebuildMesh(GetRayStartPoint(), transform.forward);
    }

    private void Update()
    {
        if (debugUpdateFOV)
            RebuildMesh(GetRayStartPoint(), transform.forward);

        if (!isTriggered && IsPlayerVisible())
        {
            isTriggered = true;
            SetRedMaterial();
            GameOver.Instance.GameOverPanel();
        }
    }

    /// <summary>
    /// Применение смещения объектива камеры.
    /// </summary>
    private Vector3 GetRayStartPoint() =>
        transform.position
        + transform.forward * forwardMeshOffset
        + Vector3.up * upMeshOffset
        + Vector3.right * rightMeshOffset;
        

    /// <summary>
    /// Проверка попадания игрока в поле зрения камеры.
    /// </summary>
    public bool IsPlayerVisible()
    {
        if (!player) return false;

        // Грубая быстрая отсечка перед дорогой проверкой
        const float reserveDistanceSq = 4f;
        if ((player.transform.position - transform.position).sqrMagnitude > viewDistance * viewDistance * reserveDistanceSq)
            return false;

        Vector3 start = GetRayStartPoint();

        foreach (float height in CheckHeights)
        {
            Vector3 dir = player.transform.position + Vector3.up * height - start;
            float distance = dir.magnitude;

            if (distance <= viewDistance &&
                Vector3.Dot(transform.forward, dir / distance) >= CosHalfFov &&
                !CheckObstacle(start, dir.normalized, distance))
                return true;
        }

        return false;
    }
}