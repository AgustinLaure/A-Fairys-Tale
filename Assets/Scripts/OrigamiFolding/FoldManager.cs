using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FoldManager : MonoSingleton<FoldManager>
{
    [Header("Settings")]
    public float foldTolerance = 0.2f;
    public Vector2 paperSize = new Vector2(10f, 10f);

    [Header("References")]
    public Material paperMaterial;
    public OrigamiFigure currentFigure;

    [Header("State")]
    public List<PaperFace> activeFaces = new List<PaperFace>();

    private int currentObjectiveIndex = 0;
    private List<Vector2> currentTargetShape = new List<Vector2>();
    private EventBus eventBus;

    public List<Vector2> CurrentTargetShape => currentTargetShape;
    public bool IsFlipping { get; private set; }
    public bool IsComplete => currentFigure == null || currentObjectiveIndex >= currentFigure.steps.Count;
    public bool IsLevelActive { get; private set; }

    private List<ShapeRenderer> previewRenderers = new List<ShapeRenderer>();

    private void Awake()
    {
        eventBus = ServiceLocator.Instance.GetService<EventBus>();
    }

    private void Start() => StartUp();

    private void OnDestroy()
    {
        activeFaces.Clear();
        if (previewRenderers != null)
        {
            foreach (var m in previewRenderers) if (m != null) Destroy(m);
        }
    }

    public void StartUp()
    {
        currentObjectiveIndex = 0;
        IsFlipping = false;
        IsLevelActive = true;

        foreach (var face in activeFaces)
        {
            if (face != null) Destroy(face.gameObject);
        }
        activeFaces.Clear();

        if (previewRenderers != null)
        {
            foreach (var m in previewRenderers) if (m != null) Destroy(m);
            previewRenderers.Clear();
        }

        if (paperSize.x <= 0.1f || paperSize.y <= 0.1f)
        {
            paperSize = new Vector2(10f, 10f);
        }

        if (activeFaces.Count == 0)
        {
            Vector2 extents = paperSize / 2f;
            CreateInitialPaper(new Vector2(-extents.x, -extents.y), new Vector2(extents.x, extents.y));
        }

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

        StepSequenceDisplay.Instance?.RenderSequence(currentFigure, currentObjectiveIndex);
    }

    private System.Collections.IEnumerator FlipPaperCoroutine()
    {
        IsFlipping = true;

        float centerX = 0f;

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

        IsFlipping = false;

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
        int layersToGrab = IsComplete ? -1 : currentFigure.steps[currentObjectiveIndex].layersToGrab;

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

    public void DrawFoldPreview(Vector2 startLine, Vector2 endLine)
    {
        int meshIndex = 0;
        var candidateFaces = GetTargetedFaces(startLine, endLine);

        foreach (var face in activeFaces) face.gameObject.SetActive(false);

        foreach (var face in activeFaces)
        {
            if (candidateFaces.Contains(face))
            {
                if (CalculateSplit(face, startLine, endLine, out var leftV, out var leftUV, out var rightV, out var rightUV))
                {
                    if (leftV.Count >= 3)
                        UpdatePreviewMesh(meshIndex++, leftV, leftUV, face.transform.position.z);
                    if (rightV.Count >= 3)
                        UpdatePreviewMesh(meshIndex++, rightV, rightUV, face.transform.position.z - 0.01f * (activeFaces.Count + meshIndex));
                }
                else
                {
                    UpdatePreviewMesh(meshIndex++, face.vertices, face.uvs, face.transform.position.z);
                }
            }
            else
            {
                UpdatePreviewMesh(meshIndex++, face.vertices, face.uvs, face.transform.position.z);
            }
        }

        for (int i = meshIndex; i < previewRenderers.Count; i++)
        {
            previewRenderers[i].gameObject.SetActive(false);
        }
    }

    public void Deactivate()
    {
        IsLevelActive = false;

        foreach (var face in activeFaces)
        {
            if (face != null) Destroy(face.gameObject);
        }
        activeFaces.Clear();

        if (previewRenderers != null)
        {
            foreach (var m in previewRenderers) if (m != null) Destroy(m);
            previewRenderers.Clear();
        }

        OrigamiDebugVisuals.Instance?.UpdateTargetLimits(null);
        OrigamiDebugVisuals.Instance?.UpdateCheckpoints(null);
        OrigamiDebugVisuals.Instance?.UpdateSnapPoints(new List<Vector2>());

        StepSequenceDisplay.Instance?.ClearSequence();
    }

    private void UpdatePreviewMesh(int index, List<Vector2> verts, List<Vector2> uvs, float zPos)
    {
        while (previewRenderers.Count <= index)
        {
            var go = new GameObject("PreviewFace");
            go.transform.SetParent(transform);
            var sr = go.AddComponent<ShapeRenderer>();
            sr.Setup(paperMaterial);
            previewRenderers.Add(sr);
        }

        previewRenderers[index].gameObject.SetActive(true);
        previewRenderers[index].transform.position = new Vector3(0, 0, zPos);
        previewRenderers[index].DrawShape(verts, uvs);
    }

    public void ClearPreview()
    {
        foreach (var face in activeFaces)
        {
            if (face != null) face.gameObject.SetActive(true);
        }
        foreach (var preview in previewRenderers)
        {
            if (preview != null) preview.gameObject.SetActive(false);
        }
    }

    private bool TrySplitAndReflectFace(PaperFace face, Vector2 start, Vector2 end, out PaperFace leftFace, out PaperFace rightFace)
    {
        leftFace = null;
        rightFace = null;

        if (!CalculateSplit(face, start, end, out var leftV, out var leftUV, out var foldedV, out var foldedUV))
            return false;

        if (leftV.Count >= 3)
        {
            leftFace = CreateNewFace(leftV, leftUV, $"{face.name}_Left");
        }

        rightFace = CreateNewFace(foldedV, foldedUV, $"{face.name}_Folded");
        return true;
    }

    private bool CalculateSplit(PaperFace face, Vector2 start, Vector2 end, out List<Vector2> leftVerts, out List<Vector2> leftUVs, out List<Vector2> foldedVerts, out List<Vector2> foldedUVs)
    {
        leftVerts = new List<Vector2>(); leftUVs = new List<Vector2>();
        foldedVerts = new List<Vector2>(); foldedUVs = new List<Vector2>();
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

        for (int i = rightVerts.Count - 1; i >= 0; i--)
        {
            foldedVerts.Add(GeometryMath.ReflectPoint(rightVerts[i], start, end));
            foldedUVs.Add(rightUVs[i]);
        }

        return true;
    }

    private PaperFace CreateNewFace(List<Vector2> verts, List<Vector2> uvs, string name)
    {
        var go = new GameObject(name);
        go.transform.SetParent(transform);

        var shapeRenderer = go.AddComponent<ShapeRenderer>();
        shapeRenderer.Setup(paperMaterial);

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

            StepSequenceDisplay.Instance?.RenderSequence(currentFigure, currentFigure.steps.Count);

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