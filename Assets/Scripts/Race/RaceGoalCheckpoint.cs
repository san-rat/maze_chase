using UnityEngine;

namespace MazeChase.Race
{
    [RequireComponent(typeof(Collider))]
    public sealed class RaceGoalCheckpoint : MonoBehaviour
    {
        [SerializeField] private RaceGameManager raceGameManager;

        private void Reset()
        {
            Collider checkpointCollider = GetComponent<Collider>();
            checkpointCollider.isTrigger = true;
        }

        private void Awake()
        {
            if (raceGameManager == null)
            {
                raceGameManager = FindAnyObjectByType<RaceGameManager>();
            }

            Collider checkpointCollider = GetComponent<Collider>();
            checkpointCollider.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            RaceParticipant participant = other.GetComponentInParent<RaceParticipant>();
            if (participant == null)
            {
                return;
            }

            raceGameManager?.RegisterFinish(participant);
        }
    }
}
