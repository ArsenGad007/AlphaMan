using System.Collections.Generic;
using UnityEngine;

public abstract class FieldOfView : MonoBehaviour
{
    [Header("Основные настройки")]
    [SerializeField][Min(0)] protected float viewDistance = 7f;         // дальность
    [SerializeField][Range(0, 180)] protected float fovAngle = 90f;     // ширина (в градусах)
    [SerializeField][Range(1, 100)] protected int rayCount = 50;        // кол-во лучей, на которые разбивается угол
    [SerializeField] protected LayerMask obstacleMask;                  // маска слоёв, которые лучи считают препятствием
    [SerializeField] protected GameObject player;

    protected Mesh meshFOV;        // меш конуса обзора
    protected Renderer rendFOV;    // рендер конуса обзора

    protected static Material greenMat;
    protected static Material yellowMat;
    protected static Material redMat;

    protected readonly float[] CheckHeights = { 1.8f, 0.9f, 0f };               // высоты (голова/тело/ноги), по которым проверяется вход в поле зрения
    
    protected float CosHalfFov => Mathf.Cos(fovAngle * 0.5f * Mathf.Deg2Rad);   // Косинус половины угла обзора. Используется для проверки "виден ли игрок"

    protected virtual void Awake()
    {
        meshFOV = new Mesh();
        GetComponent<MeshFilter>().mesh = meshFOV;
        rendFOV = GetComponent<Renderer>();

        if (!greenMat)  greenMat = Resources.Load<Material>("FOV_mat/FOV_Walking");
        if (!yellowMat) yellowMat = Resources.Load<Material>("FOV_mat/FOV_Alert");
        if (!redMat)    redMat = Resources.Load<Material>("FOV_mat/FOV_Danger");
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
        float halfFov = fovAngle * 0.5f;
        float step = fovAngle / rayCount;

        List<Vector3> points = new List<Vector3>() { origin };

        float previousDistance = viewDistance;
        bool previousHit = false;

        for (int i = 0; i <= rayCount; i++)
        {
            float angle = -halfFov + step * i;
            Vector3 direction = Quaternion.Euler(0f, angle, 0f) * forward;

            bool hit = Physics.Raycast(
                origin,
                direction,
                out RaycastHit hitInfo,
                viewDistance,
                obstacleMask,
                QueryTriggerInteraction.Ignore
            );

            Vector3 currentPoint = hit ? hitInfo.point : origin + direction * viewDistance;

            float currentDistance = hit ? hitInfo.distance : viewDistance;

            if (i > 0 && previousHit != hit && Mathf.Abs(previousDistance - currentDistance) > 0.5f)
            {
                float leftAngle = angle - step;
                float rightAngle = angle;

                for (int j = 0; j < 5; j++)
                {
                    float middleAngle = (leftAngle + rightAngle) * 0.5f;

                    Vector3 middleDirection = Quaternion.Euler(0f, middleAngle, 0f) * forward;

                    bool middleHit = Physics.Raycast(
                        origin,
                        middleDirection,
                        out _,
                        viewDistance,
                        obstacleMask,
                        QueryTriggerInteraction.Ignore
                    );

                    if (middleHit == previousHit)
                        leftAngle = middleAngle;
                    else
                        rightAngle = middleAngle;
                }

                Vector3 boundaryDirection = Quaternion.Euler(0f, (leftAngle + rightAngle) * 0.5f, 0f) * forward;

                if (Physics.Raycast(
                    origin,
                    boundaryDirection,
                    out RaycastHit boundaryHit,
                    viewDistance,
                    obstacleMask,
                    QueryTriggerInteraction.Ignore))
                {
                    points.Add(boundaryHit.point);
                }
                else
                    points.Add(origin + boundaryDirection * viewDistance);
            }

            points.Add(currentPoint);

            previousDistance = currentDistance;
            previousHit = hit;
        }

        Vector3[] vertices = new Vector3[points.Count];

        for (int i = 0; i < points.Count; i++)
            vertices[i] = transform.InverseTransformPoint(points[i]);

        int[] triangles = new int[(points.Count - 2) * 3];

        for (int i = 0; i < points.Count - 2; i++)
        {
            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = i + 2;
        }

        meshFOV.Clear();
        meshFOV.vertices = vertices;
        meshFOV.triangles = triangles;
        meshFOV.RecalculateNormals();
    }
}