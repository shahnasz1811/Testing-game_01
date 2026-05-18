using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    private List<EnemyDeath> enemies = new List<EnemyDeath>();

    private void Awake()
    {
        instance = this;
    }

    public void RegisterEnemy(EnemyDeath enemy)
    {
        enemies.Add(enemy);
    }

    public void ResetEnemies()
    {
        foreach (EnemyDeath enemy in enemies)
        {
            enemy.ResetEnemy();
        }
    }
}