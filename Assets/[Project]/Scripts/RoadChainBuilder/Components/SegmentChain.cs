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

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Unity.Collections;
using static UnityEngine.Rendering.RayTracingAccelerationStructure;

// This is the parent container for all the road segments
public class SegmentChain : MonoBehaviour {
	[ReadOnly] public int ChainIndex = 0;
    [HideInInspector] public int SegmentIndex = 0;
    public bool loop = false; // Whether or not the last segment should connect to the first
	public List<RoadSegment> organizedSegments = new List<RoadSegment>();
	public LineRenderer line;
	public LayerMask trackLayer;
	public UVMode uvMode = UVMode.TiledDeltaCompensated; // More info on what this is in the enum!

	public MeshSpawnPoints meshSpawnPoints;

	[HideInInspector] public List<GameObject> activatedPooledObjects = new List<GameObject>();
	[HideInInspector] public Road road;
	[HideInInspector] public RoadSettings roadSettings;

	//public void InitializeAllSegments(RoadSettings settings, List<RoadSegment> roadSegments)
 //   {
	//	devSettings.Add(settings);
	//	meshSpawnPoints = new MeshSpawnPoints();
	//	organizedSegments = roadSegments;

	//	foreach (RoadSegment rs in organizedSegments)
	//		rs.transform.SetParent(this.transform);

	//	foreach (RoadSegment seg in organizedSegments)
 //       {
	//		CreateMesh(settings, seg);
	//		ActivateVegetationAssetTriggersOnAssetSpawnEdge(settings, seg);
	//	}
	//}

	public void SetOrganizedSegments(List<RoadSegment> segments)
    {
		organizedSegments = segments;
		foreach (RoadSegment rs in organizedSegments)
		{
			rs.CreateBezier();
			rs.transform.SetParent(this.transform);
		}
	}

	public void CreateSegmentMesh(RoadSettings settings, RoadSegment roadSegment)
    {
		roadSettings = settings;
		CreateMesh(settings, roadSegment);
		ActivateVegetationAssetTriggersOnAssetSpawnEdge(settings, roadSegment);
	}

	public void CreateMesh(RoadSettings roadSettings, RoadSegment segment)
	{
		segment.CreateMesh(Vector2.zero, roadSettings, organizedSegments.IndexOf(segment), ChainIndex); //Creates mesh
	}

	public OrientedPoint GetOrientedPointOnRoad(float percentage, int segmentIndex, Ease ease)
    {
		Debug.Log(segmentIndex);
		return organizedSegments[segmentIndex].bezier.GetOrientedPoint(percentage, .01f, ease);
	}

	public void ActivateDecor(RoadSegment segment, RoadDecoration roadDecoration)
    {
		SegmentChainBuilder roadChainBuilder = SegmentChainBuilder.instance;
		RoadSettings roadSettings = segment.roadSetting;
		List<GameObject> decorObjects = ObjectPooler.Instance.GetRoadDecorationFromPool(roadDecoration.poolIndex);
		int segmendLoops = segment.edgeLoopCount;
		int decorIndex = 0;
		foreach (Decoration decorItem in roadDecoration.decor)
		{
			int currentEdgeloop = (int)Mathf.Lerp(0, segmendLoops, 1f - Mathf.Clamp01(roadDecoration.mainCurveTime + decorItem.curveTime));
			int edgeLoop = segmendLoops - currentEdgeloop;
			Vector3 noise = Vector3.zero;
			foreach (NoiseChannel nts in roadSettings.noiseChannels)
				noise += nts.generatorInstance.GetNoise(segment.startEndEdgeLoop.x + edgeLoop, nts);

			Vector3 decorPosition = decorItem.position;

			OrientedPoint nextPoint = GetOrientedPointOnRoad(Mathf.Clamp01(roadDecoration.mainCurveTime + decorItem.curveTime + .15f), segment.index, segment.roadSetting.rotationEasing);
			Vector3 next = segment.transform.TransformPoint(nextPoint.LocalToWorldPos(decorPosition + noise));
			OrientedPoint prevPoint = GetOrientedPointOnRoad(Mathf.Clamp01(roadDecoration.mainCurveTime + decorItem.curveTime - .15f), segment.index, segment.roadSetting.rotationEasing);
			Vector3 prev = segment.transform.TransformPoint(prevPoint.LocalToWorldPos(decorPosition + noise));

			OrientedPoint orientedPoint = GetOrientedPointOnRoad(Mathf.Clamp01(roadDecoration.mainCurveTime + decorItem.curveTime), segment.index, segment.roadSetting.rotationEasing);
			Vector3 localPosition = orientedPoint.LocalToWorldPos(decorPosition + noise);
			Vector3 worldPosition = segment.transform.TransformPoint(localPosition);

			GameObject decorObject = decorObjects[decorIndex];

            decorObject.transform.position = worldPosition;
			Vector3 positionOut = new Vector3();
            decorObject.transform.rotation = GetUpVector(out positionOut, worldPosition) * Quaternion.LookRotation(next - prev);
			decorObject.transform.position = positionOut;

			decorObject.SetActive(true);
			activatedPooledObjects.Add(decorObject);

			if (decorObject.CompareTag("StartPosition"))
				roadChainBuilder.vehicleStartTransform = decorObject.transform.GetChild(0);

			//if (decorObject.CompareTag("Trigger") && decorObject.GetComponent<TriggerObject>().triggerType == TriggerType.SegmentPoint)
			//	Debug.Log(decorObject, decorObject);

			decorIndex++;
		}
	}

	private Quaternion GetUpVector(out Vector3 positionOut, Vector3 position)
    {
		RaycastHit hit;
		if (Physics.Raycast(position + Vector3.up, Vector3.down, out hit, Mathf.Infinity, trackLayer))
		{
			positionOut = hit.point;
			return Quaternion.Euler(hit.normal);
		}
		positionOut = position;
		return Quaternion.identity;
	}

	public void OnDestroy()
    {
        foreach (GameObject item in activatedPooledObjects)
			if(item)
				item.SetActive(false);
    }

    #region Initialize VegetationAssetTriggers
    public void ActivateVegetationAssetTriggersOnAssetSpawnEdge(RoadSettings roadSettings, RoadSegment segment)
    {
        foreach (AssetSpawnEdge spawnEdge in segment.assetSpawnEdges)
        {
			InitializeAssetSpawnEdge(spawnEdge, roadSettings);
		}
    }

	private void InitializeAssetSpawnEdge(AssetSpawnEdge spawnEdge, RoadSettings road)
    {
        foreach (AssetSpawnPoint item in spawnEdge.assetSpawnPoints)
        {
            if (item.spawnBetweenPoints)
            {
				for (float i = 0.5f; i <= item.spawnPointsBetweenAmount; i++)
				{
					Vector3 point = Vector3.Lerp(spawnEdge.leftPoint, spawnEdge.rightPoint, (float)i / item.spawnPointsBetweenAmount);
					AssetTrigger x = ObjectPooler.Instance.ActivateAssetTrigger(point, item.assetPointType);
					x.gameObject.name = road.roadTypeTag;
				}
            }
            else
            {
				AssetTrigger x = ObjectPooler.Instance.ActivateAssetTrigger(spawnEdge.leftPoint, item.assetPointType);
				x.gameObject.name = road.roadTypeTag;
			}
		}
	}
	#endregion
}

[System.Serializable]
public class MeshSpawnPoints {

	public List<PointRow> pointRows = new List<PointRow>();

	public void AddPoint(Vector3 point, int index)
	{
		PointRow row = pointRows.Find(i => i.index == index);
		if (row == null)
        {
			row = new PointRow(index);
			pointRows.Add(row);
		}

		row.points.Add(point);

	}

	public PointRow GetPointRow(int index)
    {
		PointRow row = pointRows.Find(i => i.index == index);
		if (row != null)
			return row;
		return null;
	}
}

public class AssetSpawnEdge
{
	public Vector3 leftPoint;
	public Vector3 rightPoint;
	public List<AssetSpawnPoint> assetSpawnPoints = new List<AssetSpawnPoint>();

	public AssetSpawnEdge(Vector3 left, Vector3 right, List<AssetSpawnPoint> att)
    {
		assetSpawnPoints = att;
		leftPoint = left;
		rightPoint = right;
    }
}

[System.Serializable]
public class PointRow
{
	public int index;
	public List<Vector3> points = new List<Vector3>();
	public PointRow(int index)
	{
		this.index = index;
	}
}

//[ContextMenu("spawnGameObjects")]
//public void SpawnGameObjects()
//   {
//	powerlineSettings.Clear();
//	fenceSettings.Clear();
//	foreach (SpawnableGameObject sgo in spawnableGameObjects)
//       {
//           foreach (int rowIndex in sgo.spawnRows)
//           {
//			PointRow row = meshSpawnPoints.GetPointRow(rowIndex);
//               for (int i = sgo.pointsBetween; i < row.points.Count; i += sgo.pointsBetween)
//               {
//				//Get rotation for object | Set all rotations to a value to reduce errors
//				Vector3 current = new Vector3(row.points[i].x, 0, row.points[i].z);
//				Vector3 infront = new Vector3(row.points[i].x, 0, row.points[i].z);
//				Vector3 behind = new Vector3(row.points[i].x, 0, row.points[i].z);
//				if (i - sgo.pointsBetween > 0)
//					infront = new Vector3(row.points[i - sgo.pointsBetween].x, 0, row.points[i - sgo.pointsBetween].z);
//				if (i + sgo.pointsBetween < row.points.Count)
//					behind = new Vector3(row.points[i + sgo.pointsBetween].x, 0, row.points[i + sgo.pointsBetween].z);

//				Quaternion rotation = Quaternion.identity;
//                   switch (sgo.rotationStyle)
//                   {
//                       case Rotation.averageBetweenPoints:
//						rotation = Quaternion.FromToRotation(Vector3.forward, behind - infront);
//						break;
//                       case Rotation.toNextPoint:
//						rotation = Quaternion.FromToRotation(Vector3.forward, behind - current);
//                           break;
//                       case Rotation.toLastPoint:
//						rotation = Quaternion.FromToRotation(Vector3.forward, behind - current);
//						break;
//                       default:
//                           break;
//                   }

//				GameObject spawndObject = Instantiate(sgo.prefab, row.points[i], rotation, sgo.transformParent);

//				switch (sgo.type)
//                   {
//                       case SpawnableGameObjectType.powerpole:
//						PowerPole p = spawndObject.GetComponent<PowerPole>();
//						p.OnInstantiation();
//						powerlineSettings.instantiatedPowerpoles.Add(p);
//						break;
//					case SpawnableGameObjectType.fence:
//						FencePart f = spawndObject.GetComponent<FencePart>();
//						fenceSettings.instantiatedFenceParts.Add(f);
//						break;
//                   }
//                   spawnedGameObjects.Add(spawndObject);
//               }
//			 }
//		}
//	powerlineSettings.CreatePowerlines();
//	fenceSettings.CreateFenceBars();
//   }

//[System.Serializable]
//public class SpawnableGameObject
//{
//	public GameObject prefab;
//	public List<int> spawnRows;
//	public int pointsBetween;
//	public SpawnableGameObjectType type;
//	public Rotation rotationStyle;
//	public Transform transformParent;
//}

//public enum SpawnableGameObjectType
//{
//	powerpole = 0,
//	fence,
//}

//public enum Rotation
//{
//	averageBetweenPoints = 0,
//	toNextPoint,
//	toLastPoint,
//}
