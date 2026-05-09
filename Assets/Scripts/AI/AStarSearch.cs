using System.Collections.Generic;
using UnityEngine;

namespace MazeChase.AI
{
    public class AStarSearch
    {
        public SearchResult FindPath(Vector3 startPos, Vector3 goalPos, List<Vector3> allNodes, Dictionary<Vector3, List<(Vector3 neighbor, float cost)>> adjacency)
        {
            SearchResult result = new SearchResult();

            if (allNodes == null || allNodes.Count == 0)
            {
                Debug.LogWarning("AStarSearch: No nodes provided.");
                return result;
            }

            // Find closest nodes to start and goal in the graph
            Vector3 startNode = FindClosestNode(startPos, allNodes);
            Vector3 goalNode = FindClosestNode(goalPos, allNodes);

            // Priority queue stores: (fScore, unique id, node)
            // fScore = gScore (cost from start) + hScore (estimated cost to goal)
            var frontier = new SortedSet<(float fScore, int id, Vector3 node)>(
                Comparer<(float, int, Vector3)>.Create((a, b) => {
                    int c = a.Item1.CompareTo(b.Item1);
                    return c != 0 ? c : a.Item2.CompareTo(b.Item2);
                })
            );

            Dictionary<Vector3, float> gScore = new Dictionary<Vector3, float>();
            Dictionary<Vector3, Vector3?> cameFrom = new Dictionary<Vector3, Vector3?>();
            int idCounter = 0;

            // Initialize start node
            gScore[startNode] = 0f;
            float initialH = Vector3.Distance(startNode, goalNode);
            frontier.Add((initialH, idCounter++, startNode));
            cameFrom[startNode] = null;

            while (frontier.Count > 0)
            {
                // Get node with lowest fScore
                var current = frontier.Min;
                frontier.Remove(current);
                Vector3 currentNode = current.node;

                result.visitedPositions.Add(currentNode);
                result.expandedNodeCount++;

                // Goal check (with small tolerance)
                if (Vector3.Distance(currentNode, goalNode) < 0.5f)
                {
                    // Reconstruct path
                    Vector3? step = goalNode;
                    while (step.HasValue)
                    {
                        result.path.Insert(0, step.Value);
                        step = cameFrom.ContainsKey(step.Value) ? cameFrom[step.Value] : null;
                    }

                    result.totalCost = gScore[goalNode];
                    result.pathFound = true;
                    Debug.Log($"A*: Path found! Nodes expanded: {result.expandedNodeCount}, Cost: {result.totalCost}");
                    return result;
                }

                if (!adjacency.ContainsKey(currentNode)) continue;

                foreach (var (neighbor, edgeCost) in adjacency[currentNode])
                {
                    float tentativeGScore = gScore[currentNode] + edgeCost;

                    // If this path to neighbor is better than any previous one
                    if (!gScore.ContainsKey(neighbor) || tentativeGScore < gScore[neighbor])
                    {
                        cameFrom[neighbor] = currentNode;
                        gScore[neighbor] = tentativeGScore;
                        
                        // fScore = g + h (distance to goal)
                        float fScore = tentativeGScore + Vector3.Distance(neighbor, goalNode);
                        
                        frontier.Add((fScore, idCounter++, neighbor));
                    }
                }
            }

            Debug.LogWarning("A*: No path found.");
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