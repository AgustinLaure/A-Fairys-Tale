using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LayerCheckpoint
{
    public Vector2 position;

    [Min(0)]
    public int requiredLayers = 1;
}

[System.Serializable]
public class OrigamiObjective
{
    [SerializeField] public string stepName = "New step";

    [SerializeField] public bool flipStep;

    public List<Vector2> targetShape = new List<Vector2>();

    [Tooltip("-1 means all")]
    public int layersToGrab = -1;

    public List<LayerCheckpoint> layerCheckpoints = new List<LayerCheckpoint>();
}