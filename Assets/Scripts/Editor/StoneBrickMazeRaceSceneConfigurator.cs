using MazeChase.Race;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

namespace MazeChase.EditorTools
{
    public static class StoneBrickMazeRaceSceneConfigurator
    {
        private const string MenuPath = "Maze Chase/Configure StoneBrickMaze Race Scene";
        private const string MinimapTexturePath = "Assets/Minimap/Minimap.renderTexture";

        [MenuItem(MenuPath)]
        public static void Configure()
        {
            GameObject gameplay = FindOrCreate("Gameplay");
            GameObject runtimeMinimap = FindOrCreate("RuntimeMinimap", gameplay.transform);
            GameObject raceCameras = FindOrCreate("RaceCameras", gameplay.transform);

            ConfigureMinimapCamera(runtimeMinimap.transform);
            ConfigureMinimapCanvas(runtimeMinimap.transform);
            ConfigureRaceStatusText(runtimeMinimap.transform);
            ConfigureRaceIcons();

            EditorSceneManager.MarkSceneDirty(gameplay.scene);
            EditorSceneManager.SaveScene(gameplay.scene);
        }

        [MenuItem("Maze Chase/Validate StoneBrickMaze Race Scene")]
        public static void Validate()
        {
            string[] requiredObjects =
            {
                "Player_Racer",
                "AI_Racer",
                "RaceGameManager",
                "GoalCheckpoint",
                "MinimapCamera",
                "MinimapCanvas"
            };

            foreach (string objectName in requiredObjects)
            {
                if (GameObject.Find(objectName) == null)
                {
                    Debug.LogError($"StoneBrickMazeRaceSceneConfigurator: Missing required object {objectName}.");
                }
            }

            GameObject ai = GameObject.Find("AI_Racer");
            GameObject goal = GameObject.Find("GoalCheckpoint");
            if (ai == null || goal == null)
            {
                return;
            }

            bool aiOnNavMesh = NavMesh.SamplePosition(ai.transform.position, out NavMeshHit aiHit, 4f, NavMesh.AllAreas);
            bool goalOnNavMesh = NavMesh.SamplePosition(goal.transform.position, out NavMeshHit goalHit, 6f, NavMesh.AllAreas);
            NavMeshPath path = new NavMeshPath();
            bool hasCompletePath = aiOnNavMesh
                && goalOnNavMesh
                && NavMesh.CalculatePath(aiHit.position, goalHit.position, NavMesh.AllAreas, path)
                && path.status == NavMeshPathStatus.PathComplete;

            if (hasCompletePath)
            {
                Debug.Log("StoneBrickMazeRaceSceneConfigurator: Validation passed. AI has a complete NavMesh path to the goal.");
            }
            else
            {
                Debug.LogError($"StoneBrickMazeRaceSceneConfigurator: NavMesh validation failed. aiOnNavMesh={aiOnNavMesh}, goalOnNavMesh={goalOnNavMesh}, pathStatus={path.status}.");
            }
        }

        private static void ConfigureMinimapCamera(Transform parent)
        {
            GameObject cameraObject = GameObject.Find("MinimapCamera") ?? GameObject.Find("TopDownPreviewCamera");
            if (cameraObject == null)
            {
                cameraObject = new GameObject("MinimapCamera", typeof(Camera));
            }

            cameraObject.name = "MinimapCamera";
            cameraObject.transform.SetParent(parent, false);
            cameraObject.transform.position = new Vector3(0f, 110f, 0f);
            cameraObject.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

            Camera camera = cameraObject.GetComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 48f;
            camera.nearClipPlane = 0.3f;
            camera.farClipPlane = 220f;
            camera.targetTexture = AssetDatabase.LoadAssetAtPath<RenderTexture>(MinimapTexturePath);
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.12f, 0.12f, 0.13f, 1f);
        }

        private static void ConfigureMinimapCanvas(Transform parent)
        {
            RenderTexture minimapTexture = AssetDatabase.LoadAssetAtPath<RenderTexture>(MinimapTexturePath);
            GameObject canvasObject = GameObject.Find("MinimapCanvas");
            if (canvasObject == null)
            {
                canvasObject = new GameObject("MinimapCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            }

            canvasObject.transform.SetParent(parent, false);

            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            GameObject imageObject = GameObject.Find("MinimapRawImage");
            if (imageObject == null)
            {
                imageObject = new GameObject("MinimapRawImage", typeof(RectTransform), typeof(RawImage));
            }

            imageObject.transform.SetParent(canvasObject.transform, false);
            RectTransform rect = imageObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.anchoredPosition = new Vector2(-24f, -24f);
            rect.sizeDelta = new Vector2(320f, 210f);

            RawImage rawImage = imageObject.GetComponent<RawImage>();
            rawImage.texture = minimapTexture;
            rawImage.color = Color.white;
        }

        private static void ConfigureRaceStatusText(Transform parent)
        {
            GameObject canvasObject = GameObject.Find("MinimapCanvas");
            if (canvasObject == null)
            {
                return;
            }

            GameObject textObject = GameObject.Find("RaceStatusText");
            if (textObject == null)
            {
                textObject = new GameObject("RaceStatusText", typeof(RectTransform), typeof(Text));
            }

            textObject.transform.SetParent(canvasObject.transform, false);
            RectTransform rect = textObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, -24f);
            rect.sizeDelta = new Vector2(720f, 44f);

            Text text = textObject.GetComponent<Text>();
            text.text = "Race ready";
            text.alignment = TextAnchor.MiddleCenter;
            text.fontSize = 28;
            text.color = Color.white;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            RaceGameManager manager = Object.FindAnyObjectByType<RaceGameManager>();
            if (manager != null)
            {
                SerializedObject serializedManager = new SerializedObject(manager);
                serializedManager.FindProperty("statusText").objectReferenceValue = text;
                serializedManager.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(manager);
            }
        }

        private static void ConfigureRaceIcons()
        {
            Material playerMaterial = GetOrCreateColorMaterial("Assets/Materials/Race_PlayerIcon.mat", new Color(0.1f, 0.45f, 1f, 1f));
            Material aiMaterial = GetOrCreateColorMaterial("Assets/Materials/Race_AIIcon.mat", new Color(1f, 0.85f, 0.1f, 1f));
            Material startMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/StoneBrickMaze_StartPad.mat");
            Material goalMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/StoneBrickMaze_ExitPad.mat");

            CreateIcon("PlayerMinimapIcon", GameObject.Find("Player_Racer")?.transform, Vector3.up * 3f, playerMaterial, 0.75f);
            CreateIcon("AIMinimapIcon", GameObject.Find("AI_Racer")?.transform, Vector3.up * 3f, aiMaterial, 0.75f);
            CreateIcon("StartMinimapIcon", GameObject.Find("SharedStartZone_GreenPad")?.transform, Vector3.up * 1.2f, startMaterial, 1.6f);
            CreateIcon("GoalMinimapIcon", GameObject.Find("GoalCheckpoint")?.transform, Vector3.up * 1.2f, goalMaterial, 1.6f);
        }

        private static void CreateIcon(string name, Transform parent, Vector3 localPosition, Material material, float diameter)
        {
            if (parent == null)
            {
                return;
            }

            GameObject icon = GameObject.Find(name);
            if (icon == null)
            {
                icon = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                icon.name = name;
            }

            icon.transform.SetParent(parent, false);
            icon.transform.localPosition = localPosition;
            icon.transform.localRotation = Quaternion.identity;
            icon.transform.localScale = new Vector3(diameter, 0.04f, diameter);

            Collider collider = icon.GetComponent<Collider>();
            if (collider != null)
            {
                Object.DestroyImmediate(collider);
            }

            Renderer renderer = icon.GetComponent<Renderer>();
            if (renderer != null && material != null)
            {
                renderer.sharedMaterial = material;
            }
        }

        private static Material GetOrCreateColorMaterial(string path, Color color)
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }

            material.color = color;
            EditorUtility.SetDirty(material);
            return material;
        }

        private static GameObject FindOrCreate(string name, Transform parent = null)
        {
            GameObject gameObject = GameObject.Find(name);
            if (gameObject == null)
            {
                gameObject = new GameObject(name);
            }

            if (parent != null)
            {
                gameObject.transform.SetParent(parent, false);
            }

            return gameObject;
        }
    }
}
