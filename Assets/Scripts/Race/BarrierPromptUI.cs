using UnityEngine;
using TMPro;

public class BarrierPromptUI : MonoBehaviour
{
    [Header("UI Reference")]
    public TextMeshProUGUI promptText;

    [Header("Settings")]
    public float detectionDistance = 3f;

    private BarrierController[] barriers;
    private Transform player;

    void Start()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
        else
            Debug.LogWarning("BarrierPromptUI: No object tagged Player found!");

        barriers = FindObjectsOfType<BarrierController>();

        if (promptText != null)
            promptText.gameObject.SetActive(false);
        else
            Debug.LogWarning("BarrierPromptUI: promptText not assigned!");
    }

    void Update()
    {
        // Null checks before doing anything
        if (player == null || promptText == null || barriers == null) return;

        bool nearBarrier = false;

        foreach (BarrierController barrier in barriers)
        {
            // Skip null or already raised barriers
            if (barrier == null) continue;
            if (barrier.isRaised) continue;

            float distance = Vector3.Distance(
                player.position,
                barrier.transform.position);

            if (distance <= detectionDistance)
            {
                nearBarrier = true;
                break;
            }
        }

        promptText.gameObject.SetActive(nearBarrier);
    }
}