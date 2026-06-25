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
        PlayerPrefs.Save();
    }
}