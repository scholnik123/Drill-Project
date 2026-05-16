using System.Collections.Generic;
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
            Material ground = MakeMaterial("SCADA Field Ground", new Color(0.08f, 0.24f, 0.09f));
            Material grid = MakeMaterial("SCADA Field Grid", new Color(0.68f, 0.82f, 0.68f, 0.62f));
            Material road = MakeMaterial("SCADA Access Road", new Color(0.08f, 0.08f, 0.08f));
            Material steel = MakeMaterial("SCADA Steel", new Color(0.36f, 0.38f, 0.38f));
            Material grass = CreateGrassBladeMaterial();

            AddCube(root, "Survey Field", new Vector3(0f, -0.08f, 0f), new Vector3(140f, 0.08f, 140f), ground);
            AddGrassField(root, 140f, 5200, grass);
            AddDewHighlights(root, 132f, 180, MakeMaterial("Morning Dew Highlights", new Color(0.86f, 0.98f, 1f, 0.62f)));
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
            Material sea = CreateOceanMaterial("SCADA Calm Crystal Ocean", new Color(0.02f, 0.25f, 0.34f, 0.78f));
            Material seaLine = MakeMaterial("SCADA Sea Grid", new Color(0.68f, 0.93f, 1f, 0.36f));

            AddCube(root, "Sea Plane", new Vector3(0f, -1.16f, 0f), new Vector3(92f, 0.08f, 92f), sea);
            AddWaveSurface(root, CreateOceanMaterial("SCADA Gentle Sunset Waves", new Color(0.04f, 0.31f, 0.42f, 0.72f)), 96f, 58, 0.075f, 0.42f);
            AddSunsetReflection(root);
            AddWaterCaustics(root);
            AddSurveyGrid(root, 86f, 12f, seaLine, -0.9f);
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

        private static void AddSurveyGrid(Transform root, float size, float spacing, Material material, float yPosition = 0.02f)
        {
            float half = size * 0.5f;
            for (float offset = -half; offset <= half + 0.01f; offset += spacing)
            {
                AddCube(root, "Grid X", new Vector3(0f, yPosition, offset), new Vector3(size, 0.035f, 0.045f), material);
                AddCube(root, "Grid Z", new Vector3(offset, yPosition, 0f), new Vector3(0.045f, 0.035f, size), material);
            }
        }

        private static void AddGrassField(Transform root, float size, int bladeCount, Material material)
        {
            if (material == null)
            {
                return;
            }

            material.SetInt("_Cull", (int)CullMode.Off);
            GameObject grassObject = new GameObject("Procedural Field Grass");
            grassObject.transform.SetParent(root, false);

            List<Vector3> vertices = new List<Vector3>(bladeCount * 8);
            List<int> triangles = new List<int>(bladeCount * 12);
            List<Vector2> uvs = new List<Vector2>(bladeCount * 8);
            float half = size * 0.5f;

            for (int i = 0; i < bladeCount; i++)
            {
                float x = Mathf.Lerp(-half, half, Hash01(i, 0.11f));
                float z = Mathf.Lerp(-half, half, Hash01(i, 0.37f));
                if (IsClearOperationalPad(x, z))
                {
                    continue;
                }

                float height = Mathf.Lerp(0.55f, 1.55f, Hash01(i, 0.59f));
                float width = Mathf.Lerp(0.045f, 0.13f, Hash01(i, 0.73f));
                float angle = Hash01(i, 0.91f) * Mathf.PI * 2f;
                Vector3 side = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * width;
                Vector3 crossSide = new Vector3(Mathf.Cos(angle + Mathf.PI * 0.5f), 0f, Mathf.Sin(angle + Mathf.PI * 0.5f)) * width * 0.82f;
                Vector3 lean = new Vector3(Mathf.Cos(angle + 1.3f), 0f, Mathf.Sin(angle + 1.3f)) * height * 0.18f;
                lean += Vector3.right * Mathf.Sin(i * 0.37f) * 0.035f;
                Vector3 basePosition = new Vector3(x, 0.025f, z);
                Vector3 topPosition = basePosition + Vector3.up * height + lean;
                int start = vertices.Count;

                vertices.Add(basePosition - side);
                vertices.Add(basePosition + side);
                vertices.Add(topPosition + side * 0.25f);
                vertices.Add(topPosition - side * 0.25f);
                triangles.Add(start);
                triangles.Add(start + 1);
                triangles.Add(start + 2);
                triangles.Add(start);
                triangles.Add(start + 2);
                triangles.Add(start + 3);
                uvs.Add(new Vector2(0f, 0f));
                uvs.Add(new Vector2(1f, 0f));
                uvs.Add(new Vector2(1f, 1f));
                uvs.Add(new Vector2(0f, 1f));

                int crossStart = vertices.Count;
                vertices.Add(basePosition - crossSide);
                vertices.Add(basePosition + crossSide);
                vertices.Add(topPosition + crossSide * 0.2f);
                vertices.Add(topPosition - crossSide * 0.2f);
                triangles.Add(crossStart);
                triangles.Add(crossStart + 1);
                triangles.Add(crossStart + 2);
                triangles.Add(crossStart);
                triangles.Add(crossStart + 2);
                triangles.Add(crossStart + 3);
                uvs.Add(new Vector2(0f, 0f));
                uvs.Add(new Vector2(1f, 0f));
                uvs.Add(new Vector2(1f, 1f));
                uvs.Add(new Vector2(0f, 1f));
            }

            Mesh mesh = new Mesh { name = "Procedural Grass Mesh" };
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.SetUVs(0, uvs);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            MeshFilter filter = grassObject.AddComponent<MeshFilter>();
            filter.sharedMesh = mesh;

            MeshRenderer renderer = grassObject.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }

        private static void AddDewHighlights(Transform root, float size, int count, Material material)
        {
            if (material == null)
            {
                return;
            }

            float half = size * 0.5f;
            for (int i = 0; i < count; i++)
            {
                float x = Mathf.Lerp(-half, half, Hash01(i, 0.21f));
                float z = Mathf.Lerp(-half, half, Hash01(i, 0.43f));
                if (IsClearOperationalPad(x, z))
                {
                    continue;
                }

                GameObject dew = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                dew.name = "Morning Dew Droplet";
                dew.transform.SetParent(root, false);
                dew.transform.localPosition = new Vector3(x, Mathf.Lerp(0.08f, 0.36f, Hash01(i, 0.67f)), z);
                float scale = Mathf.Lerp(0.045f, 0.11f, Hash01(i, 0.89f));
                dew.transform.localScale = Vector3.one * scale;
                Renderer renderer = dew.GetComponent<Renderer>();
                renderer.sharedMaterial = material;
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderer.receiveShadows = false;
            }
        }

        private static bool IsClearOperationalPad(float x, float z)
        {
            if (Mathf.Abs(x) < 18f && Mathf.Abs(z) < 14f)
            {
                return true;
            }

            return Mathf.Abs(z + 2f - x * 0.47f) < 3.8f;
        }

        private static void AddWaveSurface(
            Transform root,
            Material material,
            float surfaceSize = 82f,
            int resolution = 44,
            float amplitude = 0.22f,
            float speed = 1.05f)
        {
            if (material == null)
            {
                return;
            }

            GameObject waves = new GameObject("Animated Water Waves");
            waves.transform.SetParent(root, false);
            waves.transform.localPosition = new Vector3(0f, -1.02f, 0f);

            MeshRenderer renderer = waves.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;

            waves.AddComponent<MeshFilter>();
            waves.AddComponent<ProceduralWaveSurface>().Configure(surfaceSize, resolution, amplitude, speed);
        }

        private static void AddSunsetReflection(Transform root)
        {
            Material reflection = MakeMaterial("Golden Hour Water Reflection", new Color(1f, 0.58f, 0.18f, 0.34f));
            for (int i = 0; i < 8; i++)
            {
                float width = Mathf.Lerp(7f, 24f, Hash01(i, 0.18f));
                float z = Mathf.Lerp(-28f, 30f, i / 7f);
                GameObject streak = AddCube(root, "Golden Reflection Streak", new Vector3(16f, -0.86f, z), new Vector3(width, 0.018f, 0.32f), reflection);
                streak.transform.localRotation = Quaternion.Euler(0f, -12f + i * 2.6f, 0f);
            }
        }

        private static void AddWaterCaustics(Transform root)
        {
            Material caustic = MakeMaterial("Soft Seabed Caustics", new Color(0.72f, 0.95f, 1f, 0.22f));
            for (int i = 0; i < 24; i++)
            {
                float x = Mathf.Lerp(-38f, 38f, Hash01(i, 0.31f));
                float z = Mathf.Lerp(-38f, 38f, Hash01(i, 0.53f));
                float length = Mathf.Lerp(7f, 18f, Hash01(i, 0.75f));
                GameObject line = AddCube(root, "Volumetric Water Caustic", new Vector3(x, -1.02f, z), new Vector3(length, 0.012f, 0.075f), caustic);
                line.transform.localRotation = Quaternion.Euler(0f, Hash01(i, 0.97f) * 180f, 0f);
            }
        }

        private static float Hash01(int index, float salt)
        {
            return Mathf.Repeat(Mathf.Sin(index * 12.9898f + salt * 78.233f) * 43758.5453f, 1f);
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
            if (color.a < 0.99f)
            {
                material.SetFloat("_Mode", 3f);
                material.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
                material.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
                material.SetInt("_ZWrite", 0);
                material.DisableKeyword("_ALPHATEST_ON");
                material.EnableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = 3000;
            }

            return material;
        }

        private static Material CreateGrassBladeMaterial()
        {
            Material material = MakeMaterial("Dense Hyper Realistic Grass Blades", new Color(0.08f, 0.55f, 0.11f));
            if (material == null)
            {
                return null;
            }

            material.mainTexture = CreateGrassTexture();
            material.SetFloat("_Glossiness", 0.38f);
            material.SetColor("_SpecColor", new Color(0.45f, 0.74f, 0.46f, 1f));
            return material;
        }

        private static Material CreateOceanMaterial(string name, Color color)
        {
            Material material = MakeMaterial(name, color);
            if (material == null)
            {
                return null;
            }

            material.mainTexture = CreateWaterTexture();
            material.SetFloat("_Glossiness", 0.82f);
            material.SetColor("_SpecColor", new Color(0.95f, 0.78f, 0.45f, 1f));
            return material;
        }

        private static Texture2D CreateGrassTexture()
        {
            const int size = 128;
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, true)
            {
                name = "Procedural Dense Grass Texture",
                wrapMode = TextureWrapMode.Repeat,
                filterMode = FilterMode.Trilinear
            };

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float blade = Mathf.Abs(Mathf.Sin(x * 0.42f + Mathf.Sin(y * 0.07f) * 2.4f));
                    float noise = Mathf.PerlinNoise(x * 0.08f, y * 0.08f);
                    float dew = noise > 0.82f && y > size * 0.42f ? 0.28f : 0f;
                    Color color = Color.Lerp(
                        new Color(0.035f, 0.22f, 0.045f),
                        new Color(0.22f, 0.82f, 0.17f),
                        Mathf.Clamp01(blade * 0.65f + noise * 0.45f));
                    color = Color.Lerp(color, new Color(0.82f, 1f, 0.88f), dew);
                    texture.SetPixel(x, y, color);
                }
            }

            texture.Apply(true, false);
            return texture;
        }

        private static Texture2D CreateWaterTexture()
        {
            const int size = 128;
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, true)
            {
                name = "Procedural Crystal Ocean Texture",
                wrapMode = TextureWrapMode.Repeat,
                filterMode = FilterMode.Trilinear
            };

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float ripple = Mathf.Sin(x * 0.17f + y * 0.04f) * 0.5f + Mathf.Sin(y * 0.21f) * 0.28f;
                    float caustic = Mathf.Pow(Mathf.Clamp01(Mathf.Sin((x + y) * 0.16f) * 0.5f + 0.5f), 5f);
                    float sunset = Mathf.Clamp01(1f - Mathf.Abs(y - size * 0.55f) / 28f) * 0.22f;
                    Color water = Color.Lerp(new Color(0.01f, 0.18f, 0.26f, 0.74f), new Color(0.1f, 0.5f, 0.62f, 0.74f), ripple * 0.35f + 0.35f);
                    water = Color.Lerp(water, new Color(1f, 0.62f, 0.22f, 0.78f), sunset);
                    water = Color.Lerp(water, new Color(0.78f, 0.96f, 1f, 0.82f), caustic * 0.18f);
                    texture.SetPixel(x, y, water);
                }
            }

            texture.Apply(true, false);
            return texture;
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

    public sealed class ProceduralWaveSurface : MonoBehaviour
    {
        private Mesh mesh;
        private Vector3[] vertices;
        private int resolution = 36;
        private float size = 70f;
        private float amplitude = 0.18f;
        private float speed = 1f;

        public void Configure(float surfaceSize, int gridResolution, float waveAmplitude, float waveSpeed)
        {
            size = Mathf.Max(8f, surfaceSize);
            resolution = Mathf.Clamp(gridResolution, 8, 80);
            amplitude = Mathf.Max(0.01f, waveAmplitude);
            speed = Mathf.Max(0.1f, waveSpeed);
            BuildMesh();
        }

        private void Awake()
        {
            if (mesh == null)
            {
                BuildMesh();
            }
        }

        private void Update()
        {
            if (mesh == null || vertices == null)
            {
                return;
            }

            float t = Time.time * speed;
            int width = resolution + 1;
            for (int z = 0; z <= resolution; z++)
            {
                for (int x = 0; x <= resolution; x++)
                {
                    int index = z * width + x;
                    Vector3 vertex = vertices[index];
                    float waveA = Mathf.Sin(vertex.x * 0.34f + vertex.z * 0.18f + t);
                    float waveB = Mathf.Sin(vertex.x * -0.16f + vertex.z * 0.41f + t * 1.45f);
                    vertex.y = (waveA + waveB * 0.55f) * amplitude;
                    vertices[index] = vertex;
                }
            }

            mesh.vertices = vertices;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
        }

        private void BuildMesh()
        {
            MeshFilter filter = GetComponent<MeshFilter>();
            if (filter == null)
            {
                filter = gameObject.AddComponent<MeshFilter>();
            }

            mesh = new Mesh { name = "Animated Water Wave Mesh" };
            mesh.MarkDynamic();

            int width = resolution + 1;
            vertices = new Vector3[width * width];
            Vector2[] uvs = new Vector2[vertices.Length];
            int[] triangles = new int[resolution * resolution * 6];
            float half = size * 0.5f;

            for (int z = 0; z <= resolution; z++)
            {
                for (int x = 0; x <= resolution; x++)
                {
                    int index = z * width + x;
                    float px = Mathf.Lerp(-half, half, x / (float)resolution);
                    float pz = Mathf.Lerp(-half, half, z / (float)resolution);
                    vertices[index] = new Vector3(px, 0f, pz);
                    uvs[index] = new Vector2(x / (float)resolution, z / (float)resolution);
                }
            }

            int triangleIndex = 0;
            for (int z = 0; z < resolution; z++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    int start = z * width + x;
                    triangles[triangleIndex++] = start;
                    triangles[triangleIndex++] = start + width;
                    triangles[triangleIndex++] = start + 1;
                    triangles[triangleIndex++] = start + 1;
                    triangles[triangleIndex++] = start + width;
                    triangles[triangleIndex++] = start + width + 1;
                }
            }

            mesh.vertices = vertices;
            mesh.uv = uvs;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            filter.sharedMesh = mesh;
        }
    }
}
