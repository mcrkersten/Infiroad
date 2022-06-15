using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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

    public Queue<RoadChain> fixedRoadChains = new Queue<RoadChain>();
    private RoadChain lastFixedRoadChain;

    private EdgePoint lastEdgePoint;
    public GameObject startSegment;

    //Mesh task variables
    [HideInInspector] public MeshtaskTypeHandler meshtaskTypeHandler;


    [HideInInspector] public ExtrusionVariables extrusionVariables;
    [HideInInspector] public Vector3 lastMeshPosition = Vector3.positiveInfinity;
    [HideInInspector] public List<MeshTask> meshtasks = new List<MeshTask>();

    MeshtaskExtruder meshtaskExtruder = new MeshtaskExtruder();

    public Road road;
    private ObjectPooler objectPooler;

    private bool startLineIsGenerated = false;

    [Header("Debug")]
    public int generatedRoadEdgeloops = 0;

    public List<MeshtaskSettings> meshtaskSettings = new List<MeshtaskSettings>();

    private void Awake()
    {
        instance = this;
        PositionStartSegment();
    }

    public void InitializeGenerator()
    {
        meshtaskTypeHandler = new MeshtaskTypeHandler();
        foreach (MeshtaskSettings item in meshtaskSettings)
            meshtaskTypeHandler.SetDictionary(item, null);
        UpdateAllRoadSettings();
        extrusionVariables = new ExtrusionVariables(.1f);
        InstantiateAssetPools();
    }

    public void GenerateRoadForGamemode(GameModeManager gameModeManager)
    {
        createdRoadChains.Clear();
        switch (gameModeManager.gameMode)
        {
            case GameMode.Relaxed:
                StartRandomRoadChain();
                break;
            case GameMode.TimeTrial:
                StartRandomRoadChain();
                break;
            case GameMode.RandomSectors:
                StartRandomRoadChain();
                break;
            case GameMode.FixedSectors:
                Debug.Log("BUILD");
                StartFixedRoadChain(gameModeManager.fixedSectors);
                break;
        }

        if (vehicle != null)
            SetVehicleStartPosition();
    }

    public void CreateNextRoadChain()
    {
        CreateNextRandomRoadChain(lastEdgePoint);
        if (createdRoadChains.Count == 4)
            DeleteRoadChain(0);
    }

    public void CreateNextFixedSector_Trigger()
    {
        RoadChain next = SelectAndPositionNextFixedRoadChain();
        CreateSegmentMeshes(next.organizedSegments, next, RoadShape.straights);
        SpawnAllDecoration(next.organizedSegments, next);
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

    public Sector GenerateSector()
    {
        List<RoadSegment> segments_0 = CreateNextRandomRoadChain(new EdgePoint(EdgeLocation.none, 3, startSegment), false);
        RoadChain chain = segments_0[0].transform.root.GetComponent<RoadChain>();
        chain.organizedSegments = segments_0;
        return new Sector(chain);
    }

    private void PositionStartSegment()
    {
        startSegment.transform.position = new Vector3(0, 0, -settings.gridSize / 2f);
    }

    private void StartRandomRoadChain()
    {
        CreateNextRandomRoadChain(new EdgePoint(EdgeLocation.none, 3, startSegment));
        CreateNextRoadChain();
        EventTriggerManager.roadChainTrigger += CreateNextRoadChain;
    }

    private void StartFixedRoadChain(List<Sector> sectors)
    {
        foreach (Sector s in sectors)
            fixedRoadChains.Enqueue(s.roadChain);

        Debug.Log(fixedRoadChains.Count);
        EventTriggerManager.roadChainTrigger += CreateNextFixedSector_Trigger;
        CreateNextFixedSector_Trigger();
        CreateNextFixedSector_Trigger();
    }

    /// <summary>
    /// Creates a full list of all materials that can spawn on the road.
    /// </summary>
    private void UpdateAllRoadSettings()
    {
        foreach (VariationSettings variation in road.roadVariation)
            foreach (VariationSettings.Variation var in variation.roadSettings)
                var.roadSettings.InitializeRoadSettings();
    }

    private void SetVehicleStartPosition()
    {
        vehicle.transform.position = vehicleStartTransform.position;
        vehicle.transform.rotation = vehicleStartTransform.rotation;
    }

    #region InstantiatePools
    private void InstantiateAssetPools()
    {
        objectPooler = new ObjectPooler();
        objectPooler.InstantiateVegetationTriggerPool(road.assetSpawnPointPoolSize, road.assetSpawnPoint);
        objectPooler.InstantiateAirDecoration(road.skyDecoration);
        InstantiateVegetationPools();
        InstantiateRoadDecorationPools();
        InstantiateMeshtaskPools();
    }

    private void InstantiateMeshtaskPools()
    {
        foreach (VariationSettings s in road.roadVariation)
            foreach (VariationSettings.Variation v in s.roadSettings)
                foreach (MeshtaskSettings mts in v.roadSettings.meshtaskSettings)
                    objectPooler.InstantiateMeshtaskObjects(mts);
    }

    private void InstantiateVegetationPools()
    {
        foreach (VariationSettings s in road.roadVariation)
            foreach (VariationSettings.Variation v in s.roadSettings)
                objectPooler.InstantiateVegitationPool(v.roadSettings.assetPools, v.roadSettings.roadTypeTag);
    }

    private void InstantiateRoadDecorationPools()
    {
        foreach (RoadDecoration pool in road.standardDecoration)
            objectPooler.InstantiateRoadDecorationDecorationPool(pool);
        foreach (RoadDecoration pool in road.randomizedDecoration)
            objectPooler.InstantiateRoadDecorationDecorationPool(pool);
    }
    #endregion

    private List<RoadSegment> CreateNextRandomRoadChain(EdgePoint lastExitPoint, bool decoration = true)
    {
        //Instantiate
        RoadChain roadChain = InstantiateRoadChain();
        Vector3 position = CalculateRoadChainObjectPosition(lastExitPoint);
        roadChain.transform.position = position;
        this.transform.position = position;

        RoadShape roadShape;
        List<RoadSegment> organized = CreateSegments(lastExitPoint, out roadShape);
        roadChain.SetOrganizedSegments(organized);
        if (decoration)
        {
            createdRoadChains.Add(roadChain);
            CreateSegmentMeshes(organized, roadChain, roadShape);
            SpawnAllDecoration(organized, roadChain);
        }
        return organized;
    }

    private RoadChain SelectAndPositionNextFixedRoadChain()
    {
        if (lastFixedRoadChain == null)
            return CreateFirstFixedRoadChain();

        RoadChain next = fixedRoadChains.Dequeue();
        next.gameObject.SetActive(true);
        Transform lastSegment = lastFixedRoadChain.organizedSegments[lastFixedRoadChain.organizedSegments.Count - 1].transform;

        List<Transform> toDestroy = new List<Transform>();
        foreach (Transform child in next.transform)
            if (!child.CompareTag("Segment"))
                toDestroy.Add(child);

        int count = toDestroy.Count;
        for (int i = 0; i < count; i++)
            Destroy(toDestroy[i].gameObject);


        GameObject temp = new GameObject("Temp");
        next.transform.rotation = lastSegment.rotation;
        temp.transform.position = next.organizedSegments[0].transform.position;
        next.transform.parent = temp.transform;
        temp.transform.position = lastSegment.position;
        next.transform.parent = null;
        Destroy(temp);

        fixedRoadChains.Enqueue(lastFixedRoadChain);
        lastFixedRoadChain = next;
        return next;
    }

    private RoadChain CreateFirstFixedRoadChain()
    {
        RoadChain next = fixedRoadChains.Dequeue();
        next.gameObject.SetActive(true);
        next.transform.rotation = Quaternion.identity;
        next.transform.position = Vector3.zero;
        lastFixedRoadChain = next;


        return next;
    }

    private void PositionSegments(List<RoadSegment> segments)
    {
        SetRandomHeightToSegments(segments, settings.isFixedBetweenRange, settings.segmentHeightRange);
        OrientSegments(segments);
        SetRandomXaxisToSegments(segments, settings.XaxisVariation);
        OrientSegments(segments); //Orient again to make nice
        SetTangentLenght(segments);
    }

    private void CreateSegmentMeshes(List<RoadSegment> segments, RoadChain roadChain, RoadShape roadShape)
    {
        roadChain.SetOrganizedSegments(segments);
        foreach (RoadSegment segment in segments)
        {
            RoadSettings roadSettting = road.SelectRoadSetting(roadShape, segment);
            roadChain.CreateSegmentMesh(roadSettting, segment);

            //Create Guardrails
            foreach (MeshtaskSettings meshtaskSettings in roadSettting.meshtaskSettings)
                HandleMeshTasks(meshtaskSettings, roadChain);

            meshtasks.Clear();
        }
    }

    private void SpawnAllDecoration(List<RoadSegment> segments, RoadChain roadChain)
    {
        int index = Random.Range(1, segments.Count - 1);
        SpawnRandomRoadDecoration(roadChain, segments[index], index);
        SpawnStandardRoadDecoration(roadChain, segments);
        SpawnSkyDecoration();
    }

    private void SpawnStandardRoadDecoration(RoadChain roadChain, List<RoadSegment> segments)
    {
        
        if (!startLineIsGenerated)
        {
            //Create on first segment a startline
            RoadDecoration deco_0 = road.standardDecoration.First(t => t.poolIndex == 0);
            roadChain.ActivateDecor(segments[0], deco_0, 0);
            startLineIsGenerated = true;
        }

        //Create on last segment a checkpoint
        RoadDecoration deco_1 = road.standardDecoration.First(t => t.poolIndex == 1);
        int lastIndex = segments.Count - 2; //Always mark last index as checkpoint
        roadChain.ActivateDecor(segments[lastIndex], deco_1, lastIndex);
    }

    private void SpawnRandomRoadDecoration(RoadChain roadChain, RoadSegment segment, int segmentIndex)
    {
        if(road.randomizedDecoration.Count != 0)
        {
            RoadDecoration deco = road.randomizedDecoration[Random.Range(0, road.randomizedDecoration.Count)];
            roadChain.ActivateDecor(segment, deco, segmentIndex);
        }
    }

    private void SpawnRandomSceneryObjects(RoadChain roadChain, RoadSegment segment, int segmentIndex)
    {
        RoadDecoration deco = road.sceneryObjects[Random.Range(0, road.sceneryObjects.Count)];
        roadChain.ActivateDecor(segment, deco, segmentIndex);
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

    /// <summary>
    /// Create, organize and position segments in right order
    /// </summary>
    /// <param name="lastExitPoint"></param>
    /// <param name="roadShape"></param>
    /// <returns></returns>
    private List<RoadSegment> CreateSegments(EdgePoint lastExitPoint, out RoadShape roadShape)
    {
        EdgePoint entryPoint = CreateEntry(lastExitPoint);
        EdgePoint exitPoint = CreateExit(entryPoint);

        //Create random points between entry and exit
        int nPoints = GetPointAmount(entryPoint, exitPoint);
        List<RoadSegment> unOrganized = CreatePointsbetweenEntryStart(entryPoint, exitPoint, nPoints);
        lastEdgePoint = exitPoint;

        roadShape = GetRoadShape(entryPoint, exitPoint);
        List<RoadSegment> organized = OrganizeSegments(unOrganized, entryPoint, exitPoint);
        PositionSegments(organized);
        return organized;
    }

    private void HandleMeshTasks(MeshtaskSettings settings, RoadChain roadchain)
    {
        foreach (MeshTask task in meshtasks)
            if(settings == task.meshtaskSettings)
                if(task.positionPoints.Count > 3)
                    ExecuteMeshtask(settings, roadchain, task);
    }

    private void ExecuteMeshtask(MeshtaskSettings settings, RoadChain roadchain, MeshTask task)
    {
        if (settings.meshtaskPosition == MeshtaskPosition.Both)
        {
            task.meshPosition = MeshtaskPosition.Right;
            meshtaskExtruder.Extrude(task, roadchain, settings);
            task.meshPosition = MeshtaskPosition.Left;
            meshtaskExtruder.Extrude(task, roadchain, settings);
        }
        else
        {
            meshtaskExtruder.Extrude(task, roadchain, settings);
        }
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
        foreach (RoadSegment item in segments)
        {
            bool violation = Vector3.Distance(item.transform.position, proximity) < settings.segmentDeletionProximity;
            if (violation)
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
        RoadSegment lastSegment = null;
        foreach (RoadSegment currentSegment in segments)
        {
            if (lastSegment != null)
            {
                float baseHeight = fromZero ? 0f : lastSegment.transform.position.y;
                float calculatedHeight = baseHeight + Random.Range(-heightRange, heightRange);
                Vector3 position = currentSegment.transform.position + (Vector3.up * calculatedHeight);
                currentSegment.transform.position = position;
            }
            lastSegment = currentSegment;
        }
    }

    /// <summary>
    /// Add random to X-Axis local positions.
    /// Does not add to First and last Segment.
    /// </summary>
    /// <param name="segments"></param>
    private void SetRandomXaxisToSegments(List<RoadSegment> segments, float range)
    {
        for (int i = 1; i < segments.Count - 1; i++)
        {
            if (createdRoadChains.Count == 1 && i == 1)
                continue;

            Vector3 pos = segments[i].transform.localPosition;
            Vector3 random = new Vector3(Random.Range(-range, range), 0f, 0f);
            segments[i].transform.position = segments[i].transform.TransformPoint(random);
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

        [Space]
        [Tooltip("Amount of points on the side of a RoadchainBlock")]
        public int sidePointAmount;
        public int gridSize;//Unity UnitSize

        public int straight_NpointsBetween;
        public int corner_NpointsBetween;

        [Header("New segment settings")]
        [Range(0f, 75f)]
        public float XaxisVariation;

        [Range(0f, 25f)]
        public float segmentHeightRange;
        public bool isFixedBetweenRange;

        [Range(10, 50)]
        public int segmentDeletionProximity;
    }

    private void OnDestroy()
    {
        EventTriggerManager.roadChainTrigger -= CreateNextRoadChain;
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

public class ExtrusionVariables
{
    private float lerpSpeed;
    public float mainExtrusion { get { return MainExtrusion; } set { UpdateDelay(value); } }
    private float MainExtrusion = 0;
    private float velocity;

    public float leftExtrusion = 0;
    private float maxLeftExtrusion;
    private float leftReductionVelocity;

    public float rightExtrusion = 0;
    private float maxRightExtrusion;
    private float righReductiontVelocity;

    public float cornerRadius;

    public float averageExtrusion { get { return (rightExtrusion + leftExtrusion) / 2; } }

    public ExtrusionVariables(float lerpSpeed)
    {
        this.lerpSpeed = lerpSpeed;
    }

    public void UpdateDelay(float extrusion)
    {
        velocity = Mathf.Lerp(velocity, extrusion, .1f);
        MainExtrusion = Mathf.Lerp(MainExtrusion, velocity, lerpSpeed);
        //Left is always negative
        if (float.IsNegative(velocity) && MainExtrusion <= leftExtrusion)
        {
            leftExtrusion = MainExtrusion;
            maxLeftExtrusion = MainExtrusion;
            leftReductionVelocity = 0f;
            if(Mathf.Abs(leftExtrusion) > .05f)
                maxRightExtrusion = 0;
        }
        else if(maxLeftExtrusion == 0 || extrusion == 0)
        {
            leftExtrusion = Mathf.Lerp(leftExtrusion, 0f, leftReductionVelocity);
            leftReductionVelocity += .0001f;
        }

        if (!float.IsNegative(velocity) && MainExtrusion >= rightExtrusion)
        {
            rightExtrusion = MainExtrusion;
            maxRightExtrusion = MainExtrusion;
            righReductiontVelocity = 0f;
            if (Mathf.Abs(rightExtrusion) > .05f)
                maxLeftExtrusion = 0;
        }
        else if (maxRightExtrusion == 0 || extrusion == 0)
        {
            rightExtrusion = Mathf.Lerp(rightExtrusion, 0f, righReductiontVelocity);
            righReductiontVelocity += .0001f;
        }
    }
}

public struct ExtusionVariablesStruct
{
    public float mainExtrusion;
    public float leftExtrusion;
    public float rightExtrusion;
    public float averageExtrusion;

    public ExtusionVariablesStruct(ExtrusionVariables source)
    {
        mainExtrusion = source.mainExtrusion;
        leftExtrusion = source.leftExtrusion;
        rightExtrusion = source.rightExtrusion;
        averageExtrusion = source.averageExtrusion;
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

public class MeshtaskTypeHandler
{
    private Dictionary<string, MeshTask> activeMeshtasks = new Dictionary<string, MeshTask>();
    private Dictionary<string, Vector3> lastMeshtaskPositions = new Dictionary<string, Vector3>();

    public void SetDictionary(MeshtaskSettings meshtaskSettings, MeshTask task)
    {
        int dataKey = Random.Range(0, 1000000);
        meshtaskSettings.dataKey = dataKey;
        activeMeshtasks.Add((meshtaskSettings.meshTaskType + " " + meshtaskSettings.meshtaskPosition + " " + meshtaskSettings.dataKey), task);
        lastMeshtaskPositions.Add((meshtaskSettings.meshTaskType + " " + meshtaskSettings.meshtaskPosition + " " + meshtaskSettings.dataKey), Vector3.one * 1000f);
    }

    public void SetMeshtask(MeshTask newMeshtask, MeshtaskSettings meshtaskSettings)
    {
        activeMeshtasks[(meshtaskSettings.meshTaskType + " " + meshtaskSettings.meshtaskPosition + " " + meshtaskSettings.dataKey)] = newMeshtask;
    }

    public void SetLastMeshtaskPosition(MeshtaskSettings meshtaskSettings, Vector3 position)
    {
        lastMeshtaskPositions[(meshtaskSettings.meshTaskType + " " + meshtaskSettings.meshtaskPosition + " " + meshtaskSettings.dataKey)] = position;
    }

    public MeshTask GetMeshtask(MeshtaskSettings meshtaskSettings)
    {
        return activeMeshtasks[(meshtaskSettings.meshTaskType + " " + meshtaskSettings.meshtaskPosition + " " + meshtaskSettings.dataKey)];
    }

    public Vector3 GetLastMeshtaskPosition(MeshtaskSettings meshtaskSettings)
    {
        return lastMeshtaskPositions[(meshtaskSettings.meshTaskType + " " + meshtaskSettings.meshtaskPosition + " " + meshtaskSettings.dataKey)];
    }
}
