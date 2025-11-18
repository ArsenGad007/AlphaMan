using UnityEngine;

public class FieldOfView : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
            Mesh mesh = new Mesh();
            GetComponent<MeshFilter>().mesh = mesh;

            Vector3[] vertices = new Vector3[3];
            Vector2[] uv = new Vector2[3];
            int[] triangles = new int[3];

            vertices[0] = Vector3.zero;      //центр
            vertices[1] = new Vector3(5, 0, 1);   // вправо
            vertices[2] = new Vector3(1, 0, -5);  // назад

            triangles[0] = 0;
            triangles[1] = 1;
            triangles[2] = 2;

            mesh.vertices = vertices;
            mesh.uv = uv;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
    }
}
