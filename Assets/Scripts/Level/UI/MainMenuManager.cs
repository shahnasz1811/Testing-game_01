using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public void NewGame()
    {
        SaveManager.ResetSave();
        SceneManager.LoadScene(2); // Level_1
    }

    public void ContinueGame()
    {
        int levelToLoad = SaveManager.LastPlayedLevel;
        SceneManager.LoadScene(levelToLoad + 1);
    }

    public void OpenLevelSelect()
    {
        SceneManager.LoadScene(1); // LevelSelect
    }

    public void OpenLeaderboard()
    {
        SceneManager.LoadScene(7); //Leaderboard 
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}