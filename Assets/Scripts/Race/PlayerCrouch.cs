using UnityEngine;

public class PlayerCrouch : MonoBehaviour
{
    // Reference to the Character Controller
    private CharacterController controller;

    // Normal height and crouched height
    private float normalHeight = 1.8f;
    private float crouchHeight = 0.9f;

    // Normal center and crouched center
    private Vector3 normalCenter = new Vector3(0, 0.9f, 0);
    private Vector3 crouchCenter = new Vector3(0, 0.45f, 0);

    // Is player currently crouching?
    private bool isCrouching = false;

    void Start()
    {
        // Get the CharacterController on this object
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        // Check if Z key is pressed this frame
        if (Input.GetKeyDown(KeyCode.Z))
        {
            isCrouching = true;
            controller.height = crouchHeight;
            controller.center = crouchCenter;
            Debug.Log("Crouching!");
        }

        // Check if Z key is released
        if (Input.GetKeyUp(KeyCode.Z))
        {
            isCrouching = false;
            controller.height = normalHeight;
            controller.center = normalCenter;
            Debug.Log("Standing up!");
        }
    }
}