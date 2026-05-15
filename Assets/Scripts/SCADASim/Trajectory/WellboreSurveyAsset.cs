using System;
using System.Collections.Generic;
using UnityEngine;

namespace SCADASim.Trajectory
{
    [Serializable]
    public struct WellboreSurveyPoint
    {
        [Min(0f)] public float MeasuredDepth;
        [Range(0f, 180f)] public float InclinationDegrees;
        [Range(0f, 360f)] public float AzimuthDegrees;

        public WellboreSurveyPoint(float measuredDepth, float inclinationDegrees, float azimuthDegrees)
        {
            MeasuredDepth = measuredDepth;
            InclinationDegrees = inclinationDegrees;
            AzimuthDegrees = azimuthDegrees;
        }
    }

    public readonly struct TrajectorySample
    {
        public readonly float MeasuredDepth;
        public readonly Vector3 Position;
        public readonly Vector3 Tangent;
        public readonly float InclinationDegrees;
        public readonly float AzimuthDegrees;
        public readonly float DoglegSeverityDegPer30m;

        public TrajectorySample(
            float measuredDepth,
            Vector3 position,
            Vector3 tangent,
            float inclinationDegrees,
            float azimuthDegrees,
            float doglegSeverityDegPer30m)
        {
            MeasuredDepth = measuredDepth;
            Position = position;
            Tangent = tangent.normalized;
            InclinationDegrees = inclinationDegrees;
            AzimuthDegrees = azimuthDegrees;
            DoglegSeverityDegPer30m = doglegSeverityDegPer30m;
        }
    }

    [CreateAssetMenu(menuName = "SCADA Simulation/Trajectory/Wellbore Survey")]
    public sealed class WellboreSurveyAsset : ScriptableObject
    {
        [SerializeField] private List<WellboreSurveyPoint> surveyPoints = new List<WellboreSurveyPoint>();

        public IReadOnlyList<WellboreSurveyPoint> SurveyPoints => surveyPoints;
    }
}
