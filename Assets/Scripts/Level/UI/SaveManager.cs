using UnityEngine;

public static class SaveManager
{
    public static int HighestUnlockedLevel
    {
        get => PlayerPrefs.GetInt("HighestUnlockedLevel", 1);
        set
        {
            PlayerPrefs.SetInt("HighestUnlockedLevel", value);
            PlayerPrefs.Save();
        }
    }

    public static int LastPlayedLevel
    {
        get => PlayerPrefs.GetInt("LastPlayedLevel", 1);
        set
        {
            PlayerPrefs.SetInt("LastPlayedLevel", value);
            PlayerPrefs.Save();
        }
    }

    public static void UnlockNextLevel(int currentLevel)
    {
        int nextLevel = currentLevel + 1;

        if (nextLevel > HighestUnlockedLevel)
        {
            HighestUnlockedLevel = nextLevel;
        }

        LastPlayedLevel = nextLevel;
    }

    public static void ResetSave()
    {
        PlayerPrefs.DeleteKey("HighestUnlockedLevel");
        PlayerPrefs.DeleteKey("LastPlayedLevel");

        for (int i = 1; i <= 4; i++)
        {
            PlayerPrefs.DeleteKey("Level_" + i + "_Stars");
        }

        PlayerPrefs.Save();
    }

    public static void SaveStars(int levelNumber, int stars)
    {
        string key = "Level_" + levelNumber + "_Stars";

        int currentBest = PlayerPrefs.GetInt(key, 0);

        if (stars > currentBest)
        {
            PlayerPrefs.SetInt(key, stars);
            PlayerPrefs.Save();
        }
    }

    public static int GetStars(int levelNumber)
    {
        string key = "Level_" + levelNumber + "_Stars";
        return PlayerPrefs.GetInt(key, 0);
    }
}