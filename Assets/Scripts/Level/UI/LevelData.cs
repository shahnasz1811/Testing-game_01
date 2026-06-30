using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "Game/Level Data")]
public class LevelData : ScriptableObject
{
    public int levelNumber;
    public bool isFinalLevel = false;

    [Header("Star Requirements")]
    public float targetTime = 60f;
    public bool requireAllEnemiesKilled = true;

    [Header("Third Star")]
    public int maxDeathsForStar = 0;

    [Header("Level Info")]
    public int totalEnemies;
}