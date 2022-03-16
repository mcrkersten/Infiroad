using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class RoadChainBuilder : MonoBehaviour
{
    public static RoadChainBuilder instance;
    [HideInInspector] public Transform vehicleStartTransform;

    public RoadChainSettings settings;
    public GameObject vehicle;

    [Header("")]
    public GameObject segmentPrefab;
    public GameObject roadChainTriggerObjectPrefab;
    public GameObject roadChainPrefab;

    [Header("")]
    public List<RoadChain> createdRoadChains = new List<RoadChain>();

    private EdgePoint lastEdgePoint;
    public GameObject startSegment;

    //Mesh task variables
    [HideInInspector] public MeshTask currentMeshTask = null;
    [HideInInspector] public float radiusDelay = 0f;
    [HideInInspector] public Vector3 lastMeshPosition = Vector3.positiveInfinity;
    [HideInInspector] public List<MeshTask> meshtasks = new List<MeshTask>();
    GuardrailExtruder guardRailExtruder = new GuardrailExtruder();

    public Road road;
    private ObjectPooler objectPooler;

    private bool startLineIsGenerated = false;

    [Header("Debug")]
    public int generatedRoadEdgeloops = 0;

    private void Awake()
    {
        radiusDelay = 0f;
        InstantiateAssetPools();
    }

    private void Start()
    {
        instance = this;
        createdRoadChains.Clear();
        CreateNextRoadChain(new EdgePoint(EdgeLocation.none, 3, startSegment));
        CreateNextRoadChain();
        RoadChainTrigger.trigger += CreateNextRoadChain;

        SetVehicleStartPosition();
    }

    private void SetVehicleStartPosition()
    {
        vehicle.transform.position = vehicleStartTransform.position;
        vehicle.transform.rotation = vehicleStartTransform.rotation;
    }

    private void InstantiateAssetPools()
    {
        objectPooler = new ObjectPooler();
        objectPooler.InstantiateVegetationTriggerPool(road.assetSpawnPointPoolSize, road.assetSpawnPoint);
        InstantiateVegetationPools();
        InstantiateRoadDecorationPools();
        InstantiateSkyDecorationPools();
    }

    private void InstantiateSkyDecorationPools()
    {
        objectPooler.InstantiateAirDecoration(road.skyDecoration);
    }

    private void InstantiateVegetationPools()
    {
        foreach (VariationSettings s in road.roadVariation)
            foreach (VariationSettings.Variation v in s.roadSettings)
                objectPooler.InstantiateVegitationPool(v.roadSettings.assetPools, v.roadSettings.roadTypeTag);
    }

    private void InstantiateRoadDecorationPools()
    {
        foreach (RoadDecoration pool in road.roadDecorations)
        {
            objectPooler.InstantiateDecorationPool(pool);
        }
    }

    public void CreateNextRoadChain()
    {
        CreateNextRoadChain(lastEdgePoint);

        if (createdRoadChains.Count == 4)
            DeleteRoadChain(0);
    }

    public void CreateNextRoadChain(EdgePoint lastExitPoint)
    {
        //Instantiate
        RoadChain roadChain = InstantiateRoadChain();
        createdRoadChains.Add(roadChain);

        Vector3 position = CalculateRoadChainObjectPosition(lastExitPoint);
        roadChain.transform.position = position;
        this.transform.position = position;

        RoadShape roadShape;
        List<RoadSegment> organized = CreateSegments(lastExitPoint, out roadShape);

        SetRandomHeightToSegments(organized, settings.isFixedBetweenRange, settings.segmentHeightRange);
        OrientSegments(organized);
        SetRandomXaxisToSegments(organized, settings.XaxisVariation);
        OrientSegments(organized); //Orient again to make nice
        SetTangentLenght(organized);

        roadChain.SetOrganizedSegments(organized);
        foreach (RoadSegment segment in organized)
        {
            RoadSettings roadSettting = road.SelectRoadSetting(roadShape, segment);
            roadChain.InitializeSegment(roadSettting, segment);
            HandleMeshTasks(roadSettting, roadChain);
        }

        //
        SpawnRoadDecoration(roadChain, organized);
        SpawnSkyDecoration();
        //SpawnSkyDecoration(road.skyDecoration);
    }

    private void SpawnRoadDecoration(RoadChain roadChain, List<RoadSegment> segments)
    {
        
        if (!startLineIsGenerated)
        {
            //Create on first segment a startline
            int index = 0;
            RoadDecoration lrd = road.roadDecorations.First(t => t.RD_Type == RoadDecorationType.startLine);
            roadChain.ActivateDecor(segments[index], lrd, index);
            startLineIsGenerated = true;
        }

        //Create on last segment a checkpoint
        RoadDecoration rd = road.roadDecorations.First(t => t.RD_Type == RoadDecorationType.checkPoint);
        int lastIndex = segments.Count - 2; //Always mark last index as checkpoint
        roadChain.ActivateDecor(segments[lastIndex], rd, lastIndex);
    }

    private void SpawnSkyDecoration()
    {
        SkyDecoration skyDecoration = road.skyDecoration;
        for (int i = 0; i < skyDecoration.decorAmount; i++)
        {
            float probabilityRoll = Random.Range(0f, 1f);
            foreach (SkyDecor item in skyDecoration.skyDecors)
            {
                if(item.probability > probabilityRoll)
                {
                    float x = Random.Range(-item.spawnAreaSize.x / 2, item.spawnAreaSize.x / 2);
                    float y = skyDecoration.skyHeight + Random.Range(-item.spawnAreaSize.y / 2, item.spawnAreaSize.y / 2);
                    float z = Random.Range(-item.spawnAreaSize.z / 2, item.spawnAreaSize.z / 2);
                    Vector3 position = new Vector3(x, y, z) + this.transform.position;
                    ObjectPooler.Instance.ActivateSkyDecoration(item, position);
                }
            }
        }
    }

    private List<RoadSegment> CreateSegments(EdgePoint lastExitPoint, out RoadShape roadShape)
    {
        EdgePoint entryPoint = CreateEntry(lastExitPoint);
        EdgePoint exitPoint = CreateExit(entryPoint);
        //Create random points between entry and exit
        int nPoints = GetPointAmount(entryPoint, exitPoint);
        List<RoadSegment> unOrganized = CreatePointsbetweenEntryStart(entryPoint, exitPoint, nPoints);
        lastEdgePoint = exitPoint;
        roadShape = GetRoadShape(entryPoint, exitPoint);
        return  OrganizeSegments(unOrganized, entryPoint, exitPoint);
    }

    public RoadShape GetRoadShape(EdgePoint entry, EdgePoint exit)
    {
        switch (entry.edgeLocation)
        {
            case EdgeLocation.Left:
                if (exit.edgeLocation == EdgeLocation.Right)
                    return RoadShape.straights;
                return RoadShape.corners;
            case EdgeLocation.Right:
                if (exit.edgeLocation == EdgeLocation.Left)
                    return RoadShape.straights;
                return RoadShape.corners;
            case EdgeLocation.Top:
                if (exit.edgeLocation == EdgeLocation.Bottom)
                    return RoadShape.straights;
                return RoadShape.corners;
            case EdgeLocation.Bottom:
                if (exit.edgeLocation == EdgeLocation.Top)
                    return RoadShape.straights;
                return RoadShape.corners;
        }
        return RoadShape.corners;
    }

    private void HandleMeshTasks(RoadSettings settings, RoadChain roadchain)
    {
        foreach (MeshTask task in meshtasks)
        {
            if (task.bothSides)
            {
                task.mirror = false;
                guardRailExtruder.Extrude(task, roadchain, settings);
                task.mirror = true;
                guardRailExtruder.Extrude(task, roadchain, settings);
            }
            else
            {
                guardRailExtruder.Extrude(task, roadchain, settings);
            }


            //DEBUG ONLY
            if (RoadChainBuilder.instance.settings.debug)
            {
                foreach (MeshTask.Point p in task.points)
                {
                    if (Mathf.Abs(p.radius) < 1000f)
                    {
                        GameObject g = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        g.transform.position = p.position;
                        g.transform.rotation = p.rotation;
                        g.name = p.radius.ToString();
                        float x = 1f - Mathf.Clamp(Mathf.Abs(p.radius), 1f, 500f) / 500f;
                        g.GetComponent<Renderer>().material.color = RoadChainBuilder.instance.settings.gradient.Evaluate(x);
                    }
                }
            }
        }
        meshtasks.Clear();
    }

    private List<RoadSegment> OrganizeSegments(List<RoadSegment> createdSegments, EdgePoint entry, EdgePoint exit)
    {
        List<RoadSegment> segments = new List<RoadSegment>();
        segments.Add(entry.gameObject.GetComponent<RoadSegment>()); //First
        foreach (RoadSegment g in createdSegments)                               //
            segments.Add(g);                       //BETWEEN
        segments.Add(exit.gameObject.GetComponent<RoadSegment>());  //LAST
        return segments;
    }

    private RoadChain InstantiateRoadChain()
    {
        GameObject roadChainObject = Instantiate(roadChainPrefab);
        RoadChain roadChain = roadChainObject.GetComponent<RoadChain>();
        roadChainObject.name = "RoadBlock-" + createdRoadChains.Count;
        return roadChain;
    }

    private EdgePoint CreateEntry(EdgePoint exitPoint)
    {
        EdgeLocation location = GetInversedEdgeLocation(exitPoint.edgeLocation);
        GameObject segment = Instantiate(segmentPrefab);
        EdgePoint point = new EdgePoint(location, exitPoint.edgePointPositionIndex, segment);
        segment.transform.position = exitPoint.gameObject.transform.position;
        segment.transform.rotation = exitPoint.gameObject.transform.rotation;
        segment.name = "EntrySegment";
        return point;
    }

    private EdgePoint CreateExit(EdgePoint entryPoint)
    {
        EdgeLocation location = GetRandomExitLocation(entryPoint);
        GameObject segment = Instantiate(segmentPrefab);
        EdgePoint point = new EdgePoint(location, GetRandomExitPointIndex(entryPoint.edgeLocation, location), segment);
        segment.transform.position = GetEdgePointLocalLocationPosition(point) + this.transform.position;
        segment.transform.rotation = point.edgeRotation;
        segment.name = "ExitSegment";
        Instantiate(roadChainTriggerObjectPrefab, segment.transform.position, segment.transform.rotation, segment.transform);
        return point;
    }

    private void SetTangentLenght(List<RoadSegment> segments)
    {
        for (int i = 0; i < segments.Count; i++)
        {
            float distance = 0f;
            //Middle point, take smallest distance of point before or after.
            if (i != 0 && i != segments.Count - 1)
            {
                float dist0 = Vector3.Distance(segments[i].transform.position, segments[i - 1].transform.position);
                float dist1 = Vector3.Distance(segments[i].transform.position, segments[i + 1].transform.position);
                distance = Mathf.Min(dist0, dist1);
            }
            //First point, only take distance of next point
            else if (i == 0)
            {
                distance = Vector3.Distance(segments[i].transform.position, segments[i + 1].transform.position);
            }
            //Last point, only take distance of point before
            else
            {
                distance = Vector3.Distance(segments[i].transform.position, segments[i - 1].transform.position);
            }
            segments[i].tangentLength = Mathf.Clamp(distance/2f, 0, 45);
        }
    }

    private EdgeLocation GetRandomExitLocation(EdgePoint entryPoint)
    {
        List<EdgeLocation> possibility = new List<EdgeLocation> { (EdgeLocation)0, (EdgeLocation)1, (EdgeLocation)2, (EdgeLocation)3 };
        possibility.Remove(entryPoint.edgeLocation);

        if (entryPoint.edgeLocation == EdgeLocation.none)
            possibility.Remove(EdgeLocation.Bottom);

        if(entryPoint.edgeLocation == EdgeLocation.Left || entryPoint.edgeLocation == EdgeLocation.Right)
        {
            if (entryPoint.edgePointPositionIndex == 0)
                possibility.Remove(EdgeLocation.Bottom);
            if (entryPoint.edgePointPositionIndex == 4)
                possibility.Remove(EdgeLocation.Top);
        }

        if (entryPoint.edgeLocation == EdgeLocation.Top || entryPoint.edgeLocation == EdgeLocation.Bottom)
        {
            if (entryPoint.edgePointPositionIndex == 0)
                possibility.Remove(EdgeLocation.Left);
            if (entryPoint.edgePointPositionIndex == 4)
                possibility.Remove(EdgeLocation.Right);
        }

        return possibility[Random.Range(0, possibility.Count)];
    }

    private int GetPointAmount(EdgePoint entry, EdgePoint exit)
    {
        int nOfPoints = 0;
        switch (entry.edgeLocation)
        {
            case EdgeLocation.Left:
                if (exit.edgeLocation == EdgeLocation.Right) { nOfPoints = settings.straight_NpointsBetween; }
                else { nOfPoints = nOfPoints = settings.corner_NpointsBetween; }
                break;
            case EdgeLocation.Right:
                if (exit.edgeLocation == EdgeLocation.Left) { nOfPoints = settings.straight_NpointsBetween; }
                else { nOfPoints = nOfPoints = settings.corner_NpointsBetween; }
                break;
            case EdgeLocation.Top:
                if (exit.edgeLocation == EdgeLocation.Bottom) { nOfPoints = settings.straight_NpointsBetween; }
                else { nOfPoints = nOfPoints = settings.corner_NpointsBetween; }
                break;
            case EdgeLocation.Bottom:
                if (exit.edgeLocation == EdgeLocation.Top) { nOfPoints = settings.straight_NpointsBetween; }
                else { nOfPoints = nOfPoints = settings.corner_NpointsBetween; }
                break;
            case EdgeLocation.none:
                if (exit.edgeLocation == EdgeLocation.Top) { nOfPoints = settings.straight_NpointsBetween; }
                else { nOfPoints = nOfPoints = settings.corner_NpointsBetween; }
                break;
        }
        return nOfPoints;
    }

    private Vector3 GetEdgePointLocalLocationPosition(EdgePoint exitPoint)
    {
        float side = ((settings.gridSize) / 2f);

        float offset = (float)settings.gridSize / settings.sidePointAmount;
        float normalize = -((settings.gridSize - offset)/ 2f);

        float addition = offset * exitPoint.edgePointPositionIndex;
        float pointPosition = normalize + addition;

        switch (exitPoint.edgeLocation)
        {
            case EdgeLocation.Left:
                return new Vector3(-side, 0, pointPosition);
            case EdgeLocation.Right:
                return new Vector3(side, 0, pointPosition);
            case EdgeLocation.Top:
                return new Vector3(pointPosition, 0, side);
            case EdgeLocation.Bottom:
                return new Vector3(pointPosition, 0, -side);
            case EdgeLocation.none:
                return new Vector3(0, 0, -side);
        }
        return Vector3.zero;
    }

    private Vector3 CalculateRoadChainObjectPosition(EdgePoint exitPoint)
    {
        Vector3 p = exitPoint.gameObject.transform.root.transform.position;
        if (exitPoint.edgeLocation == EdgeLocation.none)
            p = Vector3.zero;

        switch (exitPoint.edgeLocation)
        {
            case EdgeLocation.Left:
                p[0] -= settings.gridSize;
                break;
            case EdgeLocation.Right:
                p[0] += settings.gridSize;
                break;
            case EdgeLocation.Top:
                p[2] += settings.gridSize;
                break;
            case EdgeLocation.Bottom:
                p[2] -= settings.gridSize;
                break;
            case EdgeLocation.none:
                break;
        }
        return p;
    }

    private int GetRandomExitPointIndex(EdgeLocation entry, EdgeLocation exit)
    {
        int max = settings.sidePointAmount;
        int min = 0;

        switch (entry)
        {
            case EdgeLocation.Left:
                if (exit != EdgeLocation.Right)
                    min += 2;
                break;
            case EdgeLocation.Right:
                if (exit != EdgeLocation.Left)
                    max -= 2;
                break;
            case EdgeLocation.Top:
                if (exit != EdgeLocation.Bottom)
                    max -= 2;
                break;
            case EdgeLocation.Bottom:
                if (exit != EdgeLocation.Top)
                    min += 2;
                break;
            default:
                break;
        }
        return Random.Range(min, max);
    }

    private List<RoadSegment> CreatePointsbetweenEntryStart(EdgePoint entry, EdgePoint exit, int nOfPoints)
    {
        RoadSegment entrySegment = entry.gameObject.GetComponent<RoadSegment>();
        RoadSegment exitSegment = exit.gameObject.GetComponent<RoadSegment>();
        List<RoadSegment> segments = new List<RoadSegment>();

        for (int i = 1; i <= nOfPoints; i++)
        {
            float t = (float)i / (nOfPoints + 1);
            Vector3 segmentPosition = Vector3.Lerp(Vector3.Lerp(entrySegment.transform.position, this.transform.position, t), Vector3.Lerp(this.transform.position, exitSegment.transform.position, t), t);

            if(!ProximityAlert(segments, segmentPosition))
            {
                RoadSegment segment = CreateSegment(segmentPosition, segments.Count + 1, this.transform);
                segments.Add(segment);
            }
        }
        return segments;
    }

    /// <summary>
    /// If "proximity" is in the proximity of any transform in list
    /// </summary>
    /// <param name="segments"></param>
    /// <param name="proximity"></param>
    /// <returns></returns>
    private bool ProximityAlert(List<RoadSegment> segments, Vector3 proximity)
    {
        if (segments.Count < 1)
            return false;

        for (int i = 1; i < segments.Count - 1; i++)
        {
            Vector3 behind = segments[i - 1].transform.position;
            bool behindProxViolated = Vector3.Distance(proximity, behind) < settings.segmentDeletionProximity;
            if (behindProxViolated)
            {
                Debug.Log("Proximity violation");
                return true;
            }
        }
        return false;
    }

    private RoadSegment CreateSegment(Vector3 position, int index, Transform parent)
    {
        GameObject segment = Instantiate(segmentPrefab, position, parent.rotation, parent);
        segment.name = "Segement-" + index;
        return segment.GetComponent<RoadSegment>();
    }

    /// <summary>
    /// Rotate segments to average between front and behind segments
    /// </summary>
    /// <param name="segments"></param>
    private void OrientSegments(List<RoadSegment> segments)
    {
        for (int i = 1; i < segments.Count - 1; i++)
        {
            Vector3 infront = segments[i + 1].transform.position;
            Vector3 behind = segments[i - 1].transform.position;
            segments[i].transform.rotation = Quaternion.LookRotation(infront - behind);
        }
    }

    /// <summary>
    /// Set random y-Height to segments
    /// </summary>
    /// <param name="segments"></param>
    /// <param name="fromZero">New random from Zero</param>
    private void SetRandomHeightToSegments(List<RoadSegment> segments, bool fromZero, float heightRange)
    {
        for (int i = 1; i < segments.Count; i++)
        {

            float baseHeight = baseHeight = segments[i - 1].transform.position.y;

            if (fromZero)
                baseHeight = 0;

            float calculatedHeight = baseHeight + Random.Range(-heightRange, heightRange);

            Transform segment = segments[i].transform;
            Vector3 position = new Vector3(segment.position.x, calculatedHeight, segments[i].transform.position.z);
            segments[i].transform.position = position;
        }
    }

    /// <summary>
    /// Add random to X-Axis local positions.
    /// Does not add to First and last Segment.
    /// </summary>
    /// <param name="segments"></param>
    private void SetRandomXaxisToSegments(List<RoadSegment> segments, float range)
    {
        for (int i = 1; i < segments.Count; i++)
        {
            Vector3 pos = segments[i].transform.localPosition;
            Vector3 random = new Vector3(Random.Range(-range, range), 0f, 0f);
            segments[i].transform.localPosition = pos + random;
        }
    }

    /// <summary>
    /// Returns the opposite of EdgeLocation enum
    /// </summary>
    /// <param name="toInverse"></param>
    /// <returns></returns>
    private EdgeLocation GetInversedEdgeLocation(EdgeLocation edgeLocation)
    {
        EdgeLocation inverse = EdgeLocation.none;
        switch (edgeLocation)
        {
            case EdgeLocation.Left:
                inverse = EdgeLocation.Right;
                break;
            case EdgeLocation.Right:
                inverse = EdgeLocation.Left;
                break;
            case EdgeLocation.Top:
                inverse = EdgeLocation.Bottom;
                break;
            case EdgeLocation.Bottom:
                inverse = EdgeLocation.Top;
                break;
            case EdgeLocation.none:
                inverse = EdgeLocation.Bottom;
                break;
        }
        return inverse;
    }

    private void DeleteRoadChain(int index)
    {
        Destroy(createdRoadChains[index].gameObject);
        createdRoadChains.RemoveAt(index);
    }

    [System.Serializable]
    public class RoadChainSettings
    {
        public bool segmentVariation;

        [Header("Debug settings")]
        public Gradient gradient;
        public bool debug;

        [Space]
        [Tooltip("Amount of points on the side of a RoadchainBlock")]
        public int sidePointAmount;
        public int gridSize;//Unity UnitSize

        public int straight_NpointsBetween;
        public int corner_NpointsBetween;

        [Header("New segment settings")]
        [Range(0f, 50f)]
        public float XaxisVariation;

        [Range(0f, 25f)]
        public float segmentHeightRange;
        public bool isFixedBetweenRange;

        [Range(10, 50)]
        public int segmentDeletionProximity;
    }

    private void OnDestroy()
    {
        RoadChainTrigger.trigger -= CreateNextRoadChain;
    }
}

[System.Serializable]
public class EdgePoint
{
    public EdgeLocation edgeLocation;
    public int edgePointPositionIndex; //Point on side.
    public Quaternion edgeRotation { get { return GetEdgeRotation(); } }
    public GameObject gameObject { get { return GO; } }
    private GameObject GO;
    public EdgePoint(EdgeLocation edge ,int index, GameObject gameObject)
    {
        this.edgePointPositionIndex = index;
        this.edgeLocation = edge;
        this.GO = gameObject;
    }

    private Quaternion GetEdgeRotation()
    {
        Quaternion rotation = Quaternion.identity;
        switch (edgeLocation)
        {
            case EdgeLocation.Left:
                rotation = Quaternion.Euler(0, -90, 0);
                break;
            case EdgeLocation.Right:
                rotation = Quaternion.Euler(0, 90, 0);
                break;
            case EdgeLocation.Top:
                rotation = Quaternion.Euler(0, 0, 0);
                break;
            case EdgeLocation.Bottom:
                rotation = Quaternion.Euler(0, 180, 0);
                break;
            case EdgeLocation.none:
                rotation = Quaternion.Euler(0, 0, 0);
                break;
            default:
                break;
        }
        return rotation;
    }
}

public enum EdgeLocation
{
    Left = 0,
    Right,
    Top,
    Bottom,
    none //automatically becomes bottom entry
}
