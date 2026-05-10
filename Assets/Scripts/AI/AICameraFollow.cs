using UnityEngine;

namespace MazeChase.AI
{
    public class AICameraFollow : MonoBehaviour
    {
        public Transform target;
        public Vector3 offset = new Vector3(0f, 4f, -7f);
        public float smoothSpeed = 8f;
        public float rotationSpeed = 5f;

        void LateUpdate()
        {
            if (target == null) return;

            // Calculate desired position behind and above AI
            Vector3 desiredPos = target.position +
                                 target.forward * offset.z +
                                 Vector3.up * offset.y;

            // Smooth position
            transform.position = Vector3.Lerp(
                transform.position,
                desiredPos,
                smoothSpeed * Time.deltaTime);

            // Smooth rotation — look at AI slightly above ground
            Quaternion desiredRot = Quaternion.LookRotation(
                (target.position + Vector3.up * 1.5f) - transform.position);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                desiredRot,
                rotationSpeed * Time.deltaTime);
        }
    }
}