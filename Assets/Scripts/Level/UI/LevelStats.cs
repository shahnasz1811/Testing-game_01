using UnityEngine;

public class LevelStats : MonoBehaviour
{
    public static LevelStats instance;

    public float timer;
    public int deathCount;
    public int enemiesKilled;

    private bool isTiming = true;

    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {
        if (isTiming)
        {
            timer += Time.deltaTime;
        }
    }

    public void RegisterDeath()
    {
        deathCount++;
    }

    public void StopTimer()
    {
        isTiming = false;
    }
}