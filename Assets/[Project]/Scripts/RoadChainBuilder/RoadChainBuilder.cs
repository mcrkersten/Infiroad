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
    public GameObject roadChainPrefab;

    [Header("")]
    public Queue<RoadChain> createdRoadChains = new Queue<RoadChain>();
    public Queue<RoadSegment> populatedSegments = new Queue<RoadSegment>();

    public Queue<RoadChain> fixedRoadChains = new Queue<RoadChain>();
    private RoadChain lastFixedRoadChain;

    private EdgePoint lastEdgePoint;
    public GameObject startSegment;

    private RoadChain currentRoadChain;

    //Mesh task variables
    [HideInInspector] public MeshtaskTypeHandler meshtaskTypeHandler;


    [HideInInspector] public RoadFormVariables roadFormVariables;
    [HideInInspector] public Vector3 lastMeshPosition = Vector3.positiveInfinity;
    [HideInInspector] public List<MeshTask> meshtasks = new List<MeshTask>();

    MeshtaskExtruder meshtaskExtruder = new MeshtaskExtruder();

    public Road road;
    private ObjectPooler objectPooler;

    [Header("Debug")]
    public int generatedRoadEdgeloops = 0;

    public List<MeshtaskSettings> meshtaskSettings = new List<MeshtaskSettings>();

    private void Awake()
    {
        instance = this;
        PositionStartSegment();
    }

    private void InitializeSinglePoolGenerator()
    {
        meshtaskTypeHandler = new MeshtaskTypeHandler();
        foreach (MeshtaskSettings item in meshtaskSettings)
            meshtaskTypeHandler.SetDictionary(item, null);
        UpdateAllRoadSettings();
        roadFormVariables = new RoadFormVariables(.05f);
        InstantiateAssetPools();
    }

    private void CreateMeshtaskDictionary()
    {
        meshtaskTypeHandler = new MeshtaskTypeHandler();
        foreach (MeshtaskSettings item in meshtaskSettings)
            meshtaskTypeHandler.SetDictionary(item, null);
        UpdateAllRoadSettings();
        roadFormVariables = new RoadFormVariables(.05f);
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
                StartFixedRoadChain(gameModeManager.fixedSectors);
                break;
        }

        if (vehicle != null)
            SetVehicleStartPosition();
    }

    public void CreateNextFixedSector_Trigger()
    {
        RoadChain next = SelectAndPositionNextFixedRoadChain();
        currentRoadChain = next;
        CreateFullSector(next.organizedSegments, RoadShape.straights);
        SpawnAllDecorationOnChain(next.organizedSegments);
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

    public Sector GenerateTimingSector()
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
        InitializeSinglePoolGenerator();

        CreateNextRandomRoadChain(new EdgePoint(EdgeLocation.none, 3, startSegment));
        EventTriggerManager.roadChainTrigger += (GameObject trigger) => { 
            currentRoadChain.activatedPooledObjects.Remove(trigger);
            CreateNextRandomRoadChain(lastEdgePoint);
            if (createdRoadChains.Count == 4)
                DeleteRoadChain();
        };

        
        EventTriggerManager.segmentTrigger += (GameObject trigger) => {
            currentRoadChain.activatedPooledObjects.Remove(trigger);
            InstigateSegment();
        };

        InstigateSegment();
        InstigateSegment();
        InstigateSegment();
        SpawnStartDecoration(currentRoadChain);
    }

    private void InstigateSegment()
    {
        if(currentRoadChain.SegmentIndex == 6) return;
        if(currentRoadChain.SegmentIndex == 4)
            SpawnRoadChainTrigger(currentRoadChain, currentRoadChain.organizedSegments[3]);
        PopulateSegment();
        DestroySegementFromQueue();
    }

    private void PopulateSegment()
    {
        RoadSegment segment = currentRoadChain.organizedSegments[currentRoadChain.SegmentIndex];
        populatedSegments.Enqueue(segment);

        CreateSegmentMesh(segment);
        SpawnRandomDecoration(currentRoadChain, segment);
        SpawnSegmentTrigger(currentRoadChain, segment);
        currentRoadChain.SegmentIndex++;
    }

    private void DestroySegementFromQueue()
    {
        //Check if queue is of minimum size
        if (populatedSegments.Count <= 4)
            return;

        RoadSegment segment = populatedSegments.Dequeue();
        Destroy(segment.gameObject);
    }

    private void StartFixedRoadChain(List<Sector> sectors)
    {
        CreateMeshtaskDictionary();
        int i = 0;
        foreach (Sector s in sectors)
        {
            s.roadChain.SegmentIndex = i++;
            fixedRoadChains.Enqueue(s.roadChain);
        }
        InstantiateAssetPools();
        Debug.Log(fixedRoadChains.Count);
        //EventTriggerManager.roadChainTrigger += CreateNextFixedSector_Trigger;

        //Build first two sectors
        CreateNextFixedSector_Trigger();
        CreateNextFixedSector_Trigger();
    }

    /// <summary>
    /// Creates a full list of all materials that can spawn on the road.
    /// </summary>
    private void UpdateAllRoadSettings()
    {
        foreach (RoadSettings variation in road.roadSettings)
            variation.InitializeRoadSettings();
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
        objectPooler.InstantiateAssetTriggersPool(road.assetSpawnPointPoolSize, road.assetSpawnPoint);
        objectPooler.InstantiateAirDecoration(road.skyDecoration);
        InstantiateVegetationPool();
        InstantiateRoadDecorationPools();
        InstantiateMeshtaskPools();
    }

    private void InstantiateMeshtaskPools()
    {
            foreach (RoadSettings v in road.roadSettings)
                foreach (MeshtaskObject mto in v.meshtaskObjects)
                    objectPooler.InstantiateMeshtaskObjects(mto.meshtaskSettings);
    }

    private void InstantiateVegetationPool()
    {
        foreach (RoadSettings s in road.roadSettings)
            objectPooler.InstantiateAssetPool(s.assetPools, s.roadTypeTag);
    }

    private void InstantiateRoadDecorationPools()
    {
        foreach (RoadDecoration pool in road.standardDecoration)
            objectPooler.InstantiateRoadDecorationDecorationPool(pool);
        foreach (RoadDecoration pool in road.randomizedDecoration)
            objectPooler.InstantiateRoadDecorationDecorationPool(pool);
    }
    #endregion

    int Rindex = 0;
    private List<RoadSegment> CreateNextRandomRoadChain(EdgePoint lastExitPoint, bool decoration = true)
    {
        //Instantiate
        RoadChain roadChain = InstantiateRoadChain();
        currentRoadChain = roadChain;

        if(Rindex > road.roadSettings[0].SurfaceSettingsCount - 1)
            Rindex = 0;
        roadChain.ChainIndex = Rindex;
        Rindex++;

        Vector3 position = CalculateRoadChainObjectPosition(lastExitPoint);
        roadChain.transform.position = position;
        this.transform.position = position;

        List<RoadSegment> organized = CreateSegments(lastExitPoint, out RoadShape roadShape);
        currentRoadChain.SetOrganizedSegments(organized);
        createdRoadChains.Enqueue(currentRoadChain);

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

    private void CreateFullSector(List<RoadSegment> segments, RoadShape roadShape)
    {
        currentRoadChain.SetOrganizedSegments(segments);
        foreach (RoadSegment segment in segments)
        {
            RoadSettings roadSettting = road.roadSettings[0];
            currentRoadChain.CreateSegmentMesh(roadSettting, segment);

            //Handle created Meshtasks
            foreach (MeshtaskObject meshtaskObject in roadSettting.meshtaskObjects)
                ExecuteMeshtasks(meshtaskObject.meshtaskSettings, currentRoadChain);

            meshtasks.Clear();
        }
    }

    private void CreateSegmentMesh(RoadSegment segment)
    {
        RoadSettings roadSettting = road.roadSettings[0];
        currentRoadChain.CreateSegmentMesh(roadSettting, segment);

        //Handle created Meshtasks
        foreach (MeshtaskObject meshtaskObject in roadSettting.meshtaskObjects)
            ExecuteMeshtasks(meshtaskObject.meshtaskSettings, currentRoadChain);

        meshtasks.Clear();
    }

    private void SpawnAllDecorationOnChain(List<RoadSegment> segments)
    {
        //SpawnSegmentTrigger(segments[segment]);

        int randomIndex = Random.Range(1, segments.Count - 1);
        SpawnRandomDecoration(currentRoadChain, segments[randomIndex]);

        SpawnSkyDecoration();
    }

    #region SPAWN DECORATION
    private void SpawnRandomDecoration(RoadChain roadChain, RoadSegment segment)
    {
        if(road.randomizedDecoration.Count != 0)
        {
            RoadDecoration deco = road.randomizedDecoration[Random.Range(0, road.randomizedDecoration.Count)];
            roadChain.ActivateDecor(segment, deco);
        }
    }

    private void SpawnStartDecoration(RoadChain roadChain)
    {
        //Create on first segment a startline
        RoadDecoration deco = road.standardDecoration.First(t => t.poolIndex == 0);
        roadChain.ActivateDecor(roadChain.organizedSegments[0], deco);
    }

    private void SpawnRoadChainTrigger(RoadChain roadChain, RoadSegment segment)
    {
        //Create on last segment a checkpoint
        RoadDecoration deco = road.standardDecoration.First(t => t.poolIndex == 1);
        roadChain.ActivateDecor(segment, deco);
    }

    private void SpawnSegmentTrigger(RoadChain roadChain, RoadSegment segment)
    {
        //Create trigger on each segment
        if (!segment.isExitSegment)
        {
            RoadDecoration deco = road.standardDecoration.First(t => t.poolIndex == 2);
            roadChain.ActivateDecor(segment,  deco);
        }
    }

    private void SpawnRandomSceneryObjects(RoadChain roadChain, RoadSegment segment, int segmentIndex)
    {
        RoadDecoration deco = road.sceneryObjects[Random.Range(0, road.sceneryObjects.Count)];
        roadChain.ActivateDecor(segment, deco);
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
    #endregion SPA
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

    private void ExecuteMeshtasks(MeshtaskSettings settings, RoadChain roadchain)
    {
        foreach (MeshTask task in meshtasks)
            if(settings == task.meshtaskObject.meshtaskSettings)
                if(task.positionVectors.Count > 2)
                    meshtaskExtruder.Extrude(task, roadchain);
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
        roadChainObject.name = "RoadChain";
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
        segment.GetComponent<RoadSegment>().isExitSegment = true;
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

    private void DeleteRoadChain()
    {
        RoadChain rc = createdRoadChains.Dequeue();
        Destroy(rc.gameObject);
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

public class RoadFormVariables
{
    private float lerpSpeed;
    public float mainExtrusion { get { return MainExtrusion; } set { UpdateDelay(value); } }
    public float cornerCamber { get { return CornerCamber; } set { UpdateCornerCamber(value); } }

    private float MainExtrusion = 0;
    private float CornerCamber = 0;
    private float extrusionVelocity;
    private float camberVelocity;

    public float leftExtrusion = 0;
    private float maxLeftExtrusion;
    private float leftReductionVelocity;

    public float rightExtrusion = 0;
    private float maxRightExtrusion;
    private float righReductiontVelocity;

    public float cornerRadius;

    public RoadFormVariables(float lerpSpeed)
    {
        this.lerpSpeed = lerpSpeed;
    }

public void UpdateDelay(float extrusion)
{
    // Smoothly adjust the extrusion velocity using linear interpolation (Lerp)
    extrusionVelocity = Mathf.Lerp(extrusionVelocity, extrusion, lerpSpeed);
    MainExtrusion = Mathf.Lerp(MainExtrusion, extrusionVelocity, lerpSpeed);

    // Update the left and right extrusions based on the new MainExtrusion value
    if (extrusionVelocity < 0f && MainExtrusion <= leftExtrusion)
    {
        // If the extrusion velocity is negative and MainExtrusion is less than or equal to the left extrusion,
        // set the left extrusion to MainExtrusion and reset the left reduction velocity to zero. 
        leftExtrusion = MainExtrusion;
        leftReductionVelocity = 0f;

        // If the absolute value of leftExtrusion is greater than 0.05f, set maxRightExtrusion to zero
        if (Mathf.Abs(leftExtrusion) > 0.05f)
        {
            maxRightExtrusion = 0f;
        }
    }
    else if ((maxLeftExtrusion == 0f || extrusion == 0f) && leftExtrusion != 0f)
    {
        // If maxLeftExtrusion is zero or extrusion is zero and leftExtrusion is not already zero,
        // smoothly reduce leftExtrusion towards zero using leftReductionVelocity and increment leftReductionVelocity by 0.0001f.
        leftExtrusion = Mathf.Lerp(leftExtrusion, 0f, leftReductionVelocity);
        leftReductionVelocity += 0.001f;
    }

    if (extrusionVelocity > 0f && MainExtrusion >= rightExtrusion)
    {
        // If the extrusion velocity is positive and MainExtrusion is greater than or equal to the right extrusion,
        // set the right extrusion to MainExtrusion and reset the right reduction velocity to zero. 
        rightExtrusion = MainExtrusion;
        righReductiontVelocity = 0f;

        // If the absolute value of rightExtrusion is greater than 0.05f, set maxLeftExtrusion to zero
        if (Mathf.Abs(rightExtrusion) > 0.05f)
        {
            maxLeftExtrusion = 0f;
        }
    }
    else if ((maxRightExtrusion == 0f || extrusion == 0f) && rightExtrusion != 0f)
    {
        // If maxRightExtrusion is zero or extrusion is zero and rightExtrusion is not already zero,
        // smoothly reduce rightExtrusion towards zero using righReductiontVelocity and increment righReductiontVelocity by 0.0001f.
        rightExtrusion = Mathf.Lerp(rightExtrusion, 0f, righReductiontVelocity);
        righReductiontVelocity += 0.001f;
    }
}

    private void UpdateCornerCamber(float extrusion)
    {
        camberVelocity = Mathf.Lerp(camberVelocity, extrusion, .1f);
        CornerCamber = ((rightExtrusion + leftExtrusion) / 2f) * Mathf.Abs(camberVelocity);
    }
}

public struct ExtusionVariablesStruct
{
    public float mainExtrusion;
    public float leftExtrusion;
    public float rightExtrusion;
    public float cornerCamber;

    public ExtusionVariablesStruct(RoadFormVariables source)
    {
        mainExtrusion = source.mainExtrusion;
        leftExtrusion = source.leftExtrusion;
        rightExtrusion = source.rightExtrusion;
        cornerCamber = source.cornerCamber;
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
    private Dictionary<string, Vector3> MeshtaskVectors = new Dictionary<string, Vector3>();

    public void SetDictionary(MeshtaskSettings meshtaskSettings, MeshTask task)
    {
        int dataKey = Random.Range(0, 1000000);
        meshtaskSettings.dataKey = dataKey;
        activeMeshtasks.Add((meshtaskSettings.meshTaskType + " " + meshtaskSettings.meshtaskPosition + " " + meshtaskSettings.dataKey), task);
        if(meshtaskSettings.meshtaskContinues) return;
        MeshtaskVectors.Add((meshtaskSettings.meshTaskType + " " + meshtaskSettings.meshtaskPosition + " " + meshtaskSettings.dataKey), Vector3.zero);
    }

    public void SetMeshtask(MeshTask newMeshtask, MeshtaskSettings meshtaskSettings)
    {
        activeMeshtasks[(meshtaskSettings.meshTaskType + " " + meshtaskSettings.meshtaskPosition + " " + meshtaskSettings.dataKey)] = newMeshtask;
    }

    public void SetMeshtaskVector(MeshtaskSettings meshtaskSettings, Vector3 position)
    {
        MeshtaskVectors[(meshtaskSettings.meshTaskType + " " + meshtaskSettings.meshtaskPosition + " " + meshtaskSettings.dataKey)] = position;
    }

    public MeshTask GetMeshtask(MeshtaskSettings meshtaskSettings)
    {
        return activeMeshtasks[(meshtaskSettings.meshTaskType + " " + meshtaskSettings.meshtaskPosition + " " + meshtaskSettings.dataKey)];
    }

    public Vector3 GetMeshtaskVector(MeshtaskSettings meshtaskSettings)
    {
        return MeshtaskVectors[(meshtaskSettings.meshTaskType + " " + meshtaskSettings.meshtaskPosition + " " + meshtaskSettings.dataKey)];
    }
}
