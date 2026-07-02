using System;

[System.Serializable]
public class LeaderboardEntry
{
    public string playerName;
    public int score;

public LeaderboardEntry(string name, int score)
    {
        this.playerName = name;
        this.score = score;
    }
}
