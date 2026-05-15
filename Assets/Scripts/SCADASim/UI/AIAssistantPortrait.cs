using UnityEngine;

namespace SCADASim.UI
{
    public sealed class AIAssistantPortrait : MonoBehaviour
    {
        private const string AssistantResourcePath = "Models/aiAssistant";
        private const int PortraitLayer = 30;

        [SerializeField] private int textureSize = 256;
        [SerializeField] private Vector3 portraitScenePosition = new Vector3(10000f, 10000f, 10000f);

        private RenderTexture renderTexture;
        private Camera portraitCamera;
        private Light keyLight;
        private Transform modelRoot;
        private Texture2D fallbackTexture;

        public Texture PortraitTexture => renderTexture != null ? renderTexture : fallbackTexture;
        public bool HasAssistantModel { get; private set; }

        private void Awake()
        {
            BuildPortraitScene();
        }

        private void LateUpdate()
        {
            if (modelRoot != null)
            {
                modelRoot.localRotation = Quaternion.Euler(0f, Mathf.Sin(Time.unscaledTime * 0.35f) * 4f, 0f);
            }

            portraitCamera?.Render();
        }

        private void OnDestroy()
        {
            if (renderTexture != null)
            {
                renderTexture.Release();
                Destroy(renderTexture);
            }

            if (fallbackTexture != null)
            {
                Destroy(fallbackTexture);
            }
        }

        private void BuildPortraitScene()
        {
            renderTexture = new RenderTexture(textureSize, textureSize, 16, RenderTextureFormat.ARGB32)
            {
                antiAliasing = 4,
                name = "AI Assistant Portrait"
            };

            fallbackTexture = CreateFallbackTexture();

            GameObject sceneRoot = new GameObject("AI Assistant Portrait Scene");
            sceneRoot.transform.SetParent(transform, false);
            sceneRoot.transform.position = portraitScenePosition;
            SetLayerRecursively(sceneRoot, PortraitLayer);

            GameObject cameraObject = new GameObject("AI Assistant Portrait Camera");
            cameraObject.transform.SetParent(sceneRoot.transform, false);
            portraitCamera = cameraObject.AddComponent<Camera>();
            portraitCamera.clearFlags = CameraClearFlags.SolidColor;
            portraitCamera.backgroundColor = new Color(0.89f, 0.87f, 0.8f, 1f);
            portraitCamera.orthographic = true;
            portraitCamera.orthographicSize = 1.15f;
            portraitCamera.nearClipPlane = 0.01f;
            portraitCamera.farClipPlane = 12f;
            portraitCamera.cullingMask = 1 << PortraitLayer;
            portraitCamera.targetTexture = renderTexture;
            portraitCamera.enabled = false;
            cameraObject.transform.localPosition = new Vector3(0f, 0.86f, -3.1f);
            cameraObject.transform.localRotation = Quaternion.LookRotation(new Vector3(0f, 0.72f, 0f) - cameraObject.transform.localPosition);

            GameObject lightObject = new GameObject("AI Assistant Portrait Key Light");
            lightObject.transform.SetParent(sceneRoot.transform, false);
            keyLight = lightObject.AddComponent<Light>();
            keyLight.type = LightType.Directional;
            keyLight.intensity = 1.8f;
            lightObject.transform.localRotation = Quaternion.Euler(42f, -28f, 0f);

            GameObject assistantPrefab = Resources.Load<GameObject>(AssistantResourcePath);
            if (assistantPrefab == null)
            {
                return;
            }

            GameObject assistant = Instantiate(assistantPrefab, sceneRoot.transform);
            assistant.name = "AI Assistant Model";
            modelRoot = assistant.transform;
            SetLayerRecursively(assistant, PortraitLayer);
            NormalizeAssistantModel(assistant, sceneRoot.transform.position + new Vector3(0f, 0.72f, 0f), 1.45f);
            HasAssistantModel = true;
        }

        private static void NormalizeAssistantModel(GameObject instance, Vector3 targetCenter, float targetHeight)
        {
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;

            if (!TryCalculateBounds(instance, out Bounds bounds))
            {
                return;
            }

            float scale = targetHeight / Mathf.Max(0.01f, bounds.size.y);
            instance.transform.localScale = Vector3.one * scale;

            if (!TryCalculateBounds(instance, out bounds))
            {
                return;
            }

            instance.transform.position += targetCenter - bounds.center;
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

        private static void SetLayerRecursively(GameObject target, int layer)
        {
            target.layer = layer;
            for (int i = 0; i < target.transform.childCount; i++)
            {
                SetLayerRecursively(target.transform.GetChild(i).gameObject, layer);
            }
        }

        private static Texture2D CreateFallbackTexture()
        {
            const int size = 64;
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = "AI Assistant Fallback"
            };

            Color background = new Color(0.89f, 0.87f, 0.8f, 1f);
            Color ink = new Color(0.07f, 0.07f, 0.065f, 1f);
            Color accent = new Color(0.85f, 0.02f, 0.08f, 1f);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool border = x < 3 || y < 3 || x > size - 4 || y > size - 4;
                    bool eye = (x > 18 && x < 25 && y > 35 && y < 42) || (x > 39 && x < 46 && y > 35 && y < 42);
                    bool mouth = x > 20 && x < 44 && y > 20 && y < 24;
                    bool signal = x > 48 && y > 48;
                    texture.SetPixel(x, y, border || eye || mouth ? ink : signal ? accent : background);
                }
            }

            texture.Apply();
            return texture;
        }
    }
}
