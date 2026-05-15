using System;
using SCADASim.Core;
using SCADASim.Crew;
using SCADASim.Trajectory;
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
        private bool hasActiveSupervisorTask;
        private SupervisorTask activeSupervisorTask;
        private float activeSupervisorTaskStartedAt;
        private float supervisorClockSeconds;
        private int supervisorTaskSequence;
        private int companyCredits;
        private int successfulSupervisorTasks;
        private int failedSupervisorTasks;
        private bool simulationStateWasSetExplicitly;
        private CrewManager subscribedCrew;

        public event Action<DrillingState> StateUpdated;

        public DrillingState CurrentState => state;
        public bool IsSimulating { get; private set; }
        public float SimulationSpeedMultiplier => simulationSpeedMultiplier;
        public int CompanyCredits => companyCredits;
        public int SuccessfulSupervisorTasks => successfulSupervisorTasks;
        public int FailedSupervisorTasks => failedSupervisorTasks;
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
                    state.ChokeOpening01 = Mathf.Clamp01(state.ChokeOpening01 - 0.08f * quality);
                    state.MudWeightSG = Mathf.Clamp(state.MudWeightSG + 0.025f * quality, 0.95f, 1.55f);
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
            supervisorTaskSequence = 0;
            supervisorClockSeconds = 0f;
            nextSupervisorTaskTime = supervisorClockSeconds + delaySeconds;
            companyCredits = 0;
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
            state.SurfaceTorqueKnM = CalculateTorque(zone, trajectory);
            state.CuttingsTransportEfficiency01 = CalculateCuttingsTransport(zone, trajectory, crew);
            state.StandpipePressureBar = CalculateStandpipePressure(zone, trajectory);
            CalculatePressureWindow(zone, trajectory);
            state.LostCirculationRisk01 = CalculateLostCirculationRisk(zone);
            state.KickRisk01 = CalculateKickRisk(zone, crew);
            state.FlowOutLps = CalculateReturnFlow();
            state.Vibration = CalculateVibration(zone, trajectory, crew);
            state.RopMPerHour = CalculateRop(zone, trajectory, crew);
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

            EvaluateOperationalIncidents(state);
            EvaluateSupervisorTasks(state);

            StateUpdated?.Invoke(state);
            eventChannel?.Raise(
                SimulationEventType.DrillingStateUpdated,
                AlertSeverity.Info,
                "Drilling state updated.",
                state);
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
                0.9f,
                1.1f,
                Mathf.PerlinNoise(Time.time * 0.05f, state.MeasuredDepth * 0.015f));

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
            float trajectoryTorque = state.DragTonnes *
                                     (0.1f + Mathf.Sin(trajectory.InclinationDegrees * Mathf.Deg2Rad) * 0.18f) *
                                     (1f + trajectory.DoglegSeverityDegPer30m * physicsConfig.DoglegTorqueGain);
            float bitWearTorque = Mathf.Lerp(0f, 7.5f, state.BitWear01) * Mathf.InverseLerp(6f, 26f, state.WeightOnBitTonnes);
            float stickSlipOscillation = Mathf.Sin(Time.time * 2.1f) * state.Vibration.LowFrequencyEnergy * 2.4f;
            float measurementNoise = (Mathf.PerlinNoise(Time.time * 0.75f, state.MeasuredDepth * 0.01f) - 0.5f) * 1.2f;

            return physicsConfig.BaseBitTorqueKnM +
                   rockTorque +
                   trajectoryTorque +
                   bitWearTorque +
                   stickSlipOscillation +
                   measurementNoise +
                   commandRampEnergy * 1.8f;
        }

        private float CalculateStandpipePressure(LithologyZone zone, TrajectorySample trajectory)
        {
            float flowRatio = Mathf.Max(0.1f, state.FlowRateLps / 38f);
            float depthFactor = 1f + state.MeasuredDepth / 2400f;
            float mudDensityFactor = Mathf.Lerp(0.82f, 1.35f, Mathf.InverseLerp(0.95f, 1.45f, state.MudWeightSG));
            float flowPressure = Mathf.Pow(flowRatio, 1.86f) * 36f * depthFactor * mudDensityFactor;
            float cuttingsLoading = (1f - state.CuttingsTransportEfficiency01) * (1f + trajectory.InclinationDegrees / 90f) * 36f;
            float lithologyRestriction = zone.Stickiness01 * 7f + zone.Instability01 * 5f;
            float packOff = state.StuckPipeRisk01 * physicsConfig.PressureTrendRiskGain * 100f;
            float chokeBackPressure = Mathf.Pow(1f - state.ChokeOpening01, 1.7f) * 22f;
            float pumpPulse = Mathf.Sin(Time.time * 3.8f) * Mathf.Lerp(0.3f, 1.2f, flowRatio);
            float formationNoise = (Mathf.PerlinNoise(Time.time * 0.22f, state.MeasuredDepth * 0.006f) - 0.5f) * 2.4f;

            return physicsConfig.BaseStandpipePressureBar + flowPressure + cuttingsLoading + lithologyRestriction + packOff + chokeBackPressure + pumpPulse + formationNoise;
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
                commandRampEnergy * 0.08f);

            float lateral = Mathf.Clamp01(
                zone.Abrasiveness01 * 0.3f +
                dogleg01 * 0.35f +
                rpm01 * 0.25f +
                state.BitWear01 * 0.2f +
                Mathf.PerlinNoise(t * 0.4f, state.MeasuredDepth * 0.01f) * 0.12f);

            float axial = Mathf.Clamp01(
                zone.RockStrengthMpa / 120f * 0.35f +
                wob01 * 0.45f +
                state.BitWear01 * 0.18f +
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

        private float CalculateReturnFlow()
        {
            float losses = Mathf.Lerp(0f, 0.42f, Mathf.Clamp01(state.LostCirculationRisk01 + lostCirculationBias));
            float influx = Mathf.Lerp(0f, 0.22f, Mathf.Clamp01(state.KickRisk01 + gasInfluxBias));
            float transientNoise = Mathf.PerlinNoise(Time.time * 0.13f, state.MeasuredDepth * 0.002f) * 0.025f - 0.012f;
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

            if (current.KickRisk01 > 0.74f)
            {
                gasInfluxBias = Mathf.Clamp01(gasInfluxBias + 0.18f);
                RaiseOperationalIncident(new OperationalIncident(
                    OperationalIncidentType.Kick,
                    AlertSeverity.Critical,
                    "Приток флюида",
                    "Расход на выходе выше входа, газопоказания растут, забойное давление ниже пластового.",
                    "Немедленно стабилизировать скважину: прикрыть штуцер, поднять плотность раствора, остановить наращивание параметров. НПВ +18 мин.",
                    current.MeasuredDepth));
                return;
            }

            if (current.LostCirculationRisk01 > 0.72f)
            {
                lostCirculationBias = Mathf.Clamp01(lostCirculationBias + 0.2f);
                RaiseOperationalIncident(new OperationalIncident(
                    OperationalIncidentType.LostCirculation,
                    AlertSeverity.Critical,
                    "Поглощение раствора",
                    "Выходной расход ниже входного, ECD приближается к давлению гидроразрыва пласта.",
                    "Снизить расход, контролировать объем в емкостях, подготовить LCM-пачку. НПВ +18 мин.",
                    current.MeasuredDepth));
                return;
            }

            if (current.StuckPipeRisk01 > 0.78f)
            {
                RaiseOperationalIncident(new OperationalIncident(
                    OperationalIncidentType.DifferentialSticking,
                    AlertSeverity.Warning,
                    "Риск дифференциального прихвата",
                    "Высокая перегрузка по давлению, повышенный drag и слабая очистка в наклонном участке.",
                    "Освободить колонну: снизить нагрузку на долото, увеличить циркуляцию, не оставлять колонну без движения. НПВ +6 мин.",
                    current.MeasuredDepth));
                return;
            }

            if (current.CuttingsTransportEfficiency01 < 0.43f)
            {
                RaiseOperationalIncident(new OperationalIncident(
                    OperationalIncidentType.PoorHoleCleaning,
                    AlertSeverity.Warning,
                    "Недостаточный вынос шлама",
                    "В горизонтальном или наклонном интервале формируется шламовая постель.",
                    "Снизить ROP, увеличить расход на 100-200 л/мин и провести промывку до стабилизации давления. НПВ +6 мин.",
                    current.MeasuredDepth));
                return;
            }

            if (current.BitWear01 > 0.82f)
            {
                RaiseOperationalIncident(new OperationalIncident(
                    OperationalIncidentType.BitWearLimit,
                    AlertSeverity.Warning,
                    "Предельный износ долота",
                    "Рост момента и осевой вибрации указывает на снижение режущей способности.",
                    "Запланировать подъем и смену долота. Продолжение бурения резко снижает ROP и повышает аварийность. НПВ +6 мин.",
                    current.MeasuredDepth));
            }
        }

        private void EvaluateSupervisorTasks(DrillingState current)
        {
            if (hasActiveSupervisorTask)
            {
                float elapsedSeconds = Mathf.Max(0f, supervisorClockSeconds - activeSupervisorTaskStartedAt);
                if (elapsedSeconds >= activeSupervisorTask.MinimumHoldSeconds &&
                    IsSupervisorTaskSuccessful(activeSupervisorTask, current, out string successSummary))
                {
                    CompleteSupervisorTask(true, successSummary);
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
                nextSupervisorTaskTime = supervisorClockSeconds + 18f;
            }
            else
            {
                failedSupervisorTasks++;
                ApplySupervisorFailurePenalty(activeSupervisorTask.Type);
                nextSupervisorTaskTime = supervisorClockSeconds + 8f;
            }

            SupervisorTaskResult result = new SupervisorTaskResult(
                activeSupervisorTask,
                succeeded,
                summary,
                creditsDelta,
                companyCredits);

            hasActiveSupervisorTask = false;

            eventChannel?.Raise(
                SimulationEventType.SupervisorTaskResult,
                succeeded ? AlertSeverity.Advisory : AlertSeverity.Warning,
                result.Summary,
                result);
        }

        private bool IsSupervisorTaskSuccessful(
            SupervisorTask task,
            DrillingState current,
            out string summary)
        {
            float flowIn = Mathf.Max(0.1f, current.FlowRateLps);
            float flowBalancePercent = (current.FlowOutLps - current.FlowRateLps) / flowIn * 100f;
            float vibrationG = current.Vibration.LowFrequencyEnergy * 3.5f;
            bool pressureWindowSafe = current.BottomHolePressureMPa > current.PorePressureMPa + 0.15f &&
                                      current.BottomHolePressureMPa < current.FracturePressureMPa - 0.25f;

            switch (task.Type)
            {
                case SupervisorTaskType.KickControl:
                {
                    bool ok = flowBalancePercent <= 3f &&
                              current.GasUnitsPercent < 6f &&
                              current.BottomHolePressureMPa > current.PorePressureMPa + 0.25f &&
                              pressureWindowSafe;
                    summary = ok
                        ? $"Приток задавлен: выход {flowBalancePercent:+0.0;-0.0;0.0}%, газ {current.GasUnitsPercent:0.0}%, BHP выше пластового."
                        : $"Не выполнено: выход {flowBalancePercent:+0.0;-0.0;0.0}%, газ {current.GasUnitsPercent:0.0}%, BHP/пластовое {current.BottomHolePressureMPa:0.0}/{current.PorePressureMPa:0.0} МПа.";
                    return ok;
                }

                case SupervisorTaskType.LossControl:
                {
                    float sppRise = current.StandpipePressureBar - task.BaselineStandpipePressureBar;
                    bool ok = flowBalancePercent >= -5f && sppRise < 12f && current.LostCirculationRisk01 < 0.42f;
                    summary = ok
                        ? $"Поглощение стабилизировано: выход {flowBalancePercent:+0.0;-0.0;0.0}%, SPP +{sppRise:0.0} бар."
                        : $"Не выполнено: выход {flowBalancePercent:+0.0;-0.0;0.0}%, SPP +{sppRise:0.0} бар, риск {current.LostCirculationRisk01 * 100f:0}%.";
                    return ok;
                }

                case SupervisorTaskType.DirectionalDrag:
                {
                    bool ok = current.DragTonnes < 35f && current.CuttingsTransportEfficiency01 > 0.65f && current.StuckPipeRisk01 < 0.58f;
                    summary = ok
                        ? $"Направленный участок под контролем: drag {current.DragTonnes:0.0} т, очистка {current.CuttingsTransportEfficiency01 * 100f:0}%."
                        : $"Не выполнено: drag {current.DragTonnes:0.0} т, очистка {current.CuttingsTransportEfficiency01 * 100f:0}%, риск прихвата {current.StuckPipeRisk01 * 100f:0}%.";
                    return ok;
                }

                case SupervisorTaskType.HoleCleaning:
                {
                    float sppRise = current.StandpipePressureBar - task.BaselineStandpipePressureBar;
                    bool ok = current.CuttingsTransportEfficiency01 > 0.7f && sppRise < 12f && vibrationG < 2.2f;
                    summary = ok
                        ? $"Промывка эффективна: очистка {current.CuttingsTransportEfficiency01 * 100f:0}%, SPP +{sppRise:0.0} бар."
                        : $"Не выполнено: очистка {current.CuttingsTransportEfficiency01 * 100f:0}%, SPP +{sppRise:0.0} бар, вибрация {vibrationG:0.0} g.";
                    return ok;
                }

                case SupervisorTaskType.ShaleStability:
                {
                    bool ok = pressureWindowSafe &&
                              current.BoreholeInstabilityRisk01 < 0.55f &&
                              Mathf.Abs(flowBalancePercent) < 5f;
                    summary = ok
                        ? $"Глинистый интервал пройден стабильно: ECD {current.EquivalentCirculatingDensitySG:0.00}, осыпь {current.BoreholeInstabilityRisk01 * 100f:0}%."
                        : $"Не выполнено: ECD {current.EquivalentCirculatingDensitySG:0.00}, осыпь {current.BoreholeInstabilityRisk01 * 100f:0}%, баланс {flowBalancePercent:+0.0;-0.0;0.0}%.";
                    return ok;
                }

                case SupervisorTaskType.BitAssessment:
                {
                    float torqueRisePercent = task.BaselineTorqueKnM > 0.1f
                        ? (current.SurfaceTorqueKnM - task.BaselineTorqueKnM) / task.BaselineTorqueKnM * 100f
                        : 0f;
                    bool ok = vibrationG < 2.2f && torqueRisePercent < 20f && current.RopMPerHour >= 8f;
                    summary = ok
                        ? $"Долото сохранено: вибрация {vibrationG:0.0} g, момент {torqueRisePercent:+0;-0;0}% к базовому, ROP {current.RopMPerHour:0.0} м/ч."
                        : $"Не выполнено: вибрация {vibrationG:0.0} g, момент {torqueRisePercent:+0;-0;0}%, ROP {current.RopMPerHour:0.0} м/ч.";
                    return ok;
                }

                default:
                {
                    bool ok = current.RopMPerHour > 10f &&
                              vibrationG < 2.2f &&
                              current.BitWear01 < 0.75f &&
                              pressureWindowSafe;
                    summary = ok
                        ? $"План смены выдержан: ROP {current.RopMPerHour:0.0} м/ч, вибрация {vibrationG:0.0} g, износ {current.BitWear01 * 100f:0}%."
                        : $"Не выполнено: ROP {current.RopMPerHour:0.0} м/ч, вибрация {vibrationG:0.0} g, износ {current.BitWear01 * 100f:0}%, окно давлений {(pressureWindowSafe ? "OK" : "нет")}.";
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
            }
        }

        private SupervisorTask BuildSupervisorTask(DrillingState current, int sequence)
        {
            int variant = sequence % 7;
            if (current.KickRisk01 > 0.42f || variant == 5)
            {
                return new SupervisorTask(
                    SupervisorTaskType.KickControl,
                    "Контроль притока",
                    "Признаки притока: выход и газ растут. Нужно вернуть скважину в overbalance без выхода за ГРП.",
                    "Успех: выход <= вход +3%, газ <6%, забойное давление выше пластового и ниже ГРП.",
                    "Смотреть: расход вход/выход, газ %, BHP, пластовое, ГРП, ECD.",
                    "Крутилки: прикрыть штуцер, поднять плотность раствора ступенью 0.02-0.05 SG, не разгонять RPM/WOB.",
                    8f,
                    55f,
                    140,
                    90,
                    current);
            }

            if (current.LostCirculationRisk01 > 0.38f || variant == 6)
            {
                return new SupervisorTask(
                    SupervisorTaskType.LossControl,
                    "Контроль поглощения",
                    "Емкости проседают: слабый пласт принимает раствор. Нужно снизить динамическую нагрузку на пласт.",
                    "Успех: выход не ниже входа более чем на 5%, SPP не вырос больше чем на 12 бар, риск поглощения <42%.",
                    "Смотреть: расход выход, давление насоса, ECD, риск поглощения.",
                    "Крутилки: снизить расход на 100-250 л/мин, открыть штуцер, не поднимать плотность.",
                    9f,
                    55f,
                    130,
                    85,
                    current);
            }

            if (current.InclinationDegrees > 55f || variant == 1)
            {
                return new SupervisorTask(
                    SupervisorTaskType.DirectionalDrag,
                    "Задание бурового мастера",
                    "В направленном участке растут torque/drag. Нужно удержать механику без перехода к прихвату.",
                    "Успех: drag <35 т, вынос шлама >65%, риск прихвата <58%.",
                    "Смотреть: момент, drag/нагрузка, зенит, вынос шлама, риск прихвата.",
                    "Крутилки: не завышать WOB, держать расход достаточным, при вибрации снижать RPM.",
                    10f,
                    60f,
                    160,
                    95,
                    current);
            }

            if (current.CuttingsTransportEfficiency01 < 0.58f || variant == 2)
            {
                return new SupervisorTask(
                    SupervisorTaskType.HoleCleaning,
                    "Промывка ствола",
                    "Шламовая постель мешает бурению. Нужно поднять очистку, не спровоцировав рост давления.",
                    "Успех: вынос шлама >70%, SPP +12 бар максимум, вибрация <2.2 g.",
                    "Смотреть: вынос шлама, SPP, вибрация, расход выход.",
                    "Крутилки: плавно поднять расход, снизить WOB/ROP, выполнить промывку ствола бригадой.",
                    7f,
                    45f,
                    120,
                    70,
                    current);
            }

            if (current.Lithology == LithologyType.Shale || variant == 3)
            {
                return new SupervisorTask(
                    SupervisorTaskType.ShaleStability,
                    "Запрос геолога",
                    "Глинистый интервал чувствителен к давлению и расходу. Нужно пройти без осыпи стенок.",
                    "Успех: ECD в безопасном окне, риск осыпи <55%, баланс расхода в пределах 5%.",
                    "Смотреть: ECD, BHP/пластовое/ГРП, риск осыпи, расход вход/выход.",
                    "Крутилки: держать плотность и штуцер без резких скачков, расход менять малыми шагами.",
                    9f,
                    60f,
                    150,
                    90,
                    current);
            }

            if (current.BitWear01 > 0.55f || variant == 4)
            {
                return new SupervisorTask(
                    SupervisorTaskType.BitAssessment,
                    "Оценка долота",
                    "Долото теряет эффективность. Нужно сохранить ROP без разрушительной вибрации.",
                    "Успех: вибрация <2.2 g, момент не вырос выше +20%, ROP >=8 м/ч.",
                    "Смотреть: вибрация, момент, ROP, износ долота.",
                    "Крутилки: снизить WOB/RPM до устойчивого режима, не давить долото насильно.",
                    8f,
                    50f,
                    125,
                    75,
                    current);
            }

            return new SupervisorTask(
                SupervisorTaskType.ShiftPlan,
                "План смены",
                "Рабочий режим без осложнений: держать скорость проходки, давление и механику в коридоре.",
                "Успех: ROP >10 м/ч, вибрация <2.2 g, износ <75%, окно давлений безопасное.",
                "Смотреть: ROP, вибрация, износ, BHP/пластовое/ГРП.",
                "Крутилки: оптимизировать RPM/WOB, расход держать под очистку, ECD не выводить за окно.",
                8f,
                55f,
                120,
                70,
                current);
        }

        private void RaiseOperationalIncident(OperationalIncident incident)
        {
            lastIncidentTime = Time.time;
            state.NonProductiveTimeMinutes += incident.Severity == AlertSeverity.Critical ? 18f : 6f;

            eventChannel?.Raise(
                SimulationEventType.OperationalIncident,
                incident.Severity,
                $"{incident.Title}: {incident.Message}",
                incident);
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
        }
    }
}
