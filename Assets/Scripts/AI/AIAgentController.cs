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

        [Header("Algorithm Selection")]
        // 0 = UCS, 1 = BFS, 2 = A*
        public int currentAlgorithmIndex = 0;

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
            agent.speed *= aiSpeedMultiplier;

            // Get graph from AIRaceController
            if (graphNodes == null || graphNodes.Count == 0)
            {
                AIRaceController rc = FindAnyObjectByType<AIRaceController>();
                if (rc != null)
                {
                    graphNodes = rc.GetGraphNodes();
                    adjacency = rc.GetAdjacency();
                    Debug.Log($"AIAgentController: Got {graphNodes.Count} nodes from AIRaceController");
                }
                else
                {
                    Debug.LogWarning("AIAgentController: AIRaceController not found!");
                }
            }

            StartCoroutine(DelayedStart());
        }

        // UI: Change algorithm at runtime
        public void SetAlgorithm(int index)
        {
            currentAlgorithmIndex = index;
            Debug.Log($"AI: Algorithm {index} selected. Recalculating path...");
            StartCoroutine(RecalculatePath());
        }

        // Select algorithm
        private SearchResult PerformSearch()
        {
            var adjMap = adjacency ?? BuildFallbackAdjacency();

            switch (currentAlgorithmIndex)
            {
                case 1:
                    return new BFSSearch().FindPath(transform.position, goalTransform.position, graphNodes, adjMap);

                case 2:
                    return new AStarSearch().FindPath(transform.position, goalTransform.position, graphNodes, adjMap);

                default:
                    return new UCSSearch().FindPath(transform.position, goalTransform.position, graphNodes, adjMap);
            }
        }

        public IEnumerator RecalculatePath()
        {
            Debug.Log("AI: Calculating path...");
            yield return new WaitForSeconds(0.1f);

            if (graphNodes == null || graphNodes.Count == 0)
            {
                Debug.LogWarning("No graph nodes found. Using fallback NavMesh.");
                FallbackToNavMesh();
                yield break;
            }

            var adjMap = adjacency ?? BuildFallbackAdjacency();

            currentResult = PerformSearch();

            if (debugVis != null)
                debugVis.SetResults(currentResult, graphNodes, adjacency);

            if (currentResult != null && currentResult.pathFound)
            {
                waypointIndex = 0;
                isMoving = true;
                SetAnim(true);
                MoveToNext();
            }
            else
            {
                Debug.LogWarning("AI: Path not found. Using fallback.");
                FallbackToNavMesh();
            }
        }

        IEnumerator DelayedStart()
        {
            SetAnim(false);
            yield return new WaitForSeconds(aiDelay);
            Debug.Log("AI ready. Select algorithm to start.");
        }

        private void FallbackToNavMesh()
        {
            if (goalTransform == null) return;

            agent.SetDestination(goalTransform.position);
            isMoving = true;
            SetAnim(true);
        }

        void Update()
        {
            if (!isMoving) return;

            if (animator != null)
                animator.SetFloat("Speed", agent.velocity.magnitude);

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
            if (currentResult == null || waypointIndex >= currentResult.path.Count) return;
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
                    if (d < 5f)
                        adj[node].Add((other, d));
                }
            }

            return adj;
        }

        // --- FOOTSTEP EVENTS ---
        public void OnFootstep(AnimationEvent animationEvent) { }
        public void OnLand(AnimationEvent animationEvent) { }
    }
}