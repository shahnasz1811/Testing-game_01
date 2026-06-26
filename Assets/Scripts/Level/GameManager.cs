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


    [Header("Level Complete")]
    [SerializeField] private float victoryCheckDelay = 1f;
    [SerializeField] private VictoryScreen victoryScreen;
    [SerializeField] private LevelData levelData;

    private bool levelCompleted = false;

    private void Awake()
    {
        instance = this;
    }

    [System.Obsolete]
    private void Start()
    {
        // Count all enemies at start
        EnemyDeath[] enemies = FindObjectsOfType<EnemyDeath>();
        totalEnemies = enemies.Length;

        /* Load saved progress
        if (IsUnlocked())
        {
            exitDoor.Unlock();
        }

        Debug.Log("Total Enemies: " + totalEnemies);*/
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

        Debug.Log("GAME OVER");
    }

    public void ResetAll()
    {
        foreach (IResettable obj in resettables)
        {
            obj.ResetState();
        }

        deadEnemies = 0; // reset counter when player dies
    }

    // 🔥 NEW FUNCTION
    public void EnemyDied()
    {
        if (levelCompleted) return;
        if (isGameOver) return;

        deadEnemies++;

        Debug.Log("Enemies killed: " + deadEnemies);

        if (deadEnemies >= totalEnemies)
        {
            StartCoroutine(CheckVictoryAfterDelay());
        }
    }

    private IEnumerator CheckVictoryAfterDelay()
    {
        levelCompleted = true;

        yield return new WaitForSeconds(victoryCheckDelay);

        if (isGameOver)
        {
            levelCompleted = false;
            yield break;
        }

        SaveManager.UnlockNextLevel(levelData.levelNumber);
        victoryScreen.Show(levelData, true, deadEnemies, totalEnemies);
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

    public void GoToNextLevel()
    {
        Time.timeScale = 1f;

        int current = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(current + 1);
    }
}