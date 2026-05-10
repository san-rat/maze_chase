using UnityEngine;
using MazeChase.Race;

public class PowerOrb : MonoBehaviour
{
    [Header("Float Settings")]
    public float floatSpeed = 1.5f;
    public float floatHeight = 0.3f;

    [Header("Grab Settings")]
    public float grabDistance = 2f;

    // Starting Y position
    private float startY;

    // Has orb been grabbed?
    private bool isGrabbed = false;

    // Player reference
    private Transform player;

    void Start()
    {
        startY = transform.position.y;

        GameObject playerObj =
            GameObject.FindWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    void Update()
    {
        if (isGrabbed) return;

        // Float up and down smoothly
        float newY = startY + Mathf.Sin(
            Time.time * floatSpeed) * floatHeight;
        transform.position = new Vector3(
            transform.position.x,
            newY,
            transform.position.z
        );

        // Rotate slowly for visual effect
        transform.Rotate(0, 50f * Time.deltaTime, 0);

        // Check player distance and E key
        if (player != null)
        {
            float distance = Vector3.Distance(
                transform.position,
                player.position
            );

            if (distance <= grabDistance &&
                Input.GetKeyDown(KeyCode.E))
            {
                GrabOrb();
            }
        }
    }

    void GrabOrb()
    {
        isGrabbed = true;
        Debug.Log("PowerOrb grabbed! AI recalculating!");

        // Trigger AI recalculation
        if (DynamicGraphUpdater.Instance != null)
            DynamicGraphUpdater.Instance
                .NotifyRecalculation();

        // Hide the orb
        gameObject.SetActive(false);
    }
}