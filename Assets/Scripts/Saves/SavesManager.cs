using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SavesManager : Singleton<SavesManager>
{
    private string[] gameplayLevelsSaveKeys = { "level_one", "level_two" };
    private string[] foldLevelsSaveKeys = { "level_airplane", "level_frog", "level_crane" };

    void UnlockGameplayLevel(int level)
    {
        PlayerPrefs.SetInt(gameplayLevelsSaveKeys[level - 1], 1);
    }

    void UnlockFoldingLevel(int level)
    {
        PlayerPrefs.SetInt(gameplayLevelsSaveKeys[level - 1], 1);
    }

    void SaveChanges()
    {
        PlayerPrefs.Save();
    }

    bool IsGameplayLevelUnlocked(int level)
    {
        if(level <= 0)
        {
            Debug.LogError("Can't check for levels under 1!");
            return false;
        }

        if(level > gameplayLevelsSaveKeys.Count())
        {
            Debug.LogError($"level {level} does not exist!");
            return false;
        }
        
        return PlayerPrefs.GetInt(gameplayLevelsSaveKeys[level - 1]) > 0;
    }

    bool IsFoldingLevelUnlocked(int level)
    {
        if (level <= 0)
        {
            Debug.LogError("Can't check for levels under 1!");
            return false;
        }

        if (level > foldLevelsSaveKeys.Count())
        {
            Debug.LogError($"level {level} does not exist!");
            return false;
        }

        return PlayerPrefs.GetInt(foldLevelsSaveKeys[level - 1]) > 0;
    }
}
