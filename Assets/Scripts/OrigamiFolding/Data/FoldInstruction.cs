using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewOrigamiFoldData", menuName = "Origami/Fold Data")]
public class OrigamiFigure : ScriptableObject
{
    public string figureName = "New Origami Fold";

    public OrigamiObjective startingShape = new OrigamiObjective();

    public List<OrigamiObjective> steps = new List<OrigamiObjective>();
}