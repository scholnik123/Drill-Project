using System;

namespace SCADASim.Core
{
    [Serializable]
    public struct DrillingModelSaveData
    {
        public DrillingState State;
        public float BitBallingRiskBias;
        public float WallSloughingRiskBias;
        public float LostCirculationBias;
        public float GasInfluxBias;
        public float CommandRampEnergy;
        public float NextIncidentCheckTime;
        public float LastIncidentTime;
        public float NextSupervisorTaskTime;
        public float SupervisorCadenceMultiplier;
        public bool HasActiveSupervisorTask;
        public SupervisorTask ActiveSupervisorTask;
        public float ActiveSupervisorTaskStartedAt;
        public float ActiveSupervisorTaskStableSeconds;
        public float LastSupervisorTaskEvaluationClock;
        public float SupervisorClockSeconds;
        public int SupervisorTaskSequence;
        public int CompanyCredits;
        public float EconomyEarnAccumulator;
        public float EconomyCostAccumulator;
        public int PerformanceCredits;
        public int OperatingCostCredits;
        public int SuccessfulSupervisorTasks;
        public int FailedSupervisorTasks;
        public bool HasLastSupervisorTaskType;
        public SupervisorTaskType LastSupervisorTaskType;
        public bool HasOpeningTaskBeenIssued;
        public bool IsSimulating;
        public float SimulationSpeedMultiplier;
    }

    [Serializable]
    public struct CrewManagerSaveData
    {
        public float ExperienceLevel;
        public float Fatigue;
        public float Morale;
        public float ProcedureDiscipline01;
        public float SituationalAwareness01;
        public float MaintenanceReadiness01;
        public float ShiftCoordination01;
        public float ShiftDurationMinutes;
        public int OperationalDay;
        public int CurrentShiftIndex;
        public float ShiftClockSeconds;
    }

    [Serializable]
    public struct SimulationSessionSaveData
    {
        public int Version;
        public string SavedAt;
        public SimulationRuntimeConfig Config;
        public DrillingModelSaveData Drilling;
        public CrewManagerSaveData Crew;
        public int LocationIndex;
        public int ProfileIndex;
        public int DepthIndex;
        public int RegionIndex;
        public int DifficultyIndex;
        public int PhysicsIndex;
        public int CrewIndex;
        public int ShiftDurationIndex;
        public int SupervisorPaceIndex;
        public int StartSpeedIndex;
        public int MusicVolumeIndex;
        public int ResolutionIndex;
        public int TutorialIndex;
    }
}
