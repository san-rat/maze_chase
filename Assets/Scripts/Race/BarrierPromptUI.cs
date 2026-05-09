using UnityEngine;
using TMPro;

public class BarrierPromptUI : MonoBehaviour
{
    [Header("UI Reference")]
    public TextMeshProUGUI promptText;

    [Header("Settings")]
    public float detectionDistance = 3f;

    // All barriers in the scene
    private BarrierController[] barriers;

    // Reference to player
    private Transform player;

    void Start()
    {
        // Find player
        GameObject playerObj =
            GameObject.FindWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;

        // Find all barriers in scene
        barriers = FindObjectsOfType<BarrierController>();

        // Hide prompt at start
        promptText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (player == null) return;

        bool nearBarrier = false;

        // Check each barrier
        foreach (BarrierController barrier in barriers)
        {
            // Skip already activated barriers
            if (barrier.isRaised) continue;

            float distance = Vector3.Distance(
                player.position,
                barrier.transform.position
            );

            // If close enough show prompt
            if (distance <= detectionDistance)
            {
                nearBarrier = true;
                break;
            }
        }

        // Show or hide prompt
        promptText.gameObject.SetActive(nearBarrier);
    }
}