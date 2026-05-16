using System;
using SCADASim.Core;
using SCADASim.Crew;
using SCADASim.Trajectory;
using System.Collections.Generic;
using UnityEngine;

namespace SCADASim.Physics
{
    public sealed class DrillingModel : MonoBehaviour
    {
        private const float MpaPerMeterPerSG = 0.00980665f;

        [Header("Dependencies")]
        [SerializeField] private WellboreProceduralMesh wellbore;
        [SerializeField] private GeologyModel geologyModel;
        [SerializeField] private DrillingPhysicsConfig physicsConfig;
        [SerializeField] private CrewManager crewManager;
        [SerializeField] private SimulationEventChannel eventChannel;

        [Header("Runtime")]
        [SerializeField] private float simulationSpeed = 35f;
        [SerializeField] private float simulationSpeedMultiplier = 1f;
        [SerializeField] private bool simulateOnStart = true;

        private DrillingState state;
        private float bitBallingRiskBias;
        private float wallSloughingRiskBias;
        private float lostCirculationBias;
        private float gasInfluxBias;
        private float commandRampEnergy;
        private float nextIncidentCheckTime;
        private float lastIncidentTime = -999f;
        private float nextSupervisorTaskTime = 2f;
        private float supervisorCadenceMultiplier = 1f;
        private bool hasActiveSupervisorTask;
        private SupervisorTask activeSupervisorTask;
        private float activeSupervisorTaskStartedAt;
        private float activeSupervisorTaskStableSeconds;
        private float lastSupervisorTaskEvaluationClock;
        private float supervisorClockSeconds;
        private int supervisorTaskSequence;
        private int companyCredits;
        private float economyEarnAccumulator;
        private float economyCostAccumulator;
        private int performanceCredits;
        private int operatingCostCredits;
        private int successfulSupervisorTasks;
        private int failedSupervisorTasks;
        private bool simulationStateWasSetExplicitly;
        private bool hasLastSupervisorTaskType;
        private SupervisorTaskType lastSupervisorTaskType;
        private bool hasOpeningTaskBeenIssued;
        private SimulationRuntimeConfig runtimeConfig = SimulationRuntimeConfig.Default;
        private CrewManager subscribedCrew;

        public event Action<DrillingState> StateUpdated;

        public DrillingState CurrentState => state;
        public bool IsSimulating { get; private set; }
        public float SimulationSpeedMultiplier => simulationSpeedMultiplier;
        public int CompanyCredits => companyCredits;
        public int PerformanceCredits => performanceCredits;
        public int OperatingCostCredits => operatingCostCredits;
        public int SuccessfulSupervisorTasks => successfulSupervisorTasks;
        public int FailedSupervisorTasks => failedSupervisorTasks;
        public float ActiveSupervisorTaskStableSeconds => activeSupervisorTaskStableSeconds;
        public float ActiveSupervisorTaskRemainingMinutes
        {
            get
            {
                if (!hasActiveSupervisorTask)
                {
                    return 0f;
                }

                float elapsedSeconds = Mathf.Max(0f, supervisorClockSeconds - activeSupervisorTaskStartedAt);
                return Mathf.Max(0f, activeSupervisorTask.DeadlineMinutes - elapsedSeconds / 60f);
            }
        }

        public void Configure(
            WellboreProceduralMesh wellboreMesh,
            GeologyModel geology,
            DrillingPhysicsConfig config,
            CrewManager crew,
            SimulationEventChannel channel)
        {
            wellbore = wellboreMesh;
            geologyModel = geology;
            physicsConfig = config;
            eventChannel = channel;
            SetCrewManager(crew);
            InitializeState();
        }

        public void ApplyRuntimeConfig(SimulationRuntimeConfig config, GeologyModel geology)
        {
            runtimeConfig = config;
            geologyModel = geology;
            physicsConfig?.ApplyRuntimeConfig(config);
            InitializeState();
            state.MeasuredDepth = Mathf.Max(0f, config.StartMeasuredDepth);
            ResetSupervisorTasks(2f);
            IsSimulating = true;
        }

        public void SetSimulating(bool isSimulating)
        {
            IsSimulating = isSimulating;
            simulateOnStart = isSimulating;
            simulationStateWasSetExplicitly = true;
        }

        public void SetSimulationSpeedMultiplier(float multiplier)
        {
            simulationSpeedMultiplier = Mathf.Clamp(multiplier, 0.25f, 20f);
        }

        public void SetSupervisorCadenceMultiplier(float multiplier)
        {
            supervisorCadenceMultiplier = Mathf.Clamp(multiplier, 0.55f, 1.8f);
        }

        public DrillingModelSaveData CaptureSaveData()
        {
            return new DrillingModelSaveData
            {
                State = state,
                BitBallingRiskBias = bitBallingRiskBias,
                WallSloughingRiskBias = wallSloughingRiskBias,
                LostCirculationBias = lostCirculationBias,
                GasInfluxBias = gasInfluxBias,
                CommandRampEnergy = commandRampEnergy,
                NextIncidentCheckTime = nextIncidentCheckTime,
                LastIncidentTime = lastIncidentTime,
                NextSupervisorTaskTime = nextSupervisorTaskTime,
                SupervisorCadenceMultiplier = supervisorCadenceMultiplier,
                HasActiveSupervisorTask = hasActiveSupervisorTask,
                ActiveSupervisorTask = activeSupervisorTask,
                ActiveSupervisorTaskStartedAt = activeSupervisorTaskStartedAt,
                ActiveSupervisorTaskStableSeconds = activeSupervisorTaskStableSeconds,
                LastSupervisorTaskEvaluationClock = lastSupervisorTaskEvaluationClock,
                SupervisorClockSeconds = supervisorClockSeconds,
                SupervisorTaskSequence = supervisorTaskSequence,
                CompanyCredits = companyCredits,
                EconomyEarnAccumulator = economyEarnAccumulator,
                EconomyCostAccumulator = economyCostAccumulator,
                PerformanceCredits = performanceCredits,
                OperatingCostCredits = operatingCostCredits,
                SuccessfulSupervisorTasks = successfulSupervisorTasks,
                FailedSupervisorTasks = failedSupervisorTasks,
                HasLastSupervisorTaskType = hasLastSupervisorTaskType,
                LastSupervisorTaskType = lastSupervisorTaskType,
                HasOpeningTaskBeenIssued = hasOpeningTaskBeenIssued,
                IsSimulating = IsSimulating,
                SimulationSpeedMultiplier = simulationSpeedMultiplier
            };
        }

        public void RestoreSaveData(DrillingModelSaveData saveData)
        {
            state = saveData.State;
            bitBallingRiskBias = saveData.BitBallingRiskBias;
            wallSloughingRiskBias = saveData.WallSloughingRiskBias;
            lostCirculationBias = saveData.LostCirculationBias;
            gasInfluxBias = saveData.GasInfluxBias;
            commandRampEnergy = saveData.CommandRampEnergy;
            nextIncidentCheckTime = Mathf.Max(Time.time + 3f, saveData.NextIncidentCheckTime);
            lastIncidentTime = saveData.LastIncidentTime;
            nextSupervisorTaskTime = saveData.NextSupervisorTaskTime;
            supervisorCadenceMultiplier = Mathf.Clamp(saveData.SupervisorCadenceMultiplier, 0.55f, 1.8f);
            hasActiveSupervisorTask = saveData.HasActiveSupervisorTask;
            activeSupervisorTask = saveData.ActiveSupervisorTask;
            activeSupervisorTaskStartedAt = saveData.ActiveSupervisorTaskStartedAt;
            activeSupervisorTaskStableSeconds = Mathf.Max(0f, saveData.ActiveSupervisorTaskStableSeconds);
            lastSupervisorTaskEvaluationClock = Mathf.Max(0f, saveData.LastSupervisorTaskEvaluationClock);
            supervisorClockSeconds = Mathf.Max(0f, saveData.SupervisorClockSeconds);
            supervisorTaskSequence = Mathf.Max(0, saveData.SupervisorTaskSequence);
            companyCredits = saveData.CompanyCredits;
            economyEarnAccumulator = Mathf.Max(0f, saveData.EconomyEarnAccumulator);
            economyCostAccumulator = Mathf.Max(0f, saveData.EconomyCostAccumulator);
            performanceCredits = saveData.PerformanceCredits;
            operatingCostCredits = saveData.OperatingCostCredits;
            successfulSupervisorTasks = Mathf.Max(0, saveData.SuccessfulSupervisorTasks);
            failedSupervisorTasks = Mathf.Max(0, saveData.FailedSupervisorTasks);
            hasLastSupervisorTaskType = saveData.HasLastSupervisorTaskType;
            lastSupervisorTaskType = saveData.LastSupervisorTaskType;
            hasOpeningTaskBeenIssued = saveData.HasOpeningTaskBeenIssued;
            simulationSpeedMultiplier = Mathf.Clamp(saveData.SimulationSpeedMultiplier, 0.25f, 20f);
            SetSimulating(saveData.IsSimulating);

            StateUpdated?.Invoke(state);
            if (hasActiveSupervisorTask)
            {
                eventChannel?.Raise(
                    SimulationEventType.SupervisorTask,
                    AlertSeverity.Advisory,
                    $"{activeSupervisorTask.Title}: {activeSupervisorTask.Objective}",
                    activeSupervisorTask);
            }
        }

        public bool TryGetActiveSupervisorTask(out SupervisorTask task)
        {
            task = activeSupervisorTask;
            return hasActiveSupervisorTask;
        }

        public void SetCrewManager(CrewManager crew)
        {
            if (subscribedCrew != null)
            {
                subscribedCrew.IncidentRaised -= HandleCrewIncident;
            }

            crewManager = crew;
            subscribedCrew = crewManager;

            if (subscribedCrew != null)
            {
                subscribedCrew.IncidentRaised += HandleCrewIncident;
            }
        }

        public void RequestSetRpm(float rpm)
        {
            RequestCommand(new DrillingCommand(DrillingCommandType.SetRpm, rpm));
        }

        public void RequestSetWeightOnBit(float tonnes)
        {
            RequestCommand(new DrillingCommand(DrillingCommandType.SetWeightOnBit, tonnes));
        }

        public void RequestSetFlowRate(float litersPerSecond)
        {
            RequestCommand(new DrillingCommand(DrillingCommandType.SetFlowRate, litersPerSecond));
        }

        public void RequestSetMudWeight(float mudWeightSG)
        {
            RequestCommand(new DrillingCommand(DrillingCommandType.SetMudWeight, mudWeightSG));
        }

        public void RequestSetChokeOpening(float opening01)
        {
            RequestCommand(new DrillingCommand(DrillingCommandType.SetChokeOpening, opening01));
        }

        public void RequestCommand(DrillingCommand command)
        {
            if (crewManager != null)
            {
                crewManager.EnqueueCommand(command, ApplyCommand);
            }
            else
            {
                ApplyCommand(command);
            }
        }

        public void ApplyCrewMitigation(CrewActionType actionType, float quality01)
        {
            float quality = Mathf.Clamp01(quality01);
            switch (actionType)
            {
                case CrewActionType.HoleCleaning:
                case CrewActionType.BackreamAndReam:
                    bitBallingRiskBias = Mathf.Clamp01(bitBallingRiskBias - 0.22f * quality);
                    state.CuttingsTransportEfficiency01 = Mathf.Clamp01(state.CuttingsTransportEfficiency01 + 0.18f * quality);
                    commandRampEnergy = Mathf.Max(0f, commandRampEnergy - 0.08f * quality);
                    break;

                case CrewActionType.KickControl:
                    gasInfluxBias = Mathf.Clamp01(gasInfluxBias - 0.28f * quality);
                    break;

                case CrewActionType.LossControl:
                    lostCirculationBias = Mathf.Clamp01(lostCirculationBias - 0.3f * quality);
                    state.FlowRateLps = Mathf.Clamp(state.FlowRateLps - 2.5f * quality, physicsConfig.MinFlowRateLps, physicsConfig.MaxFlowRateLps);
                    state.ChokeOpening01 = Mathf.Clamp01(state.ChokeOpening01 + 0.06f * quality);
                    break;

                case CrewActionType.FreeStuckPipe:
                    bitBallingRiskBias = Mathf.Clamp01(bitBallingRiskBias - 0.18f * quality);
                    wallSloughingRiskBias = Mathf.Clamp01(wallSloughingRiskBias - 0.1f * quality);
                    state.WeightOnBitTonnes = Mathf.Clamp(state.WeightOnBitTonnes - 2f * quality, physicsConfig.MinWeightOnBitTonnes, physicsConfig.MaxWeightOnBitTonnes);
                    state.NonProductiveTimeMinutes += Mathf.Lerp(4f, 1.2f, quality);
                    break;

                case CrewActionType.StickSlipMitigation:
                    commandRampEnergy = Mathf.Max(0f, commandRampEnergy - 0.35f * quality);
                    state.Rpm = Mathf.Clamp(state.Rpm - 12f * quality, physicsConfig.MinRpm, physicsConfig.MaxRpm);
                    state.WeightOnBitTonnes = Mathf.Clamp(state.WeightOnBitTonnes - 1.2f * quality, physicsConfig.MinWeightOnBitTonnes, physicsConfig.MaxWeightOnBitTonnes);
                    break;
            }
        }

        private void Awake()
        {
            if (physicsConfig == null)
            {
                physicsConfig = ScriptableObject.CreateInstance<DrillingPhysicsConfig>();
            }

            if (geologyModel == null)
            {
                geologyModel = GeologyModel.CreateRuntimeDemo();
            }

            if (wellbore == null)
            {
                wellbore = UnityEngine.Object.FindAnyObjectByType<WellboreProceduralMesh>();
            }

            SetCrewManager(crewManager);
            InitializeState();
        }

        private void Start()
        {
            if (!simulationStateWasSetExplicitly)
            {
                IsSimulating = simulateOnStart;
            }
        }

        private void OnDestroy()
        {
            if (subscribedCrew != null)
            {
                subscribedCrew.IncidentRaised -= HandleCrewIncident;
            }
        }

        private void Update()
        {
            if (!IsSimulating || wellbore == null || physicsConfig == null || geologyModel == null)
            {
                return;
            }

            supervisorClockSeconds += Time.deltaTime * simulationSpeedMultiplier;
            crewManager?.AdvanceShiftTime(Time.deltaTime * simulationSpeedMultiplier);
            SimulateStep(Time.deltaTime * simulationSpeed * simulationSpeedMultiplier);
        }

        private void InitializeState()
        {
            state = new DrillingState
            {
                MeasuredDepth = 0f,
                Rpm = 125f,
                WeightOnBitTonnes = 11.5f,
                FlowRateLps = 38f,
                FlowOutLps = 38f,
                MudWeightSG = 1.1f,
                ChokeOpening01 = 1f,
                EquivalentCirculatingDensitySG = 1.1f,
                CuttingsTransportEfficiency01 = 0.86f,
                StandpipePressureBar = physicsConfig != null ? physicsConfig.BaseStandpipePressureBar : 55f,
                HookLoadTonnes = physicsConfig != null ? physicsConfig.BaseHookLoadTonnes : 90f
            };
            ResetSupervisorTasks(2f);
        }

        private void ResetSupervisorTasks(float delaySeconds)
        {
            hasActiveSupervisorTask = false;
            activeSupervisorTaskStableSeconds = 0f;
            lastSupervisorTaskEvaluationClock = 0f;
            hasLastSupervisorTaskType = false;
            hasOpeningTaskBeenIssued = false;
            supervisorTaskSequence = 0;
            supervisorClockSeconds = 0f;
            nextSupervisorTaskTime = supervisorClockSeconds + Mathf.Max(4f, delaySeconds + UnityEngine.Random.Range(4f, 22f));
            companyCredits = 0;
            economyEarnAccumulator = 0f;
            economyCostAccumulator = 0f;
            performanceCredits = 0;
            operatingCostCredits = 0;
            successfulSupervisorTasks = 0;
            failedSupervisorTasks = 0;
        }

        private void ApplyCommand(DrillingCommand command)
        {
            if (physicsConfig == null)
            {
                return;
            }

            float oldRpm = state.Rpm;
            float oldWob = state.WeightOnBitTonnes;
            float oldFlow = state.FlowRateLps;
            float oldMudWeight = state.MudWeightSG;
            float oldChoke = state.ChokeOpening01;

            switch (command.Type)
            {
                case DrillingCommandType.SetRpm:
                    state.Rpm = Mathf.Clamp(command.TargetValue, physicsConfig.MinRpm, physicsConfig.MaxRpm);
                    break;

                case DrillingCommandType.SetWeightOnBit:
                    state.WeightOnBitTonnes = Mathf.Clamp(
                        command.TargetValue,
                        physicsConfig.MinWeightOnBitTonnes,
                        physicsConfig.MaxWeightOnBitTonnes);
                    break;

                case DrillingCommandType.SetFlowRate:
                    state.FlowRateLps = Mathf.Clamp(
                        command.TargetValue,
                        physicsConfig.MinFlowRateLps,
                        physicsConfig.MaxFlowRateLps);
                    break;

                case DrillingCommandType.SetMudWeight:
                    state.MudWeightSG = Mathf.Clamp(command.TargetValue, 0.95f, 1.55f);
                    break;

                case DrillingCommandType.SetChokeOpening:
                    state.ChokeOpening01 = Mathf.Clamp01(command.TargetValue);
                    break;
            }

            commandRampEnergy += Mathf.Abs(state.Rpm - oldRpm) / 80f;
            commandRampEnergy += Mathf.Abs(state.WeightOnBitTonnes - oldWob) / 10f;
            commandRampEnergy += Mathf.Abs(state.FlowRateLps - oldFlow) / 35f;
            commandRampEnergy += Mathf.Abs(state.MudWeightSG - oldMudWeight) / 0.08f;
            commandRampEnergy += Mathf.Abs(state.ChokeOpening01 - oldChoke) / 0.35f;
        }

        private void SimulateStep(float deltaTime)
        {
            if (!wellbore.TryEvaluateAtMeasuredDepth(state.MeasuredDepth, out TrajectorySample trajectory))
            {
                return;
            }

            LithologyZone zone = geologyModel.GetZone(state.MeasuredDepth);
            CrewInfluence crew = GetCrewInfluenceOrDefault();

            state.InclinationDegrees = trajectory.InclinationDegrees;
            state.AzimuthDegrees = trajectory.AzimuthDegrees;
            state.DoglegSeverityDegPer30m = trajectory.DoglegSeverityDegPer30m;
            state.Lithology = zone.Lithology;
            state.TrueVerticalDepth = Mathf.Max(0f, -trajectory.Position.y);

            state.DragTonnes = CalculateDrag(zone, trajectory, crew);
            state.CuttingsTransportEfficiency01 = CalculateCuttingsTransport(zone, trajectory, crew);
            state.Vibration = CalculateVibration(zone, trajectory, crew);
            state.SurfaceTorqueKnM = BlendSensor(state.SurfaceTorqueKnM, CalculateTorque(zone, trajectory), deltaTime, 1.6f);
            state.StandpipePressureBar = BlendSensor(state.StandpipePressureBar, CalculateStandpipePressure(zone, trajectory), deltaTime, 1.1f);
            CalculatePressureWindow(zone, trajectory);
            state.LostCirculationRisk01 = CalculateLostCirculationRisk(zone);
            state.KickRisk01 = CalculateKickRisk(zone, crew);
            state.FlowOutLps = BlendSensor(state.FlowOutLps, CalculateReturnFlowTarget(), deltaTime, 0.8f);
            state.RopMPerHour = BlendSensor(state.RopMPerHour, CalculateRop(zone, trajectory, crew), deltaTime, 0.9f);
            state.HookLoadTonnes = physicsConfig.BaseHookLoadTonnes + state.DragTonnes * 0.65f;
            state.StuckPipeRisk01 = CalculateStuckPipeRisk(zone, trajectory, crew);
            state.BoreholeInstabilityRisk01 = CalculateInstabilityRisk(zone, crew);
            state.BitWear01 = Mathf.Clamp01(state.BitWear01 + CalculateBitWearRate(zone, crew) * deltaTime);

            state.MeasuredDepth = Mathf.Min(
                state.MeasuredDepth + state.RopMPerHour / 3600f * deltaTime,
                wellbore.MaxMeasuredDepth);

            if (state.MeasuredDepth >= wellbore.MaxMeasuredDepth - 0.01f)
            {
                IsSimulating = false;
            }

            bitBallingRiskBias = Mathf.MoveTowards(bitBallingRiskBias, 0f, deltaTime * 0.002f);
            wallSloughingRiskBias = Mathf.MoveTowards(wallSloughingRiskBias, 0f, deltaTime * 0.002f);
            lostCirculationBias = Mathf.MoveTowards(lostCirculationBias, 0f, deltaTime * 0.0015f);
            gasInfluxBias = Mathf.MoveTowards(gasInfluxBias, 0f, deltaTime * 0.0012f);
            state.GasUnitsPercent = Mathf.MoveTowards(
                state.GasUnitsPercent,
                state.KickRisk01 * 18f,
                deltaTime * 0.08f);
            commandRampEnergy = Mathf.MoveTowards(commandRampEnergy, 0f, deltaTime * 0.08f);

            EvaluateEconomy(state, deltaTime);
            EvaluateOperationalIncidents(state);
            EvaluateSupervisorTasks(state);

            StateUpdated?.Invoke(state);
            eventChannel?.Raise(
                SimulationEventType.DrillingStateUpdated,
                AlertSeverity.Info,
                "Drilling state updated.",
                state);
        }

        private void EvaluateEconomy(DrillingState current, float deltaTime)
        {
            DifficultyTuning tuning = DifficultyProfile.Get(runtimeConfig.Difficulty);
            float simulatedMinutes = Mathf.Max(0f, deltaTime) / 60f;
            if (simulatedMinutes <= 0f)
            {
                return;
            }

            float pressureLowMargin = current.BottomHolePressureMPa - current.PorePressureMPa;
            float pressureHighMargin = current.FracturePressureMPa - current.BottomHolePressureMPa;
            float pressureQuality = pressureLowMargin > MinRequired(0.15f) && pressureHighMargin > MinRequired(0.25f)
                ? 1f
                : 0.35f;
            float risk01 = Mathf.Max(
                current.KickRisk01,
                current.LostCirculationRisk01,
                Mathf.Max(current.StuckPipeRisk01, current.BoreholeInstabilityRisk01));
            float progressQuality = Mathf.InverseLerp(4f, 22f, current.RopMPerHour) *
                                    Mathf.Lerp(0.35f, 1.0f, current.CuttingsTransportEfficiency01) *
                                    Mathf.Lerp(1f, 0.45f, current.BitWear01) *
                                    pressureQuality *
                                    (1f - risk01 * 0.45f);
            float performanceCreditsPerMinute = Mathf.Clamp(progressQuality, 0f, 1.2f) *
                                                 Mathf.Lerp(1.15f, 0.78f, Mathf.InverseLerp(0.72f, 1.45f, tuning.CorridorMultiplier)) *
                                                 tuning.CreditMultiplier;
            economyEarnAccumulator += performanceCreditsPerMinute * simulatedMinutes;
            int earned = Mathf.FloorToInt(economyEarnAccumulator);
            if (earned > 0)
            {
                economyEarnAccumulator -= earned;
                performanceCredits += earned;
                companyCredits += earned;
            }

            float fatigue = crewManager != null ? crewManager.GetInfluence().Fatigue : 0f;
            float costCreditsPerMinute = 0.18f +
                                         risk01 * 0.85f +
                                         fatigue * 0.22f +
                                         Mathf.Clamp01(commandRampEnergy) * 0.18f +
                                         (current.RopMPerHour < 1f ? 0.34f : 0f);
            economyCostAccumulator += costCreditsPerMinute * simulatedMinutes;
            int spent = Mathf.FloorToInt(economyCostAccumulator);
            if (spent > 0)
            {
                economyCostAccumulator -= spent;
                operatingCostCredits += spent;
                companyCredits -= spent;
            }
        }

        private static float BlendSensor(float current, float target, float deltaTime, float responsePerSecond)
        {
            if (Mathf.Abs(current) < 0.001f)
            {
                return target;
            }

            float alpha = 1f - Mathf.Exp(-Mathf.Max(0.01f, responsePerSecond) * Mathf.Max(0f, deltaTime));
            return Mathf.Lerp(current, target, alpha);
        }

        private float CalculateRop(LithologyZone zone, TrajectorySample trajectory, CrewInfluence crew)
        {
            float rpmFactor = Mathf.Lerp(0.25f, 1.15f, Mathf.InverseLerp(35f, 180f, state.Rpm));
            float wobFactor = Mathf.Lerp(0.3f, 1.25f, Mathf.InverseLerp(2f, 24f, state.WeightOnBitTonnes));
            float rockFactor = Mathf.Clamp(55f / Mathf.Max(10f, zone.RockStrengthMpa), 0.25f, 1.55f);
            float inclinationPenalty = 1f - Mathf.Sin(trajectory.InclinationDegrees * Mathf.Deg2Rad) * 0.18f;
            float doglegPenalty = 1f - Mathf.Clamp01(trajectory.DoglegSeverityDegPer30m / 12f) * 0.24f;
            float vibrationPenalty = 1f - Mathf.Clamp01(state.Vibration.LowFrequencyEnergy) * 0.18f;
            float incidentPenalty = 1f - Mathf.Clamp01(bitBallingRiskBias) * 0.35f;
            float overbalanceMPa = Mathf.Max(0f, state.BottomHolePressureMPa - state.PorePressureMPa);
            float differentialPenalty = 1f - Mathf.Clamp01(overbalanceMPa / 9f) * Mathf.Lerp(0.05f, 0.32f, zone.PermeabilityMd / 150f);
            float cleaningPenalty = Mathf.Lerp(0.62f, 1.08f, state.CuttingsTransportEfficiency01);
            float bitWearPenalty = Mathf.Lerp(1f, 0.42f, state.BitWear01);
            float microLithologyFactor = Mathf.Lerp(
                0.95f,
                1.05f,
                Mathf.PerlinNoise(Time.time * 0.035f, state.MeasuredDepth * 0.012f));

            return Mathf.Max(
                0.2f,
                physicsConfig.BaseRopMPerHour *
                rpmFactor *
                wobFactor *
                rockFactor *
                inclinationPenalty *
                doglegPenalty *
                vibrationPenalty *
                incidentPenalty *
                differentialPenalty *
                cleaningPenalty *
                bitWearPenalty *
                microLithologyFactor *
                crew.OperationalEfficiency);
        }

        private float CalculateDrag(LithologyZone zone, TrajectorySample trajectory, CrewInfluence crew)
        {
            float inclination = Mathf.Sin(trajectory.InclinationDegrees * Mathf.Deg2Rad);
            float dogleg = trajectory.DoglegSeverityDegPer30m;
            float lithologyFriction = 1f + zone.Stickiness01 * 0.45f + zone.Instability01 * 0.25f;
            float cuttingsBed = (1f - state.CuttingsTransportEfficiency01) * Mathf.InverseLerp(45f, 90f, trajectory.InclinationDegrees);

            return physicsConfig.PipeWeightTonnesPerMeter *
                   state.MeasuredDepth *
                   physicsConfig.BaseFrictionFactor *
                   (1f + inclination * physicsConfig.InclinationDragGain + dogleg * 0.08f + cuttingsBed * 0.65f) *
                   lithologyFriction *
                   (1f + crew.Fatigue * 0.08f);
        }

        private float CalculateTorque(LithologyZone zone, TrajectorySample trajectory)
        {
            float rockTorque = zone.RockStrengthMpa * 0.055f * Mathf.InverseLerp(1f, 28f, state.WeightOnBitTonnes);
            float rpmTorque = Mathf.InverseLerp(35f, 190f, state.Rpm) * 3.2f;
            float overloadTorque = Mathf.InverseLerp(14f, 28f, state.WeightOnBitTonnes) * 4.8f;
            float trajectoryTorque = state.DragTonnes *
                                     (0.1f + Mathf.Sin(trajectory.InclinationDegrees * Mathf.Deg2Rad) * 0.18f) *
                                     (1f + trajectory.DoglegSeverityDegPer30m * physicsConfig.DoglegTorqueGain);
            float bitWearTorque = Mathf.Lerp(0f, 7.5f, state.BitWear01) * Mathf.InverseLerp(6f, 26f, state.WeightOnBitTonnes);
            float stickSlipOscillation = Mathf.Sin(Time.time * 1.7f) * state.Vibration.LowFrequencyEnergy * 1.6f;
            float measurementNoise = (Mathf.PerlinNoise(Time.time * 0.45f, state.MeasuredDepth * 0.008f) - 0.5f) * 0.45f;

            return physicsConfig.BaseBitTorqueKnM +
                   rockTorque +
                   rpmTorque +
                   overloadTorque +
                   trajectoryTorque +
                   bitWearTorque +
                   stickSlipOscillation +
                   measurementNoise +
                   commandRampEnergy * 2.4f;
        }

        private float CalculateStandpipePressure(LithologyZone zone, TrajectorySample trajectory)
        {
            float flowRatio = Mathf.Max(0.1f, state.FlowRateLps / 38f);
            float depthFactor = 1f + state.MeasuredDepth / 2400f;
            float mudDensityFactor = Mathf.Lerp(0.82f, 1.35f, Mathf.InverseLerp(0.95f, 1.45f, state.MudWeightSG));
            float flowPressure = Mathf.Pow(flowRatio, 1.82f) * 34f * depthFactor * mudDensityFactor;
            float cuttingsLoading = (1f - state.CuttingsTransportEfficiency01) * (1f + trajectory.InclinationDegrees / 90f) * 30f;
            float lithologyRestriction = zone.Stickiness01 * 5f + zone.Instability01 * 3.5f;
            float packOff = state.StuckPipeRisk01 * physicsConfig.PressureTrendRiskGain * 100f;
            float chokeBackPressure = Mathf.Pow(1f - state.ChokeOpening01, 1.7f) * 22f;
            float rampPulse = commandRampEnergy * Mathf.Lerp(1.5f, 4.5f, flowRatio);
            float pumpPulse = Mathf.Sin(Time.time * 2.4f) * Mathf.Lerp(0.18f, 0.65f, flowRatio);
            float formationNoise = (Mathf.PerlinNoise(Time.time * 0.12f, state.MeasuredDepth * 0.005f) - 0.5f) * 0.9f;

            return physicsConfig.BaseStandpipePressureBar + flowPressure + cuttingsLoading + lithologyRestriction + packOff + chokeBackPressure + rampPulse + pumpPulse + formationNoise;
        }

        private float CalculateCuttingsTransport(LithologyZone zone, TrajectorySample trajectory, CrewInfluence crew)
        {
            float flowCleaning = Mathf.InverseLerp(22f, 62f, state.FlowRateLps);
            float inclinationPenalty = Mathf.InverseLerp(45f, 90f, trajectory.InclinationDegrees) * 0.28f;
            float ropLoading = Mathf.InverseLerp(14f, 42f, Mathf.Max(0.1f, state.RopMPerHour)) * 0.22f;
            float stickyPenalty = zone.Stickiness01 * 0.16f;
            float mudSupport = Mathf.InverseLerp(1.02f, 1.28f, state.MudWeightSG) * 0.12f;
            float crewCleaning = crew.ShiftCoordination01 * 0.08f + crew.SituationalAwareness01 * 0.04f;

            return Mathf.Clamp01(0.18f + flowCleaning * 0.76f + mudSupport + crewCleaning - inclinationPenalty - ropLoading - stickyPenalty);
        }

        private void CalculatePressureWindow(LithologyZone zone, TrajectorySample trajectory)
        {
            float tvd = Mathf.Max(1f, state.TrueVerticalDepth);
            float poreGradientSG = Mathf.Max(0.65f, zone.PorePressureGradientBarPer100m);
            float fractureGradientSG = Mathf.Max(poreGradientSG + 0.15f, zone.FractureGradientSG);
            float annularLossMPa = CalculateAnnularLossMPa(zone, trajectory);
            float chokeBackPressureMPa = Mathf.Pow(1f - state.ChokeOpening01, 1.5f) * 2.2f;

            state.PorePressureMPa = poreGradientSG * MpaPerMeterPerSG * tvd;
            state.FracturePressureMPa = fractureGradientSG * MpaPerMeterPerSG * tvd;
            state.BottomHolePressureMPa = state.MudWeightSG * MpaPerMeterPerSG * tvd + annularLossMPa + chokeBackPressureMPa;
            state.EquivalentCirculatingDensitySG = Mathf.Clamp(
                state.BottomHolePressureMPa / (MpaPerMeterPerSG * tvd),
                0.65f,
                2.1f);
        }

        private float CalculateAnnularLossMPa(LithologyZone zone, TrajectorySample trajectory)
        {
            float flowRatio = Mathf.Max(0.1f, state.FlowRateLps / 38f);
            float lengthKm = Mathf.Max(0.05f, state.MeasuredDepth / 1000f);
            float inclinationFactor = Mathf.Lerp(1f, 1.42f, Mathf.InverseLerp(35f, 90f, trajectory.InclinationDegrees));
            float cuttingsFactor = Mathf.Lerp(1.45f, 0.92f, state.CuttingsTransportEfficiency01);
            float lithologyFactor = 1f + zone.Stickiness01 * 0.18f;

            return Mathf.Pow(flowRatio, 1.75f) * lengthKm * 0.42f * inclinationFactor * cuttingsFactor * lithologyFactor;
        }

        private VibrationState CalculateVibration(
            LithologyZone zone,
            TrajectorySample trajectory,
            CrewInfluence crew)
        {
            float t = Time.time;
            float inclination01 = Mathf.InverseLerp(35f, 90f, trajectory.InclinationDegrees);
            float dogleg01 = Mathf.Clamp01(trajectory.DoglegSeverityDegPer30m / 12f);
            float rpm01 = Mathf.InverseLerp(50f, 190f, state.Rpm);
            float wob01 = Mathf.InverseLerp(4f, 26f, state.WeightOnBitTonnes);

            float lowFrequency = Mathf.Clamp01(
                0.08f +
                inclination01 * 0.24f +
                dogleg01 * 0.34f +
                zone.Stickiness01 * 0.26f +
                Mathf.InverseLerp(0.35f, 0.9f, 1f - state.CuttingsTransportEfficiency01) * 0.22f +
                crew.Fatigue * 0.18f +
                commandRampEnergy * 0.14f);

            float lateral = Mathf.Clamp01(
                zone.Abrasiveness01 * 0.3f +
                dogleg01 * 0.35f +
                rpm01 * 0.25f +
                state.BitWear01 * 0.2f +
                commandRampEnergy * 0.07f +
                Mathf.PerlinNoise(t * 0.4f, state.MeasuredDepth * 0.01f) * 0.12f);

            float axial = Mathf.Clamp01(
                zone.RockStrengthMpa / 120f * 0.35f +
                wob01 * 0.45f +
                state.BitWear01 * 0.18f +
                commandRampEnergy * 0.05f +
                Mathf.PerlinNoise(t * 0.8f, state.MeasuredDepth * 0.02f) * 0.1f);

            return new VibrationState
            {
                LowFrequencyEnergy = lowFrequency,
                LateralEnergy = lateral,
                AxialEnergy = axial,
                AccelerationG = new Vector3(
                    Mathf.Sin(t * 2.3f) * lateral,
                    Mathf.Sin(t * 5.1f) * axial,
                    Mathf.Cos(t * 1.7f) * lateral) * 2.5f
            };
        }

        private float CalculateStuckPipeRisk(
            LithologyZone zone,
            TrajectorySample trajectory,
            CrewInfluence crew)
        {
            float inclination01 = Mathf.InverseLerp(40f, 90f, trajectory.InclinationDegrees);
            float drag01 = Mathf.Clamp01(state.DragTonnes / 45f);
            float cuttingsBed = 1f - state.CuttingsTransportEfficiency01;
            float overbalance = Mathf.Clamp01((state.BottomHolePressureMPa - state.PorePressureMPa) / 8f);

            return Mathf.Clamp01(
                zone.Stickiness01 * 0.34f +
                zone.Instability01 * 0.16f +
                inclination01 * 0.22f +
                drag01 * 0.18f +
                cuttingsBed * 0.24f +
                overbalance * zone.PermeabilityMd / 180f * 0.22f +
                trajectory.DoglegSeverityDegPer30m * 0.018f +
                crew.Fatigue * physicsConfig.FatigueRiskGain +
                bitBallingRiskBias);
        }

        private float CalculateInstabilityRisk(LithologyZone zone, CrewInfluence crew)
        {
            float lowFlowPenalty = 1f - Mathf.InverseLerp(
                physicsConfig.MinFlowRateLps,
                physicsConfig.MaxFlowRateLps,
                state.FlowRateLps);
            float underbalance = Mathf.Clamp01((state.PorePressureMPa - state.BottomHolePressureMPa) / 4f);
            float fractureExcess = Mathf.Clamp01((state.BottomHolePressureMPa - state.FracturePressureMPa) / 3f);

            return Mathf.Clamp01(
                zone.Instability01 * 0.48f +
                lowFlowPenalty * 0.18f +
                underbalance * 0.34f +
                fractureExcess * 0.28f +
                crew.Fatigue * 0.1f +
                wallSloughingRiskBias);
        }

        private float CalculateBitWearRate(LithologyZone zone, CrewInfluence crew)
        {
            float abrasiveLoad = zone.Abrasiveness01 * Mathf.InverseLerp(40f, 120f, zone.RockStrengthMpa);
            float wobLoad = Mathf.InverseLerp(6f, 28f, state.WeightOnBitTonnes);
            float rpmLoad = Mathf.InverseLerp(60f, 190f, state.Rpm);
            float vibrationLoad = Mathf.Clamp01(state.Vibration.AxialEnergy + state.Vibration.LateralEnergy) * 0.5f;
            float maintenancePenalty = Mathf.Lerp(1.18f, 0.86f, crew.MaintenanceReadiness01);

            return (0.0000015f + abrasiveLoad * 0.000006f + wobLoad * rpmLoad * 0.000003f + vibrationLoad * 0.000002f) * maintenancePenalty;
        }

        private float CalculateReturnFlowTarget()
        {
            float losses = Mathf.Lerp(0f, 0.32f, Mathf.Clamp01(state.LostCirculationRisk01 + lostCirculationBias));
            float influx = Mathf.Lerp(0f, 0.18f, Mathf.Clamp01(state.KickRisk01 + gasInfluxBias));
            float transientNoise = Mathf.PerlinNoise(Time.time * 0.08f, state.MeasuredDepth * 0.002f) * 0.012f - 0.006f;
            return Mathf.Max(0f, state.FlowRateLps * (1f - losses + influx + transientNoise));
        }

        private float CalculateKickRisk(LithologyZone zone, CrewInfluence crew)
        {
            float underbalance = Mathf.Clamp01((state.PorePressureMPa - state.BottomHolePressureMPa) / 3.5f);
            float permeability = Mathf.Clamp01(zone.PermeabilityMd / 120f);
            float flowGain = Mathf.Clamp01((state.FlowOutLps - state.FlowRateLps) / 8f);

            return Mathf.Clamp01(
                underbalance * 0.58f +
                permeability * 0.16f +
                flowGain * 0.18f +
                gasInfluxBias +
                crew.Fatigue * 0.08f -
                crew.ProcedureDiscipline01 * 0.06f -
                crew.SituationalAwareness01 * 0.04f);
        }

        private float CalculateLostCirculationRisk(LithologyZone zone)
        {
            float fractureExcess = Mathf.Clamp01((state.BottomHolePressureMPa - state.FracturePressureMPa) / 3.0f);
            float weakFormation = 1f - Mathf.Clamp01(zone.FractureGradientSG / 1.85f);
            float highFlow = Mathf.InverseLerp(45f, 80f, state.FlowRateLps);

            return Mathf.Clamp01(
                fractureExcess * 0.66f +
                weakFormation * 0.12f +
                highFlow * 0.08f +
                lostCirculationBias);
        }

        private void EvaluateOperationalIncidents(DrillingState current)
        {
            if (Time.time < nextIncidentCheckTime)
            {
                return;
            }

            nextIncidentCheckTime = Time.time + 1.2f;
            if (Time.time - lastIncidentTime < 7f)
            {
                return;
            }

            LithologyZone zone = geologyModel.GetZone(current.MeasuredDepth);
            DifficultyTuning tuning = DifficultyProfile.Get(runtimeConfig.Difficulty);
            float violationMultiplier = Mathf.Max(0.25f, tuning.IncidentThresholdMultiplier);
            float flowIn = Mathf.Max(0.1f, current.FlowRateLps);
            float flowBalancePercent = (current.FlowOutLps - current.FlowRateLps) / flowIn * 100f;
            float lowPressureMargin = current.BottomHolePressureMPa - current.PorePressureMPa;
            float highPressureMargin = current.FracturePressureMPa - current.BottomHolePressureMPa;
            float vibrationG = current.Vibration.LowFrequencyEnergy * 3.5f;
            float horizontalFactor = Mathf.InverseLerp(35f, 86f, current.InclinationDegrees);
            float shaleFactor = current.Lithology == LithologyType.Shale || current.Lithology == LithologyType.Salt
                ? 1f
                : 0f;

            bool permeableReservoir = zone.PermeabilityMd > 35f || current.GasUnitsPercent > 3f;
            bool kickViolation =
                permeableReservoir &&
                (lowPressureMargin < -0.04f * violationMultiplier ||
                 current.KickRisk01 > 0.72f * violationMultiplier ||
                 (flowBalancePercent > 4.2f * violationMultiplier && current.GasUnitsPercent > 2.8f * violationMultiplier));
            if (kickViolation)
            {
                RaiseOperationalIncident(BuildOperationalIncident(OperationalIncidentType.Kick, current));
                return;
            }

            bool weakFormation = zone.FractureGradientSG < 1.66f || zone.Porosity01 > 0.18f;
            bool lossViolation =
                weakFormation &&
                (highPressureMargin < -0.06f * violationMultiplier ||
                 current.LostCirculationRisk01 > 0.7f * violationMultiplier ||
                 flowBalancePercent < -5.5f * violationMultiplier);
            if (lossViolation)
            {
                RaiseOperationalIncident(BuildOperationalIncident(OperationalIncidentType.LostCirculation, current));
                return;
            }

            bool stuckViolation =
                (zone.Stickiness01 > 0.42f || horizontalFactor > 0.45f) &&
                current.StuckPipeRisk01 > 0.68f * violationMultiplier &&
                current.DragTonnes > 28f * violationMultiplier;
            if (stuckViolation)
            {
                RaiseOperationalIncident(BuildOperationalIncident(OperationalIncidentType.DifferentialSticking, current));
                return;
            }

            bool packOffViolation =
                current.CuttingsTransportEfficiency01 < 0.45f / violationMultiplier &&
                current.StandpipePressureBar > 125f * violationMultiplier &&
                (horizontalFactor > 0.35f || zone.Stickiness01 > 0.45f);
            if (packOffViolation)
            {
                RaiseOperationalIncident(BuildOperationalIncident(OperationalIncidentType.PackOff, current));
                return;
            }

            bool wallViolation =
                shaleFactor > 0f &&
                current.BoreholeInstabilityRisk01 > 0.62f * violationMultiplier &&
                (Mathf.Abs(flowBalancePercent) > 4.5f * violationMultiplier || highPressureMargin < 0.18f / violationMultiplier);
            if (wallViolation)
            {
                RaiseOperationalIncident(BuildOperationalIncident(OperationalIncidentType.WallSloughing, current));
                return;
            }

            bool gasCutViolation =
                current.GasUnitsPercent > 5.2f * violationMultiplier &&
                lowPressureMargin < 0.22f / violationMultiplier &&
                permeableReservoir;
            if (gasCutViolation)
            {
                RaiseOperationalIncident(BuildOperationalIncident(OperationalIncidentType.GasCutMud, current));
                return;
            }

            bool topDriveViolation =
                current.SurfaceTorqueKnM > 50f * violationMultiplier &&
                vibrationG > 2.6f * violationMultiplier &&
                (current.Rpm > 140f || commandRampEnergy > 0.9f * violationMultiplier);
            if (topDriveViolation)
            {
                RaiseOperationalIncident(BuildOperationalIncident(OperationalIncidentType.TopDriveOverload, current));
                return;
            }

            bool stickSlipViolation =
                vibrationG > 3.0f * violationMultiplier &&
                current.Vibration.LowFrequencyEnergy > 0.66f * violationMultiplier &&
                current.WeightOnBitTonnes > 10f;
            if (stickSlipViolation)
            {
                RaiseOperationalIncident(BuildOperationalIncident(OperationalIncidentType.StickSlip, current));
                return;
            }

            bool shakerViolation =
                current.CuttingsTransportEfficiency01 < 0.5f / violationMultiplier &&
                current.FlowRateLps > 48f * violationMultiplier &&
                horizontalFactor > 0.25f;
            if (shakerViolation)
            {
                RaiseOperationalIncident(BuildOperationalIncident(OperationalIncidentType.ShakerOverflow, current));
                return;
            }

            bool bitViolation =
                current.BitWear01 > 0.82f * violationMultiplier &&
                (vibrationG > 2.1f * violationMultiplier || zone.Abrasiveness01 > 0.75f);
            if (bitViolation)
            {
                RaiseOperationalIncident(BuildOperationalIncident(OperationalIncidentType.BitWearLimit, current));
            }
        }

        private List<WeightedIncidentCandidate> BuildOperationalIncidentCandidates(DrillingState current)
        {
            CrewInfluence crew = GetCrewInfluenceOrDefault();
            float depthFactor = Mathf.InverseLerp(600f, 5200f, current.MeasuredDepth);
            float horizontalFactor = Mathf.InverseLerp(35f, 86f, current.InclinationDegrees);
            float highFlowFactor = Mathf.InverseLerp(44f, 76f, current.FlowRateLps);
            float pressureFactor = Mathf.InverseLerp(120f, 360f, current.StandpipePressureBar);
            float lowAwareness = 1f - crew.SituationalAwareness01;
            float poorMaintenance = 1f - crew.MaintenanceReadiness01;
            bool offshore = runtimeConfig.EnvironmentType == EnvironmentType.Offshore;
            bool arctic = runtimeConfig.GeologyRegion == GeologyRegion.ArcticShelf;

            List<WeightedIncidentCandidate> candidates = new List<WeightedIncidentCandidate>(12);
            AddIncidentCandidate(candidates, OperationalIncidentType.Kick, current.KickRisk01 * 3.2f + Mathf.InverseLerp(1.2f, 7.5f, current.GasUnitsPercent));
            AddIncidentCandidate(candidates, OperationalIncidentType.LostCirculation, current.LostCirculationRisk01 * 3.0f + highFlowFactor * 0.45f);
            AddIncidentCandidate(candidates, OperationalIncidentType.DifferentialSticking, current.StuckPipeRisk01 * 2.4f + horizontalFactor * 0.55f + depthFactor * 0.25f);
            AddIncidentCandidate(candidates, OperationalIncidentType.PoorHoleCleaning, (1f - current.CuttingsTransportEfficiency01) * 2.2f + horizontalFactor * 0.65f);
            AddIncidentCandidate(candidates, OperationalIncidentType.BitWearLimit, current.BitWear01 * 1.65f + current.Vibration.AxialEnergy * 0.5f);
            AddIncidentCandidate(candidates, OperationalIncidentType.GasCutMud, Mathf.InverseLerp(1.6f, 8f, current.GasUnitsPercent) * 2f + current.KickRisk01 * 0.85f);
            AddIncidentCandidate(candidates, OperationalIncidentType.MwdSignalLoss, depthFactor * 0.6f + horizontalFactor * 0.7f + current.DoglegSeverityDegPer30m * 0.08f + lowAwareness * 0.45f);
            AddIncidentCandidate(candidates, OperationalIncidentType.TopDriveOverload, Mathf.InverseLerp(28f, 62f, current.SurfaceTorqueKnM) * 1.7f + commandRampEnergy * 0.5f + current.Vibration.LowFrequencyEnergy * 0.55f);
            AddIncidentCandidate(candidates, OperationalIncidentType.PumpEfficiencyDrop, pressureFactor * 0.9f + highFlowFactor * 0.55f + poorMaintenance * 0.85f);
            AddIncidentCandidate(candidates, OperationalIncidentType.ShakerOverflow, (1f - current.CuttingsTransportEfficiency01) * 0.9f + highFlowFactor * 0.55f + horizontalFactor * 0.35f);
            AddIncidentCandidate(candidates, OperationalIncidentType.SevereWeather, (offshore ? 1.0f : 0.12f) + (arctic ? 0.65f : 0f) + crew.Fatigue * 0.25f);
            AddIncidentCandidate(candidates, OperationalIncidentType.DrillStringWashout, pressureFactor * 0.75f + depthFactor * 0.45f + poorMaintenance * 0.55f);
            AddIncidentCandidate(candidates, OperationalIncidentType.StickSlip, current.Vibration.LowFrequencyEnergy * 1.5f + horizontalFactor * 0.45f + Mathf.InverseLerp(130f, 190f, current.Rpm) * 0.4f);
            AddIncidentCandidate(candidates, OperationalIncidentType.PackOff, (1f - current.CuttingsTransportEfficiency01) * 1.2f + pressureFactor * 0.5f + current.StuckPipeRisk01 * 0.65f);
            AddIncidentCandidate(candidates, OperationalIncidentType.BitBalling, current.BitWear01 * 0.45f + (1f - current.CuttingsTransportEfficiency01) * 0.95f + current.Vibration.AxialEnergy * 0.4f);
            AddIncidentCandidate(candidates, OperationalIncidentType.WallSloughing, current.BoreholeInstabilityRisk01 * 1.6f + (current.Lithology == LithologyType.Shale ? 0.75f : 0f));
            return candidates;
        }

        private float CalculateOperationalIncidentProbability(DrillingState current, List<WeightedIncidentCandidate> candidates)
        {
            if (candidates == null || candidates.Count == 0)
            {
                return 0f;
            }

            CrewInfluence crew = GetCrewInfluenceOrDefault();
            float totalWeight = 0f;
            for (int i = 0; i < candidates.Count; i++)
            {
                totalWeight += candidates[i].Weight;
            }

            float difficulty = runtimeConfig.Difficulty == DifficultyLevel.Expert
                ? 0.035f
                : runtimeConfig.Difficulty == DifficultyLevel.Realistic ? 0.025f : 0.012f;
            float physics = runtimeConfig.PhysicsPreset == PhysicsPreset.Harsh
                ? 0.025f
                : runtimeConfig.PhysicsPreset == PhysicsPreset.Field ? 0.014f : 0.006f;
            float environment = runtimeConfig.EnvironmentType == EnvironmentType.Offshore ? 0.01f : 0.004f;
            float crewRisk = crew.MistakeChancePerMinute * 0.8f + crew.Fatigue * 0.018f;
            float trendRisk = Mathf.Max(current.KickRisk01, current.LostCirculationRisk01, current.StuckPipeRisk01) * 0.05f;
            return Mathf.Clamp01(0.012f + difficulty + physics + environment + crewRisk + trendRisk + totalWeight * 0.006f);
        }

        private static void AddIncidentCandidate(List<WeightedIncidentCandidate> candidates, OperationalIncidentType type, float weight)
        {
            if (weight <= 0.18f)
            {
                return;
            }

            candidates.Add(new WeightedIncidentCandidate(type, Mathf.Max(0.05f, weight)));
        }

        private static OperationalIncidentType ChooseWeightedIncident(List<WeightedIncidentCandidate> candidates)
        {
            float total = 0f;
            for (int i = 0; i < candidates.Count; i++)
            {
                total += candidates[i].Weight;
            }

            float roll = UnityEngine.Random.value * Mathf.Max(0.001f, total);
            for (int i = 0; i < candidates.Count; i++)
            {
                roll -= candidates[i].Weight;
                if (roll <= 0f)
                {
                    return candidates[i].Type;
                }
            }

            return candidates[candidates.Count - 1].Type;
        }

        private OperationalIncident BuildOperationalIncident(OperationalIncidentType type, DrillingState current)
        {
            string depthContext = BuildDepthContext(current);
            switch (type)
            {
                case OperationalIncidentType.Kick:
                    return new OperationalIncident(
                        type,
                        AlertSeverity.Critical,
                        "Приток флюида",
                        $"{depthContext} Расход на выходе выше входа, газопоказания растут, забойное давление ниже пластового.",
                        "Немедленно стабилизировать скважину: прикрыть штуцер, поднять плотность раствора, остановить наращивание параметров. Простои +18 мин.",
                        current.MeasuredDepth);

                case OperationalIncidentType.LostCirculation:
                    return new OperationalIncident(
                        type,
                        AlertSeverity.Critical,
                        "Поглощение раствора",
                        $"{depthContext} Выходной расход ниже входного, эквивалентная плотность приближается к давлению гидроразрыва пласта.",
                        "Снизить расход, контролировать объем в емкостях, подготовить материал от поглощения. Простои +18 мин.",
                        current.MeasuredDepth);

                case OperationalIncidentType.DifferentialSticking:
                    return new OperationalIncident(
                        type,
                        AlertSeverity.Warning,
                        "Риск дифференциального прихвата",
                        $"{depthContext} Высокая перегрузка по давлению, повышенное сопротивление движению и слабая очистка в наклонном участке.",
                        "Освободить колонну: снизить нагрузку на долото, увеличить циркуляцию, не оставлять колонну без движения. Простои +6 мин.",
                        current.MeasuredDepth);

                case OperationalIncidentType.PoorHoleCleaning:
                    return new OperationalIncident(
                        type,
                        AlertSeverity.Warning,
                        "Недостаточный вынос шлама",
                        $"{depthContext} В горизонтальном или наклонном интервале формируется шламовая постель.",
                        "Снизить скорость проходки, увеличить расход на 100-200 л/мин и провести промывку до стабилизации давления. Простои +6 мин.",
                        current.MeasuredDepth);

                case OperationalIncidentType.BitWearLimit:
                    return new OperationalIncident(
                        type,
                        AlertSeverity.Warning,
                        "Предельный износ долота",
                        $"{depthContext} Рост момента и осевой вибрации указывает на снижение режущей способности.",
                        "Запланировать подъем и смену долота. Продолжение бурения снижает скорость проходки и повышает аварийность. Простои +6 мин.",
                        current.MeasuredDepth);

                case OperationalIncidentType.GasCutMud:
                    return new OperationalIncident(
                        type,
                        AlertSeverity.Warning,
                        "Газированный раствор",
                        $"{depthContext} Газ в растворе снижает эффективную плотность, показания расхода становятся шумными.",
                        "Включить дегазацию, проверить баланс емкостей, не снижать забойное давление до стабилизации газа. Простои +6 мин.",
                        current.MeasuredDepth);

                case OperationalIncidentType.MwdSignalLoss:
                    return new OperationalIncident(
                        type,
                        AlertSeverity.Advisory,
                        "Потеря сигнала ННБ",
                        $"{depthContext} В наклонном/глубоком интервале ухудшилась телеметрия ННБ, траектория требует подтверждения.",
                        "Снизить темп, запросить контрольный замер, не продолжать агрессивный набор угла без данных. Простои +6 мин.",
                        current.MeasuredDepth);

                case OperationalIncidentType.TopDriveOverload:
                    return new OperationalIncident(
                        type,
                        AlertSeverity.Warning,
                        "Перегрузка верхнего привода",
                        $"{depthContext} Момент и вибрация выросли после изменения режима, привод работает близко к ограничению.",
                        "Плавно снизить обороты и нагрузку, проверить проработку ствола и исключить stick-slip. Простои +6 мин.",
                        current.MeasuredDepth);

                case OperationalIncidentType.PumpEfficiencyDrop:
                    return new OperationalIncident(
                        type,
                        AlertSeverity.Warning,
                        "Просадка эффективности насоса",
                        $"{depthContext} Давление насоса и расход расходятся: возможен износ клапанов или подсос.",
                        "Проверить насосный блок, снизить резкие изменения расхода, подтвердить фактический выход раствора. Простои +6 мин.",
                        current.MeasuredDepth);

                case OperationalIncidentType.ShakerOverflow:
                    return new OperationalIncident(
                        type,
                        AlertSeverity.Advisory,
                        "Перегрузка вибросит",
                        $"{depthContext} На очистке слишком много шлама, есть риск пропуска изменения выхода раствора.",
                        "Снизить механическую скорость, стабилизировать расход, поручить бригаде контроль сит и емкостей. Простои +6 мин.",
                        current.MeasuredDepth);

                case OperationalIncidentType.SevereWeather:
                    return new OperationalIncident(
                        type,
                        runtimeConfig.EnvironmentType == EnvironmentType.Offshore ? AlertSeverity.Warning : AlertSeverity.Advisory,
                        runtimeConfig.EnvironmentType == EnvironmentType.Offshore ? "Штормовая обстановка" : "Ухудшение погоды",
                        $"{depthContext} Внешние условия снижают готовность бригады и устойчивость операций.",
                        "Не проводить резкие операции, подтвердить связь, снизить темп команд и подготовить безопасную паузу. Простои +6 мин.",
                        current.MeasuredDepth);

                case OperationalIncidentType.DrillStringWashout:
                    return new OperationalIncident(
                        type,
                        AlertSeverity.Warning,
                        "Подозрение на промыв колонны",
                        $"{depthContext} Давление насоса ведет себя нехарактерно для текущего расхода и глубины.",
                        "Стабилизировать расход, сверить давление с предыдущим режимом, подготовить проверку герметичности. Простои +6 мин.",
                        current.MeasuredDepth);

                case OperationalIncidentType.StickSlip:
                    return new OperationalIncident(
                        type,
                        AlertSeverity.Warning,
                        "Автоколебания колонны",
                        $"{depthContext} Низкочастотная вибрация и момент растут синхронно.",
                        "Снизить обороты на 10-15%, снять часть нагрузки и возвращать режим только после затухания вибрации. Простои +6 мин.",
                        current.MeasuredDepth);

                case OperationalIncidentType.PackOff:
                    return new OperationalIncident(
                        type,
                        AlertSeverity.Warning,
                        "Сальникообразование",
                        $"{depthContext} Давление насоса растет на фоне слабой очистки и сопротивления движению.",
                        "Поднять расход ступенями, снизить нагрузку, выполнить промывку и проработку интервала. Простои +6 мин.",
                        current.MeasuredDepth);

                case OperationalIncidentType.BitBalling:
                    return new OperationalIncident(
                        type,
                        AlertSeverity.Warning,
                        "Зашламование долота",
                        $"{depthContext} Скорость проходки падает, момент и вибрация не соответствуют нагрузке.",
                        "Снизить нагрузку, промыть забой, вернуть параметры после восстановления скорости проходки. Простои +6 мин.",
                        current.MeasuredDepth);

                default:
                    return new OperationalIncident(
                        OperationalIncidentType.WallSloughing,
                        AlertSeverity.Warning,
                        "Осыпь стенок",
                        $"{depthContext} Нестабильная порода и колебания давления повышают риск каверн.",
                        "Стабилизировать плотность и расход, исключить резкие команды, контролировать шлам на ситах. Простои +6 мин.",
                        current.MeasuredDepth);
            }
        }

        private string BuildDepthContext(DrillingState current)
        {
            string environment = runtimeConfig.EnvironmentType == EnvironmentType.Offshore ? "море" : "суша";
            return $"Интервал {current.MeasuredDepth:0} м, {environment}, {ToRussianLithology(current.Lithology)}, зенит {current.InclinationDegrees:0}°.";
        }

        private static string ToRussianLithology(LithologyType lithology)
        {
            switch (lithology)
            {
                case LithologyType.Sandstone:
                    return "песчаник";
                case LithologyType.Limestone:
                    return "известняк";
                case LithologyType.Dolomite:
                    return "доломит";
                case LithologyType.Salt:
                    return "соль";
                case LithologyType.Basement:
                    return "кристаллический фундамент";
                default:
                    return "глина";
            }
        }

        private void EvaluateSupervisorTasks(DrillingState current)
        {
            if (hasActiveSupervisorTask)
            {
                float elapsedSeconds = Mathf.Max(0f, supervisorClockSeconds - activeSupervisorTaskStartedAt);
                float evaluationDeltaSeconds = Mathf.Clamp(supervisorClockSeconds - lastSupervisorTaskEvaluationClock, 0f, 3f);
                lastSupervisorTaskEvaluationClock = supervisorClockSeconds;

                bool criteriaMet = IsSupervisorTaskSuccessful(activeSupervisorTask, current, out string criteriaSummary);
                activeSupervisorTaskStableSeconds = criteriaMet
                    ? Mathf.Min(activeSupervisorTask.MinimumHoldSeconds, activeSupervisorTaskStableSeconds + evaluationDeltaSeconds)
                    : 0f;

                if (elapsedSeconds >= 0.5f &&
                    criteriaMet &&
                    activeSupervisorTaskStableSeconds >= activeSupervisorTask.MinimumHoldSeconds)
                {
                    CompleteSupervisorTask(true, $"{criteriaSummary} Цель удержана {activeSupervisorTaskStableSeconds:0}/{activeSupervisorTask.MinimumHoldSeconds:0} сек вручную.");
                    return;
                }

                if (elapsedSeconds < activeSupervisorTask.DeadlineMinutes * 60f)
                {
                    return;
                }

                IsSupervisorTaskSuccessful(activeSupervisorTask, current, out string failureSummary);
                CompleteSupervisorTask(false, failureSummary);
                return;
            }

            if (supervisorClockSeconds < nextSupervisorTaskTime)
            {
                return;
            }

            SupervisorTask task = BuildSupervisorTask(current, supervisorTaskSequence++);
            activeSupervisorTask = task;
            hasActiveSupervisorTask = true;
            activeSupervisorTaskStartedAt = supervisorClockSeconds;
            activeSupervisorTaskStableSeconds = 0f;
            lastSupervisorTaskEvaluationClock = supervisorClockSeconds;
            nextSupervisorTaskTime = supervisorClockSeconds + task.DeadlineMinutes * 60f;

            eventChannel?.Raise(
                SimulationEventType.SupervisorTask,
                AlertSeverity.Advisory,
                $"{task.Title}: {task.Objective}",
                task);
        }

        private void CompleteSupervisorTask(bool succeeded, string summary)
        {
            int creditsDelta = succeeded
                ? activeSupervisorTask.RewardCredits
                : -activeSupervisorTask.FailurePenaltyCredits;

            companyCredits += creditsDelta;
            if (succeeded)
            {
                successfulSupervisorTasks++;
                nextSupervisorTaskTime = supervisorClockSeconds + Mathf.Max(4f, 18f * supervisorCadenceMultiplier);
            }
            else
            {
                failedSupervisorTasks++;
                ApplySupervisorFailurePenalty(activeSupervisorTask.Type);
                nextSupervisorTaskTime = supervisorClockSeconds + Mathf.Max(3f, 8f * supervisorCadenceMultiplier);
            }

            SupervisorTaskResult result = new SupervisorTaskResult(
                activeSupervisorTask,
                succeeded,
                summary,
                creditsDelta,
                companyCredits);

            hasActiveSupervisorTask = false;
            activeSupervisorTaskStableSeconds = 0f;

            eventChannel?.Raise(
                SimulationEventType.SupervisorTaskResult,
                succeeded ? AlertSeverity.Advisory : AlertSeverity.Warning,
                result.Summary,
                result);
        }

        private float GetCorridorMultiplier()
        {
            return Mathf.Max(0.35f, DifficultyProfile.Get(runtimeConfig.Difficulty).CorridorMultiplier);
        }

        private float MaxAllowed(float baseValue)
        {
            return baseValue * GetCorridorMultiplier();
        }

        private float MinRequired(float baseValue)
        {
            return baseValue / GetCorridorMultiplier();
        }

        private float MaxAllowed01(float baseValue)
        {
            return Mathf.Clamp01(baseValue * GetCorridorMultiplier());
        }

        private float MinRequired01(float baseValue)
        {
            return Mathf.Clamp01(1f - (1f - baseValue) * GetCorridorMultiplier());
        }

        private bool IsSupervisorTaskSuccessful(
            SupervisorTask task,
            DrillingState current,
            out string summary)
        {
            float flowIn = Mathf.Max(0.1f, current.FlowRateLps);
            float flowBalancePercent = (current.FlowOutLps - current.FlowRateLps) / flowIn * 100f;
            float vibrationG = current.Vibration.LowFrequencyEnergy * 3.5f;
            bool pressureWindowSafe = current.BottomHolePressureMPa > current.PorePressureMPa + MinRequired(0.15f) &&
                                      current.BottomHolePressureMPa < current.FracturePressureMPa - MinRequired(0.25f);
            float pressureMarginLow = current.BottomHolePressureMPa - current.PorePressureMPa;
            float pressureMarginHigh = current.FracturePressureMPa - current.BottomHolePressureMPa;
            CrewInfluence crew = GetCrewInfluenceOrDefault();

            switch (task.Type)
            {
                case SupervisorTaskType.KickControl:
                {
                    bool ok = flowBalancePercent <= MaxAllowed(3f) &&
                              current.GasUnitsPercent < MaxAllowed(6f) &&
                              current.BottomHolePressureMPa > current.PorePressureMPa + MinRequired(0.25f) &&
                              pressureWindowSafe;
                    summary = ok
                        ? $"Приток стабилизирован: баланс выхода {flowBalancePercent:+0.0;-0.0;0.0}%, газ {current.GasUnitsPercent:0.0}%, забойное давление выше пластового."
                        : $"Не выполнено: баланс выхода {flowBalancePercent:+0.0;-0.0;0.0}%, газ {current.GasUnitsPercent:0.0}%, забойное/пластовое {current.BottomHolePressureMPa:0.0}/{current.PorePressureMPa:0.0} МПа.";
                    return ok;
                }

                case SupervisorTaskType.LossControl:
                {
                    float sppRise = current.StandpipePressureBar - task.BaselineStandpipePressureBar;
                    bool ok = flowBalancePercent >= -MaxAllowed(5f) && sppRise < MaxAllowed(12f) && current.LostCirculationRisk01 < MaxAllowed01(0.42f);
                    summary = ok
                        ? $"Поглощение стабилизировано: баланс выхода {flowBalancePercent:+0.0;-0.0;0.0}%, давление насоса +{sppRise:0.0} бар."
                        : $"Не выполнено: баланс выхода {flowBalancePercent:+0.0;-0.0;0.0}%, давление насоса +{sppRise:0.0} бар, риск {current.LostCirculationRisk01 * 100f:0}%.";
                    return ok;
                }

                case SupervisorTaskType.DirectionalDrag:
                {
                    bool ok = current.DragTonnes < MaxAllowed(35f) && current.CuttingsTransportEfficiency01 > MinRequired01(0.65f) && current.StuckPipeRisk01 < MaxAllowed01(0.58f);
                    summary = ok
                        ? $"Направленный участок под контролем: сопротивление движению {current.DragTonnes:0.0} т, очистка {current.CuttingsTransportEfficiency01 * 100f:0}%."
                        : $"Не выполнено: сопротивление движению {current.DragTonnes:0.0} т, очистка {current.CuttingsTransportEfficiency01 * 100f:0}%, риск прихвата {current.StuckPipeRisk01 * 100f:0}%.";
                    return ok;
                }

                case SupervisorTaskType.HoleCleaning:
                {
                    float sppRise = current.StandpipePressureBar - task.BaselineStandpipePressureBar;
                    bool ok = current.CuttingsTransportEfficiency01 > MinRequired01(0.7f) && sppRise < MaxAllowed(12f) && vibrationG < MaxAllowed(2.2f);
                    summary = ok
                        ? $"Промывка эффективна: очистка {current.CuttingsTransportEfficiency01 * 100f:0}%, давление насоса +{sppRise:0.0} бар."
                        : $"Не выполнено: очистка {current.CuttingsTransportEfficiency01 * 100f:0}%, давление насоса +{sppRise:0.0} бар, вибрация {vibrationG:0.0} g.";
                    return ok;
                }

                case SupervisorTaskType.ShaleStability:
                {
                    bool ok = pressureWindowSafe &&
                              current.BoreholeInstabilityRisk01 < MaxAllowed01(0.55f) &&
                              Mathf.Abs(flowBalancePercent) < MaxAllowed(5f);
                    summary = ok
                        ? $"Глинистый интервал пройден стабильно: эквив. плотность {current.EquivalentCirculatingDensitySG:0.00}, осыпь {current.BoreholeInstabilityRisk01 * 100f:0}%."
                        : $"Не выполнено: эквив. плотность {current.EquivalentCirculatingDensitySG:0.00}, осыпь {current.BoreholeInstabilityRisk01 * 100f:0}%, баланс {flowBalancePercent:+0.0;-0.0;0.0}%.";
                    return ok;
                }

                case SupervisorTaskType.BitAssessment:
                {
                    float torqueRisePercent = task.BaselineTorqueKnM > 0.1f
                        ? (current.SurfaceTorqueKnM - task.BaselineTorqueKnM) / task.BaselineTorqueKnM * 100f
                        : 0f;
                    bool ok = vibrationG < MaxAllowed(2.2f) && torqueRisePercent < MaxAllowed(20f) && current.RopMPerHour >= MinRequired(8f);
                    summary = ok
                        ? $"Долото сохранено: вибрация {vibrationG:0.0} g, момент {torqueRisePercent:+0;-0;0}% к базовому, скорость {current.RopMPerHour:0.0} м/ч."
                        : $"Не выполнено: вибрация {vibrationG:0.0} g, момент {torqueRisePercent:+0;-0;0}%, скорость {current.RopMPerHour:0.0} м/ч.";
                    return ok;
                }

                case SupervisorTaskType.PressureWindow:
                {
                    bool ok = pressureMarginLow > MinRequired(0.25f) &&
                              pressureMarginHigh > MinRequired(0.45f) &&
                              Mathf.Abs(flowBalancePercent) < MaxAllowed(4f);
                    summary = ok
                        ? $"Окно давлений удержано: запас к пластовому {pressureMarginLow:0.0} МПа, запас до гидроразрыва {pressureMarginHigh:0.0} МПа."
                        : $"Не выполнено: запас к пластовому {pressureMarginLow:0.0} МПа, до гидроразрыва {pressureMarginHigh:0.0} МПа, баланс {flowBalancePercent:+0.0;-0.0;0.0}%.";
                    return ok;
                }

                case SupervisorTaskType.CrewHandover:
                {
                    bool ok = crew.Fatigue < MaxAllowed01(0.58f) &&
                              crew.ProcedureDiscipline01 > MinRequired01(0.55f) &&
                              crew.ShiftCoordination01 > MinRequired01(0.55f);
                    summary = ok
                        ? $"Пересменка подготовлена: усталость {crew.Fatigue * 100f:0}%, дисциплина {crew.ProcedureDiscipline01 * 100f:0}%, координация {crew.ShiftCoordination01 * 100f:0}%."
                        : $"Не выполнено: усталость {crew.Fatigue * 100f:0}%, дисциплина {crew.ProcedureDiscipline01 * 100f:0}%, координация {crew.ShiftCoordination01 * 100f:0}%.";
                    return ok;
                }

                case SupervisorTaskType.PumpEfficiency:
                {
                    float sppRise = current.StandpipePressureBar - task.BaselineStandpipePressureBar;
                    bool ok = current.CuttingsTransportEfficiency01 > MinRequired01(0.64f) &&
                              sppRise < MaxAllowed(18f) &&
                              Mathf.Abs(flowBalancePercent) < MaxAllowed(6f) &&
                              vibrationG < MaxAllowed(2.4f);
                    summary = ok
                        ? $"Насосный режим эффективен: очистка {current.CuttingsTransportEfficiency01 * 100f:0}%, давление насоса +{sppRise:0.0} бар."
                        : $"Не выполнено: очистка {current.CuttingsTransportEfficiency01 * 100f:0}%, давление насоса +{sppRise:0.0} бар, баланс {flowBalancePercent:+0.0;-0.0;0.0}%.";
                    return ok;
                }

                case SupervisorTaskType.GasMonitoring:
                {
                    bool ok = current.GasUnitsPercent < MaxAllowed(4.5f) &&
                              current.KickRisk01 < MaxAllowed01(0.45f) &&
                              current.BottomHolePressureMPa > current.PorePressureMPa + MinRequired(0.22f);
                    summary = ok
                        ? $"Газовый тренд стабилен: газ {current.GasUnitsPercent:0.0}%, риск притока {current.KickRisk01 * 100f:0}%, запас к пластовому {pressureMarginLow:0.0} МПа."
                        : $"Не выполнено: газ {current.GasUnitsPercent:0.0}%, риск притока {current.KickRisk01 * 100f:0}%, запас к пластовому {pressureMarginLow:0.0} МПа.";
                    return ok;
                }

                case SupervisorTaskType.ToolfaceControl:
                {
                    bool ok = current.DoglegSeverityDegPer30m < MaxAllowed(7f) &&
                              current.DragTonnes < MaxAllowed(34f) &&
                              vibrationG < MaxAllowed(2.4f);
                    summary = ok
                        ? $"Положение отклонителя удержано: искривление {current.DoglegSeverityDegPer30m:0.0}°/30 м, сопротивление {current.DragTonnes:0.0} т, вибрация {vibrationG:0.0} g."
                        : $"Не выполнено: искривление {current.DoglegSeverityDegPer30m:0.0}°/30 м, сопротивление {current.DragTonnes:0.0} т, вибрация {vibrationG:0.0} g.";
                    return ok;
                }

                case SupervisorTaskType.PumpIntegrity:
                {
                    float sppRise = current.StandpipePressureBar - task.BaselineStandpipePressureBar;
                    bool ok = sppRise < MaxAllowed(10f) &&
                              Mathf.Abs(flowBalancePercent) < MaxAllowed(6f) &&
                              current.CuttingsTransportEfficiency01 > MinRequired01(0.6f);
                    summary = ok
                        ? $"Насосы подтверждены: давление +{sppRise:0.0} бар, баланс {flowBalancePercent:+0.0;-0.0;0.0}%, очистка {current.CuttingsTransportEfficiency01 * 100f:0}%."
                        : $"Не выполнено: давление +{sppRise:0.0} бар, баланс {flowBalancePercent:+0.0;-0.0;0.0}%, очистка {current.CuttingsTransportEfficiency01 * 100f:0}%.";
                    return ok;
                }

                case SupervisorTaskType.EquipmentInspection:
                {
                    bool ok = crew.MaintenanceReadiness01 > MinRequired01(0.58f) &&
                              vibrationG < MaxAllowed(2.5f) &&
                              commandRampEnergy < MaxAllowed(0.65f);
                    summary = ok
                        ? $"Оборудование готово: готовность {crew.MaintenanceReadiness01 * 100f:0}%, вибрация {vibrationG:0.0} g, резкость команд низкая."
                        : $"Не выполнено: готовность {crew.MaintenanceReadiness01 * 100f:0}%, вибрация {vibrationG:0.0} g, резкость команд {commandRampEnergy:0.0}.";
                    return ok;
                }

                case SupervisorTaskType.WeatherResponse:
                {
                    bool ok = crew.ShiftCoordination01 > MinRequired01(0.55f) &&
                              crew.ProcedureDiscipline01 > MinRequired01(0.55f) &&
                              Mathf.Abs(flowBalancePercent) < MaxAllowed(5f);
                    summary = ok
                        ? $"Погодный протокол выдержан: координация {crew.ShiftCoordination01 * 100f:0}%, дисциплина {crew.ProcedureDiscipline01 * 100f:0}%, баланс {flowBalancePercent:+0.0;-0.0;0.0}%."
                        : $"Не выполнено: координация {crew.ShiftCoordination01 * 100f:0}%, дисциплина {crew.ProcedureDiscipline01 * 100f:0}%, баланс {flowBalancePercent:+0.0;-0.0;0.0}%.";
                    return ok;
                }

                case SupervisorTaskType.MwdSurvey:
                {
                    bool ok = crew.SituationalAwareness01 > MinRequired01(0.58f) &&
                              current.DoglegSeverityDegPer30m < MaxAllowed(8f) &&
                              current.DragTonnes < MaxAllowed(36f);
                    summary = ok
                        ? $"Инклинометрический замер подтвержден: осведомленность {crew.SituationalAwareness01 * 100f:0}%, искривление {current.DoglegSeverityDegPer30m:0.0}°/30 м."
                        : $"Не выполнено: осведомленность {crew.SituationalAwareness01 * 100f:0}%, искривление {current.DoglegSeverityDegPer30m:0.0}°/30 м, сопротивление {current.DragTonnes:0.0} т.";
                    return ok;
                }

                case SupervisorTaskType.TorqueSmoothing:
                {
                    float torqueRisePercent = task.BaselineTorqueKnM > 0.1f
                        ? (current.SurfaceTorqueKnM - task.BaselineTorqueKnM) / task.BaselineTorqueKnM * 100f
                        : 0f;
                    bool ok = vibrationG < MaxAllowed(2.1f) &&
                              torqueRisePercent < MaxAllowed(15f) &&
                              commandRampEnergy < MaxAllowed(0.65f);
                    summary = ok
                        ? $"Момент сглажен: вибрация {vibrationG:0.0} g, момент {torqueRisePercent:+0;-0;0}%, резкость команд {commandRampEnergy:0.0}."
                        : $"Не выполнено: вибрация {vibrationG:0.0} g, момент {torqueRisePercent:+0;-0;0}%, резкость команд {commandRampEnergy:0.0}.";
                    return ok;
                }

                case SupervisorTaskType.ConnectionProcedure:
                {
                    bool ok = crew.ProcedureDiscipline01 > MinRequired01(0.58f) &&
                              crew.Fatigue < MaxAllowed01(0.65f) &&
                              Mathf.Abs(flowBalancePercent) < MaxAllowed(6f);
                    summary = ok
                        ? $"Наращивание подготовлено: дисциплина {crew.ProcedureDiscipline01 * 100f:0}%, усталость {crew.Fatigue * 100f:0}%, баланс {flowBalancePercent:+0.0;-0.0;0.0}%."
                        : $"Не выполнено: дисциплина {crew.ProcedureDiscipline01 * 100f:0}%, усталость {crew.Fatigue * 100f:0}%, баланс {flowBalancePercent:+0.0;-0.0;0.0}%.";
                    return ok;
                }

                default:
                {
                    bool ok = current.RopMPerHour > MinRequired(10f) &&
                              vibrationG < MaxAllowed(2.2f) &&
                              current.BitWear01 < MaxAllowed01(0.75f) &&
                              pressureWindowSafe;
                    summary = ok
                        ? $"План смены выдержан: скорость {current.RopMPerHour:0.0} м/ч, вибрация {vibrationG:0.0} g, износ {current.BitWear01 * 100f:0}%."
                        : $"Не выполнено: скорость {current.RopMPerHour:0.0} м/ч, вибрация {vibrationG:0.0} g, износ {current.BitWear01 * 100f:0}%, окно давлений {(pressureWindowSafe ? "в норме" : "нарушено")}.";
                    return ok;
                }
            }
        }

        private void ApplySupervisorFailurePenalty(SupervisorTaskType taskType)
        {
            state.NonProductiveTimeMinutes += 5f;

            switch (taskType)
            {
                case SupervisorTaskType.KickControl:
                    gasInfluxBias = Mathf.Clamp01(gasInfluxBias + 0.14f);
                    state.NonProductiveTimeMinutes += 10f;
                    break;

                case SupervisorTaskType.LossControl:
                    lostCirculationBias = Mathf.Clamp01(lostCirculationBias + 0.14f);
                    state.NonProductiveTimeMinutes += 8f;
                    break;

                case SupervisorTaskType.DirectionalDrag:
                case SupervisorTaskType.HoleCleaning:
                    bitBallingRiskBias = Mathf.Clamp01(bitBallingRiskBias + 0.12f);
                    break;

                case SupervisorTaskType.ShaleStability:
                    wallSloughingRiskBias = Mathf.Clamp01(wallSloughingRiskBias + 0.14f);
                    break;

                case SupervisorTaskType.BitAssessment:
                    state.BitWear01 = Mathf.Clamp01(state.BitWear01 + 0.04f);
                    commandRampEnergy = Mathf.Clamp01(commandRampEnergy + 0.1f);
                    break;

                case SupervisorTaskType.PressureWindow:
                    gasInfluxBias = Mathf.Clamp01(gasInfluxBias + 0.06f);
                    lostCirculationBias = Mathf.Clamp01(lostCirculationBias + 0.06f);
                    state.NonProductiveTimeMinutes += 4f;
                    break;

                case SupervisorTaskType.CrewHandover:
                    crewManager?.ApplyFatiguePenalty(0.08f, 0.05f);
                    state.NonProductiveTimeMinutes += 3f;
                    break;

                case SupervisorTaskType.PumpEfficiency:
                    bitBallingRiskBias = Mathf.Clamp01(bitBallingRiskBias + 0.08f);
                    commandRampEnergy = Mathf.Clamp01(commandRampEnergy + 0.08f);
                    break;

                case SupervisorTaskType.GasMonitoring:
                    gasInfluxBias = Mathf.Clamp01(gasInfluxBias + 0.1f);
                    break;

                case SupervisorTaskType.ToolfaceControl:
                case SupervisorTaskType.MwdSurvey:
                    bitBallingRiskBias = Mathf.Clamp01(bitBallingRiskBias + 0.07f);
                    commandRampEnergy = Mathf.Clamp01(commandRampEnergy + 0.08f);
                    break;

                case SupervisorTaskType.PumpIntegrity:
                    lostCirculationBias = Mathf.Clamp01(lostCirculationBias + 0.06f);
                    bitBallingRiskBias = Mathf.Clamp01(bitBallingRiskBias + 0.04f);
                    break;

                case SupervisorTaskType.EquipmentInspection:
                case SupervisorTaskType.ConnectionProcedure:
                    crewManager?.ApplyFatiguePenalty(0.05f, 0.04f);
                    commandRampEnergy = Mathf.Clamp01(commandRampEnergy + 0.08f);
                    break;

                case SupervisorTaskType.WeatherResponse:
                    crewManager?.ApplyFatiguePenalty(0.06f, 0.05f);
                    state.NonProductiveTimeMinutes += 3f;
                    break;

                case SupervisorTaskType.TorqueSmoothing:
                    commandRampEnergy = Mathf.Clamp01(commandRampEnergy + 0.14f);
                    state.BitWear01 = Mathf.Clamp01(state.BitWear01 + 0.025f);
                    break;
            }
        }

        private SupervisorTask BuildSupervisorTask(DrillingState current, int sequence)
        {
            if (!hasOpeningTaskBeenIssued)
            {
                hasOpeningTaskBeenIssued = true;
                SupervisorTaskType openingType = ChooseOpeningSupervisorTask(current);
                hasLastSupervisorTaskType = true;
                lastSupervisorTaskType = openingType;
                return TuneSupervisorTask(CreateSupervisorTask(openingType, current));
            }

            List<WeightedTaskCandidate> candidates = BuildSupervisorTaskCandidates(current);
            SupervisorTaskType selectedType = ChooseWeightedTask(candidates);
            hasLastSupervisorTaskType = true;
            lastSupervisorTaskType = selectedType;
            return TuneSupervisorTask(CreateSupervisorTask(selectedType, current));
        }

        private SupervisorTask TuneSupervisorTask(SupervisorTask task)
        {
            DifficultyTuning tuning = DifficultyProfile.Get(runtimeConfig.Difficulty);
            task.MinimumHoldSeconds *= tuning.HoldTimeMultiplier;
            task.DeadlineMinutes *= Mathf.Lerp(0.88f, 1.18f, Mathf.InverseLerp(0.72f, 1.45f, tuning.CorridorMultiplier));
            task.RewardCredits = Mathf.RoundToInt(task.RewardCredits * tuning.CreditMultiplier);
            task.FailurePenaltyCredits = Mathf.RoundToInt(task.FailurePenaltyCredits * Mathf.Lerp(0.75f, 1.35f, Mathf.InverseLerp(1.45f, 0.72f, tuning.CorridorMultiplier)));
            task.ControlHints = $"{task.ControlHints} Сложность: {tuning.DisplayName}; удерживать цель {task.MinimumHoldSeconds:0} сек вручную.";
            return task;
        }

        private SupervisorTaskType ChooseOpeningSupervisorTask(DrillingState current)
        {
            List<SupervisorTaskType> openingTasks = new List<SupervisorTaskType>
            {
                SupervisorTaskType.PressureWindow,
                SupervisorTaskType.PumpEfficiency,
                SupervisorTaskType.EquipmentInspection,
                SupervisorTaskType.ShiftPlan,
                SupervisorTaskType.ConnectionProcedure,
                SupervisorTaskType.MwdSurvey
            };

            if (current.InclinationDegrees > 35f || runtimeConfig.ProfileType != WellboreProfileType.Vertical)
            {
                openingTasks.Add(SupervisorTaskType.DirectionalDrag);
                openingTasks.Add(SupervisorTaskType.ToolfaceControl);
            }

            if (runtimeConfig.EnvironmentType == EnvironmentType.Offshore ||
                runtimeConfig.GeologyRegion == GeologyRegion.ArcticShelf)
            {
                openingTasks.Add(SupervisorTaskType.WeatherResponse);
            }

            if (current.MeasuredDepth > 1200f)
            {
                openingTasks.Add(SupervisorTaskType.HoleCleaning);
                openingTasks.Add(SupervisorTaskType.TorqueSmoothing);
                openingTasks.Add(SupervisorTaskType.GasMonitoring);
            }

            int index = UnityEngine.Random.Range(0, openingTasks.Count);
            return openingTasks[Mathf.Clamp(index, 0, openingTasks.Count - 1)];
        }

        private List<WeightedTaskCandidate> BuildSupervisorTaskCandidates(DrillingState current)
        {
            CrewInfluence crew = GetCrewInfluenceOrDefault();
            float pressureMarginLow = current.BottomHolePressureMPa - current.PorePressureMPa;
            float pressureMarginHigh = current.FracturePressureMPa - current.BottomHolePressureMPa;
            float depthFactor = Mathf.InverseLerp(600f, 5200f, current.MeasuredDepth);
            float horizontalFactor = Mathf.InverseLerp(35f, 86f, current.InclinationDegrees);
            float lowAwareness = 1f - crew.SituationalAwareness01;
            float lowDiscipline = 1f - crew.ProcedureDiscipline01;
            float poorMaintenance = 1f - crew.MaintenanceReadiness01;
            float highFlow = Mathf.InverseLerp(44f, 76f, current.FlowRateLps);
            float highPressure = Mathf.InverseLerp(130f, 360f, current.StandpipePressureBar);
            bool offshore = runtimeConfig.EnvironmentType == EnvironmentType.Offshore;
            bool arctic = runtimeConfig.GeologyRegion == GeologyRegion.ArcticShelf;

            List<WeightedTaskCandidate> candidates = new List<WeightedTaskCandidate>(18);
            AddTaskCandidate(candidates, SupervisorTaskType.KickControl, 0.35f + current.KickRisk01 * 3.6f + Mathf.InverseLerp(1.2f, 7f, current.GasUnitsPercent));
            AddTaskCandidate(candidates, SupervisorTaskType.LossControl, 0.3f + current.LostCirculationRisk01 * 3.1f + highFlow * 0.45f);
            AddTaskCandidate(candidates, SupervisorTaskType.DirectionalDrag, 0.45f + horizontalFactor * 1.7f + current.StuckPipeRisk01 * 1.2f + depthFactor * 0.35f);
            AddTaskCandidate(candidates, SupervisorTaskType.HoleCleaning, 0.55f + (1f - current.CuttingsTransportEfficiency01) * 2.1f + horizontalFactor * 0.85f);
            AddTaskCandidate(candidates, SupervisorTaskType.ShaleStability, 0.35f + current.BoreholeInstabilityRisk01 * 1.8f + (current.Lithology == LithologyType.Shale ? 1.1f : 0f));
            AddTaskCandidate(candidates, SupervisorTaskType.BitAssessment, 0.45f + current.BitWear01 * 1.8f + current.Vibration.AxialEnergy * 0.65f);
            AddTaskCandidate(candidates, SupervisorTaskType.PressureWindow, 0.5f + Mathf.InverseLerp(0.9f, 0.1f, pressureMarginLow) + Mathf.InverseLerp(1.2f, 0.25f, pressureMarginHigh));
            AddTaskCandidate(candidates, SupervisorTaskType.CrewHandover, 0.28f + crew.Fatigue * 1.4f + lowDiscipline * 0.75f);
            AddTaskCandidate(candidates, SupervisorTaskType.PumpEfficiency, 0.35f + highPressure * 0.85f + highFlow * 0.65f + (1f - current.CuttingsTransportEfficiency01) * 0.45f);
            AddTaskCandidate(candidates, SupervisorTaskType.GasMonitoring, 0.32f + Mathf.InverseLerp(0.8f, 6.5f, current.GasUnitsPercent) * 1.6f + current.KickRisk01 * 0.95f);
            AddTaskCandidate(candidates, SupervisorTaskType.ToolfaceControl, 0.28f + horizontalFactor * 1.1f + Mathf.InverseLerp(2f, 9f, current.DoglegSeverityDegPer30m) + lowAwareness * 0.55f);
            AddTaskCandidate(candidates, SupervisorTaskType.PumpIntegrity, 0.24f + highPressure * 0.9f + poorMaintenance * 0.9f + depthFactor * 0.25f);
            AddTaskCandidate(candidates, SupervisorTaskType.EquipmentInspection, 0.25f + poorMaintenance * 1.25f + commandRampEnergy * 0.3f + crew.Fatigue * 0.35f);
            AddTaskCandidate(candidates, SupervisorTaskType.WeatherResponse, 0.15f + (offshore ? 1.35f : 0.12f) + (arctic ? 0.55f : 0f) + crew.Fatigue * 0.25f);
            AddTaskCandidate(candidates, SupervisorTaskType.MwdSurvey, 0.24f + horizontalFactor * 0.85f + depthFactor * 0.45f + lowAwareness * 0.65f);
            AddTaskCandidate(candidates, SupervisorTaskType.TorqueSmoothing, 0.3f + current.Vibration.LowFrequencyEnergy * 1.3f + Mathf.InverseLerp(28f, 58f, current.SurfaceTorqueKnM) + commandRampEnergy * 0.35f);
            AddTaskCandidate(candidates, SupervisorTaskType.ConnectionProcedure, 0.25f + crew.Fatigue * 0.7f + lowDiscipline * 0.85f + Mathf.Repeat(supervisorTaskSequence, 4f) * 0.08f);
            AddTaskCandidate(candidates, SupervisorTaskType.ShiftPlan, 0.55f + UnityEngine.Random.value * 0.25f);
            return candidates;
        }

        private void AddTaskCandidate(List<WeightedTaskCandidate> candidates, SupervisorTaskType type, float weight)
        {
            if (hasLastSupervisorTaskType && lastSupervisorTaskType == type)
            {
                weight *= 0.35f;
            }

            candidates.Add(new WeightedTaskCandidate(type, Mathf.Max(0.05f, weight)));
        }

        private static SupervisorTaskType ChooseWeightedTask(List<WeightedTaskCandidate> candidates)
        {
            float total = 0f;
            for (int i = 0; i < candidates.Count; i++)
            {
                total += candidates[i].Weight;
            }

            float roll = UnityEngine.Random.value * Mathf.Max(0.001f, total);
            for (int i = 0; i < candidates.Count; i++)
            {
                roll -= candidates[i].Weight;
                if (roll <= 0f)
                {
                    return candidates[i].Type;
                }
            }

            return SupervisorTaskType.ShiftPlan;
        }

        private SupervisorTask CreateSupervisorTask(SupervisorTaskType type, DrillingState current)
        {
            switch (type)
            {
                case SupervisorTaskType.KickControl:
                    return new SupervisorTask(
                        type,
                        "Контроль притока",
                        "Признаки притока: выход и газ растут. Нужно вернуть скважину к безопасному превышению забойного давления над пластовым без выхода к гидроразрыву.",
                        "Успех: выход не выше входа более чем на 3%, газ <6%, забойное давление выше пластового и ниже гидроразрыва.",
                        "Смотреть: расход вход/выход, газ, забойное и пластовое давление, эквивалентную плотность.",
                        "Параметры: прикрыть штуцер, поднять плотность раствора на 0.02-0.05 SG, не разгонять обороты и нагрузку.",
                        8f,
                        55f,
                        140,
                        90,
                        current);

                case SupervisorTaskType.LossControl:
                    return new SupervisorTask(
                        type,
                        "Контроль поглощения",
                        "Емкости проседают: слабый пласт принимает раствор. Нужно снизить динамическую нагрузку на пласт.",
                        "Успех: выход не ниже входа более чем на 5%, давление насоса не выросло больше чем на 12 бар, риск поглощения <42%.",
                        "Смотреть: расход на выходе, давление насоса, эквивалентную плотность, риск поглощения.",
                        "Параметры: снизить расход на 100-250 л/мин, открыть штуцер, не поднимать плотность.",
                        9f,
                        55f,
                        130,
                        85,
                        current);

                case SupervisorTaskType.DirectionalDrag:
                    return new SupervisorTask(
                        type,
                        "Задание бурового мастера",
                        "В направленном участке растут момент и сопротивление движению колонны. Нужно удержать механику без перехода к прихвату.",
                        "Успех: сопротивление движению <35 т, вынос шлама >65%, риск прихвата <58%.",
                        "Смотреть: момент, нагрузку, зенитный угол, вынос шлама, риск прихвата.",
                        "Параметры: не завышать нагрузку на долото, держать расход достаточным, при вибрации снижать обороты.",
                        10f,
                        60f,
                        160,
                        95,
                        current);

                case SupervisorTaskType.HoleCleaning:
                    return new SupervisorTask(
                        type,
                        "Промывка ствола",
                        "Шламовая постель мешает бурению. Нужно поднять очистку, не спровоцировав рост давления.",
                        "Успех: вынос шлама >70%, давление насоса +12 бар максимум, вибрация <2.2 g.",
                        "Смотреть: вынос шлама, давление насоса, вибрацию, расход на выходе.",
                        "Параметры: плавно поднять расход, снизить нагрузку и скорость проходки, выполнить промывку ствола бригадой.",
                        7f,
                        45f,
                        120,
                        70,
                        current);

                case SupervisorTaskType.ShaleStability:
                    return new SupervisorTask(
                        type,
                        "Запрос геолога",
                        "Глинистый интервал чувствителен к давлению и расходу. Нужно пройти без осыпи стенок.",
                        "Успех: эквивалентная плотность в безопасном окне, риск осыпи <55%, баланс расхода в пределах 5%.",
                        "Смотреть: эквивалентную плотность, забойное/пластовое давление, гидроразрыв, риск осыпи, расход вход/выход.",
                        "Параметры: держать плотность и штуцер без резких скачков, расход менять малыми шагами.",
                        9f,
                        60f,
                        150,
                        90,
                        current);

                case SupervisorTaskType.BitAssessment:
                    return new SupervisorTask(
                        type,
                        "Оценка долота",
                        "Долото теряет эффективность. Нужно сохранить скорость проходки без разрушительной вибрации.",
                        "Успех: вибрация <2.2 g, момент не вырос выше +20%, скорость проходки >=8 м/ч.",
                        "Смотреть: вибрацию, момент, скорость проходки, износ долота.",
                        "Параметры: снизить нагрузку и обороты до устойчивого режима, не давить долото насильно.",
                        8f,
                        50f,
                        125,
                        75,
                        current);

                case SupervisorTaskType.PressureWindow:
                    return new SupervisorTask(
                        type,
                        "Распоряжение супервайзера",
                        "Проверить окно давлений: нужен запас над пластовым давлением и запас до гидроразрыва.",
                        "Успех: запас над пластовым >0.25 МПа, запас до гидроразрыва >0.45 МПа, баланс расхода в пределах 4%.",
                        "Смотреть: забойное, пластовое и давление гидроразрыва, расход вход/выход.",
                        "Параметры: корректировать плотность раствора и штуцер малыми шагами, расход менять плавно.",
                        7f,
                        45f,
                        140,
                        80,
                        current);

                case SupervisorTaskType.CrewHandover:
                    return new SupervisorTask(
                        type,
                        "Подготовка пересменки",
                        "Начальник смены требует снизить операционный риск перед передачей вахты.",
                        "Успех: усталость <58%, дисциплина процедур >55%, координация >55%.",
                        "Смотреть: усталость, дисциплину, координацию и журнал действий бригады.",
                        "Действия: провести инструктаж, осмотр оборудования и не перегружать бригаду лишними командами.",
                        6f,
                        40f,
                        110,
                        65,
                        current);

                case SupervisorTaskType.PumpEfficiency:
                    return new SupervisorTask(
                        type,
                        "Проверка насосного режима",
                        "Начальство просит подтвердить, что расход работает на очистку, а не просто разгоняет давление.",
                        "Успех: вынос шлама >64%, давление насоса +18 бар максимум, баланс расхода в пределах 6%.",
                        "Смотреть: давление насоса, расход вход/выход, вынос шлама, вибрацию.",
                        "Параметры: найти умеренный расход, не компенсировать плохую очистку резким ростом оборотов.",
                        7f,
                        50f,
                        125,
                        70,
                        current);

                case SupervisorTaskType.GasMonitoring:
                    return new SupervisorTask(
                        type,
                        "Газовый контроль",
                        "Газоанализатор показывает нестабильный тренд. Нужно подтвердить, что это не ранний приток.",
                        "Успех: газ <4.5%, риск притока <45%, забойное давление выше пластового с запасом.",
                        "Смотреть: газ, расход вход/выход, плотность раствора, забойное и пластовое давление.",
                        "Действия: поручить бригаде контроль дегазатора, держать штуцер и плотность без резких изменений.",
                        7f,
                        45f,
                        125,
                        80,
                        current);

                case SupervisorTaskType.ToolfaceControl:
                    return new SupervisorTask(
                        type,
                        "Контроль положения отклонителя",
                        "Инженер ННБ сообщает, что направленный инструмент начинает уходить с планового положения.",
                        "Успех: резкость набора угла <7°/30 м, сопротивление <34 т, вибрация <2.4 g.",
                        "Смотреть: зенит, азимут, искривление, момент и вибрацию.",
                        "Параметры: снизить грубую нагрузку, не форсировать обороты, запросить замер ННБ.",
                        8f,
                        50f,
                        135,
                        80,
                        current);

                case SupervisorTaskType.PumpIntegrity:
                    return new SupervisorTask(
                        type,
                        "Проверка насосов",
                        "Насосный режим выглядит нестабильно: надо отличить плохую очистку от проблемы насоса.",
                        "Успех: давление насоса не выросло больше +10 бар, баланс расхода в пределах 6%, вынос шлама >60%.",
                        "Смотреть: давление насоса, расход вход/выход, вынос шлама, готовность оборудования.",
                        "Действия: снизить резкие ступени расхода, поручить механику проверку насосного блока.",
                        7f,
                        45f,
                        120,
                        75,
                        current);

                case SupervisorTaskType.EquipmentInspection:
                    return new SupervisorTask(
                        type,
                        "Осмотр оборудования",
                        "Перед сложным интервалом нужна проверка верхнего привода, насосов и линии манифольда.",
                        "Успех: готовность оборудования >58%, вибрация <2.5 g, команды идут без резких скачков.",
                        "Смотреть: готовность оборудования бригады, момент, вибрацию и скорость изменения уставок.",
                        "Действия: выполнить осмотр вышки и не менять параметры крупными ступенями.",
                        6f,
                        40f,
                        110,
                        70,
                        current);

                case SupervisorTaskType.WeatherResponse:
                    return new SupervisorTask(
                        type,
                        runtimeConfig.EnvironmentType == EnvironmentType.Offshore ? "Штормовой протокол" : "Погодный протокол",
                        runtimeConfig.EnvironmentType == EnvironmentType.Offshore
                            ? "Метеоусловия ухудшились на платформе. Нужно снизить операционный риск и подтвердить связь."
                            : "Погода мешает снабжению и связи. Нужно пройти интервал без авральных команд.",
                        "Успех: координация >55%, дисциплина процедур >55%, баланс расхода в пределах 5%.",
                        "Смотреть: координацию, дисциплину, расход вход/выход, журнал бригады.",
                        "Действия: провести короткий инструктаж, снизить темп команд, подтвердить контроль емкостей.",
                        6f,
                        40f,
                        115,
                        70,
                        current);

                case SupervisorTaskType.MwdSurvey:
                    return new SupervisorTask(
                        type,
                        "Контрольный инклинометрический замер",
                        "Траектория и датчики требуют подтверждения, иначе можно уйти от планового ствола.",
                        "Успех: осведомленность бригады >58%, искривление <8°/30 м, сопротивление <36 т.",
                        "Смотреть: зенит, азимут, искривление, сопротивление движению, осведомленность ННБ.",
                        "Действия: выполнить замер инклинометрии, держать мягкие параметры до подтверждения.",
                        7f,
                        45f,
                        120,
                        75,
                        current);

                case SupervisorTaskType.TorqueSmoothing:
                    return new SupervisorTask(
                        type,
                        "Сглаживание момента",
                        "Момент реагирует рывками, возможен stick-slip или перегруз верхнего привода.",
                        "Успех: вибрация <2.1 g, момент не выше +15% к базовому, команды без резких скачков.",
                        "Смотреть: момент, вибрацию, обороты, нагрузку и энергию резких команд.",
                        "Параметры: снизить обороты и нагрузку малыми шагами, затем возвращать режим постепенно.",
                        7f,
                        45f,
                        130,
                        80,
                        current);

                case SupervisorTaskType.ConnectionProcedure:
                    return new SupervisorTask(
                        type,
                        "Процедура наращивания",
                        "Перед наращиванием нужно не потерять циркуляционный контроль и не уронить дисциплину смены.",
                        "Успех: дисциплина процедур >58%, усталость <65%, баланс расхода в пределах 6%.",
                        "Смотреть: дисциплину, усталость, расход вход/выход, давление насоса.",
                        "Действия: провести чек-лист бригады, стабилизировать расход, не ускорять время на критическом шаге.",
                        6f,
                        40f,
                        105,
                        65,
                        current);

                default:
                    return new SupervisorTask(
                        SupervisorTaskType.ShiftPlan,
                        "План смены",
                        "Рабочий режим без осложнений: держать скорость проходки, давление и механику в коридоре.",
                        "Успех: скорость проходки >10 м/ч, вибрация <2.2 g, износ <75%, окно давлений безопасное.",
                        "Смотреть: скорость проходки, вибрацию, износ, забойное/пластовое давление и гидроразрыв.",
                        "Параметры: оптимизировать обороты и нагрузку, расход держать под очистку, эквивалентную плотность не выводить за окно.",
                        8f,
                        55f,
                        120,
                        70,
                        current);
            }
        }

        private void RaiseOperationalIncident(OperationalIncident incident)
        {
            lastIncidentTime = Time.time;
            ApplyOperationalIncidentBias(incident.Type);
            state.NonProductiveTimeMinutes += incident.Severity == AlertSeverity.Critical ? 18f : 6f;

            eventChannel?.Raise(
                SimulationEventType.OperationalIncident,
                incident.Severity,
                $"{incident.Title}: {incident.Message}",
                incident);
        }

        private void ApplyOperationalIncidentBias(OperationalIncidentType type)
        {
            switch (type)
            {
                case OperationalIncidentType.Kick:
                case OperationalIncidentType.GasCutMud:
                    gasInfluxBias = Mathf.Clamp01(gasInfluxBias + 0.16f);
                    break;

                case OperationalIncidentType.LostCirculation:
                    lostCirculationBias = Mathf.Clamp01(lostCirculationBias + 0.18f);
                    break;

                case OperationalIncidentType.DifferentialSticking:
                case OperationalIncidentType.PackOff:
                    bitBallingRiskBias = Mathf.Clamp01(bitBallingRiskBias + 0.12f);
                    commandRampEnergy = Mathf.Clamp(commandRampEnergy + 0.08f, 0f, 4f);
                    break;

                case OperationalIncidentType.PoorHoleCleaning:
                case OperationalIncidentType.ShakerOverflow:
                case OperationalIncidentType.BitBalling:
                    bitBallingRiskBias = Mathf.Clamp01(bitBallingRiskBias + 0.1f);
                    break;

                case OperationalIncidentType.WallSloughing:
                    wallSloughingRiskBias = Mathf.Clamp01(wallSloughingRiskBias + 0.14f);
                    break;

                case OperationalIncidentType.TopDriveOverload:
                case OperationalIncidentType.StickSlip:
                    commandRampEnergy = Mathf.Clamp(commandRampEnergy + 0.18f, 0f, 4f);
                    break;

                case OperationalIncidentType.PumpEfficiencyDrop:
                case OperationalIncidentType.DrillStringWashout:
                    lostCirculationBias = Mathf.Clamp01(lostCirculationBias + 0.05f);
                    bitBallingRiskBias = Mathf.Clamp01(bitBallingRiskBias + 0.05f);
                    break;
            }
        }

        private CrewInfluence GetCrewInfluenceOrDefault()
        {
            if (crewManager != null)
            {
                return crewManager.GetInfluence();
            }

            return new CrewInfluence
            {
                ReactionDelaySeconds = 0.8f,
                MistakeChancePerMinute = 0.005f,
                OperationalEfficiency = 1f,
                Fatigue = 0f,
                Morale = 1f,
                ExperienceLevel = 1f,
                ProcedureDiscipline01 = 0.7f,
                SituationalAwareness01 = 0.7f,
                MaintenanceReadiness01 = 0.7f,
                ShiftCoordination01 = 0.7f
            };
        }

        private readonly struct WeightedIncidentCandidate
        {
            public readonly OperationalIncidentType Type;
            public readonly float Weight;

            public WeightedIncidentCandidate(OperationalIncidentType type, float weight)
            {
                Type = type;
                Weight = weight;
            }
        }

        private readonly struct WeightedTaskCandidate
        {
            public readonly SupervisorTaskType Type;
            public readonly float Weight;

            public WeightedTaskCandidate(SupervisorTaskType type, float weight)
            {
                Type = type;
                Weight = weight;
            }
        }

        private void HandleCrewIncident(CrewIncident incident)
        {
            if (incident.Type == CrewIncidentType.BitBalling)
            {
                bitBallingRiskBias = Mathf.Clamp01(bitBallingRiskBias + 0.18f);
            }
            else if (incident.Type == CrewIncidentType.WallSloughing)
            {
                wallSloughingRiskBias = Mathf.Clamp01(wallSloughingRiskBias + 0.2f);
            }
            else if (incident.Type == CrewIncidentType.UnsafeRamp)
            {
                commandRampEnergy = Mathf.Clamp(commandRampEnergy + 0.35f, 0f, 4f);
            }
            else if (incident.Type == CrewIncidentType.WrongMudWeight)
            {
                gasInfluxBias = Mathf.Clamp01(gasInfluxBias + 0.12f);
                wallSloughingRiskBias = Mathf.Clamp01(wallSloughingRiskBias + 0.08f);
            }
            else if (incident.Type == CrewIncidentType.MissedFlowDrop)
            {
                lostCirculationBias = Mathf.Clamp01(lostCirculationBias + 0.12f);
                bitBallingRiskBias = Mathf.Clamp01(bitBallingRiskBias + 0.08f);
            }
            else if (incident.Type == CrewIncidentType.ToolfaceDrift)
            {
                commandRampEnergy = Mathf.Clamp(commandRampEnergy + 0.16f, 0f, 4f);
                bitBallingRiskBias = Mathf.Clamp01(bitBallingRiskBias + 0.05f);
            }
            else if (incident.Type == CrewIncidentType.PumpLag)
            {
                lostCirculationBias = Mathf.Clamp01(lostCirculationBias + 0.06f);
                bitBallingRiskBias = Mathf.Clamp01(bitBallingRiskBias + 0.05f);
            }
            else if (incident.Type == CrewIncidentType.MissedGasTrend)
            {
                gasInfluxBias = Mathf.Clamp01(gasInfluxBias + 0.12f);
            }
            else if (incident.Type == CrewIncidentType.RadioMiscommunication)
            {
                commandRampEnergy = Mathf.Clamp(commandRampEnergy + 0.12f, 0f, 4f);
                crewManager?.ApplyFatiguePenalty(0.03f, 0.03f);
            }
        }
    }
}
