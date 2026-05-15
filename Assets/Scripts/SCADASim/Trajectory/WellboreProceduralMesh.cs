using System.Collections.Generic;
using SCADASim.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace SCADASim.Trajectory
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public sealed class WellboreProceduralMesh : MonoBehaviour
    {
        [Header("Survey")]
        [SerializeField] private WellboreSurveyAsset surveyAsset;
        [SerializeField] private WellboreProfileType profileType = WellboreProfileType.Horizontal;
        [SerializeField] private List<WellboreSurveyPoint> localSurveyPoints = new List<WellboreSurveyPoint>
        {
            new WellboreSurveyPoint(0f, 0f, 45f),
            new WellboreSurveyPoint(300f, 10f, 45f),
            new WellboreSurveyPoint(700f, 65f, 45f),
            new WellboreSurveyPoint(1200f, 90f, 45f)
        };

        [Header("Generated Profiles")]
        [SerializeField] private float generatedMaxMeasuredDepth = 1500f;
        [SerializeField] private float generatedStationInterval = 50f;
        [SerializeField] private float generatedAzimuthDegrees = 45f;

        [Header("Mesh")]
        [SerializeField] private float wellboreRadius = 0.18f;
        [SerializeField] private int radialSegments = 24;
        [SerializeField] private int samplesPerSurveySegment = 8;
        [SerializeField] private bool capEnds = true;
        [SerializeField] private bool updateMeshCollider = true;
        [SerializeField] private float uvMetersPerTile = 8f;

        [Header("Integration")]
        [SerializeField] private SimulationEventChannel eventChannel;
        [SerializeField] private bool regenerateOnAwake = true;

        private readonly List<TrajectorySample> samples = new List<TrajectorySample>();
        private Mesh generatedMesh;
        private MeshFilter meshFilter;
        private MeshCollider meshCollider;
        private float visibleMeasuredDepth = float.PositiveInfinity;

        public IReadOnlyList<TrajectorySample> Samples => samples;
        public float MaxMeasuredDepth => samples.Count == 0 ? 0f : samples[samples.Count - 1].MeasuredDepth;

        public void ConfigureRuntime(
            WellboreProfileType runtimeProfileType,
            float maxMeasuredDepth,
            float stationInterval,
            float azimuthDegrees,
            float radius,
            int segments,
            int surveySamples,
            SimulationEventChannel channel)
        {
            profileType = runtimeProfileType;
            generatedMaxMeasuredDepth = maxMeasuredDepth;
            generatedStationInterval = stationInterval;
            generatedAzimuthDegrees = azimuthDegrees;
            wellboreRadius = radius;
            radialSegments = segments;
            samplesPerSurveySegment = surveySamples;
            eventChannel = channel;
            OnValidate();
        }

        private void Awake()
        {
            CacheComponents();

            if (regenerateOnAwake)
            {
                Regenerate();
            }
        }

        private void OnValidate()
        {
            wellboreRadius = Mathf.Max(0.01f, wellboreRadius);
            radialSegments = Mathf.Clamp(radialSegments, 6, 96);
            samplesPerSurveySegment = Mathf.Clamp(samplesPerSurveySegment, 1, 64);
            generatedMaxMeasuredDepth = Mathf.Max(10f, generatedMaxMeasuredDepth);
            generatedStationInterval = Mathf.Max(1f, generatedStationInterval);
            uvMetersPerTile = Mathf.Max(0.1f, uvMetersPerTile);
        }

        public void Regenerate()
        {
            CacheComponents();

            IReadOnlyList<WellboreSurveyPoint> surveyPoints = ResolveSurveyPoints();
            if (surveyPoints == null || surveyPoints.Count < 2)
            {
                Debug.LogError("Wellbore mesh generation requires at least two survey points.");
                return;
            }

            samples.Clear();
            samples.AddRange(WellboreTrajectorySolver.BuildSamples(surveyPoints, samplesPerSurveySegment));

            BuildMesh(BuildVisibleTrajectory(visibleMeasuredDepth));

            eventChannel?.Raise(
                SimulationEventType.WellboreGenerated,
                AlertSeverity.Info,
                $"Wellbore generated: {samples.Count} trajectory samples, MD {MaxMeasuredDepth:0.0} m.",
                this);
        }

        public bool TryEvaluateAtMeasuredDepth(float measuredDepth, out TrajectorySample sample)
        {
            return WellboreTrajectorySolver.TryEvaluateAtMeasuredDepth(samples, measuredDepth, out sample);
        }

        public void SetVisibleMeasuredDepth(float measuredDepth)
        {
            float clampedDepth = Mathf.Clamp(measuredDepth, 0f, MaxMeasuredDepth);
            if (Mathf.Abs(clampedDepth - visibleMeasuredDepth) < 1.5f)
            {
                return;
            }

            visibleMeasuredDepth = clampedDepth;
            BuildMesh(BuildVisibleTrajectory(visibleMeasuredDepth));
        }

        private IReadOnlyList<WellboreSurveyPoint> ResolveSurveyPoints()
        {
            if (profileType != WellboreProfileType.CustomSurvey)
            {
                return WellboreTrajectorySolver.BuildStandardProfile(
                    profileType,
                    generatedMaxMeasuredDepth,
                    generatedStationInterval,
                    generatedAzimuthDegrees);
            }

            if (surveyAsset != null && surveyAsset.SurveyPoints.Count >= 2)
            {
                return surveyAsset.SurveyPoints;
            }

            return localSurveyPoints;
        }

        private void BuildMesh(IReadOnlyList<TrajectorySample> trajectory)
        {
            if (generatedMesh == null)
            {
                generatedMesh = new Mesh { name = "Procedural Wellbore Mesh" };
            }
            else
            {
                generatedMesh.Clear();
            }

            int rings = trajectory.Count;
            if (rings < 2)
            {
                meshFilter.sharedMesh = generatedMesh;
                if (updateMeshCollider && meshCollider != null)
                {
                    meshCollider.sharedMesh = null;
                }

                return;
            }
            int verticesPerRing = radialSegments + 1;
            int surfaceVertexCount = rings * verticesPerRing;
            int estimatedCapVertices = capEnds ? 2 * (radialSegments + 2) : 0;
            int estimatedTriangleCount = (rings - 1) * radialSegments * 6 + (capEnds ? radialSegments * 6 : 0);

            List<Vector3> vertices = new List<Vector3>(surfaceVertexCount + estimatedCapVertices);
            List<Vector3> normals = new List<Vector3>(surfaceVertexCount + estimatedCapVertices);
            List<Vector2> uvs = new List<Vector2>(surfaceVertexCount + estimatedCapVertices);
            List<int> triangles = new List<int>(estimatedTriangleCount);

            Vector3 previousNormal = Vector3.zero;
            RingFrame firstFrame = default;
            RingFrame lastFrame = default;

            for (int ring = 0; ring < rings; ring++)
            {
                TrajectorySample sample = trajectory[ring];
                RingFrame frame = BuildFrame(sample.Tangent, previousNormal);
                previousNormal = frame.Normal;

                if (ring == 0)
                {
                    firstFrame = frame;
                }

                if (ring == rings - 1)
                {
                    lastFrame = frame;
                }

                float v = sample.MeasuredDepth / uvMetersPerTile;

                for (int segment = 0; segment <= radialSegments; segment++)
                {
                    float u = segment / (float)radialSegments;
                    float angle = u * Mathf.PI * 2f;
                    Vector3 radialDirection =
                        Mathf.Cos(angle) * frame.Normal +
                        Mathf.Sin(angle) * frame.Binormal;

                    vertices.Add(sample.Position + radialDirection * wellboreRadius);
                    normals.Add(radialDirection.normalized);
                    uvs.Add(new Vector2(u, v));
                }
            }

            for (int ring = 0; ring < rings - 1; ring++)
            {
                int currentRing = ring * verticesPerRing;
                int nextRing = (ring + 1) * verticesPerRing;

                for (int segment = 0; segment < radialSegments; segment++)
                {
                    int a = currentRing + segment;
                    int b = nextRing + segment;
                    int c = nextRing + segment + 1;
                    int d = currentRing + segment + 1;

                    // Winding is chosen for outward normals in Unity's left-handed scene convention.
                    triangles.Add(a);
                    triangles.Add(c);
                    triangles.Add(b);

                    triangles.Add(a);
                    triangles.Add(d);
                    triangles.Add(c);
                }
            }

            if (capEnds)
            {
                AddCap(
                    trajectory[0].Position,
                    -trajectory[0].Tangent,
                    firstFrame,
                    vertices,
                    normals,
                    uvs,
                    triangles,
                    true);

                AddCap(
                    trajectory[rings - 1].Position,
                    trajectory[rings - 1].Tangent,
                    lastFrame,
                    vertices,
                    normals,
                    uvs,
                    triangles,
                    false);
            }

            if (vertices.Count > 65000)
            {
                generatedMesh.indexFormat = IndexFormat.UInt32;
            }

            generatedMesh.SetVertices(vertices);
            generatedMesh.SetNormals(normals);
            generatedMesh.SetUVs(0, uvs);
            generatedMesh.SetTriangles(triangles, 0);
            generatedMesh.RecalculateBounds();

            meshFilter.sharedMesh = generatedMesh;

            if (updateMeshCollider)
            {
                if (meshCollider == null)
                {
                    meshCollider = GetComponent<MeshCollider>();
                    if (meshCollider == null)
                    {
                        meshCollider = gameObject.AddComponent<MeshCollider>();
                    }
                }

                meshCollider.sharedMesh = null;
                meshCollider.sharedMesh = generatedMesh;
            }
        }

        private IReadOnlyList<TrajectorySample> BuildVisibleTrajectory(float measuredDepth)
        {
            if (samples.Count < 2 || measuredDepth < 2f)
            {
                return System.Array.Empty<TrajectorySample>();
            }

            if (float.IsPositiveInfinity(measuredDepth) || measuredDepth >= MaxMeasuredDepth - 0.1f)
            {
                return samples;
            }

            List<TrajectorySample> visible = new List<TrajectorySample>();
            for (int i = 0; i < samples.Count; i++)
            {
                if (samples[i].MeasuredDepth <= measuredDepth)
                {
                    visible.Add(samples[i]);
                    continue;
                }

                break;
            }

            if (TryEvaluateAtMeasuredDepth(measuredDepth, out TrajectorySample edgeSample) &&
                (visible.Count == 0 || edgeSample.MeasuredDepth - visible[visible.Count - 1].MeasuredDepth > 0.1f))
            {
                visible.Add(edgeSample);
            }

            return visible.Count >= 2 ? visible : System.Array.Empty<TrajectorySample>();
        }

        private void AddCap(
            Vector3 center,
            Vector3 capNormal,
            RingFrame frame,
            List<Vector3> vertices,
            List<Vector3> normals,
            List<Vector2> uvs,
            List<int> triangles,
            bool startCap)
        {
            int centerIndex = vertices.Count;
            vertices.Add(center);
            normals.Add(capNormal.normalized);
            uvs.Add(new Vector2(0.5f, 0.5f));

            int ringStartIndex = vertices.Count;
            for (int segment = 0; segment <= radialSegments; segment++)
            {
                float u = segment / (float)radialSegments;
                float angle = u * Mathf.PI * 2f;
                Vector3 radialDirection =
                    Mathf.Cos(angle) * frame.Normal +
                    Mathf.Sin(angle) * frame.Binormal;

                vertices.Add(center + radialDirection * wellboreRadius);
                normals.Add(capNormal.normalized);
                uvs.Add(new Vector2(
                    0.5f + Mathf.Cos(angle) * 0.5f,
                    0.5f + Mathf.Sin(angle) * 0.5f));
            }

            for (int segment = 0; segment < radialSegments; segment++)
            {
                int a = ringStartIndex + segment;
                int b = ringStartIndex + segment + 1;

                if (startCap)
                {
                    triangles.Add(centerIndex);
                    triangles.Add(b);
                    triangles.Add(a);
                }
                else
                {
                    triangles.Add(centerIndex);
                    triangles.Add(a);
                    triangles.Add(b);
                }
            }
        }

        private static RingFrame BuildFrame(Vector3 tangent, Vector3 previousNormal)
        {
            Vector3 safeTangent = tangent.sqrMagnitude > 0.0001f
                ? tangent.normalized
                : Vector3.down;

            Vector3 normal = previousNormal == Vector3.zero
                ? BuildInitialNormal(safeTangent)
                : Vector3.ProjectOnPlane(previousNormal, safeTangent).normalized;

            if (normal.sqrMagnitude < 0.0001f)
            {
                normal = BuildInitialNormal(safeTangent);
            }

            Vector3 binormal = Vector3.Cross(safeTangent, normal).normalized;
            normal = Vector3.Cross(binormal, safeTangent).normalized;

            return new RingFrame(normal, binormal);
        }

        private static Vector3 BuildInitialNormal(Vector3 tangent)
        {
            Vector3 reference = Mathf.Abs(Vector3.Dot(tangent, Vector3.up)) > 0.95f
                ? Vector3.forward
                : Vector3.up;

            return Vector3.Cross(tangent, reference).normalized;
        }

        private void CacheComponents()
        {
            if (meshFilter == null)
            {
                meshFilter = GetComponent<MeshFilter>();
            }

            if (updateMeshCollider && meshCollider == null)
            {
                meshCollider = GetComponent<MeshCollider>();
            }
        }

        private readonly struct RingFrame
        {
            public readonly Vector3 Normal;
            public readonly Vector3 Binormal;

            public RingFrame(Vector3 normal, Vector3 binormal)
            {
                Normal = normal;
                Binormal = binormal;
            }
        }
    }
}
