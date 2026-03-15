using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu, System.Serializable]
public class RoadSettings : ScriptableObject
{
	[Header("Appearance")]
    public VertexPoint[] points;
	[SerializeField] private SurfaceScriptableSector surfaceSettings;
	public int PointCount => points.Length;

	[Header("UI Color")]
	public Material UIMaterial;

	[Header("Meshtasks")]
	public List<MeshtaskObject> meshtaskObjects = new List<MeshtaskObject>();

	public float guardRailMinimalCornerRadius;

	[Space]
	public string roadTypeTag;
	public int assetTriggerResolution;
	public List<AssetPool> assetPools = new List<AssetPool>();

	public Ease rotationEasing = Ease.InOut;
	public float edgeLoopsPerMeter;

	[Range(-10f,10f)]
	public float debugRoadCurveStrenght;

	[Header("Noise settings")]
	public List<NoiseChannel> noiseChannels = new List<NoiseChannel>();

	public AnimationCurve runoffAnimationCurve;
	public float extrusionSize;
	public bool hasCornerChamfer;
	public float maxCamber;

	//CalculatedValues
	[HideInInspector] public List<Vector2> calculatedUs = new List<Vector2>();
	[HideInInspector] public float uSpan;
	[HideInInspector] public int hardEdges;

	public SegmentChainSettings segmentChainSettings;

    public void InitializeRoadSettings()
    {
		CalculateUs();
		hardEdges = CalculateLine();
		CalcUspan();

		foreach (MeshtaskObject mo in meshtaskObjects)
		{
			mo.meshtaskSettings.CalculateLine();
			mo.meshtaskSettings.CalculateUV_Distance();

			mo.meshtaskSettings.maxChamfer = maxCamber;
			mo.meshtaskSettings.extrusionSize = extrusionSize;
		}
	}

	public List<SurfaceScriptable> GetAllSurfaceSettings()
    {
		List<SurfaceScriptable> fc = new List<SurfaceScriptable>();
        fc.Add(surfaceSettings.runoffMaterial);
		fc.AddRange(surfaceSettings.layers);
		return fc;
	}

	/// <summary>
	/// Total U Lenght of object AKA horizontal
	/// </summary>
	/// <returns></returns>
	private void CalcUspan()
	{
		float dist = 0;
		for (int i = 0; i < PointCount - 1; i++)
		{
			Vector2 a = points[i].vertex_1.point;
			Vector2 b = points[i + 1].vertex_1.point;
			dist += (a - b).magnitude;
		}
		uSpan = Mathf.Abs(dist);
	}

	public int CalculateLine()
	{
		int skippedPoints = 0;
		for (int i = 0; i < PointCount; i++)
		{
			if (points[i].ishardEdge)
				skippedPoints++;
			else if (i != 0 && points[i].materialIndex != points[i - 1].materialIndex)
				skippedPoints++;
			else if (i != 0 && points[i].extrudePoint)
				skippedPoints++;
			else if (i != 0 && points[i - 1].extrudePoint)
				skippedPoints++;

			int currentVertex = i + skippedPoints;
			int nextVertex = 1 + i + skippedPoints;

			if (i == PointCount - 1)//Last point needs to connect to first point
				points[i].line = new Vector2Int(currentVertex, 0);
			else
				points[i].line = new Vector2Int(currentVertex,  nextVertex);
		}
		return skippedPoints;
	}

	private void CalculateUs()
	{
		int current = 0;
		List<Vector2> uvs = new List<Vector2>();
		List<SurfaceScriptable> sfsc = new List<SurfaceScriptable>();
		sfsc.AddRange(GetAllSurfaceSettings());

		foreach (SurfaceScriptable m in sfsc)
		{
			if (m.UV_mirrored)
			{
				uvs.Add(CalculateMirroredUV_Width(points, current));
			}
			else
			{
				uvs.Add(CalculateFullUV_Width(points, current));
			}
			current++;
		}
		calculatedUs = uvs;
	}

	private Vector2 CalculateFullUV_Width(VertexPoint[] points, int materialIndex)
	{
		float min_Xuv = float.PositiveInfinity;
		float max_Xuv = float.NegativeInfinity;
		int current = 0;
		foreach (VertexPoint p in points)
		{
			if (p.materialIndex == materialIndex)
			{
				if ((p.vertex_1.point.x) < min_Xuv)
					min_Xuv = p.vertex_1.point.x;
				if ((p.vertex_1.point.x) > max_Xuv)
					max_Xuv = p.vertex_1.point.x;
			}
			else if (current != 0 && points[current - 1].materialIndex == materialIndex)
			{
				if ((p.vertex_1.point.x) < min_Xuv)
					min_Xuv = p.vertex_1.point.x;
				if ((p.vertex_1.point.x) > max_Xuv)
					max_Xuv = p.vertex_1.point.x;
			}
			current++;
		}
		return new Vector2(min_Xuv, max_Xuv);
	}

	private Vector2 CalculateMirroredUV_Width(VertexPoint[] points, int materialIndex)
	{
		float y = 0f;
		float min_Xuv = float.PositiveInfinity;
		float max_Xuv = float.NegativeInfinity;
		foreach (VertexPoint p in points)
		{
			if (p.materialIndex == materialIndex)
			{
				y = Mathf.Abs(y - (y - p.vertex_1.point.y));
				float abs = Mathf.Abs(p.vertex_1.point.x + y);
				if (abs < min_Xuv)
					min_Xuv = abs;
				if (abs > max_Xuv)
					max_Xuv = abs;
			}
		}
		return new Vector2(min_Xuv, max_Xuv);
	}
}

[System.Serializable]
public class SegmentChainSettings
{
    [FoldoutGroup("Grid"), LabelText("Side Point Amount"), MinValue(1)]
    [Tooltip("Amount of points on each side of a road-chain block.")]
    public int sidePointAmount;
    [FoldoutGroup("Grid"), LabelText("Corner Buffer"), MinValue(0)]
    [Tooltip("Excludes edge indices near corners to avoid extreme turns.")]
	public int cornerBuffer;
    [FoldoutGroup("Grid"), LabelText("Grid Size"), MinValue(1)]
    [Tooltip("World-space size of one chain block.")]
    public int gridSize;//Unity UnitSize

    [FoldoutGroup("Point Density"), LabelText("Straight Points"), MinValue(0)]
    public int straight_NpointsBetween;
    [FoldoutGroup("Point Density"), LabelText("Corner Points"), MinValue(0)]
    public int corner_NpointsBetween;

    [FoldoutGroup("Exit Index"), PropertyRange(0f, 1f)]
    [LabelText("Corner Outside Bias")]
    [Tooltip("0 keeps closer to entry index, 1 pushes corners toward the outside.")]
    public float cornerOutsideBias = 0.65f;
    [FoldoutGroup("Exit Index"), PropertyRange(0, 4)]
    [LabelText("Straight Jitter")]
    public int straightIndexJitter = 1;
    [FoldoutGroup("Exit Index"), PropertyRange(0, 6)]
    [LabelText("Corner Jitter")]
    public int cornerIndexJitter = 2;
    [FoldoutGroup("Exit Index"), PropertyRange(0f, 0.45f)]
    [LabelText("Edge Margin")]
    [Tooltip("Normalized margin from edge corners reserved to avoid clipping turns.")]
    public float edgeMarginNormalized = 0.12f;
    [FoldoutGroup("Exit Index"), PropertyRange(0f, 0.5f)]
    [LabelText("Straight T Jitter")]
    [Tooltip("Normalized randomization of exit t for straight transitions.")]
    public float edgeTJitterStraight = 0.08f;
    [FoldoutGroup("Exit Index"), PropertyRange(0f, 0.5f)]
    [LabelText("Corner T Jitter")]
    [Tooltip("Normalized randomization of exit t for corner transitions.")]
    public float edgeTJitterCorner = 0.14f;
    [FoldoutGroup("Exit Index"), PropertyRange(0f, 300f)]
    [LabelText("Edge Inward Offset")]
    [Tooltip("Pushes entry/exit points inward from the tile border in world units.")]
    public float edgeInwardOffset = 0f;

    [FoldoutGroup("Path Shape"), PropertyRange(0f, 300f)]
    [LabelText("Lateral Variation")]
    public float segmentXaxisVariation;
    [FoldoutGroup("Path Shape"), PropertyRange(0f, 2f)]
    [LabelText("Bend Strength")]
    public float segmentBendStrength = .6f;
    [FoldoutGroup("Path Shape"), PropertyRange(0.1f, 6f)]
    [LabelText("Bend Frequency")]
    public float segmentBendFrequency = 1.2f;
    [FoldoutGroup("Path Shape"), PropertyRange(0f, 1f)]
    [LabelText("Micro Bend Ratio")]
    public float segmentMicroBendRatio = 0.1f;
    [FoldoutGroup("Path Shape"), PropertyRange(0f, 1f)]
    [LabelText("Bend Bias")]
    public float segmentBendBias = 0.6f;
    [FoldoutGroup("Path Shape"), PropertyRange(0f, 0.4f)]
    [LabelText("Endpoint Straight Fraction")]
    public float segmentEndpointStraightFraction = 0.14f;
    [FoldoutGroup("Path Shape"), PropertyRange(1f, 4f)]
    [LabelText("Endpoint Ease Power")]
    public float segmentEndpointEasePower = 1.8f;
    [FoldoutGroup("Path Shape"), PropertyRange(0f, 0.45f)]
    [LabelText("Boundary Tangent Fraction")]
    [Tooltip("Fraction of chain length used to force entry/exit tangent alignment.")]
    public float segmentBoundaryTangentFraction = 0.2f;
    [FoldoutGroup("Path Shape"), PropertyRange(0f, 200f)]
    [LabelText("Boundary Tangent Min Dist")]
    [Tooltip("Minimum world-space tangent anchor distance from the entry/exit point.")]
    public float segmentBoundaryTangentMinDistance = 20f;
    [FoldoutGroup("Path Shape"), PropertyRange(0f, 300f)]
    [LabelText("Noise Phase Jitter")]
    [Tooltip("Random phase shift per generated chain. Higher values reduce duplicate track templates.")]
    public float segmentNoisePhaseJitter = 120f;
    [FoldoutGroup("Path Shape"), PropertyRange(0f, 0.4f)]
    [LabelText("Bend Bias Jitter")]
    [Tooltip("Random per-chain variation around Bend Bias.")]
    public float segmentBendBiasJitter = 0.08f;

    [FoldoutGroup("Height"), PropertyRange(0f, 25f)]
    [LabelText("Height Range")]
    public float segmentHeightRange;
    [FoldoutGroup("Height"), LabelText("Fixed Around Zero")]
    public bool isFixedBetweenRange;
    [FoldoutGroup("Height"), LabelText("Use Vertical Profile")]
    [Tooltip("Uses civil-style grade shaping instead of random per-point heights.")]
    public bool useVerticalProfile = true;
    [FoldoutGroup("Height"), PropertyRange(0f, 40f)]
    [LabelText("Max Grade %")]
    [Tooltip("Absolute maximum longitudinal grade in percent.")]
    public float verticalMaxGradePercent = 6f;
    [FoldoutGroup("Height"), PropertyRange(0f, 40f)]
    [LabelText("Max Grade Change %")]
    [Tooltip("Maximum allowed grade delta between profile nodes in percent.")]
    public float verticalMaxGradeChangePercent = 4f;
    [FoldoutGroup("Height"), PropertyRange(0f, 1f)]
    [LabelText("Midpoint Jitter")]
    [Tooltip("Normalized midpoint offset around chain center for crest/sag placement.")]
    public float verticalMidpointJitter = 0.12f;
    [FoldoutGroup("Height"), PropertyRange(0f, 1f)]
    [LabelText("End Grade Return")]
    [Tooltip("How strongly the end grade trends back toward flat (0..1).")]
    public float verticalEndGradeReturn = 0.55f;
    [FoldoutGroup("Height"), PropertyRange(0f, 50f)]
    [LabelText("Max Chain Elevation Delta")]
    [Tooltip("Caps total elevation change from chain start to chain end in world units.")]
    public float verticalMaxChainElevationDelta = 8f;

    [FoldoutGroup("Cleanup"), PropertyRange(10, 50)]
    [LabelText("Deletion Proximity")]
    public int segmentDeletionProximity;
}
