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

        //Destroy(gameObject, 0.1f);
    }

    [System.Obsolete]
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            ClearLeaderboard();
        }
    }

    public void AddEntry(string name, float time, int deaths, int score)
    {
        data.entries.Add(new LeaderboardEntry(name, time, deaths, score));

        data.entries.Sort((a, b) => b.score.CompareTo(a.score));

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
            data = JsonUtility.FromJson<LeaderboardData>(PlayerPrefs.GetString(key));

            // JsonUtility can hand back an object with a null List if the
            // saved JSON is from an older/different schema (missing the
            // "entries" field entirely) - without this, GetEntries() would
            // return null and LeaderboardUI.BuildLeaderboard()'s first line
            // (entries.RemoveAll(...)) would throw, breaking the whole scene.
            if (data == null || data.entries == null)
                data = new LeaderboardData();
        }
    }

    [System.Obsolete]
    public void ClearLeaderboard()
    {
        data.entries.Clear();
        PlayerPrefs.DeleteKey(key);
        PlayerPrefs.Save();
        Debug.Log("Leaderboard cleared.");

        FindObjectOfType<LeaderboardUI>().RefreshUI();
    }
}