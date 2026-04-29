using UnityEngine;

namespace MazeChase.AI
{
    public class RaceCameraSwitcher : MonoBehaviour
    {
        private GameObject playerCamera;
        private GameObject aiCamera;
        private bool followingPlayer = true;

        void Start()
        {
            // Find cameras automatically by name
            playerCamera = GameObject.Find("PlayerFollowCamera");
            aiCamera = GameObject.Find("AIFollowCamera");

            if (playerCamera == null)
                Debug.LogWarning("RaceCameraSwitcher: PlayerFollowCamera not found!");
            if (aiCamera == null)
                Debug.LogWarning("RaceCameraSwitcher: AIFollowCamera not found!");

            // Start with player camera active
            if (playerCamera != null) playerCamera.SetActive(true);
            if (aiCamera != null) aiCamera.SetActive(false);

            Debug.Log("RaceCameraSwitcher ready — press C to switch cameras.");
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.C))
            {
                if (playerCamera == null || aiCamera == null) return;

                followingPlayer = !followingPlayer;
                playerCamera.SetActive(followingPlayer);
                aiCamera.SetActive(!followingPlayer);

                Debug.Log($"Camera: {(followingPlayer ? "Player" : "AI")}");
            }
        }
    }
}