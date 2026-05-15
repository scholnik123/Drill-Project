using System.Collections.Generic;
using SCADASim.Core;
using SCADASim.Physics;
using UnityEngine;

namespace SCADASim.Trajectory
{
    public sealed class Wellbore3DVisualizer : MonoBehaviour
    {
        [SerializeField] private WellboreProceduralMesh wellbore;
        [SerializeField] private DrillingModel drillingModel;
        [SerializeField] private SimulationEventChannel eventChannel;

        private readonly List<StationMarker> stationMarkers = new List<StationMarker>();
        private readonly List<CasingCollarMarker> casingCollars = new List<CasingCollarMarker>();
        private Transform stationRoot;
        private Transform collarRoot;
        private Transform bitMarker;
        private Transform bitDirection;
        private Transform riskHalo;
        private Transform referencePlane;
        private TextMesh bitTelemetryLabel;
        private Material stationMaterial;
        private Material collarMaterial;
        private Material bitMaterial;
        private Material directionMaterial;
        private Material referenceMaterial;
        private Material lowRiskMaterial;
        private Material mediumRiskMaterial;
        private Material highRiskMaterial;
        private int lastSampleCount;
        private float lastMaxMeasuredDepth;
        private Vector3 lastWellborePosition;
        private Vector3 lastWellboreScale;

        public void Configure(
            WellboreProceduralMesh targetWellbore,
            DrillingModel model,
            SimulationEventChannel channel)
        {
            wellbore = targetWellbore;
            drillingModel = model;
            eventChannel = channel;
            EnsureSceneObjects();
            RebuildStations();
        }

        private void Awake()
        {
            EnsureSceneObjects();
        }

        private void Update()
        {
            if (wellbore == null)
            {
                return;
            }

            if (NeedsStationRebuild())
            {
                RebuildStations();
            }

            UpdateBitMarker();
            AlignLabelsToCamera();
        }

        private bool NeedsStationRebuild()
        {
            return wellbore.Samples.Count != lastSampleCount ||
                   Mathf.Abs(wellbore.MaxMeasuredDepth - lastMaxMeasuredDepth) > 0.1f ||
                   (wellbore.transform.position - lastWellborePosition).sqrMagnitude > 0.0001f ||
                   (wellbore.transform.lossyScale - lastWellboreScale).sqrMagnitude > 0.0001f;
        }

        private void EnsureSceneObjects()
        {
            if (stationRoot == null)
            {
                stationRoot = new GameObject("Wellbore Station Markers").transform;
                stationRoot.SetParent(transform, false);
            }

            if (collarRoot == null)
            {
                collarRoot = new GameObject("Casing Collar Markers").transform;
                collarRoot.SetParent(transform, false);
            }

            stationMaterial ??= MakeMaterial("Station Marker", new Color(0.06f, 0.06f, 0.055f));
            collarMaterial ??= MakeMaterial("Casing Collar", new Color(0.8f, 0.82f, 0.78f));
            bitMaterial ??= MakeMaterial("Bit Marker", new Color(0.86f, 0.02f, 0.08f));
            directionMaterial ??= MakeMaterial("Bit Direction", new Color(0.08f, 0.22f, 0.9f, 0.86f));
            referenceMaterial ??= MakeMaterial("Wellhead Reference Plane", new Color(0.08f, 0.08f, 0.075f, 0.16f));
            lowRiskMaterial ??= MakeMaterial("Risk Low", new Color(0.1f, 0.55f, 0.28f, 0.72f));
            mediumRiskMaterial ??= MakeMaterial("Risk Medium", new Color(0.95f, 0.63f, 0.08f, 0.72f));
            highRiskMaterial ??= MakeMaterial("Risk High", new Color(0.86f, 0.02f, 0.08f, 0.78f));

            if (bitMarker == null)
            {
                GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                marker.name = "Current Bit Position";
                marker.transform.SetParent(transform, false);
                marker.transform.localScale = Vector3.one * 0.55f;
                marker.GetComponent<Renderer>().sharedMaterial = bitMaterial;
                bitMarker = marker.transform;
            }

            if (bitDirection == null)
            {
                GameObject direction = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                direction.name = "Current Bit Direction";
                direction.transform.SetParent(transform, false);
                direction.GetComponent<Renderer>().sharedMaterial = directionMaterial;
                bitDirection = direction.transform;
            }

            if (riskHalo == null)
            {
                GameObject halo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                halo.name = "Operational Risk Halo";
                halo.transform.SetParent(transform, false);
                halo.GetComponent<Renderer>().sharedMaterial = lowRiskMaterial;
                riskHalo = halo.transform;
            }

            if (referencePlane == null)
            {
                GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Cube);
                plane.name = "Wellhead Reference Plane";
                plane.transform.SetParent(transform, false);
                plane.GetComponent<Renderer>().sharedMaterial = referenceMaterial;
                referencePlane = plane.transform;
            }

            if (bitTelemetryLabel == null)
            {
                GameObject labelObject = new GameObject("Current Bit Telemetry Label");
                labelObject.transform.SetParent(transform, false);
                bitTelemetryLabel = labelObject.AddComponent<TextMesh>();
                bitTelemetryLabel.characterSize = 0.22f;
                bitTelemetryLabel.anchor = TextAnchor.MiddleLeft;
                bitTelemetryLabel.alignment = TextAlignment.Left;
                bitTelemetryLabel.color = Color.black;
            }
        }

        private void RebuildStations()
        {
            EnsureSceneObjects();
            ClearStations();
            ClearCollars();

            if (wellbore == null || wellbore.Samples.Count < 2)
            {
                return;
            }

            float maxMd = Mathf.Max(1f, wellbore.MaxMeasuredDepth);
            float interval = Mathf.Clamp(maxMd / 6f, 150f, 350f);

            for (float md = 0f; md <= maxMd + 1f; md += interval)
            {
                if (!wellbore.TryEvaluateAtMeasuredDepth(md, out TrajectorySample sample))
                {
                    continue;
                }

                CreateStationMarker(sample);
            }

            float collarInterval = Mathf.Clamp(maxMd / 28f, 35f, 90f);
            for (float md = collarInterval; md <= maxMd - collarInterval * 0.5f; md += collarInterval)
            {
                if (wellbore.TryEvaluateAtMeasuredDepth(md, out TrajectorySample sample))
                {
                    CreateCasingCollar(sample);
                }
            }

            lastSampleCount = wellbore.Samples.Count;
            lastMaxMeasuredDepth = wellbore.MaxMeasuredDepth;
            lastWellborePosition = wellbore.transform.position;
            lastWellboreScale = wellbore.transform.lossyScale;

            eventChannel?.Raise(
                SimulationEventType.WellboreGenerated,
                AlertSeverity.Info,
                "3D-маркеры траектории обновлены.",
                this);
        }

        private void CreateStationMarker(TrajectorySample sample)
        {
            Vector3 position = wellbore.transform.TransformPoint(sample.Position);

            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            marker.name = $"Measured Depth {sample.MeasuredDepth:0} Marker";
            marker.transform.SetParent(stationRoot, true);
            marker.transform.position = position;
            marker.transform.localScale = Vector3.one * 0.22f;
            marker.GetComponent<Renderer>().sharedMaterial = stationMaterial;

            GameObject labelObject = new GameObject($"Measured Depth {sample.MeasuredDepth:0} Label");
            labelObject.transform.SetParent(stationRoot, true);
            labelObject.transform.position = position + Vector3.up * 0.55f;

            TextMesh label = labelObject.AddComponent<TextMesh>();
            label.text = $"Ствол {sample.MeasuredDepth:0} м\nзенит {sample.InclinationDegrees:0}°";
            label.characterSize = 0.23f;
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.color = Color.black;

            stationMarkers.Add(new StationMarker(marker.transform, labelObject.transform));
        }

        private void CreateCasingCollar(TrajectorySample sample)
        {
            Vector3 position = wellbore.transform.TransformPoint(sample.Position);
            Vector3 tangent = wellbore.transform.TransformDirection(sample.Tangent).normalized;

            GameObject collar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            collar.name = $"Casing Collar Measured Depth {sample.MeasuredDepth:0}";
            collar.transform.SetParent(collarRoot, true);
            collar.transform.position = position;
            collar.transform.rotation = Quaternion.FromToRotation(Vector3.up, tangent);
            collar.transform.localScale = new Vector3(0.34f, 0.055f, 0.34f);
            collar.GetComponent<Renderer>().sharedMaterial = collarMaterial;
            collar.SetActive(false);
            casingCollars.Add(new CasingCollarMarker(sample.MeasuredDepth, collar.transform));
        }

        private void UpdateBitMarker()
        {
            if (wellbore == null || drillingModel == null || bitMarker == null || riskHalo == null)
            {
                return;
            }

            DrillingState state = drillingModel.CurrentState;
            wellbore.SetVisibleMeasuredDepth(state.MeasuredDepth);

            if (!wellbore.TryEvaluateAtMeasuredDepth(state.MeasuredDepth, out TrajectorySample sample))
            {
                return;
            }

            Vector3 position = wellbore.transform.TransformPoint(sample.Position);
            Vector3 tangent = wellbore.transform.TransformDirection(sample.Tangent).normalized;
            bitMarker.position = position;
            bitMarker.localScale = Vector3.one * 0.62f;
            bitMarker.Rotate(Vector3.up, 145f * Time.deltaTime, Space.Self);

            if (bitDirection != null)
            {
                bitDirection.position = position + tangent * 0.62f;
                bitDirection.rotation = Quaternion.FromToRotation(Vector3.up, tangent);
                bitDirection.localScale = new Vector3(0.1f, 0.62f, 0.1f);
            }

            float risk = Mathf.Max(
                state.StuckPipeRisk01,
                state.BoreholeInstabilityRisk01,
                state.KickRisk01,
                state.LostCirculationRisk01);

            riskHalo.position = position;
            float pulse = 1f + Mathf.Sin(Time.time * Mathf.Lerp(2f, 6f, risk)) * Mathf.Lerp(0.03f, 0.16f, risk);
            riskHalo.localScale = Vector3.one * Mathf.Lerp(1.1f, 3.4f, risk) * pulse;

            Renderer haloRenderer = riskHalo.GetComponent<Renderer>();
            if (haloRenderer != null)
            {
                haloRenderer.sharedMaterial = risk > 0.68f
                    ? highRiskMaterial
                    : risk > 0.42f
                        ? mediumRiskMaterial
                        : lowRiskMaterial;
            }

            if (referencePlane != null)
            {
                Vector3 wellhead = wellbore.transform.TransformPoint(Vector3.zero);
                referencePlane.position = wellhead + Vector3.down * 0.02f;
                referencePlane.rotation = Quaternion.identity;
                referencePlane.localScale = new Vector3(18f, 0.02f, 18f);
            }

            if (bitTelemetryLabel != null)
            {
                bitTelemetryLabel.transform.position = position + Vector3.up * 0.85f + Vector3.right * 0.3f;
                bitTelemetryLabel.text = $"ДОЛОТО\nпо стволу {state.MeasuredDepth:0} м\nпо вертикали {state.TrueVerticalDepth:0} м\nзенит {state.InclinationDegrees:0.0}°\nриск {risk * 100f:0}%";
            }

            UpdateCasingCollarVisibility(state.MeasuredDepth);
        }

        private void ClearStations()
        {
            for (int i = 0; i < stationMarkers.Count; i++)
            {
                if (stationMarkers[i].Marker != null)
                {
                    Destroy(stationMarkers[i].Marker.gameObject);
                }

                if (stationMarkers[i].Label != null)
                {
                    Destroy(stationMarkers[i].Label.gameObject);
                }
            }

            stationMarkers.Clear();
        }

        private void ClearCollars()
        {
            for (int i = 0; i < casingCollars.Count; i++)
            {
                if (casingCollars[i].Transform != null)
                {
                    Destroy(casingCollars[i].Transform.gameObject);
                }
            }

            casingCollars.Clear();
        }

        private void AlignLabelsToCamera()
        {
            Camera camera = Camera.main;
            if (camera == null)
            {
                return;
            }

            for (int i = 0; i < stationMarkers.Count; i++)
            {
                Transform label = stationMarkers[i].Label;
                if (label == null)
                {
                    continue;
                }

                label.rotation = Quaternion.LookRotation(label.position - camera.transform.position, Vector3.up);
            }

            if (bitTelemetryLabel != null)
            {
                bitTelemetryLabel.transform.rotation = Quaternion.LookRotation(bitTelemetryLabel.transform.position - camera.transform.position, Vector3.up);
            }
        }

        private void UpdateCasingCollarVisibility(float measuredDepth)
        {
            for (int i = 0; i < casingCollars.Count; i++)
            {
                Transform collar = casingCollars[i].Transform;
                if (collar == null)
                {
                    continue;
                }

                bool visible = casingCollars[i].MeasuredDepth <= measuredDepth + 0.5f;
                if (collar.gameObject.activeSelf != visible)
                {
                    collar.gameObject.SetActive(visible);
                }
            }
        }

        private static Material MakeMaterial(string materialName, Color color)
        {
            Material source = Resources.Load<Material>("SCADASim/RuntimeLit");
            Material material = source != null ? new Material(source) : null;

            if (material == null)
            {
                Shader shader = Shader.Find("Standard");
                material = shader != null ? new Material(shader) : null;
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
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetInt("_ZWrite", 0);
                material.DisableKeyword("_ALPHATEST_ON");
                material.EnableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = 3000;
            }
            return material;
        }

        private readonly struct StationMarker
        {
            public readonly Transform Marker;
            public readonly Transform Label;

            public StationMarker(Transform marker, Transform label)
            {
                Marker = marker;
                Label = label;
            }
        }

        private readonly struct CasingCollarMarker
        {
            public readonly float MeasuredDepth;
            public readonly Transform Transform;

            public CasingCollarMarker(float measuredDepth, Transform transform)
            {
                MeasuredDepth = measuredDepth;
                Transform = transform;
            }
        }
    }
}
