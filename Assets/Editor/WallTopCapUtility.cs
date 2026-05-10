using UnityEditor;
using UnityEngine;

public static class WallTopCapUtility
{
    private const string MaterialPath = "Assets/Textures/WallTop_PureWhite.mat";
    private const string CapName = "TopWhiteCap";

    [MenuItem("Maze Chase/Apply White Wall Tops")]
    public static void ApplyWhiteWallTops()
    {
        Material capMaterial = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
        if (capMaterial == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
                shader = Shader.Find("Standard");

            capMaterial = new Material(shader)
            {
                name = "WallTop_PureWhite"
            };

            SetColor(capMaterial, "_BaseColor", Color.white);
            SetColor(capMaterial, "_Color", Color.white);
            SetFloat(capMaterial, "_Metallic", 0f);
            SetFloat(capMaterial, "_Smoothness", 0.22f);

            AssetDatabase.CreateAsset(capMaterial, MaterialPath);
        }

        int wallLayer = LayerMask.NameToLayer("Maze_Wall");
        int updated = 0;

        foreach (MeshRenderer renderer in Object.FindObjectsByType<MeshRenderer>(FindObjectsInactive.Exclude))
        {
            GameObject wall = renderer.gameObject;
            if (wall.layer != wallLayer)
                continue;

            if (wall.name == CapName)
                continue;

            if (wall.transform.Find(CapName) != null)
                continue;

            GameObject cap = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cap.name = CapName;
            cap.layer = wall.layer;
            cap.transform.SetParent(wall.transform, false);
            cap.transform.localPosition = new Vector3(0f, 0.506f, 0f);
            cap.transform.localRotation = Quaternion.identity;
            cap.transform.localScale = new Vector3(1.01f, 0.018f, 1.01f);

            MeshRenderer capRenderer = cap.GetComponent<MeshRenderer>();
            capRenderer.sharedMaterial = capMaterial;

            Object.DestroyImmediate(cap.GetComponent<Collider>());
            updated++;
        }

        AssetDatabase.SaveAssets();
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log("Applied white wall top caps: " + updated);
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
