using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu, System.Serializable]
public class RoadSettings : ScriptableObject
{
	[Header("Appearance")]
    public VertexPoint[] points;
	[SerializeField] private List<SurfaceScriptable> surfaceSettings = new List<SurfaceScriptable>();
	[HideInInspector] public List<SurfaceScriptable> allSurfaceSettings = new List<SurfaceScriptable>();
	public SurfaceScriptable runoffMaterial;
	public int PointCount => points.Length;

	[Header("Meshtasks")]
	public List<MeshtaskSettings> meshtaskSettings = new List<MeshtaskSettings>();
	public float guardRailMinimalCornerRadius;

	[Space]
	public string roadTypeTag;
	public List<VegitationPool> assetPools = new List<VegitationPool>();

	public Ease rotationEasing = Ease.InOut;
	public float edgeLoopsPerMeter;

	public float debugRoadCurveStrenght;

	[Header("Noise settings")]
	public List<NoiseChannel> noiseChannels = new List<NoiseChannel>();

	public AnimationCurve runoffAnimationCurve;
	public float extrusionSize;
	public bool hasCornerChamfer;
	public float maxChamfer;

	//CalculatedValues
	[HideInInspector] public List<Vector2> calculatedUs = new List<Vector2>();
	[HideInInspector] public float uSpan;
	[HideInInspector] public int hardEdges;

	public void InitializeRoadSettings()
    {
		allSurfaceSettings = new List<SurfaceScriptable>();
		allSurfaceSettings.AddRange(surfaceSettings);
		allSurfaceSettings.Add(runoffMaterial);
		CalculateUs();
		hardEdges = CalculateLine();
		CalcUspan();

		foreach (MeshtaskSettings g in meshtaskSettings)
		{
			g.CalculateLine();
			g.CalculateInverseLine();
			g.ClaculateV();
		}
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
		sfsc.AddRange(allSurfaceSettings);

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
			y = Mathf.Abs(y - (y - p.vertex_1.point.y));

			if (p.materialIndex == materialIndex)
			{
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
public class NoiseChannel
{
	public int channel;
	public List<Noise> noises = new List<Noise>();
	public NoiseGenerator generatorInstance
	{
		get
		{
			if (GeneratorInstance == null) { return CreateNoiseGenerator(channel); }
			else { return GeneratorInstance; }
		}
	}

	private NoiseGenerator GeneratorInstance;

	public NoiseGenerator CreateNoiseGenerator(int groupIndex)
	{
		if (GeneratorInstance == null)
			GeneratorInstance = new NoiseGenerator(noises, groupIndex);
		return GeneratorInstance;
	}
}
[System.Serializable]
public class Noise
{
	[SerializeField] private NoiseType noiseType;
	public NoiseDirection noiseDirection;

	[Range(0f, .9999f)]
	public float noiseLenght;
	[Range(0f, 20f)]
	public float noisePower;

	private enum NoiseType
	{
		None,
		Short_Noise,
		Medium_Noise,
		Long_Noise,
	}
}

public enum NoiseDirection
{
	None,
	Horizontal,
	Vertical,
}