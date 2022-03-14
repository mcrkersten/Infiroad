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

	public void ExtrudeRoad(RoadChain roadChain, RoadSegment segment, Mesh mesh, RoadSettings roadSettings, OrientedCubicBezier3D bezier, UVMode uvMode, Vector2 nrmCoordStartEnd, float edgeLoopsPerMeter, float tilingAspectRatio) {

		// Clear all data. This could be optimized by using arrays and only reconstruct when lengths change
		ClearMesh(mesh);

		int hardEdgeCount = 0;
		List<Vector2> minMaxUvs = CalculateMinMaxUVs(roadSettings);
		//Vector2 minMaxUV = CalculateMinMaxUV(roadSettings);
		hardEdgeCount = roadSettings.CalculateLine();
		roadChainBuilder = RoadChainBuilder.instance;
		roadSettings.CalculateLine();

		// UVs/Texture fitting
		LengthTable table = null;
		if (uvMode == UVMode.TiledDeltaCompensated)
			table = new LengthTable(bezier, 12);
		float curveArcLength = bezier.GetArcLength();

		// Tiling
		float tiling = tilingAspectRatio;
		if (uvMode == UVMode.Tiled || uvMode == UVMode.TiledDeltaCompensated) {
			float uSpan = roadSettings.CalcUspan(); // World space units covered by the UVs on the U axis
			tiling *= curveArcLength / uSpan;
			tiling = Mathf.Max(1, Mathf.Round(tiling)); // Snap to nearest integer to tile correctly
		}

		// Generate vertices
		// Foreach edge loop
		// Calc edge loop count
		int targetCount = Mathf.RoundToInt(curveArcLength * edgeLoopsPerMeter);
		int edgeLoopCount = Mathf.Max(2, targetCount); // Make sure it's at least 2
		segment.edgeLoopCount = edgeLoopCount;
		segment.startEndLoop = new Vector2Int(RoadChainBuilder.instance.generatedRoadEdgeloops, edgeLoopCount);

		for (int ring = 0; ring < edgeLoopCount; ring++) {


			float t = ring / (edgeLoopCount - 1f);
			OrientedPoint op = bezier.GetOrientedPoint(t, roadSettings.rotationEasing);

			// Prepare UV coordinates. This branches lots based on type
			float tUv = t;
			if (uvMode == UVMode.TiledDeltaCompensated)
				tUv = table.TToPercentage(tUv);
			float y_UV = tUv * tiling;

			float curveRadiusInverse = 0;
			//Gets curve radius
			float curveRadius = bezier.GetRadius(t);
			float clampedRadius = curveRadius;

			if (curveRadius > 300f)
				clampedRadius = 0f;
			if (curveRadius < -300f)
				clampedRadius = 0f;

			float radiusT = Mathf.Min(Mathf.Abs(clampedRadius), 300f) / 300f; // 0f to 1.0f
			float animationCurveOutput = roadSettings.runoffAnimationCurve.Evaluate(radiusT);
			if (clampedRadius > 0)
				curveRadiusInverse = 10f * animationCurveOutput;
			if (clampedRadius < 0)
				curveRadiusInverse = -10f * animationCurveOutput;

			//stepsize
			if (ring != 0 || roadChainBuilder.generatedRoadEdgeloops == 0)
			{
				float step = Mathf.Clamp(curveRadiusInverse, -.25f, .25f);
				roadChainBuilder.radiusDelay = Mathf.Clamp(roadChainBuilder.radiusDelay + (step), -10f, 10f);
				if (clampedRadius == 0)
				{
					if (roadChainBuilder.radiusDelay > .2f)
						roadChainBuilder.radiusDelay -= .2f;
					else if (roadChainBuilder.radiusDelay < -.2f)
						roadChainBuilder.radiusDelay += .2f;
					else
						roadChainBuilder.radiusDelay = 0f;
				}
			}

			if (!roadSettings.guardrailIsContinues && Mathf.Abs(curveRadius) < roadSettings.guardRailMinimalCornerRadius)
			{
				Vector3 currentMeshPosition = segment.transform.TransformPoint(op.pos);
				float distance = Vector3.Distance(roadChainBuilder.lastMeshPosition, currentMeshPosition);
				if (distance > 2.5f)
				{
					if (roadChainBuilder.currentMeshTask != null)
						roadChainBuilder.meshtasks.Add(roadChainBuilder.currentMeshTask);
					roadChainBuilder.currentMeshTask = new MeshTask(curveRadius < 0, roadSettings, roadChainBuilder.generatedRoadEdgeloops, roadSettings.guardRailNoiseChannel);
				}

				if (distance > .01f) //Fish out duplicates
				{
					roadChainBuilder.lastMeshPosition = currentMeshPosition;
					roadChainBuilder.currentMeshTask.AddPoint(roadChainBuilder.lastMeshPosition, segment.transform.rotation * op.rot, Mathf.Abs(curveRadius));
				}
            }
            else if(roadSettings.guardrailIsContinues)
            {
				Vector3 currentMeshPosition = segment.transform.TransformPoint(op.pos);

				if(ring == 0)
					roadChainBuilder.currentMeshTask = new MeshTask(roadSettings, roadChainBuilder.generatedRoadEdgeloops, roadSettings.guardRailNoiseChannel);

				roadChainBuilder.currentMeshTask.AddPoint(currentMeshPosition, segment.transform.rotation * op.rot, Mathf.Abs(curveRadius));

				if (ring == edgeLoopCount - 1)
					roadChainBuilder.meshtasks.Add(roadChainBuilder.currentMeshTask);
			}

			// Foreach vertex in the 2D shape
			bool assetPointOpen = false;
			Vector3 openAssetPoint = Vector3.positiveInfinity;
			List<AssetSpawnPoint> assetTypes = new List<AssetSpawnPoint>();
			for (int i = 0; i < roadSettings.PointCount; i++) {

				float offsetCurve = 0f;
				if (roadSettings.points[i].scalesWithCorner)
					offsetCurve = CalculatePositiveOrNegativeCurve(roadSettings.points[i], roadChainBuilder.radiusDelay, roadSettings);

				Vector2 localPoint = new Vector2(roadSettings.points[i].vertex_1.point.x + offsetCurve, roadSettings.points[i].vertex_1.point.y);
				Vector2 noise = AddRandomNoiseValue(roadSettings.points[i].noiseChannel, roadSettings);
				//float noiseMagnitude = noise.magnitude;
				Vector3 globalPoint = op.LocalToWorldPos(localPoint + noise);

				//Asset points
				if (!assetPointOpen && roadSettings.points[i].assetSpawnPoint.Count > 0) {
					openAssetPoint = segment.transform.TransformPoint(globalPoint);
					assetPointOpen = true;
					assetTypes = roadSettings.points[i].assetSpawnPoint;
				}
				else if (assetPointOpen) {
					Vector3 closeGrassPoint = segment.transform.TransformPoint(globalPoint);

					segment.assetSpawnEdges.Add(new AssetSpawnEdge(openAssetPoint, closeGrassPoint, assetTypes));

					openAssetPoint = Vector3.positiveInfinity;
					assetPointOpen = false;
				}

				Vector2 minMaxUV = minMaxUvs[roadSettings.points[i].materialIndex];
				float x_UV = 0;
				//Left extrusion
				if (offsetCurve <= 0)
				{
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
						int index = roadSettings.points[i - 1].materialIndex;
						x_UV = Mathf.InverseLerp(minMaxUvs[index].x, minMaxUvs[index].y, localPoint.x - offsetCurve);
						uvs0.Add(new Vector2(x_UV, y_UV));
						verts.Add(globalPoint);
					}

					//MAIN MESH VERTEX
					x_UV = Mathf.InverseLerp(minMaxUV.x, minMaxUV.y, localPoint.x - offsetCurve);
					uvs0.Add(new Vector2(x_UV, y_UV));
					verts.Add(globalPoint);

					//If hard edge vertex 
					if (roadSettings.points[i].ishardEdge)
					{
						x_UV = Mathf.InverseLerp(minMaxUV.x, minMaxUV.y, localPoint.x - offsetCurve);
						uvs0.Add(new Vector2(x_UV, y_UV));
						verts.Add(globalPoint);
					}

					//Opening edge of UV extrusion
					else if (roadSettings.points[i].extrudePoint)
					{
						float normalize = Mathf.InverseLerp(minMaxUV.x, minMaxUV.y, localPoint.x - offsetCurve);
						x_UV = Mathf.InverseLerp(minMaxUV.x, minMaxUV.y, localPoint.x) - normalize;
						uvs0.Add(new Vector2(Mathf.Abs(x_UV), y_UV));
						verts.Add(globalPoint);
					}
				}

				//Right extrusion
				if (offsetCurve > 0)
				{
					//closing edge of UV extrusion
					if (i != 0 && roadSettings.points[i - 1].extrudePoint)
					{
						float normalize = Mathf.InverseLerp(minMaxUV.x, minMaxUV.y, localPoint.x - offsetCurve);
						x_UV = Mathf.InverseLerp(minMaxUV.x, minMaxUV.y, localPoint.x) - normalize;
						uvs0.Add(new Vector2(x_UV, y_UV));
						verts.Add(globalPoint);
					}

					else if (i != 0 && roadSettings.points[i].materialIndex != roadSettings.points[i - 1].materialIndex)
					{
						int index = roadSettings.points[i - 1].materialIndex;
						x_UV = Mathf.InverseLerp(minMaxUvs[index].x, minMaxUvs[index].y, localPoint.x - offsetCurve);
						uvs0.Add(new Vector2(x_UV, y_UV));
						verts.Add(globalPoint);
					}

					//MAIN MESH VERTEX
					x_UV = Mathf.InverseLerp(minMaxUV.x, minMaxUV.y, localPoint.x - offsetCurve);
					uvs0.Add(new Vector2(x_UV, y_UV));
					verts.Add(globalPoint);

					if (roadSettings.points[i].ishardEdge)
					{
						x_UV = Mathf.InverseLerp(minMaxUV.x, minMaxUV.y, localPoint.x - offsetCurve);
						uvs0.Add(new Vector2(x_UV, y_UV));
						verts.Add(globalPoint);
					}

					//opening edge of extrusion
					else if (roadSettings.points[i].extrudePoint)
					{
						x_UV = 0f;
						uvs0.Add(new Vector2(x_UV, y_UV));
						verts.Add(globalPoint);
					}
				}
			}

			if (ring != edgeLoopCount - 1)
				roadChainBuilder.generatedRoadEdgeloops++;
		}
		triIndices = CreateTriangles(hardEdgeCount, edgeLoopCount, roadSettings);


		// Assign it all to the mesh
		mesh.SetVertices(verts);
		mesh.SetUVs(0, uvs0);

		int materialIndex = 0;
		mesh.subMeshCount = triIndices.Count;
		foreach (List<int> tries in triIndices)
			mesh.SetTriangles(tries, materialIndex++);

		//mesh.SetTriangles( triIndices, 0 ); // <------------  SECOND ENTREE IS MATERIAL INDEX OF MESH
		mesh.RecalculateNormals();
		List<Material> m = new List<Material>();
		m.AddRange(roadSettings.materials);
		m.Add(roadSettings.extusionMaterial);
		segment.GetComponent<MeshRenderer>().materials = m.ToArray();
	}

	private Vector2 AddRandomNoiseValue(int noiseChannel, RoadSettings roadSettings)
	{
		//Random
		NoiseGenerator ng = roadSettings.generatorInstance;
		Vector3 noise = new Vector3();
		foreach (NoiseChannelSettings group in roadSettings.noiseChannels[noiseChannel].channelSettings)
		{
			noise += ng.getNoise(roadChainBuilder.generatedRoadEdgeloops, group);
		}
		return noise;
	}

    private Vector2 CalculateMinMaxUV(RoadSettings roadSettings)
    {
		float min_Xuv = float.PositiveInfinity;
		float max_Xuv = float.NegativeInfinity;

		foreach (var item in roadSettings.points)
		{
			if (item.vertex_1.point.x < min_Xuv)
				min_Xuv = item.vertex_1.point.x;

			if (item.vertex_1.point.x > max_Xuv)
				max_Xuv = item.vertex_1.point.x;
		}

		return new Vector2(min_Xuv, max_Xuv);
	}

	private List<Vector2> CalculateMinMaxUVs(RoadSettings roadSettings)
    {
		int current = 0;
		List<Vector2> uvs = new List<Vector2>();
        foreach (Material m in roadSettings.materials)
        {
			float min_Xuv = float.PositiveInfinity;
			float max_Xuv = float.NegativeInfinity;
			foreach (var item in roadSettings.points)
			{
				if(item.materialIndex == current)
                {
					if (item.vertex_1.point.x < min_Xuv)
						min_Xuv = item.vertex_1.point.x;

					if (item.vertex_1.point.x > max_Xuv)
						max_Xuv = item.vertex_1.point.x;
				}
			}
			uvs.Add(new Vector2(min_Xuv, max_Xuv));
			current++;
		}
		return uvs;
    }


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
		List<Material> materials = new List<Material>();
		materials.AddRange(roadSettings.materials);
		materials.Add(roadSettings.extusionMaterial);

		foreach (Material m in materials)
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

	private float CalculatePositiveOrNegativeCurve(VertexPoint vertexPoint, float curveStrenght, RoadSettings roadSettings)
	{
		if (vertexPoint.vertex_1.point.x > 0f) //Positive
			return Mathf.Max(0f, curveStrenght);
		if (vertexPoint.vertex_1.point.x < 0f) //Negative
			return Mathf.Min(0f, curveStrenght);
		return 0f;
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
