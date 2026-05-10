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
        [SerializeField] private float navMeshSampleRadius = 5.0f;

        [Header("AI Behaviour")]
        [SerializeField] private float aiDelay = 5f;
        [SerializeField] private float speedMultiplier = 1.15f;

        [Header("Algorithm Selection")]
        [SerializeField] private int algorithmIndex = 0; // 0=UCS 1=BFS 2=AStar

        private NavMeshAgent agent;
        private RaceGameManager raceGameManager;
        private Animator animator;

        private SearchResult searchResult;
        private int waypointIndex = 0;
        private bool isMoving = false;
        private bool algorithmSelected = false;
        private bool isPaused = false;

        private List<Vector3> graphNodes = new();
        private Dictionary<Vector3, List<(Vector3, float)>> adjacency = new();

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            animator = GetComponentInChildren<Animator>();
            raceGameManager = FindAnyObjectByType<RaceGameManager>();

            if (animator == null)
                Debug.LogWarning("AIRaceController: No Animator found!");
            else
                Debug.Log("AIRaceController: Animator found — " + animator.gameObject.name);

            if (goal == null)
            {
                GameObject g = GameObject.Find("TestDestination_Exit");
                goal = g != null ? g.transform : null;
            }
        }

        private void Start()
        {
            if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit,
                navMeshSampleRadius, NavMesh.AllAreas))
            {
                agent.Warp(hit.position);
                Vector3 pos = transform.position;
                pos.y = hit.position.y;
                transform.position = pos;
            }

            agent.speed = agent.speed * speedMultiplier;
            agent.updateRotation = true;
            agent.updateUpAxis = false;
            agent.isStopped = true;

            BuildGraphFromScene();
            StartCoroutine(WaitForAlgorithmSelection());
        }

        // Called by AlgorithmSelectorUI — bool version for UCS/BFS
        public void SetAlgorithm(bool ucs)
        {
            algorithmIndex = ucs ? 0 : 1;
            algorithmSelected = true;
            Debug.Log($"AIRaceController: Algorithm set to {(ucs ? "UCS" : "BFS")}");
        }

        // Called by AlgorithmSelectorUI — index version for all 3
        public void SetAlgorithmIndex(int index)
        {
            algorithmIndex = index;
            algorithmSelected = true;
            string name = index == 0 ? "UCS" : index == 1 ? "BFS" : "A*";
            Debug.Log($"AIRaceController: Algorithm set to {name}");
        }

        public void PauseAI(bool pause)
        {
            isPaused = pause;
            agent.isStopped = pause;
        }

        private void Update()
        {
            if (raceGameManager != null && raceGameManager.RaceFinished)
            {
                agent.isStopped = true;
                SetAnim(false);
                return;
            }

            if (!isMoving || isPaused) return;

            if (animator != null)
            {
                float speed = agent.velocity.magnitude;
                animator.SetFloat("Speed", speed, 0.15f, Time.deltaTime);
                animator.SetFloat("MotionSpeed", 1f);
                animator.SetBool("Grounded", true);
                animator.SetBool("FreeFall", false);
                animator.SetBool("Jump", false);
            }

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

        private IEnumerator WaitForAlgorithmSelection()
        {
            Debug.Log("AIRaceController: Waiting for algorithm selection...");
            yield return new WaitUntil(() => algorithmSelected);
            Debug.Log("AIRaceController: Algorithm selected! Starting race...");
            yield return StartCoroutine(DelayedStart());
        }

        private void BuildGraphFromScene()
        {
            graphNodes.Clear();
            adjacency.Clear();

            Vector3 mazeCenter = new Vector3(-41f, -1.66f, -907f);
            float stepSize = 2.5f;
            float range = 150f;
            float[] yHeights = new float[] { 0f, 0.5f, 1f, -0.5f, 2f, -1f };

            for (float x = -range; x <= range; x += stepSize)
            {
                for (float z = -range; z <= range; z += stepSize)
                {
                    foreach (float y in yHeights)
                    {
                        Vector3 samplePos = new Vector3(
                            mazeCenter.x + x,
                            mazeCenter.y + y,
                            mazeCenter.z + z);

                        if (NavMesh.SamplePosition(samplePos, out NavMeshHit hit, 2f, NavMesh.AllAreas))
                        {
                            bool tooClose = false;
                            foreach (Vector3 existing in graphNodes)
                            {
                                if (Vector3.Distance(hit.position, existing) < stepSize * 0.8f)
                                {
                                    tooClose = true;
                                    break;
                                }
                            }
                            if (!tooClose)
                            {
                                graphNodes.Add(hit.position);
                                break;
                            }
                        }
                    }
                }
            }

            foreach (Vector3 node in graphNodes)
            {
                adjacency[node] = new List<(Vector3, float)>();
                foreach (Vector3 other in graphNodes)
                {
                    if (node == other) continue;
                    float dist = Vector3.Distance(node, other);
                    if (dist < 6f)
                        adjacency[node].Add((other, dist));
                }
            }

            Debug.Log($"AIRaceController: NavMesh graph built — {graphNodes.Count} nodes.");
            Debug.Log($"AIRaceController: Maze centre used — {mazeCenter}");
        }

        private IEnumerator DelayedStart()
        {
            if (animator != null)
            {
                animator.SetFloat("Speed", 0f);
                animator.SetFloat("MotionSpeed", 1f);
                animator.SetBool("Grounded", true);
                animator.SetBool("FreeFall", false);
                animator.SetBool("Jump", false);
            }

            Debug.Log($"AIRaceController: AI waiting {aiDelay}s...");
            yield return new WaitForSeconds(aiDelay);

            if (graphNodes.Count > 0 && goal != null)
            {
                // Run selected algorithm
                if (algorithmIndex == 0)
                {
                    UCSSearch ucs = new UCSSearch();
                    searchResult = ucs.FindPath(
                        transform.position, goal.position,
                        graphNodes, adjacency);
                    Debug.Log("AIRaceController: Running UCS...");
                }
                else if (algorithmIndex == 1)
                {
                    BFSSearch bfs = new BFSSearch();
                    searchResult = bfs.FindPath(
                        transform.position, goal.position,
                        graphNodes, adjacency);
                    Debug.Log("AIRaceController: Running BFS...");
                }
                else if (algorithmIndex == 2)
                {
                    AStarSearch astar = new AStarSearch();
                    searchResult = astar.FindPath(
                        transform.position, goal.position,
                        graphNodes, adjacency);
                    Debug.Log("AIRaceController: Running A*...");
                }

                if (searchResult != null && searchResult.pathFound)
                {
                    string algName = algorithmIndex == 0 ? "UCS" :
                                     algorithmIndex == 1 ? "BFS" : "A*";
                    Debug.Log($"AIRaceController: {algName} path found — " +
                              $"{searchResult.path.Count} waypoints, " +
                              $"cost {searchResult.totalCost:F1}, " +
                              $"{searchResult.expandedNodeCount} nodes expanded.");

                    DebugVisualizer vis = FindAnyObjectByType<DebugVisualizer>();
                    if (vis != null)
                        vis.SetResults(searchResult, graphNodes, adjacency);

                    waypointIndex = 0;
                    isMoving = true;
                    MoveToWaypoint();
                    yield break;
                }

                Debug.LogWarning("AIRaceController: No path found — NavMesh fallback.");
            }

            if (goal != null)
            {
                agent.isStopped = false;
                agent.SetDestination(goal.position);
                isMoving = true;
            }
        }

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
            {
                animator.SetFloat("Speed", moving ? 2f : 0f);
                animator.SetFloat("MotionSpeed", 1f);
                animator.SetBool("Grounded", true);
                animator.SetBool("FreeFall", false);
                animator.SetBool("Jump", false);
            }
        }

        public List<Vector3> GetGraphNodes() => graphNodes;
        public Dictionary<Vector3, List<(Vector3, float)>> GetAdjacency() => adjacency;
    }
}