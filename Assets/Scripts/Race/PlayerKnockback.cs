using UnityEngine;

public class PlayerKnockback : MonoBehaviour
{
    [Header("Knockback Settings")]
    public float knockbackForce = 5f;
    public float knockbackDuration = 0.2f;

    // Is player currently being knocked back?
    private bool isKnockedBack = false;
    private float knockbackTimer = 0f;
    private Vector3 knockbackDirection;

    private CharacterController controller;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (!isKnockedBack) return;

        // Apply knockback movement
        controller.Move(knockbackDirection *
            knockbackForce * Time.deltaTime);

        knockbackTimer -= Time.deltaTime;
        if (knockbackTimer <= 0f)
            isKnockedBack = false;
    }

    // Called when player hits a wall
    void OnControllerColliderHit(
        ControllerColliderHit hit)
    {
        // Only react to HalfWall objects
        if (!hit.gameObject.name.Contains("HalfWall") &&
            !hit.gameObject.name.Contains("OverheadWall"))
            return;

        // Calculate bounce direction
        knockbackDirection = -hit.moveDirection;
        knockbackDirection.y = 0.3f;

        isKnockedBack = true;
        knockbackTimer = knockbackDuration;

        // Shake camera on impact
        if (CameraShake.Instance != null)
            CameraShake.Instance.Shake(0.15f, 0.15f);

        Debug.Log("Player knocked back by wall!");
    }
}