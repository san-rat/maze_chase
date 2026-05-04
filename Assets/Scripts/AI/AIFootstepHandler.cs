using UnityEngine;

namespace MazeChase.AI
{
    public class AIFootstepHandler : MonoBehaviour
    {
        // Receives footstep animation events from StarterAssets animations
        // Intentionally empty — AI has no footstep sounds
        private void OnFootstep(AnimationEvent animationEvent)
        {
            // Silently handle footstep event
        }

        private void OnLand(AnimationEvent animationEvent)
        {
            // Silently handle land event
        }
    }
}