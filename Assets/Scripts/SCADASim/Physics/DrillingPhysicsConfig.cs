using SCADASim.Core;
using UnityEngine;

namespace SCADASim.Physics
{
    [CreateAssetMenu(menuName = "SCADA Simulation/Physics/Drilling Physics Config")]
    public sealed class DrillingPhysicsConfig : ScriptableObject
    {
        [Header("Setpoint Limits")]
        public float MinRpm = 20f;
        public float MaxRpm = 220f;
        public float MinWeightOnBitTonnes = 1f;
        public float MaxWeightOnBitTonnes = 30f;
        public float MinFlowRateLps = 10f;
        public float MaxFlowRateLps = 85f;

        [Header("Base Drilling Response")]
        public float BaseRopMPerHour = 22f;
        public float PipeWeightTonnesPerMeter = 0.028f;
        public float BaseFrictionFactor = 0.24f;
        public float BaseBitTorqueKnM = 8f;
        public float BaseStandpipePressureBar = 55f;
        public float BaseHookLoadTonnes = 90f;

        [Header("Risk Response")]
        public float DoglegTorqueGain = 0.075f;
        public float InclinationDragGain = 0.85f;
        public float FatigueRiskGain = 0.16f;
        public float PressureTrendRiskGain = 0.035f;

        public void ApplyRuntimeConfig(SimulationRuntimeConfig config)
        {
            switch (config.Difficulty)
            {
                case DifficultyLevel.Training:
                    BaseRopMPerHour = 26f;
                    BaseFrictionFactor = 0.18f;
                    DoglegTorqueGain = 0.045f;
                    InclinationDragGain = 0.55f;
                    FatigueRiskGain = 0.08f;
                    PressureTrendRiskGain = 0.018f;
                    break;

                case DifficultyLevel.Easy:
                    BaseRopMPerHour = 23f;
                    BaseFrictionFactor = 0.22f;
                    DoglegTorqueGain = 0.065f;
                    InclinationDragGain = 0.75f;
                    FatigueRiskGain = 0.13f;
                    PressureTrendRiskGain = 0.028f;
                    break;

                case DifficultyLevel.Realistic:
                    BaseRopMPerHour = 20f;
                    BaseFrictionFactor = 0.27f;
                    DoglegTorqueGain = 0.085f;
                    InclinationDragGain = 0.95f;
                    FatigueRiskGain = 0.18f;
                    PressureTrendRiskGain = 0.042f;
                    break;

                case DifficultyLevel.Expert:
                    BaseRopMPerHour = 17f;
                    BaseFrictionFactor = 0.32f;
                    DoglegTorqueGain = 0.115f;
                    InclinationDragGain = 1.22f;
                    FatigueRiskGain = 0.26f;
                    PressureTrendRiskGain = 0.06f;
                    break;
            }

            switch (config.PhysicsPreset)
            {
                case PhysicsPreset.Stable:
                    BaseFrictionFactor *= 0.82f;
                    DoglegTorqueGain *= 0.82f;
                    InclinationDragGain *= 0.85f;
                    FatigueRiskGain *= 0.72f;
                    PressureTrendRiskGain *= 0.7f;
                    break;

                case PhysicsPreset.Field:
                    break;

                case PhysicsPreset.Harsh:
                    BaseFrictionFactor *= 1.18f;
                    DoglegTorqueGain *= 1.22f;
                    InclinationDragGain *= 1.2f;
                    FatigueRiskGain *= 1.3f;
                    PressureTrendRiskGain *= 1.35f;
                    break;
            }
        }
    }
}
