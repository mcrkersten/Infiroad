using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public static class TrackMetricsUtility
{
    private const float Epsilon = 1e-4f;

    public static TrackChainMetrics Evaluate(IReadOnlyList<RoadSegment> segments, float cornerRadiusThresholdMeters, int samplesPerSegment = 0)
    {
        if (segments == null || segments.Count == 0)
            return TrackChainMetrics.Empty;

        List<Vector3> points = BuildMetricPoints(segments, samplesPerSegment);
        if (points.Count < 2)
        {
            for (int i = 0; i < segments.Count; i++)
                points.Add(segments[i].transform.position);
        }

        return Evaluate(points, cornerRadiusThresholdMeters);
    }

    public static TrackChainMetrics Evaluate(IReadOnlyList<Vector3> points, float cornerRadiusThresholdMeters)
    {
        if (points == null || points.Count < 2)
            return TrackChainMetrics.Empty;

        int count = points.Count;
        float[] cumulativeDistance = new float[count];
        float chainLength = 0f;
        float minY = float.PositiveInfinity;
        float maxY = float.NegativeInfinity;

        for (int i = 0; i < count; i++)
        {
            Vector3 p = points[i];
            minY = Mathf.Min(minY, p.y);
            maxY = Mathf.Max(maxY, p.y);

            if (i == 0)
                continue;

            chainLength += Vector3.Distance(points[i - 1], p);
            cumulativeDistance[i] = chainLength;
        }

        List<float> absoluteCurvatures = new List<float>(Mathf.Max(0, count - 2));
        List<float> sampleDistances = new List<float>(Mathf.Max(0, count - 2));
        int belowThresholdCount = 0;
        float maxAbsCurvature = 0f;
        float minCornerRadius = float.PositiveInfinity;

        for (int i = 1; i < count - 1; i++)
        {
            Vector2 p0 = new Vector2(points[i - 1].x, points[i - 1].z);
            Vector2 p1 = new Vector2(points[i].x, points[i].z);
            Vector2 p2 = new Vector2(points[i + 1].x, points[i + 1].z);

            float a = Vector2.Distance(p0, p1);
            float b = Vector2.Distance(p1, p2);
            float c = Vector2.Distance(p0, p2);
            if (a < Epsilon || b < Epsilon || c < Epsilon)
                continue;

            // Curvature approximation for 3 points in a plane.
            float area2 = Mathf.Abs((p1.x - p0.x) * (p2.y - p0.y) - (p1.y - p0.y) * (p2.x - p0.x));
            float curvature = area2 / (a * b * c);
            float absCurvature = Mathf.Abs(curvature);

            absoluteCurvatures.Add(absCurvature);
            sampleDistances.Add(cumulativeDistance[i]);

            maxAbsCurvature = Mathf.Max(maxAbsCurvature, absCurvature);

            if (absCurvature > Epsilon)
            {
                float radius = 1f / absCurvature;
                minCornerRadius = Mathf.Min(minCornerRadius, radius);
                if (radius < cornerRadiusThresholdMeters)
                    belowThresholdCount++;
            }
        }

        float maxCurvatureJerk = 0f;
        for (int i = 1; i < absoluteCurvatures.Count; i++)
        {
            float ds = Mathf.Max(Epsilon, sampleDistances[i] - sampleDistances[i - 1]);
            float jerk = Mathf.Abs(absoluteCurvatures[i] - absoluteCurvatures[i - 1]) / ds;
            maxCurvatureJerk = Mathf.Max(maxCurvatureJerk, jerk);
        }

        int curvaturePeakCount = CountCurvaturePeaks(absoluteCurvatures, cornerRadiusThresholdMeters);

        return new TrackChainMetrics
        {
            PointSampleCount = count,
            ChainLengthMeters = chainLength,
            CurvatureSampleCount = absoluteCurvatures.Count,
            MaxAbsCurvaturePerMeter = maxAbsCurvature,
            MaxCurvatureJerkPerMeterSq = maxCurvatureJerk,
            MinCornerRadiusMeters = minCornerRadius,
            ElevationDeltaMeters = maxY - minY,
            BelowRadiusThresholdCount = belowThresholdCount,
            CurvaturePeakCount = curvaturePeakCount,
        };
    }

    private static List<Vector3> BuildMetricPoints(IReadOnlyList<RoadSegment> segments, int samplesPerSegment)
    {
        List<Vector3> points = new List<Vector3>(Mathf.Max(segments.Count, 2));
        int clampedSamples = Mathf.Max(0, samplesPerSegment);

        for (int i = 0; i < segments.Count; i++)
        {
            RoadSegment segment = segments[i];
            if (segment == null)
                continue;

            bool canUseBezier = clampedSamples >= 2 && segment.HasValidNextPoint;
            if (canUseBezier)
            {
                segment.CreateBezier();
                if (segment.bezier != null)
                {
                    for (int sample = 0; sample < clampedSamples; sample++)
                    {
                        if (points.Count > 0 && sample == 0)
                            continue;

                        float t = sample / (clampedSamples - 1f);
                        Vector3 worldPoint = segment.transform.TransformPoint(segment.bezier.GetPoint(t));
                        points.Add(worldPoint);
                    }
                    continue;
                }
            }

            Vector3 fallback = segment.transform.position;
            if (points.Count == 0 || Vector3.Distance(points[points.Count - 1], fallback) > Epsilon)
                points.Add(fallback);
        }

        return points;
    }

    private static int CountCurvaturePeaks(List<float> absoluteCurvatures, float cornerRadiusThresholdMeters)
    {
        if (absoluteCurvatures == null || absoluteCurvatures.Count < 3)
            return 0;

        float minimumPeakCurvature = cornerRadiusThresholdMeters > Epsilon ? 1f / cornerRadiusThresholdMeters : 0f;
        int peaks = 0;
        for (int i = 1; i < absoluteCurvatures.Count - 1; i++)
        {
            bool isPeak = absoluteCurvatures[i] > absoluteCurvatures[i - 1] &&
                          absoluteCurvatures[i] >= absoluteCurvatures[i + 1] &&
                          absoluteCurvatures[i] >= minimumPeakCurvature;
            if (isPeak)
                peaks++;
        }
        return peaks;
    }
}

public struct TrackChainMetrics
{
    public int PointSampleCount;
    public float ChainLengthMeters;
    public int CurvatureSampleCount;
    public float MaxAbsCurvaturePerMeter;
    public float MaxCurvatureJerkPerMeterSq;
    public float MinCornerRadiusMeters;
    public float ElevationDeltaMeters;
    public int BelowRadiusThresholdCount;
    public int CurvaturePeakCount;

    public float BelowRadiusThresholdPercentage
    {
        get
        {
            if (CurvatureSampleCount <= 0)
                return 0f;
            return (BelowRadiusThresholdCount / (float)CurvatureSampleCount) * 100f;
        }
    }

    public static TrackChainMetrics Empty => new TrackChainMetrics
    {
        PointSampleCount = 0,
        ChainLengthMeters = 0f,
        CurvatureSampleCount = 0,
        MaxAbsCurvaturePerMeter = 0f,
        MaxCurvatureJerkPerMeterSq = 0f,
        MinCornerRadiusMeters = float.PositiveInfinity,
        ElevationDeltaMeters = 0f,
        BelowRadiusThresholdCount = 0,
        CurvaturePeakCount = 0,
    };

    public string ToLogString()
    {
        return
            $"point_samples={PointSampleCount} " +
            $"chain_length_m={Fmt(ChainLengthMeters)} " +
            $"curvature_samples={CurvatureSampleCount} " +
            $"min_corner_radius_m={FmtFinite(MinCornerRadiusMeters)} " +
            $"max_abs_curvature_1pm={Fmt(MaxAbsCurvaturePerMeter)} " +
            $"max_curvature_jerk_1pm2={Fmt(MaxCurvatureJerkPerMeterSq)} " +
            $"elevation_delta_m={Fmt(ElevationDeltaMeters)} " +
            $"below_radius_count={BelowRadiusThresholdCount} " +
            $"below_radius_pct={Fmt(BelowRadiusThresholdPercentage)} " +
            $"curvature_peak_count={CurvaturePeakCount}";
    }

    private static string Fmt(float value)
    {
        return value.ToString("F3", CultureInfo.InvariantCulture);
    }

    private static string FmtFinite(float value)
    {
        if (float.IsInfinity(value) || float.IsNaN(value))
            return "n/a";
        return Fmt(value);
    }
}

public sealed class TrackMetricsAccumulator
{
    private int chainCount;
    private float totalChainLength;
    private float totalElevationDelta;
    private float worstMinCornerRadius = float.PositiveInfinity;
    private float peakAbsCurvature;
    private float peakCurvatureJerk;
    private float totalBelowRadiusPct;
    private int totalCurvatureSamples;
    private int totalBelowRadiusCount;

    public void Reset()
    {
        chainCount = 0;
        totalChainLength = 0f;
        totalElevationDelta = 0f;
        worstMinCornerRadius = float.PositiveInfinity;
        peakAbsCurvature = 0f;
        peakCurvatureJerk = 0f;
        totalBelowRadiusPct = 0f;
        totalCurvatureSamples = 0;
        totalBelowRadiusCount = 0;
    }

    public void Add(TrackChainMetrics metrics)
    {
        chainCount++;
        totalChainLength += metrics.ChainLengthMeters;
        totalElevationDelta += metrics.ElevationDeltaMeters;
        totalBelowRadiusPct += metrics.BelowRadiusThresholdPercentage;
        totalCurvatureSamples += metrics.CurvatureSampleCount;
        totalBelowRadiusCount += metrics.BelowRadiusThresholdCount;

        if (!float.IsNaN(metrics.MinCornerRadiusMeters) && !float.IsInfinity(metrics.MinCornerRadiusMeters))
            worstMinCornerRadius = Mathf.Min(worstMinCornerRadius, metrics.MinCornerRadiusMeters);

        peakAbsCurvature = Mathf.Max(peakAbsCurvature, metrics.MaxAbsCurvaturePerMeter);
        peakCurvatureJerk = Mathf.Max(peakCurvatureJerk, metrics.MaxCurvatureJerkPerMeterSq);
    }

    public TrackMetricsSummary BuildSummary()
    {
        TrackMetricsSummary summary = new TrackMetricsSummary();
        summary.ChainCount = chainCount;

        if (chainCount <= 0)
            return summary;

        summary.AverageChainLengthMeters = totalChainLength / chainCount;
        summary.AverageElevationDeltaMeters = totalElevationDelta / chainCount;
        summary.WorstMinCornerRadiusMeters = worstMinCornerRadius;
        summary.PeakAbsCurvaturePerMeter = peakAbsCurvature;
        summary.PeakCurvatureJerkPerMeterSq = peakCurvatureJerk;
        summary.MeanBelowRadiusThresholdPct = totalBelowRadiusPct / chainCount;
        summary.WeightedBelowRadiusThresholdPct = totalCurvatureSamples > 0
            ? (totalBelowRadiusCount / (float)totalCurvatureSamples) * 100f
            : 0f;
        summary.TotalCurvatureSamples = totalCurvatureSamples;
        return summary;
    }
}

public struct TrackMetricsSummary
{
    public int ChainCount;
    public float AverageChainLengthMeters;
    public float AverageElevationDeltaMeters;
    public float WorstMinCornerRadiusMeters;
    public float PeakAbsCurvaturePerMeter;
    public float PeakCurvatureJerkPerMeterSq;
    public float MeanBelowRadiusThresholdPct;
    public float WeightedBelowRadiusThresholdPct;
    public int TotalCurvatureSamples;

    public string ToLogString()
    {
        return
            $"chain_count={ChainCount} " +
            $"avg_chain_length_m={Fmt(AverageChainLengthMeters)} " +
            $"avg_elevation_delta_m={Fmt(AverageElevationDeltaMeters)} " +
            $"worst_min_corner_radius_m={FmtFinite(WorstMinCornerRadiusMeters)} " +
            $"peak_abs_curvature_1pm={Fmt(PeakAbsCurvaturePerMeter)} " +
            $"peak_curvature_jerk_1pm2={Fmt(PeakCurvatureJerkPerMeterSq)} " +
            $"mean_below_radius_pct={Fmt(MeanBelowRadiusThresholdPct)} " +
            $"weighted_below_radius_pct={Fmt(WeightedBelowRadiusThresholdPct)} " +
            $"total_curvature_samples={TotalCurvatureSamples}";
    }

    private static string Fmt(float value)
    {
        return value.ToString("F3", CultureInfo.InvariantCulture);
    }

    private static string FmtFinite(float value)
    {
        if (float.IsInfinity(value) || float.IsNaN(value))
            return "n/a";
        return Fmt(value);
    }
}
