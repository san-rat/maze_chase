using UnityEngine;
using TMPro;

public class PauseManager : MonoBehaviour
{
    [Header("UI Reference")]
    public GameObject pausePanel;

    private bool isPaused = false;

    public static PauseManager Instance;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // Make sure pause panel is hidden at start
        if (pausePanel != null)
            pausePanel.SetActive(false);
    }

    void Update()
    {
        // Press P to pause/unpause
        if (Input.GetKeyDown(KeyCode.P))
        {
            TogglePause();
        }

        // Press R to restart
        if (Input.GetKeyDown(KeyCode.R))
        {
            RestartLevel();
        }
    }

    public void TogglePause()
    {
        isPaused = !isPaused;

        // Time.timeScale 0 = frozen, 1 = normal
        Time.timeScale = isPaused ? 0f : 1f;

        if (pausePanel != null)
            pausePanel.SetActive(isPaused);

        Debug.Log(isPaused ? "Game Paused" :
            "Game Resumed");
    }

    public void RestartLevel()
    {
        // Reset time scale first
        Time.timeScale = 1f;

        // Reload current scene
        UnityEngine.SceneManagement.SceneManager
            .LoadScene(
            UnityEngine.SceneManagement
            .SceneManager.GetActiveScene().name);

        Debug.Log("Level restarted!");
    }
}