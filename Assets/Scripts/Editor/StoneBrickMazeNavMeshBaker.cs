using System.IO;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;

namespace MazeChase.EditorTools
{
    public static class StoneBrickMazeNavMeshBaker
    {
        private const string MenuPath = "Maze Chase/Bake StoneBrickMaze NavMesh";
        private const string NavMeshAssetFolder = "Assets/Scenes/StoneBrickMaze";
        private const string NavMeshAssetPath = NavMeshAssetFolder + "/NavMesh-StoneBrickMaze.asset";

        [MenuItem(MenuPath)]
        public static void Bake()
        {
            GameObject navigationObject = GameObject.Find("Navigation");
            if (navigationObject == null)
            {
                Debug.LogError("StoneBrickMazeNavMeshBaker: Navigation object was not found.");
                return;
            }

            NavMeshSurface surface = navigationObject.GetComponent<NavMeshSurface>();
            if (surface == null)
            {
                surface = navigationObject.AddComponent<NavMeshSurface>();
            }

            surface.agentTypeID = 0;
            surface.collectObjects = CollectObjects.Volume;
            surface.center = Vector3.zero;
            surface.size = new Vector3(140f, 20f, 92f);
            surface.layerMask = LayerMask.GetMask("Ground_Walkable", "Maze_Wall");
            surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
            surface.ignoreNavMeshAgent = true;
            surface.ignoreNavMeshObstacle = true;
            surface.overrideVoxelSize = true;
            surface.voxelSize = 0.12f;
            surface.overrideTileSize = true;
            surface.tileSize = 128;
            surface.minRegionArea = 2f;

            surface.BuildNavMesh();
            if (surface.navMeshData == null)
            {
                Debug.LogError("StoneBrickMazeNavMeshBaker: NavMesh build did not produce data.");
                return;
            }

            if (!AssetDatabase.IsValidFolder(NavMeshAssetFolder))
            {
                Directory.CreateDirectory(NavMeshAssetFolder);
                AssetDatabase.Refresh();
            }

            AssetDatabase.DeleteAsset(NavMeshAssetPath);
            AssetDatabase.CreateAsset(surface.navMeshData, NavMeshAssetPath);
            surface.navMeshData = AssetDatabase.LoadAssetAtPath<NavMeshData>(NavMeshAssetPath);

            EditorUtility.SetDirty(surface);
            EditorSceneManager.MarkSceneDirty(navigationObject.scene);
            EditorSceneManager.SaveScene(navigationObject.scene);
            AssetDatabase.SaveAssets();

            Debug.Log($"StoneBrickMazeNavMeshBaker: Baked NavMesh to {NavMeshAssetPath}.");
        }
    }
}
