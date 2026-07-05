using UnityEngine;

public static class PlayerProfile
{
    public static string PlayerName = "Player";

    public static void SetName(string name)
    {
        PlayerName = name;
        PlayerPrefs.SetString("PLAYER_NAME", name);
    }

    public static void Load()
    {
        if (PlayerPrefs.HasKey("PLAYER_NAME"))
        {
            PlayerName = PlayerPrefs.GetString("PLAYER_NAME");
        }
    }
}