using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

namespace MazeChase.Race
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;

        [Header("Countdown UI")]
        public TextMeshProUGUI countdownText;

        [Header("Win/Lose UI")]
        public GameObject winLosePanel;
        public TextMeshProUGUI winLoseText;
        public TextMeshProUGUI finalTimeText;

        [Header("Player & AI")]
        public GameObject player;
        public GameObject ai;

        private bool raceStarted = false;

        void Awake()
        {
            Instance = this;
        }

        void Start()
        {
            winLosePanel?.SetActive(false);
            countdownText?.gameObject.SetActive(true);

            // Freeze player input only — don't disable CharacterController
            if (player != null)
            {
                var input = player.GetComponent<StarterAssets.StarterAssetsInputs>();
                if (input != null)
                {
                    input.move = Vector2.zero;
                    input.enabled = false;
                }
            }

            // Stop AI
            if (ai != null)
            {
                var agent = ai.GetComponent<UnityEngine.AI.NavMeshAgent>();
                if (agent != null) agent.isStopped = true;
            }

            StartCoroutine(StartCountdown());
        }

        IEnumerator StartCountdown()
        {
            countdownText.gameObject.SetActive(true);

            countdownText.text = "3";
            yield return new WaitForSeconds(1f);

            countdownText.text = "2";
            yield return new WaitForSeconds(1f);

            countdownText.text = "1";
            yield return new WaitForSeconds(1f);

            countdownText.text = "GO!";
            yield return new WaitForSeconds(0.8f);

            countdownText.gameObject.SetActive(false);

            // Unlock player input
            if (player != null)
            {
                var input = player.GetComponent<StarterAssets.StarterAssetsInputs>();
                if (input != null) input.enabled = true;
            }

            // Unlock AI
            if (ai != null)
            {
                var agent = ai.GetComponent<UnityEngine.AI.NavMeshAgent>();
                if (agent != null) agent.isStopped = false;
            }

            // Start race timer
            if (RaceTimer.Instance != null)
                RaceTimer.Instance.StartTimer();

            raceStarted = true;
        }

        // Call this when someone finishes
        public void ShowResult(bool playerWon)
        {
            if (RaceTimer.Instance != null)
                RaceTimer.Instance.StopTimer();

            winLosePanel?.SetActive(true);

            if (playerWon)
            {
                winLoseText.text = "🏆 YOU WIN!";
                winLoseText.color = Color.green;
            }
            else
            {
                winLoseText.text = "❌ AI WINS!";
                winLoseText.color = Color.red;
            }

            if (finalTimeText != null && RaceTimer.Instance != null)
                finalTimeText.text = "Time: " + RaceTimer.Instance.GetTimeString();
        }

        // Restart button
        public void RestartGame()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        // Main menu button
        public void MainMenu()
        {
            SceneManager.LoadScene(0);
        }
    }
}