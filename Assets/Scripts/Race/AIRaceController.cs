using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using MazeChase.AI;

namespace MazeChase.Race
{
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(RaceParticipant))]
    public sealed class AIRaceController : MonoBehaviour
    {
        [Header("Race Setup")]
        [SerializeField] private Transform goal;
        [SerializeField] private float navMeshSampleRadius = 3.0f;

        [Header("AI Behaviour")]
        [SerializeField] private float aiDelay = 5f;
        [SerializeField] private float speedMultiplier = 1.15f;

        [Header("Algorithm Selection")]
        [SerializeField] private bool useUCS = true; // false = NavMesh fallback

        private NavMeshAgent agent;
        private RaceGameManager raceGameManager;
        private Animator animator;

        private SearchResult searchResult;
        private int waypointIndex = 0;
        private bool isMoving = false;

        // Built from GraphNodes in scene
        private List<Vector3> graphNodes = new();
        private Dictionary<Vector3, List<(Vector3, float)>> adjacency = new();

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            animator = GetComponentInChildren<Animator>();
            raceGameManager = FindAnyObjectByType<RaceGameManager>();

            if (goal == null)
            {
                GameObject g = GameObject.Find("GoalCheckpoint");
                goal = g != null ? g.transform : null;
            }
        }

        private void Start()
        {
            // Snap to NavMesh
            if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit,
                navMeshSampleRadius, NavMesh.AllAreas))
            {
                agent.Warp(hit.position);
            }

            agent.speed = agent.speed * speedMultiplier;

            // Read graph nodes from scene
            BuildGraphFromScene();

            // Start after delay
            StartCoroutine(DelayedStart());
        }

        private void Update()
        {
            if (raceGameManager != null && raceGameManager.RaceFinished)
            {
                agent.isStopped = true;
                SetAnim(false);
                return;
            }

            if (!isMoving) return;

            // Drive animation
            if (animator != null)
                animator.SetFloat("Speed", agent.velocity.magnitude);

            // Follow UCS waypoints
            if (searchResult != null && searchResult.pathFound)
            {
                if (!agent.pathPending &&
                    agent.remainingDistance <= agent.stoppingDistance + 0.3f)
                {
                    waypointIndex++;
                    if (waypointIndex < searchResult.path.Count)
                        MoveToWaypoint();
                    else
                        OnReachGoal();
                }
            }
        }

        // ─── Graph Builder ────────────────────────────────────────────────
        private void BuildGraphFromScene()
        {
            graphNodes.Clear();
            adjacency.Clear();

            // Collect all node positions from GraphNodes parent
            GameObject graphRoot = GameObject.Find("GraphNodes");
            if (graphRoot == null)
            {
                Debug.LogWarning("AIRaceController: GraphNodes object not found in scene.");
                return;
            }

            foreach (Transform child in graphRoot.transform)
                graphNodes.Add(child.position);

            // Build adjacency: connect nodes within 15 units of each other
            foreach (Vector3 node in graphNodes)
            {
                adjacency[node] = new List<(Vector3, float)>();
                foreach (Vector3 other in graphNodes)
                {
                    if (node == other) continue;
                    float dist = Vector3.Distance(node, other);
                    if (dist < 15f)
                        adjacency[node].Add((other, dist));
                }
            }

            Debug.Log($"AIRaceController: Graph built — {graphNodes.Count} nodes.");
        }

        // ─── Delayed Start ────────────────────────────────────────────────
        private IEnumerator DelayedStart()
        {
            SetAnim(false);
            Debug.Log($"AIRaceController: AI waiting {aiDelay}s before starting...");
            yield return new WaitForSeconds(aiDelay);

            if (useUCS && graphNodes.Count > 0 && goal != null)
            {
                // Run UCS
                UCSSearch ucs = new UCSSearch();
                searchResult = ucs.FindPath(
                    transform.position,
                    goal.position,
                    graphNodes,
                    adjacency
                );

                if (searchResult.pathFound)
                {
                    Debug.Log($"AIRaceController: UCS path found — " +
                              $"{searchResult.path.Count} waypoints, " +
                              $"cost {searchResult.totalCost:F1}, " +
                              $"{searchResult.expandedNodeCount} nodes expanded.");

                    // Pass results to debug visualizer if present
                    DebugVisualizer vis = FindAnyObjectByType<DebugVisualizer>();
                    if (vis != null)
                        vis.SetResults(searchResult, graphNodes, adjacency);

                    waypointIndex = 0;
                    isMoving = true;
                    SetAnim(true);
                    MoveToWaypoint();
                    yield break;
                }

                Debug.LogWarning("AIRaceController: UCS found no path — using NavMesh fallback.");
            }

            // NavMesh fallback
            if (goal != null)
            {
                agent.isStopped = false;
                agent.SetDestination(goal.position);
                isMoving = true;
                SetAnim(true);
            }
        }

        // ─── Movement ─────────────────────────────────────────────────────
        private void MoveToWaypoint()
        {
            if (waypointIndex >= searchResult.path.Count) return;
            agent.isStopped = false;
            agent.SetDestination(searchResult.path[waypointIndex]);
        }

        private void OnReachGoal()
        {
            isMoving = false;
            agent.isStopped = true;
            SetAnim(false);
            Debug.Log("AIRaceController: AI reached the goal!");

            RaceParticipant rp = GetComponent<RaceParticipant>();
            if (rp != null) raceGameManager?.RegisterFinish(rp);
        }

        private void SetAnim(bool moving)
        {
            if (animator != null)
                animator.SetFloat("Speed", moving ? 1f : 0f);
        }
    }
}