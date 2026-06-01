using UnityEngine;
using TMPro;
using MazeChase.Race;

public class WinLoseTrigger : MonoBehaviour
{
    [Header("UI")]
    public GameObject winLosePanel;
    public TextMeshProUGUI winLoseText;

    private bool gameEnded = false;

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("=== TRIGGER ENTER: " + other.name + " tag: " + other.tag);
        if (gameEnded) return;

        bool isPlayer = other.CompareTag("Player") ||
                        other.name.Contains("Player");
        bool isAI = other.CompareTag("AI") ||
                    other.name.Contains("AI_Racer") ||
                    other.name.Contains("Robot");

        if (isPlayer)
        {
            gameEnded = true;
            Debug.Log("PLAYER WINS!");
            if (GameManager.Instance != null)
                GameManager.Instance.ShowResult(true);
            else
                Debug.LogError("GameManager.Instance is NULL!");
        }
        else if (isAI)
        {
            gameEnded = true;
            Debug.Log("AI WINS!");
            if (GameManager.Instance != null)
                GameManager.Instance.ShowResult(false);
            else
                Debug.LogError("GameManager.Instance is NULL!");
        }
    }

    void OnCollisionEnter(Collision other)
    {
        Debug.Log("=== COLLISION ENTER: " + other.gameObject.name);
    }
}