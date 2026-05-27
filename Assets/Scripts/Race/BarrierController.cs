using UnityEngine;

public class BarrierController : MonoBehaviour
{
    [Header("Barrier Settings")]
    public float raisedHeight = 1.2f;
    public float riseSpeed = 3f;
    public float activationDistance = 3f;

    [Header("Glow Settings")]
    public Color normalColor = Color.grey;
    public Color glowColor = Color.red;

    // Is wall currently raised?
    public bool isRaised = false;

    // Target position wall moves toward
    private Vector3 loweredPosition;
    private Vector3 raisedPosition;
    private Vector3 targetPosition;

    // Player reference
    private Transform player;

    // Renderer for glow effect
    private Renderer wallRenderer;
    private bool isGlowing = false;

    // Cooldown to prevent spam pressing
    private float toggleCooldown = 0f;

    void Start()
    {
        // Save both positions
        loweredPosition = transform.position;
        raisedPosition = loweredPosition +
            new Vector3(0, raisedHeight, 0);
        targetPosition = loweredPosition;

        // Get renderer for glow
        wallRenderer = GetComponent<Renderer>();
        if (wallRenderer != null)
            wallRenderer.material.color = normalColor;

        // Find player
        GameObject playerObj =
            GameObject.FindWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    void Update()
    {
        // Smooth movement toward target
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            riseSpeed * Time.deltaTime
        );

        // Count down cooldown
        if (toggleCooldown > 0)
            toggleCooldown -= Time.deltaTime;

        if (player == null) return;

        float distance = Vector3.Distance(
            transform.position, player.position);

        // Glow red when player is close
        if (distance <= activationDistance && !isGlowing)
        {
            isGlowing = true;
            if (wallRenderer != null)
                wallRenderer.material.color = glowColor;
        }
        else if (distance > activationDistance
            && isGlowing)
        {
            isGlowing = false;
            if (wallRenderer != null)
                wallRenderer.material.color = normalColor;
        }

        // Toggle on E press when close enough
        if (distance <= activationDistance &&
            Input.GetKeyDown(KeyCode.E) &&
            toggleCooldown <= 0)
        {
            ToggleBarrier();
        }
    }

    public void ToggleBarrier()
    {
        // Add cooldown to prevent spam
        toggleCooldown = 0.5f;

        isRaised = !isRaised;

        if (isRaised)
        {
            // Rise up
            targetPosition = raisedPosition;
            Debug.Log("Barrier raised: " +
                gameObject.name);

            // Camera shake
            if (CameraShake.Instance != null)
                CameraShake.Instance.Shake(0.3f, 0.2f);

            // Update barrier count UI
            if (BarrierCountUI.Instance != null)
                BarrierCountUI.Instance.BarrierUsed();
        }
        else
        {
            // Come back down
            targetPosition = loweredPosition;
            Debug.Log("Barrier lowered: " +
                gameObject.name);

            // Update barrier count UI back up
            if (BarrierCountUI.Instance != null)
                BarrierCountUI.Instance.BarrierRestored();
        }
    }

    // Rename old function to match new system
    public void ActivateBarrier()
    {
        ToggleBarrier();
    }
}