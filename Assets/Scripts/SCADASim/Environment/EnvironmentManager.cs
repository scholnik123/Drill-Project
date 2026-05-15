using SCADASim.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace SCADASim.Environment
{
    public sealed class EnvironmentManager : MonoBehaviour
    {
        private const string TreeResourcePath = "Models/environmentTree";

        [SerializeField] private EnvironmentPresetLibrary presetLibrary;
        [SerializeField] private SimulationEventChannel eventChannel;
        [SerializeField] private Transform environmentRoot;
        [SerializeField] private EnvironmentType initialEnvironment = EnvironmentType.Onshore;
        [SerializeField] private bool loadOnStart = true;

        private GameObject currentInstance;

        public EnvironmentType CurrentEnvironment { get; private set; }

        public void Configure(SimulationEventChannel channel, Transform root = null)
        {
            eventChannel = channel;
            environmentRoot = root;
            loadOnStart = false;
        }

        private void Start()
        {
            if (loadOnStart)
            {
                LoadEnvironment(initialEnvironment);
            }
        }

        public void LoadEnvironment(EnvironmentType type)
        {
            if (presetLibrary == null || !presetLibrary.TryGetPreset(type, out EnvironmentPreset preset))
            {
                LoadProceduralEnvironment(type);
                return;
            }

            if (preset.Prefab == null)
            {
                LoadProceduralEnvironment(type);
                return;
            }

            if (currentInstance != null)
            {
                Destroy(currentInstance);
            }

            Transform parent = environmentRoot != null ? environmentRoot : transform;
            Quaternion rotation = Quaternion.Euler(preset.SpawnEulerAngles);
            currentInstance = Instantiate(preset.Prefab, preset.SpawnPosition, rotation, parent);
            CurrentEnvironment = type;

            eventChannel?.Raise(
                SimulationEventType.EnvironmentLoaded,
                AlertSeverity.Info,
                $"Environment loaded: {type}",
                type);
        }

        private void LoadProceduralEnvironment(EnvironmentType type)
        {
            if (currentInstance != null)
            {
                Destroy(currentInstance);
            }

            Transform parent = environmentRoot != null ? environmentRoot : transform;
            currentInstance = new GameObject($"Procedural {type} Environment");
            currentInstance.transform.SetParent(parent, false);

            if (type == EnvironmentType.Offshore)
            {
                BuildOffshore(currentInstance.transform);
            }
            else
            {
                BuildOnshore(currentInstance.transform);
            }

            CurrentEnvironment = type;
            eventChannel?.Raise(
                SimulationEventType.EnvironmentLoaded,
                AlertSeverity.Info,
                $"Procedural environment loaded: {type}",
                type);
        }

        private static void BuildOnshore(Transform root)
        {
            Material ground = MakeMaterial("SCADA Field Ground", new Color(0.17f, 0.35f, 0.18f));
            Material grid = MakeMaterial("SCADA Field Grid", new Color(0.78f, 0.82f, 0.76f));
            Material road = MakeMaterial("SCADA Access Road", new Color(0.08f, 0.08f, 0.08f));
            Material steel = MakeMaterial("SCADA Steel", new Color(0.36f, 0.38f, 0.38f));

            AddCube(root, "Survey Field", new Vector3(0f, -0.08f, 0f), new Vector3(140f, 0.08f, 140f), ground);
            AddSurveyGrid(root, 140f, 8f, grid);
            AddCube(root, "Access Road", new Vector3(6f, 0.01f, -2f), new Vector3(160f, 0.05f, 5.5f), road)
                .transform.localRotation = Quaternion.Euler(0f, 25f, 0f);
            AddCube(root, "Rig Floor", new Vector3(0f, 1.2f, 0f), new Vector3(8f, 0.6f, 8f), steel);
            AddCube(root, "Mud Tank A", new Vector3(-9f, 0.6f, -7f), new Vector3(6f, 1.2f, 3f), steel);
            AddCube(root, "Mud Tank B", new Vector3(-9f, 0.6f, -3.5f), new Vector3(6f, 1.2f, 3f), steel);
            AddCube(root, "Pipe Rack", new Vector3(10f, 0.45f, -6f), new Vector3(10f, 0.4f, 3f), steel);
            AddSceneTree(root, new Vector3(-32f, 0f, -18f), 4.4f, true);
            AddSceneTree(root, new Vector3(22f, 0f, 26f), 4.0f, true);
            AddSceneTree(root, new Vector3(44f, 0f, -36f), 4.7f, true);
            AddSceneTree(root, new Vector3(-50f, 0f, 32f), 4.2f, false);
            AddSceneTree(root, new Vector3(-18f, 0f, 44f), 3.8f, false);
            AddSceneTree(root, new Vector3(36f, 0f, 18f), 4.5f, false);
        }

        private static void BuildOffshore(Transform root)
        {
            Material sea = MakeMaterial("SCADA Sea", new Color(0.02f, 0.12f, 0.18f));
            Material seaLine = MakeMaterial("SCADA Sea Grid", new Color(0.16f, 0.38f, 0.48f));

            AddCube(root, "Sea Plane", new Vector3(0f, -1.1f, 0f), new Vector3(70f, 0.08f, 70f), sea);
            AddSurveyGrid(root, 70f, 10f, seaLine);
        }

        private static void BuildDerrick(Transform root, Material steel, Material accent, float yOffset)
        {
            AddCube(root, "Derrick Leg A", new Vector3(-2.6f, 6f + yOffset, -2.6f), new Vector3(0.35f, 10f, 0.35f), steel);
            AddCube(root, "Derrick Leg B", new Vector3(2.6f, 6f + yOffset, -2.6f), new Vector3(0.35f, 10f, 0.35f), steel);
            AddCube(root, "Derrick Leg C", new Vector3(-1.1f, 13f + yOffset, 1.1f), new Vector3(0.25f, 6f, 0.25f), steel);
            AddCube(root, "Derrick Leg D", new Vector3(1.1f, 13f + yOffset, 1.1f), new Vector3(0.25f, 6f, 0.25f), steel);
            AddCube(root, "Crown Block", new Vector3(0f, 16.3f + yOffset, 0f), new Vector3(3f, 0.5f, 2f), accent);
            AddCube(root, "Top Drive", new Vector3(0f, 10.2f + yOffset, 0f), new Vector3(1.4f, 1.6f, 1.4f), accent);
            AddCube(root, "Standpipe", new Vector3(3.8f, 6f + yOffset, 0f), new Vector3(0.35f, 9f, 0.35f), accent);
        }

        private static GameObject AddCube(
            Transform parent,
            string objectName,
            Vector3 position,
            Vector3 scale,
            Material material)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = objectName;
            cube.transform.SetParent(parent, false);
            cube.transform.localPosition = position;
            cube.transform.localScale = scale;

            Renderer renderer = cube.GetComponent<Renderer>();
            renderer.sharedMaterial = material;
            return cube;
        }

        private static void AddSurveyGrid(Transform root, float size, float spacing, Material material)
        {
            float half = size * 0.5f;
            for (float offset = -half; offset <= half + 0.01f; offset += spacing)
            {
                AddCube(root, "Grid X", new Vector3(0f, 0.02f, offset), new Vector3(size, 0.035f, 0.045f), material);
                AddCube(root, "Grid Z", new Vector3(offset, 0.02f, 0f), new Vector3(0.045f, 0.035f, size), material);
            }
        }

        private static void AddSimpleTree(Transform root, Vector3 position)
        {
            Material trunk = MakeMaterial("Tree Trunk", new Color(0.28f, 0.13f, 0.07f));
            Material crown = MakeMaterial("Tree Crown", new Color(0.08f, 0.28f, 0.11f));

            AddCube(root, "Tree Trunk", position + new Vector3(0f, 0.6f, 0f), new Vector3(0.35f, 1.2f, 0.35f), trunk);
            AddCube(root, "Tree Crown", position + new Vector3(0f, 1.8f, 0f), new Vector3(1.8f, 1.8f, 1.8f), crown);
        }

        private static void AddSceneTree(Transform root, Vector3 position, float targetHeightMeters, bool useExternalModel)
        {
            GameObject treePrefab = Resources.Load<GameObject>(TreeResourcePath);
            if (!useExternalModel || treePrefab == null)
            {
                AddSimpleTree(root, position);
                return;
            }

            GameObject tree = UnityEngine.Object.Instantiate(treePrefab, root);
            tree.name = "External Environment Tree";
            tree.transform.localPosition = position;
            tree.transform.localRotation = Quaternion.Euler(0f, Mathf.Repeat(position.x * 17.3f + position.z * 9.1f, 360f), 0f);
            tree.transform.localScale = Vector3.one;
            NormalizeExternalModel(tree, position, targetHeightMeters);
            OptimizeRenderers(tree);
        }

        private static void NormalizeExternalModel(GameObject instance, Vector3 groundPosition, float targetHeightMeters)
        {
            if (!TryCalculateBounds(instance, out Bounds bounds))
            {
                return;
            }

            float height = Mathf.Max(0.01f, bounds.size.y);
            instance.transform.localScale *= targetHeightMeters / height;

            if (!TryCalculateBounds(instance, out bounds))
            {
                return;
            }

            Vector3 offset = new Vector3(
                groundPosition.x - bounds.center.x,
                groundPosition.y - bounds.min.y,
                groundPosition.z - bounds.center.z);
            instance.transform.position += offset;
        }

        private static bool TryCalculateBounds(GameObject root, out Bounds bounds)
        {
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
            {
                bounds = default;
                return false;
            }

            bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            return true;
        }

        private static void OptimizeRenderers(GameObject root)
        {
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>();
            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].shadowCastingMode = ShadowCastingMode.Off;
                renderers[i].receiveShadows = false;
                renderers[i].allowOcclusionWhenDynamic = true;
            }
        }

        private static Material MakeMaterial(string materialName, Color color)
        {
            Material source = Resources.Load<Material>("SCADASim/RuntimeLit");
            Material material = source != null ? new Material(source) : null;

            if (material == null)
            {
                Shader shader = Shader.Find("Standard");
                material = shader != null ? new Material(shader) : CreateMaterialFromPrimitive();
            }

            if (material == null)
            {
                return null;
            }

            material.name = materialName;
            material.color = color;
            return material;
        }

        private static Material CreateMaterialFromPrimitive()
        {
            GameObject probe = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Renderer probeRenderer = probe.GetComponent<Renderer>();
            Material material = probeRenderer != null && probeRenderer.sharedMaterial != null
                ? new Material(probeRenderer.sharedMaterial)
                : null;

            if (Application.isPlaying)
            {
                Destroy(probe);
            }
            else
            {
                DestroyImmediate(probe);
            }

            if (material == null)
            {
                return null;
            }

            return material;
        }
    }
}
