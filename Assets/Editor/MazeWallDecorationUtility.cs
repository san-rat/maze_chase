using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class MazeWallDecorationUtility
{
    private const string DecorationRootName = "MazeWallDecorations";
    private const string WallLayerName = "Maze_Wall";
    private const string BronzeMaterialPath = "Assets/Textures/WallDeco_DarkBronze.mat";
    private const string AmberMaterialPath = "Assets/Textures/WallDeco_WarmAmber.mat";
    private const int MaxDecoratedWalls = 56;

    [MenuItem("Maze Chase/Apply Maze Wall Decorations")]
    public static void ApplyWallDecorations()
    {
        int wallLayer = LayerMask.NameToLayer(WallLayerName);
        if (wallLayer < 0)
            throw new System.InvalidOperationException("Layer " + WallLayerName + " was not found.");

        Material bronzeMaterial = CreateBronzeMaterial();
        Material amberMaterial = CreateAmberMaterial();
        List<MeshRenderer> walls = CollectWalls(wallLayer);

        DestroyIfExists(DecorationRootName);

        GameObject root = new GameObject(DecorationRootName);
        root.layer = wallLayer;

        int decorated = 0;
        int accentCount = 0;
        for (int i = 0; i < walls.Count && decorated < MaxDecoratedWalls; i++)
        {
            MeshRenderer wall = walls[i];
            if (!ShouldDecorateWall(wall, i))
                continue;

            Transform group = new GameObject("WallDeco_" + decorated.ToString("00")).transform;
            group.SetParent(root.transform, false);
            group.position = wall.bounds.center;
            group.rotation = wall.transform.rotation;
            group.localScale = Vector3.one;
            group.gameObject.layer = wallLayer;

            bool longX = wall.bounds.size.x >= wall.bounds.size.z;
            CreateFaceSet(group, longX, 1f, wall.bounds.size, bronzeMaterial, amberMaterial, decorated, ref accentCount);

            if (decorated % 3 == 0)
                CreateFaceSet(group, longX, -1f, wall.bounds.size, bronzeMaterial, amberMaterial, decorated + 7, ref accentCount);

            decorated++;
        }

        AssetDatabase.SaveAssets();
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log("Applied maze wall decorations to " + decorated + " wall sections with " + accentCount + " warm insets.");
    }

    private static List<MeshRenderer> CollectWalls(int wallLayer)
    {
        List<MeshRenderer> walls = new List<MeshRenderer>();
        foreach (MeshRenderer renderer in Object.FindObjectsByType<MeshRenderer>(FindObjectsInactive.Exclude))
        {
            if (renderer.gameObject.layer != wallLayer || renderer.name == "TopWhiteCap")
                continue;

            Bounds bounds = renderer.bounds;
            if (bounds.size.y < 0.75f || Mathf.Max(bounds.size.x, bounds.size.z) < 0.8f)
                continue;

            walls.Add(renderer);
        }

        walls.Sort((a, b) =>
        {
            int z = a.bounds.center.z.CompareTo(b.bounds.center.z);
            return z != 0 ? z : a.bounds.center.x.CompareTo(b.bounds.center.x);
        });

        return walls;
    }

    private static bool ShouldDecorateWall(MeshRenderer wall, int index)
    {
        Bounds bounds = wall.bounds;
        float length = Mathf.Max(bounds.size.x, bounds.size.z);

        if (length < 2.5f)
            return index % 5 == 0;

        return index % 3 != 1;
    }

    private static void CreateFaceSet(
        Transform parent,
        bool longX,
        float side,
        Vector3 wallSize,
        Material bronzeMaterial,
        Material amberMaterial,
        int seed,
        ref int accentCount)
    {
        float length = longX ? wallSize.x : wallSize.z;
        float faceOffset = (longX ? wallSize.z : wallSize.x) * 0.5f + 0.018f;
        float bandLength = Mathf.Clamp(length * 0.52f, 1.0f, 4.8f);
        float verticalOffset = Mathf.Clamp(wallSize.y * 0.26f, 0.22f, 0.46f);

        CreatePiece(
            parent,
            "BronzeBand",
            LocalPosition(longX, 0f, verticalOffset, side * faceOffset),
            Quaternion.identity,
            LocalScale(longX, bandLength, 0.045f, 0.035f),
            bronzeMaterial);

        if (seed % 2 == 0)
        {
            CreatePiece(
                parent,
                "BronzeLowerBand",
                LocalPosition(longX, 0f, -verticalOffset * 0.65f, side * faceOffset),
                Quaternion.identity,
                LocalScale(longX, bandLength * 0.42f, 0.035f, 0.032f),
                bronzeMaterial);
        }

        if (seed % 3 != 1)
        {
            CreateAmberInset(parent, longX, side, faceOffset, wallSize.y, seed, amberMaterial, bronzeMaterial);
            accentCount++;
        }
    }

    private static void CreateAmberInset(
        Transform parent,
        bool longX,
        float side,
        float faceOffset,
        float wallHeight,
        int seed,
        Material amberMaterial,
        Material bronzeMaterial)
    {
        float lateralOffset = seed % 2 == 0 ? -0.38f : 0.38f;
        float y = Mathf.Clamp(wallHeight * 0.03f, 0.02f, 0.08f);

        CreatePiece(
            parent,
            "AmberInset",
            LocalPosition(longX, lateralOffset, y, side * (faceOffset + 0.004f)),
            Quaternion.identity,
            LocalScale(longX, 0.16f, 0.28f, 0.04f),
            amberMaterial);

        CreatePiece(
            parent,
            "AmberInsetBrace",
            LocalPosition(longX, lateralOffset, y, side * (faceOffset + 0.001f)),
            Quaternion.identity,
            LocalScale(longX, 0.24f, 0.035f, 0.042f),
            bronzeMaterial);
    }

    private static Vector3 LocalPosition(bool longX, float along, float y, float face)
    {
        return longX ? new Vector3(along, y, face) : new Vector3(face, y, along);
    }

    private static Vector3 LocalScale(bool longX, float along, float y, float depth)
    {
        return longX ? new Vector3(along, y, depth) : new Vector3(depth, y, along);
    }

    private static GameObject CreatePiece(Transform parent, string name, Vector3 position, Quaternion rotation, Vector3 scale, Material material)
    {
        GameObject piece = GameObject.CreatePrimitive(PrimitiveType.Cube);
        piece.name = name;
        piece.layer = parent.gameObject.layer;
        piece.transform.SetParent(parent, false);
        piece.transform.localPosition = position;
        piece.transform.localRotation = rotation;
        piece.transform.localScale = scale;
        piece.GetComponent<MeshRenderer>().sharedMaterial = material;
        Object.DestroyImmediate(piece.GetComponent<Collider>());
        return piece;
    }

    private static Material CreateBronzeMaterial()
    {
        Material material = LoadOrCreateMaterial(BronzeMaterialPath, "WallDeco_DarkBronze");
        SetColor(material, "_BaseColor", new Color(0.24f, 0.18f, 0.12f, 1f));
        SetColor(material, "_Color", new Color(0.24f, 0.18f, 0.12f, 1f));
        SetFloat(material, "_Metallic", 0.22f);
        SetFloat(material, "_Smoothness", 0.48f);
        EditorUtility.SetDirty(material);
        return material;
    }

    private static Material CreateAmberMaterial()
    {
        Material material = LoadOrCreateMaterial(AmberMaterialPath, "WallDeco_WarmAmber");
        Color amber = new Color(1f, 0.55f, 0.22f, 1f);
        SetColor(material, "_BaseColor", amber);
        SetColor(material, "_Color", amber);
        if (material.HasProperty("_EmissionColor"))
        {
            material.SetColor("_EmissionColor", amber * 1.8f);
            material.EnableKeyword("_EMISSION");
        }

        SetFloat(material, "_Metallic", 0f);
        SetFloat(material, "_Smoothness", 0.34f);
        EditorUtility.SetDirty(material);
        return material;
    }

    private static Material LoadOrCreateMaterial(string path, string materialName)
    {
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material != null)
            return material;

        material = new Material(FindLitShader())
        {
            name = materialName
        };

        AssetDatabase.CreateAsset(material, path);
        return material;
    }

    private static Shader FindLitShader()
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
            shader = Shader.Find("Standard");
        return shader;
    }

    private static void DestroyIfExists(string objectName)
    {
        GameObject existing = GameObject.Find(objectName);
        if (existing != null)
            Object.DestroyImmediate(existing);
    }

    private static void SetColor(Material material, string property, Color value)
    {
        if (material.HasProperty(property))
            material.SetColor(property, value);
    }

    private static void SetFloat(Material material, string property, float value)
    {
        if (material.HasProperty(property))
            material.SetFloat(property, value);
    }
}
