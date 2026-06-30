using UnityEngine;

public class PlayerProfile : MonoBehaviour
{
    public static string PlayerName;

private void Awake()
    {
        if (PlayerPrefs.HasKey("PlayerName"))
        {
            PlayerName = PlayerPrefs.GetString("PlayerName");
        }
    }

    public void SetPlayerName(string name)
    {
        PlayerName = name;
        PlayerPrefs.SetString("PlayerName", name);
        PlayerPrefs.Save();
    }
}
