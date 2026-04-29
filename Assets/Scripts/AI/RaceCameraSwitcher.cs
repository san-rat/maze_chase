using UnityEngine;

namespace MazeChase.AI
{
    public class RaceCameraSwitcher : MonoBehaviour
    {
        [Header("Press C to switch cameras")]
        public GameObject playerCamera;
        public GameObject aiCamera;

        void Start()
        {
            if (playerCamera != null) playerCamera.SetActive(true);
            if (aiCamera != null) aiCamera.SetActive(false);
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.C))
            {
                bool showingPlayer = playerCamera.activeSelf;
                playerCamera.SetActive(!showingPlayer);
                aiCamera.SetActive(showingPlayer);
                Debug.Log(showingPlayer ? "Switched to AI camera" : "Switched to Player camera");
            }
        }
    }
}