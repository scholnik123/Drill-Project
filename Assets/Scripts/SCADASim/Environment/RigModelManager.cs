using SCADASim.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace SCADASim.Environment
{
    public sealed class RigModelManager : MonoBehaviour
    {
        private const string OnshoreRigResourcePath = "Models/modelToUsed";
        private static readonly string[] OffshorePlatformResourcePaths =
        {
            "Models/modelToUsedOffshore",
            "Models/offshorePlatform",
            "Models/oilPlatform"
        };

        [SerializeField] private SimulationEventChannel eventChannel;
        [SerializeField] private float targetModelHeightMeters = 18f;
        [SerializeField] private float offshoreRigHeightMeters = 13f;

        private GameObject currentRig;

        public bool HasExternalModel { get; private set; }
        public string CurrentModelStatus { get; private set; } = "3D-МОДЕЛЬ: ПРОЦЕДУРНАЯ";

        public void Configure(SimulationEventChannel channel)
        {
            eventChannel = channel;
        }

        public void LoadRigModel(EnvironmentType environmentType)
        {
            if (currentRig != null)
            {
                Destroy(currentRig);
            }

            if (environmentType == EnvironmentType.Offshore)
            {
                LoadOffshoreRig();
            }
            else
            {
                LoadOnshoreRig();
            }

            eventChannel?.Raise(
                SimulationEventType.EnvironmentLoaded,
                AlertSeverity.Info,
                CurrentModelStatus,
                this);
        }

        public Vector3 GetWellheadWorldPosition(EnvironmentType environmentType)
        {
            return new Vector3(0f, GetDeckY(environmentType) + 0.05f, 0f);
        }

        private void LoadOnshoreRig()
        {
            GameObject rigPrefab = Resources.Load<GameObject>(OnshoreRigResourcePath);
            HasExternalModel = rigPrefab != null;

            if (HasExternalModel)
            {
                currentRig = Instantiate(rigPrefab, transform);
                currentRig.name = "modelToUsed - Onshore Rig";
                NormalizeImportedModel(currentRig, EnvironmentType.Onshore, targetModelHeightMeters);
                OptimizeRenderers(currentRig);
                CurrentModelStatus = "3D-МОДЕЛЬ: modelToUsed.fbx";
                return;
            }

            currentRig = BuildFallbackRig(EnvironmentType.Onshore);
            CurrentModelStatus = "3D-МОДЕЛЬ: ПРОЦЕДУРНАЯ НАЗЕМНАЯ ВЫШКА";
        }

        private void LoadOffshoreRig()
        {
            GameObject platformPrefab = LoadFirstAvailable(OffshorePlatformResourcePaths);
            GameObject rigPrefab = Resources.Load<GameObject>(OnshoreRigResourcePath);

            if (platformPrefab != null)
            {
                currentRig = Instantiate(platformPrefab, transform);
                currentRig.name = "External Offshore Platform";
                NormalizeImportedModel(currentRig, EnvironmentType.Offshore, 20f);
                OptimizeRenderers(currentRig);
                HasExternalModel = true;
                CurrentModelStatus = "3D-МОДЕЛЬ: МОРСКАЯ ПЛАТФОРМА FBX";
                return;
            }

            currentRig = BuildOffshorePlatform();

            if (rigPrefab != null)
            {
                GameObject rig = Instantiate(rigPrefab, currentRig.transform);
                rig.name = "modelToUsed - Rig On Offshore Deck";
                NormalizeImportedModel(rig, EnvironmentType.Offshore, offshoreRigHeightMeters);
                OptimizeRenderers(rig);
                HasExternalModel = true;
                CurrentModelStatus = "3D-МОДЕЛЬ: МОРСКАЯ ПЛАТФОРМА + modelToUsed.fbx";
            }
            else
            {
                HasExternalModel = false;
                CurrentModelStatus = "3D-МОДЕЛЬ: ПРОЦЕДУРНАЯ МОРСКАЯ ПЛАТФОРМА";
            }
        }

        private static GameObject LoadFirstAvailable(string[] resourcePaths)
        {
            for (int i = 0; i < resourcePaths.Length; i++)
            {
                GameObject prefab = Resources.Load<GameObject>(resourcePaths[i]);
                if (prefab != null)
                {
                    return prefab;
                }
            }

            return null;
        }

        private void NormalizeImportedModel(GameObject rig, EnvironmentType environmentType, float targetHeightMeters)
        {
            rig.transform.localPosition = Vector3.zero;
            rig.transform.localRotation = Quaternion.identity;
            rig.transform.localScale = Vector3.one;

            if (!TryCalculateBounds(rig, out Bounds bounds))
            {
                rig.transform.localPosition = new Vector3(0f, GetDeckY(environmentType), 0f);
                return;
            }

            float height = Mathf.Max(0.01f, bounds.size.y);
            float scale = targetHeightMeters / height;
            rig.transform.localScale = Vector3.one * scale;

            if (!TryCalculateBounds(rig, out bounds))
            {
                return;
            }

            Vector3 offset = new Vector3(-bounds.center.x, GetDeckY(environmentType) - bounds.min.y, -bounds.center.z);
            rig.transform.position += offset;
        }

        private GameObject BuildFallbackRig(EnvironmentType environmentType)
        {
            GameObject root = new GameObject("Procedural Fallback Rig");
            root.transform.SetParent(transform, false);

            float deckY = GetDeckY(environmentType);
            Material steel = MakeMaterial("Rig Dark Steel", new Color(0.05f, 0.055f, 0.055f));
            Material lightSteel = MakeMaterial("Rig Light Steel", new Color(0.72f, 0.72f, 0.68f));
            Material red = MakeMaterial("Rig Red Accent", new Color(0.85f, 0.02f, 0.08f));

            AddCube(root.transform, "Substructure", new Vector3(0f, deckY + 0.45f, 0f), new Vector3(7.5f, 0.9f, 7.5f), lightSteel);
            AddCube(root.transform, "Rotary Table", new Vector3(0f, deckY + 1.05f, 0f), new Vector3(1.4f, 0.28f, 1.4f), red);
            AddCube(root.transform, "Pipe Rack", new Vector3(-9f, deckY + 0.3f, -5f), new Vector3(7f, 0.35f, 2.2f), steel);
            AddCube(root.transform, "Mud Module", new Vector3(8.6f, deckY + 0.55f, 4.2f), new Vector3(5f, 1.1f, 2.7f), lightSteel);

            Vector3 top = new Vector3(0f, deckY + 17f, 0f);
            Vector3 a = new Vector3(-3f, deckY + 1f, -3f);
            Vector3 b = new Vector3(3f, deckY + 1f, -3f);
            Vector3 c = new Vector3(-3f, deckY + 1f, 3f);
            Vector3 d = new Vector3(3f, deckY + 1f, 3f);

            AddBeam(root.transform, "Derrick Leg A", a, top + new Vector3(-0.8f, 0f, -0.8f), 0.18f, steel);
            AddBeam(root.transform, "Derrick Leg B", b, top + new Vector3(0.8f, 0f, -0.8f), 0.18f, steel);
            AddBeam(root.transform, "Derrick Leg C", c, top + new Vector3(-0.8f, 0f, 0.8f), 0.18f, steel);
            AddBeam(root.transform, "Derrick Leg D", d, top + new Vector3(0.8f, 0f, 0.8f), 0.18f, steel);

            AddBeam(root.transform, "Back Brace 1", a, d, 0.08f, steel);
            AddBeam(root.transform, "Back Brace 2", b, c, 0.08f, steel);
            AddBeam(root.transform, "Crown Front", top + new Vector3(-1.2f, 0f, -1.2f), top + new Vector3(1.2f, 0f, -1.2f), 0.24f, red);
            AddBeam(root.transform, "Crown Back", top + new Vector3(-1.2f, 0f, 1.2f), top + new Vector3(1.2f, 0f, 1.2f), 0.24f, red);
            AddCube(root.transform, "Top Drive", new Vector3(0f, deckY + 10f, 0f), new Vector3(1f, 1.7f, 1f), red);

            return root;
        }

        private GameObject BuildOffshorePlatform()
        {
            GameObject root = new GameObject("Procedural Offshore Platform");
            root.transform.SetParent(transform, false);

            Material darkSteel = MakeMaterial("Offshore Dark Steel", new Color(0.06f, 0.07f, 0.075f));
            Material deck = MakeMaterial("Offshore Deck", new Color(0.18f, 0.19f, 0.19f));
            Material module = MakeMaterial("Offshore Module", new Color(0.58f, 0.60f, 0.58f));
            Material safety = MakeMaterial("Offshore Safety Red", new Color(0.85f, 0.02f, 0.08f));

            float y = GetDeckY(EnvironmentType.Offshore);
            AddCube(root.transform, "Main Platform Deck", new Vector3(0f, y, 0f), new Vector3(25f, 0.75f, 22f), deck);
            AddCube(root.transform, "Drill Floor", new Vector3(0f, y + 1.0f, 0f), new Vector3(8.5f, 1.1f, 8.5f), module);
            AddCube(root.transform, "Process Module A", new Vector3(8f, y + 1.1f, -5.2f), new Vector3(6f, 2.2f, 4.5f), module);
            AddCube(root.transform, "Process Module B", new Vector3(8f, y + 1.0f, 2.4f), new Vector3(6f, 2f, 4f), module);
            AddCube(root.transform, "Helideck", new Vector3(-13.5f, y + 0.55f, 0f), new Vector3(9f, 0.35f, 9f), deck);
            AddCube(root.transform, "Helideck Mark", new Vector3(-13.5f, y + 0.76f, 0f), new Vector3(6.3f, 0.06f, 0.28f), safety);
            AddCube(root.transform, "Helideck Mark Cross", new Vector3(-13.5f, y + 0.77f, 0f), new Vector3(0.28f, 0.06f, 6.3f), safety);

            AddCube(root.transform, "Platform Leg NW", new Vector3(-9.5f, y - 2.3f, 8f), new Vector3(1f, 4.6f, 1f), darkSteel);
            AddCube(root.transform, "Platform Leg NE", new Vector3(9.5f, y - 2.3f, 8f), new Vector3(1f, 4.6f, 1f), darkSteel);
            AddCube(root.transform, "Platform Leg SW", new Vector3(-9.5f, y - 2.3f, -8f), new Vector3(1f, 4.6f, 1f), darkSteel);
            AddCube(root.transform, "Platform Leg SE", new Vector3(9.5f, y - 2.3f, -8f), new Vector3(1f, 4.6f, 1f), darkSteel);

            AddBeam(root.transform, "Crane Boom", new Vector3(11f, y + 4.4f, 7f), new Vector3(3f, y + 7.5f, 2f), 0.2f, safety);
            AddBeam(root.transform, "Crane Mast", new Vector3(11f, y + 0.8f, 7f), new Vector3(11f, y + 4.6f, 7f), 0.3f, darkSteel);

            return root;
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

        private static float GetDeckY(EnvironmentType environmentType)
        {
            return environmentType == EnvironmentType.Offshore ? 2.5f : 0.02f;
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
            cube.GetComponent<Renderer>().sharedMaterial = material;
            return cube;
        }

        private static void AddBeam(
            Transform parent,
            string objectName,
            Vector3 from,
            Vector3 to,
            float thickness,
            Material material)
        {
            GameObject beam = GameObject.CreatePrimitive(PrimitiveType.Cube);
            beam.name = objectName;
            beam.transform.SetParent(parent, false);

            Vector3 direction = to - from;
            beam.transform.localPosition = (from + to) * 0.5f;
            beam.transform.localRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
            beam.transform.localScale = new Vector3(thickness, thickness, direction.magnitude);
            beam.GetComponent<Renderer>().sharedMaterial = material;
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

            return material;
        }
    }
}
