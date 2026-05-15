using System;
using System.Collections.Generic;
using SCADASim.Core;
using UnityEngine;

namespace SCADASim.Trajectory
{
    public static class WellboreTrajectorySolver
    {
        private const float Epsilon = 0.00001f;

        public static Vector3 DirectionFromSurveyAngles(float inclinationDegrees, float azimuthDegrees)
        {
            float inc = inclinationDegrees * Mathf.Deg2Rad;
            float az = azimuthDegrees * Mathf.Deg2Rad;

            float east = Mathf.Sin(inc) * Mathf.Sin(az);
            float north = Mathf.Sin(inc) * Mathf.Cos(az);
            float down = Mathf.Cos(inc);

            // Unity convention used here: X east, Z north, Y up.
            return new Vector3(east, -down, north).normalized;
        }

        public static void AnglesFromDirection(
            Vector3 direction,
            out float inclinationDegrees,
            out float azimuthDegrees)
        {
            Vector3 normalized = direction.normalized;
            float down = Mathf.Clamp(-normalized.y, -1f, 1f);
            inclinationDegrees = Mathf.Acos(down) * Mathf.Rad2Deg;

            float azimuth = Mathf.Atan2(normalized.x, normalized.z) * Mathf.Rad2Deg;
            azimuthDegrees = azimuth < 0f ? azimuth + 360f : azimuth;
        }

        public static List<TrajectorySample> BuildSamples(
            IReadOnlyList<WellboreSurveyPoint> surveyPoints,
            int samplesPerSurveySegment)
        {
            if (surveyPoints == null || surveyPoints.Count < 2)
            {
                throw new ArgumentException("At least two survey points are required.");
            }

            List<WellboreSurveyPoint> ordered = new List<WellboreSurveyPoint>(surveyPoints);
            ordered.Sort((left, right) => left.MeasuredDepth.CompareTo(right.MeasuredDepth));

            int stepsPerSegment = Mathf.Max(1, samplesPerSurveySegment);
            List<TrajectorySample> samples = new List<TrajectorySample>(ordered.Count * stepsPerSegment);

            WellboreSurveyPoint first = ordered[0];
            Vector3 position = Vector3.zero;
            Vector3 previousDirection = DirectionFromSurveyAngles(
                first.InclinationDegrees,
                first.AzimuthDegrees);
            float previousMd = first.MeasuredDepth;

            samples.Add(new TrajectorySample(
                previousMd,
                position,
                previousDirection,
                first.InclinationDegrees,
                first.AzimuthDegrees,
                0f));

            for (int i = 0; i < ordered.Count - 1; i++)
            {
                WellboreSurveyPoint start = ordered[i];
                WellboreSurveyPoint end = ordered[i + 1];
                float intervalMd = end.MeasuredDepth - start.MeasuredDepth;

                if (intervalMd <= Epsilon)
                {
                    continue;
                }

                for (int step = 1; step <= stepsPerSegment; step++)
                {
                    float t = step / (float)stepsPerSegment;
                    float currentMd = Mathf.Lerp(start.MeasuredDepth, end.MeasuredDepth, t);
                    float inclination = Mathf.Lerp(start.InclinationDegrees, end.InclinationDegrees, t);
                    float azimuth = Mathf.LerpAngle(start.AzimuthDegrees, end.AzimuthDegrees, t);
                    Vector3 currentDirection = DirectionFromSurveyAngles(inclination, azimuth);

                    float deltaMd = currentMd - previousMd;
                    position += MinimumCurvatureDisplacement(previousDirection, currentDirection, deltaMd);

                    float doglegSeverity = CalculateDoglegSeverityDegPer30m(
                        previousDirection,
                        currentDirection,
                        deltaMd);

                    samples.Add(new TrajectorySample(
                        currentMd,
                        position,
                        currentDirection,
                        inclination,
                        NormalizeAzimuth(azimuth),
                        doglegSeverity));

                    previousMd = currentMd;
                    previousDirection = currentDirection;
                }
            }

            return samples;
        }

        public static bool TryEvaluateAtMeasuredDepth(
            IReadOnlyList<TrajectorySample> samples,
            float measuredDepth,
            out TrajectorySample sample)
        {
            if (samples == null || samples.Count == 0)
            {
                sample = default;
                return false;
            }

            if (measuredDepth <= samples[0].MeasuredDepth)
            {
                sample = samples[0];
                return true;
            }

            int lastIndex = samples.Count - 1;
            if (measuredDepth >= samples[lastIndex].MeasuredDepth)
            {
                sample = samples[lastIndex];
                return true;
            }

            for (int i = 1; i < samples.Count; i++)
            {
                TrajectorySample current = samples[i];
                if (current.MeasuredDepth < measuredDepth)
                {
                    continue;
                }

                TrajectorySample previous = samples[i - 1];
                float t = Mathf.InverseLerp(previous.MeasuredDepth, current.MeasuredDepth, measuredDepth);
                Vector3 tangent = Vector3.Slerp(previous.Tangent, current.Tangent, t).normalized;
                AnglesFromDirection(tangent, out float inclination, out float azimuth);

                sample = new TrajectorySample(
                    measuredDepth,
                    Vector3.Lerp(previous.Position, current.Position, t),
                    tangent,
                    inclination,
                    azimuth,
                    Mathf.Lerp(previous.DoglegSeverityDegPer30m, current.DoglegSeverityDegPer30m, t));
                return true;
            }

            sample = samples[lastIndex];
            return true;
        }

        public static List<WellboreSurveyPoint> BuildStandardProfile(
            WellboreProfileType profileType,
            float maxMeasuredDepth,
            float stationInterval,
            float azimuthDegrees = 45f)
        {
            float mdMax = Mathf.Max(10f, maxMeasuredDepth);
            float interval = Mathf.Max(1f, stationInterval);
            List<WellboreSurveyPoint> points = new List<WellboreSurveyPoint>();

            for (float md = 0f; md < mdMax; md += interval)
            {
                points.Add(new WellboreSurveyPoint(
                    md,
                    CalculateProfileInclination(profileType, md, mdMax),
                    azimuthDegrees));
            }

            points.Add(new WellboreSurveyPoint(
                mdMax,
                CalculateProfileInclination(profileType, mdMax, mdMax),
                azimuthDegrees));

            return points;
        }

        private static Vector3 MinimumCurvatureDisplacement(
            Vector3 previousDirection,
            Vector3 currentDirection,
            float deltaMeasuredDepth)
        {
            float doglegRadians = Mathf.Acos(Mathf.Clamp(
                Vector3.Dot(previousDirection.normalized, currentDirection.normalized),
                -1f,
                1f));

            float ratioFactor = doglegRadians < Epsilon
                ? 1f
                : 2f / doglegRadians * Mathf.Tan(doglegRadians * 0.5f);

            return 0.5f * deltaMeasuredDepth * ratioFactor * (previousDirection + currentDirection);
        }

        private static float CalculateDoglegSeverityDegPer30m(
            Vector3 previousDirection,
            Vector3 currentDirection,
            float deltaMeasuredDepth)
        {
            if (deltaMeasuredDepth <= Epsilon)
            {
                return 0f;
            }

            float doglegRadians = Mathf.Acos(Mathf.Clamp(
                Vector3.Dot(previousDirection.normalized, currentDirection.normalized),
                -1f,
                1f));

            return doglegRadians * Mathf.Rad2Deg / deltaMeasuredDepth * 30f;
        }

        private static float CalculateProfileInclination(
            WellboreProfileType profileType,
            float measuredDepth,
            float maxMeasuredDepth)
        {
            float md01 = Mathf.Clamp01(measuredDepth / maxMeasuredDepth);

            switch (profileType)
            {
                case WellboreProfileType.Vertical:
                    return 0f;

                case WellboreProfileType.JShape:
                    return md01 < 0.15f
                        ? 0f
                        : Mathf.Lerp(0f, 65f, Smooth01(Mathf.InverseLerp(0.15f, 0.55f, md01)));

                case WellboreProfileType.SShape:
                    if (md01 < 0.2f)
                    {
                        return 0f;
                    }

                    if (md01 < 0.45f)
                    {
                        return Mathf.Lerp(0f, 55f, Smooth01(Mathf.InverseLerp(0.2f, 0.45f, md01)));
                    }

                    if (md01 < 0.7f)
                    {
                        return 55f;
                    }

                    return Mathf.Lerp(55f, 0f, Smooth01(Mathf.InverseLerp(0.7f, 0.95f, md01)));

                case WellboreProfileType.Horizontal:
                    return md01 < 0.12f
                        ? 0f
                        : Mathf.Lerp(0f, 90f, Smooth01(Mathf.InverseLerp(0.12f, 0.62f, md01)));

                default:
                    return 0f;
            }
        }

        private static float Smooth01(float value)
        {
            float t = Mathf.Clamp01(value);
            return t * t * (3f - 2f * t);
        }

        private static float NormalizeAzimuth(float azimuthDegrees)
        {
            float normalized = azimuthDegrees % 360f;
            return normalized < 0f ? normalized + 360f : normalized;
        }
    }
}
