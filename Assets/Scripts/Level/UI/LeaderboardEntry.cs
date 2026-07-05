using System;

[System.Serializable]
public class LeaderboardEntry
{
    public string playerName;
    public int score;
    public float totalTime;
    public int totalDeaths;

public LeaderboardEntry(string name, float time, int deaths, int score)
    {
        this.playerName = name;
        this.totalTime = time;
        this.totalDeaths = deaths;
        this.score = score;
    }
}
