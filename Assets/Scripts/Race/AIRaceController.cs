using UnityEngine;
using UnityEngine.AI;

namespace MazeChase.Race
{
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(RaceParticipant))]
    public sealed class AIRaceController : MonoBehaviour
    {
        [SerializeField] private Transform goal;
        [SerializeField] private float repathInterval = 1.0f;
        [SerializeField] private float navMeshSampleRadius = 3.0f;

        private NavMeshAgent agent;
        private RaceGameManager raceGameManager;
        private float nextRepathTime;

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            raceGameManager = FindAnyObjectByType<RaceGameManager>();

            if (goal == null)
            {
                GameObject goalObject = GameObject.Find("GoalCheckpoint");
                goal = goalObject != null ? goalObject.transform : null;
            }
        }

        private void Start()
        {
            if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, navMeshSampleRadius, NavMesh.AllAreas))
            {
                agent.Warp(hit.position);
            }

            SetGoalDestination();
        }

        private void Update()
        {
            if (raceGameManager != null && raceGameManager.RaceFinished)
            {
                agent.isStopped = true;
                return;
            }

            if (Time.time >= nextRepathTime)
            {
                SetGoalDestination();
            }
        }

        private void SetGoalDestination()
        {
            nextRepathTime = Time.time + repathInterval;

            if (goal == null || !agent.isOnNavMesh)
            {
                return;
            }

            agent.isStopped = false;
            agent.SetDestination(goal.position);
        }
    }
}
