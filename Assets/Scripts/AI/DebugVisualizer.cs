using System.Collections.Generic;
using UnityEngine;

namespace MazeChase.AI
{
    public class DebugVisualizer : MonoBehaviour
    {
        [Header("Press Tab to toggle")]
        public bool debugMode = false;

        [Header("Colors")]
        public Color nodeColor = Color.white;
        public Color edgeColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        public Color visitedColor = Color.yellow;
        public Color pathColor = Color.green;

        private List<Vector3> allNodes = new List<Vector3>();
        private List<Vector3> visitedNodes = new List<Vector3>();
        private List<Vector3> finalPath = new List<Vector3>();
        private Dictionary<Vector3, List<(Vector3, float)>> adjacency;

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                debugMode = !debugMode;
                Debug.Log($"Debug visualizer: {(debugMode ? "ON" : "OFF")}");
            }
        }

        public void SetResults(SearchResult result, List<Vector3> nodes,
            Dictionary<Vector3, List<(Vector3, float)>> adj)
        {
            visitedNodes = result.visitedPositions;
            finalPath = result.path;
            allNodes = nodes ?? new List<Vector3>();
            adjacency = adj;
        }

        // OnDrawGizmos draws ALWAYS regardless of selection
        void OnDrawGizmos()
        {
            if (!debugMode) return;

            // Draw all nodes
            if (allNodes != null)
            {
                foreach (Vector3 node in allNodes)
                {
                    Gizmos.color = nodeColor;
                    Gizmos.DrawSphere(node, 0.2f);

                    if (adjacency != null && adjacency.ContainsKey(node))
                    {
                        Gizmos.color = edgeColor;
                        foreach (var (neighbor, _) in adjacency[node])
                            Gizmos.DrawLine(node, neighbor);
                    }
                }
            }

            // Draw visited nodes
            if (visitedNodes != null)
            {
                Gizmos.color = visitedColor;
                foreach (Vector3 v in visitedNodes)
                    Gizmos.DrawSphere(v, 0.3f);
            }

            // Draw final path
            if (finalPath != null && finalPath.Count > 1)
            {
                Gizmos.color = pathColor;
                for (int i = 0; i < finalPath.Count - 1; i++)
                    Gizmos.DrawLine(finalPath[i], finalPath[i + 1]);

                // Draw bigger spheres on path nodes
                foreach (Vector3 p in finalPath)
                {
                    Gizmos.color = pathColor;
                    Gizmos.DrawSphere(p, 0.35f);
                }
            }
        }
    }
}