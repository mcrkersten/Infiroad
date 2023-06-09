using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Experimental.Rendering.RayTracingAccelerationStructure;

public class MeshtaskExtruder
{
	// Used when generating the mesh
	List<Vector3> verts = new List<Vector3>();
	List<Vector3> normals = new List<Vector3>();
	List<Vector2> uvs0 = new List<Vector2>();
	List<List<int>> triIndices = new List<List<int>>();
	int materialCount;

	SegmentChain currentRoadchain;
	GameObject activeMeshGameObject;
	public void Extrude(MeshTask meshTask, SegmentChain parent)
    {
		currentRoadchain = parent;
		MeshtaskObject meshTaskObject = meshTask.meshtaskObject;
		MeshtaskSettings meshTaskSettings = meshTask.meshtaskObject.meshtaskSettings;

		List<Material> materials = new List<Material>();
		materials.AddRange(meshTaskSettings.materials);
		if (meshTaskSettings.variableMaterials.Count != 0)
			materials.Add(meshTaskSettings.variableMaterials[currentRoadchain.ChainIndex]);
		materialCount = materials.Count;

		activeMeshGameObject = CreateGameObject(meshTask.meshtaskObject, meshTask);
		Mesh mesh = CreateMeshFilters(materials, activeMeshGameObject);
		CleanMesh(mesh);

		ExecuteMeshtask(meshTaskObject, meshTask);

		//Build mesh
		AssignMesh(mesh);
		int vertexJumpCount = CalculateHardEdgeCount(meshTaskSettings);
		triIndices = CreateTriangles(meshTask, meshTaskSettings, vertexJumpCount);

		int materialIndex = 0;
		mesh.subMeshCount = triIndices.Count;
		foreach (List<int> tries in triIndices)
			mesh.SetTriangles(tries, materialIndex++);

		if(meshTask.meshTaskType != MeshTaskType.GrandStand)
			AssignMeshCollider(mesh, activeMeshGameObject);

		mesh.RecalculateNormals();
		mesh.RecalculateTangents();
	}

    private void AssignMeshCollider(Mesh mesh, GameObject gameObject)
    {
		MeshCollider c = gameObject.AddComponent<MeshCollider>();
		c.sharedMesh = mesh;
	}

	private Mesh CreateMeshFilters(List<Material> materials, GameObject gameObject)
	{
		MeshFilter mf = gameObject.AddComponent<MeshFilter>();
		MeshRenderer mr = gameObject.AddComponent<MeshRenderer>();
		mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.TwoSided;
		mr.materials = materials.ToArray();
		return mf.mesh;
	}

	private GameObject CreateGameObject(MeshtaskObject meshtaskObject, MeshTask meshTask)
	{
		GameObject gameObject = new GameObject();
		gameObject.name = meshtaskObject.meshtaskSettings.meshTaskType.ToString() + " " + meshtaskObject.meshtaskSettings.meshtaskPosition.ToString();
		gameObject.transform.parent = meshTask.segment.transform;
		return gameObject;
	}

	private void CleanMesh(Mesh mesh)
    {
		mesh.Clear();
		verts.Clear();
		normals.Clear();
		uvs0.Clear();
	}

	private void ExecuteMeshtask(MeshtaskObject meshtaskObject, MeshTask meshTask)
    {
		MeshtaskSettings meshtaskSettings = meshtaskObject.meshtaskSettings;
		int currentEdgeloop = 0;
		int objectCount = 0;
		float totalUVdistance = 0;
		if (meshtaskSettings.meshTaskType != meshTask.meshTaskType)
			return;

		Vector2 meshtaskOffset = new Vector2(meshtaskObject.position.x, meshtaskObject.position.y);
		
		bool isNegativeOffset = float.IsNegative(meshtaskOffset.x);
		Vector2 meshDirection =  isNegativeOffset ? (Vector2.left) : (Vector2.right);

		Vector3 noise = Vector3.zero;
		noise += meshTask.noiseChannel.generatorInstance.GetNoise(meshTask.startPointIndex + currentEdgeloop, meshTask.noiseChannel);
		foreach (MeshTask.Point point in meshTask.positionVectors)
		{
			float local_XOffset =  isNegativeOffset ? Mathf.Min(point.extrusionVariables.leftExtrusion, 0f) : Mathf.Max(0f, point.extrusionVariables.rightExtrusion);
			local_XOffset = local_XOffset * meshtaskSettings.extrusionSize;
			totalUVdistance += point.UVdistance;
			for (int i = 0; i < meshtaskSettings.PointCount; i++)
			{
				Vector2 basePoint = new Vector2(meshtaskObject.meshtaskSettings.points[i].vertex.point.x, meshtaskObject.meshtaskSettings.points[i].vertex.point.y);
				basePoint += meshtaskOffset;

				Vector3 vertex = new Vector3((basePoint.x + local_XOffset), basePoint.y, 0f);
				vertex = Quaternion.Euler(0, 0, (point.extrusionVariables.cornerCamber) * meshtaskSettings.maxChamfer) * vertex;

				Vector3 relativePosition = point.rotation * vertex;
				Vector3 position = relativePosition + (point.position) + noise;

				if (meshtaskSettings.points[i].isHardEdge)
                {
					verts.Add(position);
					if(i != 0 && meshtaskSettings.points[i].materialIndex != meshtaskSettings.points[i - 1].materialIndex)
						uvs0.Add(new Vector2(meshtaskSettings.points[i-1].uvLastDistance, totalUVdistance));
					else
						uvs0.Add(new Vector2(meshtaskSettings.points[i].uvDistance, totalUVdistance));
				}
				else if(i != 0 && meshtaskSettings.points[i].materialIndex != meshtaskSettings.points[i - 1].materialIndex)
                {
					verts.Add(position);
					uvs0.Add(new Vector2(meshtaskSettings.points[i-1].uvLastDistance, totalUVdistance));
				}

				verts.Add(position);
				uvs0.Add(new Vector2(meshtaskSettings.points[i].uvDistance, totalUVdistance));
			}

			objectCount += CreateStaticObjects(currentEdgeloop, objectCount, meshtaskObject, meshDirection, point, noise, local_XOffset, meshTask);
			currentEdgeloop++;
		}
	}

	private int CreateStaticObjects(int currentEdgeloop, int objectCount, MeshtaskObject meshtaskObject, Vector3 meshDirection, MeshTask.Point p, Vector3 noise, float local_XOffset, MeshTask meshTask)
    {
		MeshtaskSettings meshtaskSettings = meshtaskObject.meshtaskSettings;

		Vector2 mto_position = new Vector2(meshtaskObject.position.x, meshtaskObject.position.y);

		//Easy to spawn now to combat repetitiveness
		switch (meshtaskSettings.meshTaskType)
		{
			case MeshTaskType.Guardrail:
				if (currentEdgeloop - (objectCount * ((GuardrailSettings)meshtaskSettings).poleSpacing) == 0 || currentEdgeloop == meshTask.positionVectors.Count - 2)
				{
					GameObject instance = ObjectPooler.Instance.GetMeshtaskObject(meshtaskSettings.meshTaskType, MeshtaskPoolType.GuardrailPoles);
					meshtaskSettings.PlaceModelOnMesh(meshDirection, p, mto_position, noise, Mathf.Abs(local_XOffset), activeMeshGameObject, instance);
					return 1;
				}
				break;
			case MeshTaskType.CatchFence:
				if (currentEdgeloop - (objectCount * ((GuardrailSettings)meshtaskSettings).poleSpacing) == 0 || currentEdgeloop == meshTask.positionVectors.Count - 2)
				{
					GameObject instance = ObjectPooler.Instance.GetMeshtaskObject(meshtaskSettings.meshTaskType, MeshtaskPoolType.CatchfencePoles);
					meshtaskSettings.PlaceModelOnMesh(meshDirection, p, mto_position, noise, Mathf.Abs(local_XOffset), activeMeshGameObject, instance);
					return 1;
				}
				break;
			case MeshTaskType.GrandStand:
				if (true)
				{
					if(currentEdgeloop != meshTask.positionVectors.Count - 1)
						if(currentEdgeloop%2==0 && currentEdgeloop != meshTask.positionVectors.Count - 2  && currentEdgeloop != 0) 
							return 0;

					GameObject instance = ObjectPooler.Instance.GetMeshtaskObject(meshtaskSettings.meshTaskType, MeshtaskPoolType.GrandstandSides);
					meshtaskSettings.PlaceModelOnMesh(meshDirection, p, mto_position, noise, Mathf.Abs(local_XOffset), activeMeshGameObject, instance);
					return 1;
				}
				else if (currentEdgeloop == meshTask.positionVectors.Count - 2)
				{
					GameObject instance = ObjectPooler.Instance.GetMeshtaskObject(meshtaskSettings.meshTaskType, MeshtaskPoolType.GrandstandSides);
					meshtaskSettings.PlaceModelOnMesh(meshDirection, p, mto_position, noise, Mathf.Abs(local_XOffset), activeMeshGameObject, instance);
					return 1;
				}
				break;
			case MeshTaskType.TecproBarrier:
				if (currentEdgeloop == 0)
				{
					GameObject instance = ObjectPooler.Instance.GetMeshtaskObject(meshtaskSettings.meshTaskType, MeshtaskPoolType.Tecpros);
					meshtaskSettings.PlaceModelOnMesh(meshDirection, p, mto_position, noise, Mathf.Abs(local_XOffset), activeMeshGameObject, instance);
					return 1;
				}
				else if (currentEdgeloop == meshTask.positionVectors.Count - 2)
				{
					GameObject instance = ObjectPooler.Instance.GetMeshtaskObject(meshtaskSettings.meshTaskType, MeshtaskPoolType.Tecpros);
					meshtaskSettings.PlaceModelOnMesh(meshDirection, p, mto_position, noise, Mathf.Abs(local_XOffset), activeMeshGameObject, instance);
					return 1;
				}
				break;
			case MeshTaskType.VideoBillboard:
				if (currentEdgeloop == 0)
				{
					GameObject instance = ObjectPooler.Instance.GetMeshtaskObject(meshtaskSettings.meshTaskType, MeshtaskPoolType.VideoBillboardEdge);
					meshtaskSettings.PlaceModelOnMesh(meshDirection, p, mto_position, noise, Mathf.Abs(local_XOffset), activeMeshGameObject, instance);
					return 1;
				}
				else if (currentEdgeloop == meshTask.positionVectors.Count - 2)
				{
					GameObject instance = ObjectPooler.Instance.GetMeshtaskObject(meshtaskSettings.meshTaskType, MeshtaskPoolType.VideoBillboardEdge);
					meshtaskSettings.PlaceModelOnMesh(meshDirection, p, mto_position, noise, Mathf.Abs(local_XOffset), activeMeshGameObject, instance);
					return 1;
				}
				break;
			default:
				break;
		}
		return 0;
	}

	private List<List<int>> CreateTriangles(MeshTask meshtask, MeshtaskSettings meshtaskSettings, int vertexJumpedCount)
    {
		List<List<int>> tries = new List<List<int>>();
		for (int i = 0; i < materialCount; i++)
			tries.Add(new List<int>());

		// Generate Trianges
		// Foreach edge loop (except the last, since this looks ahead one step)
		for (int edgeLoop = 0; edgeLoop < meshtask.positionVectors.Count - 1; edgeLoop++)
		{
			//Debug.Log("New Edgeloop");

			int rootIndex = (meshtaskSettings.PointCount + vertexJumpedCount) * edgeLoop;
			int rootIndexNext = (meshtaskSettings.PointCount + vertexJumpedCount) * (edgeLoop + 1);
			// Foreach pair of line indices in the 2D shape
			for (int point = 0; point < meshtaskSettings.PointCount; point++)
			{
				if (!meshtaskSettings.meshIsClosed && point == (meshtaskSettings.PointCount -1)) continue; //Skip closing line

				int vertex1 = meshtaskSettings.points[point].line.x;
				int vertex2 = meshtaskSettings.points[point].line.y;

				//Bottom
				Vector2Int current = new Vector2Int();
				current[0] = vertex1 + rootIndex;
				current[1] = vertex2 + rootIndex;
				//Debug.Log(current.x +" "+current.y);

				//TOP
				Vector2Int next = new Vector2Int();
				next[0] = vertex1 + rootIndexNext;
				next[1] = vertex2 + rootIndexNext;

				int index = meshtaskSettings.points[point].materialIndex;
				tries[index].Add(current.x);
				tries[index].Add(next.x);
				tries[index].Add(next.y);

				tries[index].Add(current.x);
				tries[index].Add(next.y);
				tries[index].Add(current.y);
			}
		}
		return tries;
	}

	private int CalculateHardEdgeCount(MeshtaskSettings guardrailSettings)
    {
		int hardEdgeCount = 0;
		int lastMaterial = 0;
        for (int i = 0; i < guardrailSettings.points.Length; i++)
        {
			if (guardrailSettings.points[i].isHardEdge || guardrailSettings.points[i].materialIndex != lastMaterial)
				hardEdgeCount++;
			lastMaterial = guardrailSettings.points[i].materialIndex;
		}
		return hardEdgeCount;
	}

	private void AssignMesh(Mesh mesh)
    {
		// Assign it all to the mesh
		mesh.SetVertices(verts);
		mesh.SetUVs(0, uvs0);
		mesh.RecalculateNormals();
	}
}