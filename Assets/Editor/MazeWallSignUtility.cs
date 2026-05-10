using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;

public static class MazeWallSignUtility
{
    private const string SignRootName = "MazeWallSigns";
    private const string WallLayerName = "Maze_Wall";
    private const int MaxSigns = 28;
    private const int DistributionGridSize = 4;

    private const string PlateMaterialPath = "Assets/Textures/Sign_DarkPlate.mat";
    private const string BorderMaterialPath = "Assets/Textures/Sign_DirtyWhiteBorder.mat";
    private const string YellowTextMaterialPath = "Assets/Textures/Sign_TextDarkYellow.mat";
    private const string RedTextMaterialPath = "Assets/Textures/Sign_TextFadedRed.mat";
    private const string WhiteTextMaterialPath = "Assets/Textures/Sign_TextDirtyWhite.mat";
    private const string GrayTextMaterialPath = "Assets/Textures/Sign_TextGray.mat";

    private static readonly string[] SignTexts =
    {
        "EXIT ->",
        "DANGER",
        "SECTOR A",
        "STORAGE",
        "KEEP OUT",
        "MAINT",
        "SECTOR B",
        "<- EXIT",
        "LOW POWER",
        "CAUTION"
    };

    [MenuItem("Maze Chase/Apply Maze Wall Signs")]
    public static void ApplyWallSigns()
    {
        int wallLayer = LayerMask.NameToLayer(WallLayerName);
        if (wallLayer < 0)
            throw new System.InvalidOperationException("Layer " + WallLayerName + " was not found.");

        List<MeshRenderer> walls = CollectWalls(wallLayer);
        if (walls.Count == 0)
            throw new System.InvalidOperationException("No maze wall renderers were found.");

        DestroyIfExists(SignRootName);

        SignMaterials materials = CreateMaterials();
        List<MeshRenderer> selectedWalls = SelectDistributedWalls(walls);
        GameObject root = new GameObject(SignRootName);
        root.layer = wallLayer;

        int created = 0;
        for (int i = 0; i < selectedWalls.Count && created < MaxSigns; i++)
        {
            MeshRenderer wall = selectedWalls[i];
            Bounds bounds = wall.bounds;
            bool alongX = bounds.size.x >= bounds.size.z;
            float wallLength = alongX ? bounds.size.x : bounds.size.z;
            if (wallLength < 2.4f)
                continue;

            float side = ChooseVisibleSide(bounds, alongX);
            Vector3 normal = alongX ? new Vector3(0f, 0f, side) : new Vector3(side, 0f, 0f);
            Vector3 tangent = alongX ? Vector3.right : Vector3.forward;
            float depth = alongX ? bounds.size.z : bounds.size.x;
            float lateral = created % 2 == 0 ? -wallLength * 0.12f : wallLength * 0.13f;

            Vector3 center = bounds.center + tangent * lateral + normal * (depth * 0.5f + 0.032f);
            center.y = bounds.center.y + Mathf.Clamp(bounds.size.y * 0.16f, 0.26f, 0.48f);

            Transform sign = new GameObject("WallSign_" + created.ToString("00")).transform;
            sign.SetParent(root.transform, false);
            sign.position = center;
            sign.rotation = Quaternion.LookRotation(-normal, Vector3.up);
            sign.gameObject.layer = wallLayer;

            SignStyle style = ChooseStyle(created, materials);
            string text = SignTexts[created % SignTexts.Length];
            Vector2 size = ChooseSize(text);

            BuildSign(sign, text, size, style);
            created++;
        }

        AssetDatabase.SaveAssets();
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log("Applied " + created + " warning and direction signs to the maze walls.");
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

        return walls;
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
            Bounds bounds = wall.bounds;
            float length = Mathf.Max(bounds.size.x, bounds.size.z);
            if (length < 2.4f || i % 3 == 1)
                continue;

            int bucket = CalculateDistributionBucket(bounds.center, mazeBounds);
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
        while (selected.Count < MaxSigns)
        {
            bool addedAny = false;
            for (int bucket = 0; bucket < buckets.Length && selected.Count < MaxSigns; bucket++)
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

    private static void BuildSign(Transform parent, string text, Vector2 size, SignStyle style)
    {
        CreatePlate(parent, "Plate", Vector3.zero, new Vector3(size.x, size.y, 0.045f), style.plateMaterial);
        CreatePlate(parent, "TopBorder", new Vector3(0f, size.y * 0.48f, -0.028f), new Vector3(size.x * 1.04f, 0.035f, 0.025f), style.borderMaterial);
        CreatePlate(parent, "BottomBorder", new Vector3(0f, -size.y * 0.48f, -0.028f), new Vector3(size.x * 1.04f, 0.035f, 0.025f), style.borderMaterial);
        CreatePlate(parent, "LeftBorder", new Vector3(-size.x * 0.49f, 0f, -0.028f), new Vector3(0.035f, size.y * 1.04f, 0.025f), style.borderMaterial);
        CreatePlate(parent, "RightBorder", new Vector3(size.x * 0.49f, 0f, -0.028f), new Vector3(0.035f, size.y * 1.04f, 0.025f), style.borderMaterial);
        CreateText(parent, text, size, style.textMaterial, style.textColor);
    }

    private static GameObject CreatePlate(Transform parent, string name, Vector3 localPosition, Vector3 localScale, Material material)
    {
        GameObject plate = GameObject.CreatePrimitive(PrimitiveType.Cube);
        plate.name = name;
        plate.layer = parent.gameObject.layer;
        plate.transform.SetParent(parent, false);
        plate.transform.localPosition = localPosition;
        plate.transform.localRotation = Quaternion.identity;
        plate.transform.localScale = localScale;
        plate.GetComponent<MeshRenderer>().sharedMaterial = material;
        Object.DestroyImmediate(plate.GetComponent<Collider>());
        return plate;
    }

    private static void CreateText(Transform parent, string value, Vector2 size, Material material, Color color)
    {
        GameObject textObject = new GameObject("Text_" + value.Replace(" ", "_").Replace("<", "L").Replace(">", "R"));
        textObject.layer = parent.gameObject.layer;
        textObject.transform.SetParent(parent, false);
        textObject.transform.localPosition = new Vector3(0f, -0.012f, -0.056f);
        textObject.transform.localRotation = Quaternion.identity;
        textObject.transform.localScale = Vector3.one;

        TextMeshPro text = textObject.AddComponent<TextMeshPro>();
        text.text = value;
        text.alignment = TextAlignmentOptions.Center;
        text.fontSize = value.Length > 8 ? 0.26f : 0.31f;
        text.color = color;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.rectTransform.sizeDelta = new Vector2(size.x * 0.88f, size.y * 0.68f);
        if (material != null)
            text.fontMaterial = material;
    }

    private static Vector2 ChooseSize(string text)
    {
        if (text.Length > 8)
            return new Vector2(1.34f, 0.58f);

        if (text.Contains("EXIT"))
            return new Vector2(1.16f, 0.5f);

        return new Vector2(1.08f, 0.5f);
    }

    private static SignStyle ChooseStyle(int index, SignMaterials materials)
    {
        switch (index % 4)
        {
            case 0:
                return new SignStyle(materials.plate, materials.border, materials.yellowText, new Color(0.78f, 0.58f, 0.18f, 1f));
            case 1:
                return new SignStyle(materials.plate, materials.border, materials.redText, new Color(0.72f, 0.16f, 0.12f, 1f));
            case 2:
                return new SignStyle(materials.plate, materials.border, materials.whiteText, new Color(0.76f, 0.72f, 0.62f, 1f));
            default:
                return new SignStyle(materials.plate, materials.border, materials.grayText, new Color(0.52f, 0.55f, 0.54f, 1f));
        }
    }

    private static float ChooseVisibleSide(Bounds bounds, bool alongX)
    {
        Vector3 mazeCenter = new Vector3(-41f, bounds.center.y, -907f);
        if (alongX)
            return bounds.center.z > mazeCenter.z ? -1f : 1f;

        return bounds.center.x > mazeCenter.x ? -1f : 1f;
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

    private static SignMaterials CreateMaterials()
    {
        SignMaterials materials = new SignMaterials
        {
            plate = LoadOrCreateMaterial(PlateMaterialPath, "Sign_DarkPlate"),
            border = LoadOrCreateMaterial(BorderMaterialPath, "Sign_DirtyWhiteBorder"),
            yellowText = LoadOrCreateMaterial(YellowTextMaterialPath, "Sign_TextDarkYellow"),
            redText = LoadOrCreateMaterial(RedTextMaterialPath, "Sign_TextFadedRed"),
            whiteText = LoadOrCreateMaterial(WhiteTextMaterialPath, "Sign_TextDirtyWhite"),
            grayText = LoadOrCreateMaterial(GrayTextMaterialPath, "Sign_TextGray")
        };

        ConfigureMaterial(materials.plate, new Color(0.08f, 0.085f, 0.08f, 1f), 0.08f, 0.2f);
        ConfigureMaterial(materials.border, new Color(0.58f, 0.54f, 0.45f, 1f), 0f, 0.16f);
        ConfigureMaterial(materials.yellowText, new Color(0.78f, 0.58f, 0.18f, 1f), 0f, 0.12f);
        ConfigureMaterial(materials.redText, new Color(0.72f, 0.16f, 0.12f, 1f), 0f, 0.12f);
        ConfigureMaterial(materials.whiteText, new Color(0.76f, 0.72f, 0.62f, 1f), 0f, 0.12f);
        ConfigureMaterial(materials.grayText, new Color(0.52f, 0.55f, 0.54f, 1f), 0f, 0.12f);

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

    private sealed class SignMaterials
    {
        public Material plate;
        public Material border;
        public Material yellowText;
        public Material redText;
        public Material whiteText;
        public Material grayText;
    }

    private readonly struct SignStyle
    {
        public readonly Material plateMaterial;
        public readonly Material borderMaterial;
        public readonly Material textMaterial;
        public readonly Color textColor;

        public SignStyle(Material plateMaterial, Material borderMaterial, Material textMaterial, Color textColor)
        {
            this.plateMaterial = plateMaterial;
            this.borderMaterial = borderMaterial;
            this.textMaterial = textMaterial;
            this.textColor = textColor;
        }
    }
}
