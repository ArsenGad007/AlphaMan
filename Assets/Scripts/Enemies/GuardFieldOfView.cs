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

    private float meshUpdateInterval;
    private float lastMeshUpdate;
    private float lastUpdateLOD;                                    // Время (в секундах) последнего обновления LOD

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