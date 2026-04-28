using UnityEngine;

namespace MazeChase.Race
{
    public sealed class ThirdPersonCameraBinder : MonoBehaviour
    {
        [SerializeField] private Transform playerRoot;
        [SerializeField] private Transform cameraTarget;

        private void Start()
        {
            if (playerRoot == null)
            {
                GameObject player = GameObject.Find("Player_Racer");
                playerRoot = player != null ? player.transform : null;
            }

            if (cameraTarget == null && playerRoot != null)
            {
                cameraTarget = FindChildRecursive(playerRoot, "PlayerCameraRoot");
            }

            if (cameraTarget == null)
            {
                Debug.LogWarning("ThirdPersonCameraBinder: PlayerCameraRoot was not found.");
                return;
            }

            Component[] components = GetComponentsInChildren<Component>(true);
            foreach (Component component in components)
            {
                if (component == null || !component.GetType().Name.StartsWith("Cinemachine"))
                {
                    continue;
                }

                System.Type type = component.GetType();
                type.GetProperty("Follow")?.SetValue(component, cameraTarget);
                type.GetProperty("LookAt")?.SetValue(component, cameraTarget);
            }
        }

        private static Transform FindChildRecursive(Transform root, string childName)
        {
            foreach (Transform child in root)
            {
                if (child.name == childName)
                {
                    return child;
                }

                Transform nested = FindChildRecursive(child, childName);
                if (nested != null)
                {
                    return nested;
                }
            }

            return null;
        }
    }
}
