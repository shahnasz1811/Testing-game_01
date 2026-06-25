using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelSelectManager : MonoBehaviour
{
    public Button[] levelButtons;

    private void Start()
    {
        UpdateLevelButtons();
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
}