using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class FieldOfView : MonoBehaviour
{
    [SerializeField] private float viewDistance = 10f;
    [SerializeField] private float fov = 90f;          
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField, Range(20, 100)] private int rayCount = 100;
    [SerializeField, Range(0f, 1f)] private float updateInterval = 0.0005f;
    [SerializeField] private GameObject player;
    [SerializeField] private GameOver gameOver;

    [SerializeField] private float detectionRadius = 1f;

    private Mesh mesh;
    private Vector3 lastPosition = Vector3.zero;
    private Vector3 lastForward = Vector3.forward;
    private float lastUpdateTime;

    void Awake()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
    }

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
        CheckForPlayer();
    }

    private void RebuildMesh(Vector3 originPosition, Vector3 forwardDirection)
    {
        float halfFov = fov * 0.5f;
        float angleStep = fov / rayCount;
        const int circleSegments = 32;

        int coneVerticesCount = rayCount + 2; 
        int circleVerticesCount = circleSegments + 1; 

        Vector3[] vertices = new Vector3[coneVerticesCount + circleVerticesCount];
        Vector2[] uvs = new Vector2[vertices.Length];
        int[] triangles = new int[rayCount * 3 + circleSegments * 3];

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
        int circleCenterIndex = vertexIndex;
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
        }

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }

    //проверка для обнаружения
    private void CheckForPlayer()
    {
        if (player == null) return;

        Vector3 directionToPlayer = player.transform.position - transform.position;
        directionToPlayer.y = 0f;
        float distanceToPlayer = directionToPlayer.magnitude;


        if (distanceToPlayer <= detectionRadius)
        {
            if (!Physics.Raycast(transform.position, directionToPlayer.normalized, out RaycastHit hitCircular, distanceToPlayer, obstacleMask) ||
                hitCircular.collider.gameObject == player)
            {
                OnPlayerDetected();
                return;
            }
        }


        if (distanceToPlayer > viewDistance) return;

        
        if (Vector3.Angle(transform.forward, directionToPlayer) > fov / 2f) return;

        
        if (Physics.Raycast(transform.position, directionToPlayer.normalized, out RaycastHit hit, distanceToPlayer, obstacleMask))
        {
           
            if (hit.collider.gameObject == player)
            {
                OnPlayerDetected();
            }
        }
        else
        {
            OnPlayerDetected();
        }
    }

    //что делать при обнаружении
    private void OnPlayerDetected()
    {
        Debug.Log("Охранник заметил игрока!");
        gameOver.GameOverPanel();
    }
}