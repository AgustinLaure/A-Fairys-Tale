using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ShapeRenderer : MonoBehaviour
{
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private LineRenderer lineRenderer;

    private GameObject shadowObject;
    private MeshFilter shadowMeshFilter;
    private MeshRenderer shadowMeshRenderer;

    public Vector2 shadowOffset = new Vector2(0.08f, -0.08f);

    private void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();

        if (meshFilter.mesh == null)
        {
            meshFilter.mesh = new Mesh { name = "ShapeMesh" };
        }

        shadowObject = new GameObject("DropShadow");
        shadowObject.transform.SetParent(transform, false);
        shadowMeshFilter = shadowObject.AddComponent<MeshFilter>();
        shadowMeshRenderer = shadowObject.AddComponent<MeshRenderer>();
        shadowMeshFilter.mesh = new Mesh { name = "ShadowMesh" };
        shadowObject.SetActive(false);
    }

    public void Setup(Material fillMat, Material borderMat = null, float borderWidth = 0f, Material shadowMat = null)
    {
        if (meshRenderer == null) meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.material = fillMat;

        if (shadowMat != null && shadowMeshRenderer != null)
        {
            shadowMeshRenderer.material = shadowMat;
            shadowObject.SetActive(true);
        }
        else if (shadowObject != null)
        {
            shadowObject.SetActive(false);
        }

        if (borderMat != null)
        {
            if (!TryGetComponent(out lineRenderer))
            {
                lineRenderer = gameObject.AddComponent<LineRenderer>();
            }
            lineRenderer.useWorldSpace = false;
            lineRenderer.loop = true;
            lineRenderer.startWidth = borderWidth;
            lineRenderer.endWidth = borderWidth;
            lineRenderer.material = borderMat;
        }
    }

    public void DrawShape(List<Vector2> shape, List<Vector2> uvs = null, float fillZOffset = 0f)
    {
        if (shape == null || shape.Count < 3) return;

        Mesh mesh = meshFilter.sharedMesh;
        mesh.Clear();

        mesh.vertices = shape.Select(v => new Vector3(v.x, v.y, fillZOffset)).ToArray();
        mesh.triangles = GeometryMath.TriangulateConvex(shape);

        if (uvs != null && uvs.Count == shape.Count)
            mesh.uv = uvs.Select(v => new Vector2(v.x, v.y)).ToArray();
        else
            mesh.uv = shape.Select(v => new Vector2(v.x, v.y)).ToArray();

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        if (shadowObject != null && shadowObject.activeSelf)
        {
            Mesh sMesh = shadowMeshFilter.sharedMesh;
            sMesh.Clear();

            sMesh.vertices = shape.Select(v => new Vector3(v.x + shadowOffset.x, v.y + shadowOffset.y, fillZOffset + 0.005f)).ToArray();
            sMesh.triangles = mesh.triangles;
            sMesh.RecalculateNormals();
            sMesh.RecalculateBounds();
        }

        if (lineRenderer != null)
        {
            lineRenderer.positionCount = shape.Count;
            for (int i = 0; i < shape.Count; i++)
            {
                lineRenderer.SetPosition(i, new Vector3(shape[i].x, shape[i].y, fillZOffset - 0.001f));
            }
        }
    }

    public void SetColor(Color color, float fillAlphaMultiplier = 1f)
    {
        if (meshRenderer != null && meshRenderer.material != null)
        {
            Color fillColor = color;
            fillColor.a *= fillAlphaMultiplier;
            meshRenderer.material.color = fillColor;
        }

        if (lineRenderer != null)
        {
            lineRenderer.startColor = color;
            lineRenderer.endColor = color;
        }
    }
}