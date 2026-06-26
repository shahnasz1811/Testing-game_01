using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class VictoryScreen : MonoBehaviour
{
    [Header("Stars")]
    [SerializeField] private GameObject star1;
    [SerializeField] private GameObject star2;
    [SerializeField] private GameObject star3;

    [Header("Value Texts")]
    [SerializeField] private TMP_Text objectiveValueText;
    [SerializeField] private TMP_Text timeValueText;
    [SerializeField] private TMP_Text deathValueText;

    [Header("Criteria Icons")]
    [SerializeField] private GameObject objectiveCheck;
    [SerializeField] private GameObject timeCheck;
    [SerializeField] private GameObject deathCheck;

    [SerializeField] private GameObject objectiveFail;
    [SerializeField] private GameObject timeFail;
    [SerializeField] private GameObject deathFail;

    private void Awake()
    {
        gameObject.SetActive(false);
    }

    public void Show(LevelData levelData, bool allEnemiesKilled, int enemiesKilled, int totalEnemies)
    {
        gameObject.SetActive(true);

        LevelStats.instance.StopTimer();

        float finalTime = LevelStats.instance.timer;
        int deaths = LevelStats.instance.deathCount;

        bool earnedObjectiveStar = allEnemiesKilled;
        bool earnedTimeStar = finalTime <= levelData.targetTime;
        bool earnedDeathStar = deaths <= levelData.maxDeathsForStar;

        int starsEarned = 0;

        if (earnedObjectiveStar)
            starsEarned++;

        if (earnedTimeStar)
            starsEarned++;

        if (earnedDeathStar)
            starsEarned++;

        SaveManager.SaveStars(levelData.levelNumber, starsEarned);

        objectiveValueText.text = $"Enemies Defeated: {enemiesKilled} / {totalEnemies}";
        timeValueText.text = $"{FormatTime(finalTime)} / {FormatTime(levelData.targetTime)}";
        deathValueText.text = $"{deaths} / {levelData.maxDeathsForStar} Allowed";

        star1.SetActive(earnedObjectiveStar);
        star2.SetActive(earnedTimeStar);
        star3.SetActive(earnedDeathStar);

        objectiveCheck.SetActive(earnedObjectiveStar);
        timeCheck.SetActive(earnedTimeStar);
        deathCheck.SetActive(earnedDeathStar);

        objectiveFail.SetActive(!earnedObjectiveStar);
        timeFail.SetActive(!earnedTimeStar);
        deathFail.SetActive(!earnedDeathStar);

        Time.timeScale = 0f;
    }

    private string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);

        return $"{minutes:00}:{seconds:00}";
    }

    public void RetryLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }

    public void GoToNextLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}