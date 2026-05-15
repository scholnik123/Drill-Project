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
                    "Угроза Stick-Slip",
                    "Низкочастотная вибрация растет в горизонтальном участке при высокой усталости бригады.",
                    "Рекомендуется ручное снижение оборотов на 10-15% и стабилизация нагрузки на долото.",
                    state.MeasuredDepth));
            }

            if (pressureTrendBarPerSecond > 0.42f && state.StuckPipeRisk01 > 0.52f && CanAlert(lastPackOffAlertTime))
            {
                lastPackOffAlertTime = Time.time;
                RaiseRecommendation(new AIRecommendation(
                    AlertSeverity.Warning,
                    "Сальникообразование",
                    "Давление насоса растет вместе с drag и риском прихвата.",
                    "Увеличить циркуляцию, снизить ROP и дождаться выравнивания тренда давления.",
                    state.MeasuredDepth));
            }

            if (state.BoreholeInstabilityRisk01 > 0.68f && crew.Fatigue > 0.5f && CanAlert(lastInstabilityAlertTime))
            {
                lastInstabilityAlertTime = Time.time;
                RaiseRecommendation(new AIRecommendation(
                    AlertSeverity.Warning,
                    "Осыпь стенок",
                    "Нестабильная литология и задержка реакции бригады повышают вероятность осыпи.",
                    "Держать параметры ровно, стабилизировать расход и избегать резких изменений нагрузки.",
                    state.MeasuredDepth));
            }

            if (state.KickRisk01 > 0.62f && CanAlert(lastKickAlertTime))
            {
                lastKickAlertTime = Time.time;
                RaiseRecommendation(new AIRecommendation(
                    AlertSeverity.Critical,
                    "Признаки притока",
                    "Пластовое давление догоняет забойное, расход на выходе и газопоказания растут.",
                    "Прикрыть штуцер, остановить наращивание параметров и поднять плотность раствора ступенью 0.03-0.05 SG.",
                    state.MeasuredDepth));
            }

            if (state.LostCirculationRisk01 > 0.6f && CanAlert(lastLossAlertTime))
            {
                lastLossAlertTime = Time.time;
                RaiseRecommendation(new AIRecommendation(
                    AlertSeverity.Critical,
                    "Риск поглощения",
                    "ECD приближается к давлению гидроразрыва, выходной расход падает относительно входного.",
                    "Снизить расход на 100-200 л/мин, контролировать объем в емкостях и подготовить LCM-пачку.",
                    state.MeasuredDepth));
            }

            if (state.CuttingsTransportEfficiency01 < 0.5f && state.InclinationDegrees > 45f && CanAlert(lastCleaningAlertTime))
            {
                lastCleaningAlertTime = Time.time;
                RaiseRecommendation(new AIRecommendation(
                    AlertSeverity.Warning,
                    "Плохая очистка ствола",
                    "В наклонном интервале накапливается шламовая постель, drag и давление могут расти лавинообразно.",
                    "Снизить ROP, увеличить расход на 100 л/мин и выполнить промывку до стабилизации момента.",
                    state.MeasuredDepth));
            }

            if (state.BitWear01 > 0.75f && CanAlert(lastBitWearAlertTime))
            {
                lastBitWearAlertTime = Time.time;
                RaiseRecommendation(new AIRecommendation(
                    AlertSeverity.Warning,
                    "Износ долота",
                    "Осевая вибрация и момент указывают на потерю режущей способности.",
                    "Подготовить смену долота, ограничить нагрузку и не компенсировать падение ROP чрезмерными оборотами.",
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
