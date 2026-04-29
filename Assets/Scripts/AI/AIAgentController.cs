using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using MazeChase.Race;

namespace MazeChase.AI
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class AIAgentController : MonoBehaviour
    {
        [Header("References")]
        public Transform goalTransform;
        public Animator animator;

        [Header("Settings")]
        public float aiDelay = 5f;
        public float aiSpeedMultiplier = 1.15f;

        [Header("Graph (assigned by Member 1's GraphBuilder)")]
        public List<Vector3> graphNodes = new List<Vector3>();
        public Dictionary<Vector3, List<(Vector3, float)>> adjacency;

        private NavMeshAgent agent;
        private SearchResult currentResult;
        private int waypointIndex = 0;
        private bool isMoving = false;
        private DebugVisualizer debugVis;

        void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            debugVis = FindAnyObjectByType<DebugVisualizer>();
        }

        void Start()
        {
            agent.speed = agent.speed * aiSpeedMultiplier;
            StartCoroutine(DelayedStart());
        }

        IEnumerator DelayedStart()
        {
            SetAnim(false);
            yield return new WaitForSeconds(aiDelay);

            // Run UCS
            if (graphNodes != null && graphNodes.Count > 0 && goalTransform != null)
            {
                UCSSearch ucs = new UCSSearch();
                currentResult = ucs.FindPath(
                    transform.position,
                    goalTransform.position,
                    graphNodes,
                    adjacency ?? BuildFallbackAdjacency()
                );

                if (debugVis != null)
                    debugVis.SetResults(currentResult, graphNodes, adjacency);

                if (currentResult.pathFound)
                {
                    isMoving = true;
                    waypointIndex = 0;
                    SetAnim(true);
                    MoveToNext();
                }
                else
                {
                    // Fallback: use NavMesh directly
                    Debug.LogWarning("UCS failed — falling back to NavMesh direct path.");
                    agent.SetDestination(goalTransform.position);
                    isMoving = true;
                    SetAnim(true);
                }
            }
            else
            {
                // No graph yet — fallback
                Debug.LogWarning("No graph nodes — using NavMesh fallback.");
                if (goalTransform != null) agent.SetDestination(goalTransform.position);
                isMoving = true;
                SetAnim(true);
            }
        }

        void Update()
        {
            if (!isMoving) return;

            // Animate from velocity
            if (animator != null)
                animator.SetFloat("Speed", agent.velocity.magnitude);

            // Waypoint following
            if (currentResult != null && currentResult.pathFound)
            {
                if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.2f)
                {
                    waypointIndex++;
                    if (waypointIndex < currentResult.path.Count)
                        MoveToNext();
                    else
                        OnReachGoal();
                }
            }
        }

        void MoveToNext()
        {
            if (waypointIndex >= currentResult.path.Count) return;
            agent.SetDestination(currentResult.path[waypointIndex]);
        }

        void OnReachGoal()
        {
            isMoving = false;
            SetAnim(false);
            Debug.Log("AI reached the goal!");

            RaceGameManager gm = FindAnyObjectByType<RaceGameManager>();
            if (gm != null)
            {
                RaceParticipant rp = GetComponent<RaceParticipant>();
                if (rp != null) gm.RegisterFinish(rp);
            }
        }

        void SetAnim(bool moving)
        {
            if (animator != null)
                animator.SetFloat("Speed", moving ? 1f : 0f);
        }

        // Fallback: connect every node to its nearest neighbors by distance
        Dictionary<Vector3, List<(Vector3, float)>> BuildFallbackAdjacency()
        {
            var adj = new Dictionary<Vector3, List<(Vector3, float)>>();
            foreach (Vector3 node in graphNodes)
            {
                adj[node] = new List<(Vector3, float)>();
                foreach (Vector3 other in graphNodes)
                {
                    if (node == other) continue;
                    float d = Vector3.Distance(node, other);
                    if (d < 5f) adj[node].Add((other, d));
                }
            }
            return adj;
        }
    }
}