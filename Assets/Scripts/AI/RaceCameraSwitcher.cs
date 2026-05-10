using UnityEngine;

namespace MazeChase.AI
{
    public class RaceCameraSwitcher : MonoBehaviour
    {
        private GameObject playerFollowCam;
        private GameObject aiFollowCam;
        private GameObject topDownCamera;
        private bool isTopDown = false;
        private bool followingAI = false;

        void Start()
        {
            playerFollowCam = GameObject.Find("PlayerFollowCamera");
            aiFollowCam = GameObject.Find("AIFollowCamera");
            topDownCamera = GameObject.Find("TopDownCamera");

            // Disable AI camera at start
            if (aiFollowCam != null) aiFollowCam.SetActive(false);
            if (topDownCamera != null) topDownCamera.SetActive(false);

            Debug.Log("C = AI | V = birds eye | Tab = debug");
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.C))
            {
                if (isTopDown) return;
                followingAI = !followingAI;

                if (playerFollowCam != null) playerFollowCam.SetActive(!followingAI);
                if (aiFollowCam != null) aiFollowCam.SetActive(followingAI);

                Debug.Log(followingAI ? "Camera: AI" : "Camera: Player");
            }

            if (Input.GetKeyDown(KeyCode.V))
            {
                isTopDown = !isTopDown;

                if (isTopDown)
                {
                    if (playerFollowCam != null) playerFollowCam.SetActive(false);
                    if (aiFollowCam != null) aiFollowCam.SetActive(false);
                    if (topDownCamera != null) topDownCamera.SetActive(true);
                    Debug.Log("Birds eye ON");
                }
                else
                {
                    if (topDownCamera != null) topDownCamera.SetActive(false);
                    if (playerFollowCam != null) playerFollowCam.SetActive(!followingAI);
                    if (aiFollowCam != null) aiFollowCam.SetActive(followingAI);
                    Debug.Log("Birds eye OFF");
                }
            }

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