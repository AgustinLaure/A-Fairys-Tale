using UnityEngine;

[CreateAssetMenu(fileName = "New Level Data", menuName = "Levels/LevelData")]
public class LevelData : ScriptableObject
{
    public Level level = Level.One;

    public string levelName = "New Level";
    public string sceneName = "SCENE_NAME";
}