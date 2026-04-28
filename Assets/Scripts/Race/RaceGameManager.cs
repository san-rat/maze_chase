using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MazeChase.Race
{
    public sealed class RaceGameManager : MonoBehaviour
    {
        [SerializeField] private Text statusText;

        public RaceParticipantKind Winner { get; private set; } = RaceParticipantKind.None;
        public bool RaceFinished => Winner != RaceParticipantKind.None;

        private void Start()
        {
            SetStatus("Race started: reach the red checkpoint first.");
        }

        public void RegisterFinish(RaceParticipant participant)
        {
            if (participant == null || RaceFinished)
            {
                return;
            }

            Winner = participant.ParticipantKind;
            SetStatus(Winner == RaceParticipantKind.Player ? "Player wins!" : "AI wins!");
            Debug.Log($"RaceGameManager: {Winner} reached the goal first.");
        }

        public void RestartRace()
        {
            Scene activeScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(activeScene.name);
        }

        private void SetStatus(string message)
        {
            if (statusText != null)
            {
                statusText.text = message;
            }

            Debug.Log($"RaceGameManager: {message}");
        }
    }
}
