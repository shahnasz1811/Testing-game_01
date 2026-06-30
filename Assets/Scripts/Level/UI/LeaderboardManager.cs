using UnityEngine;
using System.Collections.Generic;

public class LeaderboardManager : MonoBehaviour
{
    public static LeaderboardManager instance;

private LeaderboardData leaderboard = new LeaderboardData();

    private string saveKey = "LEADERBOARD";

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // 🔥 KEEP DATA ACROSS SCENES
            Load();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddEntry(string name, int score)
    {
        LeaderboardEntry entry = new LeaderboardEntry
        {
            playerName = name,
            score = score
        };

        leaderboard.entries.Add(entry);

        // Sort (highest score first)
        leaderboard.entries.Sort((a, b) => b.score.CompareTo(a.score));

        // Keep top 10
        if (leaderboard.entries.Count > 10)
            leaderboard.entries.RemoveAt(10);

        Save();
    }

    public List<LeaderboardEntry> GetEntries()
    {
        return leaderboard.entries;
    }

    private void Save()
    {
        string json = JsonUtility.ToJson(leaderboard);
        PlayerPrefs.SetString(saveKey, json);
        PlayerPrefs.Save();
    }

    private void Load()
    {
        if (PlayerPrefs.HasKey(saveKey))
        {
            string json = PlayerPrefs.GetString(saveKey);
            leaderboard = JsonUtility.FromJson<LeaderboardData>(json);
        }
        else
        {
            leaderboard = new LeaderboardData();
        }
    }
}
