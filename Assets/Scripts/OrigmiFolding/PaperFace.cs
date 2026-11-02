using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class PaperFace : MonoBehaviour
{
    public List<Vector2> vertices = new List<Vector2>();
    public List<Vector2> uvs = new List<Vector2>();

    private MeshFilter meshFilter;
    private Mesh mesh;

    private void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        mesh = new Mesh();
        meshFilter.mesh = mesh;
    }

    public void UpdateMesh()
    {
        if (vertices.Count < 3) return;

        Vector3[] vertices3D = new Vector3[vertices.Count];
        for (int i = 0; i < vertices.Count; i++)
        {
            vertices3D[i] = new Vector3(vertices[i].x, vertices[i].y, 0f);
        }

        int[] triangles = GeometryMath.TriangulateConvex(vertices);

        Vector2[] meshUVs = new Vector2[vertices.Count];
        for (int i = 0; i < vertices.Count; i++)
        {
            if (uvs != null && i < uvs.Count)
            {
                meshUVs[i] = uvs[i];
            }
            else
            {
                meshUVs[i] = new Vector2(vertices[i].x, vertices[i].y);
            }
        }

        mesh.Clear();
        mesh.vertices = vertices3D;
        mesh.triangles = triangles;
        mesh.uv = meshUVs;

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }
}