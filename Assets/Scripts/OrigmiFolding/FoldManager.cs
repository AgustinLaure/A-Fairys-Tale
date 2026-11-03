using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FoldManager : MonoSingleton<FoldManager>
{
    public float foldTolerance = 0.2f;

    public Material paperMaterial;
    public OrigamiFigure currentFigure;
    public List<PaperFace> activeFaces = new List<PaperFace>();

    private int currentObjectiveIndex = 0;
    private List<Vector2> currentTargetShape = new List<Vector2>();
    private EventBus eventBus;

    private void Awake()
    {
        eventBus = ServiceLocator.Instance.GetService<EventBus>();
    }

    private void Start() => StartUp();

    private void OnDestroy()
    {
        activeFaces.Clear();
    }

    public void StartUp()
    {
        if (activeFaces.Count == 0) CreateInitialPaper(new Vector2(-5, -5), new Vector2(5, 5));

        if (currentFigure == null || currentFigure.steps.Count == 0)
        {
            currentFigure = ScriptableObject.CreateInstance<OrigamiFigure>();
            currentFigure.figureName = "Diamond";
            currentFigure.steps.Add(new OrigamiObjective
            {
                stepName = "Step 1",
                targetShape = new List<Vector2> { new Vector2(0, 5), new Vector2(-5, 0), new Vector2(0, -5), new Vector2(5, 0) }
            });
        }

        LoadObjective(0);
    }

    private void LoadObjective(int index)
    {
        currentObjectiveIndex = index;
        OrigamiObjective step = currentFigure.steps[index];
        currentTargetShape = step.targetShape;

        Debug.Log($"<color=#00FFFF>Loading Step {index + 1}: {step.stepName}</color>");

        if (step.flipStep)
        {
            OrigamiDebugVisuals.Instance?.UpdateTargetLimits(null);
            OrigamiDebugVisuals.Instance?.UpdateCheckpoints(null);
            StartCoroutine(FlipPaperCoroutine());
            return;
        }

        OrigamiDebugVisuals.Instance?.UpdateTargetLimits(currentTargetShape);
        OrigamiDebugVisuals.Instance?.UpdateCheckpoints(step.layerCheckpoints);
        OrigamiDebugVisuals.Instance?.SetTargetLimitColors(Color.red, Color.red);
    }

    private System.Collections.IEnumerator FlipPaperCoroutine()
    {
        var controller = FindFirstObjectByType<FoldController>();
        if (controller != null) controller.isInputBlocked = true;

        float minX = float.MaxValue, maxX = float.MinValue;
        foreach (var f in activeFaces)
        {
            foreach (var v in f.vertices)
            {
                if (v.x < minX) minX = v.x;
                if (v.x > maxX) maxX = v.x;
            }
        }
        float centerX = (minX + maxX) / 2f;

        GameObject pivot = new GameObject("FlipPivot");
        pivot.transform.position = new Vector3(centerX, 0, 0);

        foreach (var face in activeFaces) face.transform.SetParent(pivot.transform, true);

        float duration = 0.8f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            pivot.transform.rotation = Quaternion.Euler(0, Mathf.Lerp(0, 180f, t), 0);
            yield return null;
        }

        foreach (var face in activeFaces)
        {
            face.transform.SetParent(transform, true);
            face.transform.rotation = Quaternion.identity;

            for (int i = 0; i < face.vertices.Count; i++)
            {
                Vector2 v = face.vertices[i];
                face.vertices[i] = new Vector2(centerX - (v.x - centerX), v.y);
            }

            face.vertices.Reverse();
            if (face.uvs != null && face.uvs.Count > 0) face.uvs.Reverse();

            face.UpdateMesh();
        }

        var sortedFaces = activeFaces.OrderByDescending(f => f.transform.position.z).ToList();
        for (int i = 0; i < sortedFaces.Count; i++)
        {
            sortedFaces[i].transform.position = new Vector3(0, 0, -0.01f * (i + 1));
        }

        Destroy(pivot);

        if (controller != null) controller.isInputBlocked = false;

        CompleteCurrentStep();
    }

    private void CreateInitialPaper(Vector2 bottomLeft, Vector2 topRight)
    {
        activeFaces.Add(CreateNewFace(
            new List<Vector2> { bottomLeft, new Vector2(bottomLeft.x, topRight.y), topRight, new Vector2(topRight.x, bottomLeft.y) },
            new List<Vector2> { new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0) },
            "BasePaper"
        ));
    }

    public PaperFace GetTopFaceAt(Vector2 point)
    {
        return activeFaces
            .Where(f => IsPointInsidePolygon(point, f.vertices, 0.05f))
            .OrderBy(f => f.transform.position.z)
            .FirstOrDefault();
    }

    public void ProcessFold(Vector2 startLine, Vector2 endLine)
    {
        var candidateFaces = GetTargetedFaces(startLine, endLine);
        var newFaces = new List<PaperFace>();
        var facesToRemove = new List<PaperFace>();

        foreach (var face in candidateFaces)
        {
            if (TrySplitAndReflectFace(face, startLine, endLine, out PaperFace leftFace, out PaperFace rightFace))
            {
                facesToRemove.Add(face);
                if (leftFace != null) newFaces.Add(leftFace);
                if (rightFace != null)
                {
                    rightFace.transform.position = new Vector3(0, 0, -0.01f * (activeFaces.Count + newFaces.Count + 1));
                    newFaces.Add(rightFace);
                }
            }
        }

        foreach (var oldFace in facesToRemove)
        {
            activeFaces.Remove(oldFace);
            Destroy(oldFace.gameObject);
        }

        activeFaces.AddRange(newFaces);
        EvaluateWinCondition();
    }

    private List<PaperFace> GetTargetedFaces(Vector2 startLine, Vector2 endLine)
    {
        int layersToGrab = currentFigure.steps[currentObjectiveIndex].layersToGrab;

        var candidates = activeFaces
            .Where(f => f.vertices.Any(v => GeometryMath.PointLinePosition(v, startLine, endLine) < -0.001f))
            .OrderBy(f => f.transform.position.z)
            .ToList();

        if (layersToGrab != -1 && layersToGrab < candidates.Count)
        {
            return candidates.Take(layersToGrab).ToList();
        }
        return candidates;
    }

    private bool TrySplitAndReflectFace(PaperFace face, Vector2 start, Vector2 end, out PaperFace leftFace, out PaperFace rightFace)
    {
        leftFace = null;
        rightFace = null;

        var leftVerts = new List<Vector2>(); var leftUVs = new List<Vector2>();
        var rightVerts = new List<Vector2>(); var rightUVs = new List<Vector2>();

        Vector2 lineDir = (end - start).normalized;
        Vector2 p3 = start - lineDir * 1000f;
        Vector2 p4 = end + lineDir * 1000f;

        for (int i = 0; i < face.vertices.Count; i++)
        {
            int nextIdx = (i + 1) % face.vertices.Count;
            Vector2 p1 = face.vertices[i], p2 = face.vertices[nextIdx];
            Vector2 uv1 = (face.uvs != null && face.uvs.Count > i) ? face.uvs[i] : p1;
            Vector2 uv2 = (face.uvs != null && face.uvs.Count > nextIdx) ? face.uvs[nextIdx] : p2;

            if (GeometryMath.PointLinePosition(p1, start, end) >= 0)
            {
                leftVerts.Add(p1); leftUVs.Add(uv1);
            }
            else
            {
                rightVerts.Add(p1); rightUVs.Add(uv1);
            }

            if (GeometryMath.GetIntersection(p1, p2, p3, p4, out Vector2 intersect, out float t))
            {
                Vector2 intersectUV = Vector2.Lerp(uv1, uv2, t);
                leftVerts.Add(intersect); leftUVs.Add(intersectUV);
                rightVerts.Add(intersect); rightUVs.Add(intersectUV);
            }
        }

        if (rightVerts.Count < 3) return false;

        if (leftVerts.Count >= 3)
        {
            leftFace = CreateNewFace(leftVerts, leftUVs, $"{face.name}_Left");
        }

        var foldedVerts = new List<Vector2>();
        var foldedUVs = new List<Vector2>();

        for (int i = rightVerts.Count - 1; i >= 0; i--)
        {
            foldedVerts.Add(GeometryMath.ReflectPoint(rightVerts[i], start, end));
            foldedUVs.Add(rightUVs[i]);
        }

        rightFace = CreateNewFace(foldedVerts, foldedUVs, $"{face.name}_Folded");
        return true;
    }

    private PaperFace CreateNewFace(List<Vector2> verts, List<Vector2> uvs, string name)
    {
        var go = new GameObject(name);
        go.transform.SetParent(transform);
        go.AddComponent<MeshRenderer>().material = paperMaterial;

        var face = go.AddComponent<PaperFace>();
        face.vertices = verts;
        face.uvs = uvs;
        face.UpdateMesh();

        return face;
    }

    private void EvaluateWinCondition()
    {
        if (currentObjectiveIndex >= currentFigure.steps.Count) return;

        OrigamiObjective step = currentFigure.steps[currentObjectiveIndex];
        if (step.flipStep) return;

        bool failedUpper = HasFailedUpperLimit(foldTolerance);
        bool failedLower = HasFailedLowerLimit(foldTolerance);
        bool failedLayers = HasFailedLayerCheck(step.layerCheckpoints, foldTolerance);

        Color maxColor = failedUpper ? Color.red : Color.green;
        Color minColor = failedLower ? Color.red : Color.blue;

        OrigamiDebugVisuals.Instance?.SetTargetLimitColors(maxColor, minColor);

        if (!failedUpper && !failedLower && !failedLayers)
        {
            CompleteCurrentStep();
        }
    }

    private void CompleteCurrentStep()
    {
        Debug.Log($"<color=#00FF00>Step {currentObjectiveIndex + 1} Completed!</color>");

        currentObjectiveIndex++;
        if (currentObjectiveIndex < currentFigure.steps.Count)
        {
            LoadObjective(currentObjectiveIndex);
        }
        else
        {
            eventBus.Raise<OnOrigamiFolded>(currentFigure);
            OrigamiDebugVisuals.Instance?.SetTargetLimitColors(Color.green, Color.green);
            Debug.Log("<color=#FFFF00>Origami Figure Complete!</color>");
        }
    }

    private bool HasFailedUpperLimit(float tolerance)
    {
        return activeFaces.Any(face => face.vertices.Any(v => !IsPointInsidePolygon(v, currentTargetShape, tolerance)));
    }

    private bool HasFailedLowerLimit(float tolerance)
    {
        return currentTargetShape.Any(corner => !activeFaces.Any(f => IsPointInsidePolygon(corner, f.vertices, tolerance)));
    }

    private bool HasFailedLayerCheck(List<LayerCheckpoint> checkpoints, float tolerance)
    {
        if (checkpoints == null || checkpoints.Count == 0) return false;

        bool failed = false;
        for (int i = 0; i < checkpoints.Count; i++)
        {
            int layerCount = activeFaces.Count(f => IsPointInsidePolygon(checkpoints[i].position, f.vertices, tolerance));
            bool isCorrect = (layerCount == checkpoints[i].requiredLayers);

            OrigamiDebugVisuals.Instance?.SetCheckpointColor(i, isCorrect ? Color.green : Color.yellow);

            if (!isCorrect)
            {
                Debug.LogWarning($"<color=#FFA500>Failed Layer Check [{i}] at {checkpoints[i].position}: Expected {checkpoints[i].requiredLayers} layers, Found {layerCount} layers.</color>");
                failed = true;
            }
        }
        return failed;
    }

    private bool IsPointInsidePolygon(Vector2 point, List<Vector2> polygon, float tolerance)
    {
        if (polygon == null || polygon.Count < 3) return false;

        bool hasPositive = false;
        bool hasNegative = false;
        bool failedConvex = false;

        for (int i = 0; i < polygon.Count; i++)
        {
            Vector2 a = polygon[i];
            Vector2 b = polygon[(i + 1) % polygon.Count];
            float side = GeometryMath.PointLinePosition(point, a, b);

            if (side > tolerance) hasPositive = true;
            else if (side < -tolerance) hasNegative = true;

            if (hasPositive && hasNegative)
            {
                failedConvex = true;
                break;
            }
        }

        if (!failedConvex) return true;

        bool isInside = false;
        int j = polygon.Count - 1;

        for (int i = 0; i < polygon.Count; i++)
        {
            Vector2 pi = polygon[i];
            Vector2 pj = polygon[j];

            if (((pi.y > point.y) != (pj.y > point.y)) &&
                (point.x < (pj.x - pi.x) * (point.y - pi.y) / (pj.y - pi.y) + pi.x))
            {
                isInside = !isInside;
            }
            j = i;
        }

        if (isInside) return true;

        if (tolerance > 0f)
        {
            float sqrTol = tolerance * tolerance;
            for (int i = 0; i < polygon.Count; i++)
            {
                Vector2 a = polygon[i];
                Vector2 b = polygon[(i + 1) % polygon.Count];
                if (GeometryMath.SqrDistanceToSegment(point, a, b) <= sqrTol) return true;
            }
        }

        return false;
    }
}