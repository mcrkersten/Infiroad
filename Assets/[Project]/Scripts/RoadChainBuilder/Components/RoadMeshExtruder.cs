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
using System.Net.NetworkInformation;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.Rendering.HableCurve;

// This class contains the heart of the spline extrusion code!
// You provide data and a mesh, and this will write to that mesh for you!
public class RoadMeshExtruder {

	// Used when generating the mesh
	List<Vector3> verts = new List<Vector3>();
	List<Vector3> normals = new List<Vector3>();
	List<Vector2> uvs = new List<Vector2>();
	List<List<int>> triIndices = new List<List<int>>();

	RoadChainBuilder roadChainBuilder;

	public void ExtrudeRoad(
		RoadSegment segment, 
		Mesh mesh, 
		RoadSettings roadSettings, 
		OrientedCubicBezier3D bezier, 
		UVMode uvMode, 
		Vector2 nrmCoordStartEnd, 
		float edgeLoopsPerMeter, 
		float tilingAspectRatio, 
		int surfaceIndex = 0) 
	{

		ClearMesh(mesh);
		roadChainBuilder = RoadChainBuilder.instance;

		// UVs/Texture fitting
		LengthTable table = uvMode == UVMode.TiledDeltaCompensated ? new LengthTable(bezier, 12) : null;

		float curveArcLength = bezier.GetArcLength(); // lenght of bezier
		float tiling = CalculateTiling(roadSettings.uSpan, uvMode, tilingAspectRatio, curveArcLength);
		int edgeLoopCount = CalculateEdgeloopCount(segment, edgeLoopsPerMeter, curveArcLength);

		OrientedPoint op = new OrientedPoint();
		for (int ring = 0; ring < edgeLoopCount; ring++) {

			float time = ring / (edgeLoopCount - 1f);
			op = bezier.GetOrientedPoint(time, roadSettings.rotationEasing);

			//Calculate the radius of the corner.
			CalculateRoadFormVariables(roadSettings,ring, time, bezier);
			//Calculate corner chamfer based on radius of the corner
			Quaternion chamferAngle = CalculateCornerChamfer(roadSettings);


			CreateMeshTasksOnEdgeloop(roadSettings, ring, segment, op, edgeLoopCount);


			// Foreach vertex in the 2D shape
			bool assetPointOpen = false;
			Vector3 openAssetPoint = Vector3.positiveInfinity;
			List<AssetSpawnPoint> assetTypes = new List<AssetSpawnPoint>();

			for (int i = 0; i < roadSettings.PointCount; i++) {
				//Calculate corner extrusion
				float offsetCurve = 0f;
				if (roadSettings.points[i].scalesWithCorner)
					offsetCurve = (roadSettings.points[i].vertex_1.point.x < 0f ? Mathf.Min(0f, roadChainBuilder.roadFormVariables.leftExtrusion) : Mathf.Max(0f, roadChainBuilder.roadFormVariables.rightExtrusion)) * roadSettings.extrusionSize;

                //Create noise coordinates for vertex
                Vector2Int noiseCoordinate = new Vector2Int(i, roadChainBuilder.generatedRoadEdgeloops);
                Vector2 noise = GetCoordinateNoise(roadSettings.points[i].noiseChannel, roadSettings, noiseCoordinate);

				//Create positional coordinates for vertex
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
				Vector2 currentUV_MinMax = roadSettings.calculatedUs[roadSettings.points[i].materialIndex];
				Vector2 localUVPoint = roadSettings.points[i].vertex_1.point;

				float tUv = uvMode == UVMode.TiledDeltaCompensated ? table.TToPercentage(time) : time;
				float y_UV = tUv * tiling;
				float x_UV = 0;
				List<SurfaceScriptable> sf = roadSettings.GetAllSurfaceSettings(0);
				bool isMirrored = sf[roadSettings.points[i].materialIndex].UV_mirrored;

				{//Place vertices
					float uY = roadSettings.points[i].vertex_1.point.y;
					float uvPoint = isMirrored ? Mathf.Abs(localUVPoint.x + uY) : localUVPoint.x - uY;
					//closing edge of UV extrusion
					if (i != 0 && roadSettings.points[i - 1].extrudePoint)
					{
						x_UV = 0f;
						uvs.Add(new Vector2(x_UV, y_UV));
						verts.Add(globalPoint);
					}
					//If different material
					else if (i != 0 && roadSettings.points[i].materialIndex != roadSettings.points[i - 1].materialIndex)
					{
						Vector2 prev_currentUV_MinMax = roadSettings.calculatedUs[roadSettings.points[i - 1].materialIndex];
						bool prev_isMirrored = sf[roadSettings.points[i - 1].materialIndex].UV_mirrored;
						float prev_uvPoint = prev_isMirrored ? Mathf.Abs(localUVPoint.x + uY) : localUVPoint.x + uY;
						x_UV = Mathf.InverseLerp(prev_currentUV_MinMax.x, prev_currentUV_MinMax.y, prev_uvPoint);
						uvs.Add(new Vector2(x_UV, y_UV));
						verts.Add(globalPoint);
					}
					else if (roadSettings.points[i].ishardEdge)
					{
						x_UV = Mathf.InverseLerp(currentUV_MinMax.x, currentUV_MinMax.y, uvPoint);
						uvs.Add(new Vector2(x_UV, y_UV));
						verts.Add(globalPoint);
					}

					//MAIN MESH VERTEX
					x_UV = Mathf.InverseLerp(currentUV_MinMax.x, currentUV_MinMax.y, uvPoint);
					uvs.Add(new Vector2(x_UV, y_UV));
					verts.Add(globalPoint);

					//Opening edge of UV extrusion
					if (roadSettings.points[i].extrudePoint)
					{
						float curve = roadSettings.points[i].vertex_1.point.x < 0f ? Mathf.Min(0f, roadChainBuilder.roadFormVariables.leftExtrusion) : Mathf.Max(0f, roadChainBuilder.roadFormVariables.rightExtrusion);
						x_UV = Mathf.Abs(curve) /10f;
						uvs.Add(new Vector2(Mathf.Abs(x_UV), y_UV));
						verts.Add(globalPoint);
					}
				}
			}

			if (ring != edgeLoopCount - 1)
				roadChainBuilder.generatedRoadEdgeloops++;
		}

		CloseMeshtasks(roadSettings, op, segment);

		triIndices = CreateTriangles(roadSettings.hardEdges, edgeLoopCount, roadSettings);


		// Assign it all to the mesh
		mesh.SetVertices(verts);
		mesh.SetUVs(0, uvs);

		int materialIndex = 0;
		mesh.subMeshCount = triIndices.Count;
		foreach (List<int> tries in triIndices)
			mesh.SetTriangles(tries, materialIndex++);

		//mesh.SetTriangles( triIndices, 0 ); // <------------  SECOND ENTREE IS MATERIAL INDEX OF MESH
		mesh.RecalculateNormals();
		mesh.RecalculateTangents();
		List<SurfaceScriptable> m = new List<SurfaceScriptable>();
		m.AddRange(roadSettings.GetAllSurfaceSettings(surfaceIndex));

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

    private void CalculateRoadFormVariables(RoadSettings roadSettings, int ring, float t, OrientedCubicBezier3D bezier)
	{
		//Gets bigger with larger radius
		float extrusion = 0;
		float cornerRadius = bezier.GetCornerRadius(t);
		if(Mathf.Abs(cornerRadius) < 500f)
        {
			float clampedRadius = Mathf.Clamp(cornerRadius, -500f, 500f);
			extrusion = roadSettings.runoffAnimationCurve.Evaluate((clampedRadius / 500f));
		}

		if (ring != 0)
        {
			roadChainBuilder.roadFormVariables.mainExtrusion = extrusion;
			roadChainBuilder.roadFormVariables.cornerCamber = extrusion;
			roadChainBuilder.roadFormVariables.cornerRadius = cornerRadius;
		}
	}

	private Quaternion CalculateCornerChamfer(RoadSettings roadSettings)
    {
		if (roadSettings.hasCornerChamfer)
			return Quaternion.Euler(0, 0, (roadChainBuilder.roadFormVariables.cornerCamber) * roadSettings.maxCamber);
		return Quaternion.identity;
	}

	private Vector2 GetRandomNoiseValue(int noiseChannel, RoadSettings roadSettings, Vector2Int noiseCoordinate)
	{
		Vector3 noise = new Vector3();
		NoiseGenerator generator = roadSettings.noiseChannels[noiseChannel].generatorInstance;
        noise = generator.GetNoise(roadChainBuilder.generatedRoadEdgeloops, roadSettings.noiseChannels[noiseChannel]);
		return noise;
	}

    private Vector2 GetCoordinateNoise(int noiseChannel, RoadSettings roadSettings, Vector2Int noiseCoordinate)
    {
        Vector2 noise = new Vector2();
        NoiseGenerator generator = roadSettings.noiseChannels[noiseChannel].generatorInstance;
        noise = generator.GetCoordinateNoise(roadSettings.noiseChannels[noiseChannel], noiseCoordinate);
        return noise;
    }
    #endregion

    private void CreateMeshTasksOnEdgeloop(RoadSettings roadSettings, int ring, RoadSegment segment, OrientedPoint op, int edgeLoopCount)
    {
        foreach (MeshtaskSettings meshtaskSettings in roadSettings.meshtaskSettings)
        {
            switch (meshtaskSettings.meshtaskStyle)
            {
                case MeshtaskStyle.CornerRadius:
					CreateBasedOnCornerRadius(meshtaskSettings, roadSettings, segment, op);
					break;
                case MeshtaskStyle.ExtrusionSize:
					CreateBasedOnExtrusionSize(meshtaskSettings, roadSettings, segment, op);
					break;
                case MeshtaskStyle.Continued:
					CreateContinuedMeshtask(meshtaskSettings, roadSettings, ring, segment, op, edgeLoopCount);
					break;
            }
		}
	}

	private void CreateBasedOnCornerRadius(MeshtaskSettings meshtaskSettings, RoadSettings roadSettings, RoadSegment segment, OrientedPoint op)
    {
		float cornerRadius = roadChainBuilder.roadFormVariables.cornerRadius;
		bool inRange = cornerRadius > meshtaskSettings.minimalCornerRadius && meshtaskSettings.maximumCornerRadius > cornerRadius;
		CreateMeshtaskPoint(meshtaskSettings, roadSettings, segment, op, inRange);
	}

	private void CreateBasedOnExtrusionSize(MeshtaskSettings meshtaskSettings, RoadSettings roadSettings, RoadSegment segment, OrientedPoint op)
    {
		float extrusionSize = meshtaskSettings.meshtaskPosition == MeshtaskPosition.Left ? Mathf.Abs(roadChainBuilder.roadFormVariables.leftExtrusion) : Mathf.Abs(roadChainBuilder.roadFormVariables.rightExtrusion);
		bool inRange = extrusionSize > meshtaskSettings.minimalExtrusionSize;
		CreateMeshtaskPoint(meshtaskSettings, roadSettings, segment, op, inRange);
	}

	private void CreateContinuedMeshtask(MeshtaskSettings meshtaskSettings, RoadSettings roadSettings, int ring, RoadSegment segment, OrientedPoint op, int edgeLoopCount)
    {
		Vector3 currentMeshPosition = segment.transform.TransformPoint(op.pos);

		if (ring == 0)
			CreateNewMeshtask(meshtaskSettings, roadSettings);

        MeshTask task = roadChainBuilder.meshtaskTypeHandler.GetMeshtask(meshtaskSettings);
        if (task != null)
		{
            task.AddPoint(currentMeshPosition,
				segment.transform.rotation * op.rot,
				roadChainBuilder.roadFormVariables);

            if (ring == edgeLoopCount - 1)
                roadChainBuilder.meshtasks.Add(task);
        }
	}

	RoadSegment lastSegment = null;
    private void CreateMeshtaskPoint(MeshtaskSettings meshtaskSettings, RoadSettings roadSettings, RoadSegment segment, OrientedPoint op, bool addPoint = true)
    {
		CreateNewMeshTaskIfNecessary(roadChainBuilder, meshtaskSettings, roadSettings, segment, op);

        if (!addPoint) return;
		AddMeshTaskPoint(roadChainBuilder, meshtaskSettings, op, segment);
    }

    private void CreateNewMeshTaskIfNecessary(RoadChainBuilder roadChainBuilder, MeshtaskSettings meshtaskSettings, RoadSettings roadSettings, RoadSegment segment, OrientedPoint op)
    {
        Vector3 currentMeshTaskVector = segment.transform.TransformPoint(op.pos);
        Vector3 lastMeshTaskVector = roadChainBuilder.meshtaskTypeHandler.GetMeshtaskVector(meshtaskSettings);
        float distanceToLastMeshtask = Vector3.Distance(lastMeshTaskVector, currentMeshTaskVector);

        if (ShouldCreateNewMeshtask(distanceToLastMeshtask, meshtaskSettings) || lastMeshTaskVector == Vector3.zero)
        {
			MeshTask lastMeshtask = roadChainBuilder.meshtaskTypeHandler.GetMeshtask(meshtaskSettings);
            if (lastMeshtask != null)
                roadChainBuilder.meshtasks.Add(lastMeshtask);
            CreateNewMeshtask(meshtaskSettings, roadSettings);
        }
    }

    private bool ShouldCreateNewMeshtask(float distanceToLastMeshtask, MeshtaskSettings meshtaskSettings)
    {
		if (distanceToLastMeshtask > (meshtaskSettings.meshResolution + 1)) 
			return true; 
		else return false;
    }

    private void CreateNewMeshtask(MeshtaskSettings meshtaskSettings, RoadSettings roadSettings)
    {
        MeshTask newMeshtask = new MeshTask(roadSettings,
                                            roadChainBuilder.generatedRoadEdgeloops,
                                            roadSettings.noiseChannels[meshtaskSettings.noiseChannel],
                                            meshtaskSettings,
                                            meshtaskSettings.meshtaskPosition);
        roadChainBuilder.meshtaskTypeHandler.SetMeshtask(newMeshtask, meshtaskSettings);
    }

    private void AddMeshTaskPoint(RoadChainBuilder roadChainBuilder, MeshtaskSettings meshtaskSettings, OrientedPoint op, RoadSegment segment, bool forceAdd = false)
    {
        MeshTask currentMeshtask = roadChainBuilder.meshtaskTypeHandler.GetMeshtask(meshtaskSettings);
        if (currentMeshtask.resolutionIndex == 0 || forceAdd)
        {
			Vector3 currentMeshTaskVector = segment.transform.TransformPoint(op.pos);
			roadChainBuilder.meshtaskTypeHandler.SetMeshtaskVector(currentMeshtask.meshtaskSettings, currentMeshTaskVector);
            currentMeshtask.AddPoint(currentMeshTaskVector, segment.transform.rotation * op.rot, roadChainBuilder.roadFormVariables);
            currentMeshtask.resolutionIndex = meshtaskSettings.meshResolution;
        }
        currentMeshtask.resolutionIndex--;
    }


    private void CloseMeshtasks(RoadSettings roadSettings, OrientedPoint op, RoadSegment segment)
    {
		foreach (MeshtaskSettings meshtaskSettings in roadSettings.meshtaskSettings)
		{
			if(meshtaskSettings.meshtaskContinues) continue;

			MeshTask lastTask = roadChainBuilder.meshtaskTypeHandler.GetMeshtask(meshtaskSettings);
            if (lastTask == null) return;

			Vector3 currentMeshTaskVector = segment.transform.TransformPoint(op.pos);
			Vector3 lastMeshTaskVector = roadChainBuilder.meshtaskTypeHandler.GetMeshtaskVector(meshtaskSettings);
			float distanceToLastMeshtask = Vector3.Distance(lastMeshTaskVector, currentMeshTaskVector);

			if(!ShouldCreateNewMeshtask(distanceToLastMeshtask, meshtaskSettings))
				AddMeshTaskPoint(roadChainBuilder, meshtaskSettings, op, segment, true);

			roadChainBuilder.meshtasks.Add(lastTask);
			CreateNewMeshtask(meshtaskSettings, roadSettings);
        }
	}

    #region UV calculations


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
		uvs.Clear();
		triIndices.Clear();
	}

	private List<List<int>> CreateTriangles(int vertexJumpedCount, int edgeLoopCount, RoadSettings roadSettings)
    {
		List<List<int>> tries = new List<List<int>>();
		foreach (SurfaceScriptable surface in roadSettings.GetAllSurfaceSettings(0))
			tries.Add(new List<int>());

		// Generate Trianges
		// Foreach edge loop (except the last, since this looks ahead one step)
		for (int edgeLoop = 0; edgeLoop < edgeLoopCount - 1; edgeLoop++)
		{
			//Debug.Log("New Edgeloop");
			int rootIndex = (roadSettings.PointCount + vertexJumpedCount) * edgeLoop;
			int rootIndexNext = (roadSettings.PointCount + vertexJumpedCount) * (edgeLoop + 1);
			// Foreach pair of line indices in the 2D shape
			for (int i = 0; i < roadSettings.PointCount; i++)
			{
				int point_1 = roadSettings.points[i].line.x;
				int point_2 = roadSettings.points[i].line.y;

				Vector2Int current = new Vector2Int();
				current[0] = point_1 + rootIndex;
				current[1] = point_2 + rootIndex;

				Vector2Int next = new Vector2Int();
				next[0] = point_1 + rootIndexNext;
				next[1] = point_2 + rootIndexNext;

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
	public MeshtaskSettings meshtaskSettings;
	public MeshTaskType meshTaskType;
	public MeshtaskPosition meshPosition;
	public RoadSettings roadSettings;
	public List<Point> positionVectors = new List<Point>();
	public int startPointIndex;
	public NoiseChannel noiseChannel;
	public int resolutionIndex = 0;

	public MeshTask(RoadSettings settings, int startPointIndex, NoiseChannel noiseChannel, MeshtaskSettings meshtaskSettings, MeshtaskPosition meshPosition)
	{
		this.roadSettings = settings;
		this.startPointIndex = startPointIndex;
		this.noiseChannel = noiseChannel;
		this.meshtaskSettings = meshtaskSettings;
		this.meshPosition = meshPosition;
		this.meshTaskType = meshtaskSettings.meshTaskType;
	}

	public void AddPoint(Vector3 position, Quaternion rotation, RoadFormVariables extrusionVariables)
    {
		positionVectors.Add(new Point(position, rotation, extrusionVariables));
    }

	[System.Serializable]
	public class Point
    {
		public ExtusionVariablesStruct extrusionVariables;
		public Vector3 position;
		public Quaternion rotation;
		public Point(Vector3 position, Quaternion rotation, RoadFormVariables extrusionVariables)
        {
			this.position = position;
			this.rotation = rotation;
			this.extrusionVariables = new ExtusionVariablesStruct(extrusionVariables);
		}
    }
}

public enum MeshTaskType
{
	Guardrail = 0,
	CatchFence,
	GrandStand,
	TecproBarrier,
	VideoBillboard,
	Object
}

public enum MeshtaskStyle
{
	CornerRadius = 0,
	ExtrusionSize,
	Continued,
	Object
}


