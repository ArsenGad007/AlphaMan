using UnityEngine;

public class GuardFieldOfView : FieldOfView
{
    public enum DetectionType { None, AlertDelay, InstantDeath }    // Состояние обнаружения у охранника
    
    [Tooltip("Не пересчитывать меш чаще, чем раз в N секунд.")]
    [SerializeField, Min(0)] private float updateMeshInterval = 0.0f;

    private float instantDetectionRadius = 1.3f;    // УДАЛИТЬ!!!

    private Vector3 lastPosition = Vector3.zero;                    // Хранит позицию охранника на момент последней перестройки меша
    private Vector3 lastForward = Vector3.forward;                  // Хранит направление охранника на момент последней перестройки меша
    private float lastUpdateTime;                                   // Время (в секундах) последнего вызова перестройки меша
    private readonly int detectionFramesRequired = 2;               // Минимальное количество последовательных кадров, в которых игрок виден, чтобы сработало обнаружение.
    private int visibleFramesCount = 0;                             // Текущий счётчик последовательных кадров видимости игрока

    private const float POSITION_CHANGE_THRESHOLD_SQR = 0.01f;      // Квадрат минимального смещения позиции для обновления меша
    private const float DIRECTION_CHANGE_COS_THRESHOLD = 0.99939f;  // Косинус порога угла поворота (~2°) для обновления меша

    public Transform PlayerTransform => player?.transform;

    /// <summary>
    /// Обновляет визуальный меш
    /// </summary>
    public void UpdateFOV(Vector3 originPosition, Vector3 forwardDirection)
    {
        if (Time.time - lastUpdateTime < updateMeshInterval)
            return;

        Vector3 forward = forwardDirection;
        forward.y = 0f;
        forward = forward.sqrMagnitude < 0.0001f ? Vector3.forward : forward.normalized;

        bool shouldUpdate =
            (originPosition - lastPosition).sqrMagnitude > POSITION_CHANGE_THRESHOLD_SQR ||
            Vector3.Dot(lastForward, forward) < DIRECTION_CHANGE_COS_THRESHOLD;

        if (!shouldUpdate)
            return;

        RebuildMesh(originPosition, forward);

        lastPosition = originPosition;
        lastForward = forward;
        lastUpdateTime = Time.time;
    }

    /// <summary>
    /// Проверяет, мешает ли стена между охранником и игроком.
    /// </summary>
    private bool IsPlayerBlocked(float distance)
    {
        if (player == null) return true;

        Vector3 flatDirection = player.transform.position - transform.position;
        flatDirection.y = 0f;

        if (flatDirection.magnitude > distance)
            return true;

        int blockedCount = 0;
        foreach (float height in CheckHeights)
        {
            Vector3 rayOrigin = transform.position + Vector3.up * height;
            Vector3 toPlayer = player.transform.position - rayOrigin;

            if (CheckObstacle(rayOrigin, toPlayer.normalized, toPlayer.magnitude))
                blockedCount++;
        }

        return blockedCount >= 2;
    }

    /// <summary>
    /// Проверяет есть ли рядом стена
    /// </summary>
    /// <param name="distance"></param>
    /// <returns></returns>
    private bool IsNearWall(float distance)
    {
        Vector3 origin = transform.position + Vector3.up;
        return CheckObstacle(origin, transform.forward, distance) || CheckObstacle(origin, -transform.forward, distance);
    }

    /// <summary>
    /// Проверяет есть ли игрок рядом с стеной
    /// </summary>
    /// <param name="distance"></param>
    /// <returns></returns>
    private bool IsPlayerNearWall(float distance)
    {
        if (!player) return false;

        Vector3 toPlayer = player.transform.position - transform.position;
        toPlayer.y = 0f;
        toPlayer.Normalize();

        Vector3 playerOrigin = player.transform.position + Vector3.up;

        return CheckObstacle(playerOrigin, toPlayer, distance) || CheckObstacle(playerOrigin, -toPlayer, distance);
    }

    /// <summary>
    /// Определяет тип обнаружения игрока.
    /// </summary>
    public DetectionType CheckForDetection()
    {
        if (player == null)
        {
            visibleFramesCount = 0;
            return DetectionType.None;
        }

        Vector3 directionToPlayer = player.transform.position - transform.position;
        directionToPlayer.y = 0f;
        float distance = directionToPlayer.magnitude;

        bool isVisible = false;

        if (distance <= instantDetectionRadius)
        {
            bool bothNearWall = IsNearWall(0.4f) && IsPlayerNearWall(0.4f);

            if (!IsPlayerBlocked(instantDetectionRadius))
            {
                if (bothNearWall)
                {
                    Vector3 extraOrigin = transform.position + Vector3.up * 1.3f;

                    if (CheckObstacle(extraOrigin, directionToPlayer.normalized, distance, out RaycastHit extraHit)) { 
                        if (extraHit.collider.gameObject == player)
                            isVisible = true;
                    }
                    else
                      isVisible = true;
                }
                else
                    isVisible = true;
            }
        }
        else if (distance <= viewDistance)
        {
            Vector3 direction = directionToPlayer / distance;

            if (Vector3.Dot(transform.forward, direction) >= CosHalfFov && !IsPlayerBlocked(viewDistance))
                isVisible = true;
        }

        if (isVisible)
        {
            visibleFramesCount++;
            if (visibleFramesCount >= detectionFramesRequired)
                return distance <= instantDetectionRadius ? DetectionType.InstantDeath : DetectionType.AlertDelay;
        }
        else
            visibleFramesCount = 0;

        return DetectionType.None;
    }

    /// <summary>
    /// Проверяет, находится ли персонаж в радиусе мгновенного обнаружения (по горизонтали).
    /// </summary>
    /// <param name="person">Transform проверяемого персонажа.</param>
    /// <returns>True, если расстояние по горизонтали меньше или равно instantDetectionRadius.</returns>
    public bool IsPersonInInstantRange(Transform person)
    {
        if (person == null)
            return false;

        Vector3 delta = person.position - transform.position;
        delta.y = 0f;   // Игнорируем разницу по высоте

        return delta.sqrMagnitude <= instantDetectionRadius * instantDetectionRadius;
    }

    /// <summary>
    /// Простая проверка, видит ли охранник игрока в данный момент.
    /// </summary>
    public bool IsPlayerInFOV()
    {
        if (player == null) 
            return false;

        Vector3 toPlayer = player.transform.position - transform.position;
        float distance = toPlayer.magnitude;

        if (distance > viewDistance) 
            return false;

        Vector3 directionToPlayer = toPlayer / distance;

        // Проверка угла (только по горизонтали)
        Vector3 forward = transform.forward;
        forward.y = 0f;
        forward.Normalize();
        Vector3 dirFlat = directionToPlayer;
        dirFlat.y = 0f;
        dirFlat.Normalize();

        if (Vector3.Dot(forward, dirFlat) < CosHalfFov) 
            return false;

        // Проверка препятствий по трём высотам 
        int blockedCount = 0;
        foreach (float height in CheckHeights)
        {
            Vector3 origin = transform.position + Vector3.up * height;
            Vector3 target = player.transform.position + Vector3.up * height;
            Vector3 direction = target - origin;
            float dist = direction.magnitude;

            if (dist > viewDistance)
            {
                blockedCount++;
                continue;
            }

            if (CheckObstacle(origin, direction.normalized, dist))
                blockedCount++;
        }

        // Если хотя бы две высоты не заблокированы – игрок виден
        return blockedCount < 2;
    }
}