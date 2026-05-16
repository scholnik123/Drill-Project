using System.Collections.Generic;
using SCADASim.Core;
using SCADASim.Physics;
using UnityEngine;
using UnityEngine.Rendering;

namespace SCADASim.Trajectory
{
    public sealed class MicroDrillingVisualController : MonoBehaviour
    {
        private const int DrillStringPointCount = 22;
        private const int BubbleCount = 24;
        private const int SloughParticleCount = 18;

        private readonly List<Transform> gasBubbles = new List<Transform>(BubbleCount);
        private readonly List<Transform> sloughParticles = new List<Transform>(SloughParticleCount);

        private WellboreProceduralMesh wellbore;
        private DrillingModel drillingModel;
        private LineRenderer drillStringLine;
        private Transform bottomholeFluid;
        private Transform bopRamLeft;
        private Transform bopRamRight;
        private Transform shakerCuttings;
        private Transform chokeValve;
        private Transform annulusLevel;
        private Material drillStringMaterial;
        private Material gasMaterial;
        private Material sloughMaterial;
        private Material fluidMaterial;
        private Material equipmentMaterial;
        private Material cuttingsMaterial;

        public void Configure(WellboreProceduralMesh targetWellbore, DrillingModel model)
        {
            wellbore = targetWellbore;
            drillingModel = model;
            EnsureObjects();
        }

        private void Awake()
        {
            EnsureObjects();
        }

        private void Update()
        {
            if (wellbore == null || drillingModel == null || wellbore.Samples.Count < 2)
            {
                return;
            }

            DrillingState state = drillingModel.CurrentState;
            if (!wellbore.TryEvaluateAtMeasuredDepth(state.MeasuredDepth, out TrajectorySample bitSample))
            {
                return;
            }

            Vector3 bitPosition = wellbore.transform.TransformPoint(bitSample.Position);
            Vector3 bitTangent = wellbore.transform.TransformDirection(bitSample.Tangent).normalized;

            UpdateDrillString(state, bitSample);
            UpdateBottomholeAndAnnulus(state, bitPosition, bitTangent);
            UpdateSurfaceEquipment(state);
        }

        private void EnsureObjects()
        {
            drillStringMaterial ??= MakeMaterial("Drill String Micro", new Color(0.05f, 0.055f, 0.05f, 1f));
            gasMaterial ??= MakeMaterial("Gas Bubbles Micro", new Color(0.95f, 0.96f, 0.88f, 0.85f));
            sloughMaterial ??= MakeMaterial("Sloughing Cuttings Micro", new Color(0.38f, 0.31f, 0.22f, 1f));
            fluidMaterial ??= MakeMaterial("Annulus Fluid Micro", new Color(0.08f, 0.36f, 0.52f, 0.72f));
            equipmentMaterial ??= MakeMaterial("BOP Rams Micro", new Color(0.1f, 0.1f, 0.095f, 1f));
            cuttingsMaterial ??= MakeMaterial("Shaker Cuttings Micro", new Color(0.45f, 0.36f, 0.22f, 1f));

            if (drillStringLine == null)
            {
                GameObject lineObject = new GameObject("Animated Drill String");
                lineObject.transform.SetParent(transform, false);
                drillStringLine = lineObject.AddComponent<LineRenderer>();
                drillStringLine.useWorldSpace = true;
                drillStringLine.positionCount = DrillStringPointCount;
                drillStringLine.widthMultiplier = 0.22f;
                drillStringLine.numCornerVertices = 6;
                drillStringLine.numCapVertices = 6;
                drillStringLine.shadowCastingMode = ShadowCastingMode.On;
                drillStringLine.receiveShadows = true;
                drillStringLine.sharedMaterial = drillStringMaterial;
            }

            bottomholeFluid ??= CreatePrimitive("Dynamic Bottomhole Fluid", PrimitiveType.Sphere, fluidMaterial, transform);
            annulusLevel ??= CreatePrimitive("Annulus Fluid Level", PrimitiveType.Cylinder, fluidMaterial, transform);
            bopRamLeft ??= CreatePrimitive("BOP Ram Left", PrimitiveType.Cube, equipmentMaterial, transform);
            bopRamRight ??= CreatePrimitive("BOP Ram Right", PrimitiveType.Cube, equipmentMaterial, transform);
            shakerCuttings ??= CreatePrimitive("Shaker Cuttings Volume", PrimitiveType.Cube, cuttingsMaterial, transform);
            chokeValve ??= CreatePrimitive("Animated Choke Valve", PrimitiveType.Cylinder, equipmentMaterial, transform);

            while (gasBubbles.Count < BubbleCount)
            {
                Transform bubble = CreatePrimitive($"Gas Bubble {gasBubbles.Count + 1:00}", PrimitiveType.Sphere, gasMaterial, transform);
                gasBubbles.Add(bubble);
            }

            while (sloughParticles.Count < SloughParticleCount)
            {
                Transform particle = CreatePrimitive($"Wall Slough Particle {sloughParticles.Count + 1:00}", PrimitiveType.Cube, sloughMaterial, transform);
                sloughParticles.Add(particle);
            }
        }

        private void UpdateDrillString(DrillingState state, TrajectorySample bitSample)
        {
            float visibleLength = Mathf.Clamp(state.MeasuredDepth, 80f, 520f);
            float startMeasuredDepth = Mathf.Max(0f, state.MeasuredDepth - visibleLength);
            float vibration = Mathf.Clamp01(state.Vibration.LowFrequencyEnergy + state.Vibration.LateralEnergy * 0.65f);
            float torsion = Mathf.Clamp01(state.SurfaceTorqueKnM / 58f + state.WeightOnBitTonnes / 42f);
            float bendAmplitude = Mathf.Lerp(0.035f, 1.05f, vibration) * Mathf.Lerp(0.65f, 1.25f, torsion);
            float spin = Time.time * Mathf.Lerp(5f, 18f, vibration) + state.Rpm * 0.018f;

            for (int i = 0; i < DrillStringPointCount; i++)
            {
                float t = i / (float)(DrillStringPointCount - 1);
                float md = Mathf.Lerp(startMeasuredDepth, state.MeasuredDepth, t);
                if (!wellbore.TryEvaluateAtMeasuredDepth(md, out TrajectorySample sample))
                {
                    sample = bitSample;
                }

                Vector3 position = wellbore.transform.TransformPoint(sample.Position);
                Vector3 tangent = wellbore.transform.TransformDirection(sample.Tangent).normalized;
                Vector3 side = Vector3.Cross(tangent, Vector3.up);
                if (side.sqrMagnitude < 0.001f)
                {
                    side = Vector3.Cross(tangent, Vector3.right);
                }

                side.Normalize();
                Vector3 normal = Vector3.Cross(side, tangent).normalized;
                float envelope = Mathf.Sin(t * Mathf.PI);
                float phase = spin + t * Mathf.PI * 5.5f;
                Vector3 offset = (side * Mathf.Sin(phase) + normal * Mathf.Cos(phase * 0.72f)) * bendAmplitude * envelope;
                drillStringLine.SetPosition(i, position + offset);
            }
        }

        private void UpdateBottomholeAndAnnulus(DrillingState state, Vector3 bitPosition, Vector3 bitTangent)
        {
            float kickIntensity = Mathf.Clamp01(state.KickRisk01 + state.GasUnitsPercent / 8f);
            float sloughIntensity = Mathf.Clamp01(state.BoreholeInstabilityRisk01 + (1f - state.CuttingsTransportEfficiency01) * 0.55f);
            float lossIntensity = Mathf.Clamp01(state.LostCirculationRisk01);
            float vibration = Mathf.Clamp01(state.Vibration.LowFrequencyEnergy);

            bottomholeFluid.position = bitPosition - bitTangent * 0.65f;
            bottomholeFluid.localScale = Vector3.one * Mathf.Lerp(0.65f, 1.75f, Mathf.Max(kickIntensity, lossIntensity));

            annulusLevel.position = bitPosition + Vector3.up * Mathf.Lerp(1.4f, 3.4f, 1f - lossIntensity);
            annulusLevel.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);
            annulusLevel.localScale = new Vector3(
                Mathf.Lerp(0.55f, 1.15f, state.FlowRateLps / 62f),
                Mathf.Lerp(0.35f, 1.65f, 1f - lossIntensity),
                Mathf.Lerp(0.55f, 1.15f, state.FlowRateLps / 62f));

            for (int i = 0; i < gasBubbles.Count; i++)
            {
                Transform bubble = gasBubbles[i];
                bool visible = i < Mathf.RoundToInt(kickIntensity * gasBubbles.Count);
                bubble.gameObject.SetActive(visible);
                if (!visible)
                {
                    continue;
                }

                float seed = i * 1.618f;
                float rise = Mathf.Repeat(Time.time * (0.28f + kickIntensity * 0.65f) + seed, 1f);
                Vector3 swirl = new Vector3(
                    Mathf.Sin(seed + Time.time * 1.7f),
                    rise * 5.0f,
                    Mathf.Cos(seed * 0.7f + Time.time * 1.4f)) * Mathf.Lerp(0.18f, 0.68f, kickIntensity);
                bubble.position = bitPosition + swirl - bitTangent * 0.3f;
                bubble.localScale = Vector3.one * Mathf.Lerp(0.12f, 0.38f, kickIntensity) * (0.75f + rise * 0.5f);
            }

            for (int i = 0; i < sloughParticles.Count; i++)
            {
                Transform particle = sloughParticles[i];
                bool visible = i < Mathf.RoundToInt(sloughIntensity * sloughParticles.Count);
                particle.gameObject.SetActive(visible);
                if (!visible)
                {
                    continue;
                }

                float seed = i * 2.37f;
                float fall = Mathf.Repeat(Time.time * (0.18f + sloughIntensity * 0.35f) + seed, 1f);
                Vector3 tumble = new Vector3(
                    Mathf.Sin(seed) * 0.8f,
                    -fall * 2.6f,
                    Mathf.Cos(seed * 0.9f) * 0.8f);
                particle.position = bitPosition + tumble + bitTangent * Mathf.Lerp(0.2f, 1.2f, fall);
                particle.rotation = Quaternion.Euler(Time.time * 40f + seed * 15f, seed * 55f, Time.time * 26f);
                particle.localScale = Vector3.one * Mathf.Lerp(0.12f, 0.42f, sloughIntensity + vibration * 0.25f);
            }
        }

        private void UpdateSurfaceEquipment(DrillingState state)
        {
            Vector3 wellhead = wellbore.transform.position;
            Vector3 right = Vector3.right;
            Vector3 forward = Vector3.forward;
            float ramClose01 = Mathf.Clamp01((1f - state.ChokeOpening01) * 1.15f + state.KickRisk01 * 0.2f);

            bopRamLeft.position = wellhead + Vector3.up * 1.3f - right * Mathf.Lerp(3.0f, 0.72f, ramClose01);
            bopRamRight.position = wellhead + Vector3.up * 1.3f + right * Mathf.Lerp(3.0f, 0.72f, ramClose01);
            bopRamLeft.localScale = new Vector3(1.15f, 0.45f, 0.65f);
            bopRamRight.localScale = new Vector3(1.15f, 0.45f, 0.65f);

            chokeValve.position = wellhead + right * 4.2f + forward * 1.8f + Vector3.up * 0.9f;
            chokeValve.rotation = Quaternion.Euler(90f, 0f, Time.time * 70f + state.ChokeOpening01 * 220f);
            chokeValve.localScale = new Vector3(0.6f, 0.18f, 0.6f);

            float cuttingsVolume = Mathf.Clamp01((1f - state.CuttingsTransportEfficiency01) * 1.35f + state.RopMPerHour / 65f);
            shakerCuttings.position = wellhead - right * 4.5f + forward * 2.1f + Vector3.up * Mathf.Lerp(0.22f, 0.85f, cuttingsVolume);
            shakerCuttings.rotation = Quaternion.Euler(0f, Time.time * 5f, 0f);
            shakerCuttings.localScale = new Vector3(2.8f, Mathf.Lerp(0.16f, 1.25f, cuttingsVolume), 1.25f);
        }

        private static Transform CreatePrimitive(string name, PrimitiveType type, Material material, Transform parent)
        {
            GameObject primitive = GameObject.CreatePrimitive(type);
            primitive.name = name;
            primitive.transform.SetParent(parent, false);
            primitive.GetComponent<Renderer>().sharedMaterial = material;
            Collider collider = primitive.GetComponent<Collider>();
            if (collider != null)
            {
                Object.Destroy(collider);
            }

            return primitive.transform;
        }

        private static Material MakeMaterial(string name, Color color)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            Material material = new Material(shader) { name = name };
            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }
            else
            {
                material.color = color;
            }

            if (color.a < 0.99f)
            {
                material.renderQueue = 3000;
                material.SetFloat("_Surface", 1f);
                material.SetFloat("_AlphaClip", 0f);
                material.SetFloat("_Blend", 0f);
                material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetFloat("_ZWrite", 0f);
            }

            return material;
        }
    }
}
