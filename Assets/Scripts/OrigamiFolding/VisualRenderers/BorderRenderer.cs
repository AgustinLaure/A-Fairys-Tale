using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class ShapeBorderRenderer : MonoBehaviour
{
    private LineRenderer lineRenderer;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.loop = true;

        lineRenderer.useWorldSpace = false;
    }

    public void Setup(float width, Material mat)
    {
        if (lineRenderer == null) lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.startWidth = width;
        lineRenderer.endWidth = width;
        if (mat != null) lineRenderer.material = mat;
    }

    public void DrawShape(List<Vector2> shape)
    {
        if (shape == null || shape.Count == 0)
        {
            lineRenderer.positionCount = 0;
            return;
        }

        lineRenderer.positionCount = shape.Count;
        for (int i = 0; i < shape.Count; i++)
        {
            lineRenderer.SetPosition(i, new Vector3(shape[i].x, shape[i].y, 0f));
        }
    }

    public void SetColor(Color color)
    {
        if (lineRenderer == null) lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
    }
}