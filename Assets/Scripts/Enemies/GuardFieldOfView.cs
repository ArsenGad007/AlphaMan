using UnityEngine;

public class GuardFieldOfView : FieldOfView
{
    public enum DetectionType { None, AlertDelay, InstantDeath }    // Состояние обнаружения у охранника

    [SerializeField, Range(2, 100)] private int minRayCount = 5;           // Минимальное кол-во лучей, на которые разбивается угол

    [Tooltip("Сколько раз в секунду обновляется мэш")]
    [SerializeField, Range(1, 144)] private int meshUpdateFPS = 60;

    [Header("Настройки LOD меша")]
    [Tooltip("Кол-во LOD для оптимизации rayCount")]
    [SerializeField, Range(2, 10)] private int countLOD = 4;

    [Tooltip("Менять LOD каждые N расстояний")]
    [SerializeField, Range(2, 20)] private int distanceLODUpdate = 5;

    [Tooltip("Не пересчитывать LOD чаще, чем раз в N секунд.")]
    [SerializeField, Min(0)] private float updateLODInterval = 0.1f;

    private float instantDetectionRadius = 1.3f;    // УДАЛИТЬ!!!

    private float meshUpdateInterval;
    private float lastMeshUpdate;
    private float lastUpdateLOD;                                    // Время (в секундах) последнего обновления LOD
    private readonly int detectionFramesRequired = 2;               // Минимальное количество последовательных кадров, в которых игрок виден, чтобы сработало обнаружение.
    private int visibleFramesCount = 0;                             // Текущий счётчик последовательных кадров видимости игрока

    public Transform PlayerTransform => player?.transform;

    protected override void Start()
    {
        base.Start();

        if (maxRayCount < minRayCount)
        {
            Debug.LogError("maxRayCount меньше minRayCount");
            maxRayCount = minRayCount;
        }
            
        meshUpdateInterval = 1f / meshUpdateFPS;
    }

    /// <summary>
    /// Обновляет визуальный меш
    /// </summary>
    public void UpdateFOV(Vector3 originPosition, Vector3 forwardDirection)
    {
        if (Time.time - lastUpdateLOD >= updateLODInterval)
        {
            float distance_player = Vector3.Distance(transform.position, player.transform.position);

            int max_num_LOD = countLOD - 1;
            int num_LOD = Mathf.Clamp(Mathf.FloorToInt(distance_player / distanceLODUpdate), 0, max_num_LOD);
            rayCount = Mathf.RoundToInt(Mathf.Lerp(maxRayCount, minRayCount, (float)num_LOD / max_num_LOD));

            lastUpdateLOD = Time.time;
        }

        Vector3 forward = forwardDirection;
        forward.y = 0f;
        forward = forward.sqrMagnitude < 0.0001f ? Vector3.forward : forward.normalized;    // защита если forward почти равен нулю

        if(Time.time - lastMeshUpdate >= meshUpdateInterval)
        {
            RebuildMesh(originPosition, forward);
            lastMeshUpdate = Time.time;
        }
    }

    /// <summary>
    /// Проверяет, мешает ли стена между охранником и игроком.
    /// </summary>
    private bool IsPlayerBlocked(float distance)
    {
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