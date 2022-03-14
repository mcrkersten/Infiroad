using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu, System.Serializable]
public class GuardrailSettings : ScriptableObject
{


	[Header("Shape")]
    public VertexPosition[] points;
	public bool closedLoop;
	public int PointCount => points.Length;
	public Vector2 guardrailPosition;

	[Space]
	public Material material;

	[Range(0,10)]
	public int poleSpacing;
	public float sharpCornerRadius;


	[Header("GameObjects")]
	public GameObject guardrailPolePrefab;
	public GameObject sharpCornerGuardrailPolePrefab;

	public float guardRailWidth;

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
		int count1 = 0;
		foreach (VertexPosition item in points)
		{
			if (item.isHardEdge)
				count1++;
		}

		int count2 = 0;
		int x = 0;
		for (int i = points.Length; i > 0; i--)
		{
			if (i == 0)
				points[x].inversedLine = new Vector2Int( (points.Length - 1) + count1, i);
			else
				points[x].inversedLine = new Vector2Int( (i - 1 + count2) , i + count2);

			if (points[x].isHardEdge)
				count2++;
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


[System.Serializable]
public class VertexPosition
{
	public Vertex vertex;
	public Vector2Int line;
	public Vector2Int inversedLine;
	public bool isHardEdge;
}