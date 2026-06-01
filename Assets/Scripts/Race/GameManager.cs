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

        [Header("Countdown Sound")]
        public AudioClip beepSound;
        public AudioClip goBeepSound; // louder/different for GO!

        [Header("Win/Lose UI")]
        public GameObject winLosePanel;
        public TextMeshProUGUI winLoseText;
        public TextMeshProUGUI finalTimeText;

        [Header("Player & AI")]
        public GameObject player;
        public GameObject ai;

        private bool raceStarted = false;
        private AudioSource audioSource;

        void Awake()
        {
            Instance = this;
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();
        }

        void Start()
        {
            winLosePanel?.SetActive(false);
            countdownText?.gameObject.SetActive(true);

            // Freeze player input
            if (player != null)
            {
                var input = player.GetComponent
                    <StarterAssets.StarterAssetsInputs>();
                if (input != null)
                {
                    input.move = Vector2.zero;
                    input.enabled = false;
                }
            }

            // Stop AI
            if (ai != null)
            {
                var agent = ai.GetComponent
                    <UnityEngine.AI.NavMeshAgent>();
                if (agent != null) agent.isStopped = true;
            }

            StartCoroutine(StartCountdown());
        }

        IEnumerator StartCountdown()
        {
            countdownText.gameObject.SetActive(true);

            // Play the full beep sequence at start
            if (beepSound != null)
                audioSource.PlayOneShot(beepSound);

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
                var input = player.GetComponent
                    <StarterAssets.StarterAssetsInputs>();
                if (input != null) input.enabled = true;
            }

            // Unlock AI
            if (ai != null)
            {
                var agent = ai.GetComponent
                    <UnityEngine.AI.NavMeshAgent>();
                if (agent != null) agent.isStopped = false;
            }

            if (RaceTimer.Instance != null)
                RaceTimer.Instance.StartTimer();

            raceStarted = true;
        }

        void PlayBeep(bool isGo)
        {
            if (isGo)
            {
                // Play go beep if assigned
                // otherwise fall back to normal beep
                if (goBeepSound != null)
                    audioSource.PlayOneShot(goBeepSound);
                else if (beepSound != null)
                    audioSource.PlayOneShot(beepSound);
            }
            else
            {
                if (beepSound != null)
                    audioSource.PlayOneShot(beepSound);
            }
        }

        public void ShowResult(bool playerWon)
        {
            if (RaceTimer.Instance != null)
                RaceTimer.Instance.StopTimer();

            // Stop music on game end
            if (MusicManager.Instance != null)
                MusicManager.Instance.Pause();

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

            if (finalTimeText != null &&
                RaceTimer.Instance != null)
                finalTimeText.text = "Time: " +
                    RaceTimer.Instance.GetTimeString();
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
}