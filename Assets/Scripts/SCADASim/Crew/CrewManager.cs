using System;
using System.Collections.Generic;
using SCADASim.Core;
using UnityEngine;

namespace SCADASim.Crew
{
    public sealed class CrewManager : MonoBehaviour
    {
        [Header("Crew State")]
        [Range(0f, 1f)] [SerializeField] private float experienceLevel = 0.62f;
        [Range(0f, 1f)] [SerializeField] private float fatigue = 0.18f;
        [Range(0f, 1f)] [SerializeField] private float morale = 0.74f;

        [Header("Behavior")]
        [SerializeField] private float baseReactionDelaySeconds = 0.8f;
        [SerializeField] private float fatigueDelayMultiplier = 5.5f;
        [SerializeField] private float baseMistakeChancePerMinute = 0.012f;
        [SerializeField] private float fatigueGrowthPerRealMinute = 0.025f;
        [SerializeField] private float moraleRecoveryPerRealMinute = 0.004f;

        [Header("Shift Schedule")]
        [SerializeField] private float shiftDurationMinutes = 8f;

        [Header("Integration")]
        [SerializeField] private SimulationEventChannel eventChannel;

        private static readonly CrewShiftProfile[] ShiftProfiles =
        {
            new CrewShiftProfile(
                "дневная смена A",
                "бурильщик Иванов",
                "буровой мастер Каюмов",
                "растворщик Сафиуллин",
                "инженер ННБ Орлова",
                0.05f,
                -0.02f,
                0.03f,
                0.08f,
                0.04f,
                0.02f,
                0.06f),
            new CrewShiftProfile(
                "ночная смена B",
                "бурильщик Ахметов",
                "буровой мастер Смирнов",
                "растворщик Лебедев",
                "инженер ННБ Морозова",
                -0.02f,
                0.08f,
                -0.03f,
                -0.02f,
                0.02f,
                0.06f,
                -0.01f),
            new CrewShiftProfile(
                "смена C, стажерская поддержка",
                "бурильщик Петров",
                "буровой мастер Галиев",
                "растворщик Никитин",
                "инженер ННБ Волкова",
                -0.07f,
                0.04f,
                0.04f,
                -0.05f,
                -0.03f,
                -0.01f,
                -0.06f)
        };

        private readonly List<PendingCrewCommand> pendingCommands = new List<PendingCrewCommand>();
        private float procedureDiscipline01 = 0.58f;
        private float situationalAwareness01 = 0.55f;
        private float maintenanceReadiness01 = 0.62f;
        private float shiftCoordination01 = 0.6f;
        private float lastEventTime;
        private float presetExperienceLevel;
        private float presetFatigue;
        private float presetMorale;
        private float presetProcedureDiscipline01;
        private float presetSituationalAwareness01;
        private float presetMaintenanceReadiness01;
        private float presetShiftCoordination01;
        private int operationalDay = 1;
        private int currentShiftIndex;
        private float shiftClockSeconds;

        public event Action<CrewIncident> IncidentRaised;

        public float ExperienceLevel => experienceLevel;
        public float Fatigue => fatigue;
        public float Morale => morale;
        public int OperationalDay => operationalDay;
        public string ShiftStatusLine => $"день {operationalDay}, {CurrentShift.Name}, до пересменки {FormatRemainingShift()}";
        public string ActiveCrewLine => $"{CurrentShift.Toolpusher}; {CurrentShift.Driller}; {CurrentShift.MudEngineer}; {CurrentShift.DirectionalEngineer}";
        public string CurrentShiftName => CurrentShift.Name;

        private CrewShiftProfile CurrentShift => ShiftProfiles[Mathf.Clamp(currentShiftIndex, 0, ShiftProfiles.Length - 1)];

        public void Configure(SimulationEventChannel channel)
        {
            eventChannel = channel;
        }

        private void Awake()
        {
            SetPresetBaseline(
                experienceLevel,
                fatigue,
                morale,
                procedureDiscipline01,
                situationalAwareness01,
                maintenanceReadiness01,
                shiftCoordination01);
            ApplyCurrentShiftProfile(true, false);
        }

        public void ApplyPreset(CrewPreset preset)
        {
            switch (preset)
            {
                case CrewPreset.FullSeven:
                    SetPresetBaseline(0.78f, 0.12f, 0.82f, 0.72f, 0.68f, 0.76f, 0.74f);
                    break;

                case CrewPreset.StandardFour:
                    SetPresetBaseline(0.62f, 0.24f, 0.72f, 0.6f, 0.56f, 0.62f, 0.58f);
                    break;

                case CrewPreset.ReducedThree:
                    SetPresetBaseline(0.54f, 0.36f, 0.64f, 0.48f, 0.46f, 0.52f, 0.44f);
                    break;

                case CrewPreset.TraineeShift:
                    SetPresetBaseline(0.36f, 0.22f, 0.76f, 0.42f, 0.38f, 0.5f, 0.46f);
                    break;
            }

            operationalDay = 1;
            currentShiftIndex = 0;
            shiftClockSeconds = 0f;
            ApplyCurrentShiftProfile(true, false);
        }

        public void SetShiftDurationMinutes(float minutes)
        {
            shiftDurationMinutes = Mathf.Clamp(minutes, 3f, 24f);
            float shiftDurationSeconds = Mathf.Max(60f, shiftDurationMinutes * 60f);
            shiftClockSeconds = Mathf.Min(shiftClockSeconds, Mathf.Max(0f, shiftDurationSeconds - 1f));
        }

        public void AdvanceShiftTime(float deltaSeconds)
        {
            if (deltaSeconds <= 0f)
            {
                return;
            }

            shiftClockSeconds += deltaSeconds;
            float workload = 1f + pendingCommands.Count * 0.12f;
            CrewShiftProfile profile = CurrentShift;
            fatigue = Mathf.Clamp01(fatigue + fatigueGrowthPerRealMinute * workload * deltaSeconds / 60f);
            morale = Mathf.Clamp01(morale + moraleRecoveryPerRealMinute * deltaSeconds / 60f - fatigue * 0.002f * deltaSeconds / 60f);
            procedureDiscipline01 = Mathf.MoveTowards(procedureDiscipline01, Mathf.Clamp01(presetProcedureDiscipline01 + profile.DisciplineOffset), deltaSeconds * 0.002f / 60f);
            situationalAwareness01 = Mathf.MoveTowards(situationalAwareness01, Mathf.Clamp01(presetSituationalAwareness01 + profile.AwarenessOffset), deltaSeconds * 0.003f / 60f);
            maintenanceReadiness01 = Mathf.MoveTowards(maintenanceReadiness01, Mathf.Clamp01(presetMaintenanceReadiness01 + profile.MaintenanceOffset), deltaSeconds * 0.0015f / 60f);
            shiftCoordination01 = Mathf.MoveTowards(shiftCoordination01, Mathf.Clamp01(presetShiftCoordination01 + profile.CoordinationOffset), deltaSeconds * 0.0025f / 60f);

            float shiftDurationSeconds = Mathf.Max(60f, shiftDurationMinutes * 60f);
            while (shiftClockSeconds >= shiftDurationSeconds)
            {
                shiftClockSeconds -= shiftDurationSeconds;
                RotateShift();
            }
        }

        public void ApplyFatiguePenalty(float fatigueDelta01, float moralePenalty01)
        {
            fatigue = Mathf.Clamp01(fatigue + fatigueDelta01);
            morale = Mathf.Clamp01(morale - moralePenalty01);
            shiftCoordination01 = Mathf.Clamp01(shiftCoordination01 - moralePenalty01 * 0.7f);
        }

        private void SetPresetBaseline(
            float experience,
            float startFatigue,
            float startMorale,
            float discipline,
            float awareness,
            float maintenance,
            float coordination)
        {
            presetExperienceLevel = experience;
            presetFatigue = startFatigue;
            presetMorale = startMorale;
            presetProcedureDiscipline01 = discipline;
            presetSituationalAwareness01 = awareness;
            presetMaintenanceReadiness01 = maintenance;
            presetShiftCoordination01 = coordination;
        }

        private void RotateShift()
        {
            currentShiftIndex++;
            if (currentShiftIndex >= ShiftProfiles.Length)
            {
                currentShiftIndex = 0;
                operationalDay++;
            }

            ApplyCurrentShiftProfile(false, true);
        }

        private void ApplyCurrentShiftProfile(bool resetFatigue, bool raiseEvent)
        {
            CrewShiftProfile profile = CurrentShift;
            experienceLevel = Mathf.Clamp01(presetExperienceLevel + profile.ExperienceOffset);
            morale = Mathf.Clamp01(presetMorale + profile.MoraleOffset);
            procedureDiscipline01 = Mathf.Clamp01(presetProcedureDiscipline01 + profile.DisciplineOffset);
            situationalAwareness01 = Mathf.Clamp01(presetSituationalAwareness01 + profile.AwarenessOffset);
            maintenanceReadiness01 = Mathf.Clamp01(presetMaintenanceReadiness01 + profile.MaintenanceOffset);
            shiftCoordination01 = Mathf.Clamp01(presetShiftCoordination01 + profile.CoordinationOffset);

            float freshFatigue = Mathf.Clamp01(presetFatigue + profile.FatigueOffset);
            fatigue = resetFatigue
                ? freshFatigue
                : Mathf.Clamp01(Mathf.Lerp(fatigue, freshFatigue, 0.72f) + 0.035f);

            if (raiseEvent)
            {
                eventChannel?.Raise(
                    SimulationEventType.CrewStateUpdated,
                    AlertSeverity.Advisory,
                    $"Пересменка: {ShiftStatusLine}. {ActiveCrewLine}.",
                    GetInfluence());
            }
        }

        private string FormatRemainingShift()
        {
            float shiftDurationSeconds = Mathf.Max(60f, shiftDurationMinutes * 60f);
            return FormatMinutesSeconds(shiftDurationSeconds - shiftClockSeconds);
        }

        private static string FormatMinutesSeconds(float seconds)
        {
            int totalSeconds = Mathf.CeilToInt(Mathf.Max(0f, seconds));
            int minutes = totalSeconds / 60;
            int remainder = totalSeconds % 60;
            return $"{minutes:00}:{remainder:00}";
        }

        public CrewInfluence GetInfluence()
        {
            float experienceDelayFactor = Mathf.Lerp(1.35f, 0.68f, experienceLevel);
            float moraleDelayFactor = Mathf.Lerp(1.2f, 0.82f, morale);
            float reactionDelay = baseReactionDelaySeconds *
                                  (1f + fatigue * fatigueDelayMultiplier) *
                                  experienceDelayFactor *
                                  moraleDelayFactor;

            float mistakeChance = baseMistakeChancePerMinute *
                                  Mathf.Lerp(0.35f, 3.6f, fatigue) *
                                  Mathf.Lerp(1.45f, 0.58f, experienceLevel) *
                                  Mathf.Lerp(1.35f, 0.78f, morale) *
                                  Mathf.Lerp(1.18f, 0.72f, procedureDiscipline01) *
                                  Mathf.Lerp(1.12f, 0.78f, situationalAwareness01);

            float efficiency = Mathf.Clamp01(
                0.66f +
                experienceLevel * 0.28f -
                fatigue * 0.22f +
                morale * 0.08f +
                shiftCoordination01 * 0.1f +
                maintenanceReadiness01 * 0.06f);

            return new CrewInfluence
            {
                ReactionDelaySeconds = reactionDelay,
                MistakeChancePerMinute = mistakeChance,
                OperationalEfficiency = efficiency,
                Fatigue = fatigue,
                Morale = morale,
                ExperienceLevel = experienceLevel,
                ProcedureDiscipline01 = procedureDiscipline01,
                SituationalAwareness01 = situationalAwareness01,
                MaintenanceReadiness01 = maintenanceReadiness01,
                ShiftCoordination01 = shiftCoordination01
            };
        }

        public CrewActionReport ExecuteCrewAction(CrewActionType actionType, DrillingState state)
        {
            float quality = CalculateActionQuality(actionType);
            string role;
            string title;
            string message;
            string effect;

            switch (actionType)
            {
                case CrewActionType.MudCheck:
                    role = "Растворщик";
                    title = "Замер раствора";
                    message = $"Плотность {state.MudWeightSG:0.00} SG, эквив. плотность {state.EquivalentCirculatingDensitySG:0.00} SG, газ {state.GasUnitsPercent:0.0}%.";
                    effect = quality > 0.55f
                        ? "Замер принят, дисциплина по раствору повышена."
                        : "Замер сомнительный, возможна ошибка плотности.";
                    procedureDiscipline01 = Mathf.Clamp01(procedureDiscipline01 + quality * 0.12f);
                    situationalAwareness01 = Mathf.Clamp01(situationalAwareness01 + quality * 0.06f);
                    fatigue = Mathf.Clamp01(fatigue + 0.014f);
                    if (quality < 0.42f)
                    {
                        RaiseIncident(new CrewIncident
                        {
                            Type = CrewIncidentType.WrongMudWeight,
                            Severity = AlertSeverity.Warning,
                            Message = "Растворщик дал неуверенный замер плотности. Перепроверьте раствор перед изменением эквивалентной плотности.",
                            MeasuredDepth = state.MeasuredDepth,
                            Probability = 1f - quality
                        });
                    }
                    break;

                case CrewActionType.RigInspection:
                    role = "Механик";
                    title = "Проверка вышки";
                    message = $"Проверены тальблок, верхний привод и линия манифольда. Готовность {quality * 100f:0}%.";
                    effect = "Снижен риск отказа оборудования при резких изменениях параметров.";
                    maintenanceReadiness01 = Mathf.Clamp01(maintenanceReadiness01 + quality * 0.16f);
                    procedureDiscipline01 = Mathf.Clamp01(procedureDiscipline01 + quality * 0.04f);
                    fatigue = Mathf.Clamp01(fatigue + 0.018f);
                    break;

                case CrewActionType.BitRunPlanning:
                    role = "Буровой мастер";
                    title = "Планирование рейса";
                    message = $"Оценен износ долота {state.BitWear01 * 100f:0}% и момент {state.SurfaceTorqueKnM:0.0} кНм.";
                    effect = quality > 0.55f
                        ? "Бригада лучше держит нагрузку и не компенсирует падение скорости проходки агрессивными оборотами."
                        : "План рейса слабый, возрастает вероятность лишней нагрузки на КНБК.";
                    shiftCoordination01 = Mathf.Clamp01(shiftCoordination01 + quality * 0.14f);
                    maintenanceReadiness01 = Mathf.Clamp01(maintenanceReadiness01 + quality * 0.08f);
                    fatigue = Mathf.Clamp01(fatigue + 0.012f);
                    break;

                case CrewActionType.DirectionalSurvey:
                    role = "Инженер ННБ";
                    title = "Замер инклинометрии";
                    message = $"Глубина по стволу {state.MeasuredDepth:0} м, зенит {state.InclinationDegrees:0.0}°, азимут {state.AzimuthDegrees:0}°.";
                    effect = "Улучшена ситуационная осведомленность по траектории и резкости набора угла.";
                    situationalAwareness01 = Mathf.Clamp01(situationalAwareness01 + quality * 0.18f);
                    procedureDiscipline01 = Mathf.Clamp01(procedureDiscipline01 + quality * 0.04f);
                    fatigue = Mathf.Clamp01(fatigue + 0.015f);
                    break;

                case CrewActionType.HoleCleaning:
                    role = "Бурильщик";
                    title = "Промывка ствола";
                    message = $"Контроль выноса шлама {state.CuttingsTransportEfficiency01 * 100f:0}%, расход вход/выход {state.FlowRateLps * 60f:0}/{state.FlowOutLps * 60f:0} л/мин.";
                    effect = quality > 0.5f
                        ? "Промывка выполнена организованно, шламовая постель должна уменьшиться."
                        : "Промывка неуверенная, возможен пропуск падения выхода.";
                    shiftCoordination01 = Mathf.Clamp01(shiftCoordination01 + quality * 0.16f);
                    situationalAwareness01 = Mathf.Clamp01(situationalAwareness01 + quality * 0.08f);
                    fatigue = Mathf.Clamp01(fatigue + 0.03f);
                    if (quality < 0.38f)
                    {
                        RaiseIncident(new CrewIncident
                        {
                            Type = CrewIncidentType.MissedFlowDrop,
                            Severity = AlertSeverity.Warning,
                            Message = "Промывка проведена с плохим контролем расхода на выходе.",
                            MeasuredDepth = state.MeasuredDepth,
                            Probability = 1f - quality
                        });
                    }
                    break;

                case CrewActionType.KickControl:
                    role = "Буровой мастер";
                    title = "Действия при притоке";
                    message = $"Газ {state.GasUnitsPercent:0.0}%, выход {state.FlowOutLps * 60f:0} л/мин при входе {state.FlowRateLps * 60f:0} л/мин.";
                    effect = quality > 0.55f
                        ? "Скважина стабилизируется: закрытие на штуцере и утяжеление раствора выполняются по процедуре."
                        : "Действия запоздали, возможен рост газопоказаний и лишнее давление.";
                    procedureDiscipline01 = Mathf.Clamp01(procedureDiscipline01 + quality * 0.14f);
                    situationalAwareness01 = Mathf.Clamp01(situationalAwareness01 + quality * 0.12f);
                    fatigue = Mathf.Clamp01(fatigue + 0.035f);
                    break;

                case CrewActionType.LossControl:
                    role = "Растворщик";
                    title = "Пачка от поглощения";
                    message = $"Проверены емкости, потери {(state.FlowRateLps - state.FlowOutLps) * 60f:0} л/мин, эквив. плотность {state.EquivalentCirculatingDensitySG:0.00} SG.";
                    effect = quality > 0.5f
                        ? "Подготовлен материал от поглощения, расход снижается без резкого провала очистки."
                        : "Потери оценены грубо, есть риск недолить раствор или сорвать очистку.";
                    procedureDiscipline01 = Mathf.Clamp01(procedureDiscipline01 + quality * 0.12f);
                    shiftCoordination01 = Mathf.Clamp01(shiftCoordination01 + quality * 0.08f);
                    fatigue = Mathf.Clamp01(fatigue + 0.028f);
                    break;

                case CrewActionType.FreeStuckPipe:
                    role = "Бурильщик";
                    title = "Освобождение колонны";
                    message = $"Drag {state.DragTonnes:0.0} т, риск прихвата {state.StuckPipeRisk01 * 100f:0}%, нагрузка {state.WeightOnBitTonnes:0.0} т.";
                    effect = quality > 0.52f
                        ? "Колонну расхаживают малыми ходами, нагрузка с долота снимается без рывков."
                        : "Расхаживание неорганизованное, возможен рост момента и усталость бригады.";
                    shiftCoordination01 = Mathf.Clamp01(shiftCoordination01 + quality * 0.14f);
                    maintenanceReadiness01 = Mathf.Clamp01(maintenanceReadiness01 + quality * 0.05f);
                    fatigue = Mathf.Clamp01(fatigue + 0.045f);
                    break;

                case CrewActionType.StickSlipMitigation:
                    role = "Инженер ННБ";
                    title = "Снижение автоколебаний";
                    message = $"Момент {state.SurfaceTorqueKnM:0.0} кНм, низкочастотная вибрация {state.Vibration.LowFrequencyEnergy * 3.5f:0.0} g.";
                    effect = quality > 0.52f
                        ? "Обороты и нагрузка снижаются ступенчато, колебания должны затухнуть."
                        : "Коррекция слишком грубая, возможно повторное возбуждение автоколебаний.";
                    situationalAwareness01 = Mathf.Clamp01(situationalAwareness01 + quality * 0.16f);
                    shiftCoordination01 = Mathf.Clamp01(shiftCoordination01 + quality * 0.08f);
                    fatigue = Mathf.Clamp01(fatigue + 0.022f);
                    break;

                case CrewActionType.BackreamAndReam:
                    role = "Буровой мастер";
                    title = "Проработка ствола";
                    message = $"Зенит {state.InclinationDegrees:0.0}°, вынос шлама {state.CuttingsTransportEfficiency01 * 100f:0}%, давление насоса {state.StandpipePressureBar:0.0} бар.";
                    effect = quality > 0.5f
                        ? "Интервал проработан, шламовая постель и локальные посадки должны уменьшиться."
                        : "Проработка неполная, повышается вероятность повторного сальникообразования.";
                    shiftCoordination01 = Mathf.Clamp01(shiftCoordination01 + quality * 0.12f);
                    situationalAwareness01 = Mathf.Clamp01(situationalAwareness01 + quality * 0.1f);
                    fatigue = Mathf.Clamp01(fatigue + 0.038f);
                    break;

                default:
                    role = "Буровой мастер";
                    title = "Инструктаж смены";
                    message = "Проведен короткий разбор: окно давлений, признаки притока, порядок действий при потере циркуляции.";
                    effect = "Мораль и дисциплина смены улучшены, реакция становится предсказуемее.";
                    ApplyRest(0.06f);
                    procedureDiscipline01 = Mathf.Clamp01(procedureDiscipline01 + quality * 0.18f);
                    shiftCoordination01 = Mathf.Clamp01(shiftCoordination01 + quality * 0.12f);
                    morale = Mathf.Clamp01(morale + 0.08f);
                    break;
            }

            experienceLevel = Mathf.Clamp01(experienceLevel + quality * 0.002f);

            CrewActionReport report = new CrewActionReport(
                actionType,
                role,
                title,
                message,
                effect,
                quality,
                fatigue);

            eventChannel?.Raise(
                SimulationEventType.CrewActionCompleted,
                quality > 0.42f ? AlertSeverity.Info : AlertSeverity.Warning,
                $"{role}: {title}. {effect}",
                report);

            return report;
        }

        public void EnqueueCommand(DrillingCommand command, Action<DrillingCommand> applyCommand)
        {
            CrewInfluence influence = GetInfluence();
            float delay = influence.ReactionDelaySeconds;
            DrillingCommand finalCommand = command;

            if (UnityEngine.Random.value < influence.MistakeChancePerMinute * 0.6f)
            {
                float error = UnityEngine.Random.Range(-0.16f, 0.16f) * Mathf.Lerp(1.4f, 0.45f, experienceLevel);
                finalCommand.TargetValue *= 1f + error;

                RaiseIncident(new CrewIncident
                {
                    Type = CrewIncidentType.MissedSetpoint,
                    Severity = AlertSeverity.Warning,
                    Message = $"Отклонение уставки бригадой: {ToRussianCommand(command.Type)}",
                    Probability = influence.MistakeChancePerMinute
                });
            }

            if (command.Type == DrillingCommandType.SetMudWeight &&
                fatigue > 0.48f &&
                UnityEngine.Random.value < influence.MistakeChancePerMinute * 0.45f)
            {
                finalCommand.TargetValue += UnityEngine.Random.Range(-0.04f, 0.04f);
                RaiseIncident(new CrewIncident
                {
                    Type = CrewIncidentType.WrongMudWeight,
                    Severity = AlertSeverity.Warning,
                    Message = "Ошибка замера раствора: плотность введена с отклонением, безопасное окно давлений сузилось.",
                    Probability = influence.MistakeChancePerMinute
                });
            }

            if (fatigue > 0.68f && UnityEngine.Random.value < fatigue * 0.2f)
            {
                delay *= 1.8f;
                RaiseIncident(new CrewIncident
                {
                    Type = CrewIncidentType.DelayedResponse,
                    Severity = AlertSeverity.Advisory,
                    Message = "Реакция бригады замедлена из-за высокой усталости.",
                    Probability = influence.MistakeChancePerMinute
                });
            }

            pendingCommands.Add(new PendingCrewCommand
            {
                ExecuteAt = Time.time + delay,
                Command = finalCommand,
                ApplyCommand = applyCommand
            });
        }

        public void ApplyRest(float recovery01)
        {
            fatigue = Mathf.Clamp01(fatigue - recovery01);
            morale = Mathf.Clamp01(morale + recovery01 * 0.35f);
        }

        private void Update()
        {
            ExecutePendingCommands();
            RollForOperationalIncident();

            if (Time.time - lastEventTime > 1f)
            {
                eventChannel?.Raise(
                    SimulationEventType.CrewStateUpdated,
                    AlertSeverity.Info,
                    "Состояние бригады обновлено.",
                    GetInfluence());
                lastEventTime = Time.time;
            }
        }

        private void ExecutePendingCommands()
        {
            for (int i = pendingCommands.Count - 1; i >= 0; i--)
            {
                PendingCrewCommand pending = pendingCommands[i];
                if (Time.time < pending.ExecuteAt)
                {
                    continue;
                }

                pending.ApplyCommand?.Invoke(pending.Command);
                pendingCommands.RemoveAt(i);
            }
        }

        private void RollForOperationalIncident()
        {
            CrewInfluence influence = GetInfluence();
            float probabilityThisFrame = influence.MistakeChancePerMinute * Time.deltaTime / 60f;

            if (UnityEngine.Random.value > probabilityThisFrame)
            {
                return;
            }

            float roll = UnityEngine.Random.value;
            CrewIncidentType type;
            if (roll < 0.28f)
            {
                type = CrewIncidentType.BitBalling;
            }
            else if (roll < 0.52f)
            {
                type = CrewIncidentType.WallSloughing;
            }
            else if (roll < 0.74f)
            {
                type = CrewIncidentType.MissedFlowDrop;
            }
            else if (roll < 0.9f)
            {
                type = CrewIncidentType.WrongMudWeight;
            }
            else
            {
                type = CrewIncidentType.UnsafeRamp;
            }

            RaiseIncident(new CrewIncident
            {
                Type = type,
                Severity = AlertSeverity.Warning,
                Message = ToRussianIncident(type),
                Probability = influence.MistakeChancePerMinute
            });
        }

        private float CalculateActionQuality(CrewActionType actionType)
        {
            float disciplineWeight = actionType == CrewActionType.MudCheck || actionType == CrewActionType.ShiftBriefing
                ? procedureDiscipline01 * 0.14f
                : procedureDiscipline01 * 0.07f;
            float awarenessWeight = actionType == CrewActionType.DirectionalSurvey || actionType == CrewActionType.HoleCleaning
                ? situationalAwareness01 * 0.16f
                : situationalAwareness01 * 0.07f;
            float maintenanceWeight = actionType == CrewActionType.RigInspection || actionType == CrewActionType.BitRunPlanning
                ? maintenanceReadiness01 * 0.14f
                : maintenanceReadiness01 * 0.04f;

            return Mathf.Clamp01(
                0.24f +
                experienceLevel * 0.34f +
                morale * 0.18f -
                fatigue * 0.28f +
                shiftCoordination01 * 0.08f +
                disciplineWeight +
                awarenessWeight +
                maintenanceWeight +
                UnityEngine.Random.Range(-0.06f, 0.06f));
        }

        private static string ToRussianCommand(DrillingCommandType type)
        {
            switch (type)
            {
                case DrillingCommandType.SetRpm:
                    return "обороты ротора";
                case DrillingCommandType.SetWeightOnBit:
                    return "нагрузка на долото";
                case DrillingCommandType.SetFlowRate:
                    return "расход насосов";
                case DrillingCommandType.SetMudWeight:
                    return "плотность раствора";
                case DrillingCommandType.SetChokeOpening:
                    return "положение штуцера";
                default:
                    return "буровой параметр";
            }
        }

        private static string ToRussianIncident(CrewIncidentType type)
        {
            switch (type)
            {
                case CrewIncidentType.BitBalling:
                    return "Вероятное сальникообразование: бригада поздно скорректировала нагрузку на долото и промывку.";
                case CrewIncidentType.WallSloughing:
                    return "Признаки осыпи стенок после задержки циркуляции.";
                case CrewIncidentType.MissedFlowDrop:
                    return "Бригада поздно заметила падение выхода раствора. Возможны поглощения.";
                case CrewIncidentType.WrongMudWeight:
                    return "Плотность раствора замерена с ошибкой. Растет риск выхода из окна давлений.";
                case CrewIncidentType.UnsafeRamp:
                    return "Слишком резкое изменение параметров. Возросла ударная нагрузка на КНБК.";
                default:
                    return "Бригада допустила отклонение от безопасной процедуры.";
            }
        }

        private void RaiseIncident(CrewIncident incident)
        {
            IncidentRaised?.Invoke(incident);
            eventChannel?.Raise(
                SimulationEventType.CrewIncident,
                incident.Severity,
                incident.Message,
                incident);
        }

        private readonly struct CrewShiftProfile
        {
            public readonly string Name;
            public readonly string Driller;
            public readonly string Toolpusher;
            public readonly string MudEngineer;
            public readonly string DirectionalEngineer;
            public readonly float ExperienceOffset;
            public readonly float FatigueOffset;
            public readonly float MoraleOffset;
            public readonly float DisciplineOffset;
            public readonly float AwarenessOffset;
            public readonly float MaintenanceOffset;
            public readonly float CoordinationOffset;

            public CrewShiftProfile(
                string name,
                string driller,
                string toolpusher,
                string mudEngineer,
                string directionalEngineer,
                float experienceOffset,
                float fatigueOffset,
                float moraleOffset,
                float disciplineOffset,
                float awarenessOffset,
                float maintenanceOffset,
                float coordinationOffset)
            {
                Name = name;
                Driller = driller;
                Toolpusher = toolpusher;
                MudEngineer = mudEngineer;
                DirectionalEngineer = directionalEngineer;
                ExperienceOffset = experienceOffset;
                FatigueOffset = fatigueOffset;
                MoraleOffset = moraleOffset;
                DisciplineOffset = disciplineOffset;
                AwarenessOffset = awarenessOffset;
                MaintenanceOffset = maintenanceOffset;
                CoordinationOffset = coordinationOffset;
            }
        }

        private sealed class PendingCrewCommand
        {
            public float ExecuteAt;
            public DrillingCommand Command;
            public Action<DrillingCommand> ApplyCommand;
        }
    }
}
