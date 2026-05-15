using System;
using UnityEngine;

namespace SCADASim.Core
{
    public enum SimulationEventType
    {
        EnvironmentLoaded,
        WellboreGenerated,
        DrillingStateUpdated,
        CrewStateUpdated,
        CrewIncident,
        CrewActionCompleted,
        OperationalIncident,
        SupervisorTask,
        SupervisorTaskResult,
        EdgeAIAlert
    }

    public enum AlertSeverity
    {
        Info,
        Advisory,
        Warning,
        Critical
    }

    public readonly struct SimulationEvent
    {
        public readonly SimulationEventType Type;
        public readonly AlertSeverity Severity;
        public readonly string Message;
        public readonly object Payload;

        public SimulationEvent(
            SimulationEventType type,
            AlertSeverity severity,
            string message,
            object payload = null)
        {
            Type = type;
            Severity = severity;
            Message = message;
            Payload = payload;
        }
    }

    [CreateAssetMenu(menuName = "SCADA Simulation/Core/Simulation Event Channel")]
    public sealed class SimulationEventChannel : ScriptableObject
    {
        public event Action<SimulationEvent> Raised;

        public void Raise(SimulationEvent simulationEvent)
        {
            Raised?.Invoke(simulationEvent);
        }

        public void Raise(
            SimulationEventType type,
            AlertSeverity severity,
            string message,
            object payload = null)
        {
            Raise(new SimulationEvent(type, severity, message, payload));
        }
    }
}
