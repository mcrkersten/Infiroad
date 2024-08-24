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

	private Vector2 CalculateFullMaterialWidth(VertexPosition[] points, int materialIndex)
	{
		float min_Xuv = float.PositiveInfinity;
		float max_Xuv = float.NegativeInfinity;
		int current = 0;
		foreach (VertexPosition p in points)
		{
			if (p.materialIndex == materialIndex)
			{
				if ((p.vertex.point.x) < min_Xuv)
					min_Xuv = p.vertex.point.x;
				if ((p.vertex.point.x) > max_Xuv)
					max_Xuv = p.vertex.point.x;
			}
			else if (current != 0 && points[current - 1].materialIndex == materialIndex)
			{
				if ((p.vertex.point.x) < min_Xuv)
					min_Xuv = p.vertex.point.x;
				if ((p.vertex.point.x) > max_Xuv)
					max_Xuv = p.vertex.point.x;
			}
			current++;
		}
		return new Vector2(min_Xuv, max_Xuv);
	}

	public void CalculateUV_Distance()
	{
		int extra = variableMaterials.Count != 0 ? 1 : 0;
		float[] distances = new float[materials.Count + extra];
		for (int i = 0; i < distances.Length; i++)
			distances[i] = 0;

		VertexPosition[] lastPositions = new VertexPosition[materials.Count + extra];

		for (int i = 0; i < points.Length; i++)
		{
			if(i == 0)
				distances[points[i].materialIndex] = 0;
			else if(points[i].materialIndex == points[i - 1].materialIndex)
			{
				float distance = (Vector2.Distance(points[i].vertex.point, points[i - 1].vertex.point)/10f);
				distances[points[i].materialIndex] += distance;
				lastPositions[points[i].materialIndex] = points[i];
			}
			else
			{
				float distance = (Vector2.Distance(points[i].vertex.point, points[i - 1].vertex.point)/10f);
				points[i - 1].uvLastDistance = distances[points[i - 1].materialIndex] + distance;
				distances[points[i].materialIndex] += 0;
			}
			points[i].uvDistance = distances[points[i].materialIndex];
		}
	}

	public virtual void CalculateLine()
	{
		int count1 = 0;
		for (int i = 0; i < points.Length; i++)
		{
			if (points[i].isHardEdge)
				count1++;
			else if(i != 0 && i != 0 && points[i].materialIndex != points[i - 1].materialIndex)
				count1++;
		}

		int count2 = 0;
		for (int i = 0; i < points.Length; i++)
		{
			if (points[i].isHardEdge)
				count2++;
			else if(i != 0 && i != 0 && points[i].materialIndex != points[i - 1].materialIndex)
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
		MeshTask.Point p = meshTask.positionVectors[meshtaskPoint];

        Vector2 meshDirection = float.IsNegative(mto_position.x) ? (Vector2.left) : (Vector2.right);
        float local_XOffset = float.IsNegative(mto_position.x) ? Mathf.Min(0f, p.extrusionVariables.leftExtrusion) : Mathf.Max(0f, p.extrusionVariables.rightExtrusion);
        local_XOffset = local_XOffset * meshTask.meshtaskObject.meshtaskSettings.extrusionSize;

        Vector3 noise = Vector3.zero;
        noise += meshTask.noiseChannel.generatorInstance.GetNoise(meshTask.startPointIndex + meshtaskPoint, meshTask.noiseChannel);

        GameObject instance = ObjectPooler.Instance.GetMeshtaskObject(meshTask.meshtaskObject.meshtaskSettings.meshTaskType, meshtaskPoolType);
        PlaceModelOnMesh(meshDirection, p, mto_position, noise, Mathf.Abs(local_XOffset), parent, instance);
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
	[HideInInspector] public Vector2Int line;
	[HideInInspector] public float uvDistance;
	public float uvLastDistance;
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
