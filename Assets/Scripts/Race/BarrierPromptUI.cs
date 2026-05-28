using UnityEngine;
using TMPro;

public class BarrierPromptUI : MonoBehaviour
{
    [Header("UI Reference")]
    public TextMeshProUGUI promptText;

    [Header("Colors")]
    public Color farColor = Color.white;
    public Color nearColor = Color.yellow;
    public Color veryNearColor = Color.red;

    [Header("Settings")]
    public float detectionDistance = 3f;
    public float veryNearDistance = 1.5f;

    // All barriers in scene
    private BarrierController[] barriers;

    // Player reference
    private Transform player;

    void Start()
    {
        // Find player
        GameObject playerObj =
            GameObject.FindWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;

        // Find ALL barriers
        barriers = FindObjectsOfType
            <BarrierController>();

        // Hide prompt at start
        if (promptText != null)
            promptText.gameObject.SetActive(false);

        Debug.Log("BarrierPromptUI: Found " +
            barriers.Length + " barriers");
    }

    void Update()
    {
        if (player == null) return;

        bool nearBarrier = false;
        float closestDistance = float.MaxValue;

        // Check each barrier distance
        foreach (BarrierController barrier in barriers)
        {
            if (barrier == null) continue;

            float distance = Vector3.Distance(
                player.position,
                barrier.transform.position
            );

            if (distance <= detectionDistance)
            {
                nearBarrier = true;

                // Track closest barrier
                if (distance < closestDistance)
                    closestDistance = distance;
            }
        }

        // Show or hide prompt
        if (promptText != null)
        {
            promptText.gameObject
                .SetActive(nearBarrier);

            if (nearBarrier)
            {
                // Change color based on distance
                if (closestDistance <= veryNearDistance)
                {
                    // Very close — red and bigger
                    promptText.color = veryNearColor;
                    promptText.fontSize = 28;
                    promptText.text =
                        ">>> Press E to Toggle Barrier <<<";
                }
                else
                {
                    // Normal close — yellow
                    promptText.color = nearColor;
                    promptText.fontSize = 24;
                    promptText.text =
                        "Press E to Toggle Barrier";
                }
            }
        }
    }
}