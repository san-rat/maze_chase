using System.Collections.Generic;
using UnityEngine;

namespace MazeChase.AI
{
    public class BFSSearch
    {
        public SearchResult FindPath(Vector3 startPos, Vector3 goalPos,
            List<Vector3> allNodes,
            Dictionary<Vector3, List<(Vector3 neighbor, float cost)>> adjacency)
        {
            SearchResult result = new SearchResult();

            if (allNodes == null || allNodes.Count == 0)
            {
                Debug.LogWarning("BFSSearch: No nodes provided.");
                return result;
            }

            // Find closest nodes to start and goal
            Vector3 startNode = FindClosestNode(startPos, allNodes);
            Vector3 goalNode = FindClosestNode(goalPos, allNodes);

            // BFS uses a regular queue (not priority queue)
            Queue<Vector3> frontier = new Queue<Vector3>();
            Dictionary<Vector3, Vector3?> cameFrom = new Dictionary<Vector3, Vector3?>();

            frontier.Enqueue(startNode);
            cameFrom[startNode] = null;

            while (frontier.Count > 0)
            {
                Vector3 current = frontier.Dequeue();
                result.visitedPositions.Add(current);
                result.expandedNodeCount++;

                // Goal check
                if (Vector3.Distance(current, goalNode) < 0.5f)
                {
                    // Reconstruct path
                    Vector3? step = goalNode;
                    while (step.HasValue)
                    {
                        result.path.Insert(0, step.Value);
                        step = cameFrom.ContainsKey(step.Value)
                            ? cameFrom[step.Value] : null;
                    }

                    // Calculate total cost
                    for (int i = 0; i < result.path.Count - 1; i++)
                        result.totalCost += Vector3.Distance(
                            result.path[i], result.path[i + 1]);

                    result.pathFound = true;
                    Debug.Log($"BFS: Path found! Nodes expanded: " +
                              $"{result.expandedNodeCount}, " +
                              $"Cost: {result.totalCost}");
                    return result;
                }

                // Expand neighbors
                if (!adjacency.ContainsKey(current)) continue;

                foreach (var (neighbor, _) in adjacency[current])
                {
                    if (!cameFrom.ContainsKey(neighbor))
                    {
                        frontier.Enqueue(neighbor);
                        cameFrom[neighbor] = current;
                    }
                }
            }

            Debug.LogWarning("BFS: No path found.");
            return result;
        }

        private Vector3 FindClosestNode(Vector3 pos, List<Vector3> nodes)
        {
            Vector3 closest = nodes[0];
            float minDist = Vector3.Distance(pos, nodes[0]);
            foreach (Vector3 node in nodes)
            {
                float d = Vector3.Distance(pos, node);
                if (d < minDist) { minDist = d; closest = node; }
            }
            return closest;
        }
    }
}