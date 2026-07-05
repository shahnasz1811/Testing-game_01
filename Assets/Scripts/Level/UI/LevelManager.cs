using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour
{
    public static LevelManager instance;

    public List<IResettable> resettables = new List<IResettable>();
    private int totalEnemies;
    private int deadEnemies;
    public bool isGameOver = false;

    [SerializeField] private float victoryCheckDelay = 1f;
    [SerializeField] private VictoryScreen victoryScreen;
    [SerializeField] private LevelData levelData;

    private Coroutine victoryCoroutine;
    private bool levelCompleted = false;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
        
    void Start()
    {
        RecalculateEnemies();
    }

    public void RegisterResettable(IResettable obj)
    {
        if (!resettables.Contains(obj))
            resettables.Add(obj);
    }

    public void EnemyDied()
    {

        if (isGameOver) return;

        deadEnemies++;

        if (deadEnemies >= totalEnemies)
        {
            if (victoryCoroutine == null)
                victoryCoroutine = StartCoroutine(CheckVictoryAfterDelay());
        }
    }

    public void GameOver()
    {
        if (isGameOver) return;

        isGameOver = true;

       //🔥 HARD CANCEL victory
        if (victoryCoroutine != null)
        {
            StopCoroutine(victoryCoroutine);
            victoryCoroutine = null;
            levelCompleted = false;
        }

        Debug.Log("GAME OVER");
    }

    IEnumerator CheckVictoryAfterDelay()
    {
        levelCompleted = true;

        yield return new WaitForSeconds(victoryCheckDelay);

        SaveManager.UnlockNextLevel(levelData.levelNumber);

        LevelStats.instance.StopTimer();

        GameManager.instance.AddLevelStats(
            LevelStats.instance.timer,
            LevelStats.instance.deathCount
        );

        if (levelData.isFinalLevel)
        {
            int finalScore = GameManager.instance.CalculateScore();

            LeaderboardManager.instance.AddEntry(
                PlayerProfile.PlayerName,
                GameManager.instance.totalRunTime,
                GameManager.instance.totalDeaths,
                finalScore
            );

            yield return new WaitForSeconds(2f);
            SceneManager.LoadScene("Leaderboard");
        }
        else
        {
            victoryScreen.Show(levelData, true, deadEnemies, totalEnemies);
        }

        victoryCoroutine = null;
    }

    public void ResetGameState()
    {
        isGameOver = false;
        levelCompleted = false;

        if (victoryCoroutine != null)
        {
            StopCoroutine(victoryCoroutine);
            victoryCoroutine = null;
        }
    }

    public void ResetAll()
    {
        foreach (IResettable obj in resettables)
        {
            obj.ResetState();
        }

        deadEnemies = 0;

        RecalculateEnemies();
    }

    public void RecalculateEnemies()
    {
        EnemyDeath[] enemies = FindObjectsOfType<EnemyDeath>();

        totalEnemies = enemies.Length;
        deadEnemies = 0;

        foreach (var e in enemies)
        {
            if (e.isDead)
                deadEnemies++;
        }
    }
}