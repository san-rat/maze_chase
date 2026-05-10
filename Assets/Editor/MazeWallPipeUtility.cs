using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class MazeWallPipeUtility
{
    private const string PipeRootName = "MazeWallPipes";
    private const string WallLayerName = "Maze_Wall";
    private const int MaxPipeRuns = 48;
    private const int DistributionGridSize = 4;

    private const string DarkPipeMaterialPath = "Assets/Textures/Pipe_DarkGunmetal.mat";
    private const string BlueGrayPipeMaterialPath = "Assets/Textures/Pipe_BlueGrayMetal.mat";
    private const string RustPipeMaterialPath = "Assets/Textures/Pipe_RustBrown.mat";
    private const string BlackPipeMaterialPath = "Assets/Textures/Pipe_BlackMetal.mat";

    [MenuItem("Maze Chase/Apply Maze Wall Pipes")]
    public static void ApplyWallPipes()
    {
        int wallLayer = LayerMask.NameToLayer(WallLayerName);
        if (wallLayer < 0)
            throw new System.InvalidOperationException("Layer " + WallLayerName + " was not found.");

        List<MeshRenderer> walls = CollectWalls(wallLayer);
        if (walls.Count == 0)
            throw new System.InvalidOperationException("No maze wall renderers were found.");

        DestroyIfExists(PipeRootName);

        PipeMaterials materials = CreateMaterials();
        GameObject root = new GameObject(PipeRootName);
        root.layer = wallLayer;

        List<MeshRenderer> selectedWalls = SelectDistributedWalls(walls);
        int created = 0;
        for (int i = 0; i < selectedWalls.Count; i++)
        {
            MeshRenderer wall = selectedWalls[i];

            Bounds bounds = wall.bounds;
            bool alongX = bounds.size.x >= bounds.size.z;
            float side = ChooseVisibleSide(bounds, alongX);
            Vector3 normal = alongX ? new Vector3(0f, 0f, side) : new Vector3(side, 0f, 0f);
            Vector3 tangent = alongX ? Vector3.right : Vector3.forward;

            Transform run = new GameObject("WallPipeRun_" + created.ToString("00")).transform;
            run.SetParent(root.transform, false);
            run.position = bounds.center + normal * (FaceDepth(bounds, alongX) * 0.5f + 0.075f);
            run.rotation = Quaternion.identity;
            run.gameObject.layer = wallLayer;

            PipeMaterials.Palette palette = materials.Choose(created);
            float wallLength = alongX ? bounds.size.x : bounds.size.z;
            float horizontalLength = Mathf.Clamp(wallLength * 0.54f, 1.6f, 4.6f);
            float y = bounds.center.y + Mathf.Clamp(bounds.size.y * 0.18f, 0.28f, 0.52f);
            float lateral = created % 2 == 0 ? -wallLength * 0.14f : wallLength * 0.12f;

            Vector3 baseCenter = bounds.center + tangent * lateral + normal * (FaceDepth(bounds, alongX) * 0.5f + 0.09f);
            baseCenter.y = y;

            CreatePipe(run, "HorizontalPipe", baseCenter, tangent, horizontalLength, 0.075f, palette.main);
            CreatePipeClamp(run, "ClampA", baseCenter - tangent * horizontalLength * 0.32f, tangent, normal, palette.clampMaterial);
            CreatePipeClamp(run, "ClampB", baseCenter + tangent * horizontalLength * 0.32f, tangent, normal, palette.clampMaterial);

            if (created % 3 != 1)
                CreateVerticalBranch(run, baseCenter, normal, palette, created);

            if (created % 4 == 0)
                CreatePipeBend(run, baseCenter + tangent * horizontalLength * 0.5f, tangent, normal, palette.rust);

            created++;
        }

        AssetDatabase.SaveAssets();
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log("Applied " + created + " wall pipe runs to the maze.");
    }

    private static List<MeshRenderer> CollectWalls(int wallLayer)
    {
        List<MeshRenderer> walls = new List<MeshRenderer>();
        foreach (MeshRenderer renderer in Object.FindObjectsByType<MeshRenderer>(FindObjectsInactive.Exclude))
        {
            if (renderer.gameObject.layer != wallLayer || renderer.name == "TopWhiteCap")
                continue;

            Bounds bounds = renderer.bounds;
            if (bounds.size.y < 0.75f || Mathf.Max(bounds.size.x, bounds.size.z) < 2.2f)
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

    private static bool ShouldUseWall(MeshRenderer wall, int index)
    {
        float length = Mathf.Max(wall.bounds.size.x, wall.bounds.size.z);
        if (length < 3.2f)
            return index % 7 == 0;

        return index % 4 != 1;
    }

    private static List<MeshRenderer> SelectDistributedWalls(List<MeshRenderer> walls)
    {
        Bounds mazeBounds = CalculateMazeBounds(walls);
        int bucketCount = DistributionGridSize * DistributionGridSize;
        List<MeshRenderer>[] buckets = new List<MeshRenderer>[bucketCount];
        for (int i = 0; i < buckets.Length; i++)
            buckets[i] = new List<MeshRenderer>();

        for (int i = 0; i < walls.Count; i++)
        {
            MeshRenderer wall = walls[i];
            if (!ShouldUseWall(wall, i))
                continue;

            int bucket = CalculateDistributionBucket(wall.bounds.center, mazeBounds);
            buckets[bucket].Add(wall);
        }

        for (int i = 0; i < buckets.Length; i++)
        {
            buckets[i].Sort((a, b) =>
            {
                int ha = StableWallHash(a.bounds.center);
                int hb = StableWallHash(b.bounds.center);
                return ha.CompareTo(hb);
            });
        }

        List<MeshRenderer> selected = new List<MeshRenderer>();
        int cursor = 0;
        while (selected.Count < MaxPipeRuns)
        {
            bool addedAny = false;
            for (int bucket = 0; bucket < buckets.Length && selected.Count < MaxPipeRuns; bucket++)
            {
                if (cursor >= buckets[bucket].Count)
                    continue;

                selected.Add(buckets[bucket][cursor]);
                addedAny = true;
            }

            if (!addedAny)
                break;

            cursor++;
        }

        return selected;
    }

    private static int CalculateDistributionBucket(Vector3 position, Bounds mazeBounds)
    {
        float normalizedX = Mathf.InverseLerp(mazeBounds.min.x, mazeBounds.max.x, position.x);
        float normalizedZ = Mathf.InverseLerp(mazeBounds.min.z, mazeBounds.max.z, position.z);
        int x = Mathf.Clamp(Mathf.FloorToInt(normalizedX * DistributionGridSize), 0, DistributionGridSize - 1);
        int z = Mathf.Clamp(Mathf.FloorToInt(normalizedZ * DistributionGridSize), 0, DistributionGridSize - 1);
        return z * DistributionGridSize + x;
    }

    private static int StableWallHash(Vector3 position)
    {
        int x = Mathf.RoundToInt(position.x * 10f);
        int z = Mathf.RoundToInt(position.z * 10f);
        return Mathf.Abs((x * 73856093) ^ (z * 19349663));
    }

    private static Bounds CalculateMazeBounds(List<MeshRenderer> walls)
    {
        Bounds bounds = walls[0].bounds;
        for (int i = 1; i < walls.Count; i++)
            bounds.Encapsulate(walls[i].bounds);

        return bounds;
    }

    private static float ChooseVisibleSide(Bounds bounds, bool alongX)
    {
        Vector3 mazeCenter = new Vector3(-41f, bounds.center.y, -907f);
        if (alongX)
            return bounds.center.z > mazeCenter.z ? -1f : 1f;

        return bounds.center.x > mazeCenter.x ? -1f : 1f;
    }

    private static float FaceDepth(Bounds bounds, bool alongX)
    {
        return alongX ? bounds.size.z : bounds.size.x;
    }

    private static void CreateVerticalBranch(Transform parent, Vector3 center, Vector3 normal, PipeMaterials.Palette palette, int index)
    {
        Vector3 branchCenter = center;
        branchCenter += (index % 2 == 0 ? Vector3.right : Vector3.forward) * 0.42f;
        branchCenter.y -= 0.34f;

        CreatePipe(parent, "VerticalPipe", branchCenter, Vector3.up, 0.88f, 0.06f, palette.main);
        CreatePipe(parent, "ShortWallStub", branchCenter + Vector3.up * 0.44f + normal * 0.035f, normal, 0.18f, 0.055f, palette.clampMaterial);
    }

    private static void CreatePipeBend(Transform parent, Vector3 endPoint, Vector3 tangent, Vector3 normal, Material material)
    {
        CreatePipe(parent, "PipeBendDown", endPoint - tangent * 0.05f + Vector3.down * 0.16f, Vector3.up, 0.34f, 0.068f, material);
        CreatePipe(parent, "PipeBendStub", endPoint - tangent * 0.12f + Vector3.down * 0.33f + normal * 0.03f, normal, 0.18f, 0.06f, material);
    }

    private static void CreatePipeClamp(Transform parent, string name, Vector3 center, Vector3 tangent, Vector3 normal, Material material)
    {
        Vector3 scale = AxisScale(tangent, 0.045f, 0.18f);
        GameObject clamp = GameObject.CreatePrimitive(PrimitiveType.Cube);
        clamp.name = name;
        clamp.layer = parent.gameObject.layer;
        clamp.transform.SetParent(parent, true);
        clamp.transform.position = center + normal * 0.012f;
        clamp.transform.rotation = Quaternion.identity;
        clamp.transform.localScale = scale;
        clamp.GetComponent<MeshRenderer>().sharedMaterial = material;
        Object.DestroyImmediate(clamp.GetComponent<Collider>());
    }

    private static void CreatePipe(Transform parent, string name, Vector3 center, Vector3 axis, float length, float radius, Material material)
    {
        GameObject pipe = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pipe.name = name;
        pipe.layer = parent.gameObject.layer;
        pipe.transform.SetParent(parent, true);
        pipe.transform.position = center;
        pipe.transform.rotation = Quaternion.FromToRotation(Vector3.up, axis.normalized);
        pipe.transform.localScale = new Vector3(radius, length * 0.5f, radius);
        pipe.GetComponent<MeshRenderer>().sharedMaterial = material;
        Object.DestroyImmediate(pipe.GetComponent<Collider>());
    }

    private static Vector3 AxisScale(Vector3 tangent, float along, float cross)
    {
        if (Mathf.Abs(tangent.x) > Mathf.Abs(tangent.z))
            return new Vector3(along, cross, cross);

        return new Vector3(cross, cross, along);
    }

    private static PipeMaterials CreateMaterials()
    {
        PipeMaterials materials = new PipeMaterials
        {
            dark = LoadOrCreateMaterial(DarkPipeMaterialPath, "Pipe_DarkGunmetal"),
            blueGray = LoadOrCreateMaterial(BlueGrayPipeMaterialPath, "Pipe_BlueGrayMetal"),
            rust = LoadOrCreateMaterial(RustPipeMaterialPath, "Pipe_RustBrown"),
            black = LoadOrCreateMaterial(BlackPipeMaterialPath, "Pipe_BlackMetal")
        };

        ConfigureMaterial(materials.dark, new Color(0.09f, 0.095f, 0.095f, 1f), 0.35f, 0.3f);
        ConfigureMaterial(materials.blueGray, new Color(0.16f, 0.20f, 0.23f, 1f), 0.25f, 0.26f);
        ConfigureMaterial(materials.rust, new Color(0.38f, 0.18f, 0.075f, 1f), 0.08f, 0.18f);
        ConfigureMaterial(materials.black, new Color(0.018f, 0.017f, 0.016f, 1f), 0.2f, 0.2f);

        return materials;
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

    private static void ConfigureMaterial(Material material, Color color, float metallic, float smoothness)
    {
        SetColor(material, "_BaseColor", color);
        SetColor(material, "_Color", color);
        SetFloat(material, "_Metallic", metallic);
        SetFloat(material, "_Smoothness", smoothness);
        EditorUtility.SetDirty(material);
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

    private sealed class PipeMaterials
    {
        public Material dark;
        public Material blueGray;
        public Material rust;
        public Material black;

        public Palette Choose(int index)
        {
            Material main = index % 3 == 0 ? blueGray : dark;
            Material clamp = index % 4 == 0 ? rust : black;
            return new Palette(main, rust, clamp);
        }

        public readonly struct Palette
        {
            public readonly Material main;
            public readonly Material rust;
            public readonly Material clampMaterial;

            public Palette(Material main, Material rust, Material clampMaterial)
            {
                this.main = main;
                this.rust = rust;
                this.clampMaterial = clampMaterial;
            }
        }
    }
}
