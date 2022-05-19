using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshtaskExtruder
{
	// Used when generating the mesh
	List<Vector3> verts = new List<Vector3>();
	List<Vector3> normals = new List<Vector3>();
	List<Vector2> uvs0 = new List<Vector2>();
	List<int>[] triIndices;

	RoadChain currentRoadchain;
	GameObject currentMeshObject;
	public void Extrude(MeshTask meshTask, RoadChain parent, MeshtaskSettings meshTasksettings)
    {
		currentRoadchain = parent;
		Mesh mesh = CreateMeshFilters(meshTasksettings, meshTask.meshPosition);
		ClearMesh(mesh);
		ExecuteMeshtask(meshTasksettings, meshTask);

		//Build mesh
		AssignMesh(mesh);
		int vertexJumpCount = CalculateHardEdgeCount(meshTasksettings);
		triIndices = CreateTriangles(meshTask, meshTasksettings, vertexJumpCount);
		int materialIndex = 0;
		mesh.subMeshCount = triIndices.Length;
		foreach (List<int> tries in triIndices)
			mesh.SetTriangles(tries, materialIndex++);

		if(meshTask.meshPosition == MeshtaskPosition.Left)
			mesh.RecalculateNormals();
		/////

		if(meshTask.meshTaskType != MeshTaskType.GrandStand)
			AssignMeshCollider(mesh);

		currentMeshObject.GetComponent<MeshRenderer>().materials = meshTasksettings.materials.ToArray();
		mesh.RecalculateNormals();
		mesh.RecalculateTangents();

	}

    private void AssignMeshCollider(Mesh mesh)
    {
		MeshCollider c = currentMeshObject.AddComponent<MeshCollider>();
		c.sharedMesh = mesh;
	}

	private Mesh CreateMeshFilters(MeshtaskSettings settings, MeshtaskPosition meshtaskPosition)
	{
		GameObject g = new GameObject();
		currentMeshObject = g;
		g.name = settings.meshtaskPosition.ToString() + " " + meshtaskPosition.ToString();
		currentMeshObject.transform.parent = currentRoadchain.transform;
		MeshFilter mf = currentMeshObject.AddComponent<MeshFilter>();
		MeshRenderer mr = currentMeshObject.AddComponent<MeshRenderer>();
		mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.TwoSided;
		return mf.mesh;
	}

	private void ClearMesh(Mesh mesh)
    {
		mesh.Clear();
		verts.Clear();
		normals.Clear();
		uvs0.Clear();
	}

	private void ExecuteMeshtask(MeshtaskSettings meshtaskSettings, MeshTask meshTask)
    {
		int currentEdgeloop = 0;
		int poleCount = 0;
		if (meshtaskSettings.meshTaskType != meshTask.meshTaskType)
			return;
		Vector2 meshDirection = meshTask.meshPosition == MeshtaskPosition.Left ? (Vector2.left) : (Vector2.right);

		foreach (MeshTask.Point p in meshTask.positionPoints)
		{
			float local_XOffset = meshTask.meshPosition == MeshtaskPosition.Left ? Mathf.Min(0f, p.extrusionVariables.leftExtrusion) : Mathf.Max(0f, p.extrusionVariables.rightExtrusion);
			local_XOffset = local_XOffset * meshtaskSettings.extrusionSize;

			Vector3 noise = Vector3.zero;
			noise += meshTask.noiseChannel.generatorInstance.getNoise(meshTask.startPointIndex + currentEdgeloop, meshTask.noiseChannel);


			for (int i = 0; i < meshtaskSettings.PointCount; i++)
			{
				Vector2 uv = new Vector2(currentEdgeloop/3f, Mathf.InverseLerp(0, meshtaskSettings.uvLenght, meshtaskSettings.points[i].vertex.horizontal_UV));

				Vector2 point = meshtaskSettings.points[i].vertex.point * (Vector2.left + Vector2.up);
				Vector3 vertex = new Vector3(meshDirection.x * (point.x + meshtaskSettings.meshtaskWidth + Mathf.Abs(local_XOffset)), point.y, 0f) + noise;
				vertex = Quaternion.Euler(0, 0, (p.extrusionVariables.averageExtrusion) * meshtaskSettings.maxChamfer) * vertex;
				Vector3 relativePosition = p.rotation * vertex;
				Vector3 position = relativePosition + (p.position);

				verts.Add(position);
				uvs0.Add(uv);

				if (meshtaskSettings.points[i].isHardEdge)
                {
					verts.Add(position);
					uvs0.Add(uv);
				}
				else if(i != 0 && meshtaskSettings.points[i].materialIndex != meshtaskSettings.points[i - 1].materialIndex)
                {
					verts.Add(position);
					uvs0.Add(uv);
				}
			}
			
			//Easy to spawn now to combat repetitiveness
			switch (meshtaskSettings.meshTaskType)
            {
                case MeshTaskType.Guardrail:
					if (currentEdgeloop - (poleCount * ((GuardrailSettings)meshtaskSettings).poleSpacing) == 0)
                    {
						GameObject instance = ObjectPooler.Instance.GetMeshtaskObject(meshtaskSettings.meshTaskType, MeshtaskPoolType.GuardrailPoles);
						meshtaskSettings.CreateModelOnMesh(meshDirection, p, noise, Mathf.Abs(local_XOffset), currentMeshObject, instance);
						poleCount++;
					}
					break;
                case MeshTaskType.CatchFence:
					if (currentEdgeloop - (poleCount * ((GuardrailSettings)meshtaskSettings).poleSpacing) == 0)
					{
						GameObject instance = ObjectPooler.Instance.GetMeshtaskObject(meshtaskSettings.meshTaskType, MeshtaskPoolType.CatchfencePoles);
						meshtaskSettings.CreateModelOnMesh(meshDirection, p, noise, Mathf.Abs(local_XOffset), currentMeshObject, instance);
						poleCount++;
					}
					break;
                case MeshTaskType.GrandStand:
					if(currentEdgeloop == 0)
                    {
						GameObject instance = ObjectPooler.Instance.GetMeshtaskObject(meshtaskSettings.meshTaskType, MeshtaskPoolType.GrandstandSides);
						meshtaskSettings.CreateModelOnMesh(meshDirection, p, noise, Mathf.Abs(local_XOffset), currentMeshObject, instance);
					}
					if(currentEdgeloop == meshTask.positionPoints.Count - 2)
                    {
						GameObject instance = ObjectPooler.Instance.GetMeshtaskObject(meshtaskSettings.meshTaskType, MeshtaskPoolType.GrandstandSides);
						meshtaskSettings.CreateModelOnMesh(meshDirection, p, noise, Mathf.Abs(local_XOffset), currentMeshObject, instance);
					}
					break;
            }
			currentEdgeloop++;
		}
		if(currentMeshObject != null)
			meshtaskSettings.PopulateMeshtask(meshTask, currentMeshObject);
	}


	private List<int>[] CreateTriangles(MeshTask meshTask, MeshtaskSettings meshtaskSettings, int vertexJumpedCount)
    {
		List<int>[] tries = new List<int>[meshtaskSettings.materials.Count];
        for (int i = 0; i < meshtaskSettings.materials.Count; i++)
			tries[i] = new List<int>();
		// Generate Trianges
		// Foreach edge loop (except the last, since this looks ahead one step)
		for (int edgeLoop = 0; edgeLoop < meshTask.positionPoints.Count - 2; edgeLoop++)
		{
			//Debug.Log("New Edgeloop");

			int rootIndex = (meshtaskSettings.PointCount + vertexJumpedCount) * edgeLoop;
			int rootIndexNext = (meshtaskSettings.PointCount + vertexJumpedCount) * (edgeLoop + 1);
			// Foreach pair of line indices in the 2D shape
			for (int point = 0; point < meshtaskSettings.PointCount; point++)
			{
				if (!meshtaskSettings.meshIsClosed && point == 0 && meshTask.meshPosition == MeshtaskPosition.Left) continue; //Skip closing line
				if (!meshtaskSettings.meshIsClosed && point == (meshtaskSettings.PointCount -1) && meshTask.meshPosition == MeshtaskPosition.Right) continue; //Skip closing line

				int vertex1 = meshTask.meshPosition == MeshtaskPosition.Left ? meshtaskSettings.points[point].line.x : meshtaskSettings.points[point].inversedLine.x;
				int vertex2 = meshTask.meshPosition == MeshtaskPosition.Left ? meshtaskSettings.points[point].line.y : meshtaskSettings.points[point].inversedLine.y;

				//Bottom
				Vector2Int current = new Vector2Int();
				current[0] = vertex1 + rootIndex;
				current[1] = vertex2 + rootIndex;
				//Debug.Log(current.x +" "+current.y);

				//TOP
				Vector2Int next = new Vector2Int();
				next[0] = vertex1 + rootIndexNext;
				next[1] = vertex2 + rootIndexNext;

				int material = meshtaskSettings.points[point].materialIndex;

				tries[material].Add(current.x);
				tries[material].Add(next.x);
				tries[material].Add(next.y);

				tries[material].Add(current.x);
				tries[material].Add(next.y);
				tries[material].Add(current.y);
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