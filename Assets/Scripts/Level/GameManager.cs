using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public List<IResettable> resettables = new List<IResettable>();
    public bool isGameOver = false;

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

        Debug.Log("Total Enemies: " + totalEnemies);
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

        // Stop all enemies / systems
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
            LevelComplete();
        }
    }

    private void LevelComplete()
    {
        Debug.Log("LEVEL COMPLETE!");

        Invoke(nameof(LoadNextLevel), 1.5f); // small delay
    }

    private void LoadNextLevel()
    {
        int current = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(current + 1);
    }
}