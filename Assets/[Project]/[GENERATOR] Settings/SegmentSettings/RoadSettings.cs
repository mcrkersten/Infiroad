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

	[Header("Guardrail")]
	public bool hasGuardrail;
	public GuardrailSettings guardRail;
	public bool guardrailIsContinues;
	public float guardRailMinimalCornerRadius;

	[Space]
	public string roadTypeTag;
	public List<VegitationPool> assetPools = new List<VegitationPool>();

	public Ease rotationEasing = Ease.InOut;
	public float edgeLoopsPerMeter;

	public float debugRoadCurveStrenght;
	public AnimationCurve runoffAnimationCurve;

	[Header("Noise settings")]
	public List<NoiseChannel> noiseChannels = new List<NoiseChannel>();
	public int guardRailNoiseChannel;

	public void UpdateAllRoadSurfaces()
    {
		allSurfaceSettings = new List<SurfaceScriptable>();
		allSurfaceSettings.AddRange(surfaceSettings);
		allSurfaceSettings.Add(runoffMaterial);
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
	[Range(0f, 1f)]
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