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
	List<int> triIndices = new List<int>();

	RoadChain currentRoadchain;
	GameObject currentMeshObject;
	public void Extrude(MeshTask meshTask, RoadChain parent, MeshtaskSettings meshTasksettings)
    {
		currentRoadchain = parent;
		Mesh mesh = CreateMeshFilters(meshTasksettings, meshTask.meshPosition);
		ClearMesh(mesh);
		ExecuteMeshtask(meshTasksettings, meshTask);

		int hardEdgeCount = CalculateHardEdgeCount(meshTasksettings);
		CreateTriangles(meshTask, meshTasksettings, hardEdgeCount);

		AssignMesh(mesh);

		if(meshTask.meshTaskType != MeshTaskType.GrandStand)
			AssignMeshCollider(mesh);

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
		currentMeshObject.AddComponent<ObjectPoolSaver>();
		mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.TwoSided;
		mr.material = settings.material;
		return mf.mesh;
	}

	private void ClearMesh(Mesh mesh)
    {
		mesh.Clear();
		verts.Clear();
		normals.Clear();
		uvs0.Clear();
		triIndices.Clear();
	}

	private void ExecuteMeshtask(MeshtaskSettings meshtaskSettings, MeshTask meshTask)
    {
		int currentEdgeloop = 0;
		int poleCount = 0;
		if (meshtaskSettings.meshTaskType != meshTask.meshTaskType)
			return;
		Vector2 meshDirection = meshTask.meshPosition == MeshtaskPosition.Left ? (Vector2.left) : (Vector2.right);

		foreach (MeshTask.Point p in meshTask.points)
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
					if(currentEdgeloop == meshTask.points.Count - 2)
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


	private void CreateTriangles(MeshTask meshTask, MeshtaskSettings meshtaskSettings, int hardEdgeCount)
    {
		// Generate Trianges
		// Foreach edge loop (except the last, since this looks ahead one step)
		for (int edgeLoop = 0; edgeLoop < meshTask.points.Count - 2; edgeLoop++)
		{
			//Debug.Log("New Edgeloop");

			int rootIndex = (meshtaskSettings.PointCount + hardEdgeCount) * edgeLoop;
			int rootIndexNext = (meshtaskSettings.PointCount + hardEdgeCount) * (edgeLoop + 1);
			// Foreach pair of line indices in the 2D shape
			for (int line = 0; line < meshtaskSettings.PointCount; line++)
			{
				if (!meshtaskSettings.meshIsClosed && line == 0 && meshTask.meshPosition == MeshtaskPosition.Left) continue; //Skip closing line
				if (!meshtaskSettings.meshIsClosed && line == (meshtaskSettings.PointCount -1) && meshTask.meshPosition == MeshtaskPosition.Right) continue; //Skip closing line

				int vertex1 = meshTask.meshPosition == MeshtaskPosition.Left ? meshtaskSettings.points[line].line.x : meshtaskSettings.points[line].inversedLine.x;
				int vertex2 = meshTask.meshPosition == MeshtaskPosition.Left ? meshtaskSettings.points[line].line.y : meshtaskSettings.points[line].inversedLine.y;
				//Bottom
				Vector2Int current = new Vector2Int();
				current[0] = vertex1 + rootIndex;
				current[1] = vertex2 + rootIndex;
				//Debug.Log(current.x +" "+current.y);

				//TOP
				Vector2Int next = new Vector2Int();
				next[0] = vertex1 + rootIndexNext;
				next[1] = vertex2 + rootIndexNext;

				triIndices.Add(current.x);
				triIndices.Add(next.x);
				triIndices.Add(next.y);

				triIndices.Add(current.x);
				triIndices.Add(next.y);
				triIndices.Add(current.y);
			}
		}
		if (meshTask.meshPosition == MeshtaskPosition.Left)
			triIndices.Reverse();
	}

	private int CalculateHardEdgeCount(MeshtaskSettings guardrailSettings)
    {
		int hardEdgeCount = 0;
		foreach (var item in guardrailSettings.points)
		{
			if (item.isHardEdge)
				hardEdgeCount++;
		}
		return hardEdgeCount;
	}

	private void AssignMesh(Mesh mesh)
    {
		// Assign it all to the mesh
		mesh.SetVertices(verts);
		mesh.SetUVs(0, uvs0);
		mesh.SetTriangles(triIndices, 0);
		mesh.RecalculateNormals();
	}
}