using UnityEngine;

public abstract class LevelData : ScriptableObject
{
    protected LevelType LevelType = LevelType.Gameplay;

    public LevelType GetLevelType()
    {
        return LevelType;
    }

    public string levelName = "New Level";
    public string sceneName = "SCENE_NAME";
}