using SCADASim.AI;
using SCADASim.Core;
using SCADASim.Crew;
using SCADASim.Environment;
using SCADASim.Physics;
using SCADASim.Trajectory;
using SCADASim.UI;
using SCADASim.Audio;
using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace SCADASim.Bootstrap
{
    public sealed class SCADASimBootstrap : MonoBehaviour
    {
        private static bool created;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoCreate()
        {
            if (created || UnityEngine.Object.FindAnyObjectByType<SCADASimBootstrap>() != null)
            {
                return;
            }

            GameObject bootstrap = new GameObject("SCADA Simulation Bootstrap");
            bootstrap.AddComponent<SCADASimBootstrap>();
            created = true;
        }

        private void Awake()
        {
            BuildRuntimeProject();
        }

        private void BuildRuntimeProject()
        {
            Application.targetFrameRate = 60;
            QualitySettings.antiAliasing = 2;
            QualitySettings.shadowDistance = 38f;
            QualitySettings.lodBias = 0.85f;
            QualitySettings.vSyncCount = 1;

            SimulationEventChannel eventChannel = ScriptableObject.CreateInstance<SimulationEventChannel>();
            DrillingPhysicsConfig physicsConfig = ScriptableObject.CreateInstance<DrillingPhysicsConfig>();
            GeologyModel geologyModel = GeologyModel.CreateRuntimeDemo();

            Camera camera = EnsureCamera();
            EnsureLighting();

            GameObject environmentRoot = new GameObject("Environment Root");
            EnvironmentManager environmentManager = environmentRoot.AddComponent<EnvironmentManager>();
            environmentManager.Configure(eventChannel, environmentRoot.transform);

            GameObject rigObject = new GameObject("Rig Model Manager");
            RigModelManager rigModelManager = rigObject.AddComponent<RigModelManager>();
            rigModelManager.Configure(eventChannel);

            GameObject wellboreObject = new GameObject("Procedural Wellbore");
            wellboreObject.transform.position = Vector3.zero;
            wellboreObject.transform.localScale = Vector3.one * 0.035f;
            WellboreProceduralMesh wellbore = wellboreObject.AddComponent<WellboreProceduralMesh>();
            wellbore.ConfigureRuntime(
                WellboreProfileType.Horizontal,
                820f,
                35f,
                45f,
                4.0f,
                32,
                10,
                eventChannel);
            wellbore.Regenerate();
            wellbore.SetVisibleMeasuredDepth(0f);
            wellboreObject.GetComponent<MeshRenderer>().sharedMaterial = MakeMaterial(
                "Casing Steel",
                new Color(0.75f, 0.78f, 0.76f, 1f));

            GameObject crewObject = new GameObject("Crew Manager");
            CrewManager crewManager = crewObject.AddComponent<CrewManager>();
            crewManager.Configure(eventChannel);

            GameObject drillingObject = new GameObject("Drilling Model");
            DrillingModel drillingModel = drillingObject.AddComponent<DrillingModel>();
            drillingModel.Configure(wellbore, geologyModel, physicsConfig, crewManager, eventChannel);

            GameObject visualizerObject = new GameObject("Wellbore 3D Visualizer");
            Wellbore3DVisualizer visualizer = visualizerObject.AddComponent<Wellbore3DVisualizer>();
            visualizer.Configure(wellbore, drillingModel, eventChannel);

            GameObject aiObject = new GameObject("Edge AI Module");
            EdgeAIModule edgeAI = aiObject.AddComponent<EdgeAIModule>();
            edgeAI.Configure(drillingModel, crewManager, eventChannel);

            GameObject crewAIObject = new GameObject("AI Crew Advisor Module");
            AICrewAdvisorModule crewAI = crewAIObject.AddComponent<AICrewAdvisorModule>();
            crewAI.Configure(drillingModel, crewManager, eventChannel);

            GameObject musicObject = new GameObject("SCADA Music Player");
            ScadaMusicPlayer musicPlayer = musicObject.AddComponent<ScadaMusicPlayer>();

            SCADASimCameraOrbit cameraOrbit = camera.gameObject.AddComponent<SCADASimCameraOrbit>();
            cameraOrbit.Configure(new Vector3(0f, 5.5f, 0f));
            cameraOrbit.SetView(140f, 28f, 34f);

            GameObject uiObject = new GameObject("SCADA Dashboard UI");
            ScadaDashboardUI ui = uiObject.AddComponent<ScadaDashboardUI>();
            ui.Configure(
                drillingModel,
                crewManager,
                environmentManager,
                rigModelManager,
                wellbore,
                cameraOrbit,
                null,
                musicPlayer,
                eventChannel);

            drillingModel.SetSimulating(false);
            Debug.Log("SCADA bootstrap ready: start UI is visible, 3D assets load after configuration.");
            StartCoroutine(CreateAssistantPortraitAfterFirstFrame(ui));
            if (HasCommandLineArgument("--capture-profile"))
            {
                StartCoroutine(CaptureProfileFrame(ui));
            }
            else if (HasCommandLineArgument("--capture-dashboard"))
            {
                StartCoroutine(CaptureDashboardFrame(ui));
            }
            else if (HasCommandLineArgument("--capture-startup"))
            {
                StartCoroutine(CaptureStartupFrame());
            }
        }

        private IEnumerator CreateAssistantPortraitAfterFirstFrame(ScadaDashboardUI ui)
        {
            yield return null;

            GameObject assistantPortraitObject = new GameObject("AI Assistant Portrait");
            AIAssistantPortrait assistantPortrait = assistantPortraitObject.AddComponent<AIAssistantPortrait>();
            ui.SetAssistantPortrait(assistantPortrait);
        }

        private IEnumerator CaptureStartupFrame()
        {
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            string path = Path.Combine(Application.persistentDataPath, "startup_probe.png");
            ScreenCapture.CaptureScreenshot(path);
            Debug.Log($"SCADA startup capture written: {path}");
        }

        private IEnumerator CaptureDashboardFrame(ScadaDashboardUI ui)
        {
            yield return null;

            ui.DebugStartDefaultSessionForCapture();

            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            string path = Path.Combine(Application.persistentDataPath, "dashboard_probe.png");
            ScreenCapture.CaptureScreenshot(path);
            Debug.Log($"SCADA dashboard capture written: {path}");

            if (HasCommandLineArgument("--quit-after-capture"))
            {
                yield return new WaitForSecondsRealtime(0.5f);
                Application.Quit();
            }
        }

        private IEnumerator CaptureProfileFrame(ScadaDashboardUI ui)
        {
            yield return null;

            ui.DebugStartProfileSessionForCapture();

            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            string path = Path.Combine(Application.persistentDataPath, "profile_probe.png");
            ScreenCapture.CaptureScreenshot(path);
            Debug.Log($"SCADA profile capture written: {path}");

            if (HasCommandLineArgument("--quit-after-capture"))
            {
                yield return new WaitForSecondsRealtime(0.5f);
                Application.Quit();
            }
        }

        private static bool HasCommandLineArgument(string argument)
        {
            string[] args = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                if (string.Equals(args[i], argument, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static Camera EnsureCamera()
        {
            Camera camera = Camera.main;
            if (camera == null)
            {
                GameObject cameraObject = new GameObject("Main Camera");
                camera = cameraObject.AddComponent<Camera>();
                camera.tag = "MainCamera";
            }

            camera.transform.position = new Vector3(18f, 16f, -24f);
            camera.transform.rotation = Quaternion.LookRotation(new Vector3(0f, 4f, 0f) - camera.transform.position);
            camera.fieldOfView = 48f;
            camera.enabled = true;
            camera.depth = -10f;
            camera.cullingMask = ~0;
            camera.targetTexture = null;
            camera.rect = new Rect(0f, 0f, 1f, 1f);
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.91f, 0.90f, 0.86f);
            return camera;
        }

        private static void EnsureLighting()
        {
            if (UnityEngine.Object.FindAnyObjectByType<Light>() != null)
            {
                return;
            }

            GameObject keyLight = new GameObject("Key Light");
            Light light = keyLight.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.15f;
            light.shadows = LightShadows.Soft;
            keyLight.transform.rotation = Quaternion.Euler(52f, -35f, 0f);

            RenderSettings.ambientLight = new Color(0.28f, 0.3f, 0.31f);
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.72f, 0.75f, 0.74f);
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = 0.0045f;
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
