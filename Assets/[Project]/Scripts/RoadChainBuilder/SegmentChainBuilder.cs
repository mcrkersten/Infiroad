using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.Rendering.HableCurve;

public class SegmentChainBuilder : MonoBehaviour
{
    public GameObject vehicle;
    public GameObject startSegment;
    public MinimapBehaviour minimap;
    public WorldBuilder worldBuilder;

    [Space]public Road road;
    public static SegmentChainBuilder instance;
    [HideInInspector] public Transform vehicleStartTransform;

    private Func< bool ,RoadSettings> GetChainSettings;
    private int chainIndex = 0;

    //Needs to be atleast 1
    static int segmentsToGenerateOnStart = 8;

    [Header("")]
    public GameObject segmentPrefab;
    public GameObject segmentChainPrefab;

    [Header("")]
    public Queue<SegmentChain> createdSegmentChains = new Queue<SegmentChain>();
    public Queue<RoadSegment> populatedSegments = new Queue<RoadSegment>();

    public Queue<SegmentChain> fixedSegmentChains = new Queue<SegmentChain>();
    private SegmentChain lastFixedRoadChain;

    private EdgePoint lastEdgePoint;

    private SegmentChain currentSegmentChain;

    //Mesh task variables
    [HideInInspector] public MeshtaskTypeHandler meshtaskTypeHandler;

    [HideInInspector] public RoadFormVariables roadFormVariables;
    [HideInInspector] public Vector3 lastMeshPosition = Vector3.positiveInfinity;
    [HideInInspector] public List<MeshTask> meshtasks = new List<MeshTask>();

    MeshtaskExtruder meshtaskExtruder = new MeshtaskExtruder();

    private ObjectPooler objectPooler;

    [Header("Debug")]
    public int generatedRoadEdgeloops = 0;

    public List<MeshtaskSettings> meshtaskSettings = new List<MeshtaskSettings>();

    [Header("Track Telemetry")]
    [SerializeField] private bool logTrackMetrics = true;
    [SerializeField, Min(1f)] private float cornerRadiusThresholdMeters = 120f;
    [SerializeField, Range(2, 64)] private int metricSamplesPerSegment = 40;
    [SerializeField] private bool logAggregateMetrics = true;

    private readonly TrackMetricsAccumulator trackMetricsAccumulator = new TrackMetricsAccumulator();
    private int generatedChainTelemetryIndex = 0;


    private void Awake()
    {
        instance = this;
        CreateFunc();
        PositionStartSegment();
    }

    private void OnDestroy()
    {
        instance = null;
        EventTriggerManager.roadChainTrigger -= OnRoadChainTriggerEvent;      
        EventTriggerManager.segmentTrigger -= OnRandomSegmentTriggerEvent;
        EventTriggerManager.roadChainTrigger -= OnRoadTriggerEvent;
        EventTriggerManager.segmentTrigger -= OnSegmentTriggerEvent;
    }

    private void CreateFunc()
    {
        GetChainSettings = (@bool) =>
        {
            if (chainIndex >= 0 && chainIndex < road.roadSettings.Count)
            {
                if(@bool)
                    CalculateNextChain();
                return road.roadSettings[chainIndex];
            }
            else
            {
                Debug.LogError("Invalid index!");
                return null;
            }
        };
    }

    private void CalculateNextChain()
    {
        chainIndex = UnityEngine.Random.Range(0, road.roadSettings.Count);
    }

    public static int GetActiveSidePointAmountOrDefault()
    {
        const int fallback = 5;
        if (instance == null || instance.road == null || instance.road.roadSettings == null || instance.road.roadSettings.Count == 0)
            return fallback;

        int safeIndex = Mathf.Clamp(instance.chainIndex, 0, instance.road.roadSettings.Count - 1);
        RoadSettings settings = instance.road.roadSettings[safeIndex];
        if (settings == null || settings.segmentChainSettings == null)
            return fallback;

        return Mathf.Max(2, settings.segmentChainSettings.sidePointAmount);
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
        createdSegmentChains.Clear();
        trackMetricsAccumulator.Reset();
        generatedChainTelemetryIndex = 0;
        switch (gameModeManager.gameMode)
        {
            case GameMode.Relaxed:
                StartRandomChain();
                break;
            case GameMode.TimeTrial:
                StartRandomChain();
                break;
            case GameMode.RandomSectors:
                StartRandomChain();
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
        if(currentSegmentChain != null)
            currentSegmentChain.SegmentIndex = 0;

        SegmentChain next = SelectAndPositionNextFixedRoadChain();
        next.SetOrganizedSegments(next.organizedSegments);
        currentSegmentChain = next;
    }

    //Menu interaction
    public Sector GenerateTimingSector()
    {
        List<RoadSegment> segments_0 = CreateNextRandomSegmentChain(new EdgePoint(startSegment));
        SegmentChain chain = segments_0[0].transform.root.GetComponent<SegmentChain>();
        chain.organizedSegments = segments_0;
        return new Sector(chain);
    }

    private void PositionStartSegment()
    {
        startSegment.transform.position = new Vector3(0, 0, -GetChainSettings(false).segmentChainSettings.gridSize / 2f);
    }

    private void StartRandomChain()
    {
        InitializeSinglePoolGenerator();

        CreateNextRandomSegmentChain(new EdgePoint(startSegment));
        worldBuilder.PopulateWorld(GetChainSettings(false).segmentChainSettings, currentSegmentChain);

        EventTriggerManager.roadChainTrigger += OnRoadChainTriggerEvent;      
        EventTriggerManager.segmentTrigger += OnRandomSegmentTriggerEvent;

        for (int i = 0; i < segmentsToGenerateOnStart; i++)
            InstigateRandomizedSegment();

        SpawnStartDecoration(currentSegmentChain);
    }

    private void OnRoadChainTriggerEvent(GameObject trigger)
    {
        currentSegmentChain.activatedPooledObjects.Remove(trigger);
        CreateNextRandomSegmentChain(lastEdgePoint);
        worldBuilder.PopulateWorld(GetChainSettings(false).segmentChainSettings, currentSegmentChain);
        //worldBuilder.CreateLineCollider(currentSegmentChain);

        //This is for now clean way of disposing chunks.
        if (createdSegmentChains.Count == 3)
            DeleteSegmentChain();
    }

    private void OnRandomSegmentTriggerEvent(GameObject trigger)
    {
        currentSegmentChain.activatedPooledObjects.Remove(trigger);
        InstigateRandomizedSegment();
    }

    private void InstigateRandomizedSegment()
    {
        if (currentSegmentChain == null || currentSegmentChain.organizedSegments == null)
            return;
        if (currentSegmentChain.SegmentIndex >= currentSegmentChain.organizedSegments.Count)
            return;

        //Create segment
        PopulateSegment();
        DestroySegementFromQueue();
    }

    private void PopulateSegment()
    {
        RoadSegment segment = currentSegmentChain.organizedSegments[currentSegmentChain.SegmentIndex];
        CreateSegmentMesh(segment);
        SpawnRandomDecoration(currentSegmentChain, segment);
        SpawnSegmentTrigger(currentSegmentChain, segment);
        if (segment.isExitSegment)
            SpawnSegmentChainTrigger(currentSegmentChain, segment);
        minimap.GenerateMinimapRoadSegment(currentSegmentChain, segment);

        populatedSegments.Enqueue(segment);
        currentSegmentChain.SegmentIndex++;
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

        foreach (Sector s in sectors)
            fixedSegmentChains.Enqueue(s.segmentChain);
        InstantiateAssetPools();

        //EventTriggerManager.roadChainTrigger += OnRoadTriggerEvent;
        CreateNextFixedSector_Trigger();

        EventTriggerManager.segmentTrigger += OnSegmentTriggerEvent;
        for (int i = 0; i < segmentsToGenerateOnStart; i++)
            InstigateSegment(currentSegmentChain.SegmentIndex++);
        //Build first sector
    }

    private void OnRoadTriggerEvent(GameObject trigger)
    {
        currentSegmentChain.activatedPooledObjects.Remove(trigger);
        CreateNextFixedSector_Trigger();
    }

    private void OnSegmentTriggerEvent(GameObject trigger)
    {
        InstigateSegment(currentSegmentChain.SegmentIndex++);
        if(currentSegmentChain.SegmentIndex == currentSegmentChain.organizedSegments.Count - 1)
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

    [Button]
    public void UpdateRoadSettingManually()
    {
        UpdateAllRoadSettings();
    }

    private void SetVehicleStartPosition()
    {
        vehicle.transform.position = vehicleStartTransform.position;
        vehicle.transform.rotation = vehicleStartTransform.rotation;
    }

    #region InstantiatePools
    private void InstantiateAssetPools()
    {
        //Debug.Log("Uhh");
        if(objectPooler != null)
            objectPooler.OnDestroy();

        objectPooler = new ObjectPooler();
        objectPooler.InstantiateAssetTriggersPool(road.assetSpawnPointPoolSize, road.assetSpawnPoint, true);
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
            objectPooler.InstantiateAssetPool(s.assetPools, s.roadTypeTag, true);
    }

    private void InstantiateRoadDecorationPools()
    {
        foreach (RoadDecoration pool in road.standardDecoration)
            objectPooler.InstantiateRoadDecorationDecorationPool(pool);
        foreach (RoadDecoration pool in road.randomizedDecoration)
            objectPooler.InstantiateRoadDecorationDecorationPool(pool);
    }
    #endregion

    private List<RoadSegment> CreateNextRandomSegmentChain(EdgePoint lastExitPoint)
    {
        //Instantiate
        SegmentChain newSegmentChain = InstantiateSegmentChain();
        GetChainSettings(true);
        currentSegmentChain = newSegmentChain;
        newSegmentChain.ChainIndex = chainIndex;

        Vector3 position = CalculateRoadChainObjectPosition(lastExitPoint);
        newSegmentChain.transform.position = position;
        this.transform.position = position;

        List<RoadSegment> organized = CreateSegments(lastExitPoint);
        currentSegmentChain.SetOrganizedSegments(organized);
        CaptureAndLogChainMetrics(currentSegmentChain, organized, "random");
        createdSegmentChains.Enqueue(currentSegmentChain);

        return organized;
    }


    private SegmentChain SelectAndPositionNextFixedRoadChain()
    {
        if (lastFixedRoadChain == null)
            return CreateFirstFixedSegmentChain();
     
        SegmentChain nextChain = fixedSegmentChains.Dequeue();
        chainIndex = nextChain.ChainIndex;
        nextChain.gameObject.SetActive(true);
        Transform lastSegmentTransform = lastFixedRoadChain.organizedSegments[lastFixedRoadChain.organizedSegments.Count - 1].transform;

        //Clean up the last segment chain
        List<Transform> toDestroy = new List<Transform>();
        foreach (Transform segment in nextChain.transform)
            if (segment.CompareTag("Segment"))
            {
                segment.GetComponent<MeshFilter>().mesh = null;
                foreach (Transform child in segment.transform)
                    toDestroy.Add(child);
            }
        int count = toDestroy.Count;
        for (int i = 0; i < count; i++)
            Destroy(toDestroy[i].gameObject);


        // Create a temporary GameObject to assist with positioning
        GameObject temp = new GameObject("Temp");
        // Align the rotation of 'nextChain' with the 'lastSegmentTransform'
        nextChain.transform.rotation = lastSegmentTransform.rotation;
        // Set the temporary GameObject's position to the first segment in 'nextChain'
        temp.transform.position = nextChain.organizedSegments[0].transform.position;
        // Temporarily parent 'nextChain' to the temporary GameObject
        nextChain.transform.parent = temp.transform;
        // Move the temporary GameObject to the 'lastSegmentTransform' position
        temp.transform.position = lastSegmentTransform.position;
        // Unparent 'nextChain', leaving it in the new position
        nextChain.transform.parent = null;
        // Destroy the temporary GameObject as it's no longer needed
        Destroy(temp);


        fixedSegmentChains.Enqueue(lastFixedRoadChain);
        lastFixedRoadChain = nextChain;
        CaptureAndLogChainMetrics(nextChain, nextChain.organizedSegments, "fixed-swap");
        return nextChain;
    }

    private SegmentChain CreateFirstFixedSegmentChain()
    {
        SegmentChain next = fixedSegmentChains.Dequeue();
        chainIndex = next.ChainIndex;
        next.gameObject.SetActive(true);
        next.transform.rotation = Quaternion.identity;
        next.transform.position = Vector3.zero;
        lastFixedRoadChain = next;
        CaptureAndLogChainMetrics(next, next.organizedSegments, "fixed-start");
        return next;
    }

    private void CaptureAndLogChainMetrics(SegmentChain chain, List<RoadSegment> segments, string source)
    {
        if (!logTrackMetrics || chain == null || segments == null || segments.Count < 3)
            return;

        TrackChainMetrics metrics = TrackMetricsUtility.Evaluate(segments, cornerRadiusThresholdMeters, metricSamplesPerSegment);
        trackMetricsAccumulator.Add(metrics);
        generatedChainTelemetryIndex++;

        string roadSettingName = "Unknown";
        if (road != null && chain.ChainIndex >= 0 && chain.ChainIndex < road.roadSettings.Count && road.roadSettings[chain.ChainIndex] != null)
            roadSettingName = road.roadSettings[chain.ChainIndex].name;

        Debug.Log($"[TrackMetrics][Chain] source={source} telemetry_index={generatedChainTelemetryIndex} chain_index={chain.ChainIndex} road_settings=\"{roadSettingName}\" {metrics.ToLogString()}");

        if (logAggregateMetrics)
        {
            TrackMetricsSummary summary = trackMetricsAccumulator.BuildSummary();
            Debug.Log($"[TrackMetrics][Aggregate] {summary.ToLogString()}");
        }
    }

    private void PositionSegments(List<RoadSegment> segments)
    {
        SetRandomHeightToSegments(segments, GetChainSettings(false).segmentChainSettings.isFixedBetweenRange, GetChainSettings(false).segmentChainSettings.segmentHeightRange);
        OrientSegments(segments);
        SetRandomXaxisToSegments(segments, GetChainSettings(false).segmentChainSettings.segmentXaxisVariation);
        OrientSegments(segments); //Orient again to make nice
        SetTangentLenght(segments);
    }

    private void InstigateSegment(int segmentIndex)
    {
        Debug.Log(segmentIndex);
        RoadSegment currentSegment = currentSegmentChain.organizedSegments[segmentIndex];

        if(currentSegment == null) return;

        //Debug.Log(currentSegment.name + " = " + segmentIndex, currentSegment);
        if(segmentIndex > currentSegmentChain.organizedSegments.Count - 1) return;

        CreateSegmentMesh(currentSegment);

        //We need to spawn the next segment trigger before we spawn the decoration
        SpawnSegmentTrigger(currentSegmentChain, currentSegment);

        //If we are first segment in this chain, spawn start decoration
        if (segmentIndex == 0)
            SpawnStartDecoration(currentSegmentChain);
    }

    private void CreateSegmentMesh(RoadSegment segment)
    {
        //Debug.Log(chainIndex);
        RoadSettings roadSettting = road.roadSettings[chainIndex];
        currentSegmentChain.CreateSegmentMesh(roadSettting, segment);

        //Handle created Meshtasks
        foreach (MeshtaskObject meshtaskObject in roadSettting.meshtaskObjects)
            ExecuteMeshtasks(meshtaskObject.meshtaskSettings, currentSegmentChain);

        meshtasks.Clear();
    }

    #region SPAWN DECORATION
    private void SpawnRandomDecoration(SegmentChain roadChain, RoadSegment segment)
    {
        if(road.randomizedDecoration.Count != 0)
        {
            RoadDecoration deco = road.randomizedDecoration[UnityEngine.Random.Range(0, road.randomizedDecoration.Count)];
            roadChain.ActivateDecor(segment, deco);
        }
    }

    private void SpawnStartDecoration(SegmentChain roadChain)
    {
        //Create on first segment a startline
        RoadDecoration deco = road.standardDecoration.First(t => t.poolIndex == 0);
        roadChain.ActivateDecor(roadChain.organizedSegments[0], deco);
    }

    private void SpawnSegmentChainTrigger(SegmentChain roadChain, RoadSegment segment)
    {
        //Create on last segment a checkpoint
        RoadDecoration deco = road.standardDecoration.First(t => t.poolIndex == 1);
        roadChain.ActivateDecor(segment, deco);
    }

    private void SpawnTimerTrigger(SegmentChain roadChain, RoadSegment segment)
    {
        RoadDecoration deco = road.standardDecoration.First(t => t.poolIndex == 3);
        roadChain.ActivateDecor(segment, deco);
    }

    private void SpawnSegmentTrigger(SegmentChain roadChain, RoadSegment segment)
    {
        //Create trigger on each segment, including the exit segment.
        RoadDecoration deco = road.standardDecoration.First(t => t.poolIndex == 2);
        roadChain.ActivateDecor(segment,  deco);
    }
    private void SpawnSkyDecoration()
    {
        SkyDecoration skyDecoration = road.skyDecoration;
        for (int i = 0; i < skyDecoration.decorAmount; i++)
        {
            float probabilityRoll = UnityEngine.Random.Range(0f, 1f);
            foreach (SkyDecor item in skyDecoration.skyDecors)
            {
                if(item.probability > probabilityRoll)
                {
                    float x = UnityEngine.Random.Range(-item.spawnAreaSize.x / 2, item.spawnAreaSize.x / 2);
                    float y = skyDecoration.skyHeight + UnityEngine.Random.Range(-item.spawnAreaSize.y / 2, item.spawnAreaSize.y / 2);
                    float z = UnityEngine.Random.Range(-item.spawnAreaSize.z / 2, item.spawnAreaSize.z / 2);
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
private List<RoadSegment> CreateSegments(EdgePoint lastExitPoint)
    {
        bool shouldDeleteConsumedExitPoint = lastExitPoint.gameObject != null && lastExitPoint.gameObject.name == "ExitPoint";
        EdgePoint entryPoint = CreateEntry(lastExitPoint);
        EdgePoint exitPoint = CreateExit(entryPoint);

        //Create random points between entry and exit
        int nPoints = GetPointAmount(entryPoint, exitPoint);
        List<RoadSegment> segments = CreateSmoothTrack(entryPoint, exitPoint, nPoints);

        // Endpoint rotations are part of the curve definition for the final segment pair.
        OrientSegments(segments);
        AlignExitPointToTrack(exitPoint, segments);
        SetTangentLenght(segments);

        MarkExitSegment(segments);

        if (shouldDeleteConsumedExitPoint)
            Destroy(lastExitPoint.gameObject);

        lastEdgePoint = exitPoint;

        return segments;
    }

private void AlignExitPointToTrack(EdgePoint exitPoint, List<RoadSegment> segments)
    {
        if (exitPoint == null || exitPoint.gameObject == null || segments == null || segments.Count == 0)
            return;

        int lastIndex = segments.Count - 1;
        Vector3 lastPosition = segments[lastIndex].transform.position;
        Vector3 dynamicForward = segments[lastIndex].transform.forward;
        dynamicForward.y = 0f;
        float travelGrade = exitPoint.edgeTravelGrade;

        if (segments.Count >= 2)
        {
            Vector3 previousPosition = segments[lastIndex - 1].transform.position;
            Vector3 segmentDelta = lastPosition - previousPosition;
            Vector3 planarDelta = new Vector3(segmentDelta.x, 0f, segmentDelta.z);

            if (dynamicForward.sqrMagnitude < 0.0001f)
                dynamicForward = planarDelta;

            float horizontalDistance = planarDelta.magnitude;
            if (horizontalDistance > 0.0001f)
            {
                SegmentChainSettings settings = GetChainSettings(false).segmentChainSettings;
                float maxGrade = settings == null ? 0.06f : Mathf.Max(0f, settings.verticalMaxGradePercent) / 100f;
                travelGrade = Mathf.Clamp(segmentDelta.y / horizontalDistance, -maxGrade, maxGrade);
            }
        }

        if (dynamicForward.sqrMagnitude < 0.0001f)
            dynamicForward = GetEdgeOutwardDirection(exitPoint.edgeLocation);

        exitPoint.SetEdgeForward(dynamicForward);
        exitPoint.edgeTravelGrade = travelGrade;
        Vector3 snappedExitPosition = exitPoint.gameObject.transform.position;
        snappedExitPosition.y = lastPosition.y;
        exitPoint.gameObject.transform.position = snappedExitPosition;
        exitPoint.gameObject.transform.rotation = exitPoint.edgeRotation;
    }


    private static void MarkExitSegment(List<RoadSegment> segments)
    {
        // The last segment is a handoff/placeholder point, so exit is the segment before it, that is why 2 and not 1.
        const int exitOffsetFromEnd = 2;
        int exitIndex = segments.Count - exitOffsetFromEnd;

        for (int i = 0; i < segments.Count; i++)
            segments[i].isExitSegment = i == exitIndex;
    }

private List<RoadSegment> CreateSmoothTrack(EdgePoint entry, EdgePoint exit, int nOfPoints)
    {
        Vector3 entryPoint = entry.gameObject.transform.position;
        Vector3 exitPoint = exit.gameObject.transform.position;
        SegmentChainSettings chainSettings = GetChainSettings(false).segmentChainSettings;
        List<Vector3> controlPoints = new List<Vector3>
        {
            entryPoint // Start point
        };

        Vector3 direction = (exitPoint - entryPoint).normalized;
        Vector3 entryOutward = entry.edgeForward;
        Vector3 exitOutward = exit.edgeForward;
        if (entryOutward.sqrMagnitude < 0.0001f)
            entryOutward = GetEdgeOutwardDirection(entry.edgeLocation);
        if (exitOutward.sqrMagnitude < 0.0001f)
            exitOutward = GetEdgeOutwardDirection(exit.edgeLocation);

        Vector3 entryTangent = -entryOutward.normalized; // Inward from entry edge.
        Vector3 exitTangent = exitOutward.normalized;    // Outward toward exit edge.

        float chainDistance = Vector3.Distance(entryPoint, exitPoint);
        float tangentFraction = Mathf.Clamp(chainSettings.segmentBoundaryTangentFraction, 0f, 0.45f);
        float tangentMinDistance = Mathf.Max(0f, chainSettings.segmentBoundaryTangentMinDistance);
        float tangentDistance = Mathf.Clamp(
            chainDistance * tangentFraction,
            tangentMinDistance,
            chainDistance * 0.45f);

        // Prevent boundary anchors from overpowering short/sparse chains.
        float densityDistanceCap = chainDistance / Mathf.Max(3f, nOfPoints + 3f);
        tangentDistance = Mathf.Min(tangentDistance, densityDistanceCap);

        // If boundary tangents are poorly aligned with global chain direction, shorten them.
        float entryAlignment = Mathf.Clamp01(Vector3.Dot(entryTangent, direction));
        float exitAlignment = Mathf.Clamp01(Vector3.Dot(exitTangent, direction));
        float alignmentScale = Mathf.Lerp(0.35f, 1f, Mathf.Min(entryAlignment, exitAlignment));
        tangentDistance *= alignmentScale;

        bool useBoundaryAnchors = tangentDistance > 0.001f;
        Vector3 exitAnchor = Vector3.zero;
        if (useBoundaryAnchors)
        {
            Vector3 entryAnchor = entryPoint + entryTangent * tangentDistance;
            exitAnchor = exitPoint - exitTangent * tangentDistance;
            controlPoints.Add(entryAnchor);
        }

        Vector3 perpendicular = Vector3.Cross(direction, Vector3.up);
        if (perpendicular.sqrMagnitude < 0.0001f)
            perpendicular = Vector3.right;
        perpendicular.Normalize();

        float maxLateralOffset = chainSettings.segmentXaxisVariation * chainSettings.segmentBendStrength;
        float macroFrequency = Mathf.Max(0.1f, chainSettings.segmentBendFrequency);
        float microFrequency = macroFrequency * 3f;

        float seedX = Mathf.Abs(
            entryPoint.x * 0.137f +
            entryPoint.z * 0.193f +
            exitPoint.x * 0.271f +
            exitPoint.z * 0.311f +
            (chainIndex + 1) * 1.618f);
        float seedY = seedX + 37.17f;

        // Optional per-chain jitter to avoid recurring template-like duplicates.
        float phaseJitterRange = Mathf.Max(0f, chainSettings.segmentNoisePhaseJitter);
        float phaseJitter = UnityEngine.Random.Range(-phaseJitterRange, phaseJitterRange);
        float bendBiasJitter = Mathf.Clamp(chainSettings.segmentBendBiasJitter, 0f, 0.4f);
        float effectiveBendBias = Mathf.Clamp01(
            chainSettings.segmentBendBias +
            UnityEngine.Random.Range(-bendBiasJitter, bendBiasJitter));

        float noiseSeedX = seedX + phaseJitter;
        float noiseSeedY = seedY + (phaseJitter * 0.73f);
        float bendBias = Mathf.PerlinNoise(noiseSeedX, 0.23f) * 2f - 1f;

        float endpointStraightFraction = Mathf.Clamp(chainSettings.segmentEndpointStraightFraction, 0f, 0.45f);
        float endpointEasePower = Mathf.Max(1f, chainSettings.segmentEndpointEasePower);

        for (int i = 1; i <= nOfPoints; i++)
        {
            float t = (float)i / (nOfPoints + 1);
            Vector3 segmentPosition = Vector3.Lerp(entryPoint, exitPoint, t);

            // Keep a straighter run-in/run-out and delay bend buildup near both ends.
            float endpointT = Mathf.InverseLerp(endpointStraightFraction, 1f - endpointStraightFraction, t);
            endpointT = Mathf.Clamp01(endpointT);
            float endpointEnvelope = Mathf.Pow(Mathf.Sin(endpointT * Mathf.PI), endpointEasePower);

            float macroNoise = Mathf.PerlinNoise(noiseSeedX + t * macroFrequency, noiseSeedY) * 2f - 1f;
            float microNoise = Mathf.PerlinNoise(noiseSeedX + 113f + t * microFrequency, noiseSeedY + 71f) * 2f - 1f;

            float longBend = Mathf.Lerp(macroNoise, bendBias, effectiveBendBias);
            float combinedNoise = longBend + (microNoise * chainSettings.segmentMicroBendRatio);
            float lateralOffset = combinedNoise * maxLateralOffset * endpointEnvelope;
            segmentPosition += perpendicular * lateralOffset;

            controlPoints.Add(segmentPosition);
        }

        if (useBoundaryAnchors)
            controlPoints.Add(exitAnchor);

        controlPoints.Add(exitPoint); // End point
        ApplyVerticalProfileToControlPoints(controlPoints, entry, chainSettings);
        // Generate smooth curve using Catmull-Rom spline
        return GenerateCatmullRomSpline(controlPoints);
    }

    private void ApplyVerticalProfileToControlPoints(List<Vector3> controlPoints, EdgePoint entry, SegmentChainSettings settings)
    {
        if (settings == null || !settings.useVerticalProfile || controlPoints == null || controlPoints.Count < 2)
            return;

        float maxGrade = Mathf.Max(0f, settings.verticalMaxGradePercent) / 100f;
        if (maxGrade <= 0f)
            return;

        float maxGradeChange = Mathf.Max(0f, settings.verticalMaxGradeChangePercent) / 100f;
        float midpointJitter = Mathf.Clamp(settings.verticalMidpointJitter, 0f, 0.45f);
        float endGradeReturn = Mathf.Clamp01(settings.verticalEndGradeReturn);
        float maxChainElevationDelta = Mathf.Max(0f, settings.verticalMaxChainElevationDelta);

        float[] cumulativeS = new float[controlPoints.Count];
        for (int i = 1; i < controlPoints.Count; i++)
        {
            Vector3 from = controlPoints[i - 1];
            Vector3 to = controlPoints[i];
            float dx = to.x - from.x;
            float dz = to.z - from.z;
            cumulativeS[i] = cumulativeS[i - 1] + Mathf.Sqrt((dx * dx) + (dz * dz));
        }

        float totalLength = cumulativeS[controlPoints.Count - 1];
        if (totalLength < 0.001f)
            return;

        float yStart = controlPoints[0].y;
        float gStart = Mathf.Clamp(entry != null ? entry.edgeTravelGrade : 0f, -maxGrade, maxGrade);

        float midpointShift = UnityEngine.Random.Range(-midpointJitter, midpointJitter);
        float midpointFraction = Mathf.Clamp(0.5f + midpointShift, 0.2f, 0.8f);
        float sMid = Mathf.Clamp(totalLength * midpointFraction, totalLength * 0.15f, totalLength * 0.85f);

        float gMidRandom = UnityEngine.Random.Range(-maxGrade, maxGrade);
        float gMid = Mathf.Clamp(gMidRandom, gStart - maxGradeChange, gStart + maxGradeChange);
        gMid = Mathf.Clamp(gMid, -maxGrade, maxGrade);

        float gEndTarget = Mathf.Lerp(gMid, 0f, endGradeReturn);
        float gEnd = Mathf.Clamp(gEndTarget, gMid - maxGradeChange, gMid + maxGradeChange);
        gEnd = Mathf.Clamp(gEnd, -maxGrade, maxGrade);

        float yEndRaw = EvaluateParabolicVerticalProfile(totalLength, yStart, sMid, totalLength, gStart, gMid, gEnd);
        float endCorrection = 0f;
        if (maxChainElevationDelta > 0f)
        {
            float elevationDelta = yEndRaw - yStart;
            float clampedDelta = Mathf.Clamp(elevationDelta, -maxChainElevationDelta, maxChainElevationDelta);
            endCorrection = clampedDelta - elevationDelta;
        }

        for (int i = 0; i < controlPoints.Count; i++)
        {
            float s = cumulativeS[i];
            float y = EvaluateParabolicVerticalProfile(s, yStart, sMid, totalLength, gStart, gMid, gEnd);

            if (Mathf.Abs(endCorrection) > 0.0001f)
            {
                float u = s / totalLength;
                y += endCorrection * u * u;
            }

            Vector3 p = controlPoints[i];
            p.y = y;
            controlPoints[i] = p;
        }
    }

    private static float EvaluateParabolicVerticalProfile(float s, float yStart, float sMid, float totalLength, float gStart, float gMid, float gEnd)
    {
        if (totalLength <= 0.0001f)
            return yStart;

        float clampedS = Mathf.Clamp(s, 0f, totalLength);
        float firstLength = Mathf.Max(0.0001f, sMid);
        float secondLength = Mathf.Max(0.0001f, totalLength - sMid);

        if (clampedS <= sMid)
        {
            float gradeSlope = (gMid - gStart) / firstLength;
            return yStart + (gStart * clampedS) + (0.5f * gradeSlope * clampedS * clampedS);
        }

        float yMid = yStart + (0.5f * (gStart + gMid) * sMid);
        float ds = clampedS - sMid;
        float gradeSlope2 = (gEnd - gMid) / secondLength;
        return yMid + (gMid * ds) + (0.5f * gradeSlope2 * ds * ds);
    }

    private List<RoadSegment> GenerateCatmullRomSpline(List<Vector3> controlPoints)
    {
        List<RoadSegment> smoothSegments = new List<RoadSegment>();

        for (int i = 0; i < controlPoints.Count - 1; i++)
        {
            Vector3 p0 = (i == 0) ? controlPoints[i] : controlPoints[i - 1];
            Vector3 p1 = controlPoints[i];
            Vector3 p2 = controlPoints[i + 1];
            Vector3 p3 = (i == controlPoints.Count - 2) ? controlPoints[i + 1] : controlPoints[i + 2];

            // Subdivide segment into smaller smooth sections
            int subdivisions = 2; // Adjust for smoother curves
            for (int j = 0; j <= subdivisions; j++)
            {
                if (i > 0 && j == 0)
                    continue;

                float t = j / (float)subdivisions;
                Vector3 position = CatmullRom(p0, p1, p2, p3, t);

                RoadSegment segment = CreateSegment(position, smoothSegments.Count + 1, this.transform);
                smoothSegments.Add(segment);
            }
        }
        return smoothSegments;
    }

    private Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float t2 = t * t;
        float t3 = t2 * t;

        return 0.5f * (
            (2 * p1) +
            (-p0 + p2) * t +
            (2 * p0 - 5 * p1 + 4 * p2 - p3) * t2 +
            (-p0 + 3 * p1 - 3 * p2 + p3) * t3
        );
    }

    private void ExecuteMeshtasks(MeshtaskSettings settings, SegmentChain roadchain)
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
        foreach (RoadSegment g in createdSegments)
            segments.Add(g);                       //BETWEEN
        segments.Add(exit.gameObject.GetComponent<RoadSegment>());  //LAST
        return segments;
    }

    private SegmentChain InstantiateSegmentChain()
    {
        GameObject segmentChainObject = Instantiate(segmentChainPrefab);
        SegmentChain roadChain = segmentChainObject.GetComponent<SegmentChain>();
        segmentChainObject.name = "SegmentChain";
        return roadChain;
    }

private EdgePoint CreateEntry(EdgePoint exitPoint)
    {
        EdgeLocation location = GetInversedEdgeLocation(exitPoint.edgeLocation);
        Vector3 entryForward = -exitPoint.edgeForward;
        if (entryForward.sqrMagnitude < 0.0001f)
            entryForward = GetEdgeOutwardDirection(location);

        return new EdgePoint(location, exitPoint.edgePointT, entryForward, exitPoint.edgeTravelGrade, exitPoint.gameObject);
    }

private EdgePoint CreateExit(EdgePoint entryPoint)
    {
        EdgeLocation exitLocation = GetRandomExitLocation(entryPoint);

        GameObject segment = new GameObject("ExitPoint");
        EdgePoint point = new EdgePoint(exitLocation, GetSmartExitPointT(entryPoint, exitLocation), segment);
        segment.transform.position = GetEdgePointLocalPosition(point) + this.transform.position;

        Vector3 edgeOutward = GetEdgeOutwardDirection(exitLocation);
        Vector3 chainDirection = segment.transform.position - entryPoint.gameObject.transform.position;
        chainDirection.y = 0f;
        if (chainDirection.sqrMagnitude < 0.0001f)
            chainDirection = edgeOutward;

        chainDirection.Normalize();
        Vector3 blendedForward = Vector3.Slerp(edgeOutward, chainDirection, 0.45f);
        if (Vector3.Dot(blendedForward, edgeOutward) < 0f)
            blendedForward = edgeOutward;

        point.SetEdgeForward(blendedForward);
        point.edgeTravelGrade = entryPoint.edgeTravelGrade;
        segment.transform.rotation = point.edgeRotation;
        return point;
    }

private static Vector3 GetEdgeOutwardDirection(EdgeLocation edgeLocation)
    {
        return edgeLocation switch
        {
            EdgeLocation.Left => Vector3.left,
            EdgeLocation.Right => Vector3.right,
            EdgeLocation.Top => Vector3.forward,
            EdgeLocation.Bottom => Vector3.back,
            EdgeLocation.none => Vector3.forward,
            _ => Vector3.forward
        };
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
                distance = Vector3.Distance(segments[i].transform.position, segments[i + 1].transform.position);
            //Last point, only take distance of point before
            else
                distance = Vector3.Distance(segments[i].transform.position, segments[i - 1].transform.position);

            segments[i].tangentLength = Mathf.Clamp(distance/2f, 0, 45);
        }
    }

    private List<EdgeLocation> GetPossibleExitLocations(EdgePoint entryPoint)
    {
        List<EdgeLocation> possibility = new List<EdgeLocation> { (EdgeLocation)0, (EdgeLocation)1, (EdgeLocation)2, (EdgeLocation)3 };
        possibility.Remove(entryPoint.edgeLocation);

        SegmentChainSettings settings = GetChainSettings(false).segmentChainSettings;
        float margin = Mathf.Clamp01(settings.edgeMarginNormalized);
        float t = Mathf.Clamp01(entryPoint.edgePointT);
        bool nearLow = t <= margin;
        bool nearHigh = t >= 1f - margin;

        if (entryPoint.edgeLocation == EdgeLocation.none)
            possibility.Remove(EdgeLocation.Bottom);

        if(entryPoint.edgeLocation == EdgeLocation.Left || entryPoint.edgeLocation == EdgeLocation.Right)
        {
            if (nearLow)
                possibility.Remove(EdgeLocation.Bottom);
            if (nearHigh)
                possibility.Remove(EdgeLocation.Top);
        }

        if (entryPoint.edgeLocation == EdgeLocation.Top || entryPoint.edgeLocation == EdgeLocation.Bottom)
        {
            if (nearLow)
                possibility.Remove(EdgeLocation.Left);
            if (nearHigh)
                possibility.Remove(EdgeLocation.Right);
        }
        return possibility;
    }

    private EdgeLocation GetRandomExitLocation(EdgePoint entryPoint)
    {
        List<EdgeLocation> possibilities = GetPossibleExitLocations(entryPoint);
        return possibilities[UnityEngine.Random.Range(0, possibilities.Count)];
    }

    private int GetPointAmount(EdgePoint entry, EdgePoint exit)
    {
        int nOfPoints = 0;
        switch (entry.edgeLocation)
        {
            case EdgeLocation.Left:
                if (exit.edgeLocation == EdgeLocation.Right) { nOfPoints = GetChainSettings(false).segmentChainSettings.straight_NpointsBetween; }
                else { nOfPoints = GetChainSettings(false).segmentChainSettings.corner_NpointsBetween; }
                break;
            case EdgeLocation.Right:
                if (exit.edgeLocation == EdgeLocation.Left) { nOfPoints = GetChainSettings(false).segmentChainSettings.straight_NpointsBetween; }
                else { nOfPoints = GetChainSettings(false).segmentChainSettings.corner_NpointsBetween; }
                break;
            case EdgeLocation.Top:
                if (exit.edgeLocation == EdgeLocation.Bottom) { nOfPoints = GetChainSettings(false).segmentChainSettings.straight_NpointsBetween; }
                else { nOfPoints = GetChainSettings(false).segmentChainSettings.corner_NpointsBetween; }
                break;
            case EdgeLocation.Bottom:
                if (exit.edgeLocation == EdgeLocation.Top) { nOfPoints = GetChainSettings(false).segmentChainSettings.straight_NpointsBetween; }
                else { nOfPoints = GetChainSettings(false).segmentChainSettings.corner_NpointsBetween; }
                break;
            case EdgeLocation.none:
                if (exit.edgeLocation == EdgeLocation.Top) { nOfPoints = GetChainSettings(false).segmentChainSettings.straight_NpointsBetween; }
                else { nOfPoints = GetChainSettings(false).segmentChainSettings.corner_NpointsBetween; }
                break;
        }
        return nOfPoints;
    }

    private Vector3 GetEdgePointLocalPosition(EdgePoint exitPoint)
    {
        SegmentChainSettings settings = GetChainSettings(false).segmentChainSettings;
        float side = settings.gridSize / 2f;
        float pointT = Mathf.Clamp01(exitPoint.edgePointT);
        float pointPosition = Mathf.Lerp(-side, side, pointT);
        float inwardOffset = Mathf.Clamp(settings.edgeInwardOffset, 0f, side);

        switch (exitPoint.edgeLocation)
        {
            case EdgeLocation.Left:
                return new Vector3(-side + inwardOffset, 0, pointPosition);
            case EdgeLocation.Right:
                return new Vector3(side - inwardOffset, 0, pointPosition);
            case EdgeLocation.Top:
                return new Vector3(pointPosition, 0, side - inwardOffset);
            case EdgeLocation.Bottom:
                return new Vector3(pointPosition, 0, -side + inwardOffset);
            case EdgeLocation.none:
                return Vector3.zero;
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
                p[0] -= GetChainSettings(false).segmentChainSettings.gridSize;
                break;
            case EdgeLocation.Right:
                p[0] += GetChainSettings(false).segmentChainSettings.gridSize;
                break;
            case EdgeLocation.Top:
                p[2] += GetChainSettings(false).segmentChainSettings.gridSize;
                break;
            case EdgeLocation.Bottom:
                p[2] -= GetChainSettings(false).segmentChainSettings.gridSize;
                break;
            case EdgeLocation.none:
                break;
        }
        return p;
    }

    private float GetSmartExitPointT(EdgePoint entryPoint, EdgeLocation exit)
    {
        SegmentChainSettings settings = GetChainSettings(false).segmentChainSettings;
        float min = 0f;
        float max = 1f;
        float bufferNormalized = settings.sidePointAmount <= 1
            ? 0f
            : settings.cornerBuffer / (float)(settings.sidePointAmount - 1);
        float edgeMargin = Mathf.Clamp(settings.edgeMarginNormalized, 0f, 0.45f);

        switch (entryPoint.edgeLocation)
        {
            case EdgeLocation.Left:
                if (exit != EdgeLocation.Right)
                    min += bufferNormalized;
                break;
            case EdgeLocation.Right:
                if (exit != EdgeLocation.Left)
                    max -= bufferNormalized;
                break;
            case EdgeLocation.Top:
                if (exit != EdgeLocation.Bottom)
                    max -= bufferNormalized;
                break;
            case EdgeLocation.Bottom:
                if (exit != EdgeLocation.Top)
                    min += bufferNormalized;
                break;
            default:
                break;
        }

        min = Mathf.Clamp(min, edgeMargin, 1f - edgeMargin);
        max = Mathf.Clamp(max, min + 0.0001f, 1f - edgeMargin);

        bool isStraight = IsStraightTransition(entryPoint.edgeLocation, exit);
        float clampedEntryT = Mathf.Clamp01(entryPoint.edgePointT);
        float normalizedEntryT = Mathf.Clamp(clampedEntryT, min, max);

        float targetNormalizedT = normalizedEntryT;
        if (!isStraight)
        {
            float outsideT = PrefersHighCornerIndex(entryPoint.edgeLocation) ? 1f : 0f;
            targetNormalizedT = Mathf.Lerp(normalizedEntryT, outsideT, settings.cornerOutsideBias);
        }

        float targetT = Mathf.Clamp(targetNormalizedT, min, max);

        float jitter = isStraight ? settings.edgeTJitterStraight : settings.edgeTJitterCorner;
        float randomMin = Mathf.Clamp(targetT - jitter, min, max);
        float randomMax = Mathf.Clamp(targetT + jitter, min, max);

        if (randomMin >= randomMax)
            return targetT;

        return UnityEngine.Random.Range(randomMin, randomMax);
    }

    private static bool IsStraightTransition(EdgeLocation entry, EdgeLocation exit)
    {
        return (entry == EdgeLocation.Left && exit == EdgeLocation.Right) ||
               (entry == EdgeLocation.Right && exit == EdgeLocation.Left) ||
               (entry == EdgeLocation.Top && exit == EdgeLocation.Bottom) ||
               (entry == EdgeLocation.Bottom && exit == EdgeLocation.Top);
    }

    private static bool PrefersHighCornerIndex(EdgeLocation entry)
    {
        return entry == EdgeLocation.Left || entry == EdgeLocation.Bottom;
    }

    private List<RoadSegment> CreatePointsbetweenEntryStart(EdgePoint entry, EdgePoint exit, int nOfPoints)
    {
        Vector3 entryPoint = entry.gameObject.transform.position;
        Vector3 exitPoint = exit.gameObject.transform.position;
        List<RoadSegment> segments = new List<RoadSegment>();
        for (int i = 1; i <= nOfPoints; i++)
        {
            float t = (float)i / (nOfPoints + 1);
            Vector3 segmentPosition = Vector3.Lerp(entryPoint, exitPoint, t);

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
            bool violation = Vector3.Distance(item.transform.position, proximity) < GetChainSettings(false).segmentChainSettings.segmentDeletionProximity;
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
        if (segments == null || segments.Count == 0)
            return;

        if (segments.Count == 1)
            return;

        // First point: use forward difference.
        Vector3 firstDirection = segments[1].transform.position - segments[0].transform.position;
        if (firstDirection.sqrMagnitude > 0.0001f)
            segments[0].transform.rotation = Quaternion.LookRotation(firstDirection, Vector3.up);

        // Middle points: use centered difference.
        for (int i = 1; i < segments.Count - 1; i++)
        {
            Vector3 infront = segments[i + 1].transform.position;
            Vector3 behind = segments[i - 1].transform.position;
            Vector3 direction = infront - behind;

            if (direction.sqrMagnitude > 0.0001f)
                segments[i].transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
        }

        // Last point: use backward difference.
        int lastIndex = segments.Count - 1;
        Vector3 lastDirection = segments[lastIndex].transform.position - segments[lastIndex - 1].transform.position;
        if (lastDirection.sqrMagnitude > 0.0001f)
            segments[lastIndex].transform.rotation = Quaternion.LookRotation(lastDirection, Vector3.up);
        else
            segments[lastIndex].transform.rotation = segments[lastIndex - 1].transform.rotation;
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
                float calculatedHeight = baseHeight + UnityEngine.Random.Range(-heightRange, heightRange);
                Vector3 position = currentSegment.transform.position;
                position.y = calculatedHeight;
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
            if (createdSegmentChains.Count == 1 && i == 1)
                continue;
            _ = segments[i].transform.localPosition;
            Vector3 random = new Vector3(UnityEngine.Random.Range(-range, range), 0f, 0f);
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

    private void DeleteSegmentChain()
    {
        SegmentChain rc = createdSegmentChains.Dequeue();
        Destroy(rc.gameObject);
    }
}

[System.Serializable]

public class EdgePoint
{
    public EdgeLocation edgeLocation = EdgeLocation.none;
    public float edgePointT = 0.5f; // Normalized position on side (0..1).
    public float edgeTravelGrade = 0f; // Signed rise/run grade carried between chains.
    public Quaternion edgeRotation { get { return Quaternion.LookRotation(edgeForward, Vector3.up); } }
    public Vector3 edgeForward
    {
        get
        {
            if (_edgeForward.sqrMagnitude < 0.0001f)
                _edgeForward = GetDefaultEdgeForward(edgeLocation);
            return _edgeForward.normalized;
        }
    }
    public GameObject gameObject { get { return GO; } }

    private GameObject GO;
    private Vector3 _edgeForward;

    public EdgePoint(EdgeLocation edge, float t, Vector3 forward, float travelGrade, GameObject gameObject)
    {
        edgePointT = Mathf.Clamp01(t);
        edgeLocation = edge;
        edgeTravelGrade = travelGrade;
        GO = gameObject;
        SetEdgeForward(forward);
    }

    public EdgePoint(EdgeLocation edge, float t, Vector3 forward, GameObject gameObject)
        : this(edge, t, forward, 0f, gameObject)
    {
    }

    public EdgePoint(EdgeLocation edge, float t, GameObject gameObject)
        : this(edge, t, GetDefaultEdgeForward(edge), 0f, gameObject)
    {
    }

    public EdgePoint(EdgeLocation edge, int index, GameObject gameObject)
    {
        int sidePointAmount = SegmentChainBuilder.GetActiveSidePointAmountOrDefault();
        float denominator = Mathf.Max(1f, sidePointAmount - 1f);
        edgePointT = Mathf.Clamp01(index / denominator);
        edgeLocation = edge;
        edgeTravelGrade = 0f;
        GO = gameObject;
        _edgeForward = GetDefaultEdgeForward(edge);
    }

    public EdgePoint(GameObject gameObject)
    {
        edgePointT = 0.5f;
        edgeTravelGrade = 0f;
        GO = gameObject;
        _edgeForward = GetDefaultEdgeForward(EdgeLocation.none);
    }

    public void SetEdgeForward(Vector3 forward)
    {
        forward.y = 0f;
        if (forward.sqrMagnitude < 0.0001f)
            forward = GetDefaultEdgeForward(edgeLocation);

        _edgeForward = forward.normalized;
    }

    private static Vector3 GetDefaultEdgeForward(EdgeLocation edgeLocation)
    {
        return edgeLocation switch
        {
            EdgeLocation.Left => Vector3.left,
            EdgeLocation.Right => Vector3.right,
            EdgeLocation.Top => Vector3.forward,
            EdgeLocation.Bottom => Vector3.back,
            EdgeLocation.none => Vector3.forward,
            _ => Vector3.forward
        };
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

    //Left extrusion
    public float leftExtrusion = 0;
    private float maxLeftExtrusion;
    private float leftReductionVelocity;

    //Right extrusion
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
        int dataKey = UnityEngine.Random.Range(0, 1000000);
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
