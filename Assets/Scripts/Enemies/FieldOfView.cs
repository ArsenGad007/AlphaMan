using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class FieldOfView : MonoBehaviour
{
    public Transform PlayerTransform => player?.transform;
    [SerializeField] private float viewDistance = 5f;
    [SerializeField] private float fov = 90f;
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField, Range(20, 100)] private int rayCount = 80;
    [SerializeField, Range(0f, 1f)] private float updateInterval = 0.001f;
    [SerializeField] private GameObject player;
    //[SerializeField] private GameOver gameOver;
    private Renderer fieldOfViewRenderer;

    [SerializeField] private float detectionRadius = 3f;
    public float alertDelay = 1f;

    private Mesh mesh;
    private Vector3 lastPosition = Vector3.zero;
    private Vector3 lastForward = Vector3.forward;
    private float lastUpdateTime;
    private int detectionFramesRequired = 2;
    private int visibleFramesCount = 0;
    private float raycastForwardOffset = 1f;

    void Awake()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        fieldOfViewRenderer = GetComponent<Renderer>();

    }
    /// <summary>
    /// Смена цвета конуса зрения.
    /// Используется для индикации состояния.
    /// </summary>
    public void SetMaterial(Material mat)
    {
        fieldOfViewRenderer.sharedMaterial = mat;
    }
    /// <summary>
    /// Обновляет визуальный меш и проверяет обнаружение игрока.
    /// </summary>
    public void UpdateFOV(Vector3 originPosition, Vector3 forwardDirection)
    {
        if (Time.time - lastUpdateTime < updateInterval)
            return;

        Vector3 forward = forwardDirection;
        forward.y = 0f;
        if (forward == Vector3.zero) forward = Vector3.forward;

        bool shouldUpdate =
            Vector3.Distance(originPosition, lastPosition) > 0.1f ||
            Vector3.Angle(lastForward, forward) > 2f;

        if (!shouldUpdate) return;

        RebuildMesh(originPosition, forward);
        lastPosition = originPosition;
        lastForward = forward;
        lastUpdateTime = Time.time;
    }
    /// <summary>
    /// Генерирует Mesh для визуализации конуса зрения.
    /// </summary>
    private void RebuildMesh(Vector3 originPosition, Vector3 forwardDirection)
    {
        float halfFov = fov * 0.5f;
        float angleStep = fov / rayCount;
        //  const int circleSegments = 32;

        int coneVerticesCount = rayCount + 2;
        //  int circleVerticesCount = circleSegments + 1; 

        Vector3[] vertices = new Vector3[coneVerticesCount];
        Vector2[] uvs = new Vector2[vertices.Length];
        int[] triangles = new int[rayCount * 3];

        int vertexIndex = 0;
        int triangleIndex = 0;

        //конус
        vertices[vertexIndex] = transform.InverseTransformPoint(originPosition);
        vertexIndex++;

        for (int i = 0; i <= rayCount; i++)
        {
            float angle = -halfFov + i * angleStep;
            Vector3 rayDirection = Quaternion.Euler(0, angle, 0) * forwardDirection.normalized;

            // интерполяция
            Vector3 finalPoint;

            if (i == 0)
            {
                if (Physics.Raycast(originPosition, rayDirection, out RaycastHit hit, viewDistance, obstacleMask, QueryTriggerInteraction.Ignore))
                {
                    finalPoint = hit.point;
                }
                else
                {
                    finalPoint = originPosition + rayDirection * viewDistance;
                }
            }
            else
            {
                Vector3 prevPoint = vertices[vertexIndex - 1];
                Vector3 currentPoint;

                if (Physics.Raycast(originPosition, rayDirection, out RaycastHit hit, viewDistance, obstacleMask, QueryTriggerInteraction.Ignore))
                {
                    currentPoint = hit.point;
                }
                else
                {
                    currentPoint = originPosition + rayDirection * viewDistance;
                }

                if (Vector3.Distance(prevPoint, currentPoint) > 1f)
                {
                    float midAngle = -halfFov + (i - 0.5f) * angleStep;
                    Vector3 midDirection = Quaternion.Euler(0, midAngle, 0) * forwardDirection.normalized;

                    if (Physics.Raycast(originPosition, midDirection, out RaycastHit midHit, viewDistance, obstacleMask, QueryTriggerInteraction.Ignore))
                    {
                        Vector3[] candidates = { prevPoint, midHit.point, currentPoint };
                        finalPoint = candidates.OrderBy(p => Vector3.Distance(p, originPosition)).First();
                    }
                    else
                    {
                        finalPoint = currentPoint;
                    }
                }
                else
                {
                    finalPoint = currentPoint;
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

        //круг
        /*int circleCenterIndex = vertexIndex;
        vertices[vertexIndex] = transform.InverseTransformPoint(originPosition); 
        vertexIndex++;

        for (int i = 0; i < circleSegments; i++)
        {
            float angle = (i / (float)circleSegments) * 360f;
            Vector3 circleDirection = Quaternion.Euler(0, angle, 0) * Vector3.forward;
            if (Physics.Raycast(originPosition, circleDirection, out RaycastHit hit, detectionRadius, obstacleMask))
            {
                vertices[vertexIndex] = transform.InverseTransformPoint(hit.point);
            }
            else
            {
                Vector3 circlePoint = originPosition + circleDirection * detectionRadius;
                vertices[vertexIndex] = transform.InverseTransformPoint(circlePoint);
            }
            vertexIndex++;
        }
    

        for (int i = 0; i < circleSegments; i++)
        {
            int currentVertex = circleCenterIndex + 1 + i;
            int nextVertex = circleCenterIndex + 1 + ((i + 1) % circleSegments);

            triangles[triangleIndex] = circleCenterIndex; 
            triangles[triangleIndex + 1] = currentVertex; 
            triangles[triangleIndex + 2] = nextVertex;    
            triangleIndex += 3;
        }*/

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }
    /// <summary>
    /// Проверяет, мешает ли стена между охранником и игроком
    /// </summary>
    private bool IsPlayerBlocked(float distance)
    {
        if (player == null) return true;

        Vector3 dir = player.transform.position - transform.position;
        dir.y = 0f;
        float distMag = dir.magnitude;

        if (distMag > distance) return true;
        //if (distMag < 0.05f) return false;   

        float[] heights = { 0.6f, 1.0f, 1.7f };

        foreach (float h in heights)
        {
            Vector3 rayOrigin = transform.position - transform.forward * raycastForwardOffset + Vector3.up * h;

            if (Physics.Raycast(rayOrigin, dir.normalized, out RaycastHit hit, distMag, obstacleMask, QueryTriggerInteraction.Ignore))
            {
                if (hit.distance < 0.12f)
                    continue;
                if (hit.collider.gameObject != player)
                    return true;
            }
        }
        return false;
    }
    private bool? cachedBlockedResult;
    private int cachedBlockedFrame = -1;
    private float cachedBlockedDistance = -1f;

    /// <summary>
    /// Кешированная проверка: есть ли стена между охранником и игроком.
    /// </summary>                                          
    private bool IsPlayerBlockedCached(float distance)
    {
        if (cachedBlockedFrame == Time.frameCount &&
            Mathf.Approximately(cachedBlockedDistance, distance))
        {
            return cachedBlockedResult.Value;
        }
        bool result = IsPlayerBlocked(distance);
        cachedBlockedResult = result;
        cachedBlockedFrame = Time.frameCount;
        cachedBlockedDistance = distance;

        return result;
    }
    /// <summary>
    ///  Простая проверка видимости
    /// </summary>
    /// <returns></returns>
    public bool IsPlayerVisible()
    {
        if (player == null) return false;

        float dist = Vector3.Distance(transform.position, player.transform.position);
        if (dist > viewDistance) return false;
        if (Vector3.Angle(transform.forward, player.transform.position - transform.position) > fov * 0.5f) return false;

        return !IsPlayerBlocked(viewDistance);
    }
    /// <summary>
    /// Помогает определить, какая дб задержка.
    /// </summary>
    public DetectionType CheckForDetection()
    {
        if (player == null)
        {
            visibleFramesCount = 0;
            return DetectionType.None;
        }

        float distance = Vector3.Distance(transform.position, player.transform.position);
        Vector3 directionToPlayer = player.transform.position - transform.position;
        directionToPlayer.y = 0f;

        bool isVisible = false;
        if (distance <= detectionRadius)
        {
            bool bothNearWall = IsNearWall(0.4f) && IsPlayerNearWall(0.4f);

            if (!IsPlayerBlocked(detectionRadius))
            {
                if (bothNearWall)
                {
                    Vector3 extraOrigin = transform.position - transform.forward * raycastForwardOffset + Vector3.up * 1.3f;

                    if (Physics.Raycast(extraOrigin, directionToPlayer.normalized, out RaycastHit extraHit, distance, obstacleMask, QueryTriggerInteraction.Ignore))
                    {
                        if (extraHit.collider.gameObject != player)
                        {
                        }
                        else
                        {
                            isVisible = true;
                        }
                    }
                    else
                    {
                        isVisible = true;
                    }
                }
                else
                {
                    isVisible = true;
                }
            }
        }
        else if (distance <= viewDistance &&
                 Vector3.Angle(transform.forward, directionToPlayer.normalized) <= fov * 0.5f)
        {
            if (!IsPlayerBlocked(viewDistance))
                isVisible = true;
        }
        if (isVisible)
        {
            visibleFramesCount++;
            if (visibleFramesCount >= detectionFramesRequired)
            {
                return (distance <= detectionRadius)
                    ? DetectionType.InstantDeath
                    : DetectionType.AlertDelay;
            }
        }
        else
        {
            visibleFramesCount = 0;
        }

        return DetectionType.None;
    }
    /// <summary>Проверяет, прижат ли охранник к стене</summary>
    private bool IsNearWall(float distance)
    {
        Vector3 origin = transform.position - transform.forward * raycastForwardOffset + Vector3.up * 1.0f;
        return Physics.Raycast(origin, transform.forward, distance, obstacleMask, QueryTriggerInteraction.Ignore) ||
               Physics.Raycast(origin, -transform.forward, distance, obstacleMask, QueryTriggerInteraction.Ignore);
    }
    /// <summary>Проверяет, прижат ли игрок к стене</summary>
    private bool IsPlayerNearWall(float distance)
    {
        if (player == null) return false;
        Vector3 toPlayer = player.transform.position - transform.position;
        toPlayer.y = 0f;
        Vector3 playerOrigin = player.transform.position - toPlayer.normalized * raycastForwardOffset + Vector3.up * 1.0f;

        return Physics.Raycast(playerOrigin, toPlayer.normalized, distance, obstacleMask, QueryTriggerInteraction.Ignore) ||
               Physics.Raycast(playerOrigin, -toPlayer.normalized, distance, obstacleMask, QueryTriggerInteraction.Ignore);
    }

    /// <summary>
    /// Типы дистанции/реакции на игрока.
    /// </summary>
    public enum DetectionType
    {
        None,
        AlertDelay,
        InstantDeath
    }
}