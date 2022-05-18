using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu, System.Serializable]
public class MeshtaskSettings : ScriptableObject
{
	[HideInInspector] public int dataKey;
	public float uvLenght;
	public MeshTaskType meshTaskType;
	public MeshtaskPosition meshtaskPosition;
    public VertexPosition[] points;
	[Range(1f,10f)]
	public float meshResolution;
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
			if (i == 0)
				points[i].line = new Vector2Int(points.Length - 1 + count1, i);
			else
				points[i].line = new Vector2Int(i - 1 + count2, i + count2);

			if (points[i].isHardEdge)
				count2++;

		}
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

	public virtual void CalculateNormals()
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

	public virtual void CreateModelOnMesh(Vector2 direction, MeshTask.Point p, Vector3 noise, float localX_offset, GameObject parent, GameObject model)
	{
		Vector3 offset = new Vector3(direction.x * (this.meshtaskWidth + localX_offset), 0f, 0f) + noise;
		offset = Quaternion.Euler(0, 0, (p.extrusionVariables.averageExtrusion) * this.maxChamfer) * offset;

		Vector3 relativePosition = p.rotation * offset;
		Vector3 position = relativePosition + (p.position);

		model.transform.parent = parent.transform;
		model.transform.localPosition = position;
		model.transform.rotation = p.rotation;
		model.SetActive(true);

		if (offset.x > 0)
			model.transform.Rotate(model.transform.up, 180);

	}

	protected virtual void SpawnMeshtaskObject(MeshTask meshTask, GameObject currentMeshObject, int meshtaskPoint, MeshtaskPoolType meshtaskType)
	{
		MeshTask.Point p = meshTask.points[meshtaskPoint];

		Vector2 meshDirection = meshTask.meshPosition == MeshtaskPosition.Left ? (Vector2.left) : (Vector2.right);
		float local_XOffset = meshTask.meshPosition == MeshtaskPosition.Left ? Mathf.Min(0f, p.extrusionVariables.leftExtrusion) : Mathf.Max(0f, p.extrusionVariables.rightExtrusion);
		local_XOffset = local_XOffset * meshTask.meshtaskSettings.extrusionSize;

		Vector3 noise = Vector3.zero;
		noise += meshTask.noiseChannel.generatorInstance.getNoise(meshTask.startPointIndex + meshtaskPoint, meshTask.noiseChannel);

		GameObject instance = ObjectPooler.Instance.GetMeshtaskObject(meshTask.meshtaskSettings.meshTaskType, meshtaskType);
		CreateModelOnMesh(meshDirection, p, noise, Mathf.Abs(local_XOffset), currentMeshObject, instance);
	}

	public virtual void PopulateMeshtask(MeshTask meshTask, GameObject currentMeshObject)
    {

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

public enum MeshtaskPosition
{
	Left = 0,
	Right,
	Both
}

public enum MeshtaskPoolType
{
	GuardrailPoles = 0,
	CatchfencePoles,
	GrandstandSides,
	SmokeBombs,
	GrandstandArcs
}
