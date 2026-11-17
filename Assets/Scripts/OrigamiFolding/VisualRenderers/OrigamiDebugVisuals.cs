using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class OrigamiDebugVisuals : MonoSingleton<OrigamiDebugVisuals>
{
    [Header("Debug Action Toggles")]
    public InputActionReference toggleSnapPointsAction;
    public InputActionReference toggleDragMarkersAction;
    public InputActionReference toggleTargetLimitsAction;
    public InputActionReference toggleCheckpointsAction;

    [Header("UI")]
    public TMPro.TextMeshProUGUI debugModeText;

    [Header("Materials/Settings")]
    public Material targetLineMaterial;

    private bool showSnapPoints = false;
    private bool showDragMarkers = false;
    private bool showTargetLimits = false;
    private bool showCheckpoints = false;

    private ShapeBorderRenderer maxLimitRenderer;
    private ShapeBorderRenderer minLimitRenderer;
    private List<GameObject> checkpointMarkers = new List<GameObject>();
    private List<GameObject> snapMarkers = new List<GameObject>();
    private GameObject startRawMarker, startSnapMarker, endRawMarker, endSnapMarker;
    private bool isInitialized = false;

    private void OnEnable()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD

        showSnapPoints = true;
        showDragMarkers = true;
        showTargetLimits = true;
        showCheckpoints = true;

#endif

        if (toggleSnapPointsAction != null)
        {
            toggleSnapPointsAction.action.performed += ToggleSnapPoints;
            toggleSnapPointsAction.action.Enable();
        }
        if (toggleDragMarkersAction != null)
        {
            toggleDragMarkersAction.action.performed += ToggleDragMarkers;
            toggleDragMarkersAction.action.Enable();
        }
        if (toggleTargetLimitsAction != null)
        {
            toggleTargetLimitsAction.action.performed += ToggleTargetLimits;
            toggleTargetLimitsAction.action.Enable();
        }
        if (toggleCheckpointsAction != null)
        {
            toggleCheckpointsAction.action.performed += ToggleCheckpoints;
            toggleCheckpointsAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (toggleSnapPointsAction != null)
        {
            toggleSnapPointsAction.action.performed -= ToggleSnapPoints;
            toggleSnapPointsAction.action.Disable();
        }
        if (toggleDragMarkersAction != null)
        {
            toggleDragMarkersAction.action.performed -= ToggleDragMarkers;
            toggleDragMarkersAction.action.Disable();
        }
        if (toggleTargetLimitsAction != null)
        {
            toggleTargetLimitsAction.action.performed -= ToggleTargetLimits;
            toggleTargetLimitsAction.action.Disable();
        }
        if (toggleCheckpointsAction != null)
        {
            toggleCheckpointsAction.action.performed -= ToggleCheckpoints;
            toggleCheckpointsAction.action.Disable();
        }
    }

    private void ToggleSnapPoints(InputAction.CallbackContext context) { showSnapPoints = !showSnapPoints; RefreshUI(); }
    private void ToggleDragMarkers(InputAction.CallbackContext context) { showDragMarkers = !showDragMarkers; RefreshUI(); }
    private void ToggleTargetLimits(InputAction.CallbackContext context) { showTargetLimits = !showTargetLimits; RefreshUI(); }
    private void ToggleCheckpoints(InputAction.CallbackContext context) { showCheckpoints = !showCheckpoints; RefreshUI(); }

    private void RefreshUI()
    {
        if (debugModeText == null) return;

        if (!showSnapPoints && !showDragMarkers && !showTargetLimits && !showCheckpoints)
        {
            debugModeText.text = "";
            return;
        }

        debugModeText.text = "<color=yellow><b>Debug Modes:</b></color>\n" +
                             $"Snap Points: {(showSnapPoints ? "<color=green>On</color>" : "<color=red>Off</color>")}\n" +
                             $"Drag Markers: {(showDragMarkers ? "<color=green>On</color>" : "<color=red>Off</color>")}\n" +
                             $"Target Limits: {(showTargetLimits ? "<color=green>On</color>" : "<color=red>Off</color>")}\n" +
                             $"Checkpoints: {(showCheckpoints ? "<color=green>On</color>" : "<color=red>Off</color>")}";
    }

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

        RefreshUI();
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

    public void UpdateTargetLimits(List<Vector2> shape)
    {
        InitIfNeeded();
        maxLimitRenderer.DrawShape(shape);
        minLimitRenderer.DrawShape(shape);
    }

    public void SetTargetLimitColors(Color max, Color min)
    {
        InitIfNeeded();
        maxLimitRenderer.SetColor(max);
        minLimitRenderer.SetColor(min);
    }

    public void UpdateCheckpoints(List<LayerCheckpoint> checkpoints)
    {
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
    }

    public void SetCheckpointColor(int index, Color color)
    {
        InitIfNeeded();
        if (index >= 0 && index < checkpointMarkers.Count)
            checkpointMarkers[index].GetComponent<Renderer>().material.color = color;
    }

    public void UpdateSnapPoints(List<Vector2> points)
    {
        InitIfNeeded();
        if (!showSnapPoints) return;
        while (snapMarkers.Count < points.Count) snapMarkers.Add(CreateMarker("SnapMarker", Color.magenta, 0.15f));

        for (int i = 0; i < snapMarkers.Count; i++)
        {
            bool isActive = i < points.Count;
            snapMarkers[i].SetActive(isActive);
            if (isActive) snapMarkers[i].transform.position = new Vector3(points[i].x, points[i].y, -1.5f);
        }
    }

    public void SetDragMarkers(Vector2 rawPos, Vector2 snapPos, bool active, bool isStart)
    {
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
    }
}