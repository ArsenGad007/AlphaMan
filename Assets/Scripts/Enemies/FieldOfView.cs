using UnityEngine;

public abstract class FieldOfView : MonoBehaviour
{
    [Header("Основные настройки")]
    [SerializeField][Min(0)] protected float viewDistance = 8f;         // дальность
    [SerializeField][Range(0, 180)] protected float fovAngle = 90f;     // ширина (в градусах)
    [SerializeField][Range(1, 50)] protected int rayCount = 10;         // кол-во лучей, на которые разбивается угол
    [SerializeField] protected LayerMask obstacleMask;                  // маска слоёв, которые лучи считают препятствием
    [SerializeField] protected GameObject player;

    protected Mesh mesh;        // меш конуса обзора
    protected Renderer rend;    // рендер конуса обзора

    protected readonly float[] CheckHeights = { 1.8f, 0.9f, 0f };               // высоты (голова/тело/ноги), по которым проверяется вход в поле зрения
    
    protected float CosHalfFov => Mathf.Cos(fovAngle * 0.5f * Mathf.Deg2Rad);   // Косинус половины угла обзора. Используется для проверки "виден ли игрок"

    protected virtual void Awake()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        rend = GetComponent<Renderer>();
    }

    /// <summary>
    /// Устонавливает материал меша
    /// </summary>
    /// <param name="mat"></param>
    public void SetMaterial(Material mat) => rend.sharedMaterial = mat;

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
    protected void RebuildMesh(Vector3 originPosition, Vector3 forwardDirection)
    {
        float halfFov = fovAngle * 0.5f;
        float angleStep = fovAngle / rayCount;

        var vertices = new Vector3[rayCount + 2];
        var uvs = new Vector2[vertices.Length];
        var triangles = new int[rayCount * 3];

        int vertexIndex = 0;
        int triangleIndex = 0;

        vertices[vertexIndex++] = transform.InverseTransformPoint(originPosition);

        for (int i = 0; i <= rayCount; i++)
        {
            float angle = -halfFov + i * angleStep;
            Vector3 rayDirection = Quaternion.Euler(0, angle, 0) * forwardDirection;
            Vector3 currentPoint = CastRay(originPosition, rayDirection);
            Vector3 finalPoint = currentPoint;

            if (i > 0)
            {
                Vector3 prevPoint = vertices[vertexIndex - 1];

                if ((prevPoint - currentPoint).sqrMagnitude > 1f)
                {
                    float midAngle = -halfFov + (i - 0.5f) * angleStep;
                    Vector3 midDirection = Quaternion.Euler(0, midAngle, 0) * forwardDirection;

                    if (CheckObstacle(originPosition, midDirection, viewDistance, out RaycastHit midHit))
                    {
                        Vector3 midPoint = midHit.point;

                        float prevDistance = (prevPoint - originPosition).sqrMagnitude;
                        float midDistance = (midPoint - originPosition).sqrMagnitude;
                        float currentDistance = (currentPoint - originPosition).sqrMagnitude;

                        finalPoint = prevPoint;
                        if (midDistance < prevDistance) finalPoint = midPoint;
                        if (currentDistance < (finalPoint - originPosition).sqrMagnitude) finalPoint = currentPoint;
                    }
                }
            }

            vertices[vertexIndex] = transform.InverseTransformPoint(finalPoint);

            if (i > 0)
            {
                triangles[triangleIndex] = 0;
                triangles[triangleIndex + 1] = vertexIndex - 1;
                triangles[triangleIndex + 2] = vertexIndex;
                triangleIndex += 3;
            }
            vertexIndex++;
        }

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }

    /// <summary>
    /// Возращает точку конца луча
    /// </summary>
    /// <param name="origin"></param>
    /// <param name="direction"></param>
    /// <returns></returns>
    private Vector3 CastRay(Vector3 origin, Vector3 direction)
        => CheckObstacle(origin, direction, viewDistance, out RaycastHit hit)
            ? hit.point
            : origin + direction * viewDistance;
}