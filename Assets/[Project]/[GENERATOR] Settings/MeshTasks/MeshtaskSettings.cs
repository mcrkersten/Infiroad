using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using System;
[CreateAssetMenu, System.Serializable]
public class MeshtaskSettings : ScriptableObject
{
	public  Vector3 EDITOR_offSetPosition = Vector2.zero;

	[HideInInspector] public int dataKey;
	[HideInInspector] public List<Vector2> calculatedUs = new List<Vector2>();
	[HideInInspector] public float uvLenght;
	public float UV_scale = 1f;

	public MeshTaskType meshTaskType;
	public MeshtaskPosition meshtaskPosition;
	public MeshtaskStyle meshtaskStyle;

	[Header("Mesh settings")]
	public TextAsset PointsFile;
	public Vector2 fileOffset;
	public bool fileReverse;
	public VertexPosition[] points;
	[Range(1,10)]
	public int meshResolution;
    public bool meshtaskContinues;
    public int noiseChannel;

	[HideInInspector] public float maxChamfer;
	[HideInInspector] public float extrusionSize;

	[Header("Corner radius style")]
	public float minimalCornerRadius;
	public float maximumCornerRadius;

	[Header("Extrusion size style")]
	[Range(0f,1f)]
	public float minimalExtrusionSize;
	public int PointCount => points.Length;

    [Space]
    public List<Material> materials = new List<Material>();
	public List<Material> variableMaterials = new List<Material>();


	public bool meshIsClosed;

	public List<Decoration> meshtaskPoolingObjects = new List<Decoration>();

	public virtual void ClaculateV()
	{
		Vector2 lastUV = Vector2.zero;
		float total = 0f;
		for (int i = 0; i < points.Length; i++)
		{
			if (i == 0)
            {
				lastUV = points[0].vertex.point;
				points[0].vertex.horizontal_UV = total;
            }
            else
            {
				total += Vector2.Distance(lastUV, points[i].vertex.point);
				points[i].vertex.horizontal_UV = total;
				lastUV = points[i].vertex.point;
			}
		}
		uvLenght = total;
	}

	public virtual void CalculateLine()
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
			if (points[i].isHardEdge)
				count2++;
			if (i == PointCount - 1)
				points[i].line = new Vector2Int(points.Length - 1 + count1, 0);
			else
				points[i].line = new Vector2Int(i + count2, i + 1 + count2);
		}
	}

	public void SetListOfPoints(List<Vector2> points)
	{
		VertexPosition[] vPositions = new VertexPosition[points.Count];
		vPositions.Count();

		if (!fileReverse)
		{
			int x = 0;
			for (int i = 0; i < vPositions.Length; i++)
			{
				vPositions[i] = new VertexPosition();
				if(i == 0 || i == vPositions.Length - 1)
					vPositions[i].isHardEdge = false;
				else
					vPositions[i].isHardEdge = true;

				vPositions[i].vertex = new Vertex();
				vPositions[i].vertex.point = points[x++] + fileOffset;
			}
		}
		else
		{
			int x = 0;
			for (int i = vPositions.Length - 1; i >= 0; i--)
			{
				vPositions[i] = new VertexPosition();
				if(i == 0 || i == vPositions.Length - 1)
					vPositions[i].isHardEdge = false;
				else
					vPositions[i].isHardEdge = true;

				vPositions[i].vertex = new Vertex();
				vPositions[i].vertex.point = points[x++] + fileOffset;
			}
		}
			
		this.points = vPositions;
	}

	public virtual void CalculateInverseLine()
	{
		int x = 0;
		for (int i = 0; i < points.Length; i++)
		{
			points[x].inversedLine = points[points.Length - 1 - i].line;
			x++;
		}
	}

	public virtual void PlaceModelOnMesh(Vector2 direction, MeshTask.Point p, Vector3 meshtaskObjectPosition, Vector3 noise, float localX_offset, GameObject parent, GameObject model)
	{
		Vector3 offset = new Vector3(direction.x * localX_offset, 0f, 0f) + noise + meshtaskObjectPosition;
		offset = Quaternion.Euler(0, 0, (p.extrusionVariables.cornerCamber) * this.maxChamfer) * offset;

		Vector3 relativePosition = p.rotation * offset;
		Vector3 calculatedPosition = relativePosition + p.position;

		model.transform.localPosition = calculatedPosition + parent.transform.position;
		model.transform.rotation = p.rotation;
		model.SetActive(true);

		if (offset.x > 0)
			model.transform.Rotate(model.transform.up, 180);

	}

	protected virtual void SpawnMeshtaskObject(MeshTask meshTask, GameObject parent, int meshtaskPoint, MeshtaskPoolType meshtaskPoolType, Vector2 mto_position)
	{

	}

	public virtual void PopulateMeshtask(MeshTask meshTask, GameObject currentMeshObject, bool relfect = false)
    {

    }
}
[System.Serializable]
public class MeshtaskObject
{
	public MeshtaskSettings meshtaskSettings;
	public Vector3 position;
}

[System.Serializable]
public class VertexPosition
{
	public Vertex vertex;
	public Vector2Int line;
	public Vector2Int inversedLine;
	public bool isHardEdge;
	public int materialIndex;
}

public enum MeshtaskPoolType
{
	GuardrailPoles = 0,
	CatchfencePoles,
	GrandstandSides,
	SmokeBombs,
	GrandstandArcs,
	Tecpros,
	VideoBillboardEdge,
	Building_Small,
	Building_Medium,
	Building_Large
}

public enum MeshtaskPosition
{
	Left,
	Right
}
