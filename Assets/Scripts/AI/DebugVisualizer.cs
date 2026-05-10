using System.Collections.Generic;
using UnityEngine;

namespace MazeChase.AI
{
    public class DebugVisualizer : MonoBehaviour
    {
        [Header("Press Tab to toggle")]
        public bool debugMode = false;

        [Header("Colors")]
        public Color nodeColor = Color.blue;
        public Color visitedColor = Color.yellow;
        public Color pathColor = Color.green;

        private List<Vector3> allNodes = new List<Vector3>();
        private List<Vector3> visitedNodes = new List<Vector3>();
        private List<Vector3> finalPath = new List<Vector3>();
        private Dictionary<Vector3, List<(Vector3, float)>> adjacency;

        private List<GameObject> drawnObjects = new List<GameObject>();
        private float tabCooldown = 0f;
        private bool dataReceived = false;

        // Shared material cache — created once reused for all spheres
        private Material blueMat;
        private Material yellowMat;
        private Material greenMat;

        void Start()
        {
            // Create materials once at start
            blueMat = CreateMaterial(nodeColor, false);
            yellowMat = CreateMaterial(visitedColor, false);
            greenMat = CreateMaterial(pathColor, true);
        }

        private Material CreateMaterial(Color color, bool emission)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit")
                         ?? Shader.Find("Standard")
                         ?? Shader.Find("Diffuse");

            Material mat = new Material(shader);
            mat.color = color;

            if (emission)
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", color * 0.5f);
            }
            return mat;
        }

        void Update()
        {
            tabCooldown -= Time.deltaTime;

            if (Input.GetKeyDown(KeyCode.Tab) && tabCooldown <= 0f)
            {
                tabCooldown = 0.5f;
                debugMode = !debugMode;
                Debug.Log($"Debug visualizer: {(debugMode ? "ON" : "OFF")}");

                if (debugMode && !dataReceived)
                {
                    Debug.LogWarning("DebugVisualizer: No path data yet — " +
                                     "wait for AI to find path first!");
                    debugMode = false;
                    return;
                }
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
            dataReceived = true;

            Debug.Log($"DebugVisualizer: received {finalPath.Count} path points, " +
                      $"{visitedNodes.Count} visited nodes. Press Tab to visualize.");

            if (debugMode) RefreshVisuals();
        }

        private void RefreshVisuals()
        {
            foreach (GameObject obj in drawnObjects)
                Destroy(obj);
            drawnObjects.Clear();

            if (!debugMode || !dataReceived) return;

            Debug.Log($"DebugVisualizer: Drawing {allNodes.Count} nodes, " +
                      $"{visitedNodes.Count} visited, " +
                      $"{finalPath.Count} path points...");

            // Blue — all graph nodes smallest
            foreach (Vector3 pos in allNodes)
                CreateSphere(pos, 0.2f, blueMat);

            // Yellow — visited nodes medium
            foreach (Vector3 pos in visitedNodes)
                CreateSphere(pos, 0.35f, yellowMat);

            // Green — final path largest
            for (int i = 0; i < finalPath.Count; i++)
            {
                CreateSphere(finalPath[i], 0.6f, greenMat);
                if (i < finalPath.Count - 1)
                    CreateLine(finalPath[i], finalPath[i + 1]);
            }

            Debug.Log("DebugVisualizer: Drawing complete!");
        }

        private void CreateSphere(Vector3 pos, float radius, Material mat)
        {
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.position = pos;
            sphere.transform.localScale = Vector3.one * radius;
            sphere.transform.SetParent(this.transform);
            Destroy(sphere.GetComponent<Collider>());

            Renderer r = sphere.GetComponent<Renderer>();
            if (r != null) r.sharedMaterial = mat;

            drawnObjects.Add(sphere);
        }

        private void CreateLine(Vector3 start, Vector3 end)
        {
            GameObject lineObj = new GameObject("PathLine");
            lineObj.transform.SetParent(this.transform);
            LineRenderer lr = lineObj.AddComponent<LineRenderer>();
            lr.sharedMaterial = greenMat;
            lr.startColor = pathColor;
            lr.endColor = pathColor;
            lr.startWidth = 0.15f;
            lr.endWidth = 0.15f;
            lr.SetPosition(0, start);
            lr.SetPosition(1, end);
            drawnObjects.Add(lineObj);
        }

        void OnDestroy()
        {
            foreach (GameObject obj in drawnObjects)
                if (obj != null) Destroy(obj);
            if (blueMat != null) Destroy(blueMat);
            if (yellowMat != null) Destroy(yellowMat);
            if (greenMat != null) Destroy(greenMat);
        }

        void OnDrawGizmos()
        {
            if (!debugMode) return;

            Gizmos.color = nodeColor;
            foreach (Vector3 v in allNodes)
                Gizmos.DrawSphere(v, 0.2f);

            Gizmos.color = visitedColor;
            foreach (Vector3 v in visitedNodes)
                Gizmos.DrawSphere(v, 0.35f);

            Gizmos.color = pathColor;
            for (int i = 0; i < finalPath.Count - 1; i++)
                Gizmos.DrawLine(finalPath[i], finalPath[i + 1]);
            foreach (Vector3 p in finalPath)
                Gizmos.DrawSphere(p, 0.6f);
        }
    }
}