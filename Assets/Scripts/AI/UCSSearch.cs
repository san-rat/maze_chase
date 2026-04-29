using System.Collections.Generic;
using UnityEngine;

namespace MazeChase.AI
{
    public class UCSSearch
    {
        public SearchResult FindPath(Vector3 startPos, Vector3 goalPos, List<Vector3> allNodes, Dictionary<Vector3, List<(Vector3 neighbor, float cost)>> adjacency)
        {
            SearchResult result = new SearchResult();

            if (allNodes == null || allNodes.Count == 0)
            {
                Debug.LogWarning("UCSSearch: No nodes provided.");
                return result;
            }

            // Find closest nodes to start and goal
            Vector3 startNode = FindClosestNode(startPos, allNodes);
            Vector3 goalNode = FindClosestNode(goalPos, allNodes);

            // Priority queue: (cost, unique id, node)
            var frontier = new SortedSet<(float cost, int id, Vector3 node)>(
                Comparer<(float, int, Vector3)>.Create((a, b) => {
                    int c = a.Item1.CompareTo(b.Item1);
                    return c != 0 ? c : a.Item2.CompareTo(b.Item2);
                })
            );

            Dictionary<Vector3, float> costSoFar = new Dictionary<Vector3, float>();
            Dictionary<Vector3, Vector3?> cameFrom = new Dictionary<Vector3, Vector3?>();
            int idCounter = 0;

            frontier.Add((0f, idCounter++, startNode));
            costSoFar[startNode] = 0f;
            cameFrom[startNode] = null;

            while (frontier.Count > 0)
            {
                var current = frontier.Min;
                frontier.Remove(current);
                Vector3 currentNode = current.node;

                result.visitedPositions.Add(currentNode);
                result.expandedNodeCount++;

                // Goal check
                if (Vector3.Distance(currentNode, goalNode) < 0.5f)
                {
                    // Reconstruct path
                    Vector3? step = goalNode;
                    while (step.HasValue)
                    {
                        result.path.Insert(0, step.Value);
                        step = cameFrom.ContainsKey(step.Value) ? cameFrom[step.Value] : null;
                    }
                    result.totalCost = costSoFar[goalNode];
                    result.pathFound = true;
                    Debug.Log($"UCS: Path found! Nodes expanded: {result.expandedNodeCount}, Cost: {result.totalCost}");
                    return result;
                }

                // Expand neighbors
                if (!adjacency.ContainsKey(currentNode)) continue;

                foreach (var (neighbor, edgeCost) in adjacency[currentNode])
                {
                    float newCost = costSoFar[currentNode] + edgeCost;
                    if (!costSoFar.ContainsKey(neighbor) || newCost < costSoFar[neighbor])
                    {
                        costSoFar[neighbor] = newCost;
                        cameFrom[neighbor] = currentNode;
                        frontier.Add((newCost, idCounter++, neighbor));
                    }
                }
            }

            Debug.LogWarning("UCS: No path found.");
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