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

        // Line renderer objects for persistent drawing
        private List<GameObject> drawnObjects = new List<GameObject>();

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                debugMode = !debugMode;
                Debug.Log($"Debug visualizer: {(debugMode ? "ON" : "OFF")}");
                RefreshVisuals();
            }
        }

        public void SetResults(SearchResult result, List<Vector3> nodes,
            Dictionary<Vector3, List<(Vector3, float)>> adj)
        {
            if (result == null) return;
            visitedNodes = result.visitedPositions ?? new List<Vector3>();
            finalPath = result.path ?? new List<Vector3>();
            allNodes = nodes ?? new List<Vector3>();
            adjacency = adj;
            Debug.Log($"DebugVisualizer: received {finalPath.Count} path points, " +
                      $"{visitedNodes.Count} visited nodes.");
            RefreshVisuals();
        }

        private void RefreshVisuals()
        {
            // Clear old objects
            foreach (GameObject obj in drawnObjects)
                Destroy(obj);
            drawnObjects.Clear();

            if (!debugMode) return;

            // Draw visited nodes as yellow spheres
            foreach (Vector3 pos in visitedNodes)
                CreateSphere(pos, 0.3f, visitedColor, "VisitedNode");

            // Draw final path as green spheres and lines
            for (int i = 0; i < finalPath.Count; i++)
            {
                CreateSphere(finalPath[i], 0.4f, pathColor, "PathNode");
                if (i < finalPath.Count - 1)
                    CreateLine(finalPath[i], finalPath[i + 1], pathColor, "PathLine");
            }

            // Draw all nodes as small white spheres
            foreach (Vector3 pos in allNodes)
                CreateSphere(pos, 0.15f, nodeColor, "GraphNode");
        }

        private void CreateSphere(Vector3 pos, float radius, Color color, string label)
        {
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.name = label;
            sphere.transform.position = pos;
            sphere.transform.localScale = Vector3.one * radius;
            sphere.transform.SetParent(this.transform);

            // Remove collider so it doesn't affect gameplay
            Destroy(sphere.GetComponent<Collider>());

            // Set color
            Renderer r = sphere.GetComponent<Renderer>();
            if (r != null)
            {
                r.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                r.material.color = color;
            }

            drawnObjects.Add(sphere);
        }

        private void CreateLine(Vector3 start, Vector3 end, Color color, string label)
        {
            GameObject lineObj = new GameObject(label);
            lineObj.transform.SetParent(this.transform);
            LineRenderer lr = lineObj.AddComponent<LineRenderer>();
            lr.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            lr.material.color = color;
            lr.startColor = color;
            lr.endColor = color;
            lr.startWidth = 0.1f;
            lr.endWidth = 0.1f;
            lr.SetPosition(0, start);
            lr.SetPosition(1, end);
            drawnObjects.Add(lineObj);
        }

        void OnDestroy()
        {
            foreach (GameObject obj in drawnObjects)
                if (obj != null) Destroy(obj);
        }

        // Keep OnDrawGizmos as backup for Scene view
        void OnDrawGizmos()
        {
            if (!debugMode) return;

            Gizmos.color = visitedColor;
            foreach (Vector3 v in visitedNodes)
                Gizmos.DrawSphere(v, 0.3f);

            Gizmos.color = pathColor;
            for (int i = 0; i < finalPath.Count - 1; i++)
                Gizmos.DrawLine(finalPath[i], finalPath[i + 1]);
            foreach (Vector3 p in finalPath)
                Gizmos.DrawSphere(p, 0.4f);
        }
    }
}