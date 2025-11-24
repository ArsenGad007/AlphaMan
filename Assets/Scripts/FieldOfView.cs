using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class FieldOfView : MonoBehaviour
{
    [SerializeField] private float viewDistance = 10f;
    [SerializeField] private float fov = 90f;          
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField, Range(20, 100)] private int rayCount = 50;
    [SerializeField, Range(0f, 1f)] private float updateInterval = 0.05f;
    [SerializeField] private GameObject player;
    [SerializeField] private GameOver gameOver;

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

        Vector3[] vertices = new Vector3[rayCount + 2];
        Vector2[] uvs = new Vector2[vertices.Length];
        int[] triangles = new int[rayCount * 3];


        vertices[0] = transform.InverseTransformPoint(originPosition);

        for (int i = 0; i <= rayCount; i++)
        {
            float angle = -halfFov + i * angleStep;
            Vector3 rayDirection = Quaternion.Euler(0, angle, 0) * forwardDirection.normalized;

            if (Physics.Raycast(originPosition, rayDirection, out RaycastHit hit, viewDistance, obstacleMask))
            {
                vertices[i + 1] = transform.InverseTransformPoint(hit.point);
            }
            else
            {
                Vector3 globalEndPoint = originPosition + rayDirection * viewDistance;
                vertices[i + 1] = transform.InverseTransformPoint(globalEndPoint);
            }

            if (i > 0)
            {
                int idx = (i - 1) * 3;
                triangles[idx] = 0;
                triangles[idx + 1] = i;
                triangles[idx + 2] = i + 1;
            }
            
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