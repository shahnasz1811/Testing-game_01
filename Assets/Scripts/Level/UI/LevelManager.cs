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

    private Door exitDoor;

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

    [System.Obsolete]
    void Start()
    {
        RecalculateEnemies();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            StartCoroutine(OpenExitDoorAfterDelay());
            //LeaderboardManager.instance.ClearLeaderboard();
        }
    }
    public void RegisterResettable(IResettable obj)
    {
        if (!resettables.Contains(obj))
            resettables.Add(obj);
    }

    // The Door registers itself here when it spawns, so the level doesn't need
    // any manual Inspector wiring - just drop the Door prefab into the scene.
    public void RegisterExitDoor(Door door)
    {
        exitDoor = door;
    }

    public void EnemyDied()
    {

        if (isGameOver) return;

        deadEnemies++;

        if (deadEnemies >= totalEnemies)
        {
            if (victoryCoroutine == null)
                victoryCoroutine = StartCoroutine(OpenExitDoorAfterDelay());
        }
    }

    // For boss levels: her defeat alone should open the door, independent of
    // whatever regular EnemyDeath-based enemies exist elsewhere in the level.
    // She's never counted by RecalculateEnemies() (she uses ICrushable, not
    // EnemyDeath - see ICrushable.cs), so EnemyDied()'s deadEnemies/
    // totalEnemies check is the wrong mechanism for her: it would either
    // never trigger (if other enemies are still alive) or trigger too early/
    // coincidentally depending on unrelated enemy counts. Call this directly
    // from BossController once she's actually defeated instead.
    public void BossDefeated()
    {
        if (isGameOver) return;

        if (victoryCoroutine == null)
            victoryCoroutine = StartCoroutine(OpenExitDoorAfterDelay());
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

    // All enemies are dead - wait a beat, then open the exit door instead of
    // showing the victory screen right away. The victory screen now only
    // appears once the player actually walks through the open door.
    IEnumerator OpenExitDoorAfterDelay()
    {
        levelCompleted = true;

        yield return new WaitForSeconds(victoryCheckDelay);

        if (isGameOver)
        {
            levelCompleted = false;
            victoryCoroutine = null;
            yield break;
        }

        if (exitDoor != null)
        {
            exitDoor.Open();
        }
        else
        {
            Debug.LogWarning("LevelManager: no exit Door found in this scene - showing the victory screen immediately instead.");
            yield return StartCoroutine(ShowVictoryRoutine());
        }

        victoryCoroutine = null;
    }

    // Called by the Door once the player walks through it while it's open.
    public void OnPlayerReachedExit()
    {
        if (isGameOver) return;

        StartCoroutine(ShowVictoryRoutine());
    }

    private IEnumerator ShowVictoryRoutine()
    {
        if (MusicManager.instance != null)
            MusicManager.instance.FadeOut();

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

    [System.Obsolete]
    public void ResetAll()
    {
        foreach (IResettable obj in resettables)
        {
            obj.ResetState();
        }

        deadEnemies = 0;

        RecalculateEnemies();
    }

    [System.Obsolete]
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