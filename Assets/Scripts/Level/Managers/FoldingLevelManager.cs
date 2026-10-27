using UnityEngine;

public class FoldingLevelManager : LevelManager
{
    private FoldManager activeFoldManager;

    public void SetFoldManager(FoldManager newFoldManager)
    {
        activeFoldManager = newFoldManager;
    }

    public override void Awake()
    {
        base.Awake();
    }

    public void Start()
    {
        if (activeFoldManager == null) Debug.LogError("No level manager setup");

        activeFoldManager.currentFigure = (levelData as FoldingLevelData).objectiveFigure;
        activeFoldManager.StartUp();
    }

    public override void TogglePause()
    {
        base.TogglePause();
    }
}
