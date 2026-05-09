using UnityEngine;
using MazeChase.Race;

public class WinLoseTrigger : MonoBehaviour
{
    private bool gameEnded = false;

    void OnTriggerEnter(Collider other)
    {
        if (gameEnded) return;

        if (other.CompareTag("Player"))
        {
            gameEnded = true;
            Debug.Log("PLAYER WINS!");

            // Connect to existing RaceGameManager
            RaceGameManager gm =
                FindAnyObjectByType<RaceGameManager>();
            if (gm != null)
            {
                RaceParticipant rp =
                    other.GetComponent<RaceParticipant>();
                if (rp != null)
                    gm.RegisterFinish(rp);
            }
        }
        else if (other.CompareTag("AI") ||
                 other.name.Contains("AI_Racer"))
        {
            gameEnded = true;
            Debug.Log("AI WINS!");

            RaceGameManager gm =
                FindAnyObjectByType<RaceGameManager>();
            if (gm != null)
            {
                RaceParticipant rp =
                    other.GetComponent<RaceParticipant>();
                if (rp != null)
                    gm.RegisterFinish(rp);
            }
        }
    }
}
