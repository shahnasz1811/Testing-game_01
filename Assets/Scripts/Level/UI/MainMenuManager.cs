using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    [Header("Name Input UI")]
    public GameObject namePanel;
    public TMP_InputField nameInput;

    private int nextSceneIndex = 2; // Level_1

    void Start()
    {
        PlayerProfile.Load();

        // Hide name panel at start
        namePanel.SetActive(false);

        // Continue button logic
        if (SaveManager.LastPlayedLevel > 0)
        {
            GameObject continueButton = GameObject.Find("ContinueButton");
            if (continueButton != null)
            {
                continueButton.SetActive(true);
            }
        }
    }

    // 🔥 STEP 1: CLICK NEW GAME → SHOW INPUT
    public void NewGame()
    {
        namePanel.SetActive(true);
        GameManager.instance.StartNewRun();
    }

    // 🔥 STEP 2: CONFIRM NAME → START GAME
    public void ConfirmName()
    {
        string playerName = nameInput.text;

        if (string.IsNullOrWhiteSpace(playerName))
            playerName = "Player";

        PlayerProfile.SetName(playerName);

        SaveManager.ResetSave();
  
        SceneManager.LoadScene(nextSceneIndex);

        //GameManager.instance.StartNewRun();
    }


    public void ContinueGame()
    {
        int levelToLoad = SaveManager.LastPlayedLevel;
        SceneManager.LoadScene(levelToLoad + 1);
    }

    public void OpenLevelSelect()
    {
        SceneManager.LoadScene(1);
    }

    public void OpenLeaderboard()
    {
        SceneManager.LoadScene(7);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}