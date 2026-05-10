using UnityEngine;

public class BarrierController : MonoBehaviour
{
    [Header("Barrier Settings")]
    // How high the barrier rises when activated
    public float raisedHeight = 2.5f;

    // How fast the barrier rises
    public float riseSpeed = 3f;

    // How close player must be to activate
    public float activationDistance = 3f;

    [Header("State")]
    // Is barrier currently raised?
    public bool isRaised = false;

    // Has this barrier been used already?
    private bool isActivated = false;

    // Starting position of barrier
    private Vector3 startPosition;

    // Target position barrier moves toward
    private Vector3 targetPosition;

    // Reference to the player
    private Transform player;

    void Start()
    {
        // Save starting position
        startPosition = transform.position;
        targetPosition = startPosition;

        // Find the player by tag
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    void Update()
    {
        // Smoothly move barrier toward target
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            riseSpeed * Time.deltaTime
        );

        // Check if player is close enough
        if (player != null && !isActivated)
        {
            float distance = Vector3.Distance(
                transform.position,
                player.position
            );

            // If player is close and presses E
            if (distance <= activationDistance &&
                Input.GetKeyDown(KeyCode.E))
            {
                ActivateBarrier();
            }
        }
    }

    public void ActivateBarrier()
    {
        if (isActivated) return;

        isActivated = true;
        isRaised = true;

        // Move barrier up by raisedHeight
        targetPosition = startPosition +
            new Vector3(0, raisedHeight, 0);

        Debug.Log("Barrier activated: " + gameObject.name);
    }
}