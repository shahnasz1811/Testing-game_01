using UnityEngine;

public class PauseMenu : MonoBehaviour
{

    private bool isPaused = false;
    public GameObject container;
    // Update is called once per frame

    private void Start()
    {
        container.SetActive(false);
        Time.timeScale = 1f; // Ensure the game starts unpaused
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        container.SetActive(true);
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        isPaused = false;
        container.SetActive(false);
        Time.timeScale = 1f;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        container.SetActive(false);
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
        
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        #if UNITY_EDITOR
            // Exits Play Mode if running inside the Unity Editor
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            // Closes the actual built application (.exe, .app, Android build, etc.)
        Application.Quit();
        #endif
    }
}
