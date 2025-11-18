using UnityEngine;

public class FieldOfView : MonoBehaviour
{ [SerializeField] float viewDistance = 10f;
  [SerializeField] float fov = 90f;  
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Mesh mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        
        Vector3 origin = Vector3.zero; 
        int rayCount = 25;
        float angle = 0f;
        float angleIncrease = fov / rayCount;
        

        Vector3[] vertices = new Vector3[rayCount + 2];
        Vector2[] uv = new Vector2[vertices.Length];
        int[] triangles = new int[rayCount * 3];

        vertices[0] = origin;
        int vInd = 1;
        int tInd = 0;
        for (int i = 0; i <= rayCount; i++)
        {
            Vector3 vertex = origin + GetVectorFromAngle3D(angle) * viewDistance;
            vertices[vInd] = vertex;

            if (i > 0)
            {
                triangles[tInd + 0] = 0;
                triangles[tInd + 1] = vInd - 1;
                triangles[tInd + 2] = vInd;
                tInd += 3;
            }

            vInd++;
            angle -= angleIncrease;
        }
        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }

    public static Vector3 GetVectorFromAngle3D(float angle)
    {
        float angleRad = angle * (Mathf.PI / 180f);
        return new Vector3(Mathf.Cos(angleRad), 0f, Mathf.Sin(angleRad));
    }
}
