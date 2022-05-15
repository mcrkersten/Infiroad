using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu, System.Serializable]
public class MeshtaskSettings : ScriptableObject
{
    public MeshTaskType meshTaskType;
	public MeshtaskPosition meshtaskPosition;
    public VertexPosition[] points;
    public bool meshtaskContinues;
    public int noiseChannel;

    public float maxChamfer;
    public float extrusionSize;
    public float meshtaskWidth;

	public float minimalCornerRadius;
	public float maximumCornerRadius;
    public int PointCount => points.Length;

    [Space]
    public Material material;

    public bool meshIsClosed;


	public float CalcUspan()
	{
		float dist = 0;
		for (int i = 0; i < points.Length - 1; i++)
		{
			Vector2 a = points[i].vertex.point;
			Vector2 b = points[i + 1].vertex.point;
			dist += (a - b).magnitude;
		}
		return dist;
	}

	public void CalculateLine()
	{
		int count1 = 0;
		foreach (VertexPosition item in points)
		{
			if (item.isHardEdge)
				count1++;
		}

		int count2 = 0;
		for (int i = 0; i < points.Length; i++)
		{
			if (i == 0)
				points[i].line = new Vector2Int(points.Length - 1 + count1, i);
			else
				points[i].line = new Vector2Int(i - 1 + count2, i + count2);

			if (points[i].isHardEdge)
				count2++;

		}
	}

	public void CalculateInverseLine()
	{
		int x = 0;
		for (int i = 0; i < points.Length; i++)
		{
			points[x].inversedLine = points[points.Length - 1 - i].line;
			x++;
		}
	}

	public void CalculateNormals()
	{
		for (int i = 0; i < points.Length; i++)
		{
			Vector2 nextPoint = Vector2.zero;
			Vector2 currentPoint = points[i].vertex.point;
			if (i == points.Length - 1)
				nextPoint = points[0].vertex.point;
			else
				nextPoint = points[i + 1].vertex.point;

			float dx = nextPoint.x - currentPoint.x;
			float dy = nextPoint.y - currentPoint.y;
			points[i].vertex.normal = new Vector2(-dy, dx);
		}
	}
}

public enum MeshtaskPosition
{
	Left = 0,
	Right,
	Both
}
