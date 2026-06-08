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
        if (isGameOver) return; // prevent double trigger

        isGameOver = true;

        Debug.Log("GAME OVER");

        //Stop all enemies / systems
        //Time.timeScale = 0f; // optional freeze

        // Or instead of freeze, you can reset:
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
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
        deadEnemies++;

        Debug.Log("Enemies killed: " + deadEnemies);

        if (deadEnemies >= totalEnemies)
        {
            LoadNextLevel();
        }
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

    private void LoadNextLevel()
    {
        int current = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(current + 1);
    }
}