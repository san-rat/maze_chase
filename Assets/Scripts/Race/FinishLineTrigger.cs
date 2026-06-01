using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using MazeChase.Race;

public class FinishLineTrigger : MonoBehaviour
{
    [Header("UI")]
    public GameObject winLosePanel;
    public TextMeshProUGUI winLoseText;
    public TextMeshProUGUI finalTimeText;

    private bool gameEnded = false;

    void Start()
    {
        Debug.Log("FinishLineTrigger is ACTIVE on: " + gameObject.name);
        if (winLosePanel != null)
            winLosePanel.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("FinishLine hit by: " + other.name);
        if (gameEnded) return;

        bool isPlayer = other.CompareTag("Player") ||
                        other.name.Contains("Player") ||
                        other.name.Contains("Armature");

        bool isAI = other.name.Contains("AI") ||
                    other.name.Contains("Robot") ||
                    other.name.Contains("Racer");

        if (isPlayer) { gameEnded = true; PlayerWins(); }
        else if (isAI) { gameEnded = true; AIWins(); }
    }

    void OnTriggerStay(Collider other)
    {
        OnTriggerEnter(other);
    }

    public void AIWinsDirectCall()
    {
        if (gameEnded) return;
        gameEnded = true;
        AIWins();
    }

    void PlayerWins()
    {
        Debug.Log("=== PLAYER WINS ===");
        if (RaceTimer.Instance != null)
            RaceTimer.Instance.StopTimer();

        if (winLosePanel != null)
        {
            winLosePanel.SetActive(true);
            if (winLoseText != null)
            {
                winLoseText.text = "YOU WIN!";
                winLoseText.color = Color.green;
            }
            if (finalTimeText != null && RaceTimer.Instance != null)
                finalTimeText.text = "Time: " +
                    RaceTimer.Instance.GetTimeString();
        }
        Time.timeScale = 0f;
    }

    void AIWins()
    {
        Debug.Log("=== AI WINS ===");
        if (RaceTimer.Instance != null)
            RaceTimer.Instance.StopTimer();

        if (winLosePanel != null)
        {
            winLosePanel.SetActive(true);
            if (winLoseText != null)
            {
                winLoseText.text = "AI WINS!";
                winLoseText.color = Color.red;
            }
            if (finalTimeText != null && RaceTimer.Instance != null)
                finalTimeText.text = "Time: " +
                    RaceTimer.Instance.GetTimeString();
        }
        Time.timeScale = 0f;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(
            SceneManager.GetActiveScene().name);
    }

    public void MainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }
}   