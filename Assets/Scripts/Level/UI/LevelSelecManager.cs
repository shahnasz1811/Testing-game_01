using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelSelectManager : MonoBehaviour
{

    public GameObject[] level1Stars;
    public GameObject[] level2Stars;
    public GameObject[] level3Stars;
    public GameObject[] level4Stars;
    public GameObject[] level5Stars;
    public GameObject[] level6Stars;
    public GameObject[] level7Stars;
    public GameObject[] level8Stars;
    public GameObject[] level9Stars;
    public GameObject[] level10Stars;

    public Button[] levelButtons;

    private void Start()
    {
        UpdateLevelButtons();
        UpdateLevelStars();
    }

    private void UpdateLevelButtons()
    {
        int highestUnlocked = SaveManager.HighestUnlockedLevel;

        for (int i = 0; i < levelButtons.Length; i++)
        {
            int levelNumber = i + 1;

            levelButtons[i].interactable = levelNumber <= highestUnlocked;
        }
    }

    public void LoadLevel(int levelNumber)
    {
        SaveManager.LastPlayedLevel = levelNumber;
        SceneManager.LoadScene(levelNumber + 1);
    }

    public void BackToMainMenu()
    {
        SceneManager.LoadScene(0);
    }

    private void UpdateLevelStars()
    {
        UpdateStarsForLevel(1, level1Stars);
        UpdateStarsForLevel(2, level2Stars);
        UpdateStarsForLevel(3, level3Stars);
        UpdateStarsForLevel(4, level4Stars);
        UpdateStarsForLevel(5, level5Stars);
        UpdateStarsForLevel(6, level6Stars);
        UpdateStarsForLevel(7, level7Stars);
        UpdateStarsForLevel(8, level8Stars);
        UpdateStarsForLevel(9, level9Stars);
        UpdateStarsForLevel(10, level10Stars);
    }

    private void UpdateStarsForLevel(int levelNumber, GameObject[] stars)
    {
        int highestUnlocked = SaveManager.HighestUnlockedLevel;
        int savedStars = SaveManager.GetStars(levelNumber);

        Debug.Log("Level " + levelNumber + " saved stars: " + savedStars);

        bool isUnlocked = levelNumber <= highestUnlocked;

        for (int i = 0; i < stars.Length; i++)
        {
            stars[i].SetActive(isUnlocked && i < savedStars);
        }
    }
}