using UnityEngine;

public class SecurityCameraFOV : MonoBehaviour
{
    [SerializeField] private float viewDistance = 8f;
    [SerializeField] private float fovAngle = 90f;
    [SerializeField] private int rayCount = 64;

    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private GameObject player;

    [SerializeField] private float lensOffset = 0.7f;
    [SerializeField] private float lensDrop = 1f;

    private Mesh mesh;

    void Awake()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
    }

    void Update()
    {
        Vector3 actualOrigin = transform.TransformDirection(Vector3.forward) * lensOffset + transform.position;

        UpdateFOV(actualOrigin, transform.forward);
    }

    public bool IsPlayerVisible()
    {
        if (player == null) return false;
        Vector3 rayStart = GetRayStartPoint();
        Vector3 dir = (player.transform.position - rayStart);
        float dist = dir.magnitude;

        if (dist > viewDistance) return false;

        if (Vector3.Angle(transform.forward, dir.normalized) > fovAngle * 0.5f) return false;

        return !Physics.Raycast(rayStart, dir.normalized, dist, obstacleMask);
    }

    private Vector3 GetRayStartPoint()
    {
        Vector3 forwardShift = transform.TransformDirection(Vector3.forward) * lensOffset;
        Vector3 downShift = Vector3.down * lensDrop;

        return transform.position + forwardShift + downShift;
    }

    private void UpdateFOV(Vector3 origin, Vector3 forward)
    {
        float halfFov = fovAngle * 0.5f;
        float step = fovAngle / rayCount;

        Vector3[] vertices = new Vector3[rayCount + 2];
        int[] triangles = new int[(rayCount) * 3];

        vertices[0] = transform.InverseTransformPoint(origin);

        for (int i = 1; i <= rayCount + 1; i++)
        {
            float angle = -halfFov + (i - 1) * step;
            Vector3 rayDir = Quaternion.Euler(0, angle, 0) * forward;

            if (Physics.Raycast(origin, rayDir, out RaycastHit hit, viewDistance, obstacleMask, QueryTriggerInteraction.Ignore))
                vertices[i] = transform.InverseTransformPoint(hit.point);
            else
                vertices[i] = transform.InverseTransformPoint(origin + rayDir * viewDistance);

            if (i > 1)
            {
                triangles[(i - 2) * 3 + 0] = 0;
                triangles[(i - 2) * 3 + 1] = i - 1;
                triangles[(i - 2) * 3 + 2] = i;
            }
        }

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }
}