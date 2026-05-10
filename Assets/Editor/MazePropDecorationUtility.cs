using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class MazePropDecorationUtility
{
    private const string PropRootName = "MazeFacilityProps";
    private const string WallLayerName = "Maze_Wall";
    private const int MaxProps = 20;

    private const string MetalBodyMaterialPath = "Assets/Textures/Prop_MetalSupplyBox_Dark.mat";
    private const string MetalLidMaterialPath = "Assets/Textures/Prop_MetalSupplyBox_Lid.mat";
    private const string BlackMaterialPath = "Assets/Textures/Prop_MatteBlack.mat";
    private const string WarningLabelMaterialPath = "Assets/Textures/Prop_FadedWarningLabel.mat";
    private const string IndicatorMaterialPath = "Assets/Textures/Prop_DimRedIndicator.mat";
    private const string WoodMaterialPath = "Assets/Textures/Prop_DarkWoodCrate.mat";
    private const string CardboardMaterialPath = "Assets/Textures/Prop_DullCardboard.mat";
    private const string TapeMaterialPath = "Assets/Textures/Prop_DarkTape.mat";

    [MenuItem("Maze Chase/Apply Maze Facility Props")]
    public static void ApplyFacilityProps()
    {
        int wallLayer = LayerMask.NameToLayer(WallLayerName);
        if (wallLayer < 0)
            throw new System.InvalidOperationException("Layer " + WallLayerName + " was not found.");

        List<MeshRenderer> walls = CollectWalls(wallLayer);
        if (walls.Count == 0)
            throw new System.InvalidOperationException("No maze wall renderers were found.");

        Transform graphRoot = GameObject.Find("GraphNodes")?.transform;
        if (graphRoot == null)
            throw new System.InvalidOperationException("GraphNodes root was not found.");

        DestroyIfExists(PropRootName);

        Materials materials = CreateMaterials();
        float floorY = CalculateFloorY(walls);
        List<Vector3> nodes = CollectGraphNodes(graphRoot);
        List<Candidate> candidates = BuildCandidates(nodes, walls);

        GameObject root = new GameObject(PropRootName);
        root.layer = 0;

        int propsCreated = 0;
        List<Vector3> placed = new List<Vector3>();
        for (int i = 0; i < candidates.Count && propsCreated < MaxProps; i++)
        {
            Candidate candidate = candidates[i];
            if (IsNearSpawn(candidate.position) || IsTooCloseToPlaced(candidate.position, placed))
                continue;

            PropKind kind = ChooseKind(propsCreated);
            Vector3 scale = ChooseScale(kind, propsCreated);
            Vector3 position = candidate.position;
            position.y = floorY + scale.y * 0.5f;

            Quaternion rotation = Quaternion.LookRotation(candidate.normal, Vector3.up);
            rotation *= Quaternion.Euler(0f, propsCreated % 2 == 0 ? -7.5f : 6f, 0f);

            Transform prop = new GameObject(kind.ToString() + "_" + propsCreated.ToString("00")).transform;
            prop.SetParent(root.transform, false);
            prop.position = position;
            prop.rotation = rotation;
            prop.localScale = Vector3.one;

            switch (kind)
            {
                case PropKind.MetalSupplyBox:
                    BuildMetalSupplyBox(prop, scale, materials, propsCreated);
                    break;
                case PropKind.WoodCrate:
                    BuildWoodCrate(prop, scale, materials);
                    break;
                case PropKind.CardboardBox:
                    BuildCardboardBox(prop, scale, materials);
                    break;
            }

            placed.Add(candidate.position);
            propsCreated++;
        }

        AssetDatabase.SaveAssets();
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log("Applied " + propsCreated + " maze facility props near walls and corners.");
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

        return walls;
    }

    private static List<Vector3> CollectGraphNodes(Transform graphRoot)
    {
        List<Vector3> nodes = new List<Vector3>();
        foreach (Transform child in graphRoot)
            nodes.Add(child.position);

        nodes.Sort((a, b) =>
        {
            int z = a.z.CompareTo(b.z);
            return z != 0 ? z : a.x.CompareTo(b.x);
        });

        return nodes;
    }

    private static List<Candidate> BuildCandidates(List<Vector3> nodes, List<MeshRenderer> walls)
    {
        List<Candidate> candidates = new List<Candidate>();
        for (int i = 0; i < nodes.Count; i++)
        {
            Vector3 node = nodes[i];
            if (!FindWallAnchor(node, walls, out Vector3 anchor, out Vector3 normal))
                continue;

            int degree = EstimateNodeDegree(node, nodes);
            Vector3 tangent = new Vector3(-normal.z, 0f, normal.x);
            float sideOffset = i % 2 == 0 ? 0.42f : -0.42f;
            Vector3 position = anchor + normal * 0.48f + tangent * sideOffset;

            float score = degree <= 1 ? 0f : degree == 2 ? 1f : 2f;
            score += i * 0.01f;
            candidates.Add(new Candidate(position, normal, score));
        }

        candidates.Sort((a, b) => a.score.CompareTo(b.score));
        return candidates;
    }

    private static bool FindWallAnchor(Vector3 node, List<MeshRenderer> walls, out Vector3 anchor, out Vector3 normal)
    {
        anchor = Vector3.zero;
        normal = Vector3.zero;
        float bestDistance = float.MaxValue;

        for (int i = 0; i < walls.Count; i++)
        {
            Bounds bounds = walls[i].bounds;
            Vector3 closest = bounds.ClosestPoint(node);
            Vector3 outward = node - closest;
            outward.y = 0f;
            float distance = outward.magnitude;
            if (distance < 0.15f || distance > 4.5f || distance >= bestDistance)
                continue;

            anchor = closest;
            anchor.y = bounds.min.y;
            normal = outward.normalized;
            bestDistance = distance;
        }

        return normal != Vector3.zero;
    }

    private static int EstimateNodeDegree(Vector3 node, List<Vector3> nodes)
    {
        int degree = 0;
        for (int i = 0; i < nodes.Count; i++)
        {
            Vector3 other = nodes[i];
            if (other == node)
                continue;

            float dx = Mathf.Abs(other.x - node.x);
            float dz = Mathf.Abs(other.z - node.z);
            bool aligned = dx < 0.75f || dz < 0.75f;
            if (aligned && Vector2.Distance(new Vector2(other.x, other.z), new Vector2(node.x, node.z)) < 7.25f)
                degree++;
        }

        return degree;
    }

    private static bool IsNearSpawn(Vector3 position)
    {
        return IsNearObject(position, "PlayerArmature", 9f) || IsNearObject(position, "AI_Racer_Robot", 9f);
    }

    private static bool IsNearObject(Vector3 position, string objectName, float radius)
    {
        GameObject target = GameObject.Find(objectName);
        if (target == null)
            return false;

        return Vector2.Distance(
            new Vector2(position.x, position.z),
            new Vector2(target.transform.position.x, target.transform.position.z)) < radius;
    }

    private static bool IsTooCloseToPlaced(Vector3 position, List<Vector3> placed)
    {
        for (int i = 0; i < placed.Count; i++)
        {
            if (Vector2.Distance(new Vector2(position.x, position.z), new Vector2(placed[i].x, placed[i].z)) < 8f)
                return true;
        }

        return false;
    }

    private static PropKind ChooseKind(int index)
    {
        if (index % 7 == 4)
            return PropKind.CardboardBox;

        if (index % 5 == 3)
            return PropKind.WoodCrate;

        return PropKind.MetalSupplyBox;
    }

    private static Vector3 ChooseScale(PropKind kind, int index)
    {
        float variation = index % 3 == 0 ? 1.08f : index % 3 == 1 ? 0.92f : 1f;
        switch (kind)
        {
            case PropKind.WoodCrate:
                return new Vector3(0.78f, 0.72f, 0.78f) * variation;
            case PropKind.CardboardBox:
                return new Vector3(0.86f, 0.52f, 0.62f) * variation;
            default:
                return new Vector3(0.9f, 0.56f, 0.62f) * variation;
        }
    }

    private static float CalculateFloorY(List<MeshRenderer> walls)
    {
        GameObject ground = GameObject.Find("Maze_geometry/Cube");
        if (ground != null && ground.TryGetComponent(out MeshRenderer groundRenderer))
            return groundRenderer.bounds.max.y + 0.02f;

        float minY = float.MaxValue;
        for (int i = 0; i < walls.Count; i++)
            minY = Mathf.Min(minY, walls[i].bounds.min.y);

        return minY + 0.02f;
    }

    private static void BuildMetalSupplyBox(Transform root, Vector3 size, Materials materials, int index)
    {
        CreatePiece(root, "Body", Vector3.zero, size, materials.metalBody);
        CreatePiece(root, "TopLid", new Vector3(0f, size.y * 0.54f, 0f), new Vector3(size.x * 1.04f, size.y * 0.08f, size.z * 1.04f), materials.metalLid);
        CreatePiece(root, "FrontHandle", new Vector3(0f, size.y * 0.12f, size.z * 0.52f), new Vector3(size.x * 0.34f, size.y * 0.09f, size.z * 0.08f), materials.black);
        CreatePiece(root, "WarningLabel", new Vector3(-size.x * 0.2f, -size.y * 0.08f, size.z * 0.535f), new Vector3(size.x * 0.25f, size.y * 0.18f, size.z * 0.025f), materials.warningLabel);

        if (index % 2 == 0)
            CreatePiece(root, "DimIndicator", new Vector3(size.x * 0.24f, size.y * 0.1f, size.z * 0.545f), new Vector3(size.x * 0.075f, size.y * 0.075f, size.z * 0.03f), materials.indicator);
    }

    private static void BuildWoodCrate(Transform root, Vector3 size, Materials materials)
    {
        CreatePiece(root, "Body", Vector3.zero, size, materials.wood);
        CreatePiece(root, "VerticalBraceLeft", new Vector3(-size.x * 0.36f, 0f, size.z * 0.525f), new Vector3(size.x * 0.08f, size.y * 1.05f, size.z * 0.035f), materials.wood);
        CreatePiece(root, "VerticalBraceRight", new Vector3(size.x * 0.36f, 0f, size.z * 0.525f), new Vector3(size.x * 0.08f, size.y * 1.05f, size.z * 0.035f), materials.wood);
        CreatePiece(root, "CrossBraceA", new Vector3(0f, 0f, size.z * 0.555f), new Vector3(size.x * 1.12f, size.y * 0.075f, size.z * 0.035f), materials.wood, Quaternion.Euler(0f, 0f, 38f));
        CreatePiece(root, "CrossBraceB", new Vector3(0f, 0f, size.z * 0.56f), new Vector3(size.x * 1.12f, size.y * 0.075f, size.z * 0.035f), materials.wood, Quaternion.Euler(0f, 0f, -38f));
    }

    private static void BuildCardboardBox(Transform root, Vector3 size, Materials materials)
    {
        CreatePiece(root, "Body", Vector3.zero, size, materials.cardboard);
        CreatePiece(root, "TopTape", new Vector3(0f, size.y * 0.53f, 0f), new Vector3(size.x * 0.18f, size.y * 0.035f, size.z * 1.04f), materials.tape);
        CreatePiece(root, "FrontTape", new Vector3(0f, size.y * 0.08f, size.z * 0.525f), new Vector3(size.x * 0.16f, size.y * 0.82f, size.z * 0.025f), materials.tape);
    }

    private static GameObject CreatePiece(Transform parent, string name, Vector3 position, Vector3 scale, Material material)
    {
        return CreatePiece(parent, name, position, scale, material, Quaternion.identity);
    }

    private static GameObject CreatePiece(Transform parent, string name, Vector3 position, Vector3 scale, Material material, Quaternion rotation)
    {
        GameObject piece = GameObject.CreatePrimitive(PrimitiveType.Cube);
        piece.name = name;
        piece.layer = 0;
        piece.transform.SetParent(parent, false);
        piece.transform.localPosition = position;
        piece.transform.localRotation = rotation;
        piece.transform.localScale = scale;
        piece.GetComponent<MeshRenderer>().sharedMaterial = material;
        Object.DestroyImmediate(piece.GetComponent<Collider>());
        return piece;
    }

    private static Materials CreateMaterials()
    {
        Materials materials = new Materials
        {
            metalBody = LoadOrCreateMaterial(MetalBodyMaterialPath, "Prop_MetalSupplyBox_Dark"),
            metalLid = LoadOrCreateMaterial(MetalLidMaterialPath, "Prop_MetalSupplyBox_Lid"),
            black = LoadOrCreateMaterial(BlackMaterialPath, "Prop_MatteBlack"),
            warningLabel = LoadOrCreateMaterial(WarningLabelMaterialPath, "Prop_FadedWarningLabel"),
            indicator = LoadOrCreateMaterial(IndicatorMaterialPath, "Prop_DimRedIndicator"),
            wood = LoadOrCreateMaterial(WoodMaterialPath, "Prop_DarkWoodCrate"),
            cardboard = LoadOrCreateMaterial(CardboardMaterialPath, "Prop_DullCardboard"),
            tape = LoadOrCreateMaterial(TapeMaterialPath, "Prop_DarkTape")
        };

        ConfigureMaterial(materials.metalBody, new Color(0.12f, 0.16f, 0.18f, 1f), 0.18f, 0.28f);
        ConfigureMaterial(materials.metalLid, new Color(0.28f, 0.31f, 0.32f, 1f), 0.12f, 0.24f);
        ConfigureMaterial(materials.black, new Color(0.015f, 0.014f, 0.013f, 1f), 0f, 0.18f);
        ConfigureMaterial(materials.warningLabel, new Color(0.82f, 0.58f, 0.18f, 1f), 0f, 0.2f);
        ConfigureMaterial(materials.indicator, new Color(0.5f, 0.04f, 0.03f, 1f), 0f, 0.26f, new Color(0.7f, 0.03f, 0.02f, 1f));
        ConfigureMaterial(materials.wood, new Color(0.18f, 0.10f, 0.055f, 1f), 0f, 0.16f);
        ConfigureMaterial(materials.cardboard, new Color(0.42f, 0.32f, 0.21f, 1f), 0f, 0.12f);
        ConfigureMaterial(materials.tape, new Color(0.22f, 0.16f, 0.10f, 1f), 0f, 0.1f);

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

    private static void ConfigureMaterial(Material material, Color baseColor, float metallic, float smoothness, Color? emission = null)
    {
        SetColor(material, "_BaseColor", baseColor);
        SetColor(material, "_Color", baseColor);
        SetFloat(material, "_Metallic", metallic);
        SetFloat(material, "_Smoothness", smoothness);

        if (emission.HasValue && material.HasProperty("_EmissionColor"))
        {
            material.SetColor("_EmissionColor", emission.Value);
            material.EnableKeyword("_EMISSION");
        }

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

    private enum PropKind
    {
        MetalSupplyBox,
        WoodCrate,
        CardboardBox
    }

    private readonly struct Candidate
    {
        public readonly Vector3 position;
        public readonly Vector3 normal;
        public readonly float score;

        public Candidate(Vector3 position, Vector3 normal, float score)
        {
            this.position = position;
            this.normal = normal;
            this.score = score;
        }
    }

    private sealed class Materials
    {
        public Material metalBody;
        public Material metalLid;
        public Material black;
        public Material warningLabel;
        public Material indicator;
        public Material wood;
        public Material cardboard;
        public Material tape;
    }
}
