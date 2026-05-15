using System;
using System.Collections.Generic;
using SCADASim.Core;
using UnityEngine;

namespace SCADASim.Physics
{
    [Serializable]
    public struct LithologyZone
    {
        public float FromMeasuredDepth;
        public float ToMeasuredDepth;
        public LithologyType Lithology;
        public float RockStrengthMpa;
        [Range(0f, 1f)] public float Abrasiveness01;
        [Range(0f, 1f)] public float Instability01;
        [Range(0f, 1f)] public float Stickiness01;
        public float PorePressureGradientBarPer100m;
        public float FractureGradientSG;
        [Range(0f, 1f)] public float Porosity01;
        public float PermeabilityMd;
        public float GammaRayApi;

        public bool Contains(float measuredDepth)
        {
            return measuredDepth >= FromMeasuredDepth && measuredDepth < ToMeasuredDepth;
        }
    }

    [CreateAssetMenu(menuName = "SCADA Simulation/Physics/Geology Model")]
    public sealed class GeologyModel : ScriptableObject
    {
        [SerializeField] private List<LithologyZone> zones = new List<LithologyZone>();

        public IReadOnlyList<LithologyZone> Zones => zones;

        public LithologyZone GetZone(float measuredDepth)
        {
            for (int i = 0; i < zones.Count; i++)
            {
                if (zones[i].Contains(measuredDepth))
                {
                    return zones[i];
                }
            }

            if (zones.Count == 0)
            {
                return CreateDefaultZone();
            }

            return measuredDepth < zones[0].FromMeasuredDepth
                ? zones[0]
                : zones[zones.Count - 1];
        }

        public static GeologyModel CreateRuntimeDemo(GeologyRegion region = GeologyRegion.WestSiberia)
        {
            GeologyModel model = CreateInstance<GeologyModel>();
            float shaleInstabilityBoost = region == GeologyRegion.ArcticShelf ? 0.16f : 0f;
            float strengthBoost = region == GeologyRegion.Caspian ? 12f : 0f;
            float pressureBoost = region == GeologyRegion.VolgaUral ? -0.08f : region == GeologyRegion.ArcticShelf ? 0.14f : 0f;
            float fractureBoost = region == GeologyRegion.Caspian ? 0.08f : region == GeologyRegion.ArcticShelf ? -0.06f : 0f;

            model.zones.Add(new LithologyZone
            {
                FromMeasuredDepth = 0f,
                ToMeasuredDepth = 320f,
                Lithology = LithologyType.Shale,
                RockStrengthMpa = 34f + strengthBoost,
                Abrasiveness01 = 0.22f,
                Instability01 = Mathf.Clamp01(0.48f + shaleInstabilityBoost),
                Stickiness01 = 0.62f,
                PorePressureGradientBarPer100m = 1.08f + pressureBoost,
                FractureGradientSG = 1.62f + fractureBoost,
                Porosity01 = 0.31f,
                PermeabilityMd = 1.4f,
                GammaRayApi = 112f
            });
            model.zones.Add(new LithologyZone
            {
                FromMeasuredDepth = 320f,
                ToMeasuredDepth = 760f,
                Lithology = LithologyType.Sandstone,
                RockStrengthMpa = 58f + strengthBoost,
                Abrasiveness01 = 0.7f,
                Instability01 = 0.24f,
                Stickiness01 = 0.2f,
                PorePressureGradientBarPer100m = 1.0f + pressureBoost,
                FractureGradientSG = 1.67f + fractureBoost,
                Porosity01 = 0.22f,
                PermeabilityMd = 120f,
                GammaRayApi = 48f
            });
            model.zones.Add(new LithologyZone
            {
                FromMeasuredDepth = 760f,
                ToMeasuredDepth = 1120f,
                Lithology = LithologyType.Limestone,
                RockStrengthMpa = 86f + strengthBoost,
                Abrasiveness01 = 0.82f,
                Instability01 = 0.18f,
                Stickiness01 = 0.12f,
                PorePressureGradientBarPer100m = 0.94f + pressureBoost,
                FractureGradientSG = 1.78f + fractureBoost,
                Porosity01 = 0.12f,
                PermeabilityMd = 28f,
                GammaRayApi = 22f
            });
            model.zones.Add(new LithologyZone
            {
                FromMeasuredDepth = 1120f,
                ToMeasuredDepth = 2000f,
                Lithology = LithologyType.Shale,
                RockStrengthMpa = 42f + strengthBoost,
                Abrasiveness01 = 0.35f,
                Instability01 = Mathf.Clamp01(0.76f + shaleInstabilityBoost),
                Stickiness01 = 0.78f,
                PorePressureGradientBarPer100m = 1.22f + pressureBoost,
                FractureGradientSG = 1.58f + fractureBoost,
                Porosity01 = 0.27f,
                PermeabilityMd = 0.8f,
                GammaRayApi = 138f
            });
            model.zones.Add(new LithologyZone
            {
                FromMeasuredDepth = 2000f,
                ToMeasuredDepth = 2900f,
                Lithology = LithologyType.Sandstone,
                RockStrengthMpa = 72f + strengthBoost,
                Abrasiveness01 = 0.68f,
                Instability01 = 0.34f,
                Stickiness01 = 0.24f,
                PorePressureGradientBarPer100m = 1.28f + pressureBoost,
                FractureGradientSG = 1.68f + fractureBoost,
                Porosity01 = 0.19f,
                PermeabilityMd = 85f,
                GammaRayApi = 62f
            });
            model.zones.Add(new LithologyZone
            {
                FromMeasuredDepth = 2900f,
                ToMeasuredDepth = 3850f,
                Lithology = LithologyType.Dolomite,
                RockStrengthMpa = 108f + strengthBoost,
                Abrasiveness01 = 0.86f,
                Instability01 = 0.22f,
                Stickiness01 = 0.1f,
                PorePressureGradientBarPer100m = 1.16f + pressureBoost,
                FractureGradientSG = 1.84f + fractureBoost,
                Porosity01 = 0.1f,
                PermeabilityMd = 18f,
                GammaRayApi = 28f
            });
            model.zones.Add(new LithologyZone
            {
                FromMeasuredDepth = 3850f,
                ToMeasuredDepth = 5000f,
                Lithology = LithologyType.Salt,
                RockStrengthMpa = 46f + strengthBoost * 0.3f,
                Abrasiveness01 = 0.18f,
                Instability01 = 0.52f,
                Stickiness01 = 0.72f,
                PorePressureGradientBarPer100m = 1.34f + pressureBoost,
                FractureGradientSG = 1.74f + fractureBoost,
                Porosity01 = 0.04f,
                PermeabilityMd = 0.2f,
                GammaRayApi = 18f
            });
            model.zones.Add(new LithologyZone
            {
                FromMeasuredDepth = 5000f,
                ToMeasuredDepth = 8000f,
                Lithology = LithologyType.Basement,
                RockStrengthMpa = 132f + strengthBoost,
                Abrasiveness01 = 0.92f,
                Instability01 = 0.2f,
                Stickiness01 = 0.08f,
                PorePressureGradientBarPer100m = 1.2f + pressureBoost,
                FractureGradientSG = 1.92f + fractureBoost,
                Porosity01 = 0.03f,
                PermeabilityMd = 1.2f,
                GammaRayApi = 42f
            });

            return model;
        }

        private static LithologyZone CreateDefaultZone()
        {
            return new LithologyZone
            {
                FromMeasuredDepth = 0f,
                ToMeasuredDepth = 10000f,
                Lithology = LithologyType.Shale,
                RockStrengthMpa = 45f,
                Abrasiveness01 = 0.35f,
                Instability01 = 0.45f,
                Stickiness01 = 0.45f,
                PorePressureGradientBarPer100m = 1.05f,
                FractureGradientSG = 1.62f,
                Porosity01 = 0.25f,
                PermeabilityMd = 2f,
                GammaRayApi = 100f
            };
        }
    }
}
