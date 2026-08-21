using System.Collections.Generic;
using UnityEngine;

public abstract class FieldOfView : MonoBehaviour
{
    [SerializeField] protected LayerMask obstacleMask;                 // маска слоёв, которые лучи считают препятствием

    [Header("Настройки поля зрения")]
    [SerializeField][Min(0)] protected float viewDistance = 7f;        // дальность
    [SerializeField][Range(0, 180)] protected float fovAngle = 90f;    // ширина (в градусах)
    [SerializeField][Range(1, 180)] protected int maxRayCount = 50;    // максимальное кол-во лучей, на которые разбивается угол

    protected Mesh meshFOV;     // меш конуса обзора
    protected Renderer rendFOV; // рендер конуса обзора

    protected GameObject player;

    protected static Material greenMat;
    protected static Material yellowMat;
    protected static Material redMat;

    protected readonly float[] CheckHeights = { 1.8f, 0.9f, 0f };               // высоты (голова/тело/ноги), по которым проверяется вход в поле зрения
    
    protected float CosHalfFov => Mathf.Cos(fovAngle * 0.5f * Mathf.Deg2Rad);   // Косинус половины угла обзора. Используется для проверки "виден ли игрок"

    protected int rayCount;

    // Переиспользуемые буферы в RebuildMesh
    private List<Vector3> pointsBuffer;
    private Vector3[] verticesBuffer;
    private Vector3[] normalsBuffer;
    private int[] trianglesBuffer;
    private int lastTriangleCount = -1;

    protected virtual void Awake()
    {
        meshFOV = new Mesh();
        GetComponent<MeshFilter>().mesh = meshFOV;
        meshFOV.MarkDynamic();

        rendFOV = GetComponent<Renderer>();

        if (!greenMat)  greenMat = Resources.Load<Material>("FOV_mat/FOV_Walking");
        if (!yellowMat) yellowMat = Resources.Load<Material>("FOV_mat/FOV_Alert");
        if (!redMat)    redMat = Resources.Load<Material>("FOV_mat/FOV_Danger");

        rayCount = maxRayCount;
        pointsBuffer = new List<Vector3>(rayCount + 16);      
    }

    protected virtual void Start()
    {
        player = PlayerController.Instance.gameObject;
    }

    /// <summary>
    /// Устанавливает зеленый материал меша
    /// </summary>
    public void SetGreenMaterial() => rendFOV.sharedMaterial = greenMat;

    /// <summary>
    /// Устанавливает желтый материал меша
    /// </summary>
    public void SetYellowMaterial() => rendFOV.sharedMaterial = yellowMat;

    /// <summary>
    /// Устанавливает красный материал меша
    /// </summary>
    public void SetRedMaterial() => rendFOV.sharedMaterial = redMat;

    /// <summary>
    /// Проверяет наличие препятствия
    /// </summary>
    /// <param name="origin"></param>
    /// <param name="direction"></param>
    /// <param name="distance"></param>
    /// <param name="hit"></param>
    /// <returns></returns>
    protected bool CheckObstacle(Vector3 origin, Vector3 direction, float distance, out RaycastHit hit)
       => Physics.Raycast(origin, direction, out hit, distance, obstacleMask, QueryTriggerInteraction.Ignore);

    /// <summary>
    /// Проверяет наличие препятствия
    /// </summary>
    /// <param name="origin"></param>
    /// <param name="direction"></param>
    /// <param name="distance"></param>
    /// <returns></returns>
    protected bool CheckObstacle(Vector3 origin, Vector3 direction, float distance)
        => Physics.Raycast(origin, direction, distance, obstacleMask, QueryTriggerInteraction.Ignore);

    /// <summary>
    /// Строит меш конуса обзора от originPosition в направлении forwardDirection,
    /// сглаживая разрывы между соседними лучами (например, на краях препятствий).
    /// </summary>
    protected void RebuildMesh(Vector3 origin, Vector3 forward)
    {
        float half_fov = fovAngle * 0.5f;
        float step = fovAngle / rayCount;

        pointsBuffer.Clear();
        pointsBuffer.Add(origin);

        float previous_distance = viewDistance;
        bool previous_hit = false;

        for (int i = 0; i <= rayCount; i++)
        {
            float angle = -half_fov + step * i;
            Vector3 direction = Quaternion.Euler(0f, angle, 0f) * forward;

            bool hit = Physics.Raycast(
                origin,
                direction,
                out RaycastHit hitInfo,
                viewDistance,
                obstacleMask,
                QueryTriggerInteraction.Ignore
            );

            Vector3 current_point = hit ? hitInfo.point : origin + direction * viewDistance;
            float current_distance = hit ? hitInfo.distance : viewDistance;

            if (i > 0 && previous_hit != hit && Mathf.Abs(previous_distance - current_distance) > 0.5f)
            {
                float left_angle = angle - step;
                float right_angle = angle;

                for (int j = 0; j < 5; j++)
                {
                    float middle_angle = (left_angle + right_angle) * 0.5f;

                    Vector3 middleDirection = Quaternion.Euler(0f, middle_angle, 0f) * forward;

                    bool middle_hit = Physics.Raycast(
                        origin,
                        middleDirection,
                        out _,
                        viewDistance,
                        obstacleMask,
                        QueryTriggerInteraction.Ignore
                    );

                    if (middle_hit == previous_hit)
                        left_angle = middle_angle;
                    else
                        right_angle = middle_angle;
                }

                Vector3 boundary_direction = Quaternion.Euler(0f, (left_angle + right_angle) * 0.5f, 0f) * forward;

                if (Physics.Raycast(
                    origin,
                    boundary_direction,
                    out RaycastHit boundary_hit,
                    viewDistance,
                    obstacleMask,
                    QueryTriggerInteraction.Ignore))
                {
                    pointsBuffer.Add(boundary_hit.point);
                }
                else
                    pointsBuffer.Add(origin + boundary_direction * viewDistance);
            }

            pointsBuffer.Add(current_point);

            previous_distance = current_distance;
            previous_hit = hit;
        }
        
        int count = pointsBuffer.Count;

        // Массив нужного размера создаётся, только если старого либо ещё нет (null), либо его длина не совпадает с новым count
        if (verticesBuffer == null || verticesBuffer.Length != count)
        {
            verticesBuffer = new Vector3[count];
            normalsBuffer = new Vector3[count];

            for (int i = 0; i < count; i++) 
                normalsBuffer[i] = Vector3.up;
        }

        for (int i = 0; i < count; i++)
            verticesBuffer[i] = transform.InverseTransformPoint(pointsBuffer[i]);   // Заполнение вершин

        int triangles_count = (count - 2) * 3;
        if (trianglesBuffer == null || lastTriangleCount != triangles_count)
        {
            trianglesBuffer = new int[triangles_count];

            for (int i = 0; i < count - 2; i++)
            {
                trianglesBuffer[i * 3] = 0;
                trianglesBuffer[i * 3 + 1] = i + 1;
                trianglesBuffer[i * 3 + 2] = i + 2;
            }

            lastTriangleCount = triangles_count;
        }

        meshFOV.Clear();
        meshFOV.vertices = verticesBuffer;
        meshFOV.normals = normalsBuffer;  
        meshFOV.triangles = trianglesBuffer;
    }
}