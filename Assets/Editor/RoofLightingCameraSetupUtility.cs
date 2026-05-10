using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Unity.Cinemachine;
using MazeChase.AI;

#pragma warning disable CS0618 // Project currently uses Cinemachine 2.x components.
public static class RoofLightingCameraSetupUtility
{
    private const string RoofLayerName = "Roof";
    private const string RoofRootName = "MazeRoof";
    private const string LightingRootName = "MazeInteriorLighting";
    private const string ProbeRootName = "MazeLightProbes";
    private const string RoofMaterialPath = "Assets/Textures/Roof_DarkConcrete.mat";
    private const string StripMaterialPath = "Assets/Textures/WarmStripLight_Emissive.mat";
    private const string WallLightMaterialPath = "Assets/Textures/WarmWallLight_Emissive.mat";

    [MenuItem("Maze Chase/Apply Roof Lighting And Cameras")]
    public static void Apply()
    {
        int roofLayer = EnsureLayer(RoofLayerName);
        int minimapLayer = LayerMask.NameToLayer("MinimapIcon");
        int graphLayer = LayerMask.NameToLayer("Graph_Node");
        int wallLayer = LayerMask.NameToLayer("Maze_Wall");

        Bounds wallBounds = CalculateWallBounds(wallLayer);
        Bounds mazeBounds = CalculateMazeBounds(wallBounds);
        float roofY = Mathf.Max(wallBounds.max.y + 1.45f, mazeBounds.min.y + 6.4f);

        Material roofMaterial = CreateRoofMaterial();
        Material stripMaterial = CreateStripMaterial();
        Material wallLightMaterial = CreateWallLightMaterial();

        RecreateRoof(mazeBounds, roofY, roofLayer, roofMaterial);
        RecreateInteriorLights(roofY, roofLayer, stripMaterial, wallLightMaterial, wallLayer);
        RecreateLightProbes(roofY);
        ConfigureCharacterLighting();
        ConfigureCameras(roofLayer, minimapLayer, graphLayer);

        AssetDatabase.SaveAssets();
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log("Roof, warm interior lighting, light probes, and camera masks configured.");
    }

    private static int EnsureLayer(string layerName)
    {
        int existing = LayerMask.NameToLayer(layerName);
        if (existing >= 0)
            return existing;

        SerializedObject tagManager = new SerializedObject(
            AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty layers = tagManager.FindProperty("layers");

        for (int i = 11; i < 32; i++)
        {
            SerializedProperty layer = layers.GetArrayElementAtIndex(i);
            if (!string.IsNullOrEmpty(layer.stringValue))
                continue;

            layer.stringValue = layerName;
            tagManager.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
            return i;
        }

        throw new System.InvalidOperationException("No empty Unity layer slot is available for " + layerName);
    }

    private static Bounds CalculateWallBounds(int wallLayer)
    {
        bool hasBounds = false;
        Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);

        foreach (MeshRenderer renderer in Object.FindObjectsByType<MeshRenderer>(FindObjectsInactive.Exclude))
        {
            if (renderer.gameObject.layer != wallLayer || renderer.name == "TopWhiteCap")
                continue;

            if (!hasBounds)
            {
                bounds = renderer.bounds;
                hasBounds = true;
            }
            else
            {
                bounds.Encapsulate(renderer.bounds);
            }
        }

        if (!hasBounds)
            throw new System.InvalidOperationException("No Maze_Wall renderers found.");

        return bounds;
    }

    private static Bounds CalculateMazeBounds(Bounds fallback)
    {
        GameObject ground = GameObject.Find("Maze_geometry/Cube");
        if (ground != null && ground.TryGetComponent(out MeshRenderer renderer))
            return renderer.bounds;

        fallback.Expand(new Vector3(10f, 0f, 10f));
        return fallback;
    }

    private static Material CreateRoofMaterial()
    {
        Material material = AssetDatabase.LoadAssetAtPath<Material>(RoofMaterialPath);
        if (material == null)
        {
            material = new Material(FindLitShader())
            {
                name = "Roof_DarkConcrete"
            };
            AssetDatabase.CreateAsset(material, RoofMaterialPath);
        }

        SetColor(material, "_BaseColor", new Color(0.12f, 0.11f, 0.10f, 1f));
        SetColor(material, "_Color", new Color(0.12f, 0.11f, 0.10f, 1f));
        SetFloat(material, "_Metallic", 0f);
        SetFloat(material, "_Smoothness", 0.24f);
        EditorUtility.SetDirty(material);
        return material;
    }

    private static Material CreateStripMaterial()
    {
        Material material = AssetDatabase.LoadAssetAtPath<Material>(StripMaterialPath);
        if (material == null)
        {
            material = new Material(FindLitShader())
            {
                name = "WarmStripLight_Emissive"
            };
            AssetDatabase.CreateAsset(material, StripMaterialPath);
        }

        Color warm = new Color(1f, 0.55f, 0.22f, 1f);
        SetColor(material, "_BaseColor", warm);
        SetColor(material, "_Color", warm);
        if (material.HasProperty("_EmissionColor"))
        {
            material.SetColor("_EmissionColor", warm * 2.4f);
            material.EnableKeyword("_EMISSION");
        }
        EditorUtility.SetDirty(material);
        return material;
    }

    private static Material CreateWallLightMaterial()
    {
        Material material = AssetDatabase.LoadAssetAtPath<Material>(WallLightMaterialPath);
        if (material == null)
        {
            material = new Material(FindLitShader())
            {
                name = "WarmWallLight_Emissive"
            };
            AssetDatabase.CreateAsset(material, WallLightMaterialPath);
        }

        Color warm = new Color(1f, 0.62f, 0.32f, 1f);
        SetColor(material, "_BaseColor", warm);
        SetColor(material, "_Color", warm);
        if (material.HasProperty("_EmissionColor"))
        {
            material.SetColor("_EmissionColor", warm * 3.1f);
            material.EnableKeyword("_EMISSION");
        }

        EditorUtility.SetDirty(material);
        return material;
    }

    private static Shader FindLitShader()
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
            shader = Shader.Find("Standard");
        return shader;
    }

    private static void RecreateRoof(Bounds mazeBounds, float roofY, int roofLayer, Material roofMaterial)
    {
        DestroyIfExists(RoofRootName);

        GameObject root = new GameObject(RoofRootName);
        root.layer = roofLayer;

        int panelsX = 4;
        int panelsZ = 4;
        float overlap = 0.35f;
        float thickness = 0.35f;
        float width = mazeBounds.size.x + 7f;
        float depth = mazeBounds.size.z + 7f;
        float panelWidth = width / panelsX;
        float panelDepth = depth / panelsZ;
        Vector3 min = new Vector3(mazeBounds.center.x - width * 0.5f, roofY, mazeBounds.center.z - depth * 0.5f);

        for (int x = 0; x < panelsX; x++)
        {
            for (int z = 0; z < panelsZ; z++)
            {
                GameObject panel = GameObject.CreatePrimitive(PrimitiveType.Cube);
                panel.name = "RoofPanel_" + x + "_" + z;
                panel.layer = roofLayer;
                panel.transform.SetParent(root.transform, false);
                panel.transform.position = new Vector3(
                    min.x + panelWidth * (x + 0.5f),
                    roofY,
                    min.z + panelDepth * (z + 0.5f));
                panel.transform.localScale = new Vector3(panelWidth + overlap, thickness, panelDepth + overlap);

                panel.GetComponent<MeshRenderer>().sharedMaterial = roofMaterial;
                Object.DestroyImmediate(panel.GetComponent<Collider>());
            }
        }
    }

    private static void RecreateInteriorLights(float roofY, int roofLayer, Material stripMaterial, Material wallLightMaterial, int wallLayer)
    {
        DestroyIfExists(LightingRootName);

        GameObject root = new GameObject(LightingRootName);
        Transform graphRoot = GameObject.Find("GraphNodes")?.transform;
        if (graphRoot == null)
            return;

        List<Transform> nodes = new List<Transform>();
        foreach (Transform child in graphRoot)
            nodes.Add(child);

        nodes.Sort((a, b) =>
        {
            int z = a.position.z.CompareTo(b.position.z);
            return z != 0 ? z : a.position.x.CompareTo(b.position.x);
        });

        List<Bounds> wallBounds = CollectWallBounds(wallLayer);
        Color warm = new Color(1f, 0.55f, 0.22f);
        Color wallWarm = new Color(1f, 0.62f, 0.34f);
        int created = 0;
        for (int i = 0; i < nodes.Count; i++)
        {
            Vector3 position = nodes[i].position;
            position.y = roofY - 0.55f;

            GameObject fixture = GameObject.CreatePrimitive(PrimitiveType.Cube);
            fixture.name = "WarmCeilingStrip_" + created.ToString("00");
            fixture.layer = roofLayer;
            fixture.transform.SetParent(root.transform, true);
            fixture.transform.position = position;
            fixture.transform.rotation = Quaternion.Euler(0f, created % 2 == 0 ? 0f : 90f, 0f);
            fixture.transform.localScale = new Vector3(2.3f, 0.05f, 0.42f);
            fixture.GetComponent<MeshRenderer>().sharedMaterial = stripMaterial;
            Object.DestroyImmediate(fixture.GetComponent<Collider>());

            Light light = fixture.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = warm;
            light.intensity = 2.75f;
            light.range = 12f;
            light.shadows = LightShadows.None;
            light.lightmapBakeType = LightmapBakeType.Mixed;

            if (i % 2 == 0)
                CreateWallLight(root.transform, nodes[i].position, wallBounds, roofLayer, wallLightMaterial, wallWarm, created);

            created++;
        }

        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.15f, 0.12f, 0.095f);
    }

    private static List<Bounds> CollectWallBounds(int wallLayer)
    {
        List<Bounds> bounds = new List<Bounds>();
        foreach (MeshRenderer renderer in Object.FindObjectsByType<MeshRenderer>(FindObjectsInactive.Exclude))
        {
            if (renderer.gameObject.layer != wallLayer || renderer.name == "TopWhiteCap")
                continue;

            bounds.Add(renderer.bounds);
        }

        return bounds;
    }

    private static void CreateWallLight(Transform root, Vector3 nodePosition, List<Bounds> wallBounds, int fixtureLayer, Material material, Color color, int index)
    {
        Vector3 bestPoint = Vector3.zero;
        Vector3 bestNormal = Vector3.zero;
        float bestDistance = float.MaxValue;
        Vector3 origin = nodePosition;
        origin.y += 1.55f;

        foreach (Bounds bounds in wallBounds)
        {
            Vector3 closest = bounds.ClosestPoint(origin);
            float distance = Vector3.Distance(origin, closest);
            if (distance > 4.2f || distance >= bestDistance)
                continue;

            Vector3 normal = closest - bounds.center;
            float xExtent = Mathf.Max(bounds.extents.x, 0.001f);
            float zExtent = Mathf.Max(bounds.extents.z, 0.001f);
            float xRatio = Mathf.Abs(normal.x) / xExtent;
            float zRatio = Mathf.Abs(normal.z) / zExtent;
            normal = xRatio > zRatio
                ? new Vector3(Mathf.Sign(normal.x), 0f, 0f)
                : new Vector3(0f, 0f, Mathf.Sign(normal.z));

            if (normal == Vector3.zero)
                continue;

            bestPoint = closest;
            bestPoint.y = origin.y;
            bestNormal = normal;
            bestDistance = distance;
        }

        if (bestNormal == Vector3.zero)
            return;

        Vector3 position = bestPoint + bestNormal * 0.055f;

        GameObject fixture = GameObject.CreatePrimitive(PrimitiveType.Cube);
        fixture.name = "WarmWallSconce_" + index.ToString("00");
        fixture.layer = fixtureLayer;
        fixture.transform.SetParent(root, true);
        fixture.transform.position = position;
        fixture.transform.rotation = Quaternion.LookRotation(-bestNormal, Vector3.up);
        fixture.transform.localScale = new Vector3(0.42f, 0.32f, 0.08f);
        fixture.GetComponent<MeshRenderer>().sharedMaterial = material;
        Object.DestroyImmediate(fixture.GetComponent<Collider>());

        Light light = fixture.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = color;
        light.intensity = 1.9f;
        light.range = 6.5f;
        light.shadows = LightShadows.None;
        light.lightmapBakeType = LightmapBakeType.Mixed;
    }

    private static void RecreateLightProbes(float roofY)
    {
        DestroyIfExists(ProbeRootName);

        Transform graphRoot = GameObject.Find("GraphNodes")?.transform;
        if (graphRoot == null)
            return;

        GameObject root = new GameObject(ProbeRootName);
        LightProbeGroup group = root.AddComponent<LightProbeGroup>();
        List<Vector3> probes = new List<Vector3>();

        foreach (Transform node in graphRoot)
        {
            probes.Add(root.transform.InverseTransformPoint(new Vector3(node.position.x, node.position.y + 1.4f, node.position.z)));
            probes.Add(root.transform.InverseTransformPoint(new Vector3(node.position.x, roofY - 1.1f, node.position.z)));
        }

        group.probePositions = probes.ToArray();
    }

    private static void ConfigureCharacterLighting()
    {
        AddCharacterFillLight("PlayerArmature/Human_girl", "PlayerCharacterFillLight", new Color(1f, 0.78f, 0.55f), 0.95f);
        AddCharacterFillLight("AI_Racer_Robot/Ai_girl", "AICharacterFillLight", new Color(1f, 0.72f, 0.48f), 1.05f);

        int characterLayer = LayerMask.NameToLayer("Character");
        if (characterLayer < 0)
            return;

        foreach (Renderer renderer in Object.FindObjectsByType<Renderer>(FindObjectsInactive.Exclude))
        {
            if (renderer.gameObject.layer != characterLayer)
                continue;

            renderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.BlendProbes;
            renderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.BlendProbes;
            renderer.receiveShadows = true;
        }
    }

    private static void AddCharacterFillLight(string characterPath, string lightName, Color color, float intensity)
    {
        Transform character = GameObject.Find(characterPath)?.transform;
        if (character == null)
            return;

        Transform existing = character.Find(lightName);
        if (existing != null)
            Object.DestroyImmediate(existing.gameObject);

        GameObject lightObject = new GameObject(lightName);
        lightObject.transform.SetParent(character, false);
        lightObject.transform.localPosition = new Vector3(0f, 1.35f, 0.15f);

        Light light = lightObject.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = color;
        light.intensity = intensity;
        light.range = 3.25f;
        light.shadows = LightShadows.None;
        light.lightmapBakeType = LightmapBakeType.Realtime;
    }

    private static void ConfigureCameras(int roofLayer, int minimapLayer, int graphLayer)
    {
        int all = ~0;
        int noRoof = all & ~(1 << roofLayer);
        int noMinimapIcon = all & ~(1 << minimapLayer);
        int noMinimapIconOrRoof = noMinimapIcon & ~(1 << roofLayer);
        int noDebugIcons = noMinimapIcon & ~(1 << graphLayer);

        Camera minimap = GameObject.Find("MinimapCamera")?.GetComponent<Camera>();
        if (minimap != null)
            minimap.cullingMask = noRoof;

        Camera topDown = GameObject.Find("TopDownCamera")?.GetComponent<Camera>();
        if (topDown != null)
            topDown.cullingMask = noMinimapIconOrRoof;

        Camera main = GameObject.Find("MainCamera")?.GetComponent<Camera>();
        if (main != null)
        {
            main.cullingMask = noDebugIcons;
            main.fieldOfView = 54f;
        }

        Camera aiCamera = GameObject.Find("AIFollowCamera")?.GetComponent<Camera>();
        if (aiCamera != null)
        {
            aiCamera.cullingMask = noMinimapIcon;
            aiCamera.fieldOfView = 56f;
            aiCamera.nearClipPlane = 0.15f;
        }

        AICameraFollow aiFollow = GameObject.Find("AIFollowCamera")?.GetComponent<AICameraFollow>();
        if (aiFollow != null)
        {
            aiFollow.offset = new Vector3(0f, 1.85f, -3.65f);
            aiFollow.smoothSpeed = 7.5f;
            aiFollow.rotationSpeed = 7f;
        }

        CinemachineVirtualCamera playerCamera = GameObject.Find("PlayerFollowCamera")?.GetComponent<CinemachineVirtualCamera>();
        if (playerCamera != null)
        {
            playerCamera.m_Lens.FieldOfView = 54f;
            playerCamera.m_Lens.NearClipPlane = 0.15f;
        }

        Cinemachine3rdPersonFollow follow = GameObject.Find("PlayerFollowCamera/cm")?.GetComponent<Cinemachine3rdPersonFollow>();
        if (follow != null)
        {
            follow.ShoulderOffset = new Vector3(0.65f, 0.85f, 0f);
            follow.CameraDistance = 3.25f;
            follow.CameraSide = 0.55f;
            follow.CameraRadius = 0.2f;
            follow.Damping = new Vector3(0.1f, 0.18f, 0.22f);
            follow.CameraCollisionFilter = LayerMask.GetMask("Default", "Ground_Walkable", "Maze_Wall", "Door_Obstacle", RoofLayerName);
        }
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
