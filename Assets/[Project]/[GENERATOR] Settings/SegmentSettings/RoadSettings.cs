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
    [Space]
    [Tooltip("Amount of points on the side of a RoadchainBlock")]
    public int sidePointAmount;
	public int cornerBuffer;
    public int gridSize;//Unity UnitSize

    public int straight_NpointsBetween;
    public int corner_NpointsBetween;

    [Header("New segment settings")]
    [Range(0f, 300f)]
    public float segmentXaxisVariation;

    [Range(0f, 25f)]
    public float segmentHeightRange;
    public bool isFixedBetweenRange;

    [Range(10, 50)]
    public int segmentDeletionProximity;
}