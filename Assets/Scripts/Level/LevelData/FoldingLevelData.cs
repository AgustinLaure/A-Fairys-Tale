using UnityEngine;

[CreateAssetMenu(fileName = "New Folding Level Data", menuName = "Levels/FoldingLevelData")]
public class FoldingLevelData : LevelData
{
    FoldingLevelData()
    {
        base.LevelType = LevelType.Folding;
    }

    public FoldingLevel foldinglevel;

    public OrigamiFigure objectiveFigure;
}
