using System.Collections.Generic;
using SCADASim.Core;
using SCADASim.Crew;
using SCADASim.Physics;
using UnityEngine;

namespace SCADASim.AI
{
    public sealed class EdgeAIModule : MonoBehaviour
    {
        [SerializeField] private DrillingModel drillingModel;
        [SerializeField] private CrewManager crewManager;
        [SerializeField] private SimulationEventChannel eventChannel;
        [SerializeField] private float analysisIntervalSeconds = 0.5f;
        [SerializeField] private float alertCooldownSeconds = 8f;

        private readonly Queue<PressureSample> pressureHistory = new Queue<PressureSample>();
        private float nextAnalysisTime;
        private float lastStickSlipAlertTime = -999f;
        private float lastPackOffAlertTime = -999f;
        private float lastInstabilityAlertTime = -999f;
        private float lastKickAlertTime = -999f;
        private float lastLossAlertTime = -999f;
        private float lastCleaningAlertTime = -999f;
        private float lastBitWearAlertTime = -999f;

        public void Configure(
            DrillingModel model,
            CrewManager crew,
            SimulationEventChannel channel)
        {
            drillingModel = model;
            crewManager = crew;
            eventChannel = channel;
        }

        private void Update()
        {
            if (drillingModel == null || Time.time < nextAnalysisTime)
            {
                return;
            }

            nextAnalysisTime = Time.time + analysisIntervalSeconds;
            Analyze(drillingModel.CurrentState);
        }

        private void Analyze(DrillingState state)
        {
            pressureHistory.Enqueue(new PressureSample
            {
                Time = Time.time,
                PressureBar = state.StandpipePressureBar
            });

            while (pressureHistory.Count > 0 && Time.time - pressureHistory.Peek().Time > 12f)
            {
                pressureHistory.Dequeue();
            }

            float pressureTrendBarPerSecond = CalculatePressureTrend();
            CrewInfluence crew = crewManager != null ? crewManager.GetInfluence() : default;

            bool horizontalHole = state.InclinationDegrees > 78f;
            bool highFatigue = crew.Fatigue > 0.62f;
            bool lowFrequencyVibration = state.Vibration.LowFrequencyEnergy > 0.58f;

            if (horizontalHole && highFatigue && lowFrequencyVibration && CanAlert(lastStickSlipAlertTime))
            {
                lastStickSlipAlertTime = Time.time;
                RaiseRecommendation(new AIRecommendation(
                    AlertSeverity.Critical,
                    "Риск автоколебаний колонны",
                    "Рост низкочастотной вибрации в горизонтальном участке.",
                    "Снизить обороты на 10-15% и удерживать нагрузку без резких изменений.",
                    state.MeasuredDepth));
            }

            if (pressureTrendBarPerSecond > 0.42f && state.StuckPipeRisk01 > 0.52f && CanAlert(lastPackOffAlertTime))
            {
                lastPackOffAlertTime = Time.time;
                RaiseRecommendation(new AIRecommendation(
                    AlertSeverity.Warning,
                    "Сальникообразование",
                    "Давление насоса растет вместе с сопротивлением движению и риском прихвата.",
                    "Увеличить расход, снизить скорость проходки, контролировать стабилизацию давления насоса.",
                    state.MeasuredDepth));
            }

            if (state.BoreholeInstabilityRisk01 > 0.68f && crew.Fatigue > 0.5f && CanAlert(lastInstabilityAlertTime))
            {
                lastInstabilityAlertTime = Time.time;
                RaiseRecommendation(new AIRecommendation(
                    AlertSeverity.Warning,
                    "Риск осыпи стенок",
                    "Нестабильная литология повышает риск осложнения.",
                    "Удерживать нагрузку и обороты, стабилизировать расход, исключить резкие команды.",
                    state.MeasuredDepth));
            }

            if (state.KickRisk01 > 0.62f && CanAlert(lastKickAlertTime))
            {
                lastKickAlertTime = Time.time;
                RaiseRecommendation(new AIRecommendation(
                    AlertSeverity.Critical,
                    "Признаки притока",
                    "Пластовое давление приближается к забойному.",
                    "Остановить наращивание параметров, прикрыть штуцер, поднять плотность на 0.03-0.05 SG.",
                    state.MeasuredDepth));
            }

            if (state.LostCirculationRisk01 > 0.6f && CanAlert(lastLossAlertTime))
            {
                lastLossAlertTime = Time.time;
                RaiseRecommendation(new AIRecommendation(
                    AlertSeverity.Critical,
                    "Риск поглощения",
                    "Эквивалентная плотность приближается к давлению гидроразрыва.",
                    "Снизить расход на 100-200 л/мин, контролировать объем в емкостях, подготовить материал от поглощения.",
                    state.MeasuredDepth));
            }

            if (state.CuttingsTransportEfficiency01 < 0.5f && state.InclinationDegrees > 45f && CanAlert(lastCleaningAlertTime))
            {
                lastCleaningAlertTime = Time.time;
                RaiseRecommendation(new AIRecommendation(
                    AlertSeverity.Warning,
                    "Плохая очистка ствола",
                    "В наклонном интервале накапливается шламовая постель.",
                    "Снизить скорость проходки, увеличить расход на 100 л/мин и выполнить промывку до стабилизации момента.",
                    state.MeasuredDepth));
            }

            if (state.BitWear01 > 0.75f && CanAlert(lastBitWearAlertTime))
            {
                lastBitWearAlertTime = Time.time;
                RaiseRecommendation(new AIRecommendation(
                    AlertSeverity.Warning,
                    "Износ долота",
                    "Вибрация и момент указывают на снижение эффективности.",
                    "Ограничить нагрузку и обороты, подготовить смену долота, не форсировать скорость проходки.",
                    state.MeasuredDepth));
            }
        }

        private float CalculatePressureTrend()
        {
            if (pressureHistory.Count < 2)
            {
                return 0f;
            }

            PressureSample first = default;
            PressureSample last = default;
            bool firstAssigned = false;

            foreach (PressureSample sample in pressureHistory)
            {
                if (!firstAssigned)
                {
                    first = sample;
                    firstAssigned = true;
                }

                last = sample;
            }

            float deltaTime = Mathf.Max(0.01f, last.Time - first.Time);
            return (last.PressureBar - first.PressureBar) / deltaTime;
        }

        private bool CanAlert(float lastAlertTime)
        {
            return Time.time - lastAlertTime >= alertCooldownSeconds;
        }

        private void RaiseRecommendation(AIRecommendation recommendation)
        {
            eventChannel?.Raise(
                SimulationEventType.EdgeAIAlert,
                recommendation.Severity,
                $"{recommendation.Title}: {recommendation.RecommendedAction}",
                recommendation);
        }

        private struct PressureSample
        {
            public float Time;
            public float PressureBar;
        }
    }
}
