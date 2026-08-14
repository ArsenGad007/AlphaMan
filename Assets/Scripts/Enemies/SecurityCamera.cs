using UnityEngine;

public class SecurityCamera : MonoBehaviour
{
    [Header("Основные настройки")]
    [SerializeField] float viewDistance = 8f;  // дальность
    [SerializeField] float fovAngle = 90f;     // ширина
    [SerializeField] int rayCount = 10;
    [SerializeField] LayerMask obstacleMask;
    [SerializeField] GameObject player;

    [Header("Смещения")]
    [SerializeField] float lensOffset = 0.7f;   // назад(-) / вперед(+)
    [SerializeField] float lensDrop = 0.2f;     // вниз(-)  / вверх(+)
    [SerializeField] float lensSide = 0f;       // влево(-) / вправо(+)

    [Header("Тест")]
    [SerializeField] bool debugUpdateFOV = false;   // тестовый тумблер

    private Mesh mesh;
    private Renderer rend;

    private Material Green;
    private Material Red;

    private bool isTriggered = false;
    private static readonly float[] CheckHeights = { 1.8f, 0.6f, 0f };

    void Awake()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        rend = GetComponent<Renderer>();

        Green = Resources.Load<Material>("FOV_mat/FOV_Walking");
        Red = Resources.Load<Material>("FOV_mat/FOV_Danger");
    }
    private void Start()
    {
        SetMaterial(Green);
        UpdateFOV(GetRayStartPoint(), transform.forward);
    }

    private void Update()
    {
        if (debugUpdateFOV)
            UpdateFOV(GetRayStartPoint(), transform.forward);

        if (!isTriggered && IsPlayerVisible())
        {
            isTriggered = true;
            SetMaterial(Red);
            GameOver.Instance.GameOverPanel();
        }
    }

    private void SetMaterial(Material mat) => rend.sharedMaterial = mat;

    /// <summary>
    /// Применение Смещения
    /// </summary>
    /// <returns></returns>
    Vector3 GetRayStartPoint() => 
        transform.position
        + transform.forward * lensOffset
        + Vector3.right * lensSide
        + Vector3.up * lensDrop;
       
    /// <summary>
    /// Проверка попадания игрока в поле зрения камеры
    /// </summary>
    /// <returns></returns>
    public bool IsPlayerVisible()
    {
        if (!player) return false;

        // Грубая быстрая отсечка перед дорогой проверкой
        float reserve_distance = 4f;
        if ((player.transform.position - transform.position).sqrMagnitude > viewDistance * viewDistance * reserve_distance)
            return false; 

        Vector3 start = GetRayStartPoint();
        Vector3 pos = player.transform.position;

        float cosHalfFov = Mathf.Cos(fovAngle * 0.5f * Mathf.Deg2Rad); // косинус половины угла обзора

        foreach (float height in CheckHeights)
        {
            Vector3 dir = pos + Vector3.up * height - start;
            float distance = dir.magnitude;
            if (distance <= viewDistance &&
                Vector3.Dot(transform.forward, dir / distance) >= cosHalfFov &&
                !Physics.Raycast(start, dir.normalized, distance, obstacleMask))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Обновление видимости поля зрения камеры
    /// </summary>
    /// <param name="origin"></param>
    /// <param name="forward"></param>
    private void UpdateFOV(Vector3 origin, Vector3 forward)
    {
        float half = fovAngle / 2;
        float step = fovAngle / rayCount;
        var vertices = new Vector3[rayCount + 2];
        var triangles = new int[rayCount * 3];
        vertices[0] = transform.InverseTransformPoint(origin);

        for (int i = 1; i <= rayCount + 1; i++)
        {
            float angle = -half + (i - 1) * step;
            Vector3 dir = Quaternion.Euler(0, angle, 0) * forward;
            Vector3 point = Physics.Raycast(
                origin, dir, out RaycastHit hit, viewDistance,
                obstacleMask, QueryTriggerInteraction.Ignore)
                ? hit.point
                : origin + dir * viewDistance;
            vertices[i] = transform.InverseTransformPoint(point);

            if (i > 1)
            {
                int t = (i - 2) * 3;
                triangles[t] = 0;
                triangles[t + 1] = i - 1;
                triangles[t + 2] = i;
            }
        }

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }
}