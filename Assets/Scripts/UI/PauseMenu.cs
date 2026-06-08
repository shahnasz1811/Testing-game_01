using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public GameObject container;
    // Update is called once per frame
    void Update()
    {
       if (Input.GetKeyDown(KeyCode.Escape))
        {
            container.SetActive(true);
            Time.timeScale = 0f;
        }
    }

    public void ResumeGame()
    {
        container.SetActive(false);
        Time.timeScale = 1f;
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
