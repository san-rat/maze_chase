using UnityEngine;

namespace MazeChase.Race
{
    public enum RaceParticipantKind
    {
        None,
        Player,
        AI
    }

    public sealed class RaceParticipant : MonoBehaviour
    {
        [SerializeField] private RaceParticipantKind participantKind = RaceParticipantKind.None;

        public RaceParticipantKind ParticipantKind => participantKind;
        public Transform ParticipantTransform => transform;
    }
}
