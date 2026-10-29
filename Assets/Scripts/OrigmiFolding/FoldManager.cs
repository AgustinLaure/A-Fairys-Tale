using System.Collections.Generic;
using UnityEngine;

public class FoldManager : MonoSingleton<FoldManager>
{
    public Material paperMaterial;
    public List<PaperFace> activeFaces = new List<PaperFace>();

    public Material targetLineMaterial;

    public OrigamiFigure currentFigure;

    private int currentObjectiveIndex = 0;
    private List<Vector2> currentTargetShape = new List<Vector2>();

    private LineRenderer maxLimitRenderer;
    private LineRenderer minLimitRenderer;

    private List<GameObject> checkpointMarkers = new List<GameObject>();

    private void Start()
    {
        StartUp();
    }

    private void OnDestroy()
    {
        Debug.Log("Fold manager desroyed!");
    }

    public void StartUp()
    {
        if (activeFaces.Count == 0)
        {
            CreateInitialPaper(new Vector2(-5, -5), new Vector2(5, 5));
        }

        if (currentFigure == null || currentFigure.steps.Count == 0)
        {
            currentFigure = ScriptableObject.CreateInstance<OrigamiFigure>();
            currentFigure.figureName = "Diamond";

            OrigamiObjective defaultDiamond = new OrigamiObjective();
            defaultDiamond.stepName = "Step 1";
            defaultDiamond.targetShape = new List<Vector2>()
            {
                new Vector2(0, 5),   new Vector2(-5, 0),
                new Vector2(0, -5),  new Vector2(5, 0)
            };
            currentFigure.steps.Add(defaultDiamond);
        }

        SetupVisualRenderers();

        LoadObjective(0);
    }

    private void SetupVisualRenderers()
    {
        GameObject maxVisual = new GameObject("TargetMaxVisual");
        maxVisual.transform.SetParent(transform);
        maxLimitRenderer = maxVisual.AddComponent<LineRenderer>();
        maxLimitRenderer.loop = true;
        maxLimitRenderer.startWidth = 0.12f;
        maxLimitRenderer.endWidth = 0.12f;
        if (targetLineMaterial != null) maxLimitRenderer.material = targetLineMaterial;

        GameObject minVisual = new GameObject("TargetMinVisual");
        minVisual.transform.SetParent(transform);
        minLimitRenderer = minVisual.AddComponent<LineRenderer>();
        minLimitRenderer.loop = true;
        minLimitRenderer.startWidth = 0.05f;
        minLimitRenderer.endWidth = 0.05f;
        if (targetLineMaterial != null) minLimitRenderer.material = targetLineMaterial;
    }

    private void LoadObjective(int index)
    {
        currentObjectiveIndex = index;
        currentTargetShape = currentFigure.steps[index].targetShape;

        Debug.Log($"Shape: {currentFigure.figureName}  Step {index + 1}: {currentFigure.steps[index].stepName}");

        maxLimitRenderer.positionCount = currentTargetShape.Count;
        minLimitRenderer.positionCount = currentTargetShape.Count;

        for (int i = 0; i < currentTargetShape.Count; i++)
        {
            maxLimitRenderer.SetPosition(i, new Vector3(currentTargetShape[i].x, currentTargetShape[i].y, -0.5f));
            minLimitRenderer.SetPosition(i, new Vector3(currentTargetShape[i].x, currentTargetShape[i].y, 0.4f));
        }

        maxLimitRenderer.startColor = Color.red; maxLimitRenderer.endColor = Color.red;
        minLimitRenderer.startColor = Color.red; minLimitRenderer.endColor = Color.red;

        UpdateCheckpointMarkers(currentFigure.steps[index].layerCheckpoints);
    }

    private void UpdateCheckpointMarkers(List<LayerCheckpoint> checkpoints)
    {
        if (checkpoints == null) checkpoints = new List<LayerCheckpoint>();

        while (checkpointMarkers.Count < checkpoints.Count)
        {
            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            marker.name = "LayerCheckpointMarker";
            Destroy(marker.GetComponent<Collider>());
            marker.transform.SetParent(transform);
            marker.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);

            Renderer rend = marker.GetComponent<Renderer>();
            Shader unlit = Shader.Find("Sprites/Default");
            if (unlit != null) rend.material.shader = unlit;
            rend.material.color = Color.yellow;

            checkpointMarkers.Add(marker);
        }

        for (int i = 0; i < checkpointMarkers.Count; i++)
        {
            if (i < checkpoints.Count)
            {
                checkpointMarkers[i].transform.position = new Vector3(checkpoints[i].position.x, checkpoints[i].position.y, -0.6f);
                checkpointMarkers[i].GetComponent<Renderer>().material.color = Color.yellow;
                checkpointMarkers[i].SetActive(true);
            }
            else
            {
                checkpointMarkers[i].SetActive(false);
            }
        }
    }

    private void CreateInitialPaper(Vector2 bottomLeft, Vector2 topRight)
    {
        GameObject basePaper = new GameObject("BasePaper");
        basePaper.transform.SetParent(transform);
        basePaper.AddComponent<MeshRenderer>().material = paperMaterial;

        PaperFace face = basePaper.AddComponent<PaperFace>();
        face.vertices = new List<Vector2>()
        {
            bottomLeft,
            new Vector2(bottomLeft.x, topRight.y),
            topRight,
            new Vector2(topRight.x, bottomLeft.y)
        };
        face.UpdateMesh();
        activeFaces.Add(face);
    }

    public PaperFace GetTopFaceAt(Vector2 point)
    {
        PaperFace topFace = null;
        float minZ = float.MaxValue;

        foreach (PaperFace face in activeFaces)
        {
            if (IsPointInsideConvexPolygon(point, face.vertices, 0.05f))
            {
                if (face.transform.position.z < minZ)
                {
                    minZ = face.transform.position.z;
                    topFace = face;
                }
            }
        }
        return topFace;
    }

    public void ProcessFold(Vector2 startLine, Vector2 endLine, PaperFace targetFace = null)
    {
        List<PaperFace> newFaces = new List<PaperFace>();
        List<PaperFace> facesToRemove = new List<PaperFace>();

        foreach (PaperFace face in activeFaces)
        {
            if (targetFace != null && face != targetFace)
            {
                continue;
            }

            List<Vector2> leftVertices = new List<Vector2>();
            List<Vector2> rightVertices = new List<Vector2>();

            for (int i = 0; i < face.vertices.Count; i++)
            {
                Vector2 currentPoint = face.vertices[i];
                Vector2 nextPoint = face.vertices[(i + 1) % face.vertices.Count];

                float side = GeometryMath.PointLinePosition(currentPoint, startLine, endLine);

                if (side >= 0) leftVertices.Add(currentPoint);
                else rightVertices.Add(currentPoint);

                Vector2 intersection;
                Vector2 lineDir = (endLine - startLine).normalized;
                Vector2 p3 = startLine - lineDir * 1000f;
                Vector2 p4 = endLine + lineDir * 1000f;

                if (GeometryMath.GetIntersection(currentPoint, nextPoint, p3, p4, out intersection))
                {
                    leftVertices.Add(intersection);
                    rightVertices.Add(intersection);
                }
            }

            if (leftVertices.Count >= 3 && rightVertices.Count >= 3)
            {
                facesToRemove.Add(face);
                PaperFace leftFace = CreateNewFace(leftVertices, "Paper_Left");
                newFaces.Add(leftFace);

                List<Vector2> foldedVertices = new List<Vector2>();
                foreach (Vector2 v in rightVertices)
                {
                    foldedVertices.Add(GeometryMath.ReflectPoint(v, startLine, endLine));
                }
                foldedVertices.Reverse();

                PaperFace rightFace = CreateNewFace(foldedVertices, "Paper_Right_Folded");
                rightFace.transform.position = new Vector3(0, 0, -0.01f * (activeFaces.Count + newFaces.Count + 1));
                newFaces.Add(rightFace);
            }

            else if (rightVertices.Count >= 3 && leftVertices.Count < 3)
            {
                facesToRemove.Add(face);
                List<Vector2> foldedVertices = new List<Vector2>();
                foreach (Vector2 v in rightVertices)
                {
                    foldedVertices.Add(GeometryMath.ReflectPoint(v, startLine, endLine));
                }
                foldedVertices.Reverse();

                PaperFace rightFace = CreateNewFace(foldedVertices, face.gameObject.name + "_Folded");
                rightFace.transform.position = new Vector3(0, 0, -0.01f * (activeFaces.Count + newFaces.Count + 1));
                newFaces.Add(rightFace);
            }
        }

        foreach (PaperFace oldFace in facesToRemove)
        {
            activeFaces.Remove(oldFace);
            Destroy(oldFace.gameObject);
        }
        activeFaces.AddRange(newFaces);

        CheckWinCondition();
    }

    private PaperFace CreateNewFace(List<Vector2> verts, string name)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(transform);
        go.AddComponent<MeshRenderer>().material = paperMaterial;
        PaperFace face = go.AddComponent<PaperFace>();
        face.vertices = verts;
        face.UpdateMesh();
        return face;
    }

    private void CheckWinCondition()
    {
        if (currentObjectiveIndex >= currentFigure.steps.Count) return;

        float tolerance = 0.2f;
        OrigamiObjective currentStep = currentFigure.steps[currentObjectiveIndex];

        bool hasFailedUpperLimit = false;
        foreach (PaperFace face in activeFaces)
        {
            foreach (Vector2 vertex in face.vertices)
            {
                if (!IsPointInsideConvexPolygon(vertex, currentTargetShape, tolerance))
                {
                    hasFailedUpperLimit = true;
                    break;
                }
            }
            if (hasFailedUpperLimit) break;
        }

        bool hasFailedLowerLimit = false;
        foreach (Vector2 targetCorner in currentTargetShape)
        {
            bool cornerIsCovered = false;
            foreach (PaperFace face in activeFaces)
            {
                if (IsPointInsideConvexPolygon(targetCorner, face.vertices, tolerance))
                {
                    cornerIsCovered = true;
                    break;
                }
            }

            if (!cornerIsCovered)
            {
                hasFailedLowerLimit = true;
                break;
            }
        }

        bool hasFailedLayerCheck = false;
        if (currentStep.layerCheckpoints != null)
        {
            for (int i = 0; i < currentStep.layerCheckpoints.Count; i++)
            {
                LayerCheckpoint checkpoint = currentStep.layerCheckpoints[i];
                int layerCount = 0;

                foreach (PaperFace face in activeFaces)
                {
                    if (IsPointInsideConvexPolygon(checkpoint.position, face.vertices, tolerance))
                    {
                        layerCount++;
                    }
                }

                bool isCorrect = (layerCount == checkpoint.requiredLayers);
                if (i < checkpointMarkers.Count && checkpointMarkers[i].activeSelf)
                {
                    checkpointMarkers[i].GetComponent<Renderer>().material.color = isCorrect ? Color.green : Color.yellow;
                }

                if (!isCorrect)
                {
                    hasFailedLayerCheck = true;
                }
            }
        }

        if (maxLimitRenderer != null)
        {
            maxLimitRenderer.startColor = hasFailedUpperLimit ? Color.red : Color.green;
            maxLimitRenderer.endColor = hasFailedUpperLimit ? Color.red : Color.green;
        }
        if (minLimitRenderer != null)
        {
            minLimitRenderer.startColor = hasFailedLowerLimit ? Color.red : Color.blue;
            minLimitRenderer.endColor = hasFailedLowerLimit ? Color.red : Color.blue;
        }

        if (!hasFailedUpperLimit && !hasFailedLowerLimit && !hasFailedLayerCheck)
        {
            Debug.Log($"{currentStep.stepName} step done");

            currentObjectiveIndex++;
            if (currentObjectiveIndex < currentFigure.steps.Count)
            {
                LoadObjective(currentObjectiveIndex);
            }
            else
            {
                Debug.Log($"Shape done");
                minLimitRenderer.startColor = Color.green; minLimitRenderer.endColor = Color.green;
            }
        }
    }

    public List<Vector2> GetAllVertices()
    {
        List<Vector2> allVerts = new List<Vector2>();
        foreach (PaperFace face in activeFaces)
        {
            foreach (Vector2 v in face.vertices)
            {
                allVerts.Add(v);
            }
        }
        return allVerts;
    }

    private bool IsPointInsideConvexPolygon(Vector2 point, List<Vector2> polygon, float tolerance)
    {
        if (polygon.Count < 3) return false;
        bool hasPositive = false;
        bool hasNegative = false;

        for (int i = 0; i < polygon.Count; i++)
        {
            Vector2 a = polygon[i];
            Vector2 b = polygon[(i + 1) % polygon.Count];

            float side = GeometryMath.PointLinePosition(point, a, b);

            if (side > tolerance) hasPositive = true;
            else if (side < -tolerance) hasNegative = true;

            if (hasPositive && hasNegative) return false;
        }
        return true;
    }
}
