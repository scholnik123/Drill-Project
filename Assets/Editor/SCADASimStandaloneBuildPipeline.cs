#if UNITY_EDITOR
using System;
using System.IO;
using SCADASim.Bootstrap;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace SCADASim.EditorTools
{
    public static class SCADASimStandaloneBuildPipeline
    {
        private const string ScenePath = "Assets/Scenes/SCADASimMain.unity";
        private const string RuntimeMaterialPath = "Assets/Resources/SCADASim/RuntimeLit.mat";
        private const string RigModelAssetPath = "Assets/Resources/Models/modelToUsed.fbx";
        private const string OffshorePlatformAssetPath = "Assets/Resources/Models/offshorePlatform.fbx";
        private const string EnvironmentTreeAssetPath = "Assets/Resources/Models/environmentTree.fbx";
        private const string AIAssistantAssetPath = "Assets/Resources/Models/aiAssistant.fbx";
        private const string CompanyIntroVideoPath = "Assets/StreamingAssets/CompanyIntro.mp4";
        private const string MusicStreamingDirectory = "Assets/StreamingAssets/Music";
        private const string BuildDirectory = "Builds/SCADA_Intelligent_Simulation_v2";
        private const string WindowsBuildPath = BuildDirectory + "/SCADA_Intelligent_Simulation_v2.exe";

        [MenuItem("SCADA Simulation/Prepare Standalone Scene")]
        public static void PrepareStandaloneScene()
        {
            EnsureStandaloneScene();
            EnsureRuntimeMaterial();
            EnsureRigModelAsset();
            EnsureOptionalModelAssets();
            EnsureCompanyIntroVideo();
            EnsureAphexTwinMusic();
            ConfigurePlayerSettings();
            AssetDatabase.SaveAssets();
        }

        [MenuItem("SCADA Simulation/Build Windows Standalone")]
        public static void BuildWindowsStandalone()
        {
            EnsureStandaloneScene();
            EnsureRuntimeMaterial();
            EnsureRigModelAsset();
            EnsureOptionalModelAssets();
            EnsureCompanyIntroVideo();
            EnsureAphexTwinMusic();
            ConfigurePlayerSettings();
            Directory.CreateDirectory(BuildDirectory);

            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = WindowsBuildPath,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            BuildSummary summary = report.summary;

            if (summary.result != BuildResult.Succeeded)
            {
                throw new Exception($"Standalone build failed: {summary.result}. See Unity Editor log for details.");
            }

            Debug.Log($"Standalone build completed: {Path.GetFullPath(WindowsBuildPath)}");
        }

        private static void EnsureStandaloneScene()
        {
            Directory.CreateDirectory("Assets/Scenes");

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObject bootstrap = new GameObject("SCADA Simulation Bootstrap");
            bootstrap.AddComponent<SCADASimBootstrap>();

            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(ScenePath, true)
            };

            AssetDatabase.Refresh();
        }

        private static void ConfigurePlayerSettings()
        {
            PlayerSettings.companyName = "SCADA Simulation Lab";
            PlayerSettings.productName = "SCADA Intelligent Simulation v2.0";
            PlayerSettings.applicationIdentifier = "com.scadasim.intelligent.v2";
            PlayerSettings.runInBackground = true;
            PlayerSettings.defaultScreenWidth = 1600;
            PlayerSettings.defaultScreenHeight = 900;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            PlayerSettings.resizableWindow = true;
            PlayerSettings.visibleInBackground = true;
            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.StandaloneWindows64, false);
            PlayerSettings.SetGraphicsAPIs(
                BuildTarget.StandaloneWindows64,
                new[] { GraphicsDeviceType.Direct3D11 });
        }

        private static void EnsureRuntimeMaterial()
        {
            Directory.CreateDirectory("Assets/Resources/SCADASim");

            Shader shader = Shader.Find("Standard");
            if (shader == null)
            {
                shader = Shader.Find("Universal Render Pipeline/Lit");
            }

            if (shader == null)
            {
                shader = Shader.Find("Unlit/Color");
            }

            if (shader == null)
            {
                throw new InvalidOperationException("No supported runtime material shader found.");
            }

            Material material = AssetDatabase.LoadAssetAtPath<Material>(RuntimeMaterialPath);
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, RuntimeMaterialPath);
            }
            else
            {
                material.shader = shader;
            }

            material.name = "RuntimeLit";
            material.color = Color.white;
            EditorUtility.SetDirty(material);
            AssetDatabase.SaveAssets();
        }

        private static void EnsureRigModelAsset()
        {
            Directory.CreateDirectory("Assets/Resources/Models");

            if (!File.Exists(RigModelAssetPath))
            {
                string projectRoot = Directory.GetCurrentDirectory();
                string downloadsPath = Path.Combine(
                    System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile),
                    "Downloads",
                    "modelToUsed.fbx");

                string[] candidates =
                {
                    Path.Combine(projectRoot, "modelToUsed.fbx"),
                    Path.Combine(projectRoot, "Assets", "modelToUsed.fbx"),
                    downloadsPath
                };

                foreach (string candidate in candidates)
                {
                    if (!File.Exists(candidate))
                    {
                        continue;
                    }

                    File.Copy(candidate, RigModelAssetPath, true);
                    Debug.Log($"Imported rig model for standalone build: {candidate}");
                    break;
                }
            }

            if (File.Exists(RigModelAssetPath))
            {
                AssetDatabase.ImportAsset(RigModelAssetPath, ImportAssetOptions.ForceUpdate);
            }
            else
            {
                Debug.LogWarning("modelToUsed.fbx was not found. The simulator will use the procedural fallback rig until the FBX is placed in Assets/Resources/Models.");
            }
        }

        private static void EnsureOptionalModelAssets()
        {
            Directory.CreateDirectory("Assets/Resources/Models");
            CopyOptionalModel(
                Path.Combine(
                    System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile),
                    "Downloads",
                    "e5f9e196ddeb1240de5387649ae9377d.fbx"),
                AIAssistantAssetPath);
            CopyOptionalModel(
                Path.Combine(
                    System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile),
                    "Downloads",
                    "dc90bebd444d2460a3d092a9d521c4f0.fbx"),
                EnvironmentTreeAssetPath);
            CopyOptionalModel(
                Path.Combine(
                    System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile),
                    "Downloads",
                    "e5073e06eb284c82902a23f248e4145d.fbx"),
                OffshorePlatformAssetPath);
        }

        private static void CopyOptionalModel(string sourcePath, string destinationPath)
        {
            if (File.Exists(sourcePath) &&
                (!File.Exists(destinationPath) ||
                 new FileInfo(sourcePath).Length != new FileInfo(destinationPath).Length))
            {
                File.Copy(sourcePath, destinationPath, true);
                Debug.Log($"Imported optional SCADA model: {sourcePath}");
            }

            if (File.Exists(destinationPath))
            {
                AssetDatabase.ImportAsset(destinationPath, ImportAssetOptions.ForceUpdate);
            }
        }

        private static void EnsureCompanyIntroVideo()
        {
            Directory.CreateDirectory("Assets/StreamingAssets");

            string sourcePath = Path.Combine(
                System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile),
                "Downloads",
                "mp__Вы_можете_скачать_его_по_ссылке_выше.mp4");

            if (File.Exists(sourcePath) &&
                (!File.Exists(CompanyIntroVideoPath) ||
                 new FileInfo(sourcePath).Length != new FileInfo(CompanyIntroVideoPath).Length))
            {
                File.Copy(sourcePath, CompanyIntroVideoPath, true);
                Debug.Log($"Imported company intro video: {sourcePath}");
            }

            if (File.Exists(CompanyIntroVideoPath))
            {
                AssetDatabase.ImportAsset(CompanyIntroVideoPath, ImportAssetOptions.ForceUpdate);
            }
        }

        private static void EnsureAphexTwinMusic()
        {
            Directory.CreateDirectory(MusicStreamingDirectory);

            string sourceRoot = Path.Combine(
                System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile),
                "Downloads",
                "Aphex Twin");

            if (!Directory.Exists(sourceRoot))
            {
                Debug.LogWarning("Aphex Twin folder was not found in Downloads. Music player will stay silent until tracks are placed in StreamingAssets/Music.");
                return;
            }

            string[] supportedExtensions = { ".mp3", ".wav", ".ogg", ".aif", ".aiff" };
            string[] sourceFiles = Directory.GetFiles(sourceRoot, "*.*", SearchOption.AllDirectories);
            Array.Sort(sourceFiles, StringComparer.OrdinalIgnoreCase);

            int copied = 0;
            for (int i = 0; i < sourceFiles.Length; i++)
            {
                string extension = Path.GetExtension(sourceFiles[i]).ToLowerInvariant();
                if (Array.IndexOf(supportedExtensions, extension) < 0)
                {
                    continue;
                }

                string destinationPath = Path.Combine(MusicStreamingDirectory, Path.GetFileName(sourceFiles[i]));
                if (!File.Exists(destinationPath) ||
                    new FileInfo(sourceFiles[i]).Length != new FileInfo(destinationPath).Length)
                {
                    File.Copy(sourceFiles[i], destinationPath, true);
                    copied++;
                }
            }

            if (copied > 0)
            {
                Debug.Log($"Imported SCADA music tracks: {copied}");
            }

            AssetDatabase.ImportAsset(MusicStreamingDirectory, ImportAssetOptions.ImportRecursive | ImportAssetOptions.ForceUpdate);
        }
    }
}
#endif
