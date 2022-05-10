//
// Dedicated to all my Patrons on Patreon,
// as a thanks for your continued support 💖
//
// Source code © Freya Holmér, 2019
// This code is provided exclusively to supporters,
// under the Attribution Assurance License
// "https://tldrlegal.com/license/attribution-assurance-license-(aal)"
// 
// You can basically do whatever you want with this code,
// as long as you include this license and credit me for it,
// in both the source code and any released binaries using this code
//
// Thank you so much again <3
//
// Freya
//

using System;
using System.Collections.Generic;
using UnityEngine;

// This class contains the heart of the spline extrusion code!
// You provide data and a mesh, and this will write to that mesh for you!
public class RoadMeshExtruder {

	// Used when generating the mesh
	List<Vector3> verts = new List<Vector3>();
	List<Vector3> normals = new List<Vector3>();
	List<Vector2> uvs0 = new List<Vector2>();
	List<List<int>> triIndices = new List<List<int>>();

	RoadChainBuilder roadChainBuilder;

	public void ExtrudeRoad(RoadSegment segment, Mesh mesh, RoadSettings roadSettings, OrientedCubicBezier3D bezier, UVMode uvMode, Vector2 nrmCoordStartEnd, float edgeLoopsPerMeter, float tilingAspectRatio) {

		ClearMesh(mesh);
		roadChainBuilder = RoadChainBuilder.instance;
		List<Vector2> calculatedUV = CalculateUV(roadSettings);;
		roadSettings.CalculateLine();

		// UVs/Texture fitting
		LengthTable table = uvMode == UVMode.TiledDeltaCompensated ? new LengthTable(bezier, 12) : null;

		float curveArcLength = bezier.GetArcLength(); // lenght of bezier
		float tiling = CalculateTiling(roadSettings.CalcUspan(), uvMode, tilingAspectRatio, curveArcLength);
		int edgeLoopCount = CalculateEdgeloopCount(segment, edgeLoopsPerMeter, curveArcLength);

		for (int ring = 0; ring < edgeLoopCount; ring++) {

			float time = ring / (edgeLoopCount - 1f);
			OrientedPoint op = bezier.GetOrientedPoint(time, roadSettings.rotationEasing);

			//Calculate the radius of the corner.
			CalculateCornerRadius(roadSettings,ring, time, bezier);
			//Calculate corner chamfer based on radius of the corner
			Quaternion chamferAngle = CalculateCornerChamfer(roadSettings, roadChainBuilder.radiusDelay.delay);
			//Create tasks to spawn guardrails
			CreateGuardrailMeshTask(roadSettings, ring, segment, op, edgeLoopCount);

			// Foreach vertex in the 2D shape
			bool assetPointOpen = false;
			Vector3 openAssetPoint = Vector3.positiveInfinity;
			List<AssetSpawnPoint> assetTypes = new List<AssetSpawnPoint>();

			for (int i = 0; i < roadSettings.PointCount; i++) {
				//Calculate corner extrusion
				float offsetCurve = 0f;
				if (roadSettings.points[i].scalesWithCorner)
					offsetCurve = roadSettings.points[i].vertex_1.point.x < 0f ? Mathf.Min(0f, roadChainBuilder.radiusDelay.leftDelay) : Mathf.Max(0f, roadChainBuilder.radiusDelay.rightDelay);

				//Create coordinates for vertex
				Vector2 noise = AddRandomNoiseValue(roadSettings.points[i].noiseChannel, roadSettings);
				Vector2 localPoint = new Vector2(roadSettings.points[i].vertex_1.point.x + offsetCurve, roadSettings.points[i].vertex_1.point.y);
				Vector3 globalPoint = op.LocalToWorldPos(chamferAngle * (localPoint + noise));

				if (assetPointOpen)
				{
					Vector3 closeGrassPoint = segment.transform.TransformPoint(globalPoint);
					segment.assetSpawnEdges.Add(new AssetSpawnEdge(openAssetPoint, closeGrassPoint, assetTypes));
					openAssetPoint = Vector3.positiveInfinity;
					assetPointOpen = false;
				}
				//Asset points
				if (!assetPointOpen && roadSettings.points[i].assetSpawnPoint.Count > 0) {
					openAssetPoint = segment.transform.TransformPoint(globalPoint);
					assetPointOpen = true;
					assetTypes = roadSettings.points[i].assetSpawnPoint;
				}

				// Prepare UV coordinates. This branches lots based on type
				Vector2 currentUV_MinMax = calculatedUV[roadSettings.points[i].materialIndex];
				Vector2 localUVPoint = roadSettings.points[i].vertex_1.point;

				float tUv = uvMode == UVMode.TiledDeltaCompensated ? table.TToPercentage(time) : time;
				float y_UV = tUv * tiling;
				float x_UV = 0;
				bool isMirrored = roadSettings.allSurfaceSettings[roadSettings.points[i].materialIndex].UV_mirrored;

				{//Place vertices
					float uY = roadSettings.points[i].vertex_1.uv.y;
					float uvPoint = isMirrored ? Mathf.Abs(localUVPoint.x - uY) : localUVPoint.x - uY;
					//closing edge of UV extrusion
					if (i != 0 && roadSettings.points[i - 1].extrudePoint)
					{
						x_UV = 0f;
						uvs0.Add(new Vector2(x_UV, y_UV));
						verts.Add(globalPoint);
					}
					//If different material
					else if (i != 0 && roadSettings.points[i].materialIndex != roadSettings.points[i - 1].materialIndex)
					{
						Vector2 prev_currentUV_MinMax = calculatedUV[roadSettings.points[i - 1].materialIndex];
						bool prev_isMirrored = roadSettings.allSurfaceSettings[roadSettings.points[i - 1].materialIndex].UV_mirrored;
						float prev_uvPoint = prev_isMirrored ? Mathf.Abs(localUVPoint.x + uY) : localUVPoint.x + uY;
						x_UV = Mathf.InverseLerp(prev_currentUV_MinMax.x, prev_currentUV_MinMax.y, prev_uvPoint);
						uvs0.Add(new Vector2(x_UV, y_UV));
						verts.Add(globalPoint);
					}
					else if (roadSettings.points[i].ishardEdge)
					{
						x_UV = Mathf.InverseLerp(currentUV_MinMax.x, currentUV_MinMax.y, uvPoint);
						uvs0.Add(new Vector2(x_UV, y_UV));
						verts.Add(globalPoint);
					}

					//MAIN MESH VERTEX
					x_UV = Mathf.InverseLerp(currentUV_MinMax.x, currentUV_MinMax.y, uvPoint);
					uvs0.Add(new Vector2(x_UV, y_UV));
					verts.Add(globalPoint);

					//Opening edge of UV extrusion
					if (roadSettings.points[i].extrudePoint)
					{
						float curve = roadSettings.points[i].vertex_1.point.x < 0f ? Mathf.Min(0f, roadChainBuilder.radiusDelay.leftDelay) : Mathf.Max(0f, roadChainBuilder.radiusDelay.rightDelay);
						x_UV = Mathf.Abs(curve) /10f;
						uvs0.Add(new Vector2(Mathf.Abs(x_UV), y_UV));
						verts.Add(globalPoint);
					}
				}
			}

			if (ring != edgeLoopCount - 1)
				roadChainBuilder.generatedRoadEdgeloops++;
		}
		triIndices = CreateTriangles(roadSettings.CalculateLine(), edgeLoopCount, roadSettings);


		// Assign it all to the mesh
		mesh.SetVertices(verts);
		mesh.SetUVs(0, uvs0);

		int materialIndex = 0;
		mesh.subMeshCount = triIndices.Count;
		foreach (List<int> tries in triIndices)
			mesh.SetTriangles(tries, materialIndex++);

		//mesh.SetTriangles( triIndices, 0 ); // <------------  SECOND ENTREE IS MATERIAL INDEX OF MESH
		mesh.RecalculateNormals();
		mesh.RecalculateTangents();
		List<SurfaceScriptable> m = new List<SurfaceScriptable>();
		m.AddRange(roadSettings.allSurfaceSettings);

		List<Material> mat = new List<Material>();
        foreach (SurfaceScriptable item in m)
			mat.Add(item.material);
		segment.GetComponent<MeshRenderer>().materials = mat.ToArray();
	}

	#region Mesh-shape calculations
	private int CalculateEdgeloopCount(RoadSegment segment, float edgeLoopsPerMeter, float curveArcLength)
    {
		int targetCount = Mathf.RoundToInt(curveArcLength * edgeLoopsPerMeter);
		int edgeLoopCount = Mathf.Max(2, targetCount); // Make sure it's at least 2
		segment.edgeLoopCount = edgeLoopCount;
		segment.startEndEdgeLoop = new Vector2Int(RoadChainBuilder.instance.generatedRoadEdgeloops, edgeLoopCount);
		return edgeLoopCount;
	}

    private void CalculateCornerRadius(RoadSettings roadSettings, int ring, float t, OrientedCubicBezier3D bezier)
	{
		//Gets bigger with larger radius
		float exp = 0;
		float extrusionSize = bezier.GetCornerRadius(t);
		if(Mathf.Abs(extrusionSize) < 300f)
        {
			float clampedRadius = Mathf.Clamp(extrusionSize, -300f, 300f);
			exp = roadSettings.runoffAnimationCurve.Evaluate((clampedRadius / 300f)) * 12f;
		}

		if (ring != 0)
			roadChainBuilder.radiusDelay.delay = exp;
	}

	private Quaternion CalculateCornerChamfer(RoadSettings roadSettings, float radius)
    {
		if (roadSettings.hasCornerChamfer)
			return Quaternion.Euler(0, 0, (radius / 10f) * roadSettings.maxChamfer);
		return Quaternion.identity;
	}

	private Vector2 AddRandomNoiseValue(int noiseChannel, RoadSettings roadSettings)
	{
		Vector3 noise = new Vector3();
		noise = roadSettings.noiseChannels[noiseChannel].generatorInstance.getNoise(roadChainBuilder.generatedRoadEdgeloops, roadSettings.noiseChannels[noiseChannel]);
		return noise;
	}
    #endregion

    private void CreateGuardrailMeshTask(RoadSettings roadSettings, int ring, RoadSegment segment, OrientedPoint op, int edgeLoopCount)
    {
		//MESHTASK
		if (!roadSettings.guardrailIsContinues && Mathf.Abs(roadChainBuilder.radiusDelay.delay) < roadSettings.guardRailMinimalCornerRadius)
		{
			Vector3 currentMeshPosition = segment.transform.TransformPoint(op.pos);
			float distance = Vector3.Distance(roadChainBuilder.lastMeshPosition, currentMeshPosition);
			if (distance > 2.5f)
			{
				if (roadChainBuilder.currentMeshTask != null)
					roadChainBuilder.meshtasks.Add(roadChainBuilder.currentMeshTask);
				roadChainBuilder.currentMeshTask = new MeshTask(roadChainBuilder.radiusDelay.delay < 0, roadSettings, roadChainBuilder.generatedRoadEdgeloops, roadSettings.noiseChannels[roadSettings.guardRailNoiseChannel]);
			}

			if (distance > .01f) //Fish out duplicates
			{
				roadChainBuilder.lastMeshPosition = currentMeshPosition;
				roadChainBuilder.currentMeshTask.AddPoint(roadChainBuilder.lastMeshPosition, segment.transform.rotation * op.rot, roadChainBuilder.radiusDelay.delay);
			}
		}
		else if (roadSettings.guardrailIsContinues)
		{
			Vector3 currentMeshPosition = segment.transform.TransformPoint(op.pos);

			if (ring == 0)
				roadChainBuilder.currentMeshTask = new MeshTask(roadSettings, roadChainBuilder.generatedRoadEdgeloops, roadSettings.noiseChannels[roadSettings.guardRailNoiseChannel]);

			roadChainBuilder.currentMeshTask.AddPoint(currentMeshPosition, segment.transform.rotation * op.rot, roadChainBuilder.radiusDelay.delay);

			if (ring == edgeLoopCount - 1)
				roadChainBuilder.meshtasks.Add(roadChainBuilder.currentMeshTask);
		}
	}

    #region UV calculations
    private List<Vector2> CalculateUV(RoadSettings roadSettings)
    {
		int current = 0;
		List<Vector2> uvs = new List<Vector2>();
		List<SurfaceScriptable> sfsc = new List<SurfaceScriptable>();
		sfsc.AddRange(roadSettings.allSurfaceSettings);

		foreach (SurfaceScriptable m in sfsc)
        {
			if (m.UV_mirrored)
			{
				uvs.Add(CalculateMirroredUV_Width(roadSettings.points, current));
			}
			else
			{
				uvs.Add(CalculateFullUV_Width(roadSettings.points, current));
			}
			current++;
		}
		return uvs;
    }

    private Vector2 CalculateFullUV_Width(VertexPoint[] points, int materialIndex)
    {
		float min_Xuv = float.PositiveInfinity;
		float max_Xuv = float.NegativeInfinity;
		int current = 0;
		foreach (VertexPoint p in points)
		{
			if (p.materialIndex == materialIndex)
			{
				if ((p.vertex_1.point.x) < min_Xuv)
					min_Xuv = p.vertex_1.point.x;
				if ((p.vertex_1.point.x) > max_Xuv)
					max_Xuv = p.vertex_1.point.x;
			}
			else if(current != 0 && points[current - 1].materialIndex == materialIndex)
            {
				if ((p.vertex_1.point.x) < min_Xuv)
					min_Xuv = p.vertex_1.point.x;
				if ((p.vertex_1.point.x) > max_Xuv)
					max_Xuv = p.vertex_1.point.x;
			}
			current++;
		}
		return new Vector2(min_Xuv, max_Xuv);
	}

    private Vector2 CalculateMirroredUV_Width(VertexPoint[] points, int materialIndex)
    {
		float min_Xuv = float.PositiveInfinity;
		float max_Xuv = float.NegativeInfinity;
		foreach (VertexPoint p in points)
		{
			if (p.materialIndex == materialIndex)
			{
				float abs = Mathf.Abs(p.vertex_1.point.x);
				if (abs < min_Xuv)
					min_Xuv = abs;
				if (abs > max_Xuv)
					max_Xuv = abs;
			}
		}
		return new Vector2(min_Xuv, max_Xuv);
	}

	private float CalculateTiling(float uSpan, UVMode uvMode, float tilingAspectRatio, float curveArcLength)
	{
		float tiling = tilingAspectRatio;
		if (uvMode == UVMode.Tiled || uvMode == UVMode.TiledDeltaCompensated)
		{
			tiling *= curveArcLength / uSpan;
			tiling = Mathf.Max(1, Mathf.Round(tiling)); // Snap to nearest integer to tile correctly
		}
		return tiling;
	}
	#endregion

	private void ClearMesh(Mesh mesh)
    {
		mesh.Clear();
		verts.Clear();
		normals.Clear();
		uvs0.Clear();
		triIndices.Clear();
	}

	private List<List<int>> CreateTriangles(int skippedVertex, int edgeLoopCount, RoadSettings roadSettings)
    {
		List<List<int>> tries = new List<List<int>>();
		List<SurfaceScriptable> surfaceScriptables = new List<SurfaceScriptable>();
		surfaceScriptables.AddRange(roadSettings.allSurfaceSettings);

		foreach (SurfaceScriptable surface in surfaceScriptables)
			tries.Add(new List<int>());

		// Generate Trianges
		// Foreach edge loop (except the last, since this looks ahead one step)
		for (int edgeLoop = 0; edgeLoop < edgeLoopCount - 1; edgeLoop++)
		{
			//Debug.Log("New Edgeloop");
			int rootIndex = (roadSettings.PointCount + skippedVertex) * edgeLoop;
			int rootIndexNext = (roadSettings.PointCount + skippedVertex) * (edgeLoop + 1);
			// Foreach pair of line indices in the 2D shape
			for (int i = 0; i < roadSettings.PointCount; i++)
			{
				int point_1 = roadSettings.points[i].line.x;
				int point_2 = roadSettings.points[i].line.y;

				Vector2Int current = new Vector2Int(point_1 + rootIndex, point_2 + rootIndex);
				Vector2Int next = new Vector2Int(point_1 + rootIndexNext, point_2 + rootIndexNext);

				int index = roadSettings.points[i].materialIndex;
				if (roadSettings.points[i].extrudePoint)
					index = tries.Count - 1;

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
}
[System.Serializable]
public class MeshTask
{
	public RoadSettings roadSettings;
	public List<Point> points = new List<Point>();
	public int startPointIndex;
	public NoiseChannel noiseChannel;

	public bool mirror;
	public bool bothSides;
	public float curveRadius;

	public MeshTask(bool onRightSide, RoadSettings settings, int startPointIndex, NoiseChannel noiseChannel)
	{
		this.roadSettings = settings;
		this.mirror = onRightSide;
		this.startPointIndex = startPointIndex;
		this.noiseChannel = noiseChannel;
	}

	public MeshTask(RoadSettings settings, int startPointIndex, NoiseChannel noiseChannel)
	{
		this.roadSettings = settings;
		this.startPointIndex = startPointIndex;
		this.noiseChannel = noiseChannel;
		bothSides = true;
	}

	public void AddPoint(Vector3 position, Quaternion rotation, float radius)
    {
		points.Add(new Point(position, rotation, radius));
    }

	[System.Serializable]
	public class Point
    {
		public float radius;
		public Vector3 position;
		public Quaternion rotation;
		public Point(Vector3 position, Quaternion rotation, float radius)
        {
			this.position = position;
			this.rotation = rotation;
			this.radius = radius;
		}
    }
}
