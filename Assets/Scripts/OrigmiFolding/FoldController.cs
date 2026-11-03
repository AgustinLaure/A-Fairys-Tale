using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(LineRenderer))]
public class FoldController : MonoBehaviour
{
    [HideInInspector]
    public bool isInputBlocked = false;

    [Header("Snapping Settings")]
    [SerializeField] private bool enableGridSnap = true;
    [SerializeField] private float gridSpacing = 1f;
    [SerializeField] private bool enablePointSnap = true;
    [Min(1)][SerializeField] private int edgeDivisions = 3;
    [SerializeField] private float minPointDistance = 0.2f;

    [Header("Angle Snapping")]
    [SerializeField] private bool enableAngleSnap = true;
    [SerializeField] private float angleSnapStep = 15f;
    [SerializeField] private float angleSnapTolerance = 10f;

    [Header("Fold Settings")]
    [SerializeField] private float minDragDistance = 0.2f;
    [SerializeField] private float intersectionTolerance = 0.05f;

    [Header("Visuals")]
    [SerializeField] private Color validLineColor = Color.white;
    [SerializeField] private Color invalidLineColor = Color.red;

    private Camera mainCam;
    private LineRenderer lineRenderer;

    private FoldManager foldManager;

    private Vector2 startPos;
    private bool isDragging = false;

    private void Start()
    {
        mainCam = Camera.main;

        foldManager = (ServiceLocator.Instance.GetService<GameManager>().levelManager as FoldingLevelManager).foldManager;

        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 0;
        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = 0.05f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
    }

    private void Update()
    {
        if (Mouse.current == null || (FoldManager.Instance != null && FoldManager.Instance.IsFlipping)) return;

        OrigamiDebugVisuals.Instance?.UpdateSnapPoints(GetAllValidSnapPoints());
        HandleInput();
    }

    private void HandleInput()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            HandleDragStart();
        }
        else if (isDragging && Mouse.current.leftButton.isPressed)
        {
            HandleDragging();
        }
        else if (isDragging && Mouse.current.leftButton.wasReleasedThisFrame)
        {
            HandleDragEnd();
        }
    }

    private void HandleDragStart()
    {
        isDragging = true;
        Vector2 rawStart = GetMouseWorldPosition();
        startPos = GetSnappedPosition(rawStart);

        OrigamiDebugVisuals.Instance?.SetDragMarkers(rawStart, startPos, true, true);
        OrigamiDebugVisuals.Instance?.SetDragMarkers(Vector2.zero, Vector2.zero, false, false);
    }

    private void HandleDragging()
    {
        Vector2 currentPos = GetSnappedPosition(GetMouseWorldPosition());

        if (Vector2.Distance(startPos, currentPos) <= minDragDistance)
        {
            lineRenderer.positionCount = 0;
            return;
        }

        CalculateFoldLine(startPos, currentPos, out Vector2 foldStart, out Vector2 foldEnd);
        bool isValid = IsValidFold(foldStart, foldEnd);

        lineRenderer.startColor = isValid ? validLineColor : invalidLineColor;
        lineRenderer.endColor = isValid ? validLineColor : invalidLineColor;
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, new Vector3(foldStart.x, foldStart.y, -1f));
        lineRenderer.SetPosition(1, new Vector3(foldEnd.x, foldEnd.y, -1f));
    }

    private void HandleDragEnd()
    {
        isDragging = false;
        lineRenderer.positionCount = 0;

        Vector2 rawEnd = GetMouseWorldPosition();
        Vector2 endPos = GetSnappedPosition(rawEnd);

        OrigamiDebugVisuals.Instance?.SetDragMarkers(rawEnd, endPos, true, false);

        if (Vector2.Distance(startPos, endPos) > minDragDistance)
        {
            CalculateFoldLine(startPos, endPos, out Vector2 foldStart, out Vector2 foldEnd);

            if (IsValidFold(foldStart, foldEnd))
            {
                foldManager.ProcessFold(foldStart, foldEnd);
            }
            else
            {
                Debug.LogWarning("Invalid Fold Canceled");
            }
        }
    }

    private void CalculateFoldLine(Vector2 dragStart, Vector2 dragEnd, out Vector2 lineStart, out Vector2 lineEnd)
    {
        Vector2 midPoint = (dragStart + dragEnd) / 2f;
        Vector2 dragDir = SnapDirection((dragEnd - dragStart).normalized);

        Vector2 perpDir = new Vector2(dragDir.y, -dragDir.x) * 20f;

        lineStart = midPoint - perpDir;
        lineEnd = midPoint + perpDir;
    }

    private bool IsValidFold(Vector2 lineStart, Vector2 lineEnd)
    {
        foreach (var face in foldManager.activeFaces)
        {
            if (LineIntersectsOrTouchesFace(lineStart, lineEnd, face)) return true;
        }
        return false;
    }

    private bool LineIntersectsOrTouchesFace(Vector2 lineStart, Vector2 lineEnd, PaperFace face)
    {
        bool hasLeft = false, hasRight = false;
        int zeroCount = 0;

        foreach (Vector2 vertex in face.vertices)
        {
            float side = GeometryMath.PointLinePosition(vertex, lineStart, lineEnd);

            if (side > intersectionTolerance) hasLeft = true;
            else if (side < -intersectionTolerance) hasRight = true;
            else zeroCount++;
        }

        return (hasLeft && hasRight) || zeroCount >= 2;
    }

    private Vector2 GetSnappedPosition(Vector2 originalPos)
    {
        if (!enablePointSnap && !enableGridSnap) return originalPos;

        Vector2 closestPoint = originalPos;
        float closestDistance = float.MaxValue;
        bool foundSnap = false;

        foreach (Vector2 snapCandidate in GetAllValidSnapPoints())
        {
            float dist = Vector2.SqrMagnitude(originalPos - snapCandidate);
            if (dist < closestDistance)
            {
                closestDistance = dist;
                closestPoint = snapCandidate;
                foundSnap = true;
            }
        }

        return foundSnap ? closestPoint : originalPos;
    }

    private List<Vector2> GetAllValidSnapPoints()
    {
        List<Vector2> points = new List<Vector2>();

        if (foldManager == null)
        {
            Debug.LogError("No FoldingManager instance");
            return points;
        }

        if (enableGridSnap)
        {
            Vector2 extents = foldManager.paperSize / 2f;
            for (float x = -extents.x; x <= extents.x; x += gridSpacing)
            {
                for (float y = -extents.y; y <= extents.y; y += gridSpacing)
                {
                    TryAddSnapPoint(points, new Vector2(x, y));
                }
            }
        }

        if (enablePointSnap)
        {
            foreach (var face in foldManager.activeFaces)
            {
                int vertCount = face.vertices.Count;
                for (int i = 0; i < vertCount; i++)
                {
                    Vector2 p1 = face.vertices[i];
                    Vector2 p2 = face.vertices[(i + 1) % vertCount];

                    for (int d = 0; d < edgeDivisions; d++)
                    {
                        TryAddSnapPoint(points, Vector2.Lerp(p1, p2, (float)d / edgeDivisions));
                    }
                }
            }

            if (foldManager.CurrentTargetShape != null)
            {
                foreach (Vector2 targetPoint in foldManager.CurrentTargetShape)
                {
                    TryAddSnapPoint(points, targetPoint);
                }
            }
        }

        return points;
    }
    private void TryAddSnapPoint(List<Vector2> list, Vector2 newPoint)
    {
        float minSqrDist = minPointDistance * minPointDistance;
        foreach (Vector2 existingPoint in list)
        {
            if (Vector2.SqrMagnitude(existingPoint - newPoint) < minSqrDist) return;
        }
        list.Add(newPoint);
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
}