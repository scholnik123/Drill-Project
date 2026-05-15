using System;
using UnityEngine;

namespace SCADASim.Core
{
    public enum EnvironmentType
    {
        Onshore,
        Offshore
    }

    public enum WellboreProfileType
    {
        CustomSurvey,
        Vertical,
        JShape,
        SShape,
        Horizontal
    }

    public enum LithologyType
    {
        Shale,
        Sandstone,
        Limestone,
        Dolomite,
        Salt,
        Basement
    }

    public enum CrewIncidentType
    {
        MissedSetpoint,
        BitBalling,
        WallSloughing,
        DelayedResponse,
        WrongMudWeight,
        MissedFlowDrop,
        UnsafeRamp,
        ToolfaceDrift,
        PumpLag,
        MissedGasTrend,
        RadioMiscommunication
    }

    public enum CrewActionType
    {
        MudCheck,
        RigInspection,
        BitRunPlanning,
        DirectionalSurvey,
        HoleCleaning,
        ShiftBriefing,
        KickControl,
        LossControl,
        FreeStuckPipe,
        StickSlipMitigation,
        BackreamAndReam
    }

    public enum OperationalIncidentType
    {
        StickSlip,
        PackOff,
        BitBalling,
        WallSloughing,
        LostCirculation,
        Kick,
        DifferentialSticking,
        BitWearLimit,
        PoorHoleCleaning,
        GasCutMud,
        MwdSignalLoss,
        TopDriveOverload,
        PumpEfficiencyDrop,
        ShakerOverflow,
        SevereWeather,
        DrillStringWashout
    }

    public enum DrillingCommandType
    {
        SetRpm,
        SetWeightOnBit,
        SetFlowRate,
        SetMudWeight,
        SetChokeOpening,
        SetTopDriveTorqueLimit
    }

    public enum DifficultyLevel
    {
        Training,
        Easy,
        Realistic,
        Expert
    }

    public enum GeologyRegion
    {
        WestSiberia,
        VolgaUral,
        ArcticShelf,
        Caspian
    }

    public enum CrewPreset
    {
        FullSeven,
        StandardFour,
        ReducedThree,
        TraineeShift
    }

    public enum PhysicsPreset
    {
        Stable,
        Field,
        Harsh
    }

    public enum SupervisorTaskType
    {
        KickControl,
        LossControl,
        DirectionalDrag,
        HoleCleaning,
        ShaleStability,
        BitAssessment,
        PressureWindow,
        CrewHandover,
        PumpEfficiency,
        GasMonitoring,
        ToolfaceControl,
        PumpIntegrity,
        EquipmentInspection,
        WeatherResponse,
        MwdSurvey,
        TorqueSmoothing,
        ConnectionProcedure,
        ShiftPlan
    }

    [Serializable]
    public struct SimulationRuntimeConfig
    {
        public EnvironmentType EnvironmentType;
        public WellboreProfileType ProfileType;
        public GeologyRegion GeologyRegion;
        public DifficultyLevel Difficulty;
        public PhysicsPreset PhysicsPreset;
        public CrewPreset CrewPreset;
        public float StartMeasuredDepth;
        public float MaxMeasuredDepth;

        public static SimulationRuntimeConfig Default => new SimulationRuntimeConfig
        {
            EnvironmentType = EnvironmentType.Onshore,
            ProfileType = WellboreProfileType.Vertical,
            GeologyRegion = GeologyRegion.WestSiberia,
            Difficulty = DifficultyLevel.Training,
            PhysicsPreset = PhysicsPreset.Stable,
            CrewPreset = CrewPreset.FullSeven,
            StartMeasuredDepth = 0f,
            MaxMeasuredDepth = 1500f
        };
    }

    [Serializable]
    public struct DrillingCommand
    {
        public DrillingCommandType Type;
        public float TargetValue;
        public string Source;

        public DrillingCommand(DrillingCommandType type, float targetValue, string source = "Player")
        {
            Type = type;
            TargetValue = targetValue;
            Source = source;
        }
    }

    [Serializable]
    public struct CrewInfluence
    {
        public float ReactionDelaySeconds;
        public float MistakeChancePerMinute;
        public float OperationalEfficiency;
        public float Fatigue;
        public float Morale;
        public float ExperienceLevel;
        public float ProcedureDiscipline01;
        public float SituationalAwareness01;
        public float MaintenanceReadiness01;
        public float ShiftCoordination01;
    }

    [Serializable]
    public struct CrewIncident
    {
        public CrewIncidentType Type;
        public AlertSeverity Severity;
        public string Message;
        public float MeasuredDepth;
        public float Probability;
    }

    [Serializable]
    public struct CrewActionReport
    {
        public CrewActionType Type;
        public string Role;
        public string Title;
        public string Message;
        public string Effect;
        public float Quality01;
        public float FatigueAfter01;

        public CrewActionReport(
            CrewActionType type,
            string role,
            string title,
            string message,
            string effect,
            float quality01,
            float fatigueAfter01)
        {
            Type = type;
            Role = role;
            Title = title;
            Message = message;
            Effect = effect;
            Quality01 = quality01;
            FatigueAfter01 = fatigueAfter01;
        }
    }

    [Serializable]
    public struct VibrationState
    {
        public Vector3 AccelerationG;
        public float LowFrequencyEnergy;
        public float LateralEnergy;
        public float AxialEnergy;
    }

    [Serializable]
    public struct DrillingState
    {
        public float MeasuredDepth;
        public float InclinationDegrees;
        public float AzimuthDegrees;
        public float DoglegSeverityDegPer30m;
        public LithologyType Lithology;
        public float Rpm;
        public float WeightOnBitTonnes;
        public float FlowRateLps;
        public float RopMPerHour;
        public float SurfaceTorqueKnM;
        public float DragTonnes;
        public float StandpipePressureBar;
        public float HookLoadTonnes;
        public float StuckPipeRisk01;
        public float BoreholeInstabilityRisk01;
        public float TrueVerticalDepth;
        public float MudWeightSG;
        public float EquivalentCirculatingDensitySG;
        public float PorePressureMPa;
        public float BottomHolePressureMPa;
        public float FracturePressureMPa;
        public float CuttingsTransportEfficiency01;
        public float BitWear01;
        public float FlowOutLps;
        public float ChokeOpening01;
        public float GasUnitsPercent;
        public float LostCirculationRisk01;
        public float KickRisk01;
        public float NonProductiveTimeMinutes;
        public VibrationState Vibration;
    }

    [Serializable]
    public struct OperationalIncident
    {
        public OperationalIncidentType Type;
        public AlertSeverity Severity;
        public string Title;
        public string Message;
        public string Consequence;
        public float MeasuredDepth;

        public OperationalIncident(
            OperationalIncidentType type,
            AlertSeverity severity,
            string title,
            string message,
            string consequence,
            float measuredDepth)
        {
            Type = type;
            Severity = severity;
            Title = title;
            Message = message;
            Consequence = consequence;
            MeasuredDepth = measuredDepth;
        }
    }

    [Serializable]
    public struct SupervisorTask
    {
        public SupervisorTaskType Type;
        public string Title;
        public string Objective;
        public string SuccessCriteria;
        public string SensorFocus;
        public string ControlHints;
        public float DeadlineMinutes;
        public float MinimumHoldSeconds;
        public int RewardCredits;
        public int FailurePenaltyCredits;
        public float BaselineFlowInLps;
        public float BaselineFlowOutLps;
        public float BaselineStandpipePressureBar;
        public float BaselineTorqueKnM;
        public float BaselineDragTonnes;

        public SupervisorTask(
            SupervisorTaskType type,
            string title,
            string objectiveText,
            string successCriteria,
            string sensorFocus,
            string controlHints,
            float deadlineMinutes,
            float minimumHoldSeconds,
            int rewardCredits,
            int failurePenaltyCredits,
            DrillingState baseline)
        {
            Type = type;
            Title = title;
            Objective = objectiveText;
            SuccessCriteria = successCriteria;
            SensorFocus = sensorFocus;
            ControlHints = controlHints;
            DeadlineMinutes = deadlineMinutes;
            MinimumHoldSeconds = minimumHoldSeconds;
            RewardCredits = rewardCredits;
            FailurePenaltyCredits = failurePenaltyCredits;
            BaselineFlowInLps = baseline.FlowRateLps;
            BaselineFlowOutLps = baseline.FlowOutLps;
            BaselineStandpipePressureBar = baseline.StandpipePressureBar;
            BaselineTorqueKnM = baseline.SurfaceTorqueKnM;
            BaselineDragTonnes = baseline.DragTonnes;
        }
    }

    [Serializable]
    public struct SupervisorTaskResult
    {
        public SupervisorTask Task;
        public bool Succeeded;
        public string Summary;
        public int CreditsDelta;
        public int TotalCredits;

        public SupervisorTaskResult(
            SupervisorTask task,
            bool succeeded,
            string summary,
            int creditsDelta,
            int totalCredits)
        {
            Task = task;
            Succeeded = succeeded;
            Summary = summary;
            CreditsDelta = creditsDelta;
            TotalCredits = totalCredits;
        }
    }

    [Serializable]
    public struct AIRecommendation
    {
        public AlertSeverity Severity;
        public string Title;
        public string Message;
        public string RecommendedAction;
        public float MeasuredDepth;

        public AIRecommendation(
            AlertSeverity severity,
            string title,
            string message,
            string recommendedAction,
            float measuredDepth)
        {
            Severity = severity;
            Title = title;
            Message = message;
            RecommendedAction = recommendedAction;
            MeasuredDepth = measuredDepth;
        }
    }
}
