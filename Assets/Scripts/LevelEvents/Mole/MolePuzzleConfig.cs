using UnityEngine;

[CreateAssetMenu(fileName = "MolePuzzleConfig", menuName = "Scriptable Objects/MolePuzzleConfig")]
public class MolePuzzleConfig : ScriptableObject
{
    [SerializeField] public bool[] startActivatedMoles;

    [SerializeField] public int[] moleOneNeighbors;
    [SerializeField] public int[] moleTwoNeighbors;
    [SerializeField] public int[] moleThreeNeighbors;
    [SerializeField] public int[] moleFourNeighbors;
    [SerializeField] public int[] moleFiveNeighbors;

}
