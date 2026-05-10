using UnityEngine;

namespace MazeChase.AI
{
    public class AICameraFollow : MonoBehaviour
    {
        public Transform target;        // AI_Racer_Robot
        public Vector3 offset = new Vector3(0f, 3f, -6f);
        public float smoothSpeed = 5f;

        void LateUpdate()
        {
            if (target == null) return;

            // Follow position
            Vector3 desiredPos = target.position +
                                 target.TransformDirection(offset);
            transform.position = Vector3.Lerp(
                transform.position, desiredPos,
                smoothSpeed * Time.deltaTime);

            // Look at AI
            transform.LookAt(target.position + Vector3.up * 1.5f);
        }
    }
}