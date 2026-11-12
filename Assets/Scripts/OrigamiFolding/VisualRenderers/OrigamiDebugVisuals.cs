using System.Collections.Generic;
using UnityEngine;

public class OrigamiDebugVisuals : MonoSingleton<OrigamiDebugVisuals>
{
    [Header("Toggles")]
    public bool showSnapPoints = true;
    public bool showDragMarkers = true;
    public bool showTargetLimits = true;
    public bool showCheckpoints = true;

    [Header("Materials/Settings")]
    public Material targetLineMaterial;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    private ShapeBorderRenderer maxLimitRenderer;
    private ShapeBorderRenderer minLimitRenderer;
    private List<GameObject> checkpointMarkers = new List<GameObject>();
    private List<GameObject> snapMarkers = new List<GameObject>();
    private GameObject startRawMarker, startSnapMarker, endRawMarker, endSnapMarker;
    private bool isInitialized = false;

    private void InitIfNeeded()
    {
        if (isInitialized) return;
        isInitialized = true;

        maxLimitRenderer = CreateLimitRenderer("TargetMaxVisual", 0.12f, -0.5f);
        minLimitRenderer = CreateLimitRenderer("TargetMinVisual", 0.05f, -0.4f);

        startRawMarker = CreateMarker("StartRaw", Color.blue, 0.25f);
        startSnapMarker = CreateMarker("StartSnap", Color.green, 0.25f);
        endRawMarker = CreateMarker("EndRaw", Color.blue, 0.25f);
        endSnapMarker = CreateMarker("EndSnap", Color.green, 0.25f);
    }

    private void Start() => InitIfNeeded();

    private ShapeBorderRenderer CreateLimitRenderer(string name, float width, float zDepth)
    {
        var go = new GameObject(name);
        go.transform.SetParent(transform);
        go.transform.localPosition = new Vector3(0, 0, zDepth);

        var sbr = go.AddComponent<ShapeBorderRenderer>();
        sbr.Setup(width, targetLineMaterial);
        return sbr;
    }

    private GameObject CreateMarker(string name, Color color, float size)
    {
        var marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        marker.name = name;
        Destroy(marker.GetComponent<Collider>());
        marker.transform.SetParent(transform);
        marker.transform.localScale = Vector3.one * size;

        var rend = marker.GetComponent<Renderer>();
        var unlit = Shader.Find("Sprites/Default");
        if (unlit != null) rend.material.shader = unlit;
        rend.material.color = color;

        marker.SetActive(false);
        return marker;
    }

    private void Update()
    {
        if (maxLimitRenderer != null) maxLimitRenderer.gameObject.SetActive(showTargetLimits);
        if (minLimitRenderer != null) minLimitRenderer.gameObject.SetActive(showTargetLimits);

        if (!showSnapPoints) snapMarkers.ForEach(m => m.SetActive(false));
        if (!showDragMarkers)
        {
            startRawMarker?.SetActive(false); startSnapMarker?.SetActive(false);
            endRawMarker?.SetActive(false); endSnapMarker?.SetActive(false);
        }
        if (!showCheckpoints) checkpointMarkers.ForEach(m => m.SetActive(false));
    }
#endif

    public void UpdateTargetLimits(List<Vector2> shape)
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        InitIfNeeded();
        maxLimitRenderer.DrawShape(shape);
        minLimitRenderer.DrawShape(shape);
#endif
    }

    public void SetTargetLimitColors(Color max, Color min)
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        InitIfNeeded();
        maxLimitRenderer.SetColor(max);
        minLimitRenderer.SetColor(min);
#endif
    }

    public void UpdateCheckpoints(List<LayerCheckpoint> checkpoints)
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        InitIfNeeded();
        checkpoints = checkpoints ?? new List<LayerCheckpoint>();
        while (checkpointMarkers.Count < checkpoints.Count)
        {
            checkpointMarkers.Add(CreateMarker("LayerCheckpointMarker", Color.yellow, 0.3f));
        }
        for (int i = 0; i < checkpointMarkers.Count; i++)
        {
            bool isActive = i < checkpoints.Count;
            checkpointMarkers[i].SetActive(isActive);
            if (isActive)
                checkpointMarkers[i].transform.position = new Vector3(checkpoints[i].position.x, checkpoints[i].position.y, -0.6f);
        }
#endif
    }

    public void SetCheckpointColor(int index, Color color)
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        InitIfNeeded();
        if (index >= 0 && index < checkpointMarkers.Count)
            checkpointMarkers[index].GetComponent<Renderer>().material.color = color;
#endif
    }

    public void UpdateSnapPoints(List<Vector2> points)
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        InitIfNeeded();
        if (!showSnapPoints) return;
        while (snapMarkers.Count < points.Count) snapMarkers.Add(CreateMarker("SnapMarker", Color.magenta, 0.15f));

        for (int i = 0; i < snapMarkers.Count; i++)
        {
            bool isActive = i < points.Count;
            snapMarkers[i].SetActive(isActive);
            if (isActive) snapMarkers[i].transform.position = new Vector3(points[i].x, points[i].y, -1.5f);
        }
#endif
    }

    public void SetDragMarkers(Vector2 rawPos, Vector2 snapPos, bool active, bool isStart)
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        InitIfNeeded();
        if (!showDragMarkers) return;
        var raw = isStart ? startRawMarker : endRawMarker;
        var snap = isStart ? startSnapMarker : endSnapMarker;

        raw.SetActive(active); snap.SetActive(active);
        if (active)
        {
            raw.transform.position = new Vector3(rawPos.x, rawPos.y, -2f);
            snap.transform.position = new Vector3(snapPos.x, snapPos.y, -2f);
        }
#endif
    }
}