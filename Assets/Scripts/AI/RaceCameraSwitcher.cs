using UnityEngine;

namespace MazeChase.AI
{
    public class RaceCameraSwitcher : MonoBehaviour
    {
        private GameObject playerCamera;
        private GameObject aiCamera;
        private GameObject topDownCamera;
        private bool isTopDown = false;

        void Start()
        {
            playerCamera = GameObject.Find("PlayerFollowCamera");
            aiCamera = GameObject.Find("AIFollowCamera");
            topDownCamera = GameObject.Find("TopDownCamera");

            if (playerCamera == null) Debug.LogWarning("PlayerFollowCamera not found!");
            if (aiCamera == null) Debug.LogWarning("AIFollowCamera not found!");
            if (topDownCamera == null) Debug.LogWarning("TopDownCamera not found!");

            // Start with player camera
            if (playerCamera != null) playerCamera.SetActive(true);
            if (aiCamera != null) aiCamera.SetActive(false);
            if (topDownCamera != null) topDownCamera.SetActive(false);

            Debug.Log("C = AI camera | V = birds eye view | Scroll = zoom | Tab = debug nodes");
        }

        void Update()
        {
            // C — switch between player and AI camera
            if (Input.GetKeyDown(KeyCode.C))
            {
                if (isTopDown) return; // don't switch while in top down

                bool playerActive = playerCamera != null && playerCamera.activeSelf;
                if (playerCamera != null) playerCamera.SetActive(!playerActive);
                if (aiCamera != null) aiCamera.SetActive(playerActive);

                Debug.Log(playerActive ? "Camera: AI" : "Camera: Player");
            }

            // V — toggle birds eye top down view
            if (Input.GetKeyDown(KeyCode.V))
            {
                isTopDown = !isTopDown;

                if (isTopDown)
                {
                    // Switch to top down
                    if (playerCamera != null) playerCamera.SetActive(false);
                    if (aiCamera != null) aiCamera.SetActive(false);
                    if (topDownCamera != null) topDownCamera.SetActive(true);
                    Debug.Log("Birds eye view ON — scroll to zoom");
                }
                else
                {
                    // Switch back to player
                    if (playerCamera != null) playerCamera.SetActive(true);
                    if (aiCamera != null) aiCamera.SetActive(false);
                    if (topDownCamera != null) topDownCamera.SetActive(false);
                    Debug.Log("Birds eye view OFF");
                }
            }

            // Scroll wheel — zoom in top down view
            if (isTopDown && topDownCamera != null)
            {
                Camera cam = topDownCamera.GetComponent<Camera>();
                if (cam != null && cam.orthographic)
                {
                    cam.orthographicSize -= Input.mouseScrollDelta.y * 5f;
                    cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, 20f, 200f);
                }
            }
        }
    }
}