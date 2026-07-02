using UnityEngine;
using System.Collections.Generic;

public class LeaderboardManager : MonoBehaviour
{
    public static LeaderboardManager instance;

private LeaderboardData data = new LeaderboardData();
    private string key = "LEADERBOARD";

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            Load();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddEntry(string name, int score)
    {
        data.entries.Add(new LeaderboardEntry(name, score));

        data.entries.Sort((a, b) => b.score.CompareTo(a.score));

        if (data.entries.Count > 10)
            data.entries.RemoveAt(10);

        Save();
    }

    public List<LeaderboardEntry> GetEntries()
    {
        return data.entries;
    }

    void Save()
    {
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(key, json);
        PlayerPrefs.Save();
    }

    void Load()
    {
        if (PlayerPrefs.HasKey(key))
        {
            data = JsonUtility.FromJson<LeaderboardData>(
                PlayerPrefs.GetString(key)
            );
        }
    }
}
