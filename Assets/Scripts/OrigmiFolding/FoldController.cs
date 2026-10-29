using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(LineRenderer))]
public class FoldController : MonoBehaviour
{
    [SerializeField] private bool enableGridSnap = true;
    [SerializeField] private float gridSpacing = 1f;
    [SerializeField] private float gridSize = 5f;

    [SerializeField] private bool enablePointSnap = true;
    [Min(1)]
    [SerializeField] private int edgeDivisions = 3;

    [SerializeField] private float snapRadius = 0.5f;
    [SerializeField] private float minPointDistance = 0.2f;

    [SerializeField] private bool enableAngleSnap = true;
    [SerializeField] private float angleSnapStep = 15f;
    [SerializeField] private float angleSnapTolerance = 10f;

    [SerializeField] private float minDragDistance = 0.2f;
    [SerializeField] private float intersectionTolerance = 0.05f;

    [SerializeField] private bool showSnapPoints = true;
    [SerializeField] private Color validLineColor = Color.white;
    [SerializeField] private Color invalidLineColor = Color.red;

    private Camera mainCam;
    private LineRenderer lineRenderer;

    private Vector2 startPos;
    private bool isDragging = false;
    private PaperFace currentTargetFace;

    private GameObject startRealMarker;
    private GameObject startSnapMarker;
    private GameObject endRealMarker;
    private GameObject endSnapMarker;

    private List<GameObject> magentaSnapMarkers = new List<GameObject>();

    private void Start()
    {
        mainCam = Camera.main;
        lineRenderer = GetComponent<LineRenderer>();

        lineRenderer.positionCount = 0;
        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = 0.05f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));

        startRealMarker = CreateMarker("StartReal", Color.blue);
        startSnapMarker = CreateMarker("StartSnap", Color.green);
        endRealMarker = CreateMarker("EndReal", Color.blue);
        endSnapMarker = CreateMarker("EndSnap", Color.green);
    }

    private void Update()
    {
        if (Mouse.current == null) return;

        UpdateMagentaMarkers();

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            isDragging = true;

            Vector2 rawStart = GetMouseWorldPosition();

            currentTargetFace = FoldManager.Instance.GetTopFaceAt(rawStart);

            startPos = GetSnappedPosition(rawStart);

            endRealMarker.SetActive(false);
            endSnapMarker.SetActive(false);

            startRealMarker.transform.position = new Vector3(rawStart.x, rawStart.y, -2f);
            startSnapMarker.transform.position = new Vector3(startPos.x, startPos.y, -2f);

            startRealMarker.SetActive(true);
            startSnapMarker.SetActive(true);
        }

        if (isDragging && Mouse.current.leftButton.isPressed)
        {
            Vector2 currentPos = GetSnappedPosition(GetMouseWorldPosition());

            if (Vector2.Distance(startPos, currentPos) > minDragDistance)
            {
                Vector2 foldStart, foldEnd;
                CalculateFoldLine(startPos, currentPos, out foldStart, out foldEnd);

                bool isValid = IsValidFold(foldStart, foldEnd);
                lineRenderer.startColor = isValid ? validLineColor : invalidLineColor;
                lineRenderer.endColor = isValid ? validLineColor : invalidLineColor;

                lineRenderer.positionCount = 2;
                lineRenderer.SetPosition(0, new Vector3(foldStart.x, foldStart.y, -1f));
                lineRenderer.SetPosition(1, new Vector3(foldEnd.x, foldEnd.y, -1f));
            }
            else
            {
                lineRenderer.positionCount = 0;
            }
        }

        if (isDragging && Mouse.current.leftButton.wasReleasedThisFrame)
        {
            isDragging = false;

            Vector2 rawEnd = GetMouseWorldPosition();
            Vector2 endPos = GetSnappedPosition(rawEnd);

            endRealMarker.transform.position = new Vector3(rawEnd.x, rawEnd.y, -2f);
            endSnapMarker.transform.position = new Vector3(endPos.x, endPos.y, -2f);

            endRealMarker.SetActive(true);
            endSnapMarker.SetActive(true);

            lineRenderer.positionCount = 0;

            if (Vector2.Distance(startPos, endPos) > minDragDistance)
            {
                Vector2 foldStart, foldEnd;
                CalculateFoldLine(startPos, endPos, out foldStart, out foldEnd);

                if (IsValidFold(foldStart, foldEnd))
                {
                    FoldManager.Instance.ProcessFold(foldStart, foldEnd, currentTargetFace);
                }
                else
                {
                    Debug.LogWarning("Canceled");
                }
            }
        }
    }

    private void CalculateFoldLine(Vector2 dragStart, Vector2 dragEnd, out Vector2 lineStart, out Vector2 lineEnd)
    {
        Vector2 midPoint = (dragStart + dragEnd) / 2f;
        Vector2 dragDir = (dragEnd - dragStart).normalized;
        dragDir = SnapDirection(dragDir);

        Vector2 perpDir = new Vector2(dragDir.y, -dragDir.x);
        float lineLength = 20f;
        lineStart = midPoint - perpDir * lineLength;
        lineEnd = midPoint + perpDir * lineLength;
    }

    private bool IsValidFold(Vector2 lineStart, Vector2 lineEnd)
    {
        if (FoldManager.Instance == null || FoldManager.Instance.activeFaces.Count == 0) return false;

        if (currentTargetFace != null)
        {
            return LineIntersectsOrTouchesFace(lineStart, lineEnd, currentTargetFace);
        }
        else
        {
            foreach (var face in FoldManager.Instance.activeFaces)
            {
                if (LineIntersectsOrTouchesFace(lineStart, lineEnd, face)) return true;
            }
            return false;
        }
    }

    private bool LineIntersectsOrTouchesFace(Vector2 lineStart, Vector2 lineEnd, PaperFace face)
    {
        int leftCount = 0;
        int rightCount = 0;
        int zeroCount = 0;

        for (int i = 0; i < face.vertices.Count; i++)
        {
            float side = (face.vertices[i].x - lineStart.x) * (lineEnd.y - lineStart.y) -
                         (face.vertices[i].y - lineStart.y) * (lineEnd.x - lineStart.x);

            if (side > intersectionTolerance) leftCount++;
            else if (side < -intersectionTolerance) rightCount++;
            else zeroCount++;
        }

        if (leftCount > 0 && rightCount > 0) return true;

        if (zeroCount >= 2) return true;

        return false;
    }

    private List<Vector2> GetAllValidSnapPoints()
    {
        List<Vector2> points = new List<Vector2>();

        if (enableGridSnap)
        {
            for (float x = -gridSize; x <= gridSize; x += gridSpacing)
            {
                for (float y = -gridSize; y <= gridSize; y += gridSpacing)
                {
                    TryAddSnapPoint(points, new Vector2(x, y));
                }
            }
        }

        if (enablePointSnap && FoldManager.Instance != null)
        {
            foreach (var face in FoldManager.Instance.activeFaces)
            {
                int vertCount = face.vertices.Count;
                for (int i = 0; i < vertCount; i++)
                {
                    Vector2 p1 = face.vertices[i];
                    Vector2 p2 = face.vertices[(i + 1) % vertCount];

                    for (int d = 0; d < edgeDivisions; d++)
                    {
                        float t = (float)d / edgeDivisions;
                        TryAddSnapPoint(points, Vector2.Lerp(p1, p2, t));
                    }
                }
            }
        }

        return points;
    }

    private void TryAddSnapPoint(List<Vector2> list, Vector2 newPoint)
    {
        foreach (Vector2 existingPoint in list)
        {
            if (Vector2.Distance(existingPoint, newPoint) < minPointDistance)
            {
                return;
            }
        }
        list.Add(newPoint);
    }

    private Vector2 GetSnappedPosition(Vector2 originalPos)
    {
        if (!enablePointSnap && !enableGridSnap) return originalPos;

        Vector2 closestPoint = originalPos;
        float closestDistance = snapRadius;
        bool foundSnap = false;

        List<Vector2> validPoints = GetAllValidSnapPoints();

        foreach (Vector2 snapCandidate in validPoints)
        {
            float dist = Vector2.Distance(originalPos, snapCandidate);
            if (dist < closestDistance)
            {
                closestDistance = dist;
                closestPoint = snapCandidate;
                foundSnap = true;
            }
        }

        return foundSnap ? closestPoint : originalPos;
    }

    private void UpdateMagentaMarkers()
    {
        if (!showSnapPoints)
        {
            foreach (var m in magentaSnapMarkers) m.SetActive(false);
            return;
        }

        List<Vector2> validPoints = GetAllValidSnapPoints();

        while (magentaSnapMarkers.Count < validPoints.Count)
        {
            magentaSnapMarkers.Add(CreateMarker("MagentaSnap", Color.magenta, 0.15f));
        }

        for (int i = 0; i < magentaSnapMarkers.Count; i++)
        {
            if (i < validPoints.Count)
            {
                magentaSnapMarkers[i].transform.position = new Vector3(validPoints[i].x, validPoints[i].y, -1.5f);
                magentaSnapMarkers[i].SetActive(true);
            }
            else
            {
                magentaSnapMarkers[i].SetActive(false);
            }
        }
    }

    private Vector2 SnapDirection(Vector2 dir)
    {
        if (!enableAngleSnap || angleSnapStep <= 0f) return dir;

        float currentAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        float snappedAngle = Mathf.Round(currentAngle / angleSnapStep) * angleSnapStep;

        if (Mathf.Abs(Mathf.DeltaAngle(currentAngle, snappedAngle)) <= angleSnapTolerance)
        {
            float rad = snappedAngle * Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)).normalized;
        }

        return dir;
    }

    private Vector2 GetMouseWorldPosition()
    {
        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        Vector3 worldPos = mainCam.ScreenToWorldPoint(new Vector3(mouseScreenPos.x, mouseScreenPos.y, Mathf.Abs(mainCam.transform.position.z)));
        return new Vector2(worldPos.x, worldPos.y);
    }

    private GameObject CreateMarker(string name, Color color, float size = 0.25f)
    {
        GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        marker.name = name;
        Destroy(marker.GetComponent<Collider>());
        marker.transform.localScale = new Vector3(size, size, size);

        Renderer rend = marker.GetComponent<Renderer>();
        Shader unlit = Shader.Find("Sprites/Default");
        if (unlit != null) rend.material.shader = unlit;
        rend.material.color = color;

        marker.SetActive(false);
        return marker;
    }
}