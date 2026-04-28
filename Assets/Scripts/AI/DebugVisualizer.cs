using System.Collections.Generic;
using UnityEngine;

namespace MazeChase.AI
{
    public class DebugVisualizer : MonoBehaviour
    {
        [Header("Toggle — press Tab in Play mode")]
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

        void OnDrawGizmos()
        {
            if (!debugMode) return;

            // Draw all nodes and edges
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
            Gizmos.color = visitedColor;
            if (visitedNodes != null)
                foreach (Vector3 v in visitedNodes)
                    Gizmos.DrawSphere(v, 0.25f);

            // Draw final path
            Gizmos.color = pathColor;
            if (finalPath != null)
                for (int i = 0; i < finalPath.Count - 1; i++)
                    Gizmos.DrawLine(finalPath[i], finalPath[i + 1]);
        }
    }
}