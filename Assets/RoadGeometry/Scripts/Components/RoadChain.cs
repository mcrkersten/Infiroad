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
using System.Linq;
using System.Collections;
using System.Collections.Generic;

// This is the parent container for all the road segments
[ExecuteInEditMode]
public class RoadChain : MonoBehaviour {
	public EdgePoint entryPoint;
	public EdgePoint exitPoint;
	[HideInInspector] public List<Transform> segmentPoints = new List<Transform>(); 

	public Mesh2D mesh2D = null; // The 2D shape to be extruded
	public bool loop = false; // Whether or not the last segment should connect to the first
	public float edgeLoopsPerMeter = 2; // Triangle density, in loops per meter!
	public UVMode uvMode = UVMode.TiledDeltaCompensated; // More info on what this is in the enum!


	public MeshSpawnPoints meshSpawnPoints;
	public List<SpawnableGameObject> spawnableGameObjects = new List<SpawnableGameObject>();
	public PowerlineSettings powerlineSettings;
	public FenceSettings fenceSettings;

	private List<GameObject> spawnedGameObjects = new List<GameObject>();

	public RoadSegment[] allSegments;
	private RoadSegment[] segmentsWithMesh;
	private RoadSegment[] segmentsWithoutMesh;

	// Regenerate mesh on instantiation.
	// If you save the mesh in the scene you don't have to do this, but, it's pretty fast anyway so whatevs!
	void Awake() => UpdateMeshes();

#if UNITY_EDITOR
	// For a proper version, make sure you don't do this every frame, this is pretty dang expensive~
	// You'd want to only run this code when the shape is actually changed
	//void Update() => UpdateMeshes();
#endif

	// Iterates through all children / road segments, and updates their meshes!
	public void UpdateMeshes() {
		allSegments = GetComponentsInChildren<RoadSegment>();
		if(allSegments.Length > 0)
        {
			segmentsWithMesh = allSegments.Where(s => s.HasValidNextPoint).ToArray();
			segmentsWithoutMesh = allSegments.Where(s => s.HasValidNextPoint == false).ToArray();

			meshSpawnPoints = new MeshSpawnPoints();
			// We calculate the total length of the road, in order for us to be able to supply a normalized
			// coordinate for how far along the track you are, where
			// 0 = start of the track
			// 0.5 = halfway through the track
			// 1.0 = end of the track
			float[] lengths = segmentsWithMesh.Select(x => x.GetBezierRepresentation(Space.Self).GetArcLength()).ToArray();
			float totalRoadLength = lengths.Sum();

			float startDist = 0f;
			for (int i = 0; i < segmentsWithMesh.Length; i++)
			{
				float endDist = startDist + lengths[i];
				Vector2 uvzStartEnd = new Vector2(
					startDist / totalRoadLength,        // Percentage along track start
					endDist / totalRoadLength           // Percentage along track end
				);
				segmentsWithMesh[i].UpdateMesh(uvzStartEnd);
				startDist = endDist;
			}

			// Clear all segments without meshes
			foreach (RoadSegment seg in segmentsWithoutMesh)
			{
				seg.UpdateMesh(Vector2.zero);
			}
		}
	}

	[ContextMenu("spawnGameObjects")]
	public void SpawnGameObjects()
    {
		powerlineSettings.Clear();
		fenceSettings.Clear();
		foreach (SpawnableGameObject sgo in spawnableGameObjects)
        {
            foreach (int rowIndex in sgo.spawnRows)
            {
				PointRow row = meshSpawnPoints.GetPointRow(rowIndex);
                for (int i = sgo.pointsBetween; i < row.points.Count; i += sgo.pointsBetween)
                {
					//Get rotation for object | Set all rotations to a value to reduce errors
					Vector3 current = new Vector3(row.points[i].x, 0, row.points[i].z);
					Vector3 infront = new Vector3(row.points[i].x, 0, row.points[i].z);
					Vector3 behind = new Vector3(row.points[i].x, 0, row.points[i].z);
					if (i - sgo.pointsBetween > 0)
						infront = new Vector3(row.points[i - sgo.pointsBetween].x, 0, row.points[i - sgo.pointsBetween].z);
					if (i + sgo.pointsBetween < row.points.Count)
						behind = new Vector3(row.points[i + sgo.pointsBetween].x, 0, row.points[i + sgo.pointsBetween].z);

					Quaternion rotation = Quaternion.identity;
                    switch (sgo.rotationStyle)
                    {
                        case Rotation.averageBetweenPoints:
							rotation = Quaternion.FromToRotation(Vector3.forward, behind - infront);
							break;
                        case Rotation.toNextPoint:
							rotation = Quaternion.FromToRotation(Vector3.forward, behind - current);
                            break;
                        case Rotation.toLastPoint:
							rotation = Quaternion.FromToRotation(Vector3.forward, behind - current);
							break;
                        default:
                            break;
                    }

					GameObject spawndObject = Instantiate(sgo.prefab, row.points[i], rotation, sgo.transformParent);

					switch (sgo.type)
                    {
                        case SpawnableGameObjectType.powerpole:
							PowerPole p = spawndObject.GetComponent<PowerPole>();
							p.OnInstantiation();
							powerlineSettings.instantiatedPowerpoles.Add(p);
							break;
						case SpawnableGameObjectType.fence:
							FencePart f = spawndObject.GetComponent<FencePart>();
							fenceSettings.instantiatedFenceParts.Add(f);
							break;
                    }
                    spawnedGameObjects.Add(spawndObject);
                }
			}
        }
		powerlineSettings.CreatePowerlines();
		fenceSettings.CreateFenceBars();
    }
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

[System.Serializable]
public class SpawnableGameObject
{
	public GameObject prefab;
	public List<int> spawnRows;
	public int pointsBetween;
	public SpawnableGameObjectType type;
	public Rotation rotationStyle;
	public Transform transformParent;
}

public enum SpawnableGameObjectType
{
	powerpole = 0,
	fence,
}

public enum Rotation
{
	averageBetweenPoints = 0,
	toNextPoint,
	toLastPoint,
}
