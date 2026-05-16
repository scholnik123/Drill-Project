using SCADASim.Core;
using UnityEngine;

namespace SCADASim.Procedures
{
    public enum FieldProcedureKind
    {
        None,
        KickKill,
        HoleCleaning
    }

    public struct FieldProcedureCompletion
    {
        public bool Completed;
        public CrewActionType ActionType;
        public float Quality01;
        public string RadioMessage;
    }

    public sealed class FieldProcedureManager
    {
        private FieldProcedureKind kind;
        private CrewActionType actionType;
        private DifficultyTuning tuning;
        private int stepIndex;
        private float stableSeconds;
        private float baselineFlowLps;
        private float baselineStandpipePressureBar;
        private float circulationCycleSeconds;

        public bool HasActive => kind != FieldProcedureKind.None;
        public string StatusText { get; private set; } = "ПРОЦЕДУРА: нет активного процесса.";

        public bool Start(CrewActionType requestedAction, DrillingState state, DifficultyTuning difficultyTuning, out string message)
        {
            if (HasActive)
            {
                message = "ПРОЦЕДУРА: уже идет процесс. Завершите текущие шаги перед запуском новой операции.";
                return false;
            }

            if (requestedAction != CrewActionType.KickControl && requestedAction != CrewActionType.HoleCleaning)
            {
                message = "ПРОЦЕДУРА: для этой операции многошаговый режим не требуется.";
                return false;
            }

            tuning = difficultyTuning;
            actionType = requestedAction;
            kind = requestedAction == CrewActionType.KickControl
                ? FieldProcedureKind.KickKill
                : FieldProcedureKind.HoleCleaning;
            stepIndex = 0;
            stableSeconds = 0f;
            baselineFlowLps = Mathf.Max(0.1f, state.FlowRateLps);
            baselineStandpipePressureBar = state.StandpipePressureBar;
            circulationCycleSeconds = Mathf.Clamp(state.MeasuredDepth / Mathf.Max(1f, state.FlowRateLps) * 0.42f, 28f, 92f) *
                                      Mathf.Max(0.7f, tuning.HoldTimeMultiplier);

            StatusText = BuildStatus(state, false);
            message = StatusText;
            return true;
        }

        public void Reset()
        {
            kind = FieldProcedureKind.None;
            actionType = default;
            stepIndex = 0;
            stableSeconds = 0f;
            baselineFlowLps = 0f;
            baselineStandpipePressureBar = 0f;
            circulationCycleSeconds = 0f;
            StatusText = "ПРОЦЕДУРА: нет активного процесса.";
        }

        public bool Tick(DrillingState state, float deltaSeconds, out FieldProcedureCompletion completion)
        {
            completion = default;
            if (!HasActive)
            {
                return false;
            }

            float requiredSeconds = GetRequiredSeconds();
            bool criteriaMet = AreStepCriteriaMet(state);
            stableSeconds = criteriaMet
                ? Mathf.Min(requiredSeconds, stableSeconds + Mathf.Max(0f, deltaSeconds))
                : 0f;

            if (stableSeconds >= requiredSeconds)
            {
                if (AdvanceStepOrComplete(state, out completion))
                {
                    Reset();
                    StatusText = completion.RadioMessage;
                    return true;
                }

                stableSeconds = 0f;
            }

            StatusText = BuildStatus(state, criteriaMet);
            return false;
        }

        private bool AdvanceStepOrComplete(DrillingState state, out FieldProcedureCompletion completion)
        {
            completion = default;
            stepIndex++;

            if ((kind == FieldProcedureKind.KickKill && stepIndex < 4) ||
                (kind == FieldProcedureKind.HoleCleaning && stepIndex < 5))
            {
                return false;
            }

            completion = new FieldProcedureCompletion
            {
                Completed = true,
                ActionType = actionType,
                Quality01 = CalculateCompletionQuality(state),
                RadioMessage = kind == FieldProcedureKind.KickKill
                    ? "ПРОЦЕДУРА: глушение завершено. Давления и выход раствора стабилизированы, эффект будет учтен моделью."
                    : "ПРОЦЕДУРА: промывка завершена. Цикл циркуляции пройден, вынос шлама подтвержден."
            };
            return true;
        }

        private float GetRequiredSeconds()
        {
            float hold = Mathf.Max(0.65f, tuning.HoldTimeMultiplier);
            if (kind == FieldProcedureKind.KickKill)
            {
                switch (stepIndex)
                {
                    case 0:
                        return 6f * hold;
                    case 1:
                        return 8f * hold;
                    case 2:
                        return 16f * hold;
                    default:
                        return 22f * hold;
                }
            }

            switch (stepIndex)
            {
                case 0:
                    return 4f * hold;
                case 1:
                    return 10f * hold;
                case 2:
                    return 12f * hold;
                case 3:
                    return circulationCycleSeconds;
                default:
                    return 18f * hold;
            }
        }

        private bool AreStepCriteriaMet(DrillingState state)
        {
            float corridor = Mathf.Max(0.35f, tuning.CorridorMultiplier);
            float flowBalancePercent = GetFlowBalancePercent(state);
            float pressureLowMargin = state.BottomHolePressureMPa - state.PorePressureMPa;
            float pressureHighMargin = state.FracturePressureMPa - state.BottomHolePressureMPa;
            float sppRise = state.StandpipePressureBar - baselineStandpipePressureBar;
            float vibrationG = state.Vibration.LowFrequencyEnergy * 3.5f;
            float baselineFlowLpm = baselineFlowLps * 60f;
            float currentFlowLpm = state.FlowRateLps * 60f;

            if (kind == FieldProcedureKind.KickKill)
            {
                switch (stepIndex)
                {
                    case 0:
                        return state.Rpm <= 45f * corridor && state.WeightOnBitTonnes <= 4.5f * corridor;
                    case 1:
                        return state.ChokeOpening01 <= 0.38f * corridor &&
                               currentFlowLpm <= baselineFlowLpm + 80f * corridor;
                    case 2:
                        return pressureLowMargin > 0.25f / corridor &&
                               pressureHighMargin > 0.35f / corridor;
                    default:
                        return Mathf.Abs(flowBalancePercent) <= 3.5f * corridor &&
                               state.GasUnitsPercent < 6f * corridor &&
                               pressureLowMargin > 0.18f / corridor &&
                               pressureHighMargin > 0.25f / corridor;
                }
            }

            switch (stepIndex)
            {
                case 0:
                    return true;
                case 1:
                    return currentFlowLpm >= baselineFlowLpm + 90f / corridor &&
                           currentFlowLpm <= baselineFlowLpm + 240f * corridor &&
                           sppRise < 18f * corridor;
                case 2:
                    return currentFlowLpm >= baselineFlowLpm + 190f / corridor &&
                           currentFlowLpm <= baselineFlowLpm + 420f * corridor &&
                           sppRise < 24f * corridor;
                case 3:
                    return state.CuttingsTransportEfficiency01 > 1f - (1f - 0.62f) * corridor &&
                           sppRise < 24f * corridor &&
                           Mathf.Abs(flowBalancePercent) < 8f * corridor;
                default:
                    return state.CuttingsTransportEfficiency01 > 1f - (1f - 0.70f) * corridor &&
                           vibrationG < 2.2f * corridor &&
                           Mathf.Abs(flowBalancePercent) < 6f * corridor;
            }
        }

        private string BuildStatus(DrillingState state, bool criteriaMet)
        {
            float requiredSeconds = GetRequiredSeconds();
            string progress = $"{stableSeconds:0}/{requiredSeconds:0} сек";
            string stateText = criteriaMet ? "держать" : "добиться";

            if (kind == FieldProcedureKind.KickKill)
            {
                switch (stepIndex)
                {
                    case 0:
                        return $"ПРОЦЕДУРА: ПРИТОК 1/4 - ручной стоп. Нужно: RPM <=45, WOB <=4.5 т; {stateText} {progress}.";
                    case 1:
                        return $"ПРОЦЕДУРА: ПРИТОК 2/4 - закрытие превентора. Нужно: штуцер <=38%, расход без разгона; {stateText} {progress}.";
                    case 2:
                        return $"ПРОЦЕДУРА: ПРИТОК 3/4 - выравнивание давлений. Нужно: забойное выше пластового и ниже ГРП; {stateText} {progress}.";
                    default:
                        return $"ПРОЦЕДУРА: ПРИТОК 4/4 - штуцер ступенями. Нужно: баланс +/-3.5%, газ <6%, окно давлений; {stateText} {progress}.";
                }
            }

            switch (stepIndex)
            {
                case 0:
                    return $"ПРОЦЕДУРА: ПРОМЫВКА 1/5 - расчет цикла циркуляции {circulationCycleSeconds:0} сек; {stateText} {progress}.";
                case 1:
                    return $"ПРОЦЕДУРА: ПРОМЫВКА 2/5 - насосы ступень +100-240 л/мин, давление +<18 бар; {stateText} {progress}.";
                case 2:
                    return $"ПРОЦЕДУРА: ПРОМЫВКА 3/5 - насосы ступень +200-420 л/мин, давление +<24 бар; {stateText} {progress}.";
                case 3:
                    return $"ПРОЦЕДУРА: ПРОМЫВКА 4/5 - полный цикл циркуляции, вынос шлама >62%, баланс +/-8%; {stateText} {progress}.";
                default:
                    return $"ПРОЦЕДУРА: ПРОМЫВКА 5/5 - стабилизация: вынос >70%, вибрация <2.2 g, баланс +/-6%; {stateText} {progress}.";
            }
        }

        private float CalculateCompletionQuality(DrillingState state)
        {
            float flowBalancePenalty = Mathf.Clamp01(Mathf.Abs(GetFlowBalancePercent(state)) / 12f);
            if (kind == FieldProcedureKind.KickKill)
            {
                float gasPenalty = Mathf.Clamp01(state.GasUnitsPercent / 10f);
                float riskPenalty = Mathf.Clamp01(state.KickRisk01);
                return Mathf.Clamp01(0.95f - flowBalancePenalty * 0.18f - gasPenalty * 0.22f - riskPenalty * 0.22f);
            }

            float cleaningBonus = Mathf.Clamp01((state.CuttingsTransportEfficiency01 - 0.55f) / 0.35f);
            float pressurePenalty = Mathf.Clamp01((state.StandpipePressureBar - baselineStandpipePressureBar) / 35f);
            return Mathf.Clamp01(0.46f + cleaningBonus * 0.42f - flowBalancePenalty * 0.12f - pressurePenalty * 0.14f);
        }

        private static float GetFlowBalancePercent(DrillingState state)
        {
            float flowIn = Mathf.Max(0.1f, state.FlowRateLps);
            return (state.FlowOutLps - state.FlowRateLps) / flowIn * 100f;
        }
    }
}
