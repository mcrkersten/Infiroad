using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuardrailExtruder
{
	// Used when generating the mesh
	List<Vector3> verts = new List<Vector3>();
	List<Vector3> normals = new List<Vector3>();
	List<Vector2> uvs0 = new List<Vector2>();
	List<Vector2> uvs1 = new List<Vector2>();
	List<int> triIndices = new List<int>();

	RoadChain currentRoadchain;
	GameObject currentGuardrail;
	public void Extrude(MeshTask meshTask, RoadChain parent, RoadSettings roadSettings)
    {
		currentRoadchain = parent;
		Mesh mesh = CreateMesh(roadSettings);

		ClearMesh(mesh);

		CreateGuardrail(roadSettings.guardRail, meshTask);
		int hardEdgeCount = CalculateHardEdgeCount(roadSettings.guardRail);
		CreateTriangles(meshTask, roadSettings.guardRail, hardEdgeCount);

		AssignMesh(mesh);

	}

	private Mesh CreateMesh(RoadSettings roadSettings)
    {
		GameObject g = new GameObject();
		currentGuardrail = g;
		currentGuardrail.transform.parent = currentRoadchain.transform;
		MeshFilter mf = currentGuardrail.AddComponent<MeshFilter>();
		MeshRenderer mr = currentGuardrail.AddComponent<MeshRenderer>();
		mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.TwoSided;
		mr.material = roadSettings.guardRail.material;
		Mesh mesh = mf.mesh;
		return mesh;
	}

	private void ClearMesh(Mesh mesh)
    {
		mesh.Clear();
		verts.Clear();
		normals.Clear();
		uvs0.Clear();
		triIndices.Clear();
	}

	private void CreateGuardrail(GuardrailSettings guardrailSettings, MeshTask meshTask)
    {
		int currentEdgeloop = 0;
		int poleCount = 0;

		float local_XOffset = 0f;//The offset on the X-axis when in curve
		Vector2 meshDirection = meshTask.mirror ? (Vector2.left) : (Vector2.right);
		foreach (MeshTask.Point p in meshTask.points)
		{
			local_XOffset = meshTask.mirror ? Mathf.Min(0f, p.radius) : Mathf.Max(0f, p.radius);
			Vector3 noise = Vector3.zero;
			foreach (NoiseChannelSettings channel in meshTask.noiseChannel.channelSettings)
            {
				noise += meshTask.roadSettings.generatorInstance.getNoise(meshTask.startPointIndex + currentEdgeloop, channel);
            }

			for (int i = 0; i < guardrailSettings.PointCount; i++)
			{
				Vector2 uv = new Vector2(currentEdgeloop, guardrailSettings.points[i].vertex.uv.y);

				Vector2 point = guardrailSettings.points[i].vertex.point * (Vector2.left + Vector2.up);

				Vector3 offset = new Vector3(meshDirection.x * (point.x + guardrailSettings.guardRailWidth + Mathf.Abs(local_XOffset)), point.y, 0f) + noise;

				Vector3 relativePosition = p.rotation * offset;
				Vector3 position = relativePosition + (p.position);
				verts.Add(position);
				uvs0.Add(uv);

				if (guardrailSettings.points[i].isHardEdge)
				{
					verts.Add(position); //World position of point
				}
			}

			if (currentEdgeloop - (poleCount * guardrailSettings.poleSpacing) == 0)
			{
				CreateGuardrailPole(meshDirection, guardrailSettings, p, noise, Mathf.Abs(local_XOffset));
				poleCount++;
			}
			currentEdgeloop++;
		}
	}


	private void CreateTriangles(MeshTask meshTask, GuardrailSettings guardrailSettings, int hardEdgeCount)
    {
		// Generate Trianges
		// Foreach edge loop (except the last, since this looks ahead one step)
		for (int edgeLoop = 0; edgeLoop < meshTask.points.Count - 2; edgeLoop++)
		{
			//Debug.Log("New Edgeloop");

			int rootIndex = (guardrailSettings.PointCount + hardEdgeCount) * edgeLoop;
			int rootIndexNext = (guardrailSettings.PointCount + hardEdgeCount) * (edgeLoop + 1);
			// Foreach pair of line indices in the 2D shape
			for (int line = 0; line < guardrailSettings.PointCount; line++)
			{
				if (!guardrailSettings.closedLoop && line == 0) continue; //Skip closing line

				int vertex1 = meshTask.mirror ? guardrailSettings.points[line].line.x : guardrailSettings.points[line].inversedLine.x;


				int vertex2 = meshTask.mirror ? guardrailSettings.points[line].line.y : guardrailSettings.points[line].inversedLine.y;
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
		if (meshTask.mirror)
			triIndices.Reverse();
	}

	private void CreateGuardrailPole(Vector2 direction, GuardrailSettings guardrailSettings, MeshTask.Point p, Vector3 noise, float X_offset)
	{
		Vector3 offset = new Vector3(direction.x * (guardrailSettings.guardrailPosition.x + guardrailSettings.guardRailWidth + X_offset), guardrailSettings.guardrailPosition.y, 0f) + noise;
		Vector3 relativePosition = p.rotation * offset;
		Vector3 position = relativePosition + (p.position);

		GameObject pole = null;
		if (Mathf.Abs(guardrailSettings.sharpCornerRadius) < X_offset)
			pole = GameObject.Instantiate(guardrailSettings.sharpCornerGuardrailPolePrefab, position, p.rotation, currentGuardrail.transform);
		else
			pole = GameObject.Instantiate(guardrailSettings.guardrailPolePrefab, position, p.rotation, currentGuardrail.transform);

		if (offset.x < 0)
			pole.transform.localScale = new Vector3(1, 1, 1);
		else
			pole.transform.localScale = new Vector3(-1, 1, 1);

	}


	private int CalculateHardEdgeCount(GuardrailSettings guardrailSettings)
    {
		int hardEdgeCount = 0;
		foreach (var item in guardrailSettings.points)
		{
			if (item.isHardEdge)
				hardEdgeCount++;
		}
		return hardEdgeCount;
	}

	private float CalculatePositiveOrNegativeCurve(float xValue, float curveStrenght, RoadSettings roadSettings)
	{
		if (xValue > 0f) //Positive
			return Mathf.Max(0f, curveStrenght);
		if (xValue < 0f) //Negative
			return Mathf.Min(0f, curveStrenght);
		return 0f;
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