using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class PaperFace : MonoBehaviour
{
    public List<Vector2> vertices = new List<Vector2>();
    public List<Vector2> uvs = new List<Vector2>();

    private MeshFilter meshFilter;

    private void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshFilter.mesh = new Mesh { name = "PaperMesh" };
    }

    public void UpdateMesh()
    {
        if (vertices == null || vertices.Count < 3) return;

        Mesh mesh = meshFilter.sharedMesh;
        mesh.Clear();

        mesh.vertices = vertices.Select(v => new Vector3(v.x, v.y, 0f)).ToArray();
        mesh.triangles = GeometryMath.TriangulateConvex(vertices);

        mesh.uv = vertices.Select((v, i) => (uvs != null && i < uvs.Count) ? uvs[i] : v).ToArray();

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }
}