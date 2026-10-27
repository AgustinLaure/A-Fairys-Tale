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
    public string stepName = "New step";

    public List<Vector2> targetShape = new List<Vector2>();

    public List<LayerCheckpoint> layerCheckpoints = new List<LayerCheckpoint>();
}

[CreateAssetMenu(fileName = "NewOrigamiFoldData", menuName = "Origami/Fold Data")]
public class OrigamiFigure : ScriptableObject
{
    public string figureName = "New Origami Fold";

    public List<OrigamiObjective> steps = new List<OrigamiObjective>();
}