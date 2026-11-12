using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ShapeRenderer))]
public class PaperFace : MonoBehaviour
{
    public List<Vector2> vertices = new List<Vector2>();
    public List<Vector2> uvs = new List<Vector2>();

    private ShapeRenderer shapeRenderer;

    private void Awake()
    {
        shapeRenderer = GetComponent<ShapeRenderer>();
    }

    public void UpdateMesh()
    {
        if (shapeRenderer == null) shapeRenderer = GetComponent<ShapeRenderer>();
        shapeRenderer.DrawShape(vertices, uvs);
    }
}