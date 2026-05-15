using SCADASim.Core;
using SCADASim.Crew;
using SCADASim.Physics;
using UnityEngine;

namespace SCADASim.AI
{
    public sealed class AICrewAdvisorModule : MonoBehaviour
    {
        [SerializeField] private DrillingModel drillingModel;
        [SerializeField] private CrewManager crewManager;
        [SerializeField] private SimulationEventChannel eventChannel;
        [SerializeField] private float analysisIntervalSeconds = 1.2f;
        [SerializeField] private float alertCooldownSeconds = 7f;
        [SerializeField] private float routineCommentIntervalSeconds = 18f;

        private float nextAnalysisTime;
        private float lastAlertTime = -999f;
        private float lastRoutineCommentTime = -999f;

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
            CrewInfluence crew = crewManager != null ? crewManager.GetInfluence() : default;
            float flowIn = Mathf.Max(0.1f, state.FlowRateLps);
            float flowBalancePercent = (state.FlowOutLps - state.FlowRateLps) / flowIn * 100f;
            float vibrationG = state.Vibration.LowFrequencyEnergy * 3.5f;

            if (CanAlert() && crew.Fatigue > 0.68f && crew.ProcedureDiscipline01 < 0.58f)
            {
                RaiseCrewAdvice(
                    AlertSeverity.Warning,
                    "ИИ-бригадир: усталость смены",
                    $"Бригада реагирует медленнее: усталость {crew.Fatigue * 100f:0}%, дисциплина {crew.ProcedureDiscipline01 * 100f:0}%.",
                    "Провести короткий чек-лист, не менять сразу несколько параметров и держать скорость времени x1 на критическом интервале.",
                    state.MeasuredDepth);
                return;
            }

            if (CanAlert() && state.GasUnitsPercent > 3.2f && flowBalancePercent > 1.5f)
            {
                RaiseCrewAdvice(
                    AlertSeverity.Critical,
                    "ИИ-бригадир: газ на выходе",
                    $"Газ {state.GasUnitsPercent:0.0}% и выход выше входа на {flowBalancePercent:0.0}%. Бригада должна подтвердить емкости.",
                    "Поручить раствору контроль дегазатора, бурильщику не разгонять обороты, штуцер менять только малыми шагами.",
                    state.MeasuredDepth);
                return;
            }

            if (CanAlert() && state.CuttingsTransportEfficiency01 < 0.56f && state.InclinationDegrees > 45f)
            {
                RaiseCrewAdvice(
                    AlertSeverity.Warning,
                    "ИИ-бригадир: шламовая постель",
                    $"Вынос шлама {state.CuttingsTransportEfficiency01 * 100f:0}% в наклонном интервале. Момент может начать расти рывками.",
                    "Отправить бригаду на контроль сит, снизить нагрузку и выполнить промывку перед ростом расхода.",
                    state.MeasuredDepth);
                return;
            }

            if (CanAlert() && vibrationG > 2.3f && state.SurfaceTorqueKnM > 28f)
            {
                RaiseCrewAdvice(
                    AlertSeverity.Warning,
                    "ИИ-бригадир: момент и вибрация",
                    $"Момент {state.SurfaceTorqueKnM:0.0} кНм при вибрации {vibrationG:0.0} g. Похоже на stick-slip или перегрузку привода.",
                    "Бурильщику снизить обороты на один шаг, ННБ подтвердить toolface, нагрузку возвращать только после затухания вибрации.",
                    state.MeasuredDepth);
                return;
            }

            if (CanAlert() && state.StandpipePressureBar > 240f && crew.MaintenanceReadiness01 < 0.58f)
            {
                RaiseCrewAdvice(
                    AlertSeverity.Warning,
                    "ИИ-бригадир: насосы",
                    $"Давление насоса {state.StandpipePressureBar:0} бар, готовность оборудования {crew.MaintenanceReadiness01 * 100f:0}%.",
                    "Механику проверить насосный блок, раствору сверить вход/выход, расход менять ступенями не больше 100 л/мин.",
                    state.MeasuredDepth);
                return;
            }

            if (Time.time - lastRoutineCommentTime > routineCommentIntervalSeconds && UnityEngine.Random.value < 0.36f)
            {
                lastRoutineCommentTime = Time.time;
                RaiseRoutineCrewComment(state, crew, flowBalancePercent, vibrationG);
            }
        }

        private bool CanAlert()
        {
            return Time.time - lastAlertTime >= alertCooldownSeconds;
        }

        private void RaiseRoutineCrewComment(
            DrillingState state,
            CrewInfluence crew,
            float flowBalancePercent,
            float vibrationG)
        {
            int variant = UnityEngine.Random.Range(0, 4);
            switch (variant)
            {
                case 0:
                    RaiseCrewAdvice(
                        AlertSeverity.Advisory,
                        "ИИ-бригадир: сводка бурильщика",
                        $"Обороты {state.Rpm:0}, нагрузка {state.WeightOnBitTonnes:0.0} т, момент {state.SurfaceTorqueKnM:0.0} кНм.",
                        "Режим держать плавно: один параметр за раз, затем ждать реакцию давления и вибрации.",
                        state.MeasuredDepth,
                        false);
                    break;

                case 1:
                    RaiseCrewAdvice(
                        AlertSeverity.Advisory,
                        "ИИ-бригадир: раствор",
                        $"Баланс расхода {flowBalancePercent:+0.0;-0.0;0.0}%, ECD {state.EquivalentCirculatingDensitySG:0.00} SG, газ {state.GasUnitsPercent:0.0}%.",
                        "Растворщик держит контроль емкостей; при уходе баланса за 4% сразу сверить вход и выход.",
                        state.MeasuredDepth,
                        false);
                    break;

                case 2:
                    RaiseCrewAdvice(
                        AlertSeverity.Advisory,
                        "ИИ-бригадир: ННБ",
                        $"Зенит {state.InclinationDegrees:0.0}°, азимут {state.AzimuthDegrees:0}°, dogleg {state.DoglegSeverityDegPer30m:0.0}°/30 м.",
                        "Если момент начнет расти, запросить контрольный замер и не форсировать набор угла.",
                        state.MeasuredDepth,
                        false);
                    break;

                default:
                    RaiseCrewAdvice(
                        AlertSeverity.Advisory,
                        "ИИ-бригадир: состояние смены",
                        $"Усталость {crew.Fatigue * 100f:0}%, координация {crew.ShiftCoordination01 * 100f:0}%, вибрация {vibrationG:0.0} g.",
                        "Смена стабильна, но при сложном интервале лучше заранее выполнить инструктаж и проверку вышки.",
                        state.MeasuredDepth,
                        false);
                    break;
            }
        }

        private void RaiseCrewAdvice(
            AlertSeverity severity,
            string title,
            string message,
            string action,
            float measuredDepth,
            bool countAsAlert = true)
        {
            if (countAsAlert)
            {
                lastAlertTime = Time.time;
                lastRoutineCommentTime = Time.time;
            }

            AIRecommendation recommendation = new AIRecommendation(
                severity,
                title,
                message,
                action,
                measuredDepth);

            eventChannel?.Raise(
                SimulationEventType.CrewAIAlert,
                severity,
                $"{title}: {action}",
                recommendation);
        }
    }
}
