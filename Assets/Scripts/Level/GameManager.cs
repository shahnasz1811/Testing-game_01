using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public List<IResettable> resettables = new List<IResettable>();
    public bool isGameOver = false;
    
    //public int areaIndex;
    //public Door exitDoor;
    private int totalEnemies;
    private int deadEnemies;
    public float totalRunTime = 0f;
    public int totalDeaths = 0;


    [Header("Level Complete")]
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
            DontDestroyOnLoad(gameObject); // 🔥 keeps data across levels
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

    void Update()
    {
        if (!isGameOver && !levelCompleted)
        {
            totalRunTime += Time.deltaTime;
        }
    }

    public int CalculateScore()
    {
        int baseScore = 10000;

        int timePenalty = (int)(totalRunTime * 8f);
        int deathPenalty = totalDeaths * 250;

        return Mathf.Max(0, baseScore - timePenalty - deathPenalty);
    }

    public void RegisterResettable(IResettable obj)
    {
        if (!resettables.Contains(obj))
            resettables.Add(obj);
    }

    public void GameOver()
    {
        if (isGameOver) return;

        isGameOver = true;

        // 🔥 HARD CANCEL victory
        if (victoryCoroutine != null)
        {
            StopCoroutine(victoryCoroutine);
            victoryCoroutine = null;
            levelCompleted = false;
        }

        Debug.Log("GAME OVER");
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

    // 🔥 NEW FUNCTION
    public void EnemyDied()
    {
        if (isGameOver) return;

        deadEnemies++;

        Debug.Log("Enemies killed: " + deadEnemies + "/" + totalEnemies);

        if (deadEnemies > totalEnemies)
        {
            Debug.LogError("OVERCOUNTING BUG!");
        }

        if (deadEnemies >= totalEnemies)
        {
            if (victoryCoroutine == null)
                victoryCoroutine = StartCoroutine(CheckVictoryAfterDelay());
        }
    }


    private IEnumerator CheckVictoryAfterDelay()
    {
        levelCompleted = true;

        float timer = 0f;

        while (timer < victoryCheckDelay)
        {
            if (isGameOver)
            {
                Debug.Log("Victory cancelled (player died)");
                levelCompleted = false;
                victoryCoroutine = null;
                yield break;
            }

            timer += Time.deltaTime;
            yield return null;
        }

        if (isGameOver)
        {
            levelCompleted = false;
            victoryCoroutine = null;
            yield break;
        }

        SaveManager.UnlockNextLevel(levelData.levelNumber);

        if (levelData.isFinalLevel)
        {
            int finalScore = GameManager.instance.CalculateScore();

            LeaderboardManager.instance.AddEntry(
                PlayerProfile.PlayerName,
                finalScore
            );

            yield return new WaitForSeconds(2f);

            SceneManager.LoadScene("LeaderboardScene");
        }
        else
        {
            victoryScreen.Show(levelData, true, deadEnemies, totalEnemies);
        }

        victoryCoroutine = null;
    }

    /*public bool AreAllEnemiesDead()
    {
        return deadEnemies >= totalEnemies;
    }*/

    /*void UnlockArea()
    {
        Debug.Log("AREA UNLOCKED!");
        // Save progress
        PlayerPrefs.SetInt("Area" + areaIndex, 1);
        // Unlock exit door
        exitDoor.Unlock();
    }

    bool IsUnlocked()
    {
        return PlayerPrefs.GetInt("Area" + areaIndex, 0) == 1;
    }

    /*private void LevelComplete()
    {
        Debug.Log("LEVEL COMPLETE!");

        Invoke(nameof(LoadNextLevel), 1.5f); // small delay
    }*/

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

        Debug.Log($"Recalculated: {deadEnemies}/{totalEnemies}");

        // 🔥 IMPORTANT: re-trigger victory if already satisfied
        if (deadEnemies >= totalEnemies && totalEnemies > 0)
        {
            if (victoryCoroutine == null && !isGameOver)
                victoryCoroutine = StartCoroutine(CheckVictoryAfterDelay());
        }
    }

    public void GoToLeaderboard()
    {

    }

    public void GoToNextLevel()
    {
        Time.timeScale = 1f;

        int current = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(current + 1);
    }
}