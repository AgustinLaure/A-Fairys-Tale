using UnityEngine;

public abstract class LevelData : ScriptableObject
{
    public LevelData nextLevel;
    
    protected LevelType LevelType = LevelType.Gameplay;

    public LevelType GetLevelType()
    {
        return LevelType;
    }

    public int levelId = 0;
    public string levelName = "New Level";
    public string sceneName = "SCENE_NAME";
}