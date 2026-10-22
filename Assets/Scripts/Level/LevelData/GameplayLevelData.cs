using UnityEngine;

[CreateAssetMenu(fileName = "New Gameplay Level Data", menuName = "Levels/GameplayLevelData")]
public class GameplayLevelData : LevelData
{
    GameplayLevelData()
    {
        base.LevelType = LevelType.Gameplay;
    }

    public GameplayLevel level = GameplayLevel.One;
}