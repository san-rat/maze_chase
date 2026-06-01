using UnityEngine;
using UnityEngine.SceneManagement;
using MazeChase.Race;

namespace MazeChase.Race
{
    public class DashboardManager : MonoBehaviour
    {
        public static DashboardManager Instance;

        [Header("Dashboard UI")]
        public GameObject dashboardPanel;

        private bool isPaused = false;

        void Awake()
        {
            Instance = this;
        }

        // Called by Pause button
        public void TogglePause()
        {
            isPaused = !isPaused;

            if (isPaused)
            {
                Time.timeScale = 0f;
                Debug.Log("Game Paused");

                // Stop background music
                if (MusicManager.Instance != null)
                    MusicManager.Instance.Pause();
            }
            else
            {
                Time.timeScale = 1f;
                Debug.Log("Game Resumed");

                // Resume background music
                if (MusicManager.Instance != null)
                    MusicManager.Instance.Resume();
            }
        }

        // Called by Restart button
        public void RestartGame()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(
                SceneManager.GetActiveScene().name);
        }
    }
}