using System.Collections.Generic;
using UnityEngine;

public static class GeometryMath
{
    public static float PointLinePosition(Vector2 p, Vector2 a, Vector2 b)
    {
        return (b.x - a.x) * (p.y - a.y) - (b.y - a.y) * (p.x - a.x);
    }

    public static Vector2 ReflectPoint(Vector2 p, Vector2 a, Vector2 b)
    {
        Vector2 lineDir = (b - a).normalized;
        Vector2 normal = new Vector2(-lineDir.y, lineDir.x);

        float distToLine = Vector2.Dot(p - a, normal);

        return p - 2f * distToLine * normal;
    }

    public static bool GetIntersection(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, out Vector2 intersection, out float t)
    {
        intersection = Vector2.zero;
        t = 0f;

        float denom = (p1.x - p2.x) * (p3.y - p4.y) - (p1.y - p2.y) * (p3.x - p4.x);
        if (Mathf.Abs(denom) < 0.0001f) return false;

        t = ((p1.x - p3.x) * (p3.y - p4.y) - (p1.y - p3.y) * (p3.x - p4.x)) / denom;
        float u = -((p1.x - p2.x) * (p1.y - p3.y) - (p1.y - p2.y) * (p1.x - p3.x)) / denom;

        if (t >= 0f && t <= 1f && u >= 0f && u <= 1f)
        {
            intersection = Vector2.Lerp(p1, p2, t);
            return true;
        }

        return false;
    }

    public static float SqrDistanceToSegment(Vector2 p, Vector2 a, Vector2 b)
    {
        Vector2 ab = b - a;
        float l2 = ab.sqrMagnitude;
        if (l2 == 0f) return (p - a).sqrMagnitude;

        float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / l2);
        Vector2 projection = a + t * ab;
        return (p - projection).sqrMagnitude;
    }

    public static int[] TriangulateConvex(List<Vector2> vertices)
    {
        if (vertices == null || vertices.Count < 3) return new int[0];

        int[] triangles = new int[(vertices.Count - 2) * 3];
        for (int i = 1; i < vertices.Count - 1; i++)
        {
            int triIndex = (i - 1) * 3;
            triangles[triIndex] = 0;
            triangles[triIndex + 1] = i;
            triangles[triIndex + 2] = i + 1;
        }
        return triangles;
    }
}