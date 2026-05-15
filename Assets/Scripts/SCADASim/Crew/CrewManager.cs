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

        [Header("Integration")]
        [SerializeField] private SimulationEventChannel eventChannel;

        private readonly List<PendingCrewCommand> pendingCommands = new List<PendingCrewCommand>();
        private float procedureDiscipline01 = 0.58f;
        private float situationalAwareness01 = 0.55f;
        private float maintenanceReadiness01 = 0.62f;
        private float shiftCoordination01 = 0.6f;
        private float lastEventTime;

        public event Action<CrewIncident> IncidentRaised;

        public float ExperienceLevel => experienceLevel;
        public float Fatigue => fatigue;
        public float Morale => morale;

        public void Configure(SimulationEventChannel channel)
        {
            eventChannel = channel;
        }

        public void ApplyPreset(CrewPreset preset)
        {
            switch (preset)
            {
                case CrewPreset.FullSeven:
                    experienceLevel = 0.78f;
                    fatigue = 0.12f;
                    morale = 0.82f;
                    procedureDiscipline01 = 0.72f;
                    situationalAwareness01 = 0.68f;
                    maintenanceReadiness01 = 0.76f;
                    shiftCoordination01 = 0.74f;
                    break;

                case CrewPreset.StandardFour:
                    experienceLevel = 0.62f;
                    fatigue = 0.24f;
                    morale = 0.72f;
                    procedureDiscipline01 = 0.6f;
                    situationalAwareness01 = 0.56f;
                    maintenanceReadiness01 = 0.62f;
                    shiftCoordination01 = 0.58f;
                    break;

                case CrewPreset.ReducedThree:
                    experienceLevel = 0.54f;
                    fatigue = 0.36f;
                    morale = 0.64f;
                    procedureDiscipline01 = 0.48f;
                    situationalAwareness01 = 0.46f;
                    maintenanceReadiness01 = 0.52f;
                    shiftCoordination01 = 0.44f;
                    break;

                case CrewPreset.TraineeShift:
                    experienceLevel = 0.36f;
                    fatigue = 0.22f;
                    morale = 0.76f;
                    procedureDiscipline01 = 0.42f;
                    situationalAwareness01 = 0.38f;
                    maintenanceReadiness01 = 0.5f;
                    shiftCoordination01 = 0.46f;
                    break;
            }
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
                    message = $"Плотность {state.MudWeightSG:0.00} SG, ECD {state.EquivalentCirculatingDensitySG:0.00} SG, газ {state.GasUnitsPercent:0.0}%.";
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
                            Message = "Растворщик дал неуверенный замер плотности. Перепроверьте раствор перед изменением ECD.",
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
                        ? "Бригада лучше держит нагрузку и не компенсирует падение ROP агрессивными оборотами."
                        : "План рейса слабый, возрастает вероятность лишней нагрузки на КНБК.";
                    shiftCoordination01 = Mathf.Clamp01(shiftCoordination01 + quality * 0.14f);
                    maintenanceReadiness01 = Mathf.Clamp01(maintenanceReadiness01 + quality * 0.08f);
                    fatigue = Mathf.Clamp01(fatigue + 0.012f);
                    break;

                case CrewActionType.DirectionalSurvey:
                    role = "Инженер ННБ";
                    title = "Замер инклинометрии";
                    message = $"MD {state.MeasuredDepth:0} м, зенит {state.InclinationDegrees:0.0}°, азимут {state.AzimuthDegrees:0}°.";
                    effect = "Улучшена ситуационная осведомленность по траектории и dogleg.";
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
                    message = $"Проверены емкости, потери {(state.FlowRateLps - state.FlowOutLps) * 60f:0} л/мин, ECD {state.EquivalentCirculatingDensitySG:0.00} SG.";
                    effect = quality > 0.5f
                        ? "Подготовлена LCM-пачка, расход снижается без резкого провала очистки."
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
                    title = "Снижение Stick-Slip";
                    message = $"Момент {state.SurfaceTorqueKnM:0.0} кНм, низкочастотная вибрация {state.Vibration.LowFrequencyEnergy * 3.5f:0.0} g.";
                    effect = quality > 0.52f
                        ? "Обороты и нагрузка снижаются ступенчато, колебания должны затухнуть."
                        : "Коррекция слишком грубая, возможно повторное возбуждение Stick-Slip.";
                    situationalAwareness01 = Mathf.Clamp01(situationalAwareness01 + quality * 0.16f);
                    shiftCoordination01 = Mathf.Clamp01(shiftCoordination01 + quality * 0.08f);
                    fatigue = Mathf.Clamp01(fatigue + 0.022f);
                    break;

                case CrewActionType.BackreamAndReam:
                    role = "Буровой мастер";
                    title = "Проработка ствола";
                    message = $"Зенит {state.InclinationDegrees:0.0}°, вынос шлама {state.CuttingsTransportEfficiency01 * 100f:0}%, SPP {state.StandpipePressureBar:0.0} бар.";
                    effect = quality > 0.5f
                        ? "Интервал проработан, шламовая постель и локальные посадки должны уменьшиться."
                        : "Проработка неполная, повышается вероятность повторного pack-off.";
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
            fatigue = Mathf.Clamp01(fatigue + fatigueGrowthPerRealMinute * Time.deltaTime / 60f);
            morale = Mathf.Clamp01(morale + moraleRecoveryPerRealMinute * Time.deltaTime / 60f - fatigue * 0.002f * Time.deltaTime / 60f);
            procedureDiscipline01 = Mathf.MoveTowards(procedureDiscipline01, 0.52f, Time.deltaTime * 0.002f / 60f);
            situationalAwareness01 = Mathf.MoveTowards(situationalAwareness01, 0.5f, Time.deltaTime * 0.003f / 60f);
            maintenanceReadiness01 = Mathf.MoveTowards(maintenanceReadiness01, 0.56f, Time.deltaTime * 0.0015f / 60f);
            shiftCoordination01 = Mathf.MoveTowards(shiftCoordination01, 0.54f, Time.deltaTime * 0.0025f / 60f);

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

        private sealed class PendingCrewCommand
        {
            public float ExecuteAt;
            public DrillingCommand Command;
            public Action<DrillingCommand> ApplyCommand;
        }
    }
}
