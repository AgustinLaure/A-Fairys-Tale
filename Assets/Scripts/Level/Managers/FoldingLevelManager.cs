using UnityEngine;

public class FoldingLevelManager : LevelManager
{
    private FoldManager activeFoldManager = null;

    public override void Awake()
    {
        base.Awake();

        activeFoldManager = BootStrapper.CreateMonoSingletonGameObject<FoldManager>();

        if (activeFoldManager != null)
        {
            Debug.Log("Created new fold manager!");
        }
        else
        {
            Debug.LogError("Cant create fold manager!");
        }
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

    public void OnDestroy()
    {
        Destroy(activeFoldManager.gameObject);
    }
}
