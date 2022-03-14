using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu, System.Serializable]
public class RoadSettings : ScriptableObject
{
	[Header("Appearance")]
    public VertexPoint[] points;
	public List<Material> materials;
	public Material extusionMaterial;
	public int PointCount => points.Length;

	[Header("Guardrail")]
	public bool hasGuardrail;
	public GuardrailSettings guardRail;
	public bool guardrailIsContinues;
	public float guardRailMinimalCornerRadius;

	[Space]
	public string roadTypeTag;
	public List<VegitationPool> assetPools = new List<VegitationPool>();

	public float heightNoise;
	public Ease rotationEasing;
	public float edgeLoopsPerMeter;

	public float debugRoadCurveStrenght;
	public AnimationCurve runoffAnimationCurve;

	[Header("Noise settings")]
	public List<NoiseChannel> noiseChannels = new List<NoiseChannel>();
	public NoiseChannel guardRailNoiseChannel;

	public RoadDecoration startDecoration;
	public RoadDecoration sectorDecoration;

	[Header("Short")]
	[Range(0f,1f)]
	public float shortNoise;
	[Range(0f, 1f)]
	public float shortPower;

	[Header("Medium")]
	[Range(0f, 1f)]
	public float mediumNoise;
	[Range(0f, 1f)]
	public float mediumPower;

	[Header("Long")]
	[Range(0f, 1f)]
	public float longNoise;
	[Range(0f, 20f)]
	public float longPower;

	public int seed;

	public NoiseGenerator generatorInstance 
	{ 
		get { 
			if (GeneratorInstance == null) { return CreateNoiseGenerator(); }
            else { return GeneratorInstance; }
		}
	}

	private NoiseGenerator GeneratorInstance;

	public NoiseGenerator CreateNoiseGenerator()
    {
		if(GeneratorInstance == null)
			GeneratorInstance = new NoiseGenerator(shortNoise, shortPower, mediumNoise, mediumPower, longNoise, longPower, seed);
		return GeneratorInstance;
    }

	/// <summary>
	/// Total U Lenght of object AKA horizontal
	/// </summary>
	/// <returns></returns>
	public float CalcUspan()
	{
		float dist = 0;
		for (int i = 0; i < PointCount - 1; i++)
		{
			Vector2 a = points[i].vertex_1.point;
			Vector2 b = points[i + 1].vertex_1.point;
			dist += (a - b).magnitude;
		}
		return Mathf.Abs(dist);
	}

	public int CalculateLine()
	{
		int skippedPoints = 0;
		for (int i = 0; i < PointCount; i++)
		{
			if (points[i].ishardEdge)
				skippedPoints++;
			if (i != 0 && points[i].materialIndex != points[i - 1].materialIndex)
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

	[ContextMenu("Calculate normals")]
	public void CalculateNormals()
	{
		for (int i = 0; i < PointCount; i++)
		{
			Vector2 nextPoint = Vector2.zero;
			Vector2 currentPoint = points[i].vertex_1.point;
			if (i == PointCount - 1)
				nextPoint = points[0].vertex_1.point;
			else
				nextPoint = points[i + 1].vertex_1.point;

			float dx = nextPoint.x - currentPoint.x;
			float dy = nextPoint.y - currentPoint.y;
			points[i].vertex_1.normal = new Vector2(-dy, dx);
		}
	}

	public float NoiseCreator(int pointIndex)
    {
		float n = Mathf.PerlinNoise(pointIndex, pointIndex);
		return n;
    }
}

[System.Serializable]
public class NoiseChannel
{
	public List<NoiseChannelSettings> channelSettings = new List<NoiseChannelSettings>();
}

[System.Serializable]
public class NoiseChannelSettings
{
	public int group;
	public NoiseType noiseType;
	public NoiseDirection noiseDirection;
}

public enum NoiseType
{
	none,
	short_Noise,
	medium_Noise,
	long_Noise,
	shortMedium_Noise,
	mediumLong_Noise,
	shortLong_Noise,
	allTypes
}

public enum NoiseDirection
{
	None,
	Horizontal,
	Vertical,
	Biderectional
}